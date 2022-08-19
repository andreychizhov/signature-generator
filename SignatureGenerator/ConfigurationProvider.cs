namespace SignatureGenerator
{
    public class ConfigurationProvider
    {
        public bool TryGetFromStandardInput(string[] args, out Configuration parameters)
        {
            var fileName = @"d:\Video\durak_2014.avi";
            const int MAX_BUFFER = 1024 * 1024;
            
            parameters = new Configuration(fileName, MAX_BUFFER);
            return true;
        }
    }

    public readonly struct Configuration
    {
        public Configuration(string filePath, int blockSize)
        {
            FilePath = filePath;
            BlockSize = blockSize;
        }
        
        public readonly string FilePath;
        public readonly int BlockSize;
    }
}