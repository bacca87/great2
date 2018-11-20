using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using Great.Models.Database;
using Great.Utils;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        /// <summary>
        /// The <see cref="Recipients" /> property's name.
        /// </summary>
        private ObservableCollection<OrderEmailRecipient> _Recipients;

        /// <summary>
        /// Sets and gets the Recipients property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>        
        public ObservableCollection<OrderEmailRecipient> Recipients
        {
            get
            {
                return _Recipients;
            }
            set
            {
                _Recipients = value;
                RaisePropertyChanged(nameof(Recipients), true);
            }
        }

        /// <summary>
        /// The <see cref="SelectedRecipient" /> property's name.
        /// </summary>
        private OrderEmailRecipient _selectedRecipient;

        /// <summary>
        /// Sets and gets the SelectedRecipient property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public OrderEmailRecipient SelectedRecipient
        {
            get
            {
                return _selectedRecipient;
            }

            set
            {
                var oldValue = _selectedRecipient;
                _selectedRecipient = value;

                RaisePropertyChanged(nameof(SelectedRecipient), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="Order" /> property's name.
        /// </summary>
        private long _order;

        /// <summary>
        /// Sets and gets the Order property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public long Order
        {
            get
            {
                return _order;
            }

            set
            {
                var oldValue = _order;
                _order = value;

                Recipients = new ObservableCollection<OrderEmailRecipient>(_db.OrderEmailRecipients.Where(r => r.Order == _order).ToList());

                RaisePropertyChanged(nameof(Order), oldValue, value);
            }
        }

        /// <summary>
        /// The <see cref="InputAddress" /> property's name.
        /// </summary>
        private string _inputAddress;

        /// <summary>
        /// Sets and gets the InputAddress property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public string InputAddress
        {
            get
            {
                return _inputAddress;
            }

            set
            {
                var oldValue = _inputAddress;
                _inputAddress = value;
                RaisePropertyChanged(nameof(InputAddress), oldValue, value);
            }
        }

        private DBArchive _db;
        #endregion

        #region Command Definitions
        public RelayCommand<string> AddCommand { get; set; }
        public RelayCommand<OrderEmailRecipient> RemoveCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        public OrderRecipientsViewModel(DBArchive db)
        {
            _db = db;

            AddCommand = new RelayCommand<string>(AddRecipient);
            RemoveCommand = new RelayCommand<OrderEmailRecipient>(RemoveRecipient);

            Recipients = new ObservableCollection<OrderEmailRecipient>();
        }

        public void AddRecipient(string address)
        {
            string error;

            if (!MSExchangeProvider.CheckEmailAddress(address, out error))
            {
                MessageBox.Show(error, "Invalid Email Address", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OrderEmailRecipient recipient = new OrderEmailRecipient();
            recipient.Address = address;
            recipient.Order = Order;

            _db.OrderEmailRecipients.Add(recipient);
            _db.SaveChanges();

            Recipients.Add(recipient);
            InputAddress = string.Empty;
        }

        public void RemoveRecipient(OrderEmailRecipient recipient)
        {
            _db.OrderEmailRecipients.Remove(recipient);
            _db.SaveChanges();

            Recipients.Remove(recipient);
        }
    }
}