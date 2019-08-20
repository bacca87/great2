﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Great.Controls;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Great.ViewModels
{
    public class FactoriesViewModel : ViewModelBase
    {
        #region Properties
        public ObservableCollection<TransferTypeDTO> TransferTypes { get; set; }

        public ObservableCollectionEx<FactoryEVM> Factories { get; set; }

        private FactoryEVM _selectedFactory;
        public FactoryEVM SelectedFactory
        {
            get => _selectedFactory;
            set => Set(ref _selectedFactory, value ?? new FactoryEVM());
        }

        public Action<FactoryEVM> OnZoomOnFactoryRequest { get; set; }
        #endregion

        #region Commands
        public RelayCommand<FactoryEVM> DeleteFactoryCommand { get; set; }
        public RelayCommand<FactoryEVM> SaveFactoryCommand { get; set; }
        public RelayCommand ClearSelectionCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the FactoriesViewModel class.
        /// </summary>
        public FactoriesViewModel()
        {
            using (DBArchive db = new DBArchive())
            {
                Factories = new ObservableCollectionEx<FactoryEVM>(db.Factories.ToList().Select(f => new FactoryEVM(f)));
                TransferTypes = new ObservableCollection<TransferTypeDTO>(db.TransferTypes.ToList().Select(t => new TransferTypeDTO(t)));
            }

            DeleteFactoryCommand = new RelayCommand<FactoryEVM>(DeleteFactory);
            SaveFactoryCommand = new RelayCommand<FactoryEVM>(SaveFactory);
            ClearSelectionCommand = new RelayCommand(ClearSelection);

            MessengerInstance.Register<NewItemMessage<FactoryEVM>>(this, NewFactory);
        }

        public void NewFactory(NewItemMessage<FactoryEVM> item)
        {
            // Using the dispatcher for preventing thread conflicts
            Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    if (item.Content != null && !Factories.Any(f => f.Id == item.Content.Id))
                        Factories.Add(item.Content);
                })
            );
        }

        public void ZoomOnFactoryRequest(FactoryEVM factory)
        {
            OnZoomOnFactoryRequest?.Invoke(factory);
        }

        private void ClearSelection()
        {
            SelectedFactory = null;
        }

        private void DeleteFactory(FactoryEVM factory)
        {
            using (DBArchive db = new DBArchive())
            {
                int fdlCount = db.FDLs.Count(f => f.Factory.HasValue && f.Factory.Value == factory.Id);

                if (fdlCount > 0)
                {
                    MetroMessageBox.Show($"The factory {factory.Name} is bound to {fdlCount} FDLs!\nBefore deleting a factory, you must unbound all the realted FDLs!\nOperation cancelled!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (factory.Delete(db))
                {
                    Factories.Remove(factory);
                    SelectedFactory = null;

                    Messenger.Default.Send(new DeletedItemMessage<FactoryEVM>(this, factory));
                }
            }   
        }

        private void SaveFactory(FactoryEVM factory)
        {
            factory.NotifyAsNew = false;

            if (factory.Save())
            {
                if (!Factories.Contains(factory))
                {
                    Factories.Add(factory);
                    Messenger.Default.Send(new NewItemMessage<FactoryEVM>(this, factory));
                }
                else
                    Messenger.Default.Send(new ItemChangedMessage<FactoryEVM>(this, factory));

                SelectedFactory = factory;
            }
        }
    }
}