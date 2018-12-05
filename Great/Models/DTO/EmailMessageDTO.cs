using Microsoft.Exchange.WebServices.Data;
using System.Collections.Generic;

namespace Great.Models.DTO
{
    public class EmailMessageDTO
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public Importance Importance { get; set; }
        public IList<string> ToRecipients { get; set; }
        public IList<string> CcRecipients { get; set; }
        public IList<string> Attachments { get; set; }

        public EmailMessageDTO()
        {
            ToRecipients = new List<string>();
            CcRecipients = new List<string>();
            Attachments = new List<string>();
        }
    }    
}
