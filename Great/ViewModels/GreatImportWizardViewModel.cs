using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
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

        private GreatMigration _greatMigra;
        #endregion

        #region Commands Definitions
        public RelayCommand StartMigrationCommand { get; set; }
        public RelayCommand SelectFolderCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the GreatImportWizardViewModel class.
        /// </summary>
        public GreatImportWizardViewModel()
        {
            _greatMigra = new GreatMigration();
            _greatMigra.OnOperationCompleted += _greatMigra_OnOperationCompleted;

            StartMigrationCommand = new RelayCommand(StartMigration);
            SelectFolderCommand = new RelayCommand(SelectFolder);

            Reset();
        }

        private void _greatMigra_OnOperationCompleted(object source, EventImportArgs args)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    Status = args.Result;
                    Completed = _greatMigra.Completed;
                })
            );
        }

        private void Reset()
        {
            Completed = false;
            InstallationFolder = GreatMigration.sGreatDefaultInstallationFolder;
        }

        private void SelectFolder()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            dialog.Title = "My Title";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = InstallationFolder;

            dialog.AddToMostRecentlyUsedList = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.DefaultDirectory = GreatMigration.sGreatDefaultInstallationFolder;
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