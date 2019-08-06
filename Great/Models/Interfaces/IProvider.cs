using Great.Models.DTO;
using System;

namespace Great.Models.Interfaces
{
    public interface IProvider
    {
        EProviderStatus Status { get; set; }
        void SendEmail(EmailMessageDTO message);

        event EventHandler<NewMessageEventArgs> OnNewMessage;

    }
}
