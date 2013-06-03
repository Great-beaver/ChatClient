namespace Chat.Main.Packet.DataTypes
{
    public interface IData
    {
        DataType Type { get; }
        byte[] Content { get; set; }
        byte LastPacket { get; }
        byte PacketNumber{ get;}
        long FileLenght { get; }
        string FileName { get; }
    }
}