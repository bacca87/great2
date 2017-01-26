using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Great.Models;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class FactoriesViewModel : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// Gets the TransferTypes property.  
        /// </summary>
        public IList<TransferType> TransferTypes
        {
            get
            {
                return _db.TransferTypes.ToList();
            }
        }

        /// <summary>
        /// The <see cref="Factories" /> property's name.
        /// </summary>
        private IList<Factory> _factories;

        /// <summary>
        /// Sets and gets the Factories property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public IList<Factory> Factories
        {
            get
            {
                return _factories;
            }

            set
            {   
                _factories = value;
                RaisePropertyChanged(nameof(Factories));
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

                FactoryInfo = _selectedFactory != null ? _selectedFactory.Clone() : new Factory();
                
                RaisePropertyChanged(nameof(SelectedFactory), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="FactoryInfo" /> property's name.
        /// </summary>
        private Factory _factoryInfo = new Factory();

        /// <summary>
        /// Sets and gets the FactoryInfo property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Factory FactoryInfo
        {
            get
            {
                return _factoryInfo;
            }

            set
            {
                var oldValue = _factoryInfo;
                _factoryInfo = value;
                RaisePropertyChanged(nameof(FactoryInfo), oldValue, value);
            }
        }


        private DBEntities _db { get; set; }
        #endregion

        #region Commands
        public RelayCommand<Factory> SaveFactoryCommand { get; set; }
        public RelayCommand ClearSelectionCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the FactoriesViewModel class.
        /// </summary>
        public FactoriesViewModel(DBEntities db)
        {
            _db = db;

            SaveFactoryCommand = new RelayCommand<Factory>(SaveFactory);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

            RefreshFactories();
        }

        /// <summary>
        /// Refresh the factories list
        /// </summary>
        public void RefreshFactories()
        {
            Factories = _db.Factories.ToList();
        }
        
        private void ClearSelection()
        {
            SelectedFactory = null;            
        }

        private void SaveFactory(Factory factory)
        {   
            _db.Factories.AddOrUpdate(factory);

            if (_db.SaveChanges() > 0)
            {
                RefreshFactories();
                SelectedFactory = factory;
            }
        }
    }
}