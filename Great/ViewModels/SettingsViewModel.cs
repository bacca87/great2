using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using Microsoft.Exchange.WebServices.Data;
using Nager.Date;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using System.Diagnostics;

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
        /// Sets and gets the Country property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public CountryCode Country
        {
            get
            {
                return UserSettings.Localization.Country;
            }

            set
            {
                if (UserSettings.Localization.Country == value)
                    return;

                var oldValue = UserSettings.Localization.Country;
                UserSettings.Localization.Country = value;

                RaisePropertyChanged(nameof(Country), oldValue, value);
            }
        }

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
                return UserSettings.Email.EmailPassword;
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
        /// Sets and gets the VacationColor property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public ESkin Skin
        {
            get
            {
                return UserSettings.Themes.Skin;
            }

            set
            {
                UserSettings.Themes.Skin = value;
                RaisePropertyChanged(nameof(Skin));
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

        public string FDLCancelRequestRecipients
        {
            get
            {
                string recipients = string.Empty;

                if(UserSettings.Email.Recipients.FDLCancelRequest != null)
                {
                    foreach (string address in UserSettings.Email.Recipients.FDLCancelRequest)
                        recipients += recipients == string.Empty ? address : "; " + address;
                }
                return recipients;
            }
            set
            {
                StringCollection recipients = new StringCollection();
                string[] addresses = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < addresses.Length; i++)
                    recipients.Add(addresses[i].Trim());
                UserSettings.Email.Recipients.FDLCancelRequest = recipients;
            }
        }

        private string _DataDirectory;
        public string DataDirectory
        {
            get => _DataDirectory;
            set
            {
                Set(ref _DataDirectory, value);
                MigrateDataCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Commands Definitions
        public RelayCommand SelectFolderCommand { get; set; }
        public RelayCommand MigrateDataCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        public SettingsViewModel()
        {   
            SelectFolderCommand = new RelayCommand(SelectFolder);
            MigrateDataCommand = new RelayCommand(MigrateData, () => { return DataDirectory != ApplicationSettings.Directories.Data; });

            DataDirectory = ApplicationSettings.Directories.Data;
        }

        public void SelectFolder()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            dialog.Title = "Data Folder";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = ApplicationSettings.Directories.Data;

            dialog.AddToMostRecentlyUsedList = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            dialog.EnsureReadOnly = false;
            dialog.EnsureValidNames = true;
            dialog.Multiselect = false;
            dialog.ShowPlacesList = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DataDirectory = dialog.FileName;
            }
        }

        public void MigrateData()
        {
            if (MessageBox.Show("Are you sure to migrate all the data in the new destination folder?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            try
            {
                using (new WaitCursor())
                {
                    string SourcePath = ApplicationSettings.Directories.Data.TrimEnd('\\');
                    string DestinationPath = DataDirectory.TrimEnd('\\');

                    //Now Create all of the directories
                    foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                        SearchOption.AllDirectories))
                        Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

                    //Copy all the files & Replaces any files with the same name
                    foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                        SearchOption.AllDirectories))
                        File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);

                    ApplicationSettings.Directories.Data = DataDirectory;
                    MigrateDataCommand.RaiseCanExecuteChanged();
                }

                MessageBox.Show("Migration Completed!\nThe application will be restarted in order to apply changes.", "Restart Required", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start(Application.ResourceAssembly.Location, "-m");
                Application.Current.Shutdown();                
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Migration Failed!\nException: {ex.Message}", "Migration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}