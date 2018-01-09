using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace tfNet
{
    public static class statistics
    {
        #region public variables
        #region per TF statistics
        public static Dictionary<string, TF> tfStatsPre = new Dictionary<string, TF>();

        public static Dictionary<string, int> peaksPerChromosomePre = new Dictionary<string, int>();
        #endregion

        #region peaks' statistics
        public static List<int> peaksInRegionDistance = new List<int>();

        public static List<int> allPeaksDistance = new List<int>();

        public static Dictionary<string, int> peaksPerChromosomePost = new Dictionary<string, int>();

        public static Dictionary<string, TF> tfStatsPost = new Dictionary<string, TF>();
        #endregion

        #region regions' statistics
        public static List<int> allRegionsDistance = new List<int>();

        public static Dictionary<int, regs> regionLength = new Dictionary<int, regs>();

        public static Dictionary<double, TF> regionScore = new Dictionary<double, TF>();

        public static Dictionary<string, int> regionsPerChromosome = new Dictionary<string, int>();
        #endregion
        #endregion

        #region local variables
        private static int totalNumOfTFsPre;
        private static int totalNumOfTFsPost;
        private static int totalNumOfPeaksPre;
        private static int totalNumOfPeaksPost;

        private static int totalNumOfRegions;

        private static double meanPeaksInRegionDistance;
        private static double meanAllPeaksDistance;
        private static double meanAllRegionsDistance;

        private static double variancePeaksInRegionDistance;
        private static double varianceAllPeaksDistance;
        private static double varianceAllRegionsDistance;

        private static double standardDeviationPeaksInRegionDistance;
        private static double standardDeviationAllPeaksDistance;
        private static double standardDeviationAllRegionsDistance;
        #endregion

        public static void prepareStatisticsPre()
        {
            totalNumOfTFsPre = tfStatsPre.Count;
            totalNumOfPeaksPre = peaksPerChromosomePre.Sum(x => x.Value);
            foreach (KeyValuePair<string, TF> p in tfStatsPre.Where(x => x.Value.lengths.Count != 0).ToDictionary(x => x.Key, x => x.Value))
            {
                p.Value.meanLength = Math.Round(p.Value.lengths.Average(), 2);
                p.Value.percentage = decimal.Round(Convert.ToDecimal((Convert.ToDecimal(p.Value.numOfPeaks) * Convert.ToDecimal(100)) / Convert.ToDecimal(totalNumOfPeaksPre)), 2, MidpointRounding.AwayFromZero);
            }
        }

        public static void prepareStatisticsData()
        {
            totalNumOfRegions = regionsPerChromosome.Sum(x => x.Value);
            totalNumOfPeaksPost = tfStatsPost.Sum(x => x.Value.numOfPeaks);
            totalNumOfTFsPost = tfStatsPost.Count; ;

            foreach (KeyValuePair<string, TF> p in tfStatsPost.Where(x => x.Value.lengths.Count != 0).ToDictionary(x => x.Key, x => x.Value))
            {
                p.Value.meanLength = Math.Round(p.Value.lengths.Average(), 2);
                p.Value.percentage = decimal.Round(Convert.ToDecimal((Convert.ToDecimal(p.Value.numOfPeaks) * Convert.ToDecimal(100)) / Convert.ToDecimal(totalNumOfPeaksPost)), 2, MidpointRounding.AwayFromZero);
            }

            foreach (KeyValuePair<double, TF> rs in regionScore.Where(x => x.Value.lengths.Count != 0).ToDictionary(x => x.Key, x => x.Value))
            {
                rs.Value.meanLength = Math.Round(rs.Value.lengths.Average(), 2);
                rs.Value.percentage = decimal.Round(Convert.ToDecimal((Convert.ToDecimal(rs.Value.numOfPeaks) * Convert.ToDecimal(100)) / Convert.ToDecimal(totalNumOfRegions)), 2, MidpointRounding.AwayFromZero);
            }

            foreach (KeyValuePair<int, regs> rl in regionLength.Where(x => x.Value.lengths.Count != 0).ToDictionary(x => x.Key, x => x.Value))
            {
                rl.Value.meanLength = Math.Round(rl.Value.lengths.Average(), 2);
                rl.Value.percentage = decimal.Round(Convert.ToDecimal((Convert.ToDecimal(rl.Value.numOfPeaks) * Convert.ToDecimal(100)) / Convert.ToDecimal(totalNumOfRegions)), 2, MidpointRounding.AwayFromZero);
                rl.Value.meanScore = Math.Round(rl.Value.score.Average(), 2);
            }

            meanPeaksInRegionDistance = (peaksInRegionDistance.Count == 0) ? 0 : Math.Round(peaksInRegionDistance.Average(), 2);
            meanAllPeaksDistance = (allPeaksDistance.Count == 0) ? 0 : Math.Round(allPeaksDistance.Average(), 2);
            meanAllRegionsDistance = (allRegionsDistance.Count == 0) ? 0 : Math.Round(allRegionsDistance.Average(), 2);

            variancePeaksInRegionDistance = (meanPeaksInRegionDistance == 0) ? 0 : Math.Round(Variance(peaksInRegionDistance, meanPeaksInRegionDistance), 2);
            varianceAllPeaksDistance = (meanAllPeaksDistance == 0) ? 0 : Math.Round(Variance(allPeaksDistance, meanAllPeaksDistance), 2);
            varianceAllRegionsDistance = (meanAllRegionsDistance == 0) ? 0 : Math.Round(Variance(allRegionsDistance, meanAllRegionsDistance), 2);

            standardDeviationPeaksInRegionDistance = (variancePeaksInRegionDistance == 0) ? 0 : Math.Round(Math.Sqrt(variancePeaksInRegionDistance), 2);
            standardDeviationAllPeaksDistance = (varianceAllPeaksDistance == 0) ? 0 : Math.Round(Math.Sqrt(varianceAllPeaksDistance), 2);
            standardDeviationAllRegionsDistance = (varianceAllRegionsDistance == 0) ? 0 : Math.Round(Math.Sqrt(varianceAllRegionsDistance), 2);
        }

        private static double Variance(IEnumerable<int> values, double mean)
        {
            return (values.Sum(d => Math.Pow(d - mean, 2))) / (values.Count() - 1);
        }

        public static void printStatisticData(string resultsDirectory, char OSseparator, string outfileName)
        {
            prepareStatisticsPre();
            prepareStatisticsData();
            TextWriter output = new StreamWriter(@"" + resultsDirectory + OSseparator + outfileName + "_statistics.csv");
            output.WriteLine("results of experiment," + outfileName);

            DateTime dt = DateTime.Now;

            output.WriteLine("date of experiment," + string.Format("{0:dddd dd/MM/yyyy HH:mm:ss}", dt));
            output.WriteLine("last modification date," + string.Format("{0:dddd dd/MM/yyyy HH:mm:ss}", dt));
            output.WriteLine();
            output.WriteLine("summary of the input peak data");
            output.WriteLine("total number of input TFs," + totalNumOfTFsPre);
            output.WriteLine("total number of input peaks," + totalNumOfPeaksPre);
            output.WriteLine("mean peak distance," + meanAllPeaksDistance);
            output.WriteLine("variance of peak distance," + varianceAllPeaksDistance);
            output.WriteLine("standard deviation of peak distance," + standardDeviationAllPeaksDistance);
            output.WriteLine();

            output.WriteLine("per TF data pre and post processing");
            output.WriteLine("TF,no. of peaks pre,mean length pre,percentage pre,no. of peaks post,mean length post,percentage post");
            foreach (KeyValuePair<string, TF> stf in tfStatsPre.OrderBy(x => x.Key))
            {
                if (tfStatsPost.ContainsKey(stf.Key))
                {
                    output.WriteLine(stf.Key + "," + stf.Value.numOfPeaks + "," + stf.Value.meanLength + "," + stf.Value.percentage + "%," +
                        tfStatsPost[stf.Key].numOfPeaks + "," + tfStatsPost[stf.Key].meanLength + "," + tfStatsPost[stf.Key].percentage + "%");
                }
                else
                {
                    output.WriteLine(stf.Key + "," + stf.Value.numOfPeaks + "," + stf.Value.meanLength + "," + stf.Value.percentage + "%,0,0,0%");
                }
            }
            output.WriteLine("total," + tfStatsPre.Values.Select(x => x.numOfPeaks).Sum() + "," + Math.Round(tfStatsPre.Values.Select(x => x.meanLength).ToList().Average(), 2)
                + "," + Math.Round(tfStatsPre.Values.Select(x => x.percentage).ToList().Sum(), 2) + "%," + tfStatsPost.Values.Select(x => x.numOfPeaks).Sum() + ","
                + Math.Round(tfStatsPost.Values.Select(x => x.meanLength).ToList().Average(), 2) + "," + Math.Round(tfStatsPost.Values.Select(x => x.percentage).ToList().Sum(), 2) + "%");
            output.WriteLine();
            output.WriteLine("summary of the results");
            output.WriteLine("total number of identified regions," + totalNumOfRegions);
            output.WriteLine("mean region length," + meanAllRegionsDistance);
            output.WriteLine("variance of region length," + varianceAllRegionsDistance);
            output.WriteLine("standard deviation of region length," + standardDeviationAllRegionsDistance);
            output.WriteLine();
            output.WriteLine("mean peaks in regions distance," + meanPeaksInRegionDistance);
            output.WriteLine("variance of peaks in regions distance," + variancePeaksInRegionDistance);
            output.WriteLine("standard deviation of peaks in regions distance," + standardDeviationPeaksInRegionDistance);
            output.WriteLine();

            output.WriteLine("region length distribution with 100bp intervals");
            output.WriteLine("length interval,no. of regions,percentage, mean length, mean score");
            regionLength = regionLength.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
            for (int i = 0; i < regionLength.Count; i++)
            {
                //first element in the region length list
                if (i == 0)
                {
                    output.WriteLine("1-" + regionLength.ElementAt(i).Key + "," + regionLength.ElementAt(i).Value.numOfPeaks + "," + regionLength.ElementAt(i).Value.percentage + "%," +
                        regionLength.ElementAt(i).Value.meanLength + "," + regionLength.ElementAt(i).Value.meanScore);
                }
                //last element in the region length list
                else if (i == (regionLength.Count - 1))
                {
                    output.WriteLine(">" + regionLength.ElementAt(i).Key + "," + regionLength.ElementAt(i).Value.numOfPeaks + "," + regionLength.ElementAt(i).Value.percentage + "%," +
                        regionLength.ElementAt(i).Value.meanLength + "," + regionLength.ElementAt(i).Value.meanScore);


                }
                //intermediate elements in the region length list
                else
                {
                    output.WriteLine(Convert.ToString(regionLength.ElementAt(i - 1).Key + 1) + "-" + regionLength.ElementAt(i).Key + "," +
                        regionLength.ElementAt(i).Value.numOfPeaks + "," + regionLength.ElementAt(i).Value.percentage + "%," + regionLength.ElementAt(i).Value.meanLength + "," +
                        regionLength.ElementAt(i).Value.meanScore);
                }
            }
            output.WriteLine("total," + regionLength.Values.Select(x => x.numOfPeaks).Sum() + "," + Math.Round(regionLength.Values.Select(x => x.percentage).Sum(), 2) + "," +
                Math.Round(regionLength.Values.Select(x => x.meanLength).Average(), 2) + "," + Math.Round(regionLength.Values.Select(x => x.meanScore).Average(), 2));
            output.WriteLine();

            output.WriteLine("region score distribution");
            output.WriteLine("region score,number of regions,percentage,average length of the regions");
            regionScore = regionScore.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
            for (int i = 0; i < regionScore.Count; i++)
            {
                if (i == (regionScore.Count - 1))
                {
                    output.WriteLine(">" + regionScore.ElementAt(i).Key + "," + regionScore.ElementAt(i).Value.numOfPeaks + "," + regionScore.ElementAt(i).Value.percentage + "%," +
                        regionScore.ElementAt(i).Value.meanLength);
                    break;
                }
                output.WriteLine(regionScore.ElementAt(i).Key + "," + regionScore.ElementAt(i).Value.numOfPeaks + "," + regionScore.ElementAt(i).Value.percentage + "%," +
                    regionScore.ElementAt(i).Value.meanLength);
            }
            output.WriteLine("total," + regionScore.Values.Select(x => x.numOfPeaks).Sum() + "," + Math.Round(regionScore.Values.Select(x => x.percentage).ToList().Sum(), 2) + "," +
                Math.Round(regionScore.Values.Select(x => x.meanLength).ToList().Average(), 2));
            output.WriteLine();

            output.WriteLine("per chromosome data");
            output.WriteLine("chromosome,number of regions,number of peaks pre, number of peaks post");
            foreach (KeyValuePair<string, int> chr in peaksPerChromosomePre.OrderBy(x => x.Key))
            {
                if (regionsPerChromosome.ContainsKey(chr.Key))
                {
                    output.WriteLine(chr.Key + "," + regionsPerChromosome[chr.Key] + "," + chr.Value + "," + peaksPerChromosomePost[chr.Key]);
                }
                else
                {
                    output.WriteLine(chr.Key + ",0," + chr.Value + ",0");
                }
            }
            output.WriteLine("total," + totalNumOfRegions + "," + peaksPerChromosomePre.Sum(x => x.Value) + "," + peaksPerChromosomePost.Sum(x => x.Value));
            output.Close();
        }

        #region add data to ductionaries
        public static void addToTfStatsPre(string keyToAdd, List<int> len, int num)
        {
            if (tfStatsPre.ContainsKey(keyToAdd))
            {
                tfStatsPre[keyToAdd].lengths.AddRange(len);
                tfStatsPre[keyToAdd].numOfPeaks += num;
            }
            else
            {
                tfStatsPre.Add(keyToAdd, new TF()
                    {
                        lengths = new List<int>(len),
                        numOfPeaks = num
                    });
            }
        }

        public static void addToPeaksPerChromosomePre(string keyToAdd, int num)
        {
            if (peaksPerChromosomePre.ContainsKey(keyToAdd))
            {
                peaksPerChromosomePre[keyToAdd] += num;
            }
            else
            {
                peaksPerChromosomePre.Add(keyToAdd, num);
            }
        }

        public static void addToTfStatsPost(string keyToAdd, List<int> len, int num)
        {
            if (tfStatsPost.ContainsKey(keyToAdd))
            {
                tfStatsPost[keyToAdd].lengths.AddRange(len);
                tfStatsPost[keyToAdd].numOfPeaks += num;
            }
            else
            {
                tfStatsPost.Add(keyToAdd, new TF()
                {
                    lengths = new List<int>(len),
                    numOfPeaks = num
                });
            }
        }

        public static void addToPeaksPerChromosomePost(string keyToAdd, int num)
        {
            if (peaksPerChromosomePost.ContainsKey(keyToAdd))
            {
                peaksPerChromosomePost[keyToAdd] += num;
            }
            else
            {
                peaksPerChromosomePost.Add(keyToAdd, num);
            }
        }

        public static void addToRegionLength(List<int> len, List<double> scr, int num)
        {
            int neededLen;
            for (int i = 0; i < len.Count; i++)
            {
                neededLen = addToRegLen(len[i]);
                if (regionLength.ContainsKey(neededLen))
                {
                    regionLength[neededLen].lengths.Add(neededLen);
                    regionLength[neededLen].numOfPeaks++;
                    regionLength[neededLen].score.Add(scr[i]);
                }
                else
                {
                    regionLength.Add(neededLen, new regs()
                        {
                            lengths = new List<int>() { neededLen },
                            numOfPeaks = 1,
                            score = new List<double>() { scr[i] }
                        });
                }
            }
        }

        public static int addToRegLen(int length)
        {
            if ((length % 100) != 0)
            {
                return (length > 900) ? 1000 : ((length - (length % 100)) + 100);
            }
            else
            {
                return (length > 900) ? 1000 : length;
            }
        }

        public static void addToRegionScore(List<int> len, List<double> scr, int num)
        {
            double neededScore;
            for (int i = 0; i < scr.Count; i++)
            {
                neededScore = addToRegScr(scr[i]);
                if (regionScore.ContainsKey(neededScore))
                {
                    regionScore[neededScore].lengths.Add(len[i]);
                    regionScore[neededScore].numOfPeaks++;
                }
                else
                {
                    regionScore.Add(neededScore, new TF()
                        {
                            lengths = new List<int>() { len[i] },
                            numOfPeaks = 1
                        });
                }
            }
        }

        //add to region score
        public static double addToRegScr(double score)
        {
            return (score > 20) ? 20 : score;
        }

        public static void addToRegionsPerChromosome(string keyToAdd, int num)
        {
            if (regionsPerChromosome.ContainsKey(keyToAdd))
            {
                regionsPerChromosome[keyToAdd] += num;
            }
            else
            {
                regionsPerChromosome.Add(keyToAdd, num);
            }
        }
        #endregion
    }

    public class TF
    {
        public int numOfPeaks = 0;
        public List<int> lengths = new List<int>();
        public double meanLength = 0;
        public decimal percentage = 0;
    }

    public class regs : TF
    {
        public List<double> score = new List<double>();
        public double meanScore = 0;
    }
}

