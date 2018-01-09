using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tfNet
{
    public class network : filtAndNet
    {
        #region parameters
        public string inputFile { get; set; }
        public List<region> regions { get; set; }
        #endregion

        public network(string _mode)
            : base(_mode)
        {
            
        }

        public void start()
        {
            try
            {
                if (checkFileFormat(inputFile)) //bed file
                {
                    Console.WriteLine("only co-occurring network will be provided");
                    regions = loadRegions(inputFile);
                    Console.Write("network...");
                    findAllPeaksCoOccuring(statistics.tfStatsPost.ToDictionary(x => x.Key, y => y.Value.numOfPeaks));
                    Console.WriteLine(" done!");
                }
                else //xml file
                {
                    regions = readFromXML(inputFile);
                    run();
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

        public void run()
        {
            Console.Write("network...");
            //List<Task> tasks = new List<Task>();
            //tasks.Add(Task.Factory.StartNew(() => findAllPeaksCoOccuring(statistics.tfStatsPost.ToDictionary(x => x.Key, y => y.Value.numOfPeaks))));
            //tasks.Add(Task.Factory.StartNew(() => findAllNeighbouring(statistics.tfStatsPost.ToDictionary(x => x.Key, y => y.Value.numOfPeaks))));
            //tasks.Add(Task.Factory.StartNew(() => findAllCooccurring(statistics.tfStatsPost.ToDictionary(x => x.Key, y => y.Value.numOfPeaks))));
            //while (tasks.Any(t => !t.IsCompleted)) { }

            List<netHelp> netHelpList = new List<netHelp>();
            netHelpList.Add(findAllPeaksCoOccuring(statistics.tfStatsPost.ToDictionary(x => x.Key, y => y.Value.numOfPeaks)));
            netHelpList.Add(findAllNeighbouring(statistics.tfStatsPost.ToDictionary(x => x.Key, y => y.Value.numOfPeaks)));
            netHelpList.Add(findAllOverlapping(statistics.tfStatsPost.ToDictionary(x => x.Key, y => y.Value.numOfPeaks)));

            if (!noRscript)
            {
                foreach (netHelp item in netHelpList)
                {
                    runRscript(item);
                }
            }

            Console.WriteLine(" done!");
        }

		netHelp findAllPeaksCoOccuring(Dictionary<string, int> tfCount)
        {
            string coOccurringOutput = resultsDirectory + OSseparator + "allCoOccouringPeaks.csv";
            TextWriter output = new StreamWriter(@"" + coOccurringOutput);
            foreach (KeyValuePair<string, Dictionary<string, tfOccurrences>> tfA in tfOccs.OrderBy(x => x.Key))
            {
                foreach (KeyValuePair<string, tfOccurrences> tfB in tfA.Value.OrderBy(x => x.Key))
                {
                    output.WriteLine(tfA.Key + "," + tfB.Key + "," + tfB.Value.count + "," +
                                     tfCount[tfA.Key] + "," + tfCount[tfB.Key]);
                }
            }
            output.Close();
            
            string arg = "";
            if (OSseparator == '/')
            {
                arg = scriptPath + " -i " + coOccurringOutput + " -l " +
                    statistics.tfStatsPost.Sum(x => x.Value.numOfPeaks) + " -o " + resultsDirectory + OSseparator + outfileName + "_cooccuring.csv -" +
                        filterOption + " " + filterValue + " -t " + title + "_cooccurring -a b";
            }
            else
            {
                arg = "\"" + scriptPath + "\" -i \"" + coOccurringOutput + "\" -l " +
                    statistics.tfStatsPost.Sum(x => x.Value.numOfPeaks) + " -o \"" + resultsDirectory + OSseparator + outfileName + "_cooccuring.csv\" -" +
                        filterOption + " " + filterValue + " -t " + title + "_cooccurring -a b";
            }
			return new netHelp (){ args = arg, file = coOccurringOutput };
        }

		netHelp findAllNeighbouring(Dictionary<string, int> tfCount)
        {
            string neighbouringOutput = resultsDirectory + OSseparator + "allNeighboringPeaks.csv";
            TextWriter output = new StreamWriter(@"" + neighbouringOutput);
            foreach (KeyValuePair<string, Dictionary<string, tfOccurrences>> tfA in tfOccs.OrderBy(x => x.Key))
            {
                foreach (KeyValuePair<string, tfOccurrences> tfB in tfA.Value.OrderBy(x => x.Key))
                {
                    output.WriteLine(tfA.Key + "," + tfB.Key + "," + tfB.Value.distance.Where(x => x >= neighborDistanceLow && x <= neighborDistanceHigh).Count() + "," +
                                     tfCount[tfA.Key] + "," + tfCount[tfB.Key]);
                }
            }
            output.Close();

            string arg = "";
            if (OSseparator == '/')
            {
				arg = scriptPath + " -i " + neighbouringOutput + " -l " +
                    statistics.tfStatsPost.Sum(x => x.Value.numOfPeaks) + " -o " + resultsDirectory + OSseparator + outfileName + "_neighboring.csv -" + filterOption + " " + filterValue +
                    " -t " + title + "_neighbouring -a h";
            }
            else
            {
				arg = "\"" + scriptPath + "\" -i \"" + neighbouringOutput + "\" -l " +
                    statistics.tfStatsPost.Sum(x => x.Value.numOfPeaks) + " -o \"" + resultsDirectory + OSseparator + outfileName + "_neighboring.csv\" -" + filterOption + " " + filterValue +
                        " -t " + title + "_neighbouring -a h";
            }
			return new netHelp (){ args = arg, file = neighbouringOutput };
        }

		netHelp findAllOverlapping(Dictionary<string, int> tfCount)
        {
            string overlappingOutput = resultsDirectory + OSseparator + "allOverlappingPeaks.csv";
            TextWriter output = new StreamWriter(@"" + overlappingOutput);
            foreach (KeyValuePair<string, Dictionary<string, tfOccurrences>> tfA in tfOccs.OrderBy(x => x.Key))
            {
                foreach (KeyValuePair<string, tfOccurrences> tfB in tfA.Value.OrderBy(x => x.Key))
                {
                    output.WriteLine(tfA.Key + "," + tfB.Key + "," + tfB.Value.distance.Where(x => x <= overlapDistance).Count() + "," +
                                     tfCount[tfA.Key] + "," + tfCount[tfB.Key]);
                }
            }
            output.Close();

            string arg = "";
            if (OSseparator == '/')
            {
				arg = scriptPath + " -i " + overlappingOutput + " -l " +
                    statistics.tfStatsPost.Sum(x => x.Value.numOfPeaks) + " -o " + resultsDirectory + OSseparator + outfileName + "_overlapping.csv -" + filterOption + " " + filterValue +
                    " -t " + title + "_overlapping -a h";
            }
            else
            {
				arg = "\"" + scriptPath + "\" -i \"" + overlappingOutput + "\" -l " +
                    statistics.tfStatsPost.Sum(x => x.Value.numOfPeaks) + " -o \"" + resultsDirectory + OSseparator + outfileName + "_overlapping.csv\" -" + filterOption + " " + filterValue +
                        " -t " + title + "_overlapping -a h";
            }
			return new netHelp (){ args = arg, file = overlappingOutput };
        }

		void runRscript(netHelp inpt)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = "Rscript";
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardError = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;

			startInfo.Arguments = inpt.args;
			Process newProcess = new Process();
			newProcess.StartInfo = startInfo;
			newProcess.Start();
			newProcess.WaitForExit();string stderrx = newProcess.StandardError.ReadToEnd();
			if (newProcess.ExitCode != 0)
			{
				newProcess.Close();
				dlt = false;
				string checkOutput = stderrx.Split('\n').First();
				if (checkOutput.Split(' ').First() == "Error:" && checkOutput.Split(' ').ElementAt(1) == "tfNet" && checkOutput.Split(' ').ElementAt(2) == "-")
				{
					exit(string.Join(" ", checkOutput.Split(' ').ToList().GetRange(3, checkOutput.Split(' ').Length - 3)));
				}
				else
				{
					Console.WriteLine("Error : {0}", stderrx);
					//exit("there was an error while creating the overlapping network.");
				}
				dlt = true;
			}
			else
			{
				File.Delete(@"" + inpt.file);
			}
		}
    }

	class netHelp
	{
		public string args { get; set; }
		public string file { get; set; }
	}
}
