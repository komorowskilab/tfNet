using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tfNet
{
    public abstract class pk
    {
        public string chromosome { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public string name { get; set; }
        public double score { get; set; }
        public char strand { get; set; }
        public double signalValue { get; set; }
        public double pValue { get; set; }
        public double qValue { get; set; }
        public int summit { get; set; }
    }

    public class peak : pk
    {
        public string TFname { get; set; }
        public string peakName { get; set; }
        public int middle { get; set; }
        //public bool cutoff { get; set; }
    }

    public class region : pk
    {
        public string regionName { get; set; }
        public List<peak> peakList { get; set; }
    }
}
