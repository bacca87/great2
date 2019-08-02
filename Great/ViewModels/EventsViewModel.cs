using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Great.Controls;
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
using static Great.Models.EventManager;
using EventManager = Great.Models.EventManager;

namespace Great.ViewModels
{
    public class EventsViewModel : ViewModelBase
    {
        #region Properties
        EventManager _eventManager;

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
                    IsInputEnabled = true;
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

        #endregion

        public EventsViewModel(EventManager evm)
        {
            _eventManager = evm;

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
            DeleteCommand = new RelayCommand<EventEVM>(RequestCancellation);
            NewCommand = new RelayCommand<EventEVM>(AddEvent);

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
            if (ev == null || ev.EStatus != EEventStatus.Pending)
                return;

            ev.StartDate = new DateTime(ev.StartDate.Year, ev.StartDate.Month, ev.StartDate.Day, BeginHour, BeginMinutes, 0);
            ev.EndDate = new DateTime(ev.EndDate.Year, ev.EndDate.Month, ev.EndDate.Day, EndHour, EndMinutes, 0);
            ev.Days = null;

            if (ev.StartDate > ev.EndDate) return;

            //id backup
            var Id = ev.Id;

            ev.Save();

            if (Id == 0)
            {
                _eventManager.Add(ev);
                Events.Add(ev);
            }

            else _eventManager.Update(ev);

        }
        public void RequestCancellation(EventEVM ev)
        {
            if (ev == null || ev.EStatus == EEventStatus.Rejected)
                return;

            _eventManager.Delete(ev);

        }

        public void MarkAsAccepted(EventEVM ev)
        {
            if (ev == null || ev.EStatus == EEventStatus.Accepted)
                return;

            if (MetroMessageBox.Show("Are you sure to mark as accepted the selected Vacation?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ev.EStatus = EEventStatus.Accepted;
            ev.IsSent = true;
            ev.Save();

        }
        public void MarkAsCancelled(EventEVM ev)
        {
            if (ev == null || ev.EStatus == EEventStatus.Rejected)
                return;

            if (MetroMessageBox.Show("Are you sure to mark as Cancelled the selected Vacation?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ev.EStatus = EEventStatus.Rejected;
            ev.IsSent = true;
            ev.Save();

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
        public void AddEvent(EventEVM ev)
        {
            IsInputEnabled = true;
            SelectedEvent = new EventEVM();
            SelectedEvent.EStatus = EEventStatus.Pending;
            SelectedEvent.StartDate = DateTime.Now;
            SelectedEvent.EndDate = DateTime.Now;
        }
    }
}
