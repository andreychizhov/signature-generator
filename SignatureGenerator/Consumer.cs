using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SignatureGenerator
{
    public class Consumer
    {
        private readonly BlockingCollection<QueueWorkItem> _workingQueue;

        public Consumer(BlockingCollection<QueueWorkItem> workingQueue)
        {
            _workingQueue = workingQueue;
        }

        public void Consume(object callback)
        {
            if (callback is Action c)
            {
                Consume(c);
            }
            else
            {
                throw new ArgumentException("Invalid callback type", nameof(callback));
            }
        }

        private void Consume(Action onCompletion)
        {
            var spinWait = new SpinWait();
            while (true)
            {
                var dequeueResult = _workingQueue.TryTake(out var item);

                if (dequeueResult)
                {
                    // Console.WriteLine("Worker: {0,3} | Block: {1,5} | Hash: {2}", Thread.CurrentThread.ManagedThreadId,
                    //     item.BlockNumber, HashHelper.CalculateSha256(item.BlockData));
                    Console.WriteLine("{0,5} : {1}",
                        item.BlockNumber, HashHelper.CalculateSha256(item.BlockData));
                }
                else if (_workingQueue.IsAddingCompleted)
                {
                    break;
                }
                else
                {
                    spinWait.SpinOnce();
                }
            }

            onCompletion();
        }

    }
}