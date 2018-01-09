using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace tfNet
{
    public class filtAndReg : filtAndNet
    {
        #region parameters
        public int topX { get; set; } //keep the X highest ranked regions
        public double topXP { get; set; } //keep the X% highest ranked regions
        //true: ascending; false: descending; null: noSort
        public bool? sortRegionsBySore { get; set; } //sort the output regions according to score
        public bool strandSpecificity { get; set; } //create regions based on strand and distance
        public bool statFile { get; set; } //print statistics file for regions and peaks
        public bool xmlFile { get; set; } //print xml file for regions
        public bool peakFile { get; set; } //print combined peaks file
        //true: summit to summit; false: end to start
        public bool lowMemory { get; set; }
        #endregion

        #region filtering parameters
        public List<string> chromosome { get; set; } //keep the given chromosomes
        public int startIndex { get; set; } //keep regions starting after startIndex
        public int endIndex { get; set; } //keep regions ending before endIndex
        public List<string> regsName { get; set; } //keep the given regions
        public List<string> tfName { get; set; } //keep regions containing these TFs
        public List<string> peakName { get; set; } //keep regions containing these peaks
        public int lowerScore { get; set; } //keep regions with score higher than this
        public int higherScore { get; set; } //keep regions with score lower than this
        #endregion

        public filtAndReg(string _mode)
            : base(_mode)
        {
            if (lowMemory)
            {
                sortRegionsBySore = null;
                topX = -1;
                topXP = -1;
            }
        }

        /// <summary>
        /// Writes a genomic region into the xml file.
        /// </summary>
        /// <param name="writer">Writer.</param>
        /// <param name="reg">Reg.</param>
        /// <param name="numOfCols">Number of cols.</param>
        public void writeRegionInXML(XmlWriter writer, region reg)
        {
            #region write region
            writer.WriteStartElement("reg");
            writer.WriteAttributeString("chr", reg.chromosome);
            writer.WriteAttributeString("s", Convert.ToString(reg.startIndex));
            writer.WriteAttributeString("e", Convert.ToString(reg.endIndex));
            writer.WriteAttributeString("n", reg.name);
            writer.WriteAttributeString("scr", Convert.ToString(reg.score));
            writer.WriteAttributeString("strd", Convert.ToString(reg.strand));
            writer.WriteAttributeString("pv", Convert.ToString(reg.pValue));
            writer.WriteAttributeString("qv", Convert.ToString(reg.qValue));
            #endregion

            #region peak
            foreach (peak pk in reg.peakList)
            {
                writer.WriteStartElement("pk");
                switch (numOfCols)
                {
                    case 3:
                        writer.WriteAttributeString("chr", pk.chromosome);
                        writer.WriteAttributeString("s", Convert.ToString(pk.startIndex));
                        writer.WriteAttributeString("e", Convert.ToString(pk.endIndex));
                        //writer.WriteAttributeString("cut", Convert.ToString(pk.cutoff));
                        break;
                    case 4:
                        writer.WriteAttributeString("chr", pk.chromosome);
                        writer.WriteAttributeString("s", Convert.ToString(pk.startIndex));
                        writer.WriteAttributeString("e", Convert.ToString(pk.endIndex));
                        writer.WriteAttributeString("n", pk.name);
                        //writer.WriteAttributeString("cut", Convert.ToString(pk.cutoff));
                        break;
                    case 5:
                        writer.WriteAttributeString("chr", pk.chromosome);
                        writer.WriteAttributeString("s", Convert.ToString(pk.startIndex));
                        writer.WriteAttributeString("e", Convert.ToString(pk.endIndex));
                        writer.WriteAttributeString("n", pk.name);
                        writer.WriteAttributeString("scr", Convert.ToString(pk.score));
                        //writer.WriteAttributeString("cut", Convert.ToString(pk.cutoff));
                        break;
                    case 6:
                        writer.WriteAttributeString("chr", pk.chromosome);
                        writer.WriteAttributeString("s", Convert.ToString(pk.startIndex));
                        writer.WriteAttributeString("e", Convert.ToString(pk.endIndex));
                        writer.WriteAttributeString("n", pk.name);
                        writer.WriteAttributeString("scr", Convert.ToString(pk.score));
                        writer.WriteAttributeString("strd", Convert.ToString(pk.strand));
                        //writer.WriteAttributeString("cut", Convert.ToString(pk.cutoff));
                        break;
                    case 7:
                        writer.WriteAttributeString("chr", pk.chromosome);
                        writer.WriteAttributeString("s", Convert.ToString(pk.startIndex));
                        writer.WriteAttributeString("e", Convert.ToString(pk.endIndex));
                        writer.WriteAttributeString("n", pk.name);
                        writer.WriteAttributeString("scr", Convert.ToString(pk.score));
                        writer.WriteAttributeString("strd", Convert.ToString(pk.strand));
                        writer.WriteAttributeString("sv", Convert.ToString(pk.signalValue));
                        //writer.WriteAttributeString("cut", Convert.ToString(pk.cutoff));
                        break;
                    case 8:
                        writer.WriteAttributeString("chr", pk.chromosome);
                        writer.WriteAttributeString("s", Convert.ToString(pk.startIndex));
                        writer.WriteAttributeString("e", Convert.ToString(pk.endIndex));
                        writer.WriteAttributeString("n", pk.name);
                        writer.WriteAttributeString("scr", Convert.ToString(pk.score));
                        writer.WriteAttributeString("strd", Convert.ToString(pk.strand));
                        writer.WriteAttributeString("sv", Convert.ToString(pk.signalValue));
                        writer.WriteAttributeString("pv", Convert.ToString(pk.pValue));
                        //writer.WriteAttributeString("cut", Convert.ToString(pk.cutoff));
                        break;
                    case 9:
                        writer.WriteAttributeString("chr", pk.chromosome);
                        writer.WriteAttributeString("s", Convert.ToString(pk.startIndex));
                        writer.WriteAttributeString("e", Convert.ToString(pk.endIndex));
                        writer.WriteAttributeString("n", pk.name);
                        writer.WriteAttributeString("scr", Convert.ToString(pk.score));
                        writer.WriteAttributeString("strd", Convert.ToString(pk.strand));
                        writer.WriteAttributeString("sv", Convert.ToString(pk.signalValue));
                        writer.WriteAttributeString("pv", Convert.ToString(pk.pValue));
                        writer.WriteAttributeString("qv", Convert.ToString(pk.qValue));
                        //writer.WriteAttributeString("cut", Convert.ToString(pk.cutoff));
                        break;
                    case 10:
                        writer.WriteAttributeString("chr", pk.chromosome);
                        writer.WriteAttributeString("s", Convert.ToString(pk.startIndex));
                        writer.WriteAttributeString("e", Convert.ToString(pk.endIndex));
                        writer.WriteAttributeString("n", pk.name);
                        writer.WriteAttributeString("scr", Convert.ToString(pk.score));
                        writer.WriteAttributeString("strd", Convert.ToString(pk.strand));
                        writer.WriteAttributeString("sv", Convert.ToString(pk.signalValue));
                        writer.WriteAttributeString("pv", Convert.ToString(pk.pValue));
                        writer.WriteAttributeString("qv", Convert.ToString(pk.qValue));
                        writer.WriteAttributeString("sm", Convert.ToString(pk.summit));
                        //writer.WriteAttributeString("cut", Convert.ToString(pk.cutoff));
                        break;
                    default:
                        exit("something went wrong while printing xml");
                        break;
                }
                writer.WriteEndElement();
            }
            #endregion
            writer.WriteEndElement();
        }
        
        public List<region> filterTopRegions(List<region> regionList)
        {
            if (topXP > 0.0)
            {
                topX = Convert.ToInt32((topXP * regionList.Count) / 100);
            }
            if (topX != 0)
            {
                return regionList.OrderByDescending(orderByScore => orderByScore.score).Take(topX).ToList();
            }
            return regionList;
        }

        /// <summary>
        /// Sort regions.
        /// </summary>
        /// <returns>The sorted list of regions.</returns>
        /// <param name="inputRegionsList">Input regions list.</param>
        /// <param name="sortMode">Sort mode: null: natural sorting; true: ascending sorting; false: descending sroting</param>
        public List<region> regionSorting(List<region> inputRegionsList, bool? sortMode)
        {
            if (sortMode == null)
            {
                return inputRegionsList;
            }
            else if (sortMode == true)
            {
                return inputRegionsList.AsEnumerable().OrderBy(sortByScore => sortByScore.score).ToList();
            }
            else if (sortMode == false)
            {
                return inputRegionsList.AsEnumerable().OrderByDescending(sortByScore => sortByScore.score).ToList();
            }
            return null;
        }

        //print the region given when the method is called
        public void printRegion(region r, StreamWriter outputRegion, StreamWriter outputPeak)
        {
            printPeak(new peak()
            {
                chromosome = r.chromosome,
                startIndex = r.startIndex,
                endIndex = r.endIndex,
                name = r.name,
                score = r.score,
                strand = r.strand,
                signalValue = -1,
                pValue = (r.pValue == 0.0) ? -1 : r.pValue,
                qValue = (r.qValue == 0.0) ? -1 : r.qValue,
                summit = -1
            }, outputRegion);
            if (peakFile)
            {
                foreach (peak pk in r.peakList)
                {
                    printPeak(pk, outputPeak);
                }
            }
        }

        public void printRegXmlPeakFile(List<region> regionList)
        {
            if (!lowMemory)
            {
                Console.Write("printing results...");

				if (xmlFile) {
					string xmlFileName = resultsDirectory + OSseparator + outfileName + "_regions.xml";
					XmlWriterSettings settings = new XmlWriterSettings ();
					settings.Indent = true;
					settings.IndentChars = "\t";
					XmlWriter writer = XmlWriter.Create (@"" + xmlFileName, settings);
					writer.WriteStartDocument ();
					writer.WriteStartElement ("regs");

					string regionsFileName = resultsDirectory + OSseparator + outfileName + "_regions." + fileType;
					StreamWriter outputRegion = new StreamWriter (@"" + regionsFileName);

					if (peakFile)
					{
						string peaksInRegionsFileName = resultsDirectory + OSseparator + outfileName + "_regions_peaks." + fileType;
						StreamWriter outputPeak = new StreamWriter (@"" + peaksInRegionsFileName);

						foreach (region r in regionList)
						{
							printRegion (r, outputRegion, outputPeak);
							writeRegionInXML (writer, r);
						}
						outputPeak.Close ();
					}
					else
					{
						foreach (region r in regionList)
						{
							printRegion (r, outputRegion, null);
							writeRegionInXML (writer, r);
						}
					}
					outputRegion.Close ();
					writer.WriteEndElement ();
					writer.WriteEndDocument ();
					writer.Close ();
				}
				else
				{
					string regionsFileName = resultsDirectory + OSseparator + outfileName + "_regions." + fileType;
					StreamWriter outputRegion = new StreamWriter (@"" + regionsFileName);

					if (peakFile)
					{
						string peaksInRegionsFileName = resultsDirectory + OSseparator + outfileName + "_regions_peaks." + fileType;
						StreamWriter outputPeak = new StreamWriter (@"" + peaksInRegionsFileName);

						foreach (region r in regionList)
						{
							printRegion (r, outputRegion, outputPeak);
						}
						outputPeak.Close ();
					}
					else
					{
						foreach (region r in regionList)
						{
							printRegion (r, outputRegion, null);
						}
					}
					outputRegion.Close ();
				}
            }
            if (statFile)
            {
                statistics.printStatisticData(resultsDirectory, OSseparator, outfileName);
            }
            if (!lowMemory)
            {
                Console.WriteLine(" done!");
            }
        }

		#region filtering on the fly
        public bool filterRegion(region newRegion)
        {
            if (!filterChromosome(newRegion.chromosome))
            {
                return false;
            }
            
            if (!filterStart(newRegion.startIndex))
	        {
		         return false;
	        }

            if (!filterEnd(newRegion.endIndex))
            {
                return false;
            }

            if (!filterRegion(newRegion.regionName))
            {
                return false;
            }

            if (!filterTfName(newRegion.peakList.Select(x => x.TFname).ToList()))
            {
                return false;
            }

            if (!filterPeakName(newRegion.peakList.Select(x => x.peakName).ToList()))
            {
                return false;
            }

            if (!filterLowerScore(newRegion.score))
            {
                return false;
            }

            if (!filterHigherScore(newRegion.score))
            {
                return false;
            }

            if (!filterStrand(newRegion.peakList.Select(x => x.strand).ToList(), newRegion.strand))
            {
                return false;
            }

            return true;
        }

        bool filterChromosome(string chr)
        {
            if (chromosome != null)
            {
                return chromosome.Contains(chr);
            }
            return true;
        }

        bool filterStart(int start)
        {
            if (startIndex != 0)
            {
                return (start >= startIndex) ? true : false;
            }
            return true;
        }

        bool filterEnd(int end)
        {
            if (endIndex != 0)
            {
                return (end <= endIndex) ? true : false;
            }
            return true;
        }

        bool filterRegion(string regionName)
        {
            if (regsName != null)
            {
                return regsName.Contains(regionName);
            }
            return true;
        }

        bool filterTfName(List<string> tfList)
        {
            if (tfName != null)
            {
                return tfList.Any(x => tfName.Contains(x));
            }
            return true;
        }

        bool filterPeakName(List<string> peakNameList)
        {
            if (peakName != null)
            {
                return peakNameList.Any(x => peakName.Contains(x));
            }
            return true;
        }

        bool filterLowerScore(double score)
        {
            if (lowerScore != 0)
            {
                return (score >= lowerScore) ? true : false;
            }
            return true;
        }

        bool filterHigherScore(double score)
        {
            if (higherScore != 0)
            {
                return (score <= higherScore) ? true : false;
            }
            return true;
        }

        bool filterStrand(List<char> strandList, char regStrand)
        {
            if (strandSpecificity)
            {
                if (strandList.Any(x => x != regStrand))
                {
                    return false;
                }
                return true;
            }
            return true;
        }
        #endregion
    }
}
