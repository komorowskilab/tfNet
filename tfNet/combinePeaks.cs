using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace tfNet
{
    public class combinePeaks : peakAndReg
    {
        #region parameters
        public string peakFilesDirectory { get; set; } //the path of the directory containing the peak files
        public int sortPeaks { get; set; } //sort the output regions according to score //1: by start; 2: start+summit; 3: noSort; 4: start+middle
        public List<string> acceptedFileExtensions { get; set; }
        #endregion

        public combinePeaks(string _mode) 
            : base(_mode)
        {
            
        }
        
        /// <summary>
        /// start running peak combining
        /// </summary>
        public void start()
        {
            try
            {
                //combine peaks
                combineTheGivenPeakFiles();
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
        /// combine a collection of peaks files
        /// </summary>
        /// <returns></returns>
        public List<peak> combineTheGivenPeakFiles()
        {
            Console.Write("working on peaks...");

            //load and combine peaks
            List<peak> loadedPeaks = new List<peak>();
            foreach (string peakFile in Directory.GetFiles(@"" + peakFilesDirectory).Where(x => acceptedFileExtensions.Contains(x.Split(OSseparator).Last().Split('.').Last())))
            {
                loadedPeaks.AddRange(loadPeaks(peakFile, fileNameAsTfName ? useFileNameAsTfName(peakFile.Split(OSseparator).Last()) : findTFname(peakFile.Split(OSseparator).Last())));
            }

            //if peak and some kind of sort needed
            //will run only if the run mode is peaks
            //for other run modes that use this we apply different sorting strategy according to the requirements
            if (mode == "peaks")
            {
                sortAndPrintPeaks(loadedPeaks);
            }

            return loadedPeaks;
        }

        /// <summary>
        /// sort (if needed) and print peaks
        /// </summary>
        /// <param name="loadedPeaks"></param>
        /// <returns></returns>
        List<peak> sortAndPrintPeaks(List<peak> loadedPeaks)
        {
            switch (sortPeaks)
            {
                case 1:
                    loadedPeaks =  peakSorting(loadedPeaks, null);
                    break;
                case 2:
                    loadedPeaks = peakSorting(loadedPeaks, true);
                    break;
                case 3:
                    loadedPeaks = peakSorting(loadedPeaks, false);
                    break;
                default:
                    break;
            }
            
            string combinedPeaksFileName = resultsDirectory + OSseparator + outfileName + "_peaks." + fileType;
            StreamWriter output = new StreamWriter(@"" + combinedPeaksFileName);
            foreach (peak p in loadedPeaks)
            {
                printPeak(p, output);
            }
            output.Close();
            Console.WriteLine(" done!");

            return loadedPeaks;
        }

        /// <summary>
        /// detects the TF name of the input file
        /// </summary>
        /// <param name="givenFile">input file</param>
        /// <returns>TF name</returns>
        string findTFname(string inputFile)
        {
            string TFname = tfList.Where(x => inputFile.ToUpper().Contains(x.ToUpper())).OrderBy(x => x.Length).LastOrDefault();

            if (string.IsNullOrEmpty(TFname) || string.IsNullOrWhiteSpace(TFname))
            {
                Console.WriteLine("TF name of file " + inputFile.Split('/').ElementAt(inputFile.Split('/').Length - 1) + " could not be found. Please insert it:");
                TFname = Console.ReadLine().ToUpper();
                tfList.Add(TFname);
            }
            return TFname;
        }

        /// <summary>
        /// returns the file name excluding the file extension
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        string useFileNameAsTfName(string inputFile)
        {
            return string.Join(".", inputFile.Split('.').ToList().GetRange(0, inputFile.Split('.').Length - 1));
        }
    }
}
