using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace tfNet
{
    public class peakAndReg : parent
    {
        public peakAndReg(string _mode)
            : base(_mode)
        {

        }
        
        /// <summary>
        /// loads peaks from a given bed format file.
        /// </summary>
        /// <returns>The list of loaded peaks if low memory is NOT selected.</returns>
        /// <param name="peakFile">the bed format peak file to load.</param>
        /// <param name="TFname">the name of the transcription factor the peaks represent.</param>
        /// <param name="output">the output stream to write in.</param>
        public List<peak> loadPeaks(string peakFile, string TFname)
        {
            List<peak> loadedPeaks = new List<peak>();
            peak newPeak;
            int numOfCols = checkNumberOfFieldsInBedFile(peakFile);
            //List<double> listToRank = new List<double>();

            FileStream fs = File.Open(@"" + peakFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BufferedStream bs = new BufferedStream(fs);
            StreamReader sr = new StreamReader(bs);
            string line;
            int lineCounter = 1;
            while ((line = sr.ReadLine()) != null)
            {
                if ((newPeak = peakFromLine(line, numOfCols, peakFile, lineCounter, TFname)) == null)
                {
                    lineCounter++;
                    if ((newPeak = peakFromLine(line, numOfCols, peakFile, lineCounter, TFname)) == null)
                    {
                        exit("wrong file format in the combined peak file " + peakFile.Split(OSseparator).Last());
                    }
                }

                loadedPeaks.Add(newPeak);
                statistics.addToTfStatsPre(newPeak.TFname, new List<int>() { newPeak.endIndex - newPeak.startIndex }, 1);
                statistics.addToPeaksPerChromosomePre(newPeak.chromosome, 1);
                //if (regionPvalue)
                //{
                //    listToRank.Add(newPeak.qValue);
                //}
                lineCounter++;
            }
            sr.Close();
            //if (regionPvalue)
            //{
            //    Dictionary<double, double> pVals = listOfPvalues(listToRank);
            //    int len = loadedPeaks.Count;
            //    double maxQvalue = loadedPeaks.Select (x => x.qValue).Max ();
            //    double m10 = maxQvalue * len;
            //    foreach (peak p in loadedPeaks)
            //    {
            //        p.pValue = p.qValue * pVals[p.qValue] / m10;
            //    }
            //}
            return loadedPeaks;
        }

        //public Dictionary<double, double> listOfPvalues(List<double> listToRank)
        //{
        //    List<Tuple<double, int>> nums = new List<Tuple<double, int>>();
        //    int cnt = 1;
        //    foreach (double i in listToRank.OrderBy(x => x))
        //    {
        //        nums.Add(new Tuple<double, int>(i, cnt++));
        //    }
        //    Dictionary<double, double> final = new Dictionary<double, double>();
        //    nums.GroupBy(x => x.Item1).ToList().ForEach(x => final.Add(x.ElementAt(0).Item1, calculateRank(x.Select(y => y.Item2).ToList())));
        //    return final;
        //}

        //double calculateRank(List<int> num)
        //{
        //    double a = 0;
        //    try
        //    {
        //        a = num.Sum();
        //    }
        //    catch (OverflowException)
        //    {
        //        foreach (int i in num)
        //        {
        //            a += i;
        //        }
        //    }
        //    return Math.Round(a / Convert.ToDouble(num.Count), 2);
        //}

        /// <summary>
        /// Checks the number of fields in bed file.
        /// </summary>
        /// <returns>The number of fields in bed file.</returns>
        /// <param name="fileToCheck">the name of the bed format file to check.</param>
        public int checkNumberOfFieldsInBedFile(string fileToCheck)
        {
            FileStream fs = File.Open(@"" + fileToCheck, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BufferedStream bs = new BufferedStream(fs);
            StreamReader sr = new StreamReader(bs);
            int inputFileLength = 0;
            string line = sr.ReadLine();
            Regex checkBedFormat = new Regex("^[A-Za-z0-9]+$");
            if (!checkBedFormat.IsMatch(line.Split('\t').First()))
            {
                line = sr.ReadLine();
            }
            if (checkBedFormat.IsMatch(line.Split('\t').First()))
            {
                inputFileLength = line.Split('\t').Length;
            }
            else
            {
                exit("file " + fileToCheck.Split(OSseparator).Last() + " not in bed format");
            }
            //if (!line.StartsWith("chr"))
            //{
            //    line = sr.ReadLine();
            //}
            //if (line.StartsWith("chr"))
            //    inputFileLength = line.Split('\t').Length;
            //else
            //{
            //    exit("file " + fileToCheck.Split(OSseparator).Last() + " not in bed format");
            //}
            sr.Close();

            if (inputFileLength < 3 || inputFileLength > 10)
            {
                exit("the file " + fileToCheck.Split(OSseparator).Last() + " has a non-acceptable format");
            }

            return inputFileLength;
        }

        /// <summary>
        /// Sort peaks.
        /// </summary>
        /// <returns>The sorted list of peaks.</returns>
        /// <param name="inputPeakList">Input peak list.</param>
        /// <param name="narrowThePeak"></param>
        /// <param name="peakPlusSummit"></param>
        /// if true: natural sort by start + summit
        /// if false: natural sort by start + middle
        /// if null: natural sort by start
        public List<peak> peakSorting(List<peak> inputPeakList, bool? sortOption)
        {
            Dictionary<string, List<peak>> myDict = inputPeakList.GroupBy(x => x.chromosome)
                .AsEnumerable().OrderBy(x => x.Key, new NaturalSortComparer<string>()).ToDictionary(x => x.Key, x => x.ToList());
            List<peak> sortedListOfPeaks = new List<peak>();
            foreach (KeyValuePair<string, List<peak>> peaksPerChromosome in myDict)
            {
                sortedListOfPeaks.AddRange(peakSortingHelp(peaksPerChromosome.Value, sortOption));
            }
            return sortedListOfPeaks;
        }

        static List<peak> peakSortingHelp(IEnumerable<peak> inputPeakList, bool? sortOption)
        {
            if (sortOption == true)
            {
                return inputPeakList.OrderBy(sortByStartPlusSummit => (sortByStartPlusSummit.startIndex + sortByStartPlusSummit.summit)).ThenBy(sortByEnd => sortByEnd.endIndex).ToList();               
            }
            else if (sortOption == false)
            {
                return inputPeakList.OrderBy(sortByStartPlusMiddle => (sortByStartPlusMiddle.startIndex + sortByStartPlusMiddle.middle)).ThenBy(sortByEnd => sortByEnd.endIndex).ToList();
            }
            else
            {
                return inputPeakList.OrderBy(sortByStart => sortByStart.startIndex).ThenBy(sortByEnd => sortByEnd.endIndex).ToList();
            }
        }
    }

    public class NumberRank
    {
        public double Number { get; set; }
        public int Rank { get; set; }
        public int numberIndex { get; set; }

        public NumberRank(double number, int rank, int indx)
        {
            Number = number;
            Rank = rank;
            numberIndex = indx;
        }
    }
}
