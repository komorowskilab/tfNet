using System;
using System.Diagnostics;

namespace tfNet
{
    public class Program
    {
        public monitorMemory mMem { get; set; }
        public double memPrcntg { get; set; }
        static void Main(string[] args)
        {
            //args = new List<string> (){ "tfNet", "-n", "-i", "/Users/klev/Desktop/downloadedPeaks", "-o", "/Users/klev/Desktop/test1" }.ToArray ();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            string invokedVerb = "";
            object invokedVerbInstance = new object();

            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options,
              (verb, subOptions) =>
              {
                  // if parsing succeeds the verb name and correct instance
                  // will be passed to onVerbCommand delegate (string,object)
                  invokedVerb = verb;
                  invokedVerbInstance = subOptions;
              }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            validateArguments runMe = new run(invokedVerb, options);
            Console.WriteLine(watch.Elapsed);
        }
    }
}
