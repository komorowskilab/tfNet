using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using System.IO;

namespace tfNet
{
    class Options
    {
        public Options()
        {
            
        }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }

        #region verbs
        [VerbOption("peaks", HelpText = "combine peaks")]
		public peaks peakVerb { get; set; }

		[VerbOption("regions", HelpText = "detect regions")]
		//public regions regionVerb { get; set; }
        public regions regionVerb { get; set; }

        [VerbOption("tfNet", HelpText = "tfNet pipeline")]
        public tfNet tfNetVerb { get; set; }

        [VerbOption("filter", HelpText = "filter regions")]
        public filter filterVerb { get; set; }

        [VerbOption("network", HelpText = "TF network")]
        public net networkVerb { get; set; }
        #endregion
    }
    
    abstract class common
    {
        #region from common
        //input file or directory
        [Option('i', "input", Required = true, HelpText = "input path")]
        public string inputValue { get; set; }

        //output file name
        [Option('o', "output", Required = false, DefaultValue = "tfNet_default", HelpText = "full path to output directory")]
        public string outputValue { get; set; }

        //memory percentage to be used for a warning to be thrown
        [Option("mem", Required = false, DefaultValue = 30.0, HelpText = "memory percentage")]
        public double memPrcntg { get; set; }

        //number of columns in output file
        [Option("cols", Required = false, DefaultValue = 10, HelpText = "file format")]
        public int numOfCols { get; set; }

        //file to derive the list of TFs from
		[Option("tfList", Required = false, HelpText = "(Default: list) semicolon-separated file containing names of TFs")]
        public string getTfList { get; set; }

        //file to derive the list of TFs from
        [Option("chrInfo", Required = false, HelpText = "(Default: hg19) chromosome info (chr1,249250621;chr2,243199373;etc)")]
        public string getChrInfo { get; set; }

        //file to derive the list of TFs from
		[Option('a', "ignoreChrInfo", Required = false, DefaultValue = false, HelpText = "ignore chromosome info correctness")]
        public bool ignoreChrInfo { get; set; }
        #endregion
    }

    abstract class peakAndregionInCommon : common
    {
        #region from peak and region
        //narrow the peak
        [Option('n', "nPeak", Required = false, DefaultValue = false, HelpText = "do not narrow peaks")]
        public bool narrowThePeak { get; set; }

        //summit window
        [Option("win", Required = false, DefaultValue = 10, HelpText = "window around the summit")]
        public int summitWindow { get; set; }

        //transform qValues to pValues
        //[Option('r', "ret", Required = false, DefaultValue = false, HelpText = "retrieve pValues from qValues (if not given)")]
        //public bool regionPvalues { get; set; }

        //set to true if the input pValues or qValues are not in -log10
        //[Option('g', "log10", Required = false, DefaultValue = false, HelpText = "pValues or qValues not in -log10")]
        //public bool minusLogTen { get; set; }
        #endregion
    }

    abstract class filterAndRegionInCommon : common
    {
        #region from filter and region
        //regions based on strand specificity
        [Option('s', "strand", Required = false, DefaultValue = false, HelpText = "region detection based on strand specificity")]
        public bool strandSpecificity { get; set; }

        //do not print the statistics file
        [Option('t', "statistics", Required = false, DefaultValue = false, HelpText = "do not print I/O statistics file")]
        public bool statFile { get; set; }

        //do not print the xml file
        [Option('x', "xml", Required = false, DefaultValue = false, HelpText = "do not print xml file")]
        public bool xmlFile { get; set; }

        //do not print the peaks in regions file
        [Option('p', "peaks", Required = false, DefaultValue = false, HelpText = "do not print peaks file")]
        public bool peakFile { get; set; }

        //run on low memory consumption
        [Option('l', "lowMemory", Required = false, DefaultValue = false, HelpText = "detect regions on low memory consumption")]
        public bool lowMemory { get; set; }

        //filter by number (highest rated by score)
        [Option("topX", Required = false, HelpText = "keep only topX regions by score")]
        public int topX { get; set; }

        //filter by percentage (highest rated by score)
        [Option("topXP", Required = false, HelpText = "keep only topXP percentage regions by score")]
        public int topXP { get; set; }

        //sort the output regions according to score //true: by end; false: start+summit; null: noSort
        [Option("sort", Required = false, DefaultValue = 'N', HelpText = "sort regions ascending(A), descending(D) or no sort(N)")]
        public char sortRegions { get; set; }

        //filter by chromosome
        [Option("chr", Required = false, HelpText = "discard regions not in chromosome")]
        public string chromosome { get; set; }

        //filter by transcription factor
        [Option("tfName", Required = false, HelpText = "discard regions not containing transcription factor")]
        public string tfName { get; set; }

        //filter by peak name
        [Option("pkName", Required = false, HelpText = "discard regions not containing peak")]
        public string pkName { get; set; }

        //filter by start index
        [Option("start", Required = false, HelpText = "discard regions with a lower starting position")]
        public int start { get; set; }

        //filter by end index
        [Option("end", Required = false, HelpText = "discard regions with a larger starting position")]
        public int end { get; set; }

        //filter by score (lower than)
        [Option("lScore", Required = false, HelpText = "discard regions with lower score")]
        public int lScore { get; set; }

        //filter by score (greater than)
        [Option("hScore", Required = false, HelpText = "discard regions with larger score")]
        public int hScore { get; set; }
        #endregion
    }

    class peaks : peakAndregionInCommon
    {
        #region peaks
        //discard entries having pValue or qValue of -1
		[Option('v', "noValue", Required = false, DefaultValue = false, HelpText = "discard records with pValue or qValue -1")]
		public bool noValueAssigned { get; set; }

		//discard records of unknown summit
		[Option('u', "noSummit", Required = false, DefaultValue = false, HelpText = "discard records with summit -1")]
		public bool unknownSummit { get; set; }

		//filter out all these peaks having a score lower than X
        [Option("score", Required = false, DefaultValue = 0, HelpText = "discard peaks with lower score")]
		public int filterByScore { get; set; }

		//filter out all these peaks having a signal value lower than X
        [Option("sValue", Required = false, DefaultValue = 0, HelpText = "discard peaks with lower signalValue")]
		public int filterBySV { get; set; }

		//filter out all these peaks having a pValue higher than X
        [Option("pValue", Required = false, DefaultValue = 0.0, HelpText = "discard peaks with higher pValue")]
		public double filterByPvalue { get; set; }

		//filter out all these peaks having a qValue higher than X
        [Option("qValue", Required = false, DefaultValue = 0.0, HelpText = "discard peaks with higher qValue")]
		public double filterByQvalue { get; set; }

        //sort the output regions according to score //true: by end; false: start+summit; null: noSort
        [Option("sort", Required = false, DefaultValue = 'N', HelpText = "sort peaks by start(S), start+summit(M), start+middle(P) or no sort(N)")]
        public char sortPeaks { get; set; }

        //file to derive the list of TFs from
        [Option("acc", Required = false, HelpText = "(Default: narrowPeak) accepted file extensions (comma separated)")]
        public string accFilExt { get; set; }

        //use the full file name as TF name (extension excluded)
        [Option('t', "fNameTfName", Required = false, HelpText = "use the file name as TF name (extension excluded)")]
        public bool fileNameAsTfName { get; set; }
        #endregion
	}

    class filter : filterAndRegionInCommon
    {
        #region filter
        //filter by region name
        [Option("reg", Required = false, HelpText = "filter by region name (comma separated)")]
        public string regName { get; set; }

        //filter by region name
        [Option("regFile", Required = false, HelpText = "filter by region name (very long comma separated list)")]
        public string regNameFile { get; set; }
        #endregion
    }

    /// <summary>
    /// Contains the same as peakAndregionInCommon
    /// </summary>
    class regions : filterAndRegionInCommon
    {
        #region from peak and region
        //narrow the peak
        [Option('n', "nPeak", Required = false, DefaultValue = false, HelpText = "avoid peak narrowing")]
        public bool narrowThePeak { get; set; }

        //summit window
        [Option("win", Required = false, DefaultValue = 10, HelpText = "window size around the summit")]
        public int summitWindow { get; set; }

        //transform qValues to pValues
        //[Option('r', "ret", Required = false, HelpText = "retrieve pValues from qValues (id not given)")]
        //public bool regionPvalues { get; set; }

        //set to true if the input pValues or qValues are not in -log10
        //[Option('g', "log10", Required = false, HelpText = "pValues or qValues not in -log10")]
        //public bool minusLogTen { get; set; }
        #endregion
        
        #region regions
        //annotate regions based on nextEnd-thisStart distance
        [Option('e', "startEnd", Required = false, DefaultValue = false, HelpText = "clusters based on middle-point distances ")]
		public bool distanceOption { get; set; }

		//annotate regions based on nextEnd-thisStart distance
		[Option("distance", Required = false, DefaultValue = 300, HelpText = "peak distance")]
		public int peakDistance { get; set; }

        //peaks in regions cutoff
        //[Option("cut", Required = false, DefaultValue = 0.05, HelpText = "cutoff for non-important peaks in regions")]
        //public double peakInRegionCutoff { get; set; }
        #endregion
    }

    class net : common
    {
        #region network
        //filtering option to construct network based on b (bonferroni corrected pValues), p (pValues) or q (qValues)
        [Option("fopt", Required = false, DefaultValue = 'b', HelpText = "network based on b (bonferroni corrected pValues), p (pValues) or q (qValues)")]
        public char filterOption { get; set; }

        //cutoff value for the network
        [Option("fval", Required = false, DefaultValue = 0.05, HelpText = "cuttoff value for the network")]
        public double filterValue { get; set; }

        //distance to consider peaks as neighboring
        [Option("neigh", Required = false, DefaultValue = "20,60", HelpText = "upper and lower bound of neighboring distance")]
        public string neighborDistance { get; set; }

        //distance to consider peaks as overlapping
        [Option("overlap", Required = false, DefaultValue = 0, HelpText = "overlapping distance")]
        public int overlapDistance { get; set; }

        //network title
        [Option("title", Required = false, DefaultValue = "defaultNetworkTitle", HelpText = "network title")]
        public string title { get; set; }

        //Rscript path to run the script
        [Option("Rscript", Required = false, DefaultValue = "", HelpText = "path where Rscript is installed")]
        public string Rscript { get; set; }

        //the provided script path
        [Option("scr", Required = false, DefaultValue =  "tfNet.exe directory", HelpText = "provided Rscript path")]
        public string scriptPath { get; set; }

        //run Rscript or not
        [Option('c', "noR", Required = false, DefaultValue = false, HelpText = "run Rscript")]
        public bool noR { get; set; }
        #endregion
    }

    /// <summary>
    /// Contains the same as filterAndRegionInCommon
    /// Contains the same as regions
    /// Contains the same as filter
    /// Contains the same as network
    /// </summary>
    class tfNet : peaks
    {
        #region from filter and region
        //regions based on strand specificity
        [Option('s', "strand", Required = false, DefaultValue = false, HelpText = "region annotation based on strand specificity")]
        public bool strandSpecificity { get; set; }

        //do not print the statistics file
        [Option('t', "statistics", Required = false, DefaultValue = false, HelpText = "do not print statistics file")]
        public bool statFile { get; set; }

        //do not print the xml file
        [Option('x', "xml", Required = false, DefaultValue = false, HelpText = "do not print xml file")]
        public bool xmlFile { get; set; }

        //do not print the peaks in regions file
        [Option('p', "peaks", Required = false, DefaultValue = false, HelpText = "do not print peaks in regions file")]
        public bool peakFile { get; set; }

        //run on low memory consumption
        [Option('l', "lowMemory", Required = false, DefaultValue = false, HelpText = "annotate regions on low memory (slow)")]
        public bool lowMemory { get; set; }

        //filter by number (highest rated by score)
        [Option("topX", Required = false, HelpText = "filter by number (highest rated by score)")]
        public int topX { get; set; }

        //filter by percentage (highest rated by score)
        [Option("topXP", Required = false, HelpText = "filter by percentage (highest rated by score)")]
        public int topXP { get; set; }

        //sort the output regions according to score //true: by end; false: start+summit; null: noSort
        [Option("sortR", Required = false, DefaultValue = 'N', HelpText = "sort regions ascending(A), descending(D) or no sort(N)")]
        public char sortRegions { get; set; }
        #endregion

        #region regions
        //annotate regions based on nextEnd-thisStart distance
        [Option('e', "startEnd", Required = false, DefaultValue = false, HelpText = "annotate regions based on nextEnd-thisStart distance")]
        public bool distanceOption { get; set; }

        //annotate regions based on nextEnd-thisStart distance
        [Option("distance", Required = false, DefaultValue = 300, HelpText = "peak distance")]
        public int peakDistance { get; set; }

        //peaks in regions cutoff
        //[Option("cut", Required = false, DefaultValue = 0.05, HelpText = "cutoff for non-important peaks in regions")]
        //public double peakInRegionCutoff { get; set; }
        #endregion

        #region filter
        //filter by chromosome
        [Option("chr", Required = false, HelpText = "filter by chromosome")]
        public string chromosome { get; set; }

        //filter by region name
        [Option("reg", Required = false, HelpText = "filter by region name")]
        public string regName { get; set; }

        //filter by region name
        [Option("regFile", Required = false, HelpText = "filter by region name (very long comma separated list)")]
        public string regNameFile { get; set; }

        //filter by transcription factor
        [Option("tfName", Required = false, HelpText = "filter by transcription factor")]
        public string tfName { get; set; }

        //filter by peak name
        [Option("pkName", Required = false, HelpText = "filter by peak name")]
        public string pkName { get; set; }

        //filter by start index
        [Option("start", Required = false, HelpText = "filter by start index")]
        public int start { get; set; }

        //filter by end index
        [Option("end", Required = false, HelpText = "filter by end index")]
        public int end { get; set; }

        //filter by score (lower than)
        [Option("lScore", Required = false, HelpText = "filter by score (lower than)")]
        public int lScore { get; set; }

        //filter by score (greater than)
        [Option("hScore", Required = false, HelpText = "filter by score (greater than)")]
        public int hScore { get; set; }
        #endregion

        #region network
        //filtering option to construct network based on b (bonferroni corrected pValues), p (pValues) or q (qValues)
        [Option("fopt", Required = false, DefaultValue = 'b', HelpText = "network based on b (bonferroni corrected pValues), p (pValues) or q (qValues)")]
        public char filterOption { get; set; }

        //cutoff value for the network
        [Option("fval", Required = false, DefaultValue = 0.05, HelpText = "statistically important (cuttoff) value")]
        public double filterValue { get; set; }

        //distance to consider peaks as neighboring
        [Option("neigh", Required = false, DefaultValue = "20,60", HelpText = "neighboring distance")]
        public string neighborDistance { get; set; }

        //distance to consider peaks as overlapping
        [Option("overlap", Required = false, DefaultValue = 0, HelpText = "overlapping distance")]
        public int overlapDistance { get; set; }

        //network title
        [Option("title", Required = false, DefaultValue = "defaultNetworkTitle", HelpText = "network title")]
        public string title { get; set; }

        //Rscript path to run the script
        [Option("Rscript", Required = false, DefaultValue = "", HelpText = "path where Rscript is installed")]
        public string Rscript { get; set; }

        //the provided script path
        [Option("scr", Required = false, DefaultValue = "tfNet.exe directory", HelpText = "provided Rscript path")]
        public string scriptPath { get; set; }

        //run Rscript or not
        [Option('c', "noR", Required = false, DefaultValue = false, HelpText = "run Rscript")]
        public bool noR { get; set; }
        #endregion
    }
}
