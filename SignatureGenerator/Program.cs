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
            Console.WriteLine("Calculating signature");
            var sw = new Stopwatch();
            sw.Start();

            var prod = new Thread(Produce);
            prod.Start();

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
            //Console.ReadKey();
        }

        public static void Produce()
        {
            var fileName = @"d:\Video\durak_2014.avi";
            var counter = 1;
            
            foreach (var block in FileHelper.ReadFile(fileName))
            {
                _workingQueue.Enqueue(new QueueWorkItem(counter, block));
                counter++;
            }
            
            _resetEvent.Set();
            Console.WriteLine("Queue filled by {0} elements", counter);
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