using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SignatureGenerator
{
    public class ConfigurationProvider
    {
        public bool TryGetFromStandardInput(string[] args, out Configuration parameters)
        {
            var config = new Configuration();
            if (args.Length == 0)
            {
                Console.WriteLine(@"Usage: please specify path to file and block size (1 MiB is a default)

--file, -f - path to file
--block-size, -b size of block for calculating hash");
                parameters = config;
                return false;
            }

            if (!args.Contains("--file", StringComparer.OrdinalIgnoreCase) &&
                !args.Contains("-f", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine("Path to file required, please specify --file or -f parameter");
                parameters = config;
                return false;
            }

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--file", StringComparison.OrdinalIgnoreCase) ||
                    args[i].Equals("-f", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsValidFilename(args[i + 1]))
                    {
                        config.FilePath = args[i + 1];
                    }
                    else
                    {
                        Console.WriteLine("Path to file is invalid, please specify --file or -f parameter");
                        parameters = config;
                        return false;
                    }
                }

                if (args[i].Equals("--block-size", StringComparison.OrdinalIgnoreCase) ||
                    args[i].Equals("-b", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args[i + 1], out var bs))
                    {
                        config.BlockSize = bs;
                    }
                    else
                    {
                        config.BlockSize = 1024 * 1024;
                        Console.WriteLine("Block size not specified or incorrect, the default size 1 MiB will we used");
                    }
                }
            }

            parameters = config;
            return true;
        }
        
        bool IsValidFilename(string fileName) {
            var regexString = "[" + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]";
            var regex = new Regex(regexString);
            if (regex.IsMatch(fileName)) {
                return false;
            }
            
            var pathRoot = Path.GetPathRoot(fileName);
            if (!Directory.GetLogicalDrives().Contains(pathRoot.ToUpper()))
            {
                return false;
            }
            
            FileInfo fi = null;
            try {
                fi = new FileInfo(fileName);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }
            if (ReferenceEquals(fi, null))
            {
                return false;
            }

            return true;
        }
    }
}