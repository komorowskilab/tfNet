using System;
using System.Collections.Generic;
using System.IO;

namespace tfNet
{
	public class masterClass : Program
    {
        #region paramaters
        public static char OSseparator { get; set; }
        public Dictionary<string, int> chromosomeNamesAndLength { get; set; }
        public static List<char> strandSymbols { get; set; }
        public string mode { get; set; }
        public static bool dlt { get; set; }
        public string resultsDirectory { get; set; }
        public bool ignoreChromosomeLength { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_mode"></param>
		public masterClass(string _mode) : base()
        {
            OSseparator = getOSseparator();
            strandSymbols = getStrandSymbols();
            dlt = true;
            mode = _mode;
        }

        private char getOSseparator()
        {
            if ((int)Environment.OSVersion.Platform == 2)
            {
                return '\\';
            }
            else
            {
                return '/';
            }
        }

		private List<char> getStrandSymbols()
		{
			return new List<char>() { '+', '-', '.' };
		}

        /// <summary>
        /// Exiting when a detectable error occurs.
        /// </summary>
        /// <param name="error">the error message.</param>
        public void exit(string error)
        {
            mMem.RequestStop();
            Console.WriteLine("\nError: " + error + "! Exiting!");
            if (dlt)
            {
                if (Directory.Exists(@"" + resultsDirectory))
                {
                    try
                    {
                        Directory.Delete(@"" + resultsDirectory, true);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\nError: unable to delete results directory! Exiting!");
                        Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
                    }
                }
            }
            Environment.Exit(0);
        }
    }
}
