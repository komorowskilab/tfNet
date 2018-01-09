using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace tfNet
{
    class  validateArguments : masterClass
    {
        // Important Note:
		// due to the fact that the parser we use sets all the boolean variables to false by default
		// and switches them to true in case we add them in the arguments
		// we have developed methods to convert these boolean values according to the logic
        // used before the parser was added

        #region vars
        public string resDir { get; set; }
        public int nCols { get; set; }
        public int highNeigh { get; set; }
        bool noR = false;
        bool ignoChrLen = false;
        Dictionary<string, int> chromNamAndLen { get; set; }
        #endregion

        public validateArguments(string _mode) : base(_mode)
        {
            
        }

        #region common check
        public string checkInput(string _input)
        {
            switch (mode)
            {
                case "regions":
                case "network":
                case "filter":
                    if (!File.Exists(@"" + _input))
                    {
                        exit("the input file you provided does not exist");
                    }
                    break;
                case "peaks":
                case "tfNet":
                    if (!Directory.Exists(@"" + _input))
                    {
                        exit("the input directory you provided does not exist");
                    }
                    break;
                default:
                    break;
            }
            return _input;
        }

        public string checkOutput(string _output, string _input)
        {
            string resultsDirectory = "";
            if (_output == "tfNet_default")
            {
                resultsDirectory = string.Join(Convert.ToString(OSseparator), _input.Split(OSseparator).ToList().GetRange(0, _input.Split(OSseparator).Length - 1)) + OSseparator + _output;
            }
            else
            {
                if (Directory.Exists(@"" + string.Join(Convert.ToString(OSseparator), _output.Split(OSseparator).ToList().GetRange(0, _output.Split(OSseparator).Length - 1))))
                {
                    resultsDirectory = _output;
                }
                else
                {
                    exit("wrong path provided");
                }
            }

            if (Directory.Exists(@"" + resultsDirectory))
            {
                if (((resultsDirectory == _input) && ((mode == "tfNet") || (mode == "peaks"))) || 
                    ((resultsDirectory == string.Join(Convert.ToString(OSseparator), _input.Split(OSseparator).ToList().GetRange(0, _input.Split(OSseparator).Length - 1))) &&
                    ((mode == "regions") || (mode == "filter") || (mode == "network"))))
                {
                    exit("you are not allowed to read and write from the same directory");
                }
                
                ConsoleKeyInfo cki = new ConsoleKeyInfo();
                do
                {
                    Console.Write("WARNING: the output directory already exists. Delete? [Y/n] ");
                    cki = Console.ReadKey();
                } while (cki.Key != ConsoleKey.Y && cki.Key != ConsoleKey.N);

                Console.Write("\n");

                if (cki.Key == ConsoleKey.Y)
                {
                    try
                    {
                        Directory.Delete(@"" + resultsDirectory, true);
                    }
                    catch (Exception)
                    {
                        exit("the chosen results directory could not be deleted");
                    }
                }
                else
                {
                    dlt = false;
                    exit("you chose not to delete an existing directory");
                }
            }
            try
            {
                Directory.CreateDirectory(@"" + resultsDirectory);
            }
            catch (Exception)
            {
                exit("the chosen results directory could not be created");
            }
            resDir = resultsDirectory;
            return resultsDirectory;
        }

        public double checkMemoryPercentage(double _memPrcnt)
        {
            if (_memPrcnt < 0 || _memPrcnt > 100)
            {
                exit("parameter --mem must be between 0 and 100");
            }
            return _memPrcnt;
        }

        public int checkNumberOfColumns(int _numOfCols)
        {
            if (_numOfCols < 0)
            {
                exit("parameter --cols must be positive");
            }
            if (_numOfCols < 3 || _numOfCols > 10)
            {
                exit("parameter --cols must be between 3 and 10");
            }
            nCols = _numOfCols;
            return _numOfCols;
        }

        public List<string> checkTfList(string _getTfList)
        {
            List<string> tfList = new List<string>();
            if (_getTfList != null)
            {
                if (File.Exists(@"" + _getTfList))
                {
                    using (TextReader input = new StreamReader(@"" + _getTfList))
                    {
                        tfList = input.ReadLine().Split(';').Where(x => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrEmpty(x)).Distinct().ToList();
                        Console.WriteLine("Loaded " + tfList.Count + " TF names");
                    }
                }
                else
                {
                    exit("the --tfList file was not found");
                }
            }
            else
            {
                #region list of TFs **VERY LONG
                tfList = new List<string>()
				{
					"IRF4",
					"MEF2",
					"REL",
					"CEBPA",
					"ESRRG",
					"HOXB7",
					"HOXB3",
					"NR2E1",
					"GATA",
					"HOXA2",
					"PITX1",
					"PITX3",
					"NR4A",
					"RUNX2",
					"RUNX1",
					"MYB",
					"EN1",
					"NFY",
					"BHLHE22",
					"ATF3",
					"BHLHE23",
					"OLIG1",
					"TFE",
					"HOXD13",
					"GLI2",
					"IRF",
					"SOX8",
					"AP1",
					"E2F",
					"NFIC",
					"NFKB",
					"ERG",
					"FLI1",
					"SMAD3",
					"RAR",
					"HNF4",
					"PBX3",
					"ZEB1",
					"T",
					"CTCF",
					"SRF",
					"HIC1",
					"POU4F1",
					"PBX1",
					"HOXA1",
					"SOX5",
					"LHX6",
					"LMX1B",
					"SOX21",
					"SOX7",
					"ELK3",
					"NKX2-8",
					"MYC",
					"CREB3L2",
					"TFAP2",
					"NFE2",
					"NR2F6",
					"PRDM1",
					"FOXA",
					"TCF7",
					"EP300",
					"MYF6",
					"SPDEF",
					"ETS",
					"TCF7L2",
					"RBPJ",
					"SOX4",
					"RUNX",
					"STAT",
					"RFX5",
					"YY2",
					"PAX2",
					"KLF13",
					"MTF1",
					"REST",
					"ZIC3",
					"PATZ1",
					"ZIC4",
					"ESRRA",
					"ZNF524",
					"AHR",
					"NANOG",
					"TCF3",
					"TAL1",
					"TP73",
					"ZFX",
					"SP1",
					"RXRA",
					"ELF1",
					"NRF1",
					"ARNT",
					"EBF1",
					"TP53",
					"MYCN",
					"HES7",
					"TBX19",
					"NKX2-1",
					"THRB",
					"TGIF1",
					"VDR",
					"ZNF281",
					"NR2C2",
					"POU2F2",
					"SP8",
					"GTF2I",
					"ZNF263",
					"MZF1",
					"SPZ1",
					"MXI1",
					"TCF12",
					"TOPORS",
					"NFE2L2",
					"NR3C1",
					"HAND1",
					"THAP1",
					"ZNF423",
					"TATA",
					"ZNF589",
					"ZBTB33",
					"TFAP2B",
					"SREBP",
					"ZBTB14",
					"SMC3",
					"PTEN",
					"CACD",
					"EGR4",
					"SPI1",
					"SP4",
					"SP2",
					"ZBTB7A",
					"EGR1",
					"TFCP2",
					"ZIC1",
					"RARA",
					"PRDM4",
					"FEV",
					"CRX",
					"CHD2",
					"MYOD1",
					"ASCL2",
					"TFAP4",
					"TBX5",
					"GLIS3",
					"RFX2",
					"SETDB1",
					"ZNF219",
					"ZNF143",
					"ZBTB7C",
					"SIN3A",
					"PAX5",
					"TRIM28",
					"EOMES",
					"SOX17",
					"TBX20",
					"KLF16",
					"PAX1",
					"BHLHE40",
					"HES1",
					"BCL",
					"TGIF2",
					"PAX4",
					"CTCFL",
					"PLAG1",
					"SOX9",
					"ETV7",
					"EHF",
					"JDP2",
					"CREB5",
					"RAD21",
					"MESP1",
					"RFX3",
					"RUNX3",
					"RORA",
					"ELF5",
					"NFATC1",
					"BATF",
					"MEIS2",
					"HSF",
					"CDX2",
					"RARG",
					"FOXG1",
					"GBX2",
					"KLF12",
					"COMP1",
					"SEF1",
					"POU3F2",
					"TBR1",
					"HF1H3B",
					"PAX8",
					"SPIC",
					"MAF",
					"FOXO1",
					"ZNF628",
					"ETV6",
					"SOX10",
					"GRHL1",
					"SOX15",
					"NKX3-2",
					"HOXA11",
					"TBX2",
					"RARB",
					"SIRT6",
					"ZNF282",
					"YY1",
					"KLF7",
					"RREB1",
					"ZNF740",
					"E2F8",
					"CACBP",
					"CCNT2",
					"ZIC2",
					"MNT",
					"NKX2-3",
					"FOXJ1",
					"SRY",
					"FOXP1",
					"HDAC2",
					"FOXO3",
					"TCF4",
					"PLAGL1",
					"HEY1",
					"ITGB2",
					"FOXO4",
					"SOX2",
					"DMRT1",
					"NKX2-2",
					"NRL",
					"DLX5",
					"ATF4",
					"BDP1",
					"KLF14",
					"ZNF410",
					"NR5A2",
					"BRCA1",
					"GCM1",
					"FOXF1",
					"SCRT2",
					"CEBPB",
					"TBX21",
					"LHX4",
					"RXRG",
					"NFIA",
					"MGA",
					"ID4",
					"KLF4",
					"TBX1",
					"NHLH1",
					"MAFF",
					"HIC2",
					"TGIF2LX",
					"NR1H",
					"TFAP2E",
					"BCL6B",
					"GLI",
					"ZKSCAN3",
					"TFCP2L1",
					"THRA",
					"PHOX2A",
					"GFI1B",
					"FOXQ1",
					"POU5F1",
					"FOXI1",
					"CUX1",
					"ESRRB",
					"INSM1",
					"IKZF3",
					"HSF2",
					"NKX2-5",
					"PTF1A",
					"CREB3L1",
					"PKNOX2",
					"ZSCAN4",
					"RXRB",
					"SNAI2",
					"ZBTB49",
					"FOXO6",
					"ZBTB6",
					"GCM",
					"SOX11",
					"TEAD1",
					"GLIS2",
					"TCF21",
					"DPRX",
					"BHLHE41",
					"SMARC",
					"OTX1",
					"ZNF35",
					"SOX12",
					"OBOX5",
					"ISL2",
					"POU3F1",
					"MAFG",
					"FOXP3",
					"HNF1",
					"FOXK1",
					"FOXJ3",
					"HOXA13",
					"SIX5",
					"HOXA5",
					"GSX2",
					"FOXD3",
					"HOXA9",
					"E2F7",
					"ZNF652",
					"POU6F1",
					"LHX2",
					"ARID5B",
					"POU1F1",
					"PDX1",
					"DUX4",
					"MEOX2",
					"POU6F2",
					"ALX3",
					"ALX1",
					"GSX1",
					"ISX",
					"FOXJ2",
					"OBOX2",
					"POU3F3",
					"NKX6-2",
					"PRRX1",
					"PRRX2",
					"SHOX2",
					"UNCX",
					"PPARA",
					"SCRT1",
					"SOX18",
					"SOX1",
					"FOXB1",
					"HOXA6",
					"HOXD9",
					"VAX1",
					"HNF1B",
					"HOXA10",
					"ARID5A",
					"CDX1",
					"ESX1",
					"PROP1",
					"POU3F4",
					"HMX1",
					"NKX6-1",
					"CDC5L",
					"HOXC9",
					"IRX4",
					"HOXD11",
					"HOXC10",
					"HOXB9",
					"POU4F2",
					"DMRT2",
					"DBX2",
					"HOXD10",
					"BACH1",
					"SOX14",
					"FOXL1",
					"HOXB13",
					"ONECUT1",
					"FOXC1",
					"HOXD12",
					"FOXD2",
					"NFAT",
					"NKX3-1",
					"MSX2",
					"HMX2",
					"HMX3",
					"HEY2",
					"ELF3",
					"NR2F2",
					"ZNF350",
					"NKX2-4",
					"SMAD",
					"TEAD4",
					"NKX2-6",
					"MEIS3",
					"NR5A1",
					"HIF1A",
					"NR6A1",
					"ETV4",
					"ZNF691",
					"OSR1",
					"LMO2",
					"ZNF784",
					"ZNF75A",
					"OTX2",
					"HESX1",
					"ZSCAN16",
					"HMGN3",
					"VAX2",
					"PKNOX1",
					"GSC2",
					"ESR2",
					"GSC",
					"HERPUD1",
					"SOX3",
					"CREB3",
					"DLX3",
					"IRX3",
					"GFI1",
					"TFEB",
					"AIRE",
					"TEAD3",
					"CPHX",
					"OLIG2",
					"BACH2",
					"NFIB",
					"ELF4",
					"NEUROD2",
					"EGR3",
					"GLIS1",
					"TFE3",
					"SOX",
					"ZBTB18",
					"TEF",
					"VSX1",
					"IRX5",
					"PAX9",
					"ATF6",
					"HINFP",
					"LHX8",
					"NR1H4",
					"CEBPE",
					"MSX1",
					"PAX6",
					"SPIB",
					"RHOXF1",
					"EMX2",
					"LBX2",
					"BARHL2",
					"HNF1A",
					"IRX2",
					"VENTX",
					"NKX1-1",
					"FIGLA",
					"DMRTA2",
					"FOXN1",
					"BBX",
					"MEF2D",
					"ATOH1",
					"NOTO",
					"XBP1",
					"LHX5",
					"DBP",
					"WT1",
					"IKZF2",
					"NFE2L1",
					"FOXD1",
					"HOXD8",
					"CEBPD",
					"BPTF",
					"HOXC12",
					"ETV2",
					"HDX",
					"MEIS1",
					"OBOX6",
					"ZBTB16",
					"IKZF1",
					"HOXC13",
					"DLX2",
					"DLX4",
					"TBX4",
					"TBX15",
					"ELK4",
					"ARNTL",
					"PITX2",
					"FOXF2",
					"NR3C2",
					"RFX7",
					"LHX3",
					"HLX",
					"NOBOX",
					"BSX",
					"FOXC2",
					"DLX6",
					"EN2",
					"OBOX3",
					"HOXB8",
					"TLX2",
					"ARX",
					"BARHL1",
					"HLF",
					"MEF2B",
					"RAX",
					"DOBOX4",
					"ZBTB12",
					"GBX1",
					"OLIG3",
					"TCF7L1",
					"HOXB6",
					"HOXC6",
					"AFP",
					"ZNF713",
					"SOX30",
					"MEOX1",
					"NEUROG2",
					"ELF2",
					"E2F2",
					"SMAD4",
					"HMGA1",
					"ALX4",
					"POU2F3",
					"ZNF384",
					"HOXB5",
					"NKX6-3",
					"TFEC",
					"OSR2",
					"MYBL1",
					"DOBOX5",
					"HOXA3",
					"HMBOX1",
					"HBP1",
					"NFAT5",
					"TP63",
					"BHLHA15",
					"MNX1",
					"FOX",
					"SOX13",
					"ARID3A",
					"HOXC4",
					"LHX1",
					"LMX1A",
					"MIXL1",
					"PHOX2B",
					"BARX2",
					"RHOXF2",
					"LHX9",
					"BARX1",
					"NKX1-2",
					"DLX1",
					"HOXC8",
					"PAX7",
					"POU4F3",
					"HOMEZ",
					"DMRTC2",
					"EVX1",
					"HOXC11",
					"HOXD3",
					"HSF4",
					"ONECUT3",
					"VSX2",
					"OBOX1",
					"EMX1",
					"CEBPG",
					"CBX5",
					"OTX",
					"OTP",
					"DRGX",
					"ZNF232",
					"EVX2",
					"DUXA",
					"DMRT3",
					"FOXM1",
					"CDX",
					"HOXB2",
					"DMRTA1",
					"PAX3",
					"CCDC6",
					"IRX6",
					"PROX1",
					"AP3",
					"GMEB2",
					"SP100",
					"HES5",
					"ETV5",
					"DBX1",
					"CPEB1",
					"HOXC5",
					"ONECUT2",
					"HOXA7",
					"HOXA4",
					"HOXD1",
					"NFIL3",
					"HOXB4",
					"GCM2",
					"HLTF",
					"DMBX1",
					"ZSCAN26",
					"MYBL2",
					"MLX",
					"ETV3",
					"ATF2",
					"GTF2A",
					"IRC900814",
					"ATF7",
					"ERF",
					"GZF1",
					"IRF6",
					"CLOCK",
					"E4F1",
					"HSFY2",
					"ZNF8",
					"ZBED1",
					"ETV1",
					"GMEB1",
					"CENPB",
					"SIN3AK20",
					"SIN3A",
					"EBF",
					"EBF1",
					"NFKAPPAB",
					"NFKB",
					"NRSF",
					"REST",
					"ERALPHAA",
					"ESRRA",
					"BAF155",
					"SMARC",
					"PU.1",
					"SPI1",
					"CFOS",
					"CHD1A",
					"ELK",
					"EZH",
					"GABP",
					"JUND",
					"MAX",
					"MAZ",
					"MTA3",
					"P300",
					"PML",
					"POL2",
					"POL24H8",
					"POL2S2",
					"POL3",
					"PU1",
					"TAF1",
					"TBLR1",
					"TBP",
					"TR4",
					"USF1",
					"USF2",
					"WHIP",
					"ZNF274",
					"ZZZ3"
				}.Distinct().OrderByDescending(x => x.Length).ThenBy(x => x).ToList();
                #endregion
            }
            return tfList;
        }

        public string setOutFileName(string _resultsDirectory)
        {
            return _resultsDirectory.Split(OSseparator).Last();
        }

        public string setFileType(int _numOfCols)
        {
			if(_numOfCols < 3)
			{
				exit ("number of columns cannot be less than 3");
				return "";
			}
			else if (_numOfCols <= 8)
            {
                return "bed";
            }
            else if (_numOfCols == 9)
            {
                return "broadPeak";
            }
            else if (_numOfCols == 10)
            {
                return "narrowPeak";
            }
            else
            {
                exit("something went wrong with the file type");
                return null;
            }
        }

        public Dictionary<string, int> checkChromInfo(string _chromInfo)
        {
            if (!string.IsNullOrEmpty(_chromInfo))
            {
                if (!File.Exists(@"" + _chromInfo))
                {
                    exit("the chromInfo file does not exist");
                }
                else
                {
                    chromNamAndLen = getChromosomeNameAndLength(_chromInfo);
                }
            }
            else
            {
                chromNamAndLen = getChromosomeNameAndLength(null);
            }
            return chromNamAndLen;
        }

        public bool checkIgnoreChrInfo(bool _ignChrInf)
        {
            ignoChrLen = _ignChrInf;
            return _ignChrInf;
        }

        private Dictionary<string, int> getChromosomeNameAndLength(string chromInfo)
        {
            if (chromInfo == null)
            {
                return new Dictionary<string, int>()
                { 
                    { "chr1", 249250621 },
		            { "chr2", 243199373 },
		            { "chr3", 198022430 },
		            { "chr4", 191154276 },
		            { "chr5", 180915260 },
		            { "chr6", 171115067 },
		            { "chr7", 159138663 },
		            { "chrX", 155270560 },
		            { "chr8", 146364022 },
		            { "chr9", 141213431 },
		            { "chr10", 135534747 },
		            { "chr11", 135006516 },
		            { "chr12", 133851895 },
		            { "chr13", 115169878 },
		            { "chr14", 107349540 },
		            { "chr15", 102531392 },
		            { "chr16", 90354753 },
		            { "chr17", 81195210 },
		            { "chr18", 78077248 },
		            { "chr20", 63025520 },
		            { "chrY", 59373566 },
		            { "chr19", 59128983 },
		            { "chr22", 51304566 },
		            { "chr21", 48129895 },
		            { "chr6_ssto_hap7", 4928567 },
		            { "chr6_mcf_hap5", 4833398 },
		            { "chr6_cox_hap2", 4795371 },
		            { "chr6_mann_hap4", 4683263 },
		            { "chr6_apd_hap1", 4622290 },
		            { "chr6_qbl_hap6", 4611984 },
		            { "chr6_dbb_hap3", 4610396 },
		            { "chr17_ctg5_hap1", 1680828 },
		            { "chr4_ctg9_hap1", 590426 },
		            { "chr1_gl000192_random", 547496 },
		            { "chrUn_gl000225", 211173 },
		            { "chr4_gl000194_random", 191469 },
		            { "chr4_gl000193_random", 189789 },
		            { "chr9_gl000200_random", 187035 },
		            { "chrUn_gl000222", 186861 },
		            { "chrUn_gl000212", 186858 },
		            { "chr7_gl000195_random", 182896 },
		            { "chrUn_gl000223", 180455 },
		            { "chrUn_gl000224", 179693 },
		            { "chrUn_gl000219", 179198 },
		            { "chr17_gl000205_random", 174588 },
		            { "chrUn_gl000215", 172545 },
		            { "chrUn_gl000216", 172294 },
		            { "chrUn_gl000217", 172149 },
		            { "chr9_gl000199_random", 169874 },
		            { "chrUn_gl000211", 166566 },
		            { "chrUn_gl000213", 164239 },
		            { "chrUn_gl000220", 161802 },
		            { "chrUn_gl000218", 161147 },
		            { "chr19_gl000209_random", 159169 },
		            { "chrUn_gl000221", 155397 },
		            { "chrUn_gl000214", 137718 },
		            { "chrUn_gl000228", 129120 },
		            { "chrUn_gl000227", 128374 },
		            { "chr1_gl000191_random", 106433 },
		            { "chr19_gl000208_random", 92689 },
		            { "chr9_gl000198_random", 90085 },
		            { "chr17_gl000204_random", 81310 },
		            { "chrUn_gl000233", 45941 },
		            { "chrUn_gl000237", 45867 },
		            { "chrUn_gl000230", 43691 },
		            { "chrUn_gl000242", 43523 },
		            { "chrUn_gl000243", 43341 },
		            { "chrUn_gl000241", 42152 },
		            { "chrUn_gl000236", 41934 },
		            { "chrUn_gl000240", 41933 },
		            { "chr17_gl000206_random", 41001 },
		            { "chrUn_gl000232", 40652 },
		            { "chrUn_gl000234", 40531 },
		            { "chr11_gl000202_random", 40103 },
		            { "chrUn_gl000238", 39939 },
		            { "chrUn_gl000244", 39929 },
		            { "chrUn_gl000248", 39786 },
		            { "chr8_gl000196_random", 38914 },
		            { "chrUn_gl000249", 38502 },
		            { "chrUn_gl000246", 38154 },
		            { "chr17_gl000203_random", 37498 },
		            { "chr8_gl000197_random", 37175 },
		            { "chrUn_gl000245", 36651 },
		            { "chrUn_gl000247", 36422 },
		            { "chr9_gl000201_random", 36148 },
		            { "chrUn_gl000235", 34474 },
		            { "chrUn_gl000239", 33824 },
		            { "chr21_gl000210_random", 27682 },
		            { "chrUn_gl000231", 27386 },
		            { "chrUn_gl000229", 19913 },
		            { "chrM", 16571 },
		            { "chrUn_gl000226", 15008 },
		            { "chr18_gl000207_random", 4262 }
                };
            }
            else
            {
                using (TextReader input = new StreamReader(@"" + chromInfo))
                {
                    string line = input.ReadLine();
                    if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                        exit("the --chromInfo file provided is empty");
                    return line.Split(';').Where(x => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrEmpty(x)).ToDictionary(x => x.Split(',').First(), x => Convert.ToInt32(x.Split(',').Last()));
                }
            }
        }
        #endregion

		#region peakAndregionInCommon
        public bool checkPeakNoarrowing(bool _narrowThePeak)
		{
			return !_narrowThePeak;
		}

        public int checkSummitWindow(int _summitWindow)
		{
			if (_summitWindow < 0)
			{
				exit("parameter --win must be positive");
			}
			else if (_summitWindow > 10000) {
				exit("parameter --win must be lower than 10000");
			}
			return _summitWindow;
		}

        public bool checkRegionPvalues(bool _regionPvalue)
        {
            return !_regionPvalue;
        }

        public bool checkMinusLogTen(bool _minusLogTen)
        {
            return !_minusLogTen;
        }
		#endregion

		#region filterAndRegionInCommon
        public bool checkStrandSpecificity(bool _strandSpecificity)
		{
			return _strandSpecificity;
		}

        public bool checkStatFile(bool _statFile)
		{
			return !_statFile;
		}

        public bool checkXmlFile(bool _xmlFile)
		{
			return !_xmlFile;
		}

        public bool checkPeakFile(bool _peakFile)
		{
			return !_peakFile;
		}

        public bool checkLowMemory(bool _lowMemory)
		{
			return _lowMemory;
		}

        public int checkTopX(int _topX)
        {
            if (_topX < 0)
            {
                exit("--topX has to be positive");
            }
            return _topX;
        }

        public double checkTopXP(int _topXP)
        {
            if (_topXP < 0.0)
            {
                exit("--topXP has to be positive");
            }
			else if (_topXP > 100.0) {
				exit ("--topX{ has to be less than 100");
			}
            return _topXP;
        }

        public bool? checkSortRegions(char _sort)
        {
            if (_sort == 'A' || _sort == 'a')
            {
                return true;
            }
            else if (_sort == 'D' || _sort == 'd')
            {
                return false;
            }
            else if (_sort == 'N' || _sort == 'n')
            {
                return null;
            }
            else
            {
                exit("invalid parameter for --sort");
            }
            return null;
        }
		#endregion

		#region peaks
        public bool checkNoValueAssigned(bool _noValueAssigned)
		{
			return !_noValueAssigned;
		}

        public bool checkUnknownSummit(bool _unknownSummit)
		{
			return !_unknownSummit;
		}

        public int checkFilterByScore(int _filterByScore)
		{
			if (_filterByScore == -1) {
				return _filterByScore;
			} else if (_filterByScore < 0)
			{
				exit("parameter --score must be positive");
			}
			return _filterByScore;
		}

        public int checkFilterBySV(int _filterBySV)
		{
			if (_filterBySV == -1) {
				return _filterBySV;
			} else if (_filterBySV < 0)
			{
				exit("parameter --sValue must be positive");
			}
			return _filterBySV;
		}

        public double checkFilterByPvalue(double _filterByPvalue)
		{
			    if (_filterByPvalue == -1.0) {
				return _filterByPvalue;
			} else if (_filterByPvalue < 0.0 || _filterByPvalue > 1.0)
			{
				exit("parameter --pValue must be between 0 and 1");
			}
			return _filterByPvalue;
		}

        public double checkFilterByQvalue(double _filterByQvalue)
		{
			if (_filterByQvalue == -1.0) {
				return _filterByQvalue;
			} else if (_filterByQvalue < 0.0 || _filterByQvalue > 1.0)
			{
				exit("parameter --qValue must be between 0 and 1");
			}
			return _filterByQvalue;
		}

        public int checkSortPeaks(char _sort)
        {
            if (_sort == 'S' || _sort == 's')
            {
                return 1;
            }
            else if (_sort == 'M' || _sort == 'm')
            {
                return 2;
            }
            else if (_sort == 'N' || _sort == 'n')
            {
                return 3;
            }
            else if (_sort == 'P' || _sort == 'p')
            {
                return 4;
            }
            else
            {
                exit("invalid parameter for --sort");
            }
            return -1;
        }

        public List<string> checkAcceptedFileExtensions(string _accFilExt)
        {
            if (_accFilExt == null)
            {
                return new List<string>() { "narrowPeak" };
            }
            else
            {
                return _accFilExt.Split(',').Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)).ToList();
            };
        }

        public bool checkFileNameAsTfName(bool _fNameAsTfName)
        {
            return _fNameAsTfName;
        }
		#endregion

		#region regions
        public bool checkPeakDistanceOption(bool _distanceOption)
		{
			return !_distanceOption;
		}

        public int checkPeakDistance(int _peakDistance, string _chromInfo)
		{
			if (_peakDistance < 0)
			{
				exit("parameter --distance must be positive");
			} else if (_peakDistance > checkChromInfo(_chromInfo).Select(x => x.Value).Max()) {
				exit("parameter --distance must be less than 10000");
			}
			return _peakDistance;
		}

        public double checkCutoffValue(double _cutoffValue)
        {
            if (_cutoffValue < 0 || _cutoffValue > 1)
            {
                exit("the cutoff option cannot be greater than 1 or smaller than 0");
            }
            return _cutoffValue;
        }
		#endregion

		#region filter
        public List<string> checkChromosome(string _chromosome)
		{
            if (_chromosome == null)
            {
                return null;
            }

            if (!_chromosome.Split(',').Where(x => !string.IsNullOrWhiteSpace(x) && !ignoChrLen && !string.IsNullOrEmpty(x)).All(x => chromNamAndLen.ContainsKey(x)))
            {
                exit("wrong --chr parameter");
            }

            return _chromosome.Split(',').Where(x => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrEmpty(x)).ToList();
		}

        public List<string> checkRegName(string _regName, string _regNameFile)
		{
            if (_regName == null && _regNameFile == null)
            {
                return null;
            }
            else if (_regName == null)
            {
                if (File.Exists(@"" + _regNameFile))
                {
                    using (TextReader input = new StreamReader(@"" + _regNameFile))
                    {
                        string line = input.ReadLine();
                        if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                        {
                            exit("--regFile file is empty");
                        }
                        if (line.Split(',').Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)).Where(x => x.Substring(0, 3) != "reg").Count() > 0)
                        {
                            exit("wrong --regFile parameter");
                        }
                        return line.Split(',').Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                    }
                }
            }
            else
            {
                if (_regName.Split(',').Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)).Where(x => x.Substring(0, 3) != "reg").Count() > 0)
                {
                    exit("wrong --reg parameter");
                }
                return _regName.Split(',').Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            }
            return null;
		}

        public List<string> checkTfNames(string _tfName)
		{
            if (_tfName == null)
            {
                return null;
            }
            return _tfName.Split(',').Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)).ToList();
		}

        public List<string> checkPkNames(string _pkName)
		{
            if (_pkName == null)
            {
                return null;
            }
            return _pkName.Split(',').Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)).ToList();
		}

        public int checkStart(int _start)
		{
			if (_start < 0) {
				exit("--start has to be positive");
			}
			return _start;
		}

        public int checkEnd(int _end)
		{
			if (_end < 0)
            {
				exit("--end has to be positive");
			}
			return _end;
		}

        public int checkLScore(int _lScore)
		{
			if (_lScore < 0) {
				exit("--lScore has to be positive");
			}
			return _lScore;
		}

        public int checkHScore(int _hScore)
		{
			if (_hScore < 0) {
				exit("--hScore has to be positive");
			}
			return _hScore;
		}
		#endregion

        #region network
        public char checkFilterOption(char _filterOption)
        {
            if (_filterOption == 'b')
            {
                return 'b';
            }
            else if (_filterOption == 'p')
            {
                return 'p';
            }
            else if (_filterOption == 'q')
            {
                return 'q';
            }
            else
            {
                exit("--fopt has to be b, p or q");
            }
            return _filterOption;
        }

        public double checkFIlterValue(double _filterValue)
        {
            if (_filterValue < 0 || _filterValue > 1)
            {
                exit("--fval has to be between 0 and 1");
            }
            return _filterValue;
        }

        public int checkNeighboringDistance(string _neighborDistance)
        {
            if (_neighborDistance.Split(',').Length != 2)
            {
                exit("--neigh has to be of the format: lowValue,highValue");
            }

            int tmp;
            if (!int.TryParse(_neighborDistance.Split(',').Last(), out tmp))
            {
                exit("--neigh has to be of the format: lowValue,highValue. Integer expected as highValue");
            }
            if (tmp < 0)
            {
                exit("--neigh has to be of the format: lowValue,highValue. Positive expected as highValue");
            }
            highNeigh = tmp;

            if (!int.TryParse(_neighborDistance.Split(',').First(), out tmp))
            {
                exit("--neigh has to be of the format: lowValue,highValue. Integer expected as lowValue");
            }
            if (tmp < 0)
            {
                exit("--neigh has to be of the format: lowValue,highValue. Positive expected as lowValue");
            }
            return tmp;
        }

        public int checkOverlappingDistance(int _ovelapDistance)
        {
            if (_ovelapDistance < 0)
            {
                exit("--overlap has to be positive");
            }
            return _ovelapDistance;
        }

        public string checkTitle(string _title)
        {
            return _title;
        }

        public bool checkForNoRscript(bool _noRscript)
        {
            noR = _noRscript;
            return _noRscript;
        }

        public string checkForRscript(string Rscript)
        {
            if (!noR)
            {
                if (Rscript == null || Rscript == "")
                {
                    if (OSseparator == '\\')
                    {
                        string values = Environment.GetEnvironmentVariable("PATH");
                        foreach (string path in values.Split(';'))
                        {
                            string fullPath = path + OSseparator + "Rscript.exe";
                            if (File.Exists(@"" + fullPath))
                                return fullPath;
                        }
                        exit("the Rscript binary does not seem to be installed in your system");
                    }
                    else
                    {
                        try
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.FileName = "Rscript";
                            startInfo.Arguments = "";
                            startInfo.RedirectStandardOutput = true;
                            startInfo.RedirectStandardInput = true;
                            startInfo.RedirectStandardError = true;
                            startInfo.UseShellExecute = false;
                            startInfo.CreateNoWindow = true;

                            Process newProcess = new Process();
                            newProcess.StartInfo = startInfo;
                            newProcess.Start();
                            newProcess.WaitForExit();
                        }
                        catch (Exception)
                        {
                            exit("the Rscript binary does not seem to be installed in your system");
                        }
                    }
                    return Rscript;
                }
                else
                {
                    if (File.Exists(@"" + Rscript))
                    {
                        return Rscript;
                    }
                    else if (File.Exists(@"" + Rscript + OSseparator + "Rscript"))
                    {
                        return Rscript + OSseparator + "Rscript";
                    }
                    else
                    {
                        exit("the Rscript binary could not be located in the given path");
                    }
                    return Rscript;
                }
            }
            return Rscript;
        }

        public string checkForScriptPath(string _scriptPath)
        {
            if (!noR)
            {
                if (File.Exists(@"" + _scriptPath))
                {
                    return _scriptPath;
                }
                else if (File.Exists(@"" + _scriptPath + OSseparator + "tfNet.R"))
                {
                    return _scriptPath + OSseparator + "tfNet.R";
                }
                else if (File.Exists(@"" + Directory.GetCurrentDirectory() + OSseparator + "tfNet.R"))
                {
                    return Directory.GetCurrentDirectory() + OSseparator + "tfNet.R";
                }
                {
                    exit("the network R script was not found in the given path");
                }
            }
            return _scriptPath;
        }
        #endregion
    }
}
