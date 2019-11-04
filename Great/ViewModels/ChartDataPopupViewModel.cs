using GalaSoft.MvvmLight;
using Great2.Models.Database;
using Great2.Utils;
using Great2.Utils.Extensions;
using Great2.ViewModels.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great2.ViewModels
{
    public class ChartDataPopupViewModel : ViewModelBase
    {
        #region Properties

        private EChartType _chartType;
        public EChartType ChartType
        {
            get => _chartType;
            set => Set(ref _chartType, value);
        }

        private string _key;
        public string Key
        {
            get => _key;
            set => Set(ref _key, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            private set => Set(ref _title, value);
        }


        private int _year;
        public int Year
        {
            get => _year;
            set => Set(ref _year, value);
        }

        public ObservableCollectionEx<DayEVM> Days
        {

            get
            {
                ObservableCollectionEx<DayEVM> days = new ObservableCollectionEx<DayEVM>();
                string YearStr = Year.ToString();
                long startDate = new DateTime(Year, 1, 1).ToUnixTimestamp();
                long endDate = new DateTime(Year, 12, 31).ToUnixTimestamp();

                using (var db = new DBArchive())
                {
                    switch (ChartType)
                    {
                        case EChartType.DayByType:
                            {                            
                                switch (Key)
                                {
                                    case "Business Trip":
                                        Title = $"Day by Type Details: Business Trips";
                                        var values = (from d in db.Days
                                                      from ts in d.Timesheets
                                                      where ts.FDL1 != null
                                                  && ts.FDL1.Id.Substring(0, 4) == YearStr
                                                      select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;
                                    case "Vacations":

                                        Title = $"Day by Type Details: Vacations";
                                        values = (from d in db.Days
                                                  where d.DayType.Id == (long)EDayType.VacationDay && d.Timestamp >= startDate && d.Timestamp <= endDate
                                                  select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;

                                    case "Office":
                                        Title = $"Day by Type Details: Office";
                                        values = (from d in db.Days
                                                  from ts in d.Timesheets
                                                  where d.DayType.Id == (long)EDayType.WorkDay && ts.FDL1 == null && d.Timestamp >= startDate && d.Timestamp <= endDate
                                                  select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;

                                    case "Home Work":

                                        Title = $"Day by Type Details: Home Working";
                                        values = (from d in db.Days
                                                  where d.DayType.Id == (long)EDayType.HomeWorkDay && d.Timestamp >= startDate && d.Timestamp <= endDate
                                                  select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;

                                    case "Sick Leave":

                                        Title = $"Day by Type Details: Sick Leave";
                                        values = (from d in db.Days
                                                  where d.DayType.Id == (long)EDayType.SickLeave && d.Timestamp >= startDate && d.Timestamp <= endDate
                                                  select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;

                                    case "Special Leave":

                                        Title = $"Day by Type Details: Special Leave";
                                        values = (from d in db.Days
                                                  where d.DayType.Id == (long)EDayType.SpecialLeave && d.Timestamp >= startDate && d.Timestamp <= endDate
                                                  select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;

                                    case null:
                                        return null;
                                }
                                break;
                            }

                        case EChartType.Factories:
                            {
                                Title = $"Factory {Key}: Details";
                                var values = (from fdl in db.FDLs
                                              from timesheets in fdl.Timesheets
                                              where fdl.Id.Substring(0, 4) == YearStr && fdl.Factory1 != null
                                              && fdl.Factory1.Name == Key
                                              select timesheets.Day);

                                values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                return days;

                            }

                        case EChartType.FactoriesByType:
                            {
                                switch (Key)
                                {
                                    case "Italy":
                                        Title = $"Factories in Italy: Details";
                                        var values = (from d in db.Days
                                                      from ts in d.Timesheets
                                                      where ts.FDL1 != null
                                                      && ts.FDL1.Id.Substring(0, 4) == YearStr
                                                      && ts.FDL1.Factory1.TransferType1.Id == 1
                                                      select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;

                                    case "No Transfer":
                                        Title = $"No Transfer Factories: Details";
                                        values = (from d in db.Days
                                                  from ts in d.Timesheets
                                                  where ts.FDL1 != null
                                                  && ts.FDL1.Id.Substring(0, 4) == YearStr
                                                  && ts.FDL1.Factory1.TransferType1.Id == 0
                                                  select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;

                                    case "Europe":

                                        Title = $"Factories in Europe: Details";
                                        values = (from d in db.Days
                                                  from ts in d.Timesheets
                                                  where ts.FDL1 != null
                                                  && ts.FDL1.Id.Substring(0, 4) == YearStr
                                                  && ts.FDL1.Factory1.TransferType1.Id == 2
                                                  select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;

                                    case "Extra Europe":

                                        Title = $"Factories in Extra Europe: Details";
                                        values = (from d in db.Days
                                                  from ts in d.Timesheets
                                                  where ts.FDL1 != null
                                                  && ts.FDL1.Id.Substring(0, 4) == YearStr
                                                  && ts.FDL1.Factory1.TransferType1.Id == 3
                                                  select d).Distinct();

                                        values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                        return days;

                                    case null:
                                        return null;
                                }
                                break;
                            }
                    }

                    return null;
                }
            }
        }
        #endregion
    }
}
