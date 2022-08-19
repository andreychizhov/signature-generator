using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SignatureGenerator
{
    internal class Program
    {
        private static readonly ConcurrentQueue<QueueWorkItem> _workingQueue = new ConcurrentQueue<QueueWorkItem>();
        private static readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim(false);
        private static readonly SpinWait _spinWait = new SpinWait();
        private static readonly CountdownEvent _countdownEvent = new CountdownEvent(Environment.ProcessorCount);
        
        public static void Main(string[] args)
        {
            var paramValidator = new InputParametersValidator();
            if (!paramValidator.TryGetConfiguration(args, out var param))
            {
                return;
            }

            Console.WriteLine("Calculating signature");
            var sw = new Stopwatch();
            sw.Start();

            var prod = new Thread(Produce);
            prod.Start(param);

            var consumersCount = Environment.ProcessorCount;
            var consumerPool = new List<Thread>(consumersCount);
            for (var i = 0; i < consumersCount; i++)
            {
                consumerPool.Add(new Thread(Consume));
            }

            foreach (var thread in consumerPool)
            {
                thread.Start();
            }

            prod.Join();
            _countdownEvent.Wait();

            sw.Stop();
            Console.WriteLine("Total time: {0}", sw.Elapsed);
        }

        public static void Produce(object data)
        {
            if (data is CommandLineParameters p)
            {
                var counter = 1;
            
                foreach (var block in FileHelper.ReadFileByBlock(p.FilePath, p.BlockSize))
                {
                    _workingQueue.Enqueue(new QueueWorkItem(counter, block));
                    counter++;
                }
            }
            else
            {
                throw new ArgumentException("Invalid parameter data", nameof(data));
            }
            
            _resetEvent.Set();
        }

        public static void Consume()
        {
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
                    _spinWait.SpinOnce();
                }
            }

            _countdownEvent.Signal();
        }
    }
}