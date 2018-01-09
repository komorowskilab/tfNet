using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace tfNet
{
    public class parent : masterClass
    {
        #region paramaters
        public string outfileName { get; set; }
        public string fileType { get; set; }
        public List<string> tfList { get; set; }
        public int numOfCols { get; set; }
        public int summitWindow { get; set; }
        public bool narrowThePeak { get; set; }
        public bool fileNameAsTfName { get; set; }
        //public bool regionPvalue { get; set; }
        //public bool minusLog10 { get; set; }

        #region for combinePeaks
        public int filterByScore { get; set; }
        public double filterByPvalue { get; set; }
        public double filterByQvalue { get; set; }
        public double filterBySV { get; set; }
        public bool noValueAssigned { get; set; }
        public bool unknownSummit { get; set; }
        #endregion
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="tfNet_X.parent"/> class.
        /// Takes arguments given by as parameter in order to change its variables.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public parent(string _mode) 
            : base(_mode)
        {
            
        }

        /// <summary>
        /// Prints the given peak.
        /// </summary>
        /// <param name="prntPk">the peak to print.</param>
        /// <param name="output">the output stream where to print the peak.</param>
        public void printPeak(peak prntPk, StreamWriter output)
        {
            switch (numOfCols)
            {
                case 3:
                    output.WriteLine(string.Format("{0}\t{1}\t{2}", prntPk.chromosome, prntPk.startIndex, prntPk.endIndex));
                    break;
                case 4:
                    output.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", prntPk.chromosome, prntPk.startIndex, prntPk.endIndex, prntPk.name));
                    break;
                case 5:
                    output.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", prntPk.chromosome, prntPk.startIndex, prntPk.endIndex, prntPk.name, prntPk.score));
                    break;
                case 6:
                    output.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", prntPk.chromosome, prntPk.startIndex, prntPk.endIndex, prntPk.name, prntPk.score, prntPk.strand));
                    break;
                case 7:
                    output.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", prntPk.chromosome, prntPk.startIndex, prntPk.endIndex, prntPk.name, prntPk.score, prntPk.strand, prntPk.signalValue));
                    break;
                case 8:
                    output.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", prntPk.chromosome, prntPk.startIndex, prntPk.endIndex, prntPk.name, prntPk.score, prntPk.strand, prntPk.signalValue, prntPk.pValue));
                    break;
                case 9:
                    output.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", prntPk.chromosome, prntPk.startIndex, prntPk.endIndex, prntPk.name, prntPk.score, prntPk.strand, prntPk.signalValue, prntPk.pValue, prntPk.qValue));
                    break;
                case 10:
                    output.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}", prntPk.chromosome, prntPk.startIndex, prntPk.endIndex, prntPk.name, prntPk.score, prntPk.strand, prntPk.signalValue, prntPk.pValue, prntPk.qValue, prntPk.summit));
                    break;
                default:
                    exit("something went wrong while printing");
                    break;
            }
        }

        /// <summary>
        /// Creates a peak instance from a bed file line.
        /// </summary>
        /// <returns>The generated peak.</returns>
        /// <param name="line">bed format line.</param>
        /// <param name="numOfCols">number of columns.</param>
        /// <param name="peakFile">the name of the input peak file. Used for error checking purposes.</param>
        /// <param name="lineCounter">line counter of the peak file. Used for error checking purposes.</param>
        /// <param name="TFname">transcription factor name.</param>
        public peak peakFromLine(string line, int numOfCols, string peakFile, int lineCounter, string TFname)
        {
            int tmpInt;
            double tmpDbl;
            peak newPeak = new peak();
            List<string> breakAtTab = line.Split('\t').ToList();

            #region check num of cols
            if (breakAtTab.Count != numOfCols)
            {
                exit("the file " + peakFile.Split(OSseparator).Last() + " has different number on columns in line " + lineCounter);
            }
            #endregion

            #region test chromosome 0
            if (!chromosomeNamesAndLength.ContainsKey(breakAtTab[0]) && !ignoreChromosomeLength)
            {
                exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 1 line " + lineCounter);
            }
            else
            {
                newPeak.chromosome = breakAtTab[0];
            }
            #endregion

            #region test start position 1
            if (!int.TryParse(breakAtTab[1], out tmpInt))
            {
                exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 2 line " + lineCounter + ". Integer expected");
            }
            if (tmpInt < 0)
            {
                exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 2 line " + lineCounter + ". Positive expected");
            }
            newPeak.startIndex = tmpInt;
            #endregion

            #region test end position 2
            if (!int.TryParse(breakAtTab[2], out tmpInt))
            {
                exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 3 line " + lineCounter + ". Integer expected");
            }
            if (tmpInt < 0)
            {
                exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 3 line " + lineCounter + ". Positive expected");
            }
            if (!ignoreChromosomeLength)
            {
                if (tmpInt > chromosomeNamesAndLength[breakAtTab[0]])
                {
                    exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 3 line " + lineCounter + ". Exceeding chromosome's limits");
                }
            }
            newPeak.endIndex = tmpInt;
            #endregion

            #region check for equality betwen start and end
            if (newPeak.startIndex == newPeak.endIndex)
            {
                exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in line " + lineCounter + ". Start is equal to end index!");
            }
            #endregion

            #region test name 3
            if (mode == "regions" || mode == "tfNet" || mode == "peaks")
            {
                newPeak.peakName = "peak" + lineCounter;
                newPeak.TFname = TFname;
                if (numOfCols > 3)
                {
                    newPeak.name = newPeak.TFname + "_" + newPeak.peakName + "_" + breakAtTab[3];
                }
                else
                {
                    newPeak.name = newPeak.TFname + "_" + newPeak.peakName;
                }

                if (mode == "regions")
                {
                    if (breakAtTab[3].Split('_').Length > 1)
                    {
                        newPeak.TFname = breakAtTab[3].Split('_').First();
                        newPeak.peakName = breakAtTab[3].Split('_').ElementAt(1);
                        newPeak.name = newPeak.TFname + "_" + newPeak.peakName;
                    }
                }
            }
            else
            {
                newPeak.name = breakAtTab[3];
            }
            #endregion

            #region test score 4
            if (numOfCols > 4)
            {
                if (!double.TryParse(breakAtTab[4], out tmpDbl))
                {
                    exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 5 line " + lineCounter + ". Integer expected");
                }
                if (filterByScore != 0 && tmpDbl < filterByScore)
                {
                    return null;
                }
                newPeak.score = tmpDbl;
            }
            else
            {
                newPeak.score = 0;
            }
            #endregion

            #region test strand 5
            if (numOfCols > 5)
            {
                if (breakAtTab[5].Length == 0)
                {
                    exit("the file " + peakFile.Split(OSseparator).Last() + " has an empty element in column 6 line " + lineCounter + ". +/-/. expected");
                }
                if (!strandSymbols.Exists(x => x == breakAtTab[5][0]))
                {
                    exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 6 line " + lineCounter + ". +/-/. expected");
                }
                newPeak.strand = breakAtTab[5][0];
            }
            else
            {
                newPeak.strand = '.';
            }
            #endregion

            #region test signalValue 6
            if (numOfCols > 6)
            {
                if (!double.TryParse(breakAtTab[6], out tmpDbl))
                {
                    exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 7 line " + lineCounter + ". Numeric expected");
                }
                if (!(mode == "filter" || mode == "network"))
                {
                    if (filterBySV != 0 && tmpDbl < filterBySV)
                    {
                        return null;
                    }
                }
                newPeak.signalValue = tmpDbl;
            }
            else
            {
                newPeak.signalValue = 0;
            }
            #endregion

            #region test pValue 7
            if (numOfCols > 7)
            {
                if (!double.TryParse(breakAtTab[7], out tmpDbl))
                {
                    exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 8 line " + lineCounter + ". Numeric expected");
                }
                if (!(mode == "filter" || mode == "network"))
                {
                    if (filterByPvalue != 0 && tmpDbl > filterByPvalue)
                    {
                        return null;
                    }
                    if (tmpDbl == -1.0 && !noValueAssigned)
                    {
                        return null;
                    }
                }
                newPeak.pValue = tmpDbl;
                //if (tmpDbl != -1 && regionPvalue)
                //{
                //    if (minusLog10)
                //    {
                //        newPeak.pValue = Math.Pow(10, -tmpDbl);
                //    }
                //    else
                //    {
                //        newPeak.pValue = tmpDbl;
                //    }
                //}
                //else
                //{
                //    newPeak.pValue = tmpDbl;
                //}
            }
            else
            {
                newPeak.pValue = -1;
            }
            #endregion

            #region test qValue 8
            if (numOfCols > 8)
            {
                if (!double.TryParse(breakAtTab[8], out tmpDbl))
                {
                    exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 9 line " + lineCounter + ". Numeric expected");
                }
                if (!(mode == "filter" || mode == "network"))
                {
                    if (filterByQvalue != 0 && tmpDbl > filterByQvalue)
                    {
                        return null;
                    }
                    if (tmpDbl == -1.0 && !noValueAssigned)
                    {
                        return null;
                    }
                }
                newPeak.qValue = tmpDbl;
                //if (tmpDbl != -1 && regionPvalue)
                //{
                //    if (minusLog10)
                //    {
                //        newPeak.qValue = Math.Pow(10, -tmpDbl);
                //    }
                //    else
                //    {
                //        newPeak.qValue = tmpDbl;
                //    }
                //}
                //else
                //{
                //    newPeak.qValue = tmpDbl;
                //}
            }
            else
            {
                newPeak.qValue = -1;
            }
            #endregion

            #region test summit 9
            if (numOfCols > 9)
            {
                if (!int.TryParse(breakAtTab[9], out tmpInt))
                {
                    exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 10 line " + lineCounter + ". Integer expected");
                }
                if (tmpInt < 0 && tmpInt != -1)
                {
                    exit("the file " + peakFile.Split(OSseparator).Last() + " has an invalid element in column 10 line " + lineCounter + ". Positive expected");
                }
                if (!(mode == "filter" || mode == "network"))
                {
                    if (!unknownSummit && tmpInt < 1)
                    {
                        return null;
                    }
                }
                if (tmpInt == -1)
                {
                    newPeak.summit = narrowThePeak ? summitWindow : middlePoint(newPeak.startIndex, newPeak.endIndex);
                }
                else
                {
                    newPeak.summit = tmpInt;
                }
            }
            else if (!(mode == "filter" || mode == "network"))
            {
                if (!unknownSummit)
                {
                    return null;
                }
            }
            else
            {
                newPeak.summit = narrowThePeak ? summitWindow : middlePoint(newPeak.startIndex, newPeak.endIndex);
            }
            #endregion

            newPeak.middle = narrowThePeak ? summitWindow : middlePoint(newPeak.startIndex, newPeak.endIndex);
            newPeak.startIndex = narrowThePeak ? newStartIndex(newPeak.startIndex, newPeak.summit, summitWindow) : newPeak.startIndex;
            newPeak.endIndex = narrowThePeak ? newEndIndex(newPeak.startIndex, newPeak.summit, summitWindow, newPeak.endIndex, newPeak.chromosome) : newPeak.endIndex;

            return newPeak;
        }

        /// <summary>
        /// New end index.
        /// </summary>
        /// <returns>The new end index.</returns>
        /// <param name="start">Start.</param>
        /// <param name="summit">Summit.</param>
        /// <param name="ps">plus minus from the summit.</param>
        /// <param name="end">End.</param>
        int newEndIndex(int start, int summit, int ps, int end, string chr)
        {
            if ((start + summit + ps < end) && (ps > 0))
            {
                if (!ignoreChromosomeLength)
                {
                    if (((start + summit + ps) < chromosomeNamesAndLength[chr]))
                    {
                        
                    }
                    return start + summit + ps;
                }
                else if (ignoreChromosomeLength)
                {
                    return start + summit + ps;
                }
                else
                {
                    return end;
                }
            }
            else
            {
                return end;
            }
        }

        /// <summary>
        /// New start index.
        /// </summary>
        /// <returns>The start index.</returns>
        /// <param name="start">Start.</param>
        /// <param name="summit">Summit.</param>
        /// <param name="ps">plus minus from the summit.</param>
        int newStartIndex(int start, int summit, int ps)
        {
            if ((start + summit - ps > start) && (ps > 0))
                return start + summit - ps;
            else
                return start;
        }

        int middlePoint(int start, int end)
        {
            return Convert.ToInt32((end - start) / 2);
        }
    }
}

