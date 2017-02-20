using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Great.Models
{
    public class ExchangeProvider
    {
        private ExchangeService service = new ExchangeService();

        public string EmailAddress { get; set; }
        public string Password { get; set; }

        public ExchangeProvider()
        {
            service.TraceEnabled = true;
            service.TraceFlags = TraceFlags.None;

            EmailAddress = "baccarani.m@elettric80.it";
            Password = "Password1";

            Thread workThread = new Thread(ThreadWorker);
            workThread.Start();
        }

        private void ThreadWorker()
        {
            service.Credentials = new WebCredentials(EmailAddress, Password);
            service.AutodiscoverUrl(EmailAddress, RedirectionUrlValidationCallback);
            //SendEmail();
            ReadEmail();
        }

        private void ReadEmail()
        {
            ItemView itemView = new ItemView(int.MaxValue);
            
            SearchFilter searchFilter = new SearchFilter.IsEqualTo(EmailMessageSchema.From, "fdl@elettric80.it");
            
            // Process each item.
            foreach (Item myItem in FindItemsInSubfolders(new FolderId(WellKnownFolderName.MsgFolderRoot), searchFilter))
            {
                if (myItem is EmailMessage)
                {
                    Console.WriteLine((myItem as EmailMessage).Subject);
                }

                else if (myItem is MeetingRequest)
                {
                    Console.WriteLine((myItem as MeetingRequest).Subject);
                }
                else
                {
                    // Else handle other item types.
                }
            }
        }

        public IEnumerable<Item> FindItemsInSubfolders(FolderId root, SearchFilter itemsFilter)
        {
            foreach (Folder folder in service.FindFolders(root, new FolderView(int.MaxValue) { Traversal = FolderTraversal.Deep }))
                foreach(Item item in service.FindItems(folder.Id, itemsFilter, new ItemView(int.MaxValue)))
                    yield return item;

            foreach (Item item in service.FindItems(root, itemsFilter, new ItemView(int.MaxValue)))
                yield return item;
        }
        
        private void SendEmail()
        {
            //TODO TEST
            EmailMessage email = new EmailMessage(service);
            email.ToRecipients.Add(EmailAddress);
            email.Subject = "HelloWorld";
            email.Body = new MessageBody("This is the first email I've sent by using the EWS Managed API");
            email.Send();
        }

        private bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
                result = true;

            return result;
        }
    }
}
