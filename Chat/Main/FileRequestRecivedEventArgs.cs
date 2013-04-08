using System;

namespace ChatClient.Main
{
    public class FileRequestRecivedEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public long FileLenght { get; private set; }
        public byte Sender { get; private set; }
        public bool FileTransferAllowed { get; set; }

        public FileRequestRecivedEventArgs(string fileName, long fileLenght, byte sender)
        {
            FileName = fileName;
            FileLenght = fileLenght;
            Sender = sender;
        }
    }
}