using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great2.Models;
using Great2.Models.Database;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Great2.ViewModels
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
                {
                    if (db.OrderEmailRecipients.Where(r => r.Order == _order).Count() == 0 && UserSettings.Email.Recipients.NewOrderDefaults != null)
                    {
                        // add default recipients for the order
                        foreach (var recipient in UserSettings.Email.Recipients.NewOrderDefaults)
                            db.OrderEmailRecipients.Add(new OrderEmailRecipient() { Order = _order, Address = recipient });

                        db.SaveChanges();
                    }

                    Recipients = new ObservableCollection<OrderEmailRecipient>(db.OrderEmailRecipients.Where(r => r.Order == _order).ToList());
                }   
            }
        }

        private string _inputAddress;
        public string InputAddress
        {
            get => _inputAddress;
            set => Set(ref _inputAddress, value);
        }

        private bool _result;
        public bool Result
        {
            get => _result;
            set => Set(ref _result, value);
        }
        #endregion

        #region Command Definitions
        public RelayCommand<string> AddCommand { get; set; }
        public RelayCommand<OrderEmailRecipient> RemoveCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand SendCommand { get; set; }
        #endregion

        #region Actions
        public Action Close { get; set; }
        #endregion

        public OrderRecipientsViewModel()
        {
            AddCommand = new RelayCommand<string>(AddRecipient);
            RemoveCommand = new RelayCommand<OrderEmailRecipient>(RemoveRecipient);
            CancelCommand = new RelayCommand(Cancel);
            SendCommand = new RelayCommand(Send);

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

            if(address == ApplicationSettings.EmailRecipients.FDLSystem || address == ApplicationSettings.EmailRecipients.HR)
            {
                MetroMessageBox.Show("FDL system and HR recipients are already included by default. Please add only the additional recipients!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                InputAddress = string.Empty;
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

        public void Cancel()
        {
            using (DBArchive db = new DBArchive())
            {
                db.OrderEmailRecipients.RemoveRange(db.OrderEmailRecipients.Where(r => r.Order == Order));
                db.SaveChanges();
            }

            Result = false;
            Close();
        }

        public void Send()
        {
            Result = true;
            Close();
        }
    }
}