using GalaSoft.MvvmLight;
using Great.Models;
using Microsoft.Exchange.WebServices.Data;
using System.Collections.Generic;
using System.Linq;
using WpfControls;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// Sets and gets the EmailAddress property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public string EmailAddress
        {
            get
            {
                return UserSettings.Email.EmailAddress;
            }

            set
            {
                if (UserSettings.Email.EmailAddress == value)
                    return;
                
                var oldValue = UserSettings.Email.EmailAddress;
                UserSettings.Email.EmailAddress = value;

                RaisePropertyChanged(nameof(EmailAddress), oldValue, value);
            }
        }

        /// <summary>
        /// Sets and gets the EmailPassword property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public string EmailPassword
        {
            get
            {
                return "Snoop my ass faggot!!!";
            }

            set
            {
                UserSettings.Email.EmailPassword = value;
                RaisePropertyChanged(nameof(EmailPassword));
            }
        }

        /// <summary>
        /// Sets and gets the AutoAddFactories property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public bool AutoAddFactories
        {
            get
            {
                return UserSettings.Advanced.AutoAddFactories;
            }

            set
            {
                UserSettings.Advanced.AutoAddFactories = value;
                RaisePropertyChanged(nameof(AutoAddFactories));
            }
        }

        /// <summary>
        /// Sets and gets the AutoAssignFactories property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public bool AutoAssignFactories
        {
            get
            {
                return UserSettings.Advanced.AutoAssignFactories;
            }

            set
            {
                UserSettings.Advanced.AutoAssignFactories = value;
                RaisePropertyChanged(nameof(AutoAssignFactories));
            }
        }

        public SuggestionProvider EmailSuggestionProvider { get; internal set; }

        private MSExchangeProvider exProvider { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        public SettingsViewModel(MSExchangeProvider provider)
        {
            exProvider = provider;

            EmailSuggestionProvider = new SuggestionProvider((string filter) =>
            {
                string address = filter.Substring(filter.LastIndexOf(';') + 1).Trim();

                if (address != string.Empty)                
                    return exProvider.ResolveName(address);
                else
                    return null;
            });
        }
    }
}