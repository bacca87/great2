using GalaSoft.MvvmLight;
using Great.Models.Database;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Great.ViewModels.Database
{
    public abstract class EntityViewModelBase : ViewModelBase
    {
        public bool IsChanged { get; protected set; }
        public EntityViewModelBase()
        {
            PropertyChanged += EntityViewModelBase_PropertyChanged;
        }

        private void EntityViewModelBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsChanged = true;
        }

        public bool Refresh()
        {
            using (DBArchive db = new DBArchive())
                return Refresh(db);
        }

        public abstract bool Refresh(DBArchive db);

        public bool Save()
        {
            using (DBArchive db = new DBArchive())
                return Save(db);
        }

        public abstract bool Save(DBArchive db);

        public bool Delete()
        {
            using (DBArchive db = new DBArchive())
                return Delete(db);
        }

        public abstract bool Delete(DBArchive db);

        public void CheckChangedEntity()
        {
            return;
            //incomplete management
            if (!IsChanged) return;

            if (MetroMessageBox.Show("Do you want to commit changes before leave selecion?", "Save Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Save();
            else
                Refresh();

            IsChanged = false;
        }

    }
}
