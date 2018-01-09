using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace tfNet
{
    public class fromPeaksToRegions : createRegions
    {
        #region peaks options
        public string peakFilesDirectory { get; set; }
        //true: by end; false: start+summit; null: noSort
        public int sortPeaks { get; set; } //sort the output regions according to score
        public List<string> acceptedFileExtensions { get; set; }
        #endregion

        public fromPeaksToRegions(string _mode)
            : base(_mode)
        {

        }

        public new void start()
        {
            try
            {
                combinePeaks cp = returnNewPeakInstance();
                if (!lowMemory)
                {
                    network n = returnNewNetworkInstance("");
                    n.regions = createRegionsFromList(sortAndPrintPeaks(cp.combineTheGivenPeakFiles()));
                    n.run();
                }
                else
                {
                    Console.WriteLine("WARNING: options -s, -fn and -fp are disabled when low memory usage in enabled!");
                    sortAndPrintPeaks(cp.combineTheGivenPeakFiles());
                    combinedPeakfile = resultsDirectory + OSseparator + outfileName + "_peaks." + fileType;
                    createRegionsFromList(null);
                    statistics.tfStatsPost.Clear();
                    statistics.peaksPerChromosomePost.Clear();
                    tfOccs.Clear();
                    if (xmlFile)
                    {
                        network n = returnNewNetworkInstance(resultsDirectory + OSseparator + outfileName + "_regions.xml");
                        n.start();
                    }
                    else
                    {
                        network n = returnNewNetworkInstance(resultsDirectory + OSseparator + outfileName + "_regions." + fileType);
                        n.start();
                    }
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

        private List<peak> sortAndPrintPeaks(List<peak> loadedPeaks)
        {
            List<peak> sortedPeaks = peakSorting(loadedPeaks, distanceOption);

            string combinedPeaksFileName = resultsDirectory + OSseparator + outfileName + "_peaks." + fileType;
            StreamWriter output = new StreamWriter(@"" + combinedPeaksFileName);
            foreach (peak p in sortedPeaks)
            {
                printPeak(p, output);
            }
            output.Close();
            Console.WriteLine(" done!");

            return sortedPeaks;
        }

        private combinePeaks returnNewPeakInstance()
        {
            return new combinePeaks(mode)
                {
                    peakFilesDirectory = this.peakFilesDirectory,
                    sortPeaks = this.sortPeaks,
                    filterByScore = this.filterByScore,
                    filterByPvalue = this.filterByPvalue,
                    filterByQvalue = this.filterByQvalue,
                    filterBySV = this.filterBySV,
                    noValueAssigned = this.noValueAssigned,
                    unknownSummit = this.unknownSummit,
                    resultsDirectory = this.resultsDirectory,
                    tfList = this.tfList,
                    numOfCols = this.numOfCols,
                    summitWindow = this.summitWindow,
                    narrowThePeak = this.narrowThePeak,
                    mode = this.mode,
                    outfileName = this.outfileName,
                    fileType =this.fileType,
                    mMem = this.mMem,
                    memPrcntg = this.memPrcntg,
                    //regionPvalue = this.regionPvalue,
                    //minusLog10 = this.minusLog10,
                    ignoreChromosomeLength = this.ignoreChromosomeLength,
                    acceptedFileExtensions = this.acceptedFileExtensions,
                    chromosomeNamesAndLength = this.chromosomeNamesAndLength,
                    fileNameAsTfName = this.fileNameAsTfName
                };
        }

        private network returnNewNetworkInstance(string currectOutputFile)
        {
            return new network(mode)
            {
                inputFile = currectOutputFile,
                tfOccs = this.tfOccs,
                resultsDirectory = this.resultsDirectory,
                outfileName = this.outfileName,
                filterOption = this.filterOption,
                filterValue = this.filterValue,
                neighborDistanceLow = this.neighborDistanceLow,
                neighborDistanceHigh = this.neighborDistanceHigh,
                overlapDistance = this.overlapDistance,
                title = this.title,
                noRscript = noRscript,
                Rscript = this.Rscript,
                scriptPath = this.scriptPath,
                mode = this.mode,
                mMem = this.mMem
            };
        }
    }
}
