using Great.Models.DTO;
using System;

namespace Great.Models.Interfaces
{
    public interface IProvider
    {
        EProviderStatus Status { get; set; }
        void SendEmail(EmailMessageDTO message);
        bool IsServiceAvailable();

        event EventHandler<NewMessageEventArgs> OnNewMessage;
        event EventHandler<MessageEventArgs> OnMessageSent;
    }
}
