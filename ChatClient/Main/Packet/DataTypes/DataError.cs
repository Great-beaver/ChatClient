namespace ChatClient.Main.Packet.DataTypes
{
    public struct DataError : IData
    {
        public string Type { get; private set; }
        public byte[] Content { get;  set; }
        public byte LastPacket { get; private set; }
        public byte PacketNumber { get; private set; }
        public long FileLenght { get; private set; }
        public string FileName { get; private set; }

        public DataError (byte[] data) : this()
        {
            // Тип пакета
            Type = "DataError";

            // Добавлено чтобы избежать значения null    
            FileName = "";
            Content = new byte[] { 0x00 };
        }
    }
}