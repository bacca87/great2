using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Great.Models;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
using System.Linq;

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
        /// Sets and gets the Factories property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public ObservableCollection<Factory> Factories { get; set; }

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
        public FactoriesViewModel()
        {
            _db = new DBEntities();

            DeleteFactoryCommand = new RelayCommand<Factory>(DeleteFactory);
            SaveFactoryCommand = new RelayCommand<Factory>(SaveFactory);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

            RefreshFactories();
        }

        /// <summary>
        /// Refresh the factories list
        /// </summary>
        public void RefreshFactories()
        {
            Factories = new ObservableCollection<Factory>(_db.Factories);
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
            _db.Factories.AddOrUpdate(factory);

            if (_db.SaveChanges() > 0)
            {
                // check if already exist in the collection
                Factory itemFactory = Factories.Where(f => f.Id == factory.Id).FirstOrDefault();

                if (itemFactory != null)
                    Factories[Factories.IndexOf(itemFactory)] = factory;
                else
                    Factories.Add(factory);

                SelectedFactory = factory;
            }
        }
    }
}