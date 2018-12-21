using GalaSoft.MvvmLight;
using Great.Models.Database;

namespace Great.ViewModels.Database
{
    public abstract class EntityViewModelBase : ViewModelBase
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
    }
}
