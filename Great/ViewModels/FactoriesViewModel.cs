using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Great.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

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

        private DBEntities _db { get; set; }
        #endregion

        #region Commands
        //public RelayCommand Search { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the FactoriesViewModel class.
        /// </summary>
        public FactoriesViewModel(DBEntities db)
        {
            _db = db;

            //Search = new RelayCommand(SearchLocation);

            RefreshFactories();
        }

        /// <summary>
        /// Refresh the factories list
        /// </summary>
        public void RefreshFactories()
        {
            Factories = _db.Factories.ToList();
        }
    }
}