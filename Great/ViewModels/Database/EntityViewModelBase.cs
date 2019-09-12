using GalaSoft.MvvmLight;
using Great.Models.Database;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Great.ViewModels.Database
{
    public abstract class EntityViewModelBase : ViewModelBase, IRevertibleChangeTracking
    {
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

        #region IRevertibleChangesTracking

        public bool IsChanged { get; protected set; }
        public void AcceptChanges()
        {
            IsChanged = false;
        }

        public void RejectChanges()
        {
            using (DBArchive DB = new DBArchive())
            {
                Refresh();
                IsChanged = false;
            }
        }

        public void CheckChangedEntity()
        {
            if (!IsChanged) return;

            if (MetroMessageBox.Show("Do you want to commit changes before leave selecion?", "Save Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Save();
            else
                RejectChanges();


        }

        public bool SetAndCheckChanged<T>(ref T field, T newValue = default, bool broadcast = false, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
                IsChanged = true;

             return base.Set(ref field, newValue);
        }
        #endregion
    }
}
