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
                return ApplicationSettings.User.EmailAddress;
            }

            set
            {
                if (ApplicationSettings.User.EmailAddress == value)
                    return;
                
                var oldValue = ApplicationSettings.User.EmailAddress;
                ApplicationSettings.User.EmailAddress = value;

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
                return ApplicationSettings.User.EmailPassword;
            }

            set
            {
                if (ApplicationSettings.User.EmailPassword == value)
                    return;

                var oldValue = ApplicationSettings.User.EmailPassword;
                ApplicationSettings.User.EmailPassword = value;

                RaisePropertyChanged(nameof(EmailPassword), oldValue, value);
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