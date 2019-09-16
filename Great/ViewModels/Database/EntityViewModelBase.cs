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

        public bool IsChanged()
        {
            using (DBArchive db = new DBArchive())
                return IsChanged(db);
        }
        public abstract bool IsChanged(DBArchive db);

        public bool Delete()
        {
            using (DBArchive db = new DBArchive())
                return Delete(db);
        }

        public abstract bool Delete(DBArchive db);


        public void CheckChangedEntity()
        {
            if (!IsChanged()) return;

            if (MetroMessageBox.Show("Do you want to commit changes before leave selecion?", "Save Items", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Save();
            else
                Refresh();
        }

    }
}
