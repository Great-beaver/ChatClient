﻿namespace ChatClient.Main
{
    public enum MessageType
    {
        SystemMessage,
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
        Error,
        ReadPortAvailable,
        ReadPortUnavailable,
        WritePortAvailable,
        WritePortUnavailable,
        WaitFileRecipientAnswer,
    }
}