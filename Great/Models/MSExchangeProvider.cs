using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Great.Models
{
    public class MSExchangeProvider
    {
        private ExchangeService exService = new ExchangeService();
        private Thread autodiscoverThread;
        private Uri exServiceUri;

        public MSExchangeProvider()
        {
            autodiscoverThread = new Thread(Autodiscover);
            autodiscoverThread.Name = "Exchange Autodiscover Thread";
            autodiscoverThread.IsBackground = true;
            autodiscoverThread.Start();
        }

        private void Autodiscover()
        {
            do
            {
                try
                {
                    exService.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
                    exService.AutodiscoverUrl(UserSettings.Email.EmailAddress, RedirectionUrlValidationCallback);
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (exService.Url == null);

            exServiceUri = exService.Url;
        }

        public NameResolutionCollection ResolveName(string filter)
        {
            return exService.ResolveName(filter, ResolveNameSearchLocation.ContactsThenDirectory, true);
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
