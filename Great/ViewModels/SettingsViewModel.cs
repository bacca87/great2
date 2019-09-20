using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using Great.Models.Interfaces;
using Microsoft.WindowsAPICodePack.Dialogs;
using Nager.Date;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

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

        #region Appeareance
        private ETheme _Skin;
        public ETheme Theme
        {
            get => _Skin;
            set => Set(ref _Skin, value);
        }

        private EAccentColor _AccentColor;
        public EAccentColor AccentColor
        {
            get => _AccentColor;
            set => Set(ref _AccentColor, value);
        }


        private bool _IsCustomSaturdayColorUsed;
        public bool IsCustomSaturdayColorUsed
        {
            get => _IsCustomSaturdayColorUsed;
            set => Set(ref _IsCustomSaturdayColorUsed, value);

        }

        private Color _CustomSaturdayColor;
        public Color CustomSaturdayColor
        {
            get => _CustomSaturdayColor;
            set => Set(ref _CustomSaturdayColor, value);
        }

        private bool _IsCustomSundayColorUsed;
        public bool IsCustomSundayColorUsed
        {
            get => _IsCustomSundayColorUsed;
            set => Set(ref _IsCustomSundayColorUsed, value);
        }

        private Color _CustomSundayColor;
        public Color CustomSundayColor
        {
            get => _CustomSundayColor;
            set => Set(ref _CustomSundayColor, value);
        }

        private bool _IsCustomHolidayColorUsed;
        public bool IsCustomHolidayColorUsed
        {
            get => _IsCustomHolidayColorUsed;
            set => Set(ref _IsCustomHolidayColorUsed, value);
        }

        private Color _CustomHolidayColor;
        public Color CustomHolidayColor
        {
            get => _CustomHolidayColor;
            set => Set(ref _CustomHolidayColor, value);
        }

        private bool _IsCustomVacationColorUsed;
        public bool IsCustomVacationColorUsed
        {
            get => _IsCustomVacationColorUsed;
            set => Set(ref _IsCustomVacationColorUsed, value);
        }

        private Color _CustomVacationColor;
        public Color CustomVacationColor
        {
            get => _CustomVacationColor;
            set => Set(ref _CustomVacationColor, value);
        }

        private bool _IsCustomSickColorUsed;
        public bool IsCustomSickColorUsed
        {
            get => _IsCustomSickColorUsed;
            set => Set(ref _IsCustomSickColorUsed, value);
        }

        private Color _CustomSickColor;
        public Color CustomSickColor
        {
            get => _CustomSickColor;
            set => Set(ref _CustomSickColor, value);
        }

        private bool _IsCustomHomeworkColorUsed;
        public bool IsCustomHomeworkColorUsed
        {
            get => _IsCustomHomeworkColorUsed;
            set => Set(ref _IsCustomHomeworkColorUsed, value);
        }

        private Color _CustomHomeworkColor;
        public Color CustomHomeworkColor
        {
            get => _CustomHomeworkColor;
            set => Set(ref _CustomHomeworkColor, value);
        }

        private bool _IsCustomSpecialLeaveColorUsed;
        public bool IsCustomSpecialLeaveColorUsed
        {
            get => _IsCustomSpecialLeaveColorUsed;
            set => Set(ref _IsCustomSpecialLeaveColorUsed, value);
        }

        private Color _CustomSpecialLeaveColor;
        public Color CustomSpecialLeaveColor
        {
            get => _CustomSpecialLeaveColor;
            set => Set(ref _CustomSpecialLeaveColor, value);
        }
        #endregion

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
            {
                return;
            }

            try
            {
                using (new WaitCursor())
                {
                    string SourcePath = ApplicationSettings.Directories.Data.TrimEnd('\\');
                    string DestinationPath = DataDirectory.TrimEnd('\\');

                    //Now Create all of the directories
                    foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                        SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
                    }

                    //Copy all the files & Replaces any files with the same name
                    foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                        SearchOption.AllDirectories))
                    {
                        File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
                    }

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
                {
                    FDLCancelRequestRecipients += FDLCancelRequestRecipients == string.Empty ? address : "; " + address;
                }
            }

            Theme = UserSettings.Themes.Theme;
            AccentColor = UserSettings.Themes.AccentColor;

            CustomSaturdayColor = UserSettings.Themes.CustomSaturdayColor.Color;
            CustomSundayColor = UserSettings.Themes.CustomSundayColor.Color;
            CustomHolidayColor = UserSettings.Themes.CustomHolidayColor.Color;
            CustomVacationColor = UserSettings.Themes.CustomVacationColor.Color;
            CustomSickColor = UserSettings.Themes.CustomSickColor.Color;
            CustomHomeworkColor = UserSettings.Themes.CustomHomeworkColor.Color;
            CustomSpecialLeaveColor = UserSettings.Themes.CustomSpecialLeaveColor.Color;

            IsCustomSaturdayColorUsed = UserSettings.Themes.IsCustomSaturdayColorUsed;
            IsCustomSundayColorUsed = UserSettings.Themes.IsCustomSaturdayColorUsed;
            IsCustomHolidayColorUsed = UserSettings.Themes.IsCustomHolidayColorUsed;
            IsCustomVacationColorUsed = UserSettings.Themes.IsCustomVacationColorUsed;
            IsCustomSickColorUsed = UserSettings.Themes.IsCustomSickColorUsed;
            IsCustomHomeworkColorUsed = UserSettings.Themes.IsCustomHomeworkColorUsed;
            IsCustomSpecialLeaveColorUsed = UserSettings.Themes.IsCustomSpecialLeaveColorUsed;
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
                string[] addresses = FDLCancelRequestRecipients?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < addresses?.Length; i++)
                {
                    recipients.Add(addresses[i].Trim());
                }

                UserSettings.Email.Recipients.FDLCancelRequest = recipients;

                UserSettings.Themes.Theme = Theme;
                UserSettings.Themes.AccentColor = AccentColor;

                UserSettings.Themes.IsCustomSaturdayColorUsed = IsCustomSaturdayColorUsed;
                UserSettings.Themes.IsCustomSundayColorUsed = IsCustomSundayColorUsed;
                UserSettings.Themes.IsCustomHolidayColorUsed = IsCustomHolidayColorUsed;
                UserSettings.Themes.IsCustomVacationColorUsed = IsCustomVacationColorUsed;
                UserSettings.Themes.IsCustomSickColorUsed = IsCustomSickColorUsed;
                UserSettings.Themes.IsCustomHomeworkColorUsed = IsCustomHomeworkColorUsed;
                UserSettings.Themes.IsCustomSpecialLeaveColorUsed = IsCustomSpecialLeaveColorUsed;

                UserSettings.Themes.CustomSaturdayColor = new SolidColorBrush(CustomSaturdayColor);
                UserSettings.Themes.CustomSundayColor = new SolidColorBrush(CustomSundayColor);
                UserSettings.Themes.CustomHolidayColor = new SolidColorBrush(CustomHolidayColor);
                UserSettings.Themes.CustomVacationColor = new SolidColorBrush(CustomVacationColor);
                UserSettings.Themes.CustomSickColor = new SolidColorBrush(CustomSickColor);
                UserSettings.Themes.CustomHomeworkColor = new SolidColorBrush(CustomHomeworkColor);
                UserSettings.Themes.CustomSpecialLeaveColor = new SolidColorBrush(CustomSpecialLeaveColor);

                (Application.Current as App).ApplyColors();
                (Application.Current as App).ApplyThemeAccent(Theme, AccentColor);

                Close();
            }
        }
    }
}