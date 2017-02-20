using GalaSoft.MvvmLight;
using Great.Models;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
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