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
    public class EmailViewModel : ViewModelBase
    {
        private ExchangeProvider _exProvider;

        /// <summary>
        /// Initializes a new instance of the EmailViewModel class.
        /// </summary>
        public EmailViewModel(ExchangeProvider provider)
        {
            _exProvider = provider;
        }
    }
}