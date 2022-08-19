using System;
using System.Collections.Generic;
using System.IO;

namespace SignatureGenerator
{
    public class FileHelper
    {
        private const int MaxBufferSize = 1024 * 1024; //1MB 
        
        public static IEnumerable<byte[]> ReadFileByBlock(string filePath, int blockSize)
        {
            var bufferSize = blockSize > 0 && blockSize < MaxBufferSize 
                ? blockSize : MaxBufferSize;
            
            var buffer = new byte[bufferSize];
            int bytesRead;
            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bs = new BufferedStream(fs))
            {
                while ((bytesRead = bs.Read(buffer, 0, bufferSize)) != 0)
                {
                    var resultBuffer = new byte[bufferSize];
                    buffer.CopyTo(resultBuffer, 0);
                    
                    yield return resultBuffer;
                }
            }
        }
    }
}