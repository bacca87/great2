using GalaSoft.MvvmLight;
using Great.Models;
using System.ComponentModel;
using System.Linq;

namespace Great.ViewModels
{
    public class FDLViewModel : ViewModelBase
    {
        private FDLManager _fdlManager;
        private DBEntities _db;

        /// <summary>
        /// The <see cref="FDLs" /> property's name.
        /// </summary>
        private BindingList<FDL> _FDLs;

        /// <summary>
        /// Sets and gets the FDLs property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public BindingList<FDL> FDLs
        {
            get
            {
                return _FDLs;
            }
            internal set
            {
                _FDLs = value;
                RaisePropertyChanged(nameof(FDLs));
            }
        }

        /// <summary>
        /// The <see cref="SelectedFDL" /> property's name.
        /// </summary>
        private FDL _selectedFDL;

        /// <summary>
        /// Sets and gets the SelectedFDL property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public FDL SelectedFDL
        {
            get
            {
                return _selectedFDL;
            }

            set
            {
                var oldValue = _selectedFDL;
                _selectedFDL = value;
                
                RaisePropertyChanged(nameof(SelectedFDL), oldValue, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the EmailViewModel class.
        /// </summary>
        public FDLViewModel(FDLManager manager)
        {
            _db = new DBEntities();
            _fdlManager = manager;

            FDLs = new BindingList<FDL>(_db.FDLs.ToList());
        }
    }
}