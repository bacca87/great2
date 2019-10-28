using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great2.Models;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Utils;
using Great2.Utils.Extensions;
using Great2.Utils.Messages;
using Great2.ViewModels.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Great2.ViewModels
{
    public class EventsViewModel : ViewModelBase
    {
        #region Properties
        MSSharepointProvider _provider;


        private int _currentYear = DateTime.Now.Year;
        public int CurrentYear
        {
            get => _currentYear;
            set
            {
                bool updateDays = _currentYear != value;
                int year = 0;

                if (value < ApplicationSettings.Timesheets.MinYear)
                    year = ApplicationSettings.Timesheets.MinYear;
                else if (value > ApplicationSettings.Timesheets.MaxYear)
                    year = ApplicationSettings.Timesheets.MaxYear;
                else
                    year = value;

                Set(ref _currentYear, year);

                if (updateDays)
                    UpdateEventList();
            }
        }

        private bool _isInputEnabled = false;
        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set => Set(ref _isInputEnabled, value);
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
                if (SelectedEvent != null)
                    Set(ref _ShowHourTimeFields, value);
            }
        }

        private bool _showOnlyVacations = false;
        public bool ShowOnlyVacations
        {
            get => _showOnlyVacations;
            set
            {
                Set(ref _showOnlyVacations, value);
                _FilteredEvents.Refresh();
            }
        }

        private ObservableCollectionEx<EventEVM> _Events;
        public ObservableCollectionEx<EventEVM> Events
        {
            get => _Events;
            set => Set(ref _Events, value);
        }

        private ICollectionView _FilteredEvents;
        public ICollectionView FilteredEvents
        {
            get => _FilteredEvents;
            set => Set(ref _FilteredEvents, value);
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
                _SelectedEvent?.CheckChangedEntity();

                Set(ref _SelectedEvent, value);

                if (_SelectedEvent != null)
                {
                    _SelectedEvent.PropertyChanged -= _SelectedEvent_PropertyChanged;
                    _SelectedEvent.PropertyChanged += _SelectedEvent_PropertyChanged;

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


        #endregion

        #region Commands Definitions
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<EventEVM> SaveCommand { get; set; }
        public RelayCommand<EventEVM> NewCommand { get; set; }
        public RelayCommand<EventEVM> DeleteCommand { get; set; }
        public RelayCommand GotFocusCommand { get; set; }
        public RelayCommand LostFocusCommand { get; set; }
        public RelayCommand PageUnloadedCommand { get; set; }
        public RelayCommand NextYearCommand { get; set; }
        public RelayCommand PreviousYearCommand { get; set; }

        #endregion

        #region Constructors
        public EventsViewModel(MSSharepointProvider pro)
        {
            IsInputEnabled = true;
            _provider = pro;

            Minutes = new List<int>();
            Hours = new List<int>();

            var mindatefilter = new DateTime(CurrentYear, 1, 1).ToUnixTimestamp();
            var maxdatefilter = new DateTime(CurrentYear, 12, 31).ToUnixTimestamp();

            for (int i = 0; i < 24; i++)
                Hours.Add(i);

            for (int i = 0; i < 60; i = i + 5)
                Minutes.Add(i);

            ClearCommand = new RelayCommand(ClearEvent, () => { return IsInputEnabled; });
            SaveCommand = new RelayCommand<EventEVM>(SaveEvent, (EventEVM v) => { return IsInputEnabled; });
            DeleteCommand = new RelayCommand<EventEVM>(DeleteEvent);
            NewCommand = new RelayCommand<EventEVM>(AddEvent);
            GotFocusCommand = new RelayCommand(() => { ShowEditMenu = true; });
            LostFocusCommand = new RelayCommand(() => { });
            PageUnloadedCommand = new RelayCommand(() => { SelectedEvent?.CheckChangedEntity(); });


            using (DBArchive db = new DBArchive())
            {
                EventTypes = new ObservableCollection<EventTypeDTO>(db.EventTypes.ToList().Select(e => new EventTypeDTO(e)));
                Events = new ObservableCollectionEx<EventEVM>(db.Events.Where(e => e.StartDateTimeStamp >= mindatefilter && e.EndDateTimeStamp <= maxdatefilter).ToList().Select(e => new EventEVM(e)));
            }

            NextYearCommand = new RelayCommand(() => { CurrentYear++; FilteredEvents.Refresh(); });
            PreviousYearCommand = new RelayCommand(() => { CurrentYear--; FilteredEvents.Refresh(); });


            MessengerInstance.Register<ItemChangedMessage<EventEVM>>(this, EventChanged);
            MessengerInstance.Register<NewItemMessage<EventEVM>>(this, EventImportedFromCalendar);
            MessengerInstance.Register<DeletedItemMessage<EventEVM>>(this, EventDeleted);

            FilteredEvents = CollectionViewSource.GetDefaultView(_Events);
            SortDescription sd = new SortDescription("StartDate", ListSortDirection.Descending);
            FilteredEvents.SortDescriptions.Add(sd);
            FilteredEvents.Filter += Filter;
            FilteredEvents.MoveCurrentToFirst();
            SelectedEvent = (EventEVM)_FilteredEvents.CurrentItem;
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
            if (ev == null) return;

            if (!ev.IsValid)
            {
                MetroMessageBox.Show("Cannot save/edit the event. Please check the errors", "Save Event", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MetroMessageBox.Show("Are you sure to save the selected event? It will update the intranet calendar", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            ev.EStatus = EEventStatus.Pending;
            ev.IsSent = false;
            ev.Save();
            // ev.AddOrUpdateEventRelations();

            //if the event is new
            if (!Events.Any(x => x.Id == ev.Id))

                Events.Add(ev);

            ShowEditMenu = false;
            FilteredEvents.Refresh();

        }

        public void DeleteEvent(EventEVM ev)
        {

            if (MetroMessageBox.Show("Are you sure to delete the selected event? It will update the intranet calendar", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;
            if (ev.SharePointId > 0)
            {
                ev.IsSent = false;
                ev.EStatus = EEventStatus.Pending;
                ev.IsCancelRequested = true;
                ev.Save();
            }

            else
            {
                ev.Delete();
                Events.Remove(ev);
            }
            FilteredEvents.Refresh();
        }

        public void EventChanged(ItemChangedMessage<EventEVM> item)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
             new Action(() =>
             {
                 if (item.Content != null)
                 {
                     EventEVM v = Events.SingleOrDefault(x => x.Id == item.Content.Id);
                     if (v == null) return;

                     //if user change the event do not update the gui!
                     if (!v.IsChanged)
                     {
                         v.Refresh();
                         FilteredEvents.Refresh();
                     }
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
                          Events.Add(item.Content);
                          FilteredEvents.Refresh();
                      }
                  }
              }));
        }

        public void EventDeleted(DeletedItemMessage<EventEVM> item)
        {
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
             new Action(() =>
             {
                 if (item.Content != null)
                 {
                     EventEVM v = Events.SingleOrDefault(x => x.Id == item.Content.Id);
                     Events.Remove(v);
                     FilteredEvents.Refresh();
                 }
             }));
        }


        public void AddEvent(EventEVM ev)
        {
            IsInputEnabled = true;
            SelectedEvent = new EventEVM();
            SelectedEvent.EType = EEventType.Vacations;
            SelectedEvent.EStatus = EEventStatus.Pending;
            SelectedEvent.EndDate = DateTime.Now;
            SelectedEvent.StartDate = DateTime.Now;

        }


        public void UpdateEventList()
        {
            Events.Clear();

            var mindatefilter = new DateTime(CurrentYear, 1, 1).ToUnixTimestamp();
            var maxdatefilter = new DateTime(CurrentYear, 12, 31).ToUnixTimestamp();

            using (DBArchive db = new DBArchive())
            {
                (from e in db.Events
                 where e.StartDateTimeStamp >= mindatefilter && e.EndDateTimeStamp <= maxdatefilter
                 select e).ToList().ForEach(e => Events.Add(new EventEVM(e)));
            }

        }

        #endregion
    }
}
