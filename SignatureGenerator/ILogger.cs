using System;

namespace SignatureGenerator
{
    public interface ILogger
    {
        void Write(string message, params object[] args);
    }

    public class ConsoleLogger : ILogger
    {
        public void Write(string message, params object[] args)
        {
            Console.Out.WriteLine(message, args);
        }
    }
}