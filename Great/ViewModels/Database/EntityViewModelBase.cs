using GalaSoft.MvvmLight;
using Great.Models.Database;

namespace Great.ViewModels.Database
{
    public abstract class EntityViewModelBase : ViewModelBase
    {
        public bool Save()
        {
            using (DBArchive db = new DBArchive())
            {
                Save(db);
                return db.SaveChanges() > 0;
            }
        }

        public abstract bool Save(DBArchive db);

        public bool Delete()
        {
            using (DBArchive db = new DBArchive())
            {
                Delete(db);
                return db.SaveChanges() > 0;
            }
        }

        public abstract bool Delete(DBArchive db);
    }
}
