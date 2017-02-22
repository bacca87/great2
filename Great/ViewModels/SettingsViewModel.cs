using GalaSoft.MvvmLight;
using Great.Models;

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
        #endregion

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        public SettingsViewModel()
        {
        }
    }
}