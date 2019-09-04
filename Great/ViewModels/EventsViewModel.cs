﻿using GalaSoft.MvvmLight;
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
    public class EventsViewModel : ViewModelBase
    {
        #region Properties
        MSSharepointProvider _provider;

        private bool _isInputEnabled = false;
        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set =>  Set(ref _isInputEnabled, value);
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
                 Set(ref _ShowHourTimeFields, value);
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
        public RelayCommand<EventEVM> MarkAsAcceptedCommand { get; set; }
        public RelayCommand<EventEVM> MarkAsCancelledCommand { get; set; }
        public RelayCommand<EventEVM> DeleteCommand { get; set; }
        public RelayCommand GotFocusCommand { get; set; }
        public RelayCommand LostFocusCommand { get; set; }

        public RelayCommand PageLoadedCommand { get; set; }
        public RelayCommand PageUnloadedCommand { get; set; }

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
            PageLoadedCommand = new RelayCommand(PageLoaded);
            PageUnloadedCommand = new RelayCommand(PageUnloaded);

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

        private void PageUnloaded()
        {
            var changed = Events.Where(x => x.IsChanged).ToList();
            //add also new not saved valid entities
            if (SelectedEvent != null && SelectedEvent.Id == 0)
                changed.Add(SelectedEvent);

            if (changed.Count() == 0) return;

            if (MetroMessageBox.Show("You changed page without saving. Do you want to commit changes?", "Save Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                changed.ToList().ForEach(x => { if (x.IsValid) x.Save(); else x.RejectChanges(); });
            }

            else
            {
                changed.ToList().ForEach(x => { x.RejectChanges(); });
            }

        }

        private void PageLoaded()
        {
            Events.ToList().ForEach(x => x.IsChanged = false);
        }

        public void ClearEvent()
        {
            SelectedEvent.StartDate = DateTime.Now;
            SelectedEvent.EndDate = DateTime.Now;
            SelectedEvent.Description = string.Empty;
        }

        public void SaveEvent(EventEVM ev)
        {
            if (ev == null) return;
            if (ev.EStatus != EEventStatus.Pending)
            {
                MetroMessageBox.Show("Cannot save/edit the event because it is not in pending state", "Save Event", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ev.IsValid)
            {
                MetroMessageBox.Show("Cannot save/edit the event. Please check the errors", "Save Event",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            if (MetroMessageBox.Show("Are you sure to save the selected event? It will update the intranet calendar", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;


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
            SelectedEvent.EndDate = DateTime.Now;
            SelectedEvent.StartDate = DateTime.Now;

        }


        #endregion
    }
}
