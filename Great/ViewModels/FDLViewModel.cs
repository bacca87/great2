using GalaSoft.MvvmLight;
using Great.Models;

namespace Great.ViewModels
{
    public class FDLViewModel : ViewModelBase
    {
        private FDLManager _fdlManager;

        /// <summary>
        /// Initializes a new instance of the EmailViewModel class.
        /// </summary>
        public FDLViewModel(FDLManager manager)
        {
            _fdlManager = manager;
        }
    }
}