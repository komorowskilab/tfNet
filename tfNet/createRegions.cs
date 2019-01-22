using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Threading;

namespace tfNet
{
    public class createRegions : filtAndReg
    {
        #region parameters
        public int peakDistance { get; set; } //distance from peak to peak
        public string combinedPeakfile { get; set; } //the name of the combined peak file
        //true: summit to summit; false: end to start
        public bool distanceOption { get; set; } //how to estimate the peak distance
        //public double cutoffValue { get; set; }
        #endregion

        public createRegions(string _mode)
            : base(_mode)
        {
            if (mode == "tfNet")
            {
                combinedPeakfile = resultsDirectory + OSseparator + outfileName + "_peaks." + fileType;
            }
        }

        public void start()
        {
            try
            {
                //load peaks and construct regions
                if (lowMemory)
                {
                    Console.WriteLine("WARNING: options -s, -fn and -fp are disabled when low memory usage in enabled!");
                    createRegionsFromList(null);
                }
                else
                {
                    Console.WriteLine("loading peaks...");
                    createRegionsFromList(peakSorting(loadPeaks(combinedPeakfile, null), distanceOption));
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

        /// <summary>
        /// find regions given a peaks list and a distance value
        /// </summary>
        /// <param name="allPeaks">peaks list</param>
        /// <returns></returns>
        public List<region> createRegionsFromList(List<peak> loadedPeaks)
        {
            if (!lowMemory)
            {
                Console.Write("detecting regions...");
            }
            else
            {
                Console.Write("detecting regions serially...");
            }

            #region region detection

            //store per chromosome results
            List<returnLists> rLists = new List<returnLists>();

            #region parallel run
            if (!lowMemory)
            {
                Parallel.ForEach(filterPeaks(loadedPeaks).GroupBy(x => x.chromosome).Select(x => x.ToList()).ToList(), peaksPerChromosome =>
                {
                    rLists.Add(regionFinder(peaksPerChromosome));
                });
            }
            else
            {
                rLists.Add(regionFinderLowMemory());
            }
            #endregion

            #region sequential run
            //if (!lowMemory)
            //{
            //    foreach (List<peak> peaksPerChromosome in filterPeaks(loadedPeaks).GroupBy(x => x.chromosome).Select(x => x.ToList()).ToList())
            //    {
            //        rLists.Add(regionFinder(peaksPerChromosome));
            //    }
            //}
            //else
            //{
            //    rLists.Add(regionFinderLowMemory());
            //}
            #endregion
            #endregion

            #region merge results
            List<region> regionList = mergeResultsForStatistics(rLists);
            #endregion

            #region keep only x X highest ranked
            regionList = filterTopRegions(regionList);
            #endregion

            #region sort if needed
            regionList = regionSorting(regionList, sortRegionsBySore);
            Console.WriteLine(" done!");
            #endregion

            #region printing for non low memory mode
            printRegXmlPeakFile(regionList);
            #endregion

            return regionList;
        }

        /// <summary>
        /// For threads generated for every chromosome
        /// </summary>
        /// <param name="a"></param>
        /// <param name="distance"></param>
        /// <param name="allregions"></param>
        /// </summary>
        /// <returns>data needed for statistics printing</returns>
        returnLists regionFinder(List<peak> listOfPeaks)
        {
            returnLists rLists = new returnLists();
            int pkCounter = 1, realDistance = -1, numOfPeaks = listOfPeaks.Count;
            region newRegion = null;
            peak nextPeak;
            bool openRegion = false;
            List<int> tmpAllPkDist = new List<int>(), tmpPkInRegDist = new List<int>();
            
            rLists.pkPerChr.Add(listOfPeaks.First().chromosome, 0);
            rLists.regPerChr.Add(listOfPeaks.First().chromosome, 0);

            foreach (peak examinedPeak in listOfPeaks)
            {
                #region check for last peak
                if (pkCounter != numOfPeaks)
                { //as long as you are not at the last element of the peak list you have a nextpeak to assign
                    nextPeak = listOfPeaks.ElementAt(pkCounter);
                }
                else if (openRegion) //close the last opened region
                {
                    newRegion = closeTheRegion(newRegion);
                    if (filterRegion(newRegion))
                    {
                        newRegion.peakList = peakSorting(newRegion.peakList, distanceOption);
                        rLists.updateVariables(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak, summitWindow);
                    }
                    tmpAllPkDist = new List<int>();
                    tmpPkInRegDist = new List<int>();
                    break;
                }
                else //you have reached the end of the list and you have no left
                {
                    newRegion = singleRegion(examinedPeak, nextRegionCounter());
                    if (filterRegion(newRegion))
                    {
                        newRegion.peakList = peakSorting(newRegion.peakList, distanceOption);
                        rLists.updateVariables(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak, summitWindow);
                    }
                    tmpAllPkDist = new List<int>();
                    tmpPkInRegDist = new List<int>();
                    break;
                }
                #endregion

                tmpAllPkDist.Add(realDistance = distanceOfConsecutivePeaks(examinedPeak, nextPeak));

                switch (strandSpecificRegionDetectionHelp(examinedPeak.strand, nextPeak.strand, realDistance, openRegion))
                {
                    case 1:
                        newRegion = openNewRegion(examinedPeak, nextPeak, nextRegionCounter(), '.');
                        openRegion = true;
                        tmpPkInRegDist.Add(realDistance);
                        pkCounter++;
                        break;
                    case 2:
                        newRegion = addPeakToRegion(nextPeak, newRegion);
                        tmpPkInRegDist.Add(realDistance);
                        pkCounter++;
                        break;
                    case 3:
                        newRegion = singleRegion(examinedPeak, nextRegionCounter());
                        if (filterRegion(newRegion))
                        {
                            newRegion.peakList = peakSorting(newRegion.peakList, distanceOption);
                            rLists.updateVariables(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak, summitWindow);
                        }
                        tmpAllPkDist = new List<int>();
                        tmpPkInRegDist = new List<int>();
                        pkCounter++;
                        break;
                    case 4:
                        newRegion = closeTheRegion(newRegion);
                        if (filterRegion(newRegion))
                        {
                            newRegion.peakList = peakSorting(newRegion.peakList, distanceOption);
                            rLists.updateVariables(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak, summitWindow);
                        }
                        tmpAllPkDist = new List<int>();
                        tmpPkInRegDist = new List<int>();
                        openRegion = false;
                        pkCounter++;
                        break;
                    default:
                        exit("something went wrong in region creation");
                        break;
                }
            }
            rLists.allRegDist.AddRange(rLists.allPkDist.Where(x => x > peakDistance).ToList());
            rLists.pkPerChr[listOfPeaks.First().chromosome] = rLists.detectedRegs.Sum(x => x.peakList.Count);
            rLists.regPerChr[listOfPeaks.First().chromosome] = rLists.detectedRegs.Count;
            
            return rLists;
        }

        /// <summary>
        /// detect and annotate region based on low memory consumption
        /// </summary>
        /// <returns>data needed for statistics printing</returns>
        returnLists regionFinderLowMemory()
        {
            returnLists rLists = new returnLists();
            int pkCounter = 1, realDistance = -1, numOfCols = checkNumberOfFieldsInBedFile(combinedPeakfile);
            region newRegion = null;
            peak firstPeak, nextPeak;
            bool openRegion = false;
            FileStream fs = File.Open(@"" + combinedPeakfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BufferedStream bs = new BufferedStream(fs);
            StreamReader sr = new StreamReader(bs);
            string firstLine = sr.ReadLine(), nextLine;
            Dictionary<string, int> checkIfSorted = new Dictionary<string, int>();
            List<int> tmpAllPkDist = new List<int>(), tmpPkInRegDist = new List<int>();

            #region printing results
            string regionsFileName = resultsDirectory + OSseparator + outfileName + "_regions." + fileType;
            StreamWriter outputRegion = new StreamWriter(@"" + regionsFileName);

            string peaksInRegionsFileName = resultsDirectory + OSseparator + outfileName + "_regions_peaks." + fileType;
            StreamWriter outputPeak = new StreamWriter(@"" + peaksInRegionsFileName);

            string xmlFileName = resultsDirectory + OSseparator + outfileName + "_regions.xml";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            XmlWriter writer = XmlWriter.Create(@"" + xmlFileName, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("regs");
            #endregion

            #region load the very first peak
            if ((firstPeak = peakFromLine(firstLine, numOfCols, combinedPeakfile, pkCounter, null)) == null)
            {
                firstLine = sr.ReadLine();
                pkCounter++;
                if ((firstPeak = peakFromLine(firstLine, numOfCols, combinedPeakfile, pkCounter, null)) == null)
                {
                    exit("wrong file format in the combined peak file " + combinedPeakfile.Split(OSseparator).Last());
                }
            }
            //check sorted
            if (checkIfSorted.ContainsKey(firstPeak.chromosome))
            {
                if (checkIfSorted[firstPeak.chromosome] > checkSorting(firstPeak))
                {
                    exit("the combined peak file " + combinedPeakfile.Split(OSseparator).Last() + " is not sorted properly! line: " + pkCounter);
                }
                else
                {
                    checkIfSorted[firstPeak.chromosome] = checkSorting(firstPeak);
                }
            }
            else
            {
                checkIfSorted.Add(firstPeak.chromosome, checkSorting(firstPeak));
            }

            statistics.addToPeaksPerChromosomePre(firstPeak.chromosome, 1);
            statistics.addToTfStatsPre(firstPeak.TFname, new List<int>() { firstPeak.endIndex - firstPeak.startIndex }, 1);
            #endregion

            #region check format
            if (numOfCols < 3 || numOfCols > 10)
            {
                exit("the file " + combinedPeakfile.Split(OSseparator).Last() + " has a non-acceptable format");
            }
            #endregion

            while ((nextLine = sr.ReadLine()) != null)
            {
                pkCounter++;
                nextPeak = peakFromLine(nextLine, numOfCols, combinedPeakfile, pkCounter, null); //as long as you are not at the last element of the peak list you have a nextpeak to assign

                #region check for sorting
                if (checkIfSorted.ContainsKey(nextPeak.chromosome))
                {
                    if (checkIfSorted[nextPeak.chromosome] > checkSorting(nextPeak))
                    {
                        exit("the combined peak file " + combinedPeakfile.Split(OSseparator).Last() + " is not sorted properly! line: " + pkCounter);
                    }
                    else
                    {
                        checkIfSorted[nextPeak.chromosome] = checkSorting(nextPeak);
                    }
                }
                else
                {
                    checkIfSorted.Add(nextPeak.chromosome, checkSorting(nextPeak));
                }

                statistics.addToPeaksPerChromosomePre(firstPeak.chromosome, 1);
                statistics.addToTfStatsPre(firstPeak.TFname, new List<int>() { firstPeak.endIndex - firstPeak.startIndex }, 1);
                #endregion

                #region change chromosome
                if (nextPeak.chromosome != firstPeak.chromosome)
                {
                    if (openRegion) //close the last opened region
                    {
                        newRegion = closeTheRegion(newRegion);
                        if(filterRegion(newRegion))
                        {
                            printRegion(newRegion, outputRegion, outputPeak);
                            writeRegionInXML(writer, newRegion);
                            rLists.updateVariablesLowMemory(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak);
                        }
                        tmpAllPkDist = new List<int>();
                        tmpPkInRegDist = new List<int>();
                        openRegion = false;
                    }
                    else //you have reached the end of the list and you have no left
                    {
                        newRegion = singleRegion(firstPeak, nextRegionCounter());
                        if (filterRegion(newRegion))
                        {
                            printRegion(newRegion, outputRegion, outputPeak);
                            writeRegionInXML(writer, newRegion);
                            rLists.updateVariablesLowMemory(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak);
                        }
                        tmpAllPkDist = new List<int>();
                        tmpPkInRegDist = new List<int>();
                    }
                    firstPeak = nextPeak;
                    nextLine = sr.ReadLine();
                    nextPeak = peakFromLine(nextLine, numOfCols, combinedPeakfile, pkCounter, null);

                    statistics.addToPeaksPerChromosomePre(firstPeak.chromosome, 1);
                    statistics.addToTfStatsPre(firstPeak.TFname, new List<int>() { firstPeak.endIndex - firstPeak.startIndex }, 1);
                }
                #endregion

                tmpAllPkDist.Add(realDistance = distanceOfConsecutivePeaks(firstPeak, nextPeak));

                switch (strandSpecificRegionDetectionHelp(firstPeak.strand, nextPeak.strand, realDistance, openRegion))
                {
                    case 1:
                        newRegion = openNewRegion(firstPeak, nextPeak, nextRegionCounter(), '.');
                        openRegion = true;
                        tmpPkInRegDist.Add(realDistance);
                        break;
                    case 2:
                        newRegion = addPeakToRegion(nextPeak, newRegion);
                        tmpPkInRegDist.Add(realDistance);
                        break;
                    case 3:
                        newRegion = singleRegion(firstPeak, nextRegionCounter());
                        if (filterRegion(newRegion))
                        {
                            printRegion(newRegion, outputRegion, outputPeak);
                            writeRegionInXML(writer, newRegion);
                            rLists.updateVariablesLowMemory(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak);
                        }
                        tmpAllPkDist = new List<int>();
                        tmpPkInRegDist = new List<int>();
                        break;
                    case 4:
                        newRegion = closeTheRegion(newRegion);
                        if (filterRegion(newRegion))
                        {
                            printRegion(newRegion, outputRegion, outputPeak);
                            writeRegionInXML(writer, newRegion);
                            rLists.updateVariablesLowMemory(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak);
                        }
                        tmpAllPkDist = new List<int>();
                        tmpPkInRegDist = new List<int>();
                        openRegion = false;
                        break;
                    default:
                        exit("something went wrong in region creation");
                        break;
                }
                firstPeak = nextPeak;
            }
            sr.Close();

            #region last line of the input file
            if (openRegion) //close the last opened region
            {
                newRegion = closeTheRegion(newRegion);
                if (filterRegion(newRegion))
                {
                    printRegion(newRegion, outputRegion, outputPeak);
                    writeRegionInXML(writer, newRegion);
                    rLists.updateVariablesLowMemory(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak);
                }
                tmpAllPkDist = new List<int>();
                tmpPkInRegDist = new List<int>();
            }
            else //you have reached the end of the list and you have no left
            {
                newRegion = singleRegion(firstPeak, nextRegionCounter());
                if (filterRegion(newRegion))
                {
                    printRegion(newRegion, outputRegion, outputPeak);
                    writeRegionInXML(writer, newRegion);
                    rLists.updateVariablesLowMemory(newRegion, tmpAllPkDist, tmpPkInRegDist, narrowThePeak);
                }
                tmpAllPkDist = new List<int>();
                tmpPkInRegDist = new List<int>();
            }
            #endregion

            rLists.allRegDist.AddRange(rLists.allPkDist.Where(x => x > peakDistance).ToList());

            #region closing printings
            outputRegion.Close();
            outputPeak.Close();
            if (!peakFile)
            {
                File.Delete(@"" + peaksInRegionsFileName);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            if (!xmlFile)
            {
                File.Delete(@"" + xmlFileName);
            }
            #endregion

            return rLists;
        }

        /// <summary>
        /// check if the combined peak file is sorted properly
        /// in the following comments the x marks the distance that we consider
        /// in general:
        /// distanceOption is true when we calculate the distance from summit to summit and false when we calculate the distance from midpoint to midpoint
        /// </summary>
        /// <param name="examinedPeak"></param>
        /// <returns>an integer that denotes what to compare</returns>
        int checkSorting(peak examinedPeak)
        {
            if (distanceOption)
            {
                return examinedPeak.startIndex + examinedPeak.summit;
            }
            else
            {
                return examinedPeak.startIndex + examinedPeak.middle;
            }
        }

        /// <summary>
        ///A: strandFirst == strandSecond
        ///B: realDistance <= peakDistance
        ///C: openRegion
        ///L: no strand specificity output
        ///K: strand specificity output needed
        ///1: create a new region
        ///2: add peak to existing region
        ///3: create a signle region
        ///4: close the existing open region
        /// A B C | L K
        /// 0 0 0 | 3 3
        /// 0 0 1 | 4 4
        /// 0 1 0 | 1 3
        /// 0 1 1 | 2 4
        /// 1 0 0 | 3 3
        /// 1 0 1 | 4 4
        /// 1 1 0 | 1 1
        /// 1 1 1 | 2 2
        ///
        ///if (!B AND !C)
        ///	return 3;
        ///else if (!B AND C)
        ///	return 4;
        ///else if (B AND !C)
        ///	if(!strand)
        ///		return 1;
        ///	else
        ///		if (A)
        ///			return 1;
        ///		else
        ///			return 3;
        ///else if (B AND C)
        ///	if(!strand)
        ///		return 2;
        ///	else
        ///		if (A)
        ///			return 2;
        ///		else
        ///			return 4; 
        /// </summary>
        /// <param name="firstPeakStrand">the strand symbol of the examined peak</param>
        /// <param name="nextPeakStrand">the strand symbol of the next peak</param>
        /// <param name="realDistance">the distance between the peaks</param>
        /// <param name="C">a boolean denoting whether there is an open region or not</param>
        /// <returns>an integer that will guide the switch case</returns>
        int strandSpecificRegionDetectionHelp(char firstPeakStrand, char nextPeakStrand, int realDistance, bool C)
        {
            bool A = (firstPeakStrand == nextPeakStrand);
            bool B = (realDistance <= peakDistance);

            if (!B && !C)
            {
                return 3;
            }
            else if (!B && C)
            {
                return 4;
            }
            else if (B && !C)
            {
                if (!strandSpecificity)
                {
                    return 1;
                }
                else
                {
                    return A ? 1 : 3;
                }
            }
            else if (B && C)
            {
                if (!strandSpecificity)
                {
                    return 2;
                }
                else
                {
                    return A ? 2 : 4;
                }
            }
            return -1;
        }

        /// <summary>
        /// returns the distance of two consecutive peaks according to the parameters given by the user
        /// true && true: summit of narrowed peak to summit of narrowed peak
        /// true && false: summit of non-narrowed peak to summit of non-narrowed peak
        /// false && true: start of narrowed peak to end of narrowed peak
        /// false && false: start of non-narrowed peak to end of non-narrowed peak
        /// </summary>
        /// <param name="firstpeak">examined peak</param>
        /// <param name="nextpeak">next peak</param>
        /// <returns>the distance between the peaks based in the given parameters</returns>
        int distanceOfConsecutivePeaks(peak firstpeak, peak nextpeak)
        {
            if (distanceOption)
            {
                return ((nextpeak.startIndex + nextpeak.summit) - (firstpeak.startIndex + firstpeak.summit));
            }
            else
            {
                return ((nextpeak.startIndex + nextpeak.middle) - (firstpeak.startIndex + firstpeak.middle));
            }
        }

        //just to avoid some more lines of code
        //called only from regionFinder and is used only when we detect a region that contains only one peak in it
        region singleRegion(peak peakToAdd, int cnt)
        {
            return new region()
            {
                chromosome = peakToAdd.chromosome,
                regionName = "reg" + cnt,
                name = "reg" + cnt + "-" + peakToAdd.name,
                score = 1,
                peakList = new List<peak>() { peakToAdd },
                strand = strandSpecificity ? peakToAdd.strand : '.',
                startIndex = peakToAdd.startIndex,
                endIndex = peakToAdd.endIndex,
                pValue = peakToAdd.pValue
            };
        }

        //just open a new region
        region openNewRegion(peak firstpeak, peak nextpeak, int cnt, char strd)
        {
            return new region()
            {
                chromosome = firstpeak.chromosome,
                regionName = "reg" + cnt,
                name = "reg" + cnt + "-" + firstpeak.name + "," + nextpeak.name,
                score = 2,
                strand = strd,
                peakList = new List<peak>() { firstpeak, nextpeak }
            };
        }

        //add peak to an existing region
        region addPeakToRegion(peak nextpeak, region openReg)
        {
            openReg.name += "," + nextpeak.name;
            openReg.score++;
            openReg.peakList.Add(nextpeak);
            return openReg;
        }

        //close an open region
        region closeTheRegion(region openReg)
        {
            openReg.startIndex = openReg.peakList.Min(x => x.startIndex);
            openReg.endIndex = openReg.peakList.Max(x => x.endIndex);
            openReg.pValue = fishersMethod(openReg.peakList.Select(x => x.pValue).ToList());
            //double bonferroniCutoff = cutoffValue / openReg.peakList.Count;
            //openReg.peakList.Where(x => x.pValue < bonferroniCutoff).ToList().ForEach(x => x.cutoff = true);
            return openReg;
        }

        //merge results from returnLists to statClass
        private List<region> mergeResultsForStatistics(List<returnLists> rLists)
        {
            List<region> regionList = new List<region>();

            foreach (returnLists rl in rLists.Where(x => x != null))
            {
                if (!lowMemory)
                {
                    regionList.AddRange(rl.detectedRegs);
                }
                statistics.peaksInRegionDistance.AddRange(rl.pkInRegDist);
                statistics.allPeaksDistance.AddRange(rl.allPkDist);
                statistics.allRegionsDistance.AddRange(rl.allRegDist);
                foreach (KeyValuePair<string, int> ppc in rl.pkPerChr)
                {
                    if (statistics.peaksPerChromosomePost.ContainsKey(ppc.Key))
                    {
                        statistics.peaksPerChromosomePost[ppc.Key] += ppc.Value;
                    }
                    else
                    {
                        statistics.peaksPerChromosomePost.Add(ppc.Key, ppc.Value);
                    }
                }
                foreach (KeyValuePair<string, int> rpc in rl.regPerChr)
                {
                    if (statistics.regionsPerChromosome.ContainsKey(rpc.Key))
                    {
                        statistics.regionsPerChromosome[rpc.Key] += rpc.Value;
                    }
                    else
                    {
                        statistics.regionsPerChromosome.Add(rpc.Key, rpc.Value);
                    }
                }
                foreach (KeyValuePair<double, TF> rs in rl.regScr)
                {
                    if (statistics.regionScore.ContainsKey(rs.Key))
                    {
                        statistics.regionScore[rs.Key].numOfPeaks += rs.Value.numOfPeaks;
                        statistics.regionScore[rs.Key].lengths.AddRange(rs.Value.lengths);
                    }
                    else
                    {
                        statistics.regionScore.Add(rs.Key, new TF()
                            {
                                numOfPeaks = rs.Value.numOfPeaks,
                                lengths = new List<int>(rs.Value.lengths)
                            });
                    }
                }
                foreach (KeyValuePair<int, regs> rlen in rl.regLen)
                {
                    if (statistics.regionLength.ContainsKey(rlen.Key))
                    {
                        statistics.regionLength[rlen.Key].numOfPeaks += rlen.Value.numOfPeaks;
                        statistics.regionLength[rlen.Key].lengths.AddRange(rlen.Value.lengths);
                        statistics.regionLength[rlen.Key].score.AddRange(rlen.Value.score);
                    }
                    else
                    {
                        statistics.regionLength.Add(rlen.Key, new regs()
                            {
                                numOfPeaks = rlen.Value.numOfPeaks,
                                lengths = new List<int>(rlen.Value.lengths),
                                score = new List<double>(rlen.Value.score)
                            });
                    }
                    
                }
                foreach (KeyValuePair<string, TF> tfsp in rl.tfsPost)
                {
                    if (statistics.tfStatsPost.ContainsKey(tfsp.Key))
                    {
                        statistics.tfStatsPost[tfsp.Key].lengths.AddRange(tfsp.Value.lengths);
                        statistics.tfStatsPost[tfsp.Key].numOfPeaks += tfsp.Value.numOfPeaks;
                    }
                    else
                    {
                        statistics.tfStatsPost.Add(tfsp.Key, new TF()
                            {
                                lengths = new List<int>(tfsp.Value.lengths),
                                numOfPeaks = tfsp.Value.numOfPeaks
                            });
                    }
                }
                foreach (KeyValuePair<string, Dictionary<string, tfOccurrences>> tfA in rl.tfOccsperChr)
                {
                    foreach (KeyValuePair<string, tfOccurrences> tfB in tfA.Value)
                    {
                        if (tfOccs.ContainsKey(tfA.Key))
                        {
                            if (tfOccs[tfA.Key].ContainsKey(tfB.Key))
                            {
                                tfOccs[tfA.Key][tfB.Key].count += tfB.Value.count;
                                tfOccs[tfA.Key][tfB.Key].distance.AddRange(tfB.Value.distance);
                            }
                            else
                            {
                                tfOccs[tfA.Key].Add(tfB.Key, new tfOccurrences(1));
                                tfOccs[tfA.Key][tfB.Key].addRangeInList(tfB.Value.distance);
                            }
                        }
                        else
                        {
                            tfOccs.Add(tfA.Key, new Dictionary<string, tfOccurrences>() { { tfB.Key, new tfOccurrences(1) } });
                            tfOccs[tfA.Key][tfB.Key].addRangeInList(tfB.Value.distance);
                        }
                    }
                }
            }
            return regionList;
        }

        //global region counter, initially set to 0.
        private static int globalRegionCounter = 0;
        /// <summary>
        /// Sets a unique counter to newly detected regions
        /// </summary>
        /// <returns>The region counter.</returns>
        static int nextRegionCounter()
        {
            return Interlocked.Increment(ref globalRegionCounter);
        }

        /// <summary>
        /// preliminary filtering of peaks based on chromosome, start and end index
        /// </summary>
        /// <param name="loadedPeaks">the loaded list of peaks</param>
        /// <returns>the filtered list of peaks</returns>
        List<peak> filterPeaks(List<peak> loadedPeaks)
        {
            if (chromosome == null)
            {
                return loadedPeaks;
            }
            else
	        {
                return loadedPeaks.Where(x => chromosome.Contains(x.chromosome)).ToList();
	        }
        }

        int peakStartPlusSummit(peak newPeak)
        {
            return newPeak.startIndex + newPeak.summit;
        }

        double fishersMethod(List<double> pValues)
        {
            return (pValues.Any(x => x == -1)) ? -1 : (-2 * pValues.Sum(x => Math.Log(x)));
        }
    }

    public class returnLists
    {
        #region region length
        private Dictionary<int, regs> _regLen;
        public Dictionary<int, regs> regLen { get { return _regLen; } }
        #endregion

        #region region score
        private Dictionary<double, TF> _regScr;
        public Dictionary<double, TF> regScr { get { return _regScr; } }
        #endregion

        #region distance of peaks within regions
        private List<int> _pkInRegDist;
        public List<int> pkInRegDist { get { return _pkInRegDist; } }
        #endregion

        #region distance of all peaks
        private List<int> _allPkDist;
        public List<int> allPkDist { get { return _allPkDist; } }
        #endregion

        #region distance between regions
        private List<int> _allRegDist;
        public List<int> allRegDist { get { return _allRegDist; } }
        #endregion

        #region all regions
        private List<region> _detectedRegs;
        public List<region> detectedRegs { get { return _detectedRegs; } }
        #endregion

        #region regions per chromosome
        private IDictionary<string, int> _regPerChr;
        public IDictionary<string, int> regPerChr { get { return _regPerChr; } }
        #endregion

        #region peaks per chromosome
        private IDictionary<string, int> _pkPerChr;
        public IDictionary<string, int> pkPerChr { get { return _pkPerChr; } }
        #endregion

        #region tfStatsPost
        private Dictionary<string, TF> _tfsPost;
        public Dictionary<string, TF> tfsPost { get { return _tfsPost; } }
        #endregion

        #region tfOccurrences
        private Dictionary<string, Dictionary<string, tfOccurrences>> _tfOccsperChr;
        public Dictionary<string, Dictionary<string, tfOccurrences>> tfOccsperChr { get { return _tfOccsperChr; } }
        #endregion

        public returnLists()
        {
            _regLen = new Dictionary<int, regs>();
            for (int i = 100; i <= 1000; i += 100)
            {
                _regLen.Add(i, new regs());
            }
            _regScr = new Dictionary<double, TF>();
            for (int i = 1; i <= 20; i++)
            {
                _regScr.Add(i, new TF());
            }
            _pkInRegDist = new List<int>();
            _allPkDist = new List<int>();
            _allRegDist = new List<int>();
            _detectedRegs = new List<region>();
            _regPerChr = new Dictionary<string, int>();
            _pkPerChr = new Dictionary<string, int>();
            _tfOccsperChr = new Dictionary<string, Dictionary<string, tfOccurrences>>();
            _tfsPost = new Dictionary<string, TF>();
        }

        public void updateVariables(region newRegion, List<int> pkDist, List<int> pkregDist, bool narrowThePeak, int summitWindow)
        {
            detectedRegs.Add(newRegion);

            pkInRegDist.AddRange(pkregDist);

            allPkDist.AddRange(pkDist);

            double tmpValDbl = statistics.addToRegScr(newRegion.score);
            regScr[tmpValDbl].numOfPeaks++;
            regScr[tmpValDbl].lengths.Add(newRegion.endIndex - newRegion.startIndex);

            int tmpVal = statistics.addToRegLen(newRegion.endIndex - newRegion.startIndex);
            regLen[tmpVal].numOfPeaks++;
            regLen[tmpVal].lengths.Add(newRegion.endIndex - newRegion.startIndex);
            regLen[tmpVal].score.Add(newRegion.score);

            List<Tuple<string, int>> tfsInReg = new List<Tuple<string, int>>();
            int cnt = 1;
            foreach (peak p in newRegion.peakList)
            {
                if (cnt < newRegion.peakList.Count)
                {
                    tfsInReg.Add(new Tuple<string, int>(p.TFname, peakStartPlusSummit(p, narrowThePeak, summitWindow)));
                    prepareNetwork(tfsInReg, newRegion.peakList.ElementAt(cnt));
                }
                if (_tfsPost.ContainsKey(p.TFname))
                {
                    _tfsPost[p.TFname].numOfPeaks++;
                    _tfsPost[p.TFname].lengths.Add(p.endIndex - p.startIndex);
                }
                else
                {
                    _tfsPost.Add(p.TFname, new TF()
                    {
                        numOfPeaks = 1,
                        lengths = new List<int>() { p.endIndex - p.startIndex }
                    });
                }
                cnt++;
            }
        }

        public void updateVariablesLowMemory(region newRegion, List<int> pkDist, List<int> pkregDist, bool narrowThePeak)
        {
            if (regPerChr.ContainsKey(newRegion.chromosome))
            {
                regPerChr[newRegion.chromosome]++;
            }
            else
            {
                regPerChr.Add(newRegion.chromosome, 1);
            }

            if (pkPerChr.ContainsKey(newRegion.chromosome))
            {
                pkPerChr[newRegion.chromosome] += newRegion.peakList.Count;
            }
            else
            {
                pkPerChr.Add(newRegion.chromosome, newRegion.peakList.Count);
            }

            pkInRegDist.AddRange(pkregDist);

            allPkDist.AddRange(pkDist);

            double tmpValDbl = statistics.addToRegScr(newRegion.score);
            regScr[tmpValDbl].numOfPeaks++;
            regScr[tmpValDbl].lengths.Add(newRegion.endIndex - newRegion.startIndex);

            int tmpVal = statistics.addToRegLen(newRegion.endIndex - newRegion.startIndex);
            regLen[tmpVal].numOfPeaks++;
            regLen[tmpVal].lengths.Add(newRegion.endIndex - newRegion.startIndex);
            regLen[tmpVal].score.Add(newRegion.score);

            foreach (peak p in newRegion.peakList)
            {
                if (_tfsPost.ContainsKey(p.TFname))
                {
                    _tfsPost[p.TFname].numOfPeaks++;
                    _tfsPost[p.TFname].lengths.Add(p.endIndex - p.startIndex);
                }
                else
                {
                    _tfsPost.Add(p.TFname, new TF()
                    {
                        numOfPeaks = 1,
                        lengths = new List<int>() { p.endIndex - p.startIndex }
                    });
                }
            }
        }

        public void prepareNetwork(List<Tuple<string, int>> tfsInReg, peak newPeak)
        {
            if (!tfsInReg.Any(x => x.Item1 == newPeak.TFname))
            {
                foreach (Tuple<string, int> s in tfsInReg)
                {
                    if (_tfOccsperChr.ContainsKey(s.Item1))
                    {
                        if (_tfOccsperChr[s.Item1].ContainsKey(newPeak.TFname))
                        {
                            _tfOccsperChr[s.Item1][newPeak.TFname].increaseCount(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2));
                        }
                        else
                        {
                            _tfOccsperChr[s.Item1].Add(newPeak.TFname, new tfOccurrences(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2)));
                        }
                    }
                    else
                    {
                        _tfOccsperChr.Add(s.Item1, new Dictionary<string, tfOccurrences>() { { newPeak.TFname, new tfOccurrences(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2)) } });
                    }

                    if (_tfOccsperChr.ContainsKey(newPeak.TFname))
                    {
                        if (_tfOccsperChr[newPeak.TFname].ContainsKey(s.Item1))
                        {
                            _tfOccsperChr[newPeak.TFname][s.Item1].increaseCount(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2));
                        }
                        else
                        {
                            _tfOccsperChr[newPeak.TFname].Add(s.Item1, new tfOccurrences(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2)));
                        }
                    }
                    else
                    {
                        _tfOccsperChr.Add(newPeak.TFname, new Dictionary<string, tfOccurrences>() { { s.Item1, new tfOccurrences(Math.Abs((newPeak.startIndex + newPeak.summit) - s.Item2)) } });
                    }
                }
            }
        }

        int peakStartPlusSummit(peak newPeak, bool narrowThePeak, int summitWindow)
        {
            return newPeak.startIndex + newPeak.summit;
        }
    }
}