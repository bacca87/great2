using GalaSoft.MvvmLight;
using Great2.Models.Database;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Great2.ViewModels.Database
{
    public abstract class EntityViewModelBase : ViewModelBase, IChangeTracking
    {
        #region IChangeTracking
        public bool IsChanged { get; set; }
        public void AcceptChanges() => throw new System.NotImplementedException();
        #endregion

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
            //incomplete management
            if (!IsChanged)
                return;

            if (MetroMessageBox.Show("Do you want to save changes?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)           
                Save();
            else          
                Refresh();

            IsChanged = false;
        }

        protected bool SetAndCheckChanged<T>(ref T field, T newValue = default, bool broadcast = false, [CallerMemberName] string propertyName = null)
        {
            IsChanged |= !EqualityComparer<T>.Default.Equals(field, newValue);

            //Explicitly pass the propertyname and not the default CallMemberName:
            //When checking nested properties the parent name is passed and eventchanged is not fired
            return Set(ref field, newValue, propertyName);
        }
    }
}
