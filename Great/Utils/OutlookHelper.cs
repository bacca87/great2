using Great2.Models.DTO;
using System;
using System.Diagnostics;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Great2.Utils
{
    public static class OutlookHelper
    {
        public static bool NewMessage(EmailMessageDTO message)
        {
            try
            {
                Outlook.Application outlook = new Outlook.Application();
                Outlook._MailItem email = (Outlook._MailItem)outlook.CreateItem(Outlook.OlItemType.olMailItem);

                foreach (string address in message.ToRecipients)
                {
                    Outlook.Recipient recipient = email.Recipients.Add(address);
                    recipient.Type = (int)Outlook.OlMailRecipientType.olTo;
                    recipient.Resolve();
                }

                foreach (string address in message.CcRecipients)
                {
                    Outlook.Recipient recipient = email.Recipients.Add(address);
                    recipient.Type = (int)Outlook.OlMailRecipientType.olCC;
                    recipient.Resolve();
                }

                email.Subject = message.Subject;
                email.Body = message.Body;

                foreach (string attachment in message.Attachments)
                    email.Attachments.Add(attachment, Outlook.OlAttachmentType.olByValue, Type.Missing, Type.Missing);

                email.Display(false);
            }
            catch(Exception ex)
            {
                Debugger.Break();
                return false;
            }

            return true;
        }
    }
}
