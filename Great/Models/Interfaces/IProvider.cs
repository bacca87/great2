using Great.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models.Interfaces
{
    public interface IProvider
    {
        EProviderStatus Status { get; set; }
        void SendEmail(EmailMessageDTO message);

        event EventHandler<NewMessageEventArgs> OnNewMessage;

    }
}
