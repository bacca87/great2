using GalaSoft.MvvmLight;
using Great.Models.Database;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Great.ViewModels.Database
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
            {
                return Refresh(db);
            }
        }

        public abstract bool Refresh(DBArchive db);

        public bool Save()
        {
            using (DBArchive db = new DBArchive())
            {
                return Save(db);
            }
        }

        public abstract bool Save(DBArchive db);

        public bool Delete()
        {
            using (DBArchive db = new DBArchive())
            {
                return Delete(db);
            }
        }

        public abstract bool Delete(DBArchive db);

        public void CheckChangedEntity()
        {

            //incomplete management
            if (!IsChanged)
                return;

            if (MetroMessageBox.Show("Do you want to commit changes before leave selection?", "Save Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Save();
            else
            {
                Refresh();
            }

            IsChanged = false;
        }

        protected bool SetAndCheckChanged<T>(ref T field, T newValue = default, bool broadcast = false, [CallerMemberName] string propertyName = null)
        {


            IsChanged = !EqualityComparer<T>.Default.Equals(field, newValue);

            return Set(ref field, newValue);
        }


    }
}
