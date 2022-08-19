namespace SignatureGenerator
{
    public readonly struct QueueWorkItem
    {
        public QueueWorkItem(int blockNumber, byte[] blockData)
        {
            BlockNumber = blockNumber;
            BlockData = blockData;
        }
        
        public int BlockNumber { get; }
        public byte[] BlockData { get; }
    }
}