namespace ChatClient
{
    public struct DataError : IData
    {
        public string Type { get; private set; }
        public byte[] Content { get; private set; }
        public byte LastPacket { get; private set; }
        public byte PacketNumber { get; private set; }
        public long FileLenght { get; private set; }
        public string FileName { get; private set; }

        public DataError (byte[] data) : this()
        {
            // Тип пакета
            Type = "DataError";
        }
    }
}