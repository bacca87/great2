using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class GreatImportWizardViewModel : ViewModelBase
    {
        #region Properties
        private readonly Logger log = LogManager.GetLogger("GreatImport");

        /// <summary>
        /// The <see cref="InstallationFolder" /> property's name.
        /// </summary>
        private string _installationFolder;

        /// <summary>
        /// Sets and gets the InstallationPath property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public string InstallationFolder
        {
            get
            {
                return _installationFolder;
            }
            set
            {
                _installationFolder = value;
                RaisePropertyChanged(nameof(InstallationFolder));
            }
        }

        /// <summary>
        /// The <see cref="Completed" /> property's name.
        /// </summary>
        private bool _completed;

        /// <summary>
        /// Sets and gets the IsDone property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public bool Completed
        {
            get
            {
                return _completed;
            }
            internal set
            {
                _completed = value;
                RaisePropertyChanged(nameof(Completed));
            }
        }

        /// <summary>
        /// The <see cref="Status" /> property's name.
        /// </summary>
        private string _status;

        /// <summary>
        /// Sets and gets the Status property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
            internal set
            {
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// The <see cref="LogText" /> property's name.
        /// </summary>
        private string _logText;

        /// <summary>
        /// Sets and gets the LogText property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public string LogText
        {
            get
            {
                return _logText;
            }
            internal set
            {
                _logText = value;
                RaisePropertyChanged(nameof(LogText));
            }
        }

        private GreatImport _greatMigra;
        #endregion

        #region Commands Definitions
        public RelayCommand StartMigrationCommand { get; set; }
        public RelayCommand SelectFolderCommand { get; set; }
        #endregion

        #region Cache Properties
        private DateTime LastTextUpdate;
        private string CachedText;
        #endregion

        /// <summary>
        /// Initializes a new instance of the GreatImportWizardViewModel class.
        /// </summary>
        public GreatImportWizardViewModel()
        {
            _greatMigra = new GreatImport();
            _greatMigra.OnStatusChanged += _greatMigra_OnStatusChanged; ;
            _greatMigra.OnMessage += _greatMigra_OnMessage;
            _greatMigra.OnCompleted += _greatMigra_OnCompleted;

            StartMigrationCommand = new RelayCommand(StartMigration);
            SelectFolderCommand = new RelayCommand(SelectFolder);

            Reset();
        }

        private void _greatMigra_OnStatusChanged(object source, GreatImportArgs args)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    Status = args.Message;
                })
            );
        }

        private void _greatMigra_OnMessage(object source, GreatImportArgs args)
        {
            CachedText += args.Message + Environment.NewLine;

            if(DateTime.UtcNow.Subtract(LastTextUpdate).TotalSeconds > 0.5)
            {
                Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                    new Action<string>((text) =>
                    {
                        LogText += text;
                    }), 
                    CachedText
                );

                CachedText = string.Empty;
                LastTextUpdate = DateTime.UtcNow;
            }
        }

        private void _greatMigra_OnCompleted(object source)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {   
                    Completed = true;
                })
            );
        }

        private void Reset()
        {
            Completed = false;
            InstallationFolder = GreatImport.sGreatDefaultInstallationFolder;
            CachedText = string.Empty;
            LastTextUpdate = DateTime.UtcNow;
        }

        private void SelectFolder()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            dialog.Title = "My Title";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = InstallationFolder;

            dialog.AddToMostRecentlyUsedList = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.DefaultDirectory = GreatImport.sGreatDefaultInstallationFolder;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            dialog.EnsureReadOnly = false;
            dialog.EnsureValidNames = true;
            dialog.Multiselect = false;
            dialog.ShowPlacesList = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                InstallationFolder = dialog.FileName;
            }
        }

        private void StartMigration()
        {
            _greatMigra.StartMigration(InstallationFolder);
            Status = "Migration Started!";
        }
    }
}