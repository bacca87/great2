using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Great.Models;
using System.Collections.Generic;
using System.Data.Entity;
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
        private IList<TransferType> _transferTypes;

        /// <summary>
        /// Sets and gets the Factories property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public IList<TransferType> TransferTypes
        {
            get
            {
                return _transferTypes;
            }

            set
            {
                _transferTypes = value;
                RaisePropertyChanged("TransferTypes");
            }
        }

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
                RaisePropertyChanged("Factories");
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
                if (_selectedFactory == value)
                {
                    return;
                }

                var oldValue = _selectedFactory;
                _selectedFactory = value;
                RaisePropertyChanged("SelectedFactory", oldValue, value);
            }
        }


        private DBEntities _db { get; set; }
        #endregion

        #region Commands
        public RelayCommand<Factory> CreateFactoryCommand { get; set; }
        public RelayCommand ClearSelectionCommand { get; set; }
        public RelayCommand EditFactoryCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the FactoriesViewModel class.
        /// </summary>
        public FactoriesViewModel(DBEntities db)
        {
            _db = db;

            CreateFactoryCommand = new RelayCommand<Factory>(CreateFactory);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
            EditFactoryCommand = new RelayCommand(EditFactory);

            RefreshFactories();
            RefreshTransferTypes();
        }

        /// <summary>
        /// Refresh the factories list
        /// </summary>
        public void RefreshFactories()
        {
            Factories = _db.Factories.ToList();
        }

        /// <summary>
        /// Refresh the transfer types
        /// </summary>
        public void RefreshTransferTypes()
        {
            TransferTypes = _db.TransferTypes.ToList();
        }

        private void ClearSelection()
        {
            SelectedFactory = null;
        }

        private void CreateFactory(Factory factory)
        {
            _db.Factories.Add(factory);
            _db.SaveChanges();
        }

        private void EditFactory()
        {
        }
    }
}