using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great2.Models;
using Great2.Models.Database;
using Great2.Models.Interfaces;
using Microsoft.WindowsAPICodePack.Dialogs;
using Nager.Date;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Great2.ViewModels
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

        private bool _UseWindowsAuthentication;
        public bool UseWindowsAuthentication
        {
            get => _UseWindowsAuthentication;
            set
            {
                Set(ref _UseWindowsAuthentication, value);
                EmailAddress = string.Empty;
                EmailPassword = string.Empty;
            }
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

        private bool _AskOrderRecipients;
        public bool AskOrderRecipients
        {
            get => _AskOrderRecipients;
            set => Set(ref _AskOrderRecipients, value);
        }

        #region Appeareance
        private ObservableCollection<ColorItem> _AvailableColors;
        public ObservableCollection<ColorItem> AvailableColors
        {
            get => _AvailableColors;
            set => Set(ref _AvailableColors, value);
        }

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

        public string _NewOrderDefaultRecipients;
        public string NewOrderDefaultRecipients
        {
            get => _NewOrderDefaultRecipients;
            set => Set(ref _NewOrderDefaultRecipients, value);
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

        private ObservableCollection<Currency> _Currencies;
        public ObservableCollection<Currency> Currencies
        {
            get => _Currencies;
            set => Set(ref _Currencies, value);
        }

        private string _DefaultCurrency;
        public string DefaultCurrency
        {
            get => _DefaultCurrency;
            set => Set(ref _DefaultCurrency, value);
        }

        private bool _excelExpenseAccount;
        public bool ExcelExpenseAccount
        {
            get => _excelExpenseAccount;
            set => Set(ref _excelExpenseAccount, value);
        }

        private int _cdc;
        public int CDC
        {
            get => _cdc;
            set => Set(ref _cdc, value);
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

            using (DBArchive db = new DBArchive())
            {
                Currencies = new ObservableCollection<Currency>(db.Currencies);
            }

            AvailableColors = new ObservableCollection<ColorItem>(MaterialColors.Colors.Select(c => new ColorItem(ColorConverter.ConvertFromString(c.Value) as Color?, c.Key)));

            NewOrderDefaultRecipients = string.Empty;
            FDLCancelRequestRecipients = string.Empty;
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
                DataDirectory = dialog.FileName;
        }

        private void MigrateData()
        {
            if (Directory.GetDirectories(DataDirectory).Length != 0 || Directory.GetFiles(DataDirectory).Length != 0)
            {
                MetroMessageBox.Show("The selected folder is not empty! Operation cancelled.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }   

            if (MetroMessageBox.Show("Are you sure to migrate all the data in the new destination folder?\nThe application will be restarted in order to apply changes.", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            UserSettings.Advanced.MigrationDataFolder = DataDirectory;

            Process.Start(Application.ResourceAssembly.Location, "-m");
            Application.Current.Shutdown();
        }

        private void LoadData()
        {
            Country = UserSettings.Localization.Country;
            DataDirectory = ApplicationSettings.Directories.Data;

            UseWindowsAuthentication = UserSettings.Email.UseDefaultCredentials;

            EmailAddress = UserSettings.Email.EmailAddress;
            EmailPassword = UserSettings.Email.EmailPassword;

            AutoAddFactories = UserSettings.Advanced.AutoAddFactories;
            AutoAssignFactories = UserSettings.Advanced.AutoAssignFactories;

            AskOrderRecipients = UserSettings.Email.Recipients.AskOrderRecipients;
                        
            if (UserSettings.Email.Recipients.NewOrderDefaults != null)
            {
                foreach (string address in UserSettings.Email.Recipients.NewOrderDefaults)
                    NewOrderDefaultRecipients += NewOrderDefaultRecipients == string.Empty ? address : "; " + address;
            }
                        
            if (UserSettings.Email.Recipients.FDLCancelRequest != null)
            {
                foreach (string address in UserSettings.Email.Recipients.FDLCancelRequest)
                    FDLCancelRequestRecipients += FDLCancelRequestRecipients == string.Empty ? address : "; " + address;
            }

            DefaultCurrency = UserSettings.Localization.DefaultCurrency;
            ExcelExpenseAccount = UserSettings.Advanced.ExcelExpenseAccount;
            CDC = UserSettings.Advanced.CDC;

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
            IsCustomSundayColorUsed = UserSettings.Themes.IsCustomSundayColorUsed;
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

                UserSettings.Email.UseDefaultCredentials = UseWindowsAuthentication;

                if (UserSettings.Email.EmailAddress != EmailAddress || UserSettings.Email.EmailPassword != EmailPassword)
                {
                    UserSettings.Email.EmailAddress = EmailAddress;
                    UserSettings.Email.EmailPassword = EmailPassword;

                    Exchange.Disconnect();
                    Exchange.Connect();
                }

                UserSettings.Advanced.AutoAddFactories = AutoAddFactories;
                UserSettings.Advanced.AutoAssignFactories = AutoAssignFactories;

                UserSettings.Email.Recipients.AskOrderRecipients = AskOrderRecipients;

                StringCollection OrderRecipients = new StringCollection();
                OrderRecipients.AddRange(NewOrderDefaultRecipients?.Replace(" ", string.Empty).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                UserSettings.Email.Recipients.NewOrderDefaults = OrderRecipients;

                StringCollection CancellationRecipients = new StringCollection();
                CancellationRecipients.AddRange(FDLCancelRequestRecipients?.Replace(" ", string.Empty).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                UserSettings.Email.Recipients.FDLCancelRequest = CancellationRecipients;

                UserSettings.Localization.DefaultCurrency = DefaultCurrency;
                UserSettings.Advanced.ExcelExpenseAccount = ExcelExpenseAccount;
                UserSettings.Advanced.CDC = CDC;

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

                UserSettings.Themes.ApplyAllColors();
                UserSettings.Themes.ApplyThemeAccent(Theme, AccentColor);

                Close();
            }
        }
    }
}