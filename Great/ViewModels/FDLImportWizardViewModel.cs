using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using System;
using System.Diagnostics;
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
        private DispatcherTimer refreshTimer = new DispatcherTimer();
        private FDLImport _fdlImport;

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
        public RelayCommand FinishCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the GreatImportWizardViewModel class.
        /// </summary>
        public FDLImportWizardViewModel()
        {
            _fdlImport = new FDLImport();
            
            StartImportCommand = new RelayCommand(StartImport);
            SelectFolderCommand = new RelayCommand(SelectFolder);
            CancelCommand = new RelayCommand(Cancel);
            FinishCommand = new RelayCommand(Finish);

            Reset();

            refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            refreshTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            refreshTimer.Start();
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            string newText = string.Empty;

            while(!_fdlImport.Output.IsEmpty)
            {
                string text;

                if (!_fdlImport.Output.TryDequeue(out text))
                    continue;

                newText += text + Environment.NewLine;
            }

            if (newText != string.Empty)
                LogText += newText;

            if (Status != _fdlImport.Status)
                Status = _fdlImport.Status;

            if (Completed != _fdlImport.IsCompleted)
                Completed = _fdlImport.IsCompleted;

            if (CanSelectPreviousPage != _fdlImport.IsCancelled)
                CanSelectPreviousPage = _fdlImport.IsCancelled;
        }

        private void Reset()
        {
            Completed = false;
            FDLFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
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

        public void Finish()
        {
            MessageBox.Show("The application will be restarted in order to apply changes.", "Restart Required", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start(Application.ResourceAssembly.Location, "-m");
            Application.Current.Shutdown();
        }
    }
}