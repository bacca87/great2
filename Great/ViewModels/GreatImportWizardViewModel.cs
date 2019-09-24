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
    public class GreatImportWizardViewModel : ViewModelBase
    {
        #region Properties
        private readonly Logger log = LogManager.GetLogger("GreatImport");
        private DispatcherTimer refreshTimer = new DispatcherTimer();
        private GreatImport _greatMigra;

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
            get => _installationFolder;
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
        /// Sets and gets the Completed property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public bool Completed
        {
            get => _completed;
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
            get => _isRunning;
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
            get => _status;
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
            get => _logText;
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
        public GreatImportWizardViewModel()
        {
            StartImportCommand = new RelayCommand(StartImport);
            SelectFolderCommand = new RelayCommand(SelectFolder);
            CancelCommand = new RelayCommand(Cancel);
            FinishCommand = new RelayCommand(Finish);

            Reset();

            refreshTimer.Tick += (s, e) => Refresh();
            refreshTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        }

        private void Reset()
        {
            Completed = false;
            InstallationFolder = GreatImport.sGreatDefaultInstallationFolder;
            CanSelectPreviousPage = true;
        }

        private void Refresh()
        {
            string newText = string.Empty;

            while (!_greatMigra.Output.IsEmpty)
            {
                string text;

                if (!_greatMigra.Output.TryDequeue(out text))
                    continue;

                newText += text + Environment.NewLine;
            }

            if (newText != string.Empty)
                LogText += newText;

            if (Status != _greatMigra.Status)
                Status = _greatMigra.Status;

            if (Completed != _greatMigra.IsCompleted)
                Completed = _greatMigra.IsCompleted;

            if (CanSelectPreviousPage != _greatMigra.IsCancelled)
                CanSelectPreviousPage = _greatMigra.IsCancelled;
        }

        public void SelectFolder()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            dialog.Title = "GREAT Installation Folder";
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
                InstallationFolder = dialog.FileName;
        }

        public void StartImport()
        {
            _greatMigra = new GreatImport();
            _greatMigra.GreatPath = InstallationFolder;
            _greatMigra.Start();

            refreshTimer.Start();

            CanSelectPreviousPage = false;
        }

        public void Cancel()
        {
            _greatMigra.Cancel();
            refreshTimer.Stop();

            Refresh();
            Reset();
        }

        public void Finish()
        {
            refreshTimer.Stop();
            Refresh();

            MetroMessageBox.Show("The application will be restarted in order to apply changes.", "Restart Required", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start(Application.ResourceAssembly.Location, "-m");
            Application.Current.Shutdown();
        }
    }
}
