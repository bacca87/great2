using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Utils;
using Great2.Utils.Messages;
using Great2.ViewModels.Database;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Great2.ViewModels
{
    public class FactoriesViewModel : ViewModelBase
    {
        #region Properties

        private bool _showEditMenu;
        public bool ShowEditMenu
        {
            get => _showEditMenu;
            set => Set(ref _showEditMenu, value);
        }


        private bool _ShowExpandable;
        public bool ShowExpandable
        {
            get => _ShowExpandable;
            set => Set(ref _ShowExpandable, value);

        }
        public ObservableCollection<TransferTypeDTO> TransferTypes { get; set; }

        public ObservableCollectionEx<FactoryEVM> Factories { get; set; }

        private FactoryEVM _selectedFactory;
        public FactoryEVM SelectedFactory
        {
            get => _selectedFactory;
            set
            {
                _selectedFactory?.CheckChangedEntity();
                Set(ref _selectedFactory, value ?? new FactoryEVM());
                ShowEditMenu = false;
            }
        }

        public Action<FactoryEVM> OnZoomOnFactoryRequest { get; set; }
        public Action<FactoryEVM> OnFactoryUpdated { get; set; }
        #endregion

        #region Commands
        public RelayCommand<FactoryEVM> DeleteFactoryCommand { get; set; }
        public RelayCommand<FactoryEVM> SaveFactoryCommand { get; set; }
        public RelayCommand<FactoryEVM> NewFactoryCommand { get; set; }
        public RelayCommand GotFocusCommand { get; set; }
        public RelayCommand LostFocusCommand { get; set; }

        public RelayCommand PageUnloadedCommand { get; set; }

        public RelayCommand ClearSelectionCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the FactoriesViewModel class.
        /// </summary>
        public FactoriesViewModel()
        {
            using (DBArchive db = new DBArchive())
            {
                Factories = new ObservableCollectionEx<FactoryEVM>(db.Factories.ToList().Select(f => new FactoryEVM(f)));
                TransferTypes = new ObservableCollection<TransferTypeDTO>(db.TransferTypes.ToList().Select(t => new TransferTypeDTO(t)));
            }

            DeleteFactoryCommand = new RelayCommand<FactoryEVM>(DeleteFactory);
            SaveFactoryCommand = new RelayCommand<FactoryEVM>(SaveFactory);
            NewFactoryCommand = new RelayCommand<FactoryEVM>(NewFactory);
            //ClearSelectionCommand = new RelayCommand(ClearSelection);
            GotFocusCommand = new RelayCommand(() => { ShowEditMenu = true; });
            LostFocusCommand = new RelayCommand(() => { });
            PageUnloadedCommand = new RelayCommand(() => { SelectedFactory?.CheckChangedEntity(); });


            MessengerInstance.Register<NewItemMessage<FactoryEVM>>(this, NewFactory);

            SelectedFactory = Factories.FirstOrDefault();
        }



        private void NewFactory(FactoryEVM obj)
        {
            SelectedFactory = new FactoryEVM();
            ShowExpandable = true;
        }

        public void NewFactory(NewItemMessage<FactoryEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null && !Factories.Any(f => f.Id == item.Content.Id))
                        Factories.Add(item.Content);
                })
            );
        }

        public void ZoomOnFactoryRequest(FactoryEVM factory)
        {
            OnZoomOnFactoryRequest?.Invoke(factory);
        }

        private void ClearSelection()
        {
            SelectedFactory = null;
        }

        private void DeleteFactory(FactoryEVM factory)
        {
            using (DBArchive db = new DBArchive())
            {
                int fdlCount = db.FDLs.Count(f => f.Factory.HasValue && f.Factory.Value == factory.Id);

                if (fdlCount > 0)
                {
                    MetroMessageBox.Show($"The factory {factory.Name} is bound to {fdlCount} FDLs!\nBefore deleting a factory, you must unbound all the realted FDLs!\nOperation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (factory.Delete(db))
                {
                    Factories.Remove(factory);
                    SelectedFactory = null;

                    Messenger.Default.Send(new DeletedItemMessage<FactoryEVM>(this, factory));
                }
            }
        }

        private void SaveFactory(FactoryEVM factory)
        {
            if (factory == null) return;

            if (!factory.IsValid)
            {
                MetroMessageBox.Show("Cannot save/edit the factory. Please check the errors", "Save Factory", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            factory.NotifyAsNew = false;

            if (factory.Save())
            {
                if (!Factories.Contains(factory))
                {
                    Factories.Add(factory);
                    Messenger.Default.Send(new NewItemMessage<FactoryEVM>(this, factory));
                }
                else
                {
                    Messenger.Default.Send(new ItemChangedMessage<FactoryEVM>(this, factory));
                    OnFactoryUpdated?.Invoke(factory);
                }

                SelectedFactory = factory;

                ShowEditMenu = false;
            }
        }
    }
}