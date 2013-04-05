namespace ChatClient.Main
{
    public enum MessageType
    {
        Text,
        TextDelivered,
        TextUndelivered,
        FileUndelivered,
        MessageUndelivered,
        FileReceivingComplete,
        FileSendingComplete,
        FileTransferAllowed,
        FileTransferDenied,    
        FileReceivingStarted,
        FileTransferCanceled,     
        FileTransferCanceledBySender,
        FileTransferCanceledByRecipient,
        FileReceivingTimeOut,
    }
}