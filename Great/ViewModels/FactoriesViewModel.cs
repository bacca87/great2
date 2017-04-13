using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Great.Models;
using Great.Utils;
using Great.Utils.Messages;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Great.ViewModels
{
    public class FactoriesViewModel : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// Gets the TransferTypes property.  
        /// </summary>
        public ObservableCollection<TransferType> TransferTypes
        {
            get
            {
                return new ObservableCollection<TransferType>(_db.TransferTypes);
            }
        }

        /// <summary>
        /// The <see cref="Factories" /> property's name.
        /// </summary>
        private ObservableCollectionEx<Factory> _factories;

        /// <summary>
        /// Sets and gets the Factories property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public ObservableCollectionEx<Factory> Factories
        {
            get
            {
                return _factories;
            }
            set
            {
                _factories = value;
                RaisePropertyChanged(nameof(Factories), true);
            }
        }

        /// <summary>
        /// The <see cref="SelectedFactory" /> property's name.
        /// </summary>
        private Factory _selectedFactory;

        /// <summary>
        /// Sets and gets the SelectedFactory property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Factory SelectedFactory
        {
            get
            {
                return _selectedFactory;
            }

            set
            {
                var oldValue = _selectedFactory;
                _selectedFactory = value;

                SelectedFactoryClone = _selectedFactory != null ? _selectedFactory.Clone() : new Factory();
                
                RaisePropertyChanged(nameof(SelectedFactory), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="SelectedFactoryClone" /> property's name.
        /// </summary>
        private Factory _factoryInfo = new Factory();

        /// <summary>
        /// Sets and gets the SelectedFactoryClone property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Factory SelectedFactoryClone
        {
            get
            {
                return _factoryInfo;
            }

            set
            {
                var oldValue = _factoryInfo;
                _factoryInfo = value;
                RaisePropertyChanged(nameof(SelectedFactoryClone), oldValue, value);
            }
        }

        /// <summary>
        /// Sets and gets the OnZoomOnFactoryRequest Action.
        /// </summary>
        public Action<Factory> OnZoomOnFactoryRequest;

        private DBEntities _db { get; set; }
        #endregion

        #region Commands
        public RelayCommand<Factory> DeleteFactoryCommand { get; set; }
        public RelayCommand<Factory> SaveFactoryCommand { get; set; }
        public RelayCommand ClearSelectionCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the FactoriesViewModel class.
        /// </summary>
        public FactoriesViewModel(DBEntities db)
        {
            _db = db;

            Factories = new ObservableCollectionEx<Factory>(_db.Factories.ToList());
            Factories.CollectionChanged += Factories_CollectionChanged;
            
            DeleteFactoryCommand = new RelayCommand<Factory>(DeleteFactory);
            SaveFactoryCommand = new RelayCommand<Factory>(SaveFactory);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

            MessengerInstance.Register<NewItemMessage<Factory>>(this, NewFactory);
            MessengerInstance.Register<ItemChangedMessage<Factory>>(this, FactoryChanged);
        }

        private void Factories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Factories), null, Factories, true);
        }

        public void NewFactory(NewItemMessage<Factory> item)
        {
            // Using the dispatcher for preventing thread conflicts
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null)
                    {
                        Factory factory = _db.Factories.SingleOrDefault(f => f.Id == item.Content.Id);

                        if (factory != null && !Factories.Contains(factory))
                            Factories.Add(factory);
                    }
                })
            );
        }

        public void FactoryChanged(ItemChangedMessage<Factory> item)
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

        public void ZoomOnFactoryRequest(Factory factory)
        {
            OnZoomOnFactoryRequest?.Invoke(factory);
        }

        private void ClearSelection()
        {
            SelectedFactory = null;            
        }

        private void DeleteFactory(Factory factory)
        {
            _db.Factories.Remove(factory);

            if (_db.SaveChanges() > 0)
            {
                Factories.Remove(factory);
                SelectedFactory = null;
            }
        }

        private void SaveFactory(Factory factory)
        {
            factory.NotifyAsNew = false;
            _db.Factories.AddOrUpdate(factory);            

            if (_db.SaveChanges() > 0)
            {
                // check if already exist in the collection
                Factory itemFactory = Factories.Where(f => f.Id == factory.Id).FirstOrDefault();

                if (itemFactory != null)
                    Factories[Factories.IndexOf(itemFactory)] = factory;
                else
                {
                    factory.NotifyAsNew = false;
                    Factories.Add(factory);
                }

                SelectedFactory = factory;
            }
        }
    }
}