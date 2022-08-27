using System;
using System.Diagnostics;

namespace SignatureGenerator
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var configurationProvider = new ConfigurationProvider();
            if (!configurationProvider.TryGetFromStandardInput(args, out var config))
            {
                return;
            }
            
            using (var runner = new ProcessRunner(config, logger))
            {
                logger.Write("Calculating signature...");
                var sw = new Stopwatch();
                sw.Start();

                runner.Run();

                sw.Stop();
                logger.Write("Total time: {0}", sw.Elapsed);
            }
        }
    }
}