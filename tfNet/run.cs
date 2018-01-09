using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tfNet
{
    class run : validateArguments
    {
        public run(string _invokedVerb, Options _options)
            : base(_invokedVerb)
        {
            List<Task> threadList = new List<Task>();

            switch (_invokedVerb)
            {
                case "peaks":
                    mMem = mst(memPrcntg = checkMemoryPercentage(_options.peakVerb.memPrcntg));
                    threadList.Add(Task.Factory.StartNew(() => mMem.DoWork()));
                    combinePeaks cp = new combinePeaks(_invokedVerb)
                        {
                            peakFilesDirectory = checkInput(_options.peakVerb.inputValue),
                            sortPeaks = checkSortPeaks(_options.peakVerb.sortPeaks),
                            filterByScore = checkFilterByScore(_options.peakVerb.filterByScore),
                            filterByPvalue = checkFilterByPvalue(_options.peakVerb.filterByPvalue),
                            filterByQvalue = checkFilterByQvalue(_options.peakVerb.filterByQvalue),
                            filterBySV = checkFilterBySV(_options.peakVerb.filterBySV),
                            noValueAssigned = checkNoValueAssigned(_options.peakVerb.noValueAssigned),
                            unknownSummit = checkUnknownSummit(_options.peakVerb.unknownSummit),
                            resultsDirectory = checkOutput(_options.peakVerb.outputValue, _options.peakVerb.inputValue),
                            tfList = checkTfList(_options.peakVerb.getTfList),
                            numOfCols = checkNumberOfColumns(_options.peakVerb.numOfCols),
                            summitWindow = checkSummitWindow(_options.peakVerb.summitWindow),
                            narrowThePeak = checkPeakNoarrowing(_options.peakVerb.narrowThePeak),
                            mode = mode,
                            //regionPvalue = checkRegionPvalues(_options.peakVerb.regionPvalues),
                            //minusLog10 = checkMinusLogTen(_options.peakVerb.minusLogTen),
                            outfileName = setOutFileName(resDir),
                            fileType = setFileType(nCols),
                            mMem = mMem,
                            memPrcntg = memPrcntg,
                            chromosomeNamesAndLength = checkChromInfo(_options.peakVerb.getChrInfo),
                            ignoreChromosomeLength = checkIgnoreChrInfo(_options.peakVerb.ignoreChrInfo),
                            acceptedFileExtensions = checkAcceptedFileExtensions(_options.peakVerb.accFilExt),
                            fileNameAsTfName = checkFileNameAsTfName(_options.peakVerb.fileNameAsTfName)
                        };
                    threadList.Add(Task.Factory.StartNew(() => cp.start()));
                    //cp.start();
                    break;
                case "network":
                    mMem = mst(memPrcntg = checkMemoryPercentage(_options.networkVerb.memPrcntg));
                    threadList.Add(Task.Factory.StartNew(() => mMem.DoWork()));
                    network nw = new network(_invokedVerb)
                        {
                            inputFile = checkInput(_options.networkVerb.inputValue),
                            resultsDirectory = checkOutput(_options.networkVerb.outputValue, _options.networkVerb.inputValue),
                            numOfCols = checkNumberOfColumns(_options.networkVerb.numOfCols),
                            mode = mode,
                            outfileName = setOutFileName(resDir),
                            fileType = setFileType(nCols),
                            filterOption = checkFilterOption(_options.networkVerb.filterOption),
                            filterValue = checkFIlterValue(_options.networkVerb.filterValue),
                            neighborDistanceLow = checkNeighboringDistance(_options.networkVerb.neighborDistance),
                            neighborDistanceHigh = highNeigh,
                            overlapDistance = checkOverlappingDistance(_options.networkVerb.overlapDistance),
                            title = checkTitle(_options.networkVerb.title),
                            noRscript = checkForNoRscript(_options.networkVerb.noR),
                            Rscript = checkForRscript(_options.networkVerb.Rscript),
                            scriptPath = checkForScriptPath(_options.networkVerb.scriptPath),
                            mMem = mMem,
                            memPrcntg = memPrcntg,
                            chromosomeNamesAndLength = checkChromInfo(_options.networkVerb.getChrInfo),
                            ignoreChromosomeLength = checkIgnoreChrInfo(_options.networkVerb.ignoreChrInfo)
                        };
                    threadList.Add(Task.Factory.StartNew(() => nw.start()));
                    //nw.start();
                    break;
                case "filter":
                    mMem = mst(memPrcntg = checkMemoryPercentage(_options.filterVerb.memPrcntg));
                    threadList.Add(Task.Factory.StartNew(() => mMem.DoWork()));
                    filtering f = new filtering(_invokedVerb)
                        {
                            inputFile = checkInput(_options.filterVerb.inputValue),
                            numOfCols = checkNumberOfColumns(_options.filterVerb.numOfCols),
                            tfList = checkTfList(_options.filterVerb.getTfList),
                            sortRegionsBySore = checkSortRegions(_options.filterVerb.sortRegions),
                            resultsDirectory = checkOutput(_options.filterVerb.outputValue, _options.filterVerb.inputValue),
                            mode = mode,
                            topX = checkTopX(_options.filterVerb.topX),
                            topXP = checkTopXP(_options.filterVerb.topXP),
                            strandSpecificity = checkStrandSpecificity(_options.filterVerb.strandSpecificity),
                            statFile = checkStatFile(_options.filterVerb.statFile),
                            xmlFile = checkXmlFile(_options.filterVerb.xmlFile),
                            peakFile = checkPeakFile(_options.filterVerb.peakFile),
                            lowMemory = checkLowMemory(_options.filterVerb.lowMemory),
                            chromosome = checkChromosome(_options.filterVerb.chromosome),
                            startIndex = checkStart(_options.filterVerb.start),
                            endIndex = checkEnd(_options.filterVerb.end),
                            regsName = checkRegName(_options.filterVerb.regName, _options.filterVerb.regNameFile),
                            tfName = checkTfNames(_options.filterVerb.tfName),
                            peakName = checkPkNames(_options.filterVerb.pkName),
                            lowerScore = checkLScore(_options.filterVerb.lScore),
                            higherScore = checkHScore(_options.filterVerb.hScore),
                            outfileName = setOutFileName(resDir),
                            fileType = setFileType(nCols),
                            noValueAssigned = true,
                            unknownSummit = true,
                            mMem = mMem,
                            memPrcntg = memPrcntg,
                            chromosomeNamesAndLength = checkChromInfo(_options.filterVerb.getChrInfo),
                            ignoreChromosomeLength = checkIgnoreChrInfo(_options.filterVerb.ignoreChrInfo)
                        };
                    threadList.Add(Task.Factory.StartNew(() => f.start()));
                    //f.start();
                    break;
                case "regions":
                    mMem = mst(memPrcntg = checkMemoryPercentage(_options.regionVerb.memPrcntg));
                    threadList.Add(Task.Factory.StartNew(() => mMem.DoWork()));
                    createRegions cr = new createRegions(_invokedVerb)
                        {
                            combinedPeakfile = checkInput(_options.regionVerb.inputValue),
                            numOfCols = checkNumberOfColumns(_options.regionVerb.numOfCols),
                            tfList = checkTfList(_options.regionVerb.getTfList),
                            sortRegionsBySore = checkSortRegions(_options.regionVerb.sortRegions),
                            resultsDirectory = checkOutput(_options.regionVerb.outputValue, _options.regionVerb.inputValue),
                            mode = mode,
                            //regionPvalue = checkRegionPvalues(_options.regionVerb.regionPvalues),
                            //minusLog10 = checkMinusLogTen(_options.regionVerb.minusLogTen),
                            //cutoffValue = checkCutoffValue(_options.regionVerb.peakInRegionCutoff),
                            narrowThePeak = checkPeakNoarrowing(_options.regionVerb.narrowThePeak),
                            summitWindow = checkSummitWindow(_options.regionVerb.summitWindow),
                            peakDistance = checkPeakDistance(_options.regionVerb.peakDistance, _options.regionVerb.getChrInfo),
                            topX = checkTopX(_options.regionVerb.topX),
                            topXP = checkTopXP(_options.regionVerb.topXP),
                            strandSpecificity = checkStrandSpecificity(_options.regionVerb.strandSpecificity),
                            statFile = checkStatFile(_options.regionVerb.statFile),
                            xmlFile = checkXmlFile(_options.regionVerb.xmlFile),
                            peakFile = checkPeakFile(_options.regionVerb.peakFile),
                            distanceOption = checkPeakDistanceOption(_options.regionVerb.distanceOption),
                            lowMemory = checkLowMemory(_options.regionVerb.lowMemory),
                            chromosome = checkChromosome(_options.regionVerb.chromosome),
                            startIndex = checkStart(_options.regionVerb.start),
                            endIndex = checkEnd(_options.regionVerb.end),
                            tfName = checkTfNames(_options.regionVerb.tfName),
                            peakName = checkPkNames(_options.regionVerb.pkName),
                            lowerScore = checkLScore(_options.regionVerb.lScore),
                            higherScore = checkHScore(_options.regionVerb.hScore),
                            outfileName = setOutFileName(resDir),
                            fileType = setFileType(nCols),
                            noValueAssigned = true,
                            unknownSummit = true,
                            mMem = mMem,
                            memPrcntg = memPrcntg,
                            chromosomeNamesAndLength = checkChromInfo(_options.regionVerb.getChrInfo),
                            ignoreChromosomeLength = checkIgnoreChrInfo(_options.regionVerb.ignoreChrInfo)
                        };
                    threadList.Add(Task.Factory.StartNew(() => cr.start()));
                    //cr.start();
                    break;
                case "tfNet":
                    mMem = mst(memPrcntg = checkMemoryPercentage(_options.tfNetVerb.memPrcntg));
                    threadList.Add(Task.Factory.StartNew(() => mMem.DoWork()));
                    fromPeaksToRegions fpr = new fromPeaksToRegions(_invokedVerb)
                        {
                            ignoreChromosomeLength = checkIgnoreChrInfo(_options.tfNetVerb.ignoreChrInfo),
                            chromosomeNamesAndLength = checkChromInfo(_options.tfNetVerb.getChrInfo),
                            filterByScore = checkFilterByScore(_options.tfNetVerb.filterByScore),
                            filterByPvalue = checkFilterByPvalue(_options.tfNetVerb.filterByPvalue),
                            filterByQvalue = checkFilterByQvalue(_options.tfNetVerb.filterByQvalue),
                            filterBySV = checkFilterBySV(_options.tfNetVerb.filterBySV),
                            noValueAssigned = checkNoValueAssigned(_options.tfNetVerb.noValueAssigned),
                            unknownSummit = checkUnknownSummit(_options.tfNetVerb.unknownSummit),
                            peakFilesDirectory = checkInput(_options.tfNetVerb.inputValue),
                            sortPeaks = checkSortPeaks(_options.tfNetVerb.sortPeaks),
                            //regionPvalue = checkRegionPvalues(_options.tfNetVerb.regionPvalues),
                            //minusLog10 = checkMinusLogTen(_options.tfNetVerb.minusLogTen),
                            sortRegionsBySore = checkSortRegions(_options.tfNetVerb.sortRegions),
                            //cutoffValue = checkCutoffValue(_options.tfNetVerb.peakInRegionCutoff),
                            numOfCols = checkNumberOfColumns(_options.tfNetVerb.numOfCols),
                            tfList = checkTfList(_options.tfNetVerb.getTfList),
                            resultsDirectory = checkOutput(_options.tfNetVerb.outputValue, _options.tfNetVerb.inputValue),
                            mode = mode,
                            narrowThePeak = checkPeakNoarrowing(_options.tfNetVerb.narrowThePeak),
                            summitWindow = checkSummitWindow(_options.tfNetVerb.summitWindow),
                            peakDistance = checkPeakDistance(_options.tfNetVerb.peakDistance, _options.tfNetVerb.getChrInfo),
                            topX = checkTopX(_options.tfNetVerb.topX),
                            topXP = checkTopXP(_options.tfNetVerb.topXP),
                            strandSpecificity = checkStrandSpecificity(_options.tfNetVerb.strandSpecificity),
                            statFile = checkStatFile(_options.tfNetVerb.statFile),
                            xmlFile = checkXmlFile(_options.tfNetVerb.xmlFile),
                            peakFile = checkPeakFile(_options.tfNetVerb.peakFile),
                            distanceOption = checkPeakDistanceOption(_options.tfNetVerb.distanceOption),
                            lowMemory = checkLowMemory(_options.tfNetVerb.lowMemory),
                            chromosome = checkChromosome(_options.tfNetVerb.chromosome),
                            startIndex = checkStart(_options.tfNetVerb.start),
                            endIndex = checkEnd(_options.tfNetVerb.end),
                            regsName = checkRegName(_options.tfNetVerb.regName, _options.tfNetVerb.regNameFile),
                            tfName = checkTfNames(_options.tfNetVerb.tfName),
                            peakName = checkPkNames(_options.tfNetVerb.pkName),
                            lowerScore = checkLScore(_options.tfNetVerb.lScore),
                            higherScore = checkHScore(_options.tfNetVerb.hScore),
                            filterOption = checkFilterOption(_options.tfNetVerb.filterOption),
                            filterValue = checkFIlterValue(_options.tfNetVerb.filterValue),
                            neighborDistanceLow = checkNeighboringDistance(_options.tfNetVerb.neighborDistance),
                            neighborDistanceHigh = highNeigh,
                            overlapDistance = checkOverlappingDistance(_options.tfNetVerb.overlapDistance),
                            title = checkTitle(_options.tfNetVerb.title),
                            noRscript = checkForNoRscript(_options.tfNetVerb.noR),
                            Rscript = checkForRscript(_options.tfNetVerb.Rscript),
                            scriptPath = checkForScriptPath(_options.tfNetVerb.scriptPath),
                            outfileName = setOutFileName(resDir),
                            fileType = setFileType(nCols),
                            mMem = mMem,
                            memPrcntg = memPrcntg,
                            acceptedFileExtensions = checkAcceptedFileExtensions(_options.tfNetVerb.accFilExt),
                            fileNameAsTfName = checkFileNameAsTfName(_options.tfNetVerb.fileNameAsTfName)
                        };
                    threadList.Add(Task.Factory.StartNew(() => fpr.start()));
                    //fpr.start();
                    break;
                default:
                    break;
            }

            while (threadList.Any(t => !t.IsCompleted)) { }
        }

        static monitorMemory mst(double _memPrcntg)
        {
            return new monitorMemory() { memPrcntg = _memPrcntg };
        }
    }
}
