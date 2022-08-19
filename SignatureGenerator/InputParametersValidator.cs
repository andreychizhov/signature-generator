namespace SignatureGenerator
{
    public class InputParametersValidator
    {
        public bool TryGetConfiguration(string[] args, out CommandLineParameters parameters)
        {
            var fileName = @"d:\Video\durak_2014.avi";
            const int MAX_BUFFER = 1024 * 1024;
            
            parameters = new CommandLineParameters(fileName, MAX_BUFFER);
            return true;
        }
}

    public readonly struct CommandLineParameters
    {
        public CommandLineParameters(string filePath, int blockSize)
        {
            FilePath = filePath;
            BlockSize = blockSize;
        }
        
        public readonly string FilePath;
        public readonly int BlockSize;
    }
}