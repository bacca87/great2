using Great.Models.DTO;
using System;

namespace Great.Models.Interfaces
{
    public interface IProvider
    {
        EProviderStatus Status { get; set; }
        void SendEmail(EmailMessageDTO message);
        bool IsServiceAvailable();
        void Connect();
        void Disconnect();

        event EventHandler<NewMessageEventArgs> OnNewMessage;
        event EventHandler<MessageEventArgs> OnMessageSent;
    }

    public enum EProviderStatus
    {
        Offline,
        Connecting,
        Connected,
        Syncronizing,
        Syncronized,
        LoginError,
        Error
    }
}
