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
    public class FDLImportWizardViewModel : ViewModelBase
    {
        #region Properties
        private readonly Logger log = LogManager.GetLogger("FDLImport");

        /// <summary>
        /// The <see cref="FDLFolder" /> property's name.
        /// </summary>
        private string _fdlFolder;

        /// <summary>
        /// Sets and gets the InstallationPath property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public string FDLFolder
        {
            get
            {
                return _fdlFolder;
            }
            set
            {
                _fdlFolder = value;
                RaisePropertyChanged(nameof(FDLFolder));
            }
        }

        /// <summary>
        /// The <see cref="Completed" /> property's name.
        /// </summary>
        private bool _completed;

        /// <summary>
        /// Sets and gets the Completed property.
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
        /// The <see cref="CanSelectPreviousPage" /> property's name.
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Sets and gets the CanSelectPreviousPage property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public bool CanSelectPreviousPage
        {
            get
            {
                return _isRunning;
            }
            internal set
            {
                _isRunning = value;
                RaisePropertyChanged(nameof(CanSelectPreviousPage));
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

        #endregion

        #region Commands Definitions
        public RelayCommand StartImportCommand { get; set; }
        public RelayCommand SelectFolderCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        #endregion

        #region Cache Properties
        private DateTime LastTextUpdate;
        private string CachedText;
        #endregion

        private FDLImport _fdlImport;

        /// <summary>
        /// Initializes a new instance of the GreatImportWizardViewModel class.
        /// </summary>
        public FDLImportWizardViewModel()
        {
            _fdlImport = new FDLImport();
            _fdlImport.OnStatusChanged += _fdlImport_OnStatusChanged; ;
            _fdlImport.OnMessage += _fdlImport_OnMessage;
            _fdlImport.OnFinish += _fdlImport_OnFinished;

            StartImportCommand = new RelayCommand(StartImport);
            SelectFolderCommand = new RelayCommand(SelectFolder);
            CancelCommand = new RelayCommand(Cancel);

            Reset();
        }

        private void _fdlImport_OnStatusChanged(object source, ImportArgs args)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    Status = args.Message;
                })
            );
        }

        private void _fdlImport_OnMessage(object source, ImportArgs args)
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

        private void _fdlImport_OnFinished(object source, bool IsCompleted)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {   
                    Completed = IsCompleted;
                    CanSelectPreviousPage = !IsCompleted;
                    LogText += CachedText;
                })
            );
        }

        private void Reset()
        {
            Completed = false;
            FDLFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            CachedText = string.Empty;
            LastTextUpdate = DateTime.UtcNow;
            CanSelectPreviousPage = true;
        }

        public void SelectFolder()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            dialog.Title = "FDL Folder";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = FDLFolder;

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
                FDLFolder = dialog.FileName;
            }
        }

        public void StartImport()
        {
            _fdlImport.FDLFolder = FDLFolder;
            _fdlImport.Start();
            CanSelectPreviousPage = false;
        }

        public void Cancel()
        {
            _fdlImport.Cancel();
            Reset();
        }
    }
}