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
        private readonly ILogger _logger;

        public ProcessRunner(Configuration config, ILogger logger)
        {
            _configuration = config;
            _logger = logger;

            var queueUpperLimit = MaxBufferSize * 100 / config.BlockSize;

            _workingQueue = new BlockingCollection<QueueWorkItem>(queueUpperLimit);
            _countdownEvent = new CountdownEvent(ConsumersCount);
        }

        public void Run()
        {
            var prod = new Thread(new Producer(_workingQueue, _configuration, _logger).Produce);
            prod.Start();

            var consumerPool = new List<Thread>(ConsumersCount);
            for (var i = 0; i < ConsumersCount; i++)
            {
                var consumer = new Consumer(_workingQueue, _logger);
                consumerPool.Add(new Thread(consumer.Consume));
            }

            foreach (var thread in consumerPool)
            {
                thread.Start((Action) Callback);
            }

            prod.Join();
            _countdownEvent.Wait();
            
            void Callback() => _countdownEvent.Signal();
        }

        public void Dispose()
        {
            _countdownEvent?.Dispose();
        }
    }
}