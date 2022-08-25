using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SignatureGenerator
{
    public class ProcessRunner : IDisposable
    {
        private static readonly int ConsumersCount = Environment.ProcessorCount;
        private const int MaxBufferSize = Units.MiB;

        private readonly BlockingCollection<QueueWorkItem> _workingQueue;
        private readonly CountdownEvent _countdownEvent;

        private readonly Configuration _configuration;

        public ProcessRunner(Configuration config)
        {
            _configuration = config;

            var queueUpperLimit = MaxBufferSize * 200 / config.BlockSize;

            _workingQueue = new BlockingCollection<QueueWorkItem>(queueUpperLimit);
            _countdownEvent = new CountdownEvent(ConsumersCount);
        }

        public void Run()
        {
            var prod = new Thread(new Producer(_workingQueue).Produce);
            prod.Start(_configuration);

            var consumerPool = new List<Thread>(ConsumersCount);
            for (var i = 0; i < ConsumersCount; i++)
            {
                var consumer = new Consumer(_workingQueue);
                consumerPool.Add(new Thread(consumer.Consume));
            }

            foreach (var thread in consumerPool)
            {
                thread.Start(_countdownEvent.Signal());
            }

            prod.Join();
            _countdownEvent.Wait();
        }

        public void Dispose()
        {
            _countdownEvent?.Dispose();
        }
    }
}