# tfNet
<p align="justify">
tfNet is a computational tool implemented in C# for whole genome identification of putative regulatory regions and genomic signal interactions. The input of the tool is a set of ChIP-seq peak signals (or any bed files) and it computes putative regulatory regions under the assumption that the ChIP-seq signals tend to bind in close proximity to each other due to their synergistic nature.<br />
tfNet comes in a 32-bit (x86) and in a 64-bit (x64) build architecture. It runs in all Windows computers under the .NET 4.5 framework and in all Linux/OSX computers under mono. The implementation of the region detection algorithm is parallel in order to achieve better performance. Additionally to region detection, tfNet is able to generate maps of ChIP-seq signal interaction networks as described in (Diamanti et al., 2015. "Maps of context-dependent putative regulatory regions and genomic signal interactions". Nucleic Acids Research). To make this functionality possible you need to install R and the R packages qvalue, igraph and gplots.<br />
tfNet offers a plethora of functions for filtering the resulting set of putative regulatory regions. Additionally, this bioinformatics tool allows an extended parametrization in order to best adapt to the needs of researchers. In total there are 5 major functions. The first 4 perform individual steps of the algorithm such as filtering of putative regulatory regions and ChIP-seq signal interaction extraction. The fifth function runs all the previous 4 in a unified pipeline.<br />
Here you can download the source code, modify it according to your needs etc. If you just need to run tfNet they you might prefer to download the executable <a href="https://github.com/klevdiamanti/tfNet/tfNet/tfNet_x64.exe">tfNet_x64.exe</a> or <a href="https://github.com/klevdiamanti/tfNet/tfNet/tfNet_x86.exe">tfNet_x86.exe</a> together with the <a href="https://github.com/klevdiamanti/tfNet/tfNet/tfNet.R">R script for the network generation</a>.
</p>

## Run tfNet
```
[mono] tfNet_xBB.exe [verb] [options]
```
(BB is the build 64 or 86)

### Verbs
- peaks
- regions
- filter
- network
- tfNet
- help

#### peaks
<p align="justify">
Combines a given set of bed files into a single bed file to be used in the next steps of tfNet. <br /><br />
<b>Input:</b> A set of bed files. Each file should represent one ChIP-seq signal and the file name should contain the ChIP-seq signal name.<br />
<b>Output:</b> A single file in bed format that contains all the input ChIP-seq signal peaks (check the Appendix for more details about the file format).
<p>

```
tfNet_xBB.exe peaks [-i path] [-o path] [--mem integer] [--cols integer] [--tfList path] [--chrInfo path] [-a/--ignoreChrInfo flag] [-v/--noValue flag] [-u/--noSummit flag] [--score integer] [--sValue double] [--pValue double] [--qValue double] [--sort character] [--acc flag] [-t/--fNameTfName flag] [-n/--nPeak flag] [--win integer]
```
**Required parameters**
<table>
	<tr>
		<td>-i</td>
		<td>The directory where the set of files representing ChIP-seq signals are available. There should be one file for each ChIP-seq signal and the name of the corresponding ChIP-seq signal should be included in the file name. The file format should be bed.</td>
	</tr>
</table>

**Optional parameters**
<table>
	<tr>
		<td>-o</td>
		<td>The full path of the output directory where the file(s) created will be placed. The default value is “tfNet_default”. In case the default value is not changed there will be a new directory created under the parent directory of the -i option provided. In case a different output directory is needed it needs to be of the format: /full/path/to/the/output/directory/name.</td>
	</tr>
	<tr>
		<td>--mem</td>
		<td>Memory percentage to be used by tfNet. The default value is 30%. In case you are not secure that the amount of data you input is too large or if the machine you are running the software does not have enough memory then you can use this option to notify you in cases of high memory usage. You can combine this option with the -l or --lowMemory to run tfNet in a single thread mode in order to prevent it from crashing (when available).</td>
	</tr>
	<tr>
		<td>--cols</td>
		<td>The total number of tab-separated columns you prefer to be in your output files in bed format. The default value is 10 (narrowPeak format). For more information please refer to https://genome.ucsc.edu/FAQ/FAQformat.html.</td>
	</tr>
	<tr>
		<td>--tfList</td>
		<td>A file containing all the names of the ChIP-seq signals that the input files represent. The default list contains the 631 ChIP-seq signals listed in the appendix (Default ChIP-seq signal list). The ChIP-seq signal names in the file should be in a semicolon-separated format. This option is mutually exclusive with -t or --fNameTfName in case the ChIP-seq signal name is the full file name.</td>
	</tr>
	<tr>
		<td>--chrInfo</td>
		<td>A file containing information about the chromosome names of the species and the chromosome length. The default species is human and the chromosome coordinates are from http://www.ncbi.nlm.nih.gov/projects/genome/assembly/grc/human/data/index.shtml. The file format should be of the following format: chr1,249250621;chr2,243199373;. This option is mutually exclusive with -a or --ignoreChrInfo in case there is no information about the chromosome length or if this is of no interest.</td>
	</tr>
	<tr>
		<td>-a, --ignoreChrInfo</td>
		<td>Ignore chromosome name and length correctness for ChIP-seq signals and the detected regions. Mutually exclusive with --chriInfo.</td>
	</tr>
	<tr>
		<td>-v, --noValue</td>
		<td>Discard any input peaks (records) with a pValue or qValue equal to -1. The default value is false.</td>
	</tr>
	<tr>
		<td>-u, --noSummit</td>
		<td>Discard any input peaks (records) with a summit value equal to -1. The default value is false.</td>
	</tr>
	<tr>
		<td>--score</td>
		<td>Discard any input peaks (records) with a score value lower than the given one. The default value is -1 so that no peak is discarded.</td>
	</tr>
	<tr>
		<td>--sValue</td>
		<td>Discard any input peaks (records) with a signal value lower than the given one. The default value is -1 so that no peak is discarded.</td>
	</tr>
	<tr>
		<td>--pValue</td>
		<td>Discard any input peaks (records) with a p value higher than the given one. The default value is -1 so that no peak is discarded. The input may be also of scientific format (e.g. 5e-10).</td>
	</tr>
	<tr>
		<td>--qValue</td>
		<td>Discard any input peaks (records) with a q value higher than the given one. The default value is -1 so that no peak is discarded. The input may be also of scientific format (e.g. 5e-10).</td>
	</tr>
	<tr>
		<td>--sort</td>
		<td>Sort the ChIP-seq signals after merging them all together. The default option is N, meaning that no sort is required. Other options are sort peaks by start only (S), sort peaks by start+summit (M) and sort peaks by start+middle (P).</td>
	</tr>
	<tr>
		<td>--acc</td>
		<td>The accepted file extensions to be considered for the input. The default value is “narrowPeak”. The list provided should be comma separated.</td>
	</tr>
	<tr>
		<td>-t, --fNameTfName</td>
		<td>Use the file name as ChIP-seq signal name (extension excluded). This is a mutually exclusive with --tfList option that is used to provide the file names.</td>
	</tr>
	<tr>
		<td>-n, --nPeak</td>
		<td>The default option is to narrow the peaks in a window of a few base pairs around the summit or the middle point. The windows distance is given by the --win option. Activating this option the peak is not narrowed and the input peak size is used.</td>
	</tr>
	<tr>
		<td>--win</td>
		<td>The window size around the summit that the peak should be narrowed. The default value is 10bp. This is a mutually exclusive option with the -n or --nPeak.</td>
	</tr>
<table>

#### regions 
<p align="justify">
Detects clusters of ChIP-seq signals that constitute putative regulatory regions based on (Diamanti et al., 2016). <br /><br />
<b>Input:</b> A single file in bed format that contains all the input ChIP-seq signal peaks. Mind that the file should be of the same file format as the output of the peaks verb above.<br />
<b>Output:</b> A collection of files regarding the detected regulatory regions:
<ul>
  <li>*_regions.narrowPeak the set of putative regulatory regions in bed format (check the Appendix for more details about the file format).</li>
  <li>*_regions.xml the set of putative regulatory regions in xml format (check the Appendix for more details about the file format).</li>
  <li>*_regions_peaks.narrowPeak the set of ChIP-seq signal used for the detected putative regulatory regions (the file format is the same as the output from the peaks verb).</li>
	<li>*_statistics.csv some basic statistics about the detected putative regulatory regions such as the total number of regions, mean length, regions per chromosome etc. (check the Appendix for more details about the file format).</li>
</ul>
<p>

```
tfNet_xBB.exe regions [-i path] [-o path] [--mem integer] [--cols integer] [--tfList path] [--chrInfo path] [-a/--ignoreChrInfo flag] [-n/--nPeak flag] [--win integer] [-e/--startEnd flag] [--distance integer] [--chr string] [--tfName string] [--pkName string] [--start integer] [--end integer ] [--lScore integer] [--hScore integer] [-s/--strand flag] [-t/--statistics flag] [-x/--xml flag] [-p/--peaks flag] [-l/--lowMemory flag] [--topX integer] [--topXP integer] [--sort character]
```
**Required parameters**
<table>
	<tr>
		<td>-i</td>
		<td>the file where the set of the ChIP-seq signals is available in bed format. There should be one file containing all the available ChIP-seq signal, preferably the output from the peaks verb. The name field of this file should contain the ChIP-seq signal name and the peak name in the following format: “ChIPseqName_PeakName”.</td>
	</tr>
</table>

**Optional parameters**
<table>
	<tr>
		<td>-o</td>
		<td>The full path of the output directory where the file(s) created will be placed. The default value is “tfNet_default”. In case the default value is not changed there will be a new directory created under the parent directory of the -i option provided. In case a different output directory is needed it needs to be of the format: /full/path/to/the/output/directory/name.
	</tr>
	<tr>
		<td>--mem</td>
		<td>Memory percentage to be used by tfNet. The default value is 30%. In case you are not secure that the amount of data you input is too large or if the machine you are running the software does not have enough memory then you can use this option to notify you in cases of high memory usage. You can combine this option with the -l or --lowMemory to run tfNet in a single thread mode in order to prevent it from crashing (when available).
	</tr>
	<tr>
		<td>--cols</td>
		<td>The total number of tab-separated columns you prefer to be in your output files in bed format. The default value is 10 (narrowPeak format). For more information please refer to https://genome.ucsc.edu/FAQ/FAQformat.html.
	</tr>
	<tr>
		<td>--tfList</td>
		<td>A file containing all the names of the ChIP-seq signals that the input files represent. The default list contains the 631 ChIP-seq signals listed in the appendix (Default ChIP-seq signal list). The ChIP-seq signal names in the file should be in a semicolon-separated format. This option is mutually exclusive with -t or --fNameTfName in case the ChIP-seq signal name is the full file name.
	</tr>
	<tr>
		<td>--chrInfo</td>
		<td>A file containing information about the chromosome names of the species and the chromosome length. The default species is human and the chromosome coordinates are from http://www.ncbi.nlm.nih.gov/projects/genome/assembly/grc/human/data/index.shtml. The file format should be of the following format: chr1,249250621;chr2,243199373;. This option is mutually exclusive with -a or --ignoreChrInfo in case there is no information about the chromosome length or if this is of no interest.
	</tr>
	<tr>
		<td>-a, --ignoreChrInfo</td>
		<td>Ignore chromosome name and length correctness for ChIP-seq signals and the detected regions. Mutually exclusive with --chriInfo.
	</tr>
	<tr>
		<td>-n, --nPeak</td>
		<td>The default option is to narrow the peaks in a window of a few base pairs around the summit or the middle point. The windows distance is given by the --win option. Activating this option the peak is not narrowed and the input peak size is used.
	</tr>
	<tr>
		<td>--win</td>
		<td>The window size around the summit that the peak should be narrowed. The default value is 10bp. This is a mutually exclusive option with the -n or --nPeak.
	</tr>
	<tr>
		<td>-e, --startEnd</td>
		<td>Create ChIP-seq signal-clusters based on middle-point distances. By default the tool considers summit distances.
	</tr>
	<tr>
		<td>--distance</td>
		<td>The distance threshold between peaks in order to cluster them in the same region. The default value is 300bp.
	</tr>
	<tr>
		<td>--chr</td>
		<td>Discard regions that are not in the chromosomes. For multiple chromosomes use a comma-separated string. By default all chromosomes are considered.
	</tr>
	<tr>
		<td>--tfName</td>
		<td>Discard regions that do not contain the given list of transcription factors (comma separated). By default all transcription factors are considered.
	</tr>
	<tr>
		<td>--pkName</td>
		<td>Discard regions that do not contain the given list of peaks (comma separated). You need to provide the input peak names. By default all peaks are considered.
	</tr>
	<tr>
		<td>--start</td>
		<td>Discard regions that have a starting position lower than the provided coordinate. By default the regions are considered.
	</tr>
	<tr>
		<td>--end</td>
		<td>Discard regions that have an ending position larger than the provided coordinate. By default the regions are considered.
	</tr>
	<tr>
		<td>--lScore</td>
		<td>Discard regions with score lower than the provided one. By default the regions are considered.
	</tr>
	<tr>
		<td>--hScore</td>
		<td>Discard regions with score larger than the provided one. By default the regions are considered.
	</tr>
	<tr>
		<td>-s, --strand</td>
		<td>Cluster ChIP-seq signals in a strand specific manner. By default the strand specificity is not forced for the regions detection.
	</tr>
	<tr>
		<td>-t, --statistics</td>
		<td>Do not print the I/O statistics file. By default the file is printed.
	</tr>
	<tr>
		<td>-x, --xml</td>
		<td>Do not print the detailed (information rich) xml file. By default the file is printed.
	</tr>
	<tr>
		<td>-p, --peaks</td>
		<td>Do not print peaks that were clustered together to constitute the regions in a file. By default the file is printed.
	</tr>
	<tr>
		<td>-l, --lowMemory</td>
		<td>Cluster ChIP-seq signals into regions on low a memory consumption. This option omits the parallel detection of regions and it does it in a slower, old-fashioned and memory-cheap manner. We suggest you to apply this option in case the --mem option fires and prints a warning message.
	</tr>
	<tr>
		<td>--topX</td>
		<td>Sort regions by score and keep the top of them. The top regions are selected based on the provided threshold.
	</tr>
	<tr>
		<td>--topXP</td>
		<td>Sort regions by score and keep the top percentage of them. The top regions are selected based on the provided percentage threshold.
	</tr>
	<tr>
		<td>--sort</td>
		<td>Sort the regions by score. The default option is N, meaning that no sort is required. Other options are sort regions by ascending score (A) and sort regions by descending score (D).
	</tr>
</table>

#### filter 
<p align="justify">
Filters a provided set of putative regulatory regions according to the given set of arguments. <br /><br />
<b>Input:</b>A single file in bed or xml format that contains a set of detected putative regulatory regions.<br />
<b>Output:</b>A collection of files regarding the filtered regulatory regions:
<ul>
  <li>*_regions.narrowPeak the set of putative regulatory regions in bed format (check the Appendix for more details about the file format).</li>
  <li>*_regions.xml the set of putative regulatory regions in xml format (check the Appendix for more details about the file format).</li>
  <li>*_regions_peaks.narrowPeak the set of ChIP-seq signal used for the detected putative regulatory regions (the file format is the same as the output from the peaks verb).</li>
	<li>*_statistics.csv some basic statistics about the detected putative regulatory regions such as the total number of regions, mean length, regions per chromosome etc. (check the Appendix for more details about the file format).</li>
</ul>
<p>

```
tfNet_xBB.exe filter [-i path] [-o path] [--mem integer] [--cols integer] [--tfList path] [--chrInfo path] [-a/--ignoreChrInfo flag] [-e/--startEnd flag] [--distance integer] [--chr string] [--tfName string] [--pkName string] [--start integer] [--end integer ] [--lScore integer] [--hScore integer] [-s/--strand flag] [-t/--statistics flag] [-x/--xml flag] [-p/--peaks flag] [-l/--lowMemory flag] [--topX integer] [--topXP integer] [--sort character] [--reg string] [--regFile path]
```
**Required parameters**
<table>
	<tr>
		<td>-i</td>
		<td>the file where the set of the ChIP-seq signals is available in bed format. There should be one file containing all the available ChIP-seq signal, preferably the output from the peaks verb. The name field of this file should contain the ChIP-seq signal name and the peak name in the following format: “ChIPseqName_PeakName”.</td>
	</tr>
</table>

**Optional parameters**
<table>
	<tr>
		<td>-o</td>
		<td>The full path of the output directory where the file(s) created will be placed. The default value is “tfNet_default”. In case the default value is not changed there will be a new directory created under the parent directory of the -i option provided. In case a different output directory is needed it needs to be of the format: /full/path/to/the/output/directory/name.</td>
	</tr>
	<tr>
		<td>--mem</td>
		<td>Memory percentage to be used by tfNet. The default value is 30%. In case you are not secure that the amount of data you input is too large or if the machine you are running the software does not have enough memory then you can use this option to notify you in cases of high memory usage. You can combine this option with the -l or --lowMemory to run tfNet in a single thread mode in order to prevent it from crashing (when available).</td>
	</tr>
	<tr>
		<td>--cols</td>
		<td>The total number of tab-separated columns you prefer to be in your output files in bed format. The default value is 10 (narrowPeak format). For more information please refer to https://genome.ucsc.edu/FAQ/FAQformat.html.</td>
	</tr>
	<tr>
		<td>--tfList</td>
		<td>A file containing all the names of the ChIP-seq signals that the input files represent. The default list contains the 631 ChIP-seq signals listed in the appendix (Default ChIP-seq signal list). The ChIP-seq signal names in the file should be in a semicolon-separated format. This option is mutually exclusive with -t or --fNameTfName in case the ChIP-seq signal name is the full file name.</td>
	</tr>
	<tr>
		<td>--chrInfo</td>
		<td>A file containing information about the chromosome names of the species and the chromosome length. The default species is human and the chromosome coordinates are from http://www.ncbi.nlm.nih.gov/projects/genome/assembly/grc/human/data/index.shtml. The file format should be of the following format: chr1,249250621;chr2,243199373;. This option is mutually exclusive with -a or --ignoreChrInfo in case there is no information about the chromosome length or if this is of no interest.</td>
	</tr>
	<tr>
		<td>-a, --ignoreChrInfo</td>
		<td>Ignore chromosome name and length correctness for ChIP-seq signals and the detected regions. Mutually exclusive with --chriInfo.</td>
	</tr>
	<tr>
		<td>--chr</td>
		<td>Discard regions that are not in the chromosomes. For multiple chromosomes use a comma-separated string. By default all chromosomes are considered.</td>
	</tr>
	<tr>
		<td>--tfName</td>
		<td>Discard regions that do not contain the given list of transcription factors (comma separated). By default all transcription factors are considered.</td>
	</tr>
	<tr>
		<td>--pkName</td>
		<td>Discard regions that do not contain the given list of peaks (comma separated). You need to provide the input peak names. By default all peaks are considered.</td>
	</tr>
	<tr>
		<td>--start</td>
		<td>Discard regions that have a starting position lower than the provided coordinate. By default the regions are considered.</td>
	</tr>
	<tr>
		<td>--end</td>
		<td>Discard regions that have an ending position larger than the provided coordinate. By default the regions are considered.</td>
	</tr>
	<tr>
		<td>--lScore</td>
		<td>Discard regions with score lower than the provided one. By default the regions are considered.</td>
	</tr>
	<tr>
		<td>--hScore</td>
		<td>Discard regions with score larger than the provided one. By default the regions are considered.</td>
	</tr>
	<tr>
		<td>-s, --strand</td>
		<td>Cluster ChIP-seq signals in a strand specific manner. By default the strand specificity is not forced for the regions detection.</td>
	</tr>
	<tr>
		<td>-t, --statistics</td>
		<td>Do not print the I/O statistics file. By default the file is printed.</td>
	</tr>
	<tr>
		<td>-x, --xml</td>
		<td>Do not print the detailed (information rich) xml file. By default the file is printed.</td>
	</tr>
	<tr>
		<td>-p, --peaks</td>
		<td>Do not print peaks that were clustered together to constitute the regions in a file. By default the file is printed.</td>
	</tr>
	<tr>
		<td>-l, --lowMemory</td>
		<td>Cluster ChIP-seq signals into regions on low a memory consumption. This option omits the parallel detection of regions and it does it in a slower, old-fashioned and memory-cheap manner. We suggest you to apply this option in case the --mem option fires and prints a warning message.</td>
	</tr>
	<tr>
		<td>--topX</td>
		<td>Sort regions by score and keep the top of them. The top regions are selected based on the provided threshold.</td>
	</tr>
	<tr>
		<td>--topXP</td>
		<td>Sort regions by score and keep the top percentage of them. The top regions are selected based on the provided percentage threshold.</td>
	</tr>
	<tr>
		<td>--sort</td>
		<td>Sort the regions by score. The default option is N, meaning that no sort is required. Other options are sort regions by ascending score (A) and sort regions by descending score (D) </td>
	</tr>
	<tr>
		<td>--reg</td>
		<td>Discard regions except from the given ones. For multiple regions use a comma separated string. For very long lists of region names please use the option --regFile.</td>
	</tr>
	<tr>
		<td>--regFile</td>
		<td>Discard regions except from the given ones. For multiple regions use a comma separated string. For more than 20 regions please use this option and not the --reg.</td>
	</tr>
</table>

#### network  
<p align="justify">
Calculates the frequency of each ChIP-seq signal interaction-pair and calls the provided Rscript in order to create the interaction maps. <br /><br />
<b>Input:</b> A single file in bed or xml format that contains a set of detected putative regulatory regions.<br />
<b>Output:</b> A collection of files regarding the interactions of ChIP-seq signals detected in the provided set of regulatory regions:
<ul>
  <li>*_cooccuring.csv_filtered.csv contains the number of pairs of ChIP-seq signal interactions co-occurring in the regulatory regions.</li>
<li>*_cooccuring.pdf_filtered.csv the network of pairs of ChIP-seq signal interactions co-occurring in regulatory regions. The network is represented by a heatmap. The color intensity of the heatmap tiles is calculated from the Bonferroni corrected p-value of the binomial distribution.</li>
<li>*_neighboring.csv_filtered.csv contains the number of pairs of ChIP-seq signal interactions neighboring each other in the regulatory regions.</li>
<li>*_ neighboring.pdf_filtered.csv the network of pairs of ChIP-seq signal interactions neighboring each other in the regulatory regions. The network is represented by a heatmap. The color intensity of the heatmap tiles is calculated from the Bonferroni corrected p-value of the hypergeometric distribution.</li>
<li>*_overlapping.csv_filtered.csv contains the number of pairs of ChIP-seq signal interactions overlapping each other in the regulatory regions.</li>
<li>*_ overlapping.pdf_filtered.csv the network of pairs of ChIP-seq signal interactions overlapping each other in the regulatory regions. The network is represented by a heatmap. The color intensity of the heatmap tiles is calculated from the Bonferroni corrected p-value of the hypergeometric distribution.</li>
</ul>
<i>Note:</i> the calculation of the ChIP-signal interactions are explained in detail in the publication (Diamanti et al., 2016).
<p>

```
tfNet_xBB.exe network [-i path] [-o path] [--mem integer] [--cols integer] [--tfList path] [--chrInfo path] [-a/--ignoreChrInfo flag] [--fopt character] [--fval double] [--neigh string] [--overlap integer] [--title string] [--Rscript string] [--scr string] [-c/--noR flag]
```
**Required parameters**
<table>
	<tr>
		<td>-i</td>
		<td>The file where the set of the regions is available. There should be one file containing all the available regions, preferably the output from the regions verb. If the file is in bed format then the name field of this file should contain the regions id, the ChIP-seq signal names and the peak names in the following format: “regID-ChIPseqName1_PeakName,ChIPseqName2_PeakName”. If the file name is in xml format then it should be of the format that the regions verb provides. Note: The xml file is suggested.</td>
	</tr>
</table>

**Optional parameters**
<table>
	<tr>
		<td>-o</td>
		<td>The full path of the output directory where the file(s) created will be placed. The default value is “tfNet_default”. In case the default value is not changed there will be a new directory created under the parent directory of the -i option provided. In case a different output directory is needed it needs to be of the format: /full/path/to/the/output/directory/name.</td>
	</tr>
	<tr>
		<td>--mem</td>
		<td>Memory percentage to be used by tfNet. The default value is 30%. In case you are not secure that the amount of data you input is too large or if the machine you are running the software does not have enough memory then you can use this option to notify you in cases of high memory usage. You can combine this option with the -l or --lowMemory to run tfNet in a single thread mode in order to prevent it from crashing (when available).</td>
	</tr>
	<tr>
		<td>--cols</td>
		<td>The total number of tab-separated columns you prefer to be in your output files in bed format. The default value is 10 (narrowPeak format). For more information please refer to https://genome.ucsc.edu/FAQ/FAQformat.html.</td>
	</tr>
	<tr>
		<td>--tfList</td>
		<td>A file containing all the names of the ChIP-seq signals that the input files represent. The default list contains the 631 ChIP-seq signals listed in the appendix (Default ChIP-seq signal list). The ChIP-seq signal names in the file should be in a semicolon-separated format. This option is mutually exclusive with -t or --fNameTfName in case the ChIP-seq signal name is the full file name.</td>
	</tr>
	<tr>
		<td>--chrInfo</td>
		<td>A file containing information about the chromosome names of the species and the chromosome length. The default species is human and the chromosome coordinates are from http://www.ncbi.nlm.nih.gov/projects/genome/assembly/grc/human/data/index.shtml. The file format should be of the following format: chr1,249250621;chr2,243199373;. This option is mutually exclusive with -a or --ignoreChrInfo in case there is no information about the chromosome length or if this is of no interest.</td>
	</tr>
	<tr>
		<td>-a, --ignoreChrInfo</td>
		<td>Ignore chromosome name and length correctness for ChIP-seq signals and the detected regions. Mutually exclusive with --chriInfo.</td>
	</tr>
	<tr>
		<td>--fopt</td>
		<td>The filter method of the ChIP-seq pair interaction. This is based on p values, q values or Bonferroni corrected p values. The default option is b, hence Bonferroni corrected p values. You may also use p for p values and q for q values.</td>
	</tr>
	<tr>
		<td>--fval</td>
		<td>The cutoff threshold for the statistically significant ChIP-signal interactions. It is combined with the --fopt option. The default value is 0.05.</td>
	</tr>
	<tr>
		<td>--neigh</td>
		<td>The distance thresholds so that two ChIP-seq peaks are considered to be neighboring. The distance is based on the –e/--startEnd option. The string should consist of two integers separated by a comma. The first value represents the lower bound value and the second one the upper bound value. The default values are 20 and 60 bp (20,60).</td>
	</tr>
	<tr>
		<td>--overlap</td>
		<td>The distance threshold so that two ChIP-seq peaks are considered to be overlapping. The distance is based on the –e/--startEnd option. The default value is 0bp. In case the value is >0 then all the distances between 0 and the given one are considered as overlapping.</td>
	</tr>
	<tr>
		<td>--title</td>
		<td>The prefix that should be used for the network title in the heatmap pdf files.</td>
	</tr>
	<tr>
		<td>--Rscript</td>
		<td>If the Rscript is not installed in your machine as a global variable the you should use this option to provide the full path of where the Rscript is located. By default tfNet assumes that Rscript is installed as a global variable.</td>
	</tr>
	<tr>
		<td>--scr</td>
		<td>The full path of the provided script in R that generated the heatmap pdf networks is located. The tfNet assumes that the script is located under the same path as the tool. If not then you should provide the full path of where it is located.</td>
	</tr>
	<tr>
		<td>-c, --noR</td>
		<td>Do not run Rscript. Enable this flag in case you are not interested in generating the heatmap pdf files. By default this flag is disabled.</td>
	</tr>
</table>


#### tfNet  
<p align="justify">
This verb implements the whole pipeline described above. It first runs “peaks”, then “regions” and “filtering” on-the-fly, and finally it runs “networks”.<br /><br />
<b>Input:</b> A set of bed files. Each file should represent one ChIP-seq signal and the file name should contain the ChIP-seq signal name.<br />
<b>Output:</b> A collection of files regarding the detected regulatory regions and the interactions of ChIP-seq signals in these regulatory regions:<br />
<ul>
  <li>*_peaks.narrowPeak a single file in bed format that contains all the input ChIP-seq signal peaks (check the Appendix for more details about the file format).</li>
	<li>*_regions.narrowPeak the set of putative regulatory regions in bed format (check the Appendix for more details about the file format).</li>
	<li>*_regions.xml the set of putative regulatory regions in xml format (check the Appendix for more details about the file format).</li>
	<li>*_regions_peaks.narrowPeak the set of ChIP-seq signal used for the detected putative regulatory regions (the file format is the same as the output from the peaks verb).</li>
	<li>*_statistics.csv some basic statistics about the detected putative regulatory regions such as the total number of regions, mean length, regions per chromosome etc. (check the Appendix for more details about the file format).</li>
	<li>*_cooccuring.csv_filtered.csv contains the number of pairs of ChIP-seq signal interactions co-occurring in the regulatory regions.</li>
	<li>*_cooccuring.pdf_filtered.csv the network of pairs of ChIP-seq signal interactions co-occurring in regulatory regions. The network is represented by a heatmap. The color intensity of the heatmap tiles is calculated from the Bonferroni corrected p-value of the binomial distribution.</li>
	<li>*_neighboring.csv_filtered.csv contains the number of pairs of ChIP-seq signal interactions neighboring each other in the regulatory regions.</li>
	<li>*_ neighboring.pdf_filtered.csv the network of pairs of ChIP-seq signal interactions neighboring each other in the regulatory regions. The network is represented by a heatmap. The color intensity of the heatmap tiles is calculated from the Bonferroni corrected p-value of the hypergeometric distribution.</li>
	<li>*_overlapping.csv_filtered.csv contains the number of pairs of ChIP-seq signal interactions overlapping each other in the regulatory regions.</li>
	<li>*_ overlapping.pdf_filtered.csv the network of pairs of ChIP-seq signal interactions overlapping each other in the regulatory regions. The network is represented by a heatmap. The color intensity of the heatmap tiles is calculated from the Bonferroni corrected p-value of the hypergeometric distribution.</li>
</ul>
<p>

```
tfNet_x64.exe peaks [-i path] [-o path] [--mem integer] [--cols integer] [--tfList path] [--chrInfo path] [-a/--ignoreChrInfo flag] [-v/--noValue flag] [-u/--noSummit flag] [--score integer] [--sValue double] [--pValue double] [--qValue double] [--sort character] [--acc flag] [-t/--fNameTfName flag] [-n/--nPeak flag] [--win integer] [-e/--startEnd flag] [--distance integer] [--chr string] [--tfName string] [--pkName string] [--start integer] [--end integer ] [--lScore integer] [--hScore integer] [-s/--strand flag] [-t/--statistics flag] [-x/--xml flag] [-p/--peaks flag] [-l/--lowMemory flag] [--topX integer] [--topXP integer] [--sortR character] [--reg string] [--regFile path] [--fopt character] [--fval double] [--neigh string] [--overlap integer] [--title string] [--Rscript string] [--scr string] [-c/--noR flag]
```
**Required parameters**
<table>
	<tr>
		<td>-i</td>
		<td>The directory where the set of files representing ChIP-seq signals are available. There should be one file for each ChIP-seq signal and the name of the corresponding ChIP-seq signal should be included in the file name. The file format should be bed.</td>
	</tr>
</table>

**Optional parameters**
<table>
	<tr>
		<td>-o</td>
		<td>The full path of the output directory where the file(s) created will be placed. The default value is “tfNet_default”. In case the default value is not changed there will be a new directory created under the parent directory of the -i option provided. In case a different output directory is needed it needs to be of the format: /full/path/to/the/output/directory/name.</td>
	</tr>
	<tr>
		<td>--mem</td>
		<td>Memory percentage to be used by tfNet. The default value is 30%. In case you are not secure that the amount of data you input is too large or if the machine you are running the software does not have enough memory then you can use this option to notify you in cases of high memory usage. You can combine this option with the -l or --lowMemory to run tfNet in a single thread mode in order to prevent it from crashing (when available).</td>
	</tr>
	<tr>
		<td>--cols</td>
		<td>The total number of tab-separated columns you prefer to be in your output files in bed format. The default value is 10 (narrowPeak format). For more information please refer to https://genome.ucsc.edu/FAQ/FAQformat.html.</td>
	</tr>
	<tr>
		<td>--tfList</td>
		<td>A file containing all the names of the ChIP-seq signals that the input files represent. The default list contains the 631 ChIP-seq signals listed in the appendix (Default ChIP-seq signal list). The ChIP-seq signal names in the file should be in a semicolon-separated format. This option is mutually exclusive with -t or --fNameTfName in case the ChIP-seq signal name is the full file name.</td>
	</tr>
	<tr>
		<td>--chrInfo</td>
		<td>A file containing information about the chromosome names of the species and the chromosome length. The default species is human and the chromosome coordinates are from http://www.ncbi.nlm.nih.gov/projects/genome/assembly/grc/human/data/index.shtml. The file format should be of the following format: chr1,249250621;chr2,243199373;. This option is mutually exclusive with -a or --ignoreChrInfo in case there is no information about the chromosome length or if this is of no interest.</td>
	</tr>
	<tr>
		<td>-a, --ignoreChrInfo</td>
		<td>Ignore chromosome name and length correctness for ChIP-seq signals and the detected regions. Mutually exclusive with --chriInfo.</td>
	</tr>
	<tr>
		<td>-v, --noValue</td>
		<td>Discard any input peaks (records) with a pValue or qValue equal to -1. The default value is false.</td>
	</tr>
	<tr>
		<td>-u, --noSummit</td>
		<td>Discard any input peaks (records) with a summit value equal to -1. The default value is false.</td>
	</tr>
	<tr>
		<td>--score</td>
		<td>Discard any input peaks (records) with a score value lower than the given one. The default value is -1 so that no peak is discarded.</td>
	</tr>
	<tr>
		<td>--sValue</td>
		<td>Discard any input peaks (records) with a signal value lower than the given one. The default value is -1 so that no peak is discarded.</td>
	</tr>
	<tr>
		<td>--pValue</td>
		<td>Discard any input peaks (records) with a p value higher than the given one. The default value is -1 so that no peak is discarded. The input may be also of scientific format (e.g. 5e-10).</td>
	</tr>
	<tr>
		<td>--qValue</td>
		<td>Discard any input peaks (records) with a q value higher than the given one. The default value is -1 so that no peak is discarded. The input may be also of scientific format (e.g. 5e-10).</td>
	</tr>
	<tr>
		<td>--sort</td>
		<td>Sort the ChIP-seq signals after merging them all together. The default option is N, meaning that no sort is required. Other options are sort peaks by start only (S), sort peaks by start+summit (M) and sort peaks by start+middle (P).</td>
	</tr>
	<tr>
		<td>--acc</td>
		<td>The accepted file extensions to be considered for the input. The default value is “narrowPeak”. The list provided should be comma separated.</td>
	</tr>
	<tr>
		<td>-t, --fNameTfName</td>
		<td>Use the file name as ChIP-seq signal name (extension excluded). This is a mutually exclusive with --tfList option that is used to provide the file names.</td>
	</tr>
	<tr>
		<td>-n, --nPeak</td>
		<td>The default option is to narrow the peaks in a window of a few base pairs around the summit or the middle point. The windows distance is given by the --win option. Activating this option the peak is not narrowed and the input peak size is used.</td>
	</tr>
	<tr>
		<td>--win</td>
		<td>The window size around the summit that the peak should be narrowed. The default value is 10bp. This is a mutually exclusive option with the -n or --nPeak.</td>
	</tr>
	<tr>
		<td>-e, --startEnd</td>
		<td>Create ChIP-seq signal-clusters based on middle-point distances. By default the tool considers summit distances.</td>
	</tr>
	<tr>
		<td>--distance</td>
		<td>The distance threshold between peaks in order to cluster them in the same region. The default value is 300bp.</td>
	</tr>
	<tr>
		<td>--chr</td>
		<td>Discard regions that are not in the chromosomes. For multiple chromosomes use a comma-separated string. By default all chromosomes are considered.</td>
	</tr>
	<tr>
		<td>--tfName</td>
		<td>Discard regions that do not contain the given list of transcription factors (comma separated). By default all transcription factors are considered.</td>
	</tr>
	<tr>
		<td>--pkName</td>
		<td>Discard regions that do not contain the given list of peaks (comma separated). You need to provide the input peak names. By default all peaks are considered.</td>
	</tr>
	<tr>
		<td>--start</td>
		<td>Discard regions that have a starting position lower than the provided coordinate. By default the regions are considered.</td>
	</tr>
	<tr>
		<td>--end</td>
		<td>Discard regions that have an ending position larger than the provided coordinate. By default the regions are considered.</td>
	</tr>
	<tr>
		<td>--lScore</td>
		<td>Discard regions with score lower than the provided one. By default the regions are considered.</td>
	</tr>
	<tr>
		<td>--hScore</td>
		<td>Discard regions with score larger than the provided one. By default the regions are considered.</td>
	</tr>
	<tr>
		<td>-s, --strand</td>
		<td>Cluster ChIP-seq signals in a strand specific manner. By default the strand specificity is not forced for the regions detection.</td>
	</tr>
	<tr>
		<td>-t, --statistics</td>
		<td>Do not print the I/O statistics file. By default the file is printed.</td>
	</tr>
	<tr>
		<td>-x, --xml</td>
		<td>Do not print the detailed (information rich) xml file. By default the file is printed.</td>
	</tr>
	<tr>
		<td>-p, --peaks</td>
		<td>Do not print peaks that were clustered together to constitute the regions in a file. By default the file is printed.</td>
	</tr>
	<tr>
		<td>-l, --lowMemory</td>
		<td>Cluster ChIP-seq signals into regions on low a memory consumption. This option omits the parallel detection of regions and it does it in a slower, old-fashioned and memory-cheap manner. We suggest you to apply this option in case the --mem option fires and prints a warning message.</td>
	</tr>
	<tr>
		<td>--topX</td>
		<td>Sort regions by score and keep the top of them. The top regions are selected based on the provided threshold.</td>
	</tr>
	<tr>
		<td>--topXP</td>
		<td>Sort regions by score and keep the top percentage of them. The top regions are selected based on the provided percentage threshold.</td>
	</tr>
	<tr>
		<td>--sortR</td>
		<td>Sort the regions by score. The default option is N, meaning that no sort is required. Other options are sort regions by ascending score (A) and sort regions by descending score (D) </td>
	</tr>
	<tr>
		<td>--reg</td>
		<td>Discard regions except from the given ones. For multiple regions use a comma separated string. For very long lists of region names please use the option --regFile.</td>
	</tr>
	<tr>
		<td>--regFile</td>
		<td>Discard regions except from the given ones. For multiple regions use a comma separated string. For more than 20 regions please use this option and not the --reg.</td>
	</tr>
	<tr>
		<td>--fopt</td>
		<td>The filter method of the ChIP-seq pair interaction. This is based on p values, q values or Bonferroni corrected p values. The default option is b, hence Bonferroni corrected p values. You may also use p for p values and q for q values.</td>
	</tr>
	<tr>
		<td>--fval</td>
		<td>The cutoff threshold for the statistically significant ChIP-signal interactions. It is combined with the --fopt option. The default value is 0.05.</td>
	</tr>
	<tr>
		<td>--neigh</td>
		<td>The distance thresholds so that two ChIP-seq peaks are considered to be neighboring. The distance is based on the –e/--startEnd option. The string should consist of two integers separated by a comma. The first value represents the lower bound value and the second one the upper bound value. The default values are 20 and 60 bp (20,60).</td>
	</tr>
	<tr>
		<td>--overlap</td>
		<td>The distance threshold so that two ChIP-seq peaks are considered to be overlapping. The distance is based on the –e/--startEnd option. The default value is 0bp. In case the value is >0 then all the distances between 0 and the given one are considered as overlapping.</td>
	</tr>
	<tr>
		<td>--title</td>
		<td>The prefix that should be used for the network title in the heatmap pdf files.</td>
	</tr>
	<tr>
		<td>--Rscript</td>
		<td>If the Rscript is not installed in your machine as a global variable the you should use this option to provide the full path of where the Rscript is located. By default tfNet assumes that Rscript is installed as a global variable.</td>
	</tr>
	<tr>
		<td>--scr</td>
		<td>The full path of the provided script in R that generated the heatmap pdf networks is located. The tfNet assumes that the script is located under the same path as the tool. If not then you should provide the full path of where it is located.</td>
	</tr>
	<tr>
		<td>-c, --noR</td>
		<td>Do not run Rscript. Enable this flag in case you are not interested in generating the heatmap pdf files. By default this flag is disabled.</td>
	</tr>
</table>

## Citation
Klev Diamanti, Husen M Umer, Marcin Kruczyk, Michał J Dąbrowski, Marco Cavalli, Claes Wadelius, Jan Komorowski (2016). "Maps of context-dependent putative regulatory regions and genomic signal interactions". Nucleic Acids Research 44(19):9110-9120.
