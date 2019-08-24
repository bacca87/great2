using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Great.ViewModels
{
    public class EventsViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Properties
        MSSharepointProvider _provider;

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

        private bool _showEditMenu;
        public bool ShowEditMenu
        {
            get => _showEditMenu;
            set => Set(ref _showEditMenu, value);
        }

        private bool _ShowHourTimeFields = true;
        public bool ShowHourTimeFields
        {
            get => _ShowHourTimeFields;

            set
            {
                _ShowHourTimeFields = value;
                if (SelectedEvent != null)
                    RaisePropertyChanged(nameof(ShowHourTimeFields));
            }
        }

        private bool _showOnlyVacations = false;
        public bool ShowOnlyVacations
        {
            get => _showOnlyVacations;
            set
            {
                _showOnlyVacations = value;
                _FilteredEvents.Refresh();
                RaisePropertyChanged(nameof(ShowOnlyVacations));

            }
        }

        private ObservableCollectionEx<EventEVM> _Events;
        public ObservableCollectionEx<EventEVM> Events
        {
            get => _Events;
            set
            {
                Set(ref _Events, value);
            }

        }

        private ICollectionView _FilteredEvents;
        public ICollectionView FilteredEvents
        {
            get { return _FilteredEvents; }
        }

        public bool Filter(object ev)
        {
            EventEVM e = (EventEVM)ev;
            bool result = e.EType != EEventType.Vacations && !ShowOnlyVacations;
            result |= e.EType == EEventType.Vacations;
            return result;

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
                    _SelectedEvent.PropertyChanged -= _SelectedEvent_PropertyChanged;
                    Set(ref _SelectedEvent, value);
                    _SelectedEvent.PropertyChanged += _SelectedEvent_PropertyChanged;
                    BeginHour = _SelectedEvent.StartDate.Hour;
                    BeginMinutes = _SelectedEvent.StartDate.Minute;


                    EndHour = _SelectedEvent.EndDate.Hour;
                    EndMinutes = _SelectedEvent.EndDate.Minute;
                    ShowHourTimeFields = !_SelectedEvent.IsAllDay;

                    ShowEditMenu = false;
                }

            }
        }

        private void _SelectedEvent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == (nameof(SelectedEvent.IsAllDay)))
            {
                ShowHourTimeFields = !SelectedEvent.IsAllDay;
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
        public RelayCommand GotFocusCommand { get; set; }
        public RelayCommand LostFocusCommand { get; set; }

        #endregion

        #region Constructors
        public EventsViewModel(MSSharepointProvider pro)
        {
            IsInputEnabled = true;
            _provider = pro;

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
            GotFocusCommand = new RelayCommand(() => { ShowEditMenu = true; });
            LostFocusCommand = new RelayCommand(() => { });

            using (DBArchive db = new DBArchive())
            {
                EventTypes = new ObservableCollection<EventTypeDTO>(db.EventTypes.ToList().Select(e => new EventTypeDTO(e)));
                Events = new ObservableCollectionEx<EventEVM>(db.Events.ToList().Select(v => new EventEVM(v)));
            }

            MessengerInstance.Register<ItemChangedMessage<EventEVM>>(this, EventChanged);
            MessengerInstance.Register<NewItemMessage<EventEVM>>(this, EventImportedFromCalendar);

            _FilteredEvents = CollectionViewSource.GetDefaultView(Events);
            _FilteredEvents.Filter += Filter;
        }
        #endregion

        #region Methods
        public void ClearEvent()
        {
            SelectedEvent.StartDate = DateTime.Now;
            SelectedEvent.EndDate = DateTime.Now;
            SelectedEvent.Description = string.Empty;
        }

        public void SaveEvent(EventEVM ev)
        {
            if (ev == null || ev.EStatus != EEventStatus.Pending)
            {
                MetroMessageBox.Show("Cannot save/edit the event because it is approved", "Save Event", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MetroMessageBox.Show("Are you sure to save the selected event? It will update the intranet calendar", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ev.StartDate = new DateTime(ev.StartDate.Year, ev.StartDate.Month, ev.StartDate.Day, BeginHour, BeginMinutes, 0);
            ev.EndDate = new DateTime(ev.EndDate.Year, ev.EndDate.Month, ev.EndDate.Day, EndHour, EndMinutes, 0);


            if (ev.StartDate > ev.EndDate) return;

            ev.EStatus = EEventStatus.Pending;
            ev.IsSent = false;

            ev.Save();

            //if the event is new
            if (!Events.Any(x => x.Id == ev.Id))
            {
                ev.AddOrUpdateEventRelations();
                Events.Add(ev);
            }

            ShowEditMenu = false;
            //AddOrUpdateEventRelations(ev);

        }

        public void RequestCancellation(EventEVM ev)
        {
            if (ev == null || ev.EStatus == EEventStatus.Rejected)
            {
                MetroMessageBox.Show("Cannot cancel the event because it is already cancelled", "Save Event", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (MetroMessageBox.Show("Are you sure to delete the selected event? It will update the intranet calendar", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ev.IsSent = false;
            ev.EStatus = EEventStatus.Rejected;
            ev.Save();

        }

        public void MarkAsAccepted(EventEVM ev)
        {
            if (ev == null || ev.EStatus == EEventStatus.Accepted)
            {
                MetroMessageBox.Show("Cannot set to Accepted the event because it is already accepted", "Save Event", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MetroMessageBox.Show("Are you sure to mark as accepted the selected Vacation?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ev.EStatus = EEventStatus.Accepted;
            ev.IsSent = true;
            ev.Save();
            FilteredEvents.Refresh();
            _provider.UpdateEventStatus(ev);

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
            FilteredEvents.Refresh();
            _provider.UpdateEventStatus(ev);
        }

        public void EventChanged(ItemChangedMessage<EventEVM> item)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
             new Action(() =>
             {
                 if (item.Content != null)
                 {
                     EventEVM v = Events.SingleOrDefault(x => x.Id == item.Content.Id);

                     Global.Mapper.Map(item.Content, v);
                     v.Save();

                     FilteredEvents.Refresh();
                 }
             }));
        }

        public void EventImportedFromCalendar(NewItemMessage<EventEVM> item)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
              new Action(() =>
              {

                  if (item.Content != null)
                  {
                      if (!Events.Any(x => x.Id == item.Content.Id))
                      {
                          item.Content.AddOrUpdateEventRelations();
                          Events.Add(item.Content);
                          FilteredEvents.Refresh();
                      }
                  }
              }));
        }

        public void AddEvent(EventEVM ev)
        {
            IsInputEnabled = true;
            SelectedEvent = new EventEVM();
            SelectedEvent.EType = EEventType.Vacations;
            SelectedEvent.EStatus = EEventStatus.Pending;
            SelectedEvent.StartDate = DateTime.Now;
            SelectedEvent.EndDate = DateTime.Now;
        }

        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Title":
                        if (string.IsNullOrEmpty(SelectedEvent.Title) || string.IsNullOrWhiteSpace(SelectedEvent.Title))
                            return "Title of event must be set";

                        break;

                    case "StartDate":
                    case "EndDate":
                        if (SelectedEvent.StartDate > SelectedEvent.EndDate)
                            return "Time interval not valid";

                        break;

                    //case "ExpenseTypeText":
                    //    if (!string.IsNullOrEmpty(ExpenseTypeText) && !ExpenseTypes.Any(t => t.Description == ExpenseTypeText))
                    //        return "Select a valid expense type from the combo list!";
                    //    break;

                    default:
                        break;
                }

                return null;
            }
        }

        #endregion
    }
}
