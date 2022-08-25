using System;
using System.Collections.Concurrent;
using System.IO;

namespace SignatureGenerator
{
    public class Producer
    {
        private const int MaxBufferSize = Units.MiB;

        private readonly BlockingCollection<QueueWorkItem> _workingQueue;

        public Producer(BlockingCollection<QueueWorkItem> workingQueue)
        {
            _workingQueue = workingQueue;
        }

        public void Produce(object data)
        {
            if (data is Configuration p)
            {
                var counter = 1;

                var bufferSize = p.BlockSize > 0 && p.BlockSize < MaxBufferSize 
                    ? p.BlockSize : MaxBufferSize;
            
                var buffer = new byte[bufferSize];
                int bytesRead;
                using (var fs = File.Open(p.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var bs = new BufferedStream(fs))
                {

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
                        Console.WriteLine($"I/O Exception: {e.Message}\n\n{e.StackTrace}");
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine($"Access Exception: {e.Message}\n\n{e.StackTrace}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unknown Exception: {e.Message}\n\n{e.StackTrace}");
                    }
                    finally
                    {
                        _workingQueue.CompleteAdding();
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid configuration data");
            }
        }
    }
}