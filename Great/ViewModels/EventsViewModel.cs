using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils;
using Great.Utils.Extensions;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Great.ViewModels
{
    public class EventsViewModel : ViewModelBase
    {
        #region Properties
        private bool _isInputEnabled = false;
        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set
            {
                if (_isInputEnabled == value)
                {
                    return;
                }

                var oldValue = _isInputEnabled;
                _isInputEnabled = value;

                RaisePropertyChanged(nameof(IsInputEnabled), oldValue, value);

            }
        }

        private bool _showContextualMenu = false;
        public bool ShowContextualMenu
        {
            get => _showContextualMenu;

            set
            {
                if (_showContextualMenu == value)
                {
                    return;
                }

                var oldValue = _showContextualMenu;
                _showContextualMenu = value;

                RaisePropertyChanged(nameof(ShowContextualMenu), oldValue, value);

            }
        }

        private ObservableCollectionEx<EventEVM> _Events;
        public ObservableCollectionEx<EventEVM> Events
        {
            get => _Events;
            set => Set(ref _Events, value);
        }
        public ObservableCollection<EventTypeDTO> EventTypes { get; set; }
        private EventEVM _SelectedEvent;
        public EventEVM SelectedEvent
        {
            get => _SelectedEvent;
            set
            {
                Set(ref _SelectedEvent, value);

                if (_SelectedEvent != null)
                {
                    BeginHour = _SelectedEvent.StartDate.Hour;
                    BeginMinutes = _SelectedEvent.StartDate.Minute;
                    EndHour = _SelectedEvent.EndDate.Hour;
                    EndMinutes = _SelectedEvent.EndDate.Minute;
                    IsInputEnabled = true;
                }
                else
                    IsInputEnabled = false;
            }
        }

        public IList<int> Hours { get; set; }
        public IList<int> Minutes { get; set; }

        private int _BeginHour;
        public int BeginHour
        {
            get => _BeginHour;
            set => Set(ref _BeginHour, value);
        }

        private int _EndHour;
        public int EndHour
        {
            get => _EndHour;
            set => Set(ref _EndHour, value);
        }

        private int _BeginMinutes;
        public int BeginMinutes
        {
            get => _BeginMinutes;
            set => Set(ref _BeginMinutes, value);
        }


        private int _EndMinutes;
        public int EndMinutes
        {
            get => _EndMinutes;
            set => Set(ref _EndMinutes, value);
        }

        #endregion

        #region Commands Definitions
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<EventEVM> SaveCommand { get; set; }
        public RelayCommand<EventEVM> NewCommand { get; set; }
        public RelayCommand<EventEVM> MarkAsAcceptedCommand { get; set; }
        public RelayCommand<EventEVM> MarkAsCancelledCommand { get; set; }
        public RelayCommand<EventEVM> DeleteCommand { get; set; }
        public RelayCommand ShowContextualMenuCommand { get; set; }

        #endregion

        public EventsViewModel()
        {
            IsInputEnabled = true;

            Minutes = new List<int>();
            Hours = new List<int>();

            for (int i = 0; i < 24; i++)
                Hours.Add(i);

            for (int i = 0; i < 60; i = i + 5)
                Minutes.Add(i);

            ClearCommand = new RelayCommand(ClearEvent, () => { return IsInputEnabled; });
            SaveCommand = new RelayCommand<EventEVM>(SaveEvent, (EventEVM v) => { return IsInputEnabled; });
            MarkAsAcceptedCommand = new RelayCommand<EventEVM>(MarkAsAccepted);
            MarkAsCancelledCommand = new RelayCommand<EventEVM>(MarkAsCancelled);
            DeleteCommand = new RelayCommand<EventEVM>(DeleteEvent);
            NewCommand = new RelayCommand<EventEVM>(AddEEvent);
            ShowContextualMenuCommand = new RelayCommand(() => { ShowContextualMenu = true; });

            using (DBArchive db = new DBArchive())
            {
                EventTypes = new ObservableCollection<EventTypeDTO>(db.EventTypes.ToList().Select(e => new EventTypeDTO(e)));
                Events = new ObservableCollectionEx<EventEVM>(db.Events.ToList().Select(v => new EventEVM(v)));
            }

            MessengerInstance.Register<ItemChangedMessage<EventEVM>>(this, EventChanged);
        }


        public void ClearEvent()
        {
            SelectedEvent.StartDate = DateTime.Now;
            SelectedEvent.EndDate = DateTime.Now;
            SelectedEvent.Description = string.Empty;
        }

        public void SaveEvent(EventEVM ev)
        {
            if (ev == null)
                return;

            ev.StartDate = ev.StartDate.AddHours(BeginHour).AddMinutes(BeginMinutes);
            ev.EndDate = ev.EndDate.AddHours(EndHour).AddMinutes(EndMinutes);
            ev.Days = null;
            ev.Save();

            if (!Events.Any(e => e.Id == ev.Id)) Events.Add(ev);

            Messenger.Default.Send(new NewItemMessage<EventEVM>(this, ev));
        }
        public void MarkAsAccepted(EventEVM ev)
        {
            if (ev == null)
                return;

            if (MessageBox.Show("Are you sure to mark as accepted the selected Vacation?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ev.EStatus = EEventStatus.Accepted;
            ev.Save();
            Messenger.Default.Send(new ItemChangedMessage<EventEVM>(this, ev));
        }
        public void MarkAsCancelled(EventEVM ev)
        {
            if (ev == null)
                return;

            if (MessageBox.Show("Are you sure to mark as Cancelled the selected Vacation?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ev.EStatus = EEventStatus.Cancelled;
            ev.Save();
            Messenger.Default.Send(new DeletedItemMessage<EventEVM>(this, ev));
        }
        public void DeleteEvent(EventEVM ev)
        {
            if (ev == null)
                return;

            if (MessageBox.Show("Are you sure to send a cancellation request for the selected Vacation?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            //_fdlManager.SendCancellationRequest(fdl);
            ev.EStatus = EEventStatus.Cancelled;
            ev.Save();
            Messenger.Default.Send(new DeletedItemMessage<EventEVM>(this, ev));
        }
        public void EventChanged(ItemChangedMessage<EventEVM> item)
        {
            if (item.Content != null)
            {
                EventEVM v = Events.SingleOrDefault(x => x.Id == item.Content.Id);

                //update also approver andd date of approval!
                    v.EStatus = item.Content.EStatus;
                    v.Save();
            }
        }
        public void AddEEvent(EventEVM ev)
        {
            IsInputEnabled = true;
            SelectedEvent = new EventEVM();
            SelectedEvent.StartDate = DateTime.Now;
            SelectedEvent.EndDate = DateTime.Now;

        }
    }
}
