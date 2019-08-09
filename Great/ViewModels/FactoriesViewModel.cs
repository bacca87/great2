using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Great.ViewModels
{
    public class FactoriesViewModel : ViewModelBase
    {
        #region Properties
        public ObservableCollection<TransferTypeDTO> TransferTypes { get; set; }

        public ObservableCollectionEx<FactoryEVM> Factories { get; set; }

        private FactoryEVM _selectedFactory;
        public FactoryEVM SelectedFactory
        {
            get => _selectedFactory;
            set => Set(ref _selectedFactory, value ?? new FactoryEVM());
        }

        public Action<FactoryEVM> OnZoomOnFactoryRequest { get; set; }
        #endregion

        #region Commands
        public RelayCommand<FactoryEVM> DeleteFactoryCommand { get; set; }
        public RelayCommand<FactoryEVM> SaveFactoryCommand { get; set; }
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

            Factories.CollectionChanged += Factories_CollectionChanged;
            Factories.ItemPropertyChanged += Factories_ItemPropertyChanged;

            DeleteFactoryCommand = new RelayCommand<FactoryEVM>(DeleteFactory);
            SaveFactoryCommand = new RelayCommand<FactoryEVM>(SaveFactory);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

            MessengerInstance.Register<NewItemMessage<FactoryEVM>>(this, NewFactory);
            MessengerInstance.Register<ItemChangedMessage<FactoryEVM>>(this, FactoryChanged);
        }

        private void Factories_ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            Messenger.Default.Send(new ItemChangedMessage<FactoryEVM>(this, Factories[e.CollectionIndex]));
        }

        private void Factories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (FactoryEVM f in e.NewItems)
                        Messenger.Default.Send(new NewItemMessage<FactoryEVM>(this, f));
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    foreach (FactoryEVM f in e.NewItems)
                        Messenger.Default.Send(new DeletedItemMessage<FactoryEVM>(this, f));
                    break;
                default:
                    break;
            }
        }

        public void NewFactory(NewItemMessage<FactoryEVM> item)
        {
            if (!(item.Sender is FDLManager))
                return;

            // Using the dispatcher for preventing thread conflicts
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null && !Factories.Any(f => f.Id == item.Content.Id))
                        Factories.Add(item.Content);
                })
            );
        }

        public void FactoryChanged(ItemChangedMessage<FactoryEVM> item)
        {
            // NOT USED
            // Using the dispatcher for preventing thread conflicts   
            //Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
            //    new Action(() =>
            //    {
            //        //Do something
            //    })
            //);
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
            if (factory.Delete())
            {
                Factories.Remove(factory);
                SelectedFactory = null;
            }
        }

        private void SaveFactory(FactoryEVM factory)
        {
            factory.NotifyAsNew = false;

            if (factory.Save())
            {
                if (!Factories.Contains(factory))
                {
                    factory.NotifyAsNew = false;
                    Factories.Add(factory);
                }

                SelectedFactory = factory;
            }
        }
    }
}