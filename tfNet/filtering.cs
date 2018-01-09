using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace tfNet
{
    public class filtering : filtAndReg
    {
        #region parameters
        public string inputFile { get; set; } //the file to be filtered
        HashSet<region> regions { get; set; }
        #endregion

        public filtering(string _mode)
            : base(_mode)
        {
            
        }

        public void start()
        {
            try
            {
                //load peaks and construct regions
                if (lowMemory)
                {
                    Console.WriteLine("WARNING: options -s, -fn and -fp are disabled when low memory usage in enabled!");
                    if (checkFileFormat(inputFile)) //bed file
                    {
                        filterRegionSerially(inputFile);
                    }
                    else //xml file
                    {
                        filterRegionsSeriallyXML(inputFile);
                    }
                    if (statFile)
                    {
                        statistics.printStatisticData(resultsDirectory, OSseparator, outfileName);
                    }
                }
                else
                {
                    if (checkFileFormat(inputFile)) //bed file
                    {
                        regions = new HashSet<region>(loadRegions(inputFile));
                    }
                    else //xml file
                    {
                        regions = new HashSet<region>(readFromXML(inputFile));
                    }
                    filterRegionList();
                }
            }
            catch (OutOfMemoryException)
            {
                exit("not enough memory! Run the low memory usage option instead");
            }
            finally
            {
                mMem.RequestStop();
            }
        }

        public void filterRegionList()
        {
            Console.Write("filtering...");
            
            filterByChromosome();
            filterByStartIndex();
            filterByEndIndex();
            filterByRegName();
            filterByTfName();
            filterByPkName();
            filterByLowerScore();
            filterByHigherScore();
            filterByStrandSpecificity();

            //keep top X
            if (topXP > 0.0 || topX != 0)
            {
                regions = new HashSet<region>(filterTopRegions(regions.ToList()));
            }            

            //sort
            if (sortRegionsBySore != null)
            {
                regions = new HashSet<region>(regionSorting(regions.ToList(), sortRegionsBySore));
            }
            Console.WriteLine(" done!");

            List<region> currRegs = regions.ToList();
            for (int i = 1; i < currRegs.Count; i++)
            {
                calculateStatisticData(currRegs[i], currRegs[i - 1]);
            }

            printRegXmlPeakFile(regions.ToList());
        }

        public void filterRegionSerially(string input)
        {
            FileStream fs = File.Open(@"" + input, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BufferedStream bs = new BufferedStream(fs);
            StreamReader sr = new StreamReader(bs);
            string line;
            int lineCounter = 1;

            peak newPeak;
            region newRegion, previousRegion = null;

            #region check format
            int numOfCols = checkNumberOfFieldsInBedFile(input);
            if (numOfCols < 3 || numOfCols > 10)
            {
                exit("the file " + input.Split(OSseparator).Last() + " has a non-acceptable format");
            }
            #endregion

            string regionsFileName = resultsDirectory + OSseparator + outfileName + "_filtered_regions." + fileType;
            StreamWriter outputRegion = new StreamWriter(@"" + regionsFileName);

            string peaksInRegionsFileName = resultsDirectory + OSseparator + outfileName + "_filtered_regions_peaks." + fileType;
            StreamWriter outputPeak = new StreamWriter(@"" + peaksInRegionsFileName);

            string xmlFileName = resultsDirectory + OSseparator + outfileName + "_filtered_regions.xml";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            XmlWriter writer = XmlWriter.Create(@"" + xmlFileName, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("regs");

            while ((line = sr.ReadLine()) != null)
            {
                if ((newPeak = peakFromLine(line, numOfCols, input, lineCounter, null)) == null)
                {
                    continue;
                }
                newRegion = peakToRegion(newPeak, lineCounter);
                if (filterRegion(newRegion))
                {
                    previousRegion = calculateStatisticData(newRegion, previousRegion);
                    printRegion(newRegion, outputRegion, outputPeak);
                    writeRegionInXML(writer, newRegion);
                }
                lineCounter++;
            }

            outputRegion.Close();
            outputPeak.Close();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            if (!peakFile)
            {
                File.Delete(@"" + peaksInRegionsFileName);
            }
            if (!xmlFile)
            {
                File.Delete(@"" + xmlFileName);
            }

            Console.WriteLine("done!");
        }

        public void filterRegionsSeriallyXML(string input)
        {
            string regionsFileName = resultsDirectory + OSseparator + outfileName + "_filtered_regions." + fileType;
            StreamWriter outputRegion = new StreamWriter(@"" + regionsFileName);

            string peaksInRegionsFileName = resultsDirectory + OSseparator + outfileName + "_filtered_regions_peaks." + fileType;
            StreamWriter outputPeak = new StreamWriter(@"" + peaksInRegionsFileName);

            string xmlFileName = resultsDirectory + OSseparator + outfileName + "_filtered_regions.xml";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            XmlWriter writer = XmlWriter.Create(@"" + xmlFileName, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("regs");

            XmlReader reader = XmlReader.Create(@"" + inputFile);
            XmlReader sTree;
            XElement regionElement;
            region newRegion, previousRegion = null;
            int elementCounter = 1, attributeCounter = 1, lineCounter = 2;

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

                    foreach (XElement peakElement in regionElement.Descendants("pk"))
                    {
                        lineCounter++;
                        newRegion.peakList.Add(peakFromXML(peakElement, inputFile, elementCounter, attributeCounter, lineCounter));
                        attributeCounter++;
                    }
                    elementCounter++;
                    lineCounter++;

                    if (filterRegion(newRegion))
                    {
                        previousRegion = calculateStatisticData(newRegion, previousRegion);
                        printRegion(newRegion, outputRegion, outputPeak);
                        writeRegionInXML(writer, newRegion);
                    }
                }
                else
                {
                    break;
                }
            }

            outputRegion.Close();
            outputPeak.Close();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            if (!peakFile)
            {
                File.Delete(@"" + peaksInRegionsFileName);
            }
            if (!xmlFile)
            {
                File.Delete(@"" + xmlFileName);
            }

            Console.WriteLine("done!");
        }

        void filterByChromosome()
        {
            if (chromosome != null)
	        {
                regions.RemoveWhere(x => !chromosome.Contains(x.chromosome));
	        }
        }

        void filterByStartIndex()
        {
            if (startIndex != 0)
            {
                regions.RemoveWhere(x => x.startIndex <= startIndex);
            }
        }

        void filterByEndIndex()
        {
            if (endIndex != 0)
            {
                regions.RemoveWhere(x => x.endIndex >= endIndex);
            }
        }

        void filterByRegName()
        {
            if (regsName != null)
            {
                regions.RemoveWhere(x => !regsName.Contains(x.regionName));
            }
        }

        void filterByTfName()
        {
            if (tfName != null)
            {
                regions.RemoveWhere(x => !x.peakList.Any(y => tfName.Contains(y.TFname)));
            }
        }

        void filterByPkName()
        {
            if (peakName != null)
            {
                regions.RemoveWhere(x => !x.peakList.Any(y => peakName.Contains(y.peakName)));
            }
        }

        void filterByLowerScore()
        {
            if (lowerScore != 0)
            {
                regions.RemoveWhere(x => x.score <= lowerScore);
            }
        }

        void filterByHigherScore()
        {
            if (higherScore != 0)
            {
                regions.RemoveWhere(x => x.score >= higherScore);
            }
        }

        void filterByStrandSpecificity()
        {
            if (strandSpecificity)
            {
                regions.RemoveWhere(x => x.peakList.Any(y => y.strand != x.strand));
            }
        }

        region calculateStatisticData(region examinedRegion, region previousRegion)
        {
            #region peaksPerChromosome
            statistics.addToPeaksPerChromosomePost(examinedRegion.chromosome, examinedRegion.peakList.Count);
            #endregion
            #region peakInRegionDistance & allPeakDistance & tfStatsPost
            int peakIndex = 1;
            foreach (peak p in peakSorting(examinedRegion.peakList, true))
            {
                statistics.allPeaksDistance.Add(1);
                if (p.startIndex == 1 && p.endIndex == 1)
                {
                    statistics.peaksInRegionDistance.Add(1);
                }
                else
                {
                    if (peakIndex < examinedRegion.peakList.Count)
                    {
                        statistics.peaksInRegionDistance.Add((examinedRegion.peakList[peakIndex].startIndex + examinedRegion.peakList[peakIndex].summit) - (p.startIndex + p.summit));
                    }
                }
                peakIndex++;

                statistics.addToTfStatsPost(p.TFname, new List<int>() { p.endIndex - p.startIndex }, 1);
            }
            #endregion
            #region regionsPerChromosome
            statistics.addToRegionsPerChromosome(examinedRegion.chromosome, 1);
            #endregion
            #region regionScore
            statistics.addToRegionScore(new List<int>() { examinedRegion.endIndex - examinedRegion.startIndex }, new List<double>() { examinedRegion.score }, 1);
            #endregion
            #region regionLength
            statistics.addToRegionLength(new List<int>() { examinedRegion.endIndex - examinedRegion.startIndex }, new List<double>() { examinedRegion.score }, 1);
            #endregion
            #region allRegionsDistance
            if (previousRegion == null)
            {
                return examinedRegion;
            }
            if (previousRegion.chromosome == examinedRegion.chromosome)
            {
                statistics.allRegionsDistance.Add(examinedRegion.startIndex - previousRegion.endIndex);
            }
            #endregion
            return examinedRegion;
        }
    }
}
