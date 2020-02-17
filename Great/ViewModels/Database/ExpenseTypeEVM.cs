using System;
using Great2.Models.Database;

namespace Great2.ViewModels.Database
{
    public class ExpenseTypeEVM : EntityViewModelBase
    {
        #region Properties
        public long _Id;
        public long Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        public string _Description;
        public string Description
        {
            get => _Description;
            set => Set(ref _Description, value);
        }

        public long _Category;
        public long Category
        {
            get => _Category;
            set => Set(ref _Category, value);
        }
        #endregion

        public ExpenseTypeEVM() { }

        public ExpenseTypeEVM(ExpenseType type = null)
        {
            if (type != null)
                Auto.Mapper.Map(type, this);

            IsChanged = false;
        }

        public override bool Delete(DBArchive db)
        {
            throw new NotImplementedException();
        }

        public override bool Refresh(DBArchive db)
        {
            throw new NotImplementedException();
        }

        public override bool Save(DBArchive db)
        {
            throw new NotImplementedException();
        }
    }
}
