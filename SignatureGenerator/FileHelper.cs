using System;
using System.Collections.Generic;
using System.IO;

namespace SignatureGenerator
{
    public class FileHelper
    {
        public static IEnumerable<byte[]> ReadFile(string filePath)
        {
            const int MAX_BUFFER = 1024 * 1024; //1MB 
            byte[] buffer = new byte[MAX_BUFFER];
            int bytesRead;
            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bs = new BufferedStream(fs))
            {
                while ((bytesRead = bs.Read(buffer, 0, MAX_BUFFER)) != 0)
                {
                    var resultBuffer = new byte[MAX_BUFFER];
                    buffer.CopyTo(resultBuffer, 0);
                    
                    yield return resultBuffer;
                }
            }
        }
    }
}