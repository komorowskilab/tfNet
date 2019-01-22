using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml;

namespace tfNet
{
    public class filtAndNet : peakAndReg
    {
        #region network parameters
        public char filterOption { get; set; }
        public double filterValue { get; set; }
        public int neighborDistanceLow { get; set; }
        public int neighborDistanceHigh { get; set; }
        public int overlapDistance { get; set; }
        public string title { get; set; }
        public string Rscript { get; set; }
        public string scriptPath { get; set; }
        public bool noRscript { get; set; }
        #endregion

        public Dictionary<string, Dictionary<string, tfOccurrences>> tfOccs = new Dictionary<string, Dictionary<string, tfOccurrences>>();
        
        public filtAndNet(string _mode)
            : base(_mode)
        {

        }
        
        /// <summary>
        /// load regions and peaks from a bed file
        /// </summary>
        /// <param name="regionFile">the input file</param>
        /// <returns>list of loaded regions</returns>
        public List<region> loadRegions(string regionFile)
        {
            Console.Write("loading regions...");
            List<region> loadedRegions = new List<region>();
            peak newPeak;
            region newRegion;
            int numOfCols = checkNumberOfFieldsInBedFile(regionFile);
            
            FileStream fs = File.Open(@"" + regionFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BufferedStream bs = new BufferedStream(fs);
            StreamReader sr = new StreamReader(bs);
            string line;
            int lineCounter = 1;
            while ((line = sr.ReadLine()) != null)
            {
                if ((newPeak = peakFromLine(line, numOfCols, regionFile, lineCounter, null)) == null)
                {
                    lineCounter++;
                    if ((newPeak = peakFromLine(line, numOfCols, regionFile, lineCounter, null)) == null)
                    {
                        exit("wrong file format in the combined peak file " + regionFile.Split(OSseparator).Last());
                    }
                }
                loadedRegions.Add(newRegion = peakToRegion(newPeak, lineCounter));
                
                #region fill in statistics
                statistics.addToPeaksPerChromosomePre(newRegion.chromosome, newRegion.peakList.Count);
                foreach (peak p in newRegion.peakList)
                {
                    statistics.addToTfStatsPre(p.TFname, new List<int>() { 0 }, 1);

                    if (mode == "network" || mode == "tfNet")
                    {
                        statistics.addToTfStatsPost(p.TFname, new List<int>() { 0 }, 1);
                    }
                }

                if (mode == "network" || mode == "tfNet")
                {
                    if (lineCounter >= 2)
                    {
                        if (newRegion.chromosome == loadedRegions.ElementAt(lineCounter - 1).chromosome)
                        {
                            statistics.allRegionsDistance.Add(newRegion.endIndex - loadedRegions.ElementAt(lineCounter - 1).startIndex);
                        }
                    }
                    statistics.addToRegionScore(new List<int>() { newRegion.endIndex - newRegion.startIndex }, new List<double>() { newRegion.score }, 1);
                    statistics.addToRegionLength(new List<int>() { newRegion.endIndex - newRegion.startIndex }, new List<double>() { newRegion.score }, 1);
                    statistics.addToRegionsPerChromosome(newRegion.chromosome, 1);
                    statistics.addToPeaksPerChromosomePost(newRegion.chromosome, newRegion.peakList.Count);
                    statistics.peaksInRegionDistance.AddRange(new List<int>(new int[newRegion.peakList.Count - 1]));
                    statistics.allPeaksDistance.AddRange(new List<int>(new int[newRegion.peakList.Count - 1]));
                }
                #endregion
                lineCounter++;
            }
            sr.Close();
            Console.WriteLine(" done!");
            return loadedRegions;
        }

        /// <summary>
        /// load regions and peaks from an xml file
        /// </summary>
        /// <param name="inputFile">the input file</param>
        /// <returns>list of loaded regions</returns>
        public List<region> readFromXML(string inputFile)
        {
            Console.Write("loading regions...");
            List<region> allRegionsLoaded = new List<region>();
            region newRegion;
            peak newPeak;
            List<Tuple<string, int>> tfsInReg;
            int elementCounter = 1, attributeCounter = 1, lineCounter = 2;

            XmlReader reader = XmlReader.Create(@"" + inputFile);
            XmlReader sTree;
            XElement regionElement;

            while (reader.ReadToFollowing("reg"))
            {
                if (reader.IsStartElement())
                {
                    sTree = reader.ReadSubtree();
                    regionElement = XElement.Load(sTree);

                    lineCounter++;
                    newRegion = regionFromXML(regionElement, inputFile, elementCounter, lineCounter);
                    newRegion.peakList = new List<peak>();
                    attributeCounter = 1;
                    tfsInReg = new List<Tuple<string, int>>();

                    foreach (XElement peakElement in regionElement.Descendants("pk"))
                    {
                        lineCounter++;
                        newRegion.peakList.Add(newPeak = peakFromXML(peakElement, inputFile, elementCounter, attributeCounter, lineCounter));
                        #region fill in statistics
                        statistics.addToPeaksPerChromosomePre(newPeak.chromosome, 1);
                        statistics.addToTfStatsPre(newPeak.TFname, new List<int>() { newPeak.endIndex - newPeak.startIndex }, 1);

                        if (mode == "network" || mode == "tfNet")
                        {
                            if (mode == "network")
                            {
                                if (attributeCounter >= 2)
                                {
                                    statistics.peaksInRegionDistance.Add(newPeak.startIndex - newRegion.peakList.ElementAt(attributeCounter - 1).endIndex);
                                }
                                statistics.addToPeaksPerChromosomePost(newPeak.chromosome, 1);
                                statistics.allPeaksDistance.Add(0);
                            }
                            statistics.addToTfStatsPost(newPeak.TFname, new List<int>() { newPeak.endIndex - newPeak.startIndex }, 1);

                            #region keep some stats for network
                            if (!tfsInReg.Any(x => x.Item1 == newPeak.TFname))
                            {
                                foreach (Tuple<string, int> s in tfsInReg)
                                {
                                    if (tfOccs.ContainsKey(s.Item1))
                                    {
                                        if (tfOccs[s.Item1].ContainsKey(newPeak.TFname))
                                        {
                                            tfOccs[s.Item1][newPeak.TFname].increaseCount(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2));
                                        }
                                        else
                                        {
                                            tfOccs[s.Item1].Add(newPeak.TFname, new tfOccurrences(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2)));
                                        }
                                    }
                                    else
                                    {
                                        tfOccs.Add(s.Item1, new Dictionary<string, tfOccurrences>() { { newPeak.TFname, new tfOccurrences(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2)) } });
                                    }

                                    if (tfOccs.ContainsKey(newPeak.TFname))
                                    {
                                        if (tfOccs[newPeak.TFname].ContainsKey(s.Item1))
                                        {
                                            tfOccs[newPeak.TFname][s.Item1].increaseCount(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2));
                                        }
                                        else
                                        {
                                            tfOccs[newPeak.TFname].Add(s.Item1, new tfOccurrences(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2)));
                                        }
                                    }
                                    else
                                    {
                                        tfOccs.Add(newPeak.TFname, new Dictionary<string, tfOccurrences>() { { s.Item1, new tfOccurrences(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2)) } });
                                    }
                                }
                            }
                            tfsInReg.Add(new Tuple<string, int>(newPeak.TFname, newPeak.startIndex + newPeak.summit));
                            #endregion
                        }
                        #endregion
                        attributeCounter++;
                    }
                    allRegionsLoaded.Add(newRegion);
                    #region fill in statistics
                    if (mode == "network")
                    {
                        if (elementCounter >= 2)
                        {
                            if (newRegion.chromosome == allRegionsLoaded.ElementAt(elementCounter - 1).chromosome)
                            {
                                statistics.allRegionsDistance.Add(newRegion.endIndex - allRegionsLoaded.ElementAt(elementCounter - 1).startIndex);
                            }
                        }
                        statistics.addToRegionScore(new List<int>() { newRegion.endIndex - newRegion.startIndex }, new List<double>() { newRegion.score }, 1);
                        statistics.addToRegionLength(new List<int>() { newRegion.endIndex - newRegion.startIndex }, new List<double>() { newRegion.score }, 1);
                        statistics.addToRegionsPerChromosome(newRegion.chromosome, 1);
                    }
                    #endregion
                    elementCounter++;
                }
                else
                {
                    break;
                }
            }
            Console.WriteLine(" done!");
            return allRegionsLoaded;
        }

        /// <summary>
        /// construct a region from an xml file element
        /// </summary>
        /// <param name="regionElement">xml region element</param>
        /// <param name="inputFile">the name of the input xml file</param>
        /// <param name="elementCounter">region element counter</param>
        /// <param name="lineCounter">the xml file line counter</param>
        /// <returns>returns the constructed region</returns>
        public region regionFromXML(XElement regionElement, string inputFile, int elementCounter, int lineCounter)
        {
            int tmpInt; double tmpDbl;
            region newRegion = new region();

            #region test chromosome 0
            try
            {
                if (!chromosomeNamesAndLength.ContainsKey(regionElement.Attribute("chr").Value) && !ignoreChromosomeLength)
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid chromosome entry in element " + elementCounter + " (line " + lineCounter + ")");
                }
                else
                {
                    newRegion.chromosome = regionElement.Attribute("chr").Value;
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain chromosome information in element " + elementCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test start position 1
            try
            {
                if (!int.TryParse(regionElement.Attribute("s").Value, out tmpInt))
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid start index entry in element " + elementCounter + ". Integer expected" + " (line " + lineCounter + ")");
                }
                if (tmpInt < 0)
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid start index entry in element " + elementCounter + ". Positive expected" + " (line " + lineCounter + ")");
                }
                newRegion.startIndex = tmpInt;
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain start index information in element " + elementCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test end position 2
            try
            {
                if (!int.TryParse(regionElement.Attribute("e").Value, out tmpInt))
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid end index entry in element " + elementCounter + ". Integer expected" + " (line " + lineCounter + ")");
                }
                if (tmpInt < 0)
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid end index entry in element " + elementCounter + ". Positive expected" + " (line " + lineCounter + ")");
                }
                if (!ignoreChromosomeLength)
                {
                    if (tmpInt > chromosomeNamesAndLength[newRegion.chromosome])
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid end index entry in element " + elementCounter + ". Exceeding chromosome's limits" + " (line " + lineCounter + ")");
                    }
                }
                newRegion.endIndex = tmpInt;
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain end index information in element " + elementCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test name 3
            try
            {
                newRegion.name = regionElement.Attribute("n").Value;
                if (newRegion.name.Split('-').Length != 2 || !newRegion.name.Split('-').First().StartsWith("reg"))
                {
                    exit("the name field of the input file in line " + lineCounter + " is in a wrong format");
                }
                else
                {
                    newRegion.regionName = newRegion.name.Split('-').First();
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain name information in element " + elementCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test score 4
            try
            {
                if (numOfCols > 4)
                {
                    if (!double.TryParse(regionElement.Attribute("scr").Value, out tmpDbl))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid score entry in element " + elementCounter + ". Integer expected" + " (line " + lineCounter + ")");
                    }
                    newRegion.score = tmpDbl;
                }
                else
                {
                    newRegion.score = 0;
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain score information in element " + elementCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test strand 5
            try
            {
                if (numOfCols > 5)
                {
                    if (!strandSymbols.Exists(x => x == regionElement.Attribute("strd").Value[0]))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid strand entry in element " + elementCounter + ". +/-/. expected" + " (line " + lineCounter + ")");
                    }
                    newRegion.strand = regionElement.Attribute("strd").Value[0];
                }
                else
                {
                    newRegion.strand = '.';
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain strand information in element " + elementCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test pValue 7
            try
            {
                if (numOfCols > 7)
                {
                    if (!double.TryParse(regionElement.Attribute("pv").Value, out tmpDbl))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid pValue entry in element " + elementCounter + ". Numeric expected" + " (line " + lineCounter + ")");
                    }
                    newRegion.pValue = tmpDbl;
                }
                else
                {
                    newRegion.pValue = -1;
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain pValue information in element " + elementCounter + ". Numeric expected" + " (line " + lineCounter + ")");
            }
            #endregion

            #region test qValue 8
            try
            {
                if (numOfCols > 8)
                {
                    if (!double.TryParse(regionElement.Attribute("qv").Value, out tmpDbl))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid qValue entry in element " + elementCounter + ". Numeric expected" + " (line " + lineCounter + ")");
                    }
                    newRegion.qValue = tmpDbl;
                }
                else
                {
                    newRegion.qValue = -1;
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain qValue information in element " + elementCounter + ". Numeric expected" + " (line " + lineCounter + ")");
            }
            #endregion

            return newRegion;
        }

        /// <summary>
        /// construct a peak from an xml file element
        /// </summary>
        /// <param name="peakElement">xml peak element</param>
        /// <param name="inputFile">the name of the input xml file</param>
        /// <param name="elementCounter">region element counter</param>
        /// <param name="attributeCounter">peak within region attribute counter</param>
        /// <param name="lineCounter">the xml file line counter</param>
        /// <returns>returns the constructed peak</returns>
        public peak peakFromXML(XElement peakElement, string inputFile, int elementCounter, int attributeCounter, int lineCounter)
        {
            int tmpInt;
            double tmpDbl;
            peak newPeak = new peak();

            #region test chromosome 0
            try
            {
                if (!chromosomeNamesAndLength.ContainsKey(peakElement.Attribute("chr").Value) && !ignoreChromosomeLength)
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid chromosome entry in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
                }
                else
                {
                    newPeak.chromosome = peakElement.Attribute("chr").Value;
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain chromosome information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test start position 1
            try
            {
                if (!int.TryParse(peakElement.Attribute("s").Value, out tmpInt))
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid start index entry in element " + elementCounter + " attribute " + attributeCounter + ". Integer expected" + " (line " + lineCounter + ")");
                }
                if (tmpInt < 0)
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid start index entry in element " + elementCounter + " attribute " + attributeCounter + ". Positive expected" + " (line " + lineCounter + ")");
                }
                newPeak.startIndex = tmpInt;
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain start index information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test end position 2
            try
            {
                if (!int.TryParse(peakElement.Attribute("e").Value, out tmpInt))
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid end index entry in element " + elementCounter + " attribute " + attributeCounter + ". Integer expected" + " (line " + lineCounter + ")");
                }
                if (tmpInt < 0)
                {
                    exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid end index entry in element " + elementCounter + " attribute " + attributeCounter + ". Positive expected" + " (line " + lineCounter + ")");
                }
                if (!ignoreChromosomeLength)
                {
                    if (tmpInt > chromosomeNamesAndLength[newPeak.chromosome])
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid end index entry in element " + elementCounter + " attribute " + attributeCounter + ". Exceeding chromosome's limits" + " (line " + lineCounter + ")");
                    }
                }
                newPeak.endIndex = tmpInt;
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain end index information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test name 3
            try
            {
                newPeak.name = peakElement.Attribute("n").Value;
                if (newPeak.name.Split('_').Length > 1)
                {
                    newPeak.TFname = newPeak.name.Split('_').First();
                    newPeak.peakName = newPeak.name.Split('_').Last();
                }
                else
                {
                    newPeak.TFname = "TF";
                    newPeak.peakName = "peak" + Convert.ToString(lineCounter - ((2 * elementCounter) - 2));
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain name information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test score 4
            try
            {
                if (numOfCols > 4)
                {
                    if (!double.TryParse(peakElement.Attribute("scr").Value, out tmpDbl))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid score entry in element " + elementCounter + " attribute " + attributeCounter + ". Integer expected" + " (line " + lineCounter + ")");
                    }
                    newPeak.score = tmpDbl;
                }
                else
                {
                    newPeak.score = 0;
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain score information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test strand 5
            try
            {
                if (numOfCols > 5)
                {
                    if (!strandSymbols.Exists(x => x == peakElement.Attribute("strd").Value[0]))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid strand entry in element " + elementCounter + " attribute " + attributeCounter + ". +/-/. expected" + " (line " + lineCounter + ")");
                    }
                    newPeak.strand = peakElement.Attribute("strd").Value[0];
                }
                else
                {
                    newPeak.strand = '.';
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain strand information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test signalValue 6
            try
            {
                if (numOfCols > 6)
                {
                    if (!double.TryParse(peakElement.Attribute("sv").Value, out tmpDbl))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid signalValue entry in element " + elementCounter + " attribute " + attributeCounter + ". Numeric expected" + " (line " + lineCounter + ")");
                    }
                    newPeak.signalValue = tmpDbl;
                }
                else
                {
                    newPeak.signalValue = 0;
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain signalValue information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test pValue 7
            try
            {
                if (numOfCols > 7)
                {
                    if (!double.TryParse(peakElement.Attribute("pv").Value, out tmpDbl))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid pValue entry in element " + elementCounter + " attribute " + attributeCounter + ". Numeric expected" + " (line " + lineCounter + ")");
                    }
                    newPeak.pValue = tmpDbl;
                }
                else
                {
                    newPeak.pValue = -1;
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain pValue information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test qValue 8
            try
            {
                if (numOfCols > 8)
                {
                    if (!double.TryParse(peakElement.Attribute("qv").Value, out tmpDbl))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid qValue entry in element " + elementCounter + " attribute " + attributeCounter + ". Numeric expected" + " (line " + lineCounter + ")");
                    }
                    newPeak.qValue = tmpDbl;
                }
                else
                {
                    newPeak.qValue = -1;
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain qValue information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }
            #endregion

            #region test summit 9
            try
            {
                if (numOfCols > 9)
                {
                    if (!int.TryParse(peakElement.Attribute("sm").Value, out tmpInt))
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid summit entry in element " + elementCounter + " attribute " + attributeCounter + ". Integer expected" + " (line " + lineCounter + ")");
                    }
                    if (tmpInt < 0 && tmpInt != -1)
                    {
                        exit("the file " + inputFile.Split(OSseparator).Last() + " has an invalid summit entry in element " + elementCounter + " attribute " + attributeCounter + ". Positive or -1 expected" + " (line " + lineCounter + ")");
                    }
                    newPeak.summit = tmpInt;
                }
                else
                {
                    newPeak.summit = Convert.ToInt32((newPeak.endIndex - newPeak.startIndex) / 2);
                }
            }
            catch (Exception)
            {
                exit("the file " + inputFile.Split(OSseparator).Last() + " does not contain summit information in element " + elementCounter + " attribute " + attributeCounter + " (line " + lineCounter + ")");
            }

            #endregion

            return newPeak;
        }

        /// <summary>
        /// check the if the format of the input file is xml of bed
        /// </summary>
        /// <param name="inputFile">the input file</param>
        /// <returns>true for bed format and false for xml format</returns>
        public bool checkFileFormat(string inputFile)
        {
            TextReader input = new StreamReader(@"" + inputFile);
            string line = input.ReadLine();
            input.Close();
            if (line.StartsWith("<?xml "))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// converts an loaded peak to a region. checks if the region name is in the correct format and adds all the peaks in the peak list
        /// </summary>
        /// <param name="newPeak">the loaded peak (that is actually a region)</param>
        /// <param name="lineCounter">the line counter</param>
        /// <returns>the loaded region</returns>
        public region peakToRegion(peak newPeak, int lineCounter)
        {
            List<Tuple<string, int>> tfsInReg;
            
            #region convert peak to region
            region newRegion = new region()
            {
                chromosome = newPeak.chromosome,
                startIndex = newPeak.startIndex,
                endIndex = newPeak.endIndex,
                name = newPeak.name,
                score = newPeak.score,
                strand = newPeak.strand,
                signalValue = newPeak.signalValue,
                pValue = newPeak.pValue,
                qValue = newPeak.qValue,
                summit = newPeak.summit,
                peakList = new List<peak>()
            };
            #endregion

            #region check if region name correct
            if (newRegion.name.Split('-').Length != 2 || !newRegion.name.Split('-').First().StartsWith("reg"))
            {
                exit("the name field of the input file in line " + lineCounter + " is in a wrong format");
            }
            else
            {
                newRegion.regionName = newRegion.name.Split('-').First();
            }
            #endregion

            #region add peaks to peak list
            tfsInReg = new List<Tuple<string, int>>();
            foreach (string sp in newRegion.name.Split('-').Last().Split(','))
            {
                if (sp.Split('_').Length < 2)
                {
                    exit("the name field of the input file in line " + lineCounter + " is in a wrong format");
                }
                else
                {
                    newRegion.peakList.Add(new peak()
                    {
                        chromosome = newRegion.chromosome,
                        startIndex = 1,
                        endIndex = 1,
                        TFname = sp.Split('_').First(),
                        peakName = sp.Split('_').Last(),
                        name = sp,
                        score = 0,
                        strand = '.',
                        signalValue = 0,
                        pValue = -1,
                        qValue = -1,
                        summit = 1
                    });

                    #region add peak statistical data
                    statistics.addToTfStatsPre(sp.Split('_').First(), new List<int>() { 0 }, 1);
                    #endregion

                    #region keep some stats for network
                    if (!tfsInReg.Any(x => x.Item1 == sp.Split('_').First()))
                    {
                        foreach (Tuple<string, int> s in tfsInReg)
                        {
                            if (tfOccs.ContainsKey(s.Item1))
                            {
                                if (tfOccs[s.Item1].ContainsKey(sp.Split('_').First()))
                                {
                                    tfOccs[s.Item1][sp.Split('_').First()].increaseCount(0);
                                }
                                else
                                {
                                    tfOccs[s.Item1].Add(sp.Split('_').First(), new tfOccurrences(0));
                                }
                            }
                            else
                            {
                                tfOccs.Add(s.Item1, new Dictionary<string, tfOccurrences>() { { sp.Split('_').First(), new tfOccurrences(0) } });
                            }

                            if (tfOccs.ContainsKey(sp.Split('_').First()))
                            {
                                if (tfOccs[sp.Split('_').First()].ContainsKey(s.Item1))
                                {
                                    tfOccs[sp.Split('_').First()][s.Item1].increaseCount(0);
                                }
                                else
                                {
                                    tfOccs[sp.Split('_').First()].Add(s.Item1, new tfOccurrences(0));
                                }
                            }
                            else
                            {
                                tfOccs.Add(sp.Split('_').First(), new Dictionary<string, tfOccurrences>() { { s.Item1, new tfOccurrences(0) } });
                            }
                        }
                    }
                    tfsInReg.Add(new Tuple<string, int>(sp.Split('_').First(), 0));
                    #endregion
                }
            }
            #endregion

            return newRegion;
        }
    }

    public class tfOccurrences  
    {
        public int count { get; set; }
        public List<int> distance { get; set; }

        public tfOccurrences(int dist)
        {
            count = 1;
            distance = new List<int>() { dist };
        }

        public void increaseCount(int dist)
        {
            count++;
            distance.Add(dist);
        }

        public void addRangeInList(List<int> distanceList)
        {
            distance.RemoveAt(0);
            distance = distanceList;
        }
    }
}
