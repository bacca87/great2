using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Controls;
using Great.Models;
using Great.Models.Interfaces;
using Microsoft.WindowsAPICodePack.Dialogs;
using Nager.Date;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows;

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
        private CountryCode _Country;
        public CountryCode Country
        {
            get => _Country;
            set => Set(ref _Country, value);
        }

        private string _EmailAddress;
        public string EmailAddress
        {
            get => _EmailAddress;
            set => Set(ref _EmailAddress, value);
        }

        private string _EmailPassword;
        public string EmailPassword
        {
            get => _EmailPassword;
            set => Set(ref _EmailPassword, value);
        }

        private bool _AutoAddFactories;
        public bool AutoAddFactories
        {
            get => _AutoAddFactories;
            set => Set(ref _AutoAddFactories, value);
        }

        private ESkin _Skin;
        public ESkin Skin
        {
            get => _Skin;
            set => Set(ref _Skin, value);
        }

        private bool _AutoAssignFactories;
        public bool AutoAssignFactories
        {
            get => _AutoAssignFactories;
            set => Set(ref _AutoAssignFactories, value);
        }

        public string _FDLCancelRequestRecipients;
        public string FDLCancelRequestRecipients
        {
            get => _FDLCancelRequestRecipients;
            set => Set(ref _FDLCancelRequestRecipients, value);
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

        private IProvider Exchange;
        #endregion

        #region Commands Definitions
        public RelayCommand SelectFolderCommand { get; set; }
        public RelayCommand MigrateDataCommand { get; set; }
        public RelayCommand ApplyChangesCommand { get; set; }
        public RelayCommand LoadDataCommand { get; set; }
        #endregion

        #region Actions
        public Action Close { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        public SettingsViewModel(IProvider exchange)
        {
            Exchange = exchange;

            SelectFolderCommand = new RelayCommand(SelectFolder);
            MigrateDataCommand = new RelayCommand(MigrateData, () => { return DataDirectory != ApplicationSettings.Directories.Data; });
            LoadDataCommand = new RelayCommand(LoadData);
            ApplyChangesCommand = new RelayCommand(ApplyChanges);
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

        private void MigrateData()
        {
            if (MetroMessageBox.Show("Are you sure to migrate all the data in the new destination folder?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
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

                    // delete old folder and its contents
                    Directory.Delete(SourcePath, true);
                }

                MetroMessageBox.Show("Migration Completed!\nThe application will be restarted in order to apply changes.", "Restart Required", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start(Application.ResourceAssembly.Location, "-m");
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show($"Migration Failed!\nException: {ex.Message}", "Migration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            Country = UserSettings.Localization.Country;
            DataDirectory = ApplicationSettings.Directories.Data;

            EmailAddress = UserSettings.Email.EmailAddress;
            EmailPassword = UserSettings.Email.EmailPassword;

            AutoAddFactories = UserSettings.Advanced.AutoAddFactories;            
            AutoAssignFactories = UserSettings.Advanced.AutoAssignFactories;

            if (UserSettings.Email.Recipients.FDLCancelRequest != null)
            {
                FDLCancelRequestRecipients = string.Empty;

                foreach (string address in UserSettings.Email.Recipients.FDLCancelRequest)
                    FDLCancelRequestRecipients += FDLCancelRequestRecipients == string.Empty ? address : "; " + address;
            }

            Skin = UserSettings.Themes.Skin;
        }

        private void ApplyChanges()
        {
            using (new WaitCursor())
            {
                UserSettings.Localization.Country = Country;

                if (UserSettings.Email.EmailAddress != EmailAddress || UserSettings.Email.EmailPassword != EmailPassword)
                {
                    UserSettings.Email.EmailAddress = EmailAddress;
                    UserSettings.Email.EmailPassword = EmailPassword;

                    Exchange.Disconnect();
                    Exchange.Connect();
                }

                UserSettings.Advanced.AutoAddFactories = AutoAddFactories;
                UserSettings.Advanced.AutoAssignFactories = AutoAssignFactories;

                StringCollection recipients = new StringCollection();
                string[] addresses = FDLCancelRequestRecipients.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < addresses.Length; i++)
                    recipients.Add(addresses[i].Trim());
                UserSettings.Email.Recipients.FDLCancelRequest = recipients;

                if(UserSettings.Themes.Skin != Skin)
                    UserSettings.Themes.Skin = Skin;

                Close();
            }
        }
    }
}