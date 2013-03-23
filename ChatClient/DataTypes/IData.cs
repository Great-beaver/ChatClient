namespace ChatClient
{
    public interface IData
    {
        string Type { get;}
        byte[] Content { get; }
        byte LastPacket { get; }
        byte PacketNumber{ get;}
        long FileLenght { get; }
        string FileName { get; }
    }
}