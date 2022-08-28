using System;
using System.Collections.Concurrent;
using System.IO;

namespace SignatureGenerator
{
    public class Producer
    {
        private const int MaxBufferSize = Units.MiB;

        private readonly BlockingCollection<QueueWorkItem> _workingQueue;
        private readonly Configuration _config;
        private readonly ILogger _logger;

        public Producer(BlockingCollection<QueueWorkItem> workingQueue, Configuration config, ILogger logger)
        {
            _workingQueue = workingQueue;
            _config = config;
            _logger = logger;
        }

        public void Produce()
        {
            var bufferSize = _config.BlockSize > 0 && _config.BlockSize < MaxBufferSize 
                ? _config.BlockSize : MaxBufferSize;
            
            using (var fs = File.Open(_config.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bs = new BufferedStream(fs))
            {
                var buffer = new byte[bufferSize];
                var counter = 1;
                int bytesRead;

                try
                {
                    while ((bytesRead = bs.Read(buffer, 0, bufferSize)) != 0)
                    {
                        var resultBuffer = new byte[bufferSize];
                        buffer.CopyTo(resultBuffer, 0);

                        _workingQueue.Add(new QueueWorkItem(counter, resultBuffer));

                        counter++;
                    }
                }
                catch (IOException e)
                {
                    _logger.Write($"I/O Exception: {e.Message}\n\n{e.StackTrace}");
                }
                catch (UnauthorizedAccessException e)
                {
                    _logger.Write($"Access Exception: {e.Message}\n\n{e.StackTrace}");
                }
                catch (Exception e)
                {
                    _logger.Write($"Unknown Exception: {e.Message}\n\n{e.StackTrace}");
                }
                finally
                {
                    _workingQueue.CompleteAdding();
                }
            }
        }
    }
}