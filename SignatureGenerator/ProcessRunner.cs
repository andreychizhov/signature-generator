using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SignatureGenerator
{
    public class ProcessRunner : IDisposable
    {
        private static readonly int ConsumersCount = Environment.ProcessorCount;
        private const int MaxBufferSize = 1024 * 1024; //1MB 

        private readonly ConcurrentQueue<QueueWorkItem> _workingQueue;
        private readonly ManualResetEventSlim _resetEvent;
        private readonly CountdownEvent _countdownEvent;

        public ProcessRunner()
        {
            _workingQueue = new ConcurrentQueue<QueueWorkItem>();
            _resetEvent = new ManualResetEventSlim(false);
            _countdownEvent = new CountdownEvent(ConsumersCount);
        }

        public void Run(Configuration config)
        {
            var prod = new Thread(Produce);
            prod.Start(config);

            var consumerPool = new List<Thread>(ConsumersCount);
            for (var i = 0; i < ConsumersCount; i++)
            {
                consumerPool.Add(new Thread(Consume));
            }

            foreach (var thread in consumerPool)
            {
                thread.Start();
            }

            prod.Join();
            _countdownEvent.Wait();
        }

        private void Produce(object data)
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
                    
                            _workingQueue.Enqueue(new QueueWorkItem(counter, resultBuffer));
                            
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
                }
            }
            else
            {
                Console.WriteLine("Invalid configuration data");
            }
            
            _resetEvent.Set();
        }

        private void Consume()
        {
            var spinWait = new SpinWait();
            while (true)
            {
                var dequeueResult = _workingQueue.TryDequeue(out var item);

                if (dequeueResult)
                {
                    Console.WriteLine("Worker: {0,3} | Block: {1,5} | Hash: {2}", Thread.CurrentThread.ManagedThreadId,
                        item.BlockNumber, HashHelper.CalculateSha256(item.BlockData));
                }
                else if (_resetEvent.IsSet)
                {
                    break;
                }
                else
                {
                    spinWait.SpinOnce();
                }
            }

            _countdownEvent.Signal();
        }

        public void Dispose()
        {
            _resetEvent?.Dispose();
            _countdownEvent?.Dispose();
        }
    }
}