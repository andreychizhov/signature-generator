using System;
using System.Diagnostics;

namespace SignatureGenerator
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var configurationProvider = new ConfigurationProvider();
            using (var runner = new ProcessRunner())
            {
                if (!configurationProvider.TryGetFromStandardInput(args, out var config))
                {
                    return;
                }

                Console.WriteLine("Calculating signature...");
                var sw = new Stopwatch();
                sw.Start();

                runner.Run(config);

                sw.Stop();
                Console.WriteLine("Total time: {0}", sw.Elapsed);
            }
        }
    }
}