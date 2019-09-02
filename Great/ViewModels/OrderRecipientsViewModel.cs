using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using Great.Models.Database;
using System.Collections.ObjectModel;
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
    public class OrderRecipientsViewModel : ViewModelBase
    {
        #region Properties        
        private ObservableCollection<OrderEmailRecipient> _Recipients;
        public ObservableCollection<OrderEmailRecipient> Recipients
        {
            get => _Recipients;
            set => Set(ref _Recipients, value);
        }

        private OrderEmailRecipient _selectedRecipient;
        public OrderEmailRecipient SelectedRecipient
        {
            get => _selectedRecipient;
            set => Set(ref _selectedRecipient, value);
        }

        private long _order;
        public long Order
        {
            get => _order;
            set
            {
                Set(ref _order, value);

                using (DBArchive db = new DBArchive())
                    Recipients = new ObservableCollection<OrderEmailRecipient>(db.OrderEmailRecipients.Where(r => r.Order == _order).ToList());
            }
        }

        private string _inputAddress;
        public string InputAddress
        {
            get => _inputAddress;
            set => Set(ref _inputAddress, value);
        }
        #endregion

        #region Command Definitions
        public RelayCommand<string> AddCommand { get; set; }
        public RelayCommand<OrderEmailRecipient> RemoveCommand { get; set; }
        #endregion

        public OrderRecipientsViewModel()
        {
            AddCommand = new RelayCommand<string>(AddRecipient);
            RemoveCommand = new RelayCommand<OrderEmailRecipient>(RemoveRecipient);

            Recipients = new ObservableCollection<OrderEmailRecipient>();
        }

        public void AddRecipient(string address)
        {
            string error;

            if (!MSExchangeProvider.CheckEmailAddress(address, out error))
            {
                MetroMessageBox.Show(error, "Invalid Email Address", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OrderEmailRecipient recipient = new OrderEmailRecipient();
            recipient.Address = address;
            recipient.Order = Order;

            using (DBArchive db = new DBArchive())
            {
                db.OrderEmailRecipients.Add(recipient);
                db.SaveChanges();
            }

            Recipients.Add(recipient);
            InputAddress = string.Empty;
        }

        public void RemoveRecipient(OrderEmailRecipient recipient)
        {
            using (DBArchive db = new DBArchive())
            {
                OrderEmailRecipient recipientToDelete = db.OrderEmailRecipients.Where(o => o.Address == recipient.Address && o.Order == recipient.Order).FirstOrDefault();
                db.OrderEmailRecipients.Remove(recipientToDelete);
                db.SaveChanges();
            }

            Recipients.Remove(recipient);
        }
    }
}