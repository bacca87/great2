using GalaSoft.MvvmLight;
using Great.Models.Database;
using System.ComponentModel;

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

        public bool IsChanged { get; set; }
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
        #endregion
    }
}
