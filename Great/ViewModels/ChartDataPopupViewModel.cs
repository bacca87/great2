using GalaSoft.MvvmLight;
using Great2.Models;
using Great2.Models.Database;
using Great2.Utils;
using Great2.Utils.Extensions;
using Great2.ViewModels.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great2.ViewModels
{
    public class ChartDataPopupViewModel : ViewModelBase
    {
        #region Properties

        public ObservableCollectionEx<DayEVM> BusinessTripDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 from ts in d.Timesheets
                                 where ts.FDL1 != null && ts.FDL1.Id.Substring(0, 4) == YearStr && ts.FDL1.Status == (int)EFDLStatus.Accepted
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> VacationDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 where d.DayType.Id == (long)EDayType.VacationDay && d.Timestamp >= StartDate && d.Timestamp <= EndDate
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> OfficeDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 from ts in d.Timesheets
                                 where d.DayType.Id == (long)EDayType.WorkDay && ts.FDL1 == null && d.Timestamp >= StartDate && d.Timestamp <= EndDate
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> HomeWorkingDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 where d.DayType.Id == (long)EDayType.HomeWorkDay && d.Timestamp >= StartDate && d.Timestamp <= EndDate
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> SickLeaveDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 where d.DayType.Id == (long)EDayType.SickLeave && d.Timestamp >= StartDate && d.Timestamp <= EndDate
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> SpecialLeaveDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 where d.DayType.Id == (long)EDayType.SpecialLeave && d.Timestamp >= StartDate && d.Timestamp <= EndDate
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> ItalyTransferDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 from ts in d.Timesheets
                                 where ts.FDL1 != null && ts.FDL1.Status == (int)EFDLStatus.Accepted
                                 && ts.FDL1.Id.Substring(0, 4) == YearStr
                                 && ts.FDL1.Factory1.TransferType1.Id == 1
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> NoTransferDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 from ts in d.Timesheets
                                 where ts.FDL1 != null && ts.FDL1.Status == (int)EFDLStatus.Accepted
                                 && ts.FDL1.Id.Substring(0, 4) == YearStr
                                 && ts.FDL1.Factory1.TransferType1.Id == 0
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> ExtraEuropeDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 from ts in d.Timesheets
                                 where ts.FDL1 != null && ts.FDL1.Status == (int)EFDLStatus.Accepted
                                 && ts.FDL1.Id.Substring(0, 4) == YearStr
                                 && ts.FDL1.Factory1.TransferType1.Id == 3
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> EuropeDays
        {
            get
            {
                ObservableCollectionEx<DayEVM> result = new ObservableCollectionEx<DayEVM>();
                using (var db = new DBArchive())
                {
                    var items = (from d in db.Days
                                 from ts in d.Timesheets
                                 where ts.FDL1 != null && ts.FDL1.Status == (int)EFDLStatus.Accepted
                                 && ts.FDL1.Id.Substring(0, 4) == YearStr
                                 && ts.FDL1.Factory1.TransferType1.Id == 2
                                 select d).Distinct().ToList();

                    items.ForEach(x => result.Add(new DayEVM(x)));
                }
                return result;
            }
        }
        public ObservableCollectionEx<DayEVM> TotalDaysWorked
        {
            get => new ObservableCollectionEx<DayEVM>(BusinessTripDays.Union(OfficeDays).Where(x => x.TotalTime > 0));
        }
        public ObservableCollectionEx<DayEVM> TotalHolidaysWorked
        {
            get => new ObservableCollectionEx<DayEVM>(TotalDaysWorked.Where(x => x.IsHoliday && x.TotalTime > 0));
        }
        public ObservableCollectionEx<DayEVM> TotalSaturdaysWorked
        {
            get => new ObservableCollectionEx<DayEVM>(TotalDaysWorked.Where(x => x.Date.DayOfWeek == DayOfWeek.Saturday && x.TotalTime > 0));
        }
        public ObservableCollectionEx<DayEVM> TotalSundaysWorked
        {
            get => new ObservableCollectionEx<DayEVM>(TotalDaysWorked.Where(x => x.Date.DayOfWeek == DayOfWeek.Sunday && x.TotalTime > 0));
        }

        private EChartType _chartType;
        public EChartType ChartType
        {
            get => _chartType;
            set => Set(ref _chartType, value);
        }

        private List<string> _selectedTags;
        public List<string> SelectedTags
        {
            get => _selectedTags;
            set => Set(ref _selectedTags, value);
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

        private bool _hyperlinksEnabled;
        public bool HyperlinksEnabled
        {
            get => _hyperlinksEnabled;
            set => Set(ref _hyperlinksEnabled, value);
        }

        public string YearStr { get => Year.ToString(); }

        public long StartDate { get => new DateTime(Year, 1, 1).ToUnixTimestamp(); }
        public long EndDate { get => new DateTime(Year, 12, 31).ToUnixTimestamp(); }

        public ObservableCollectionEx<DayEVM> Days
        {
            get
            {
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
                                        return BusinessTripDays;

                                    case "Vacations":
                                        Title = $"Day by Type Details: Vacations";
                                        return VacationDays;

                                    case "Office":
                                        Title = $"Day by Type Details: Office";
                                        return OfficeDays;

                                    case "Home Work":
                                        Title = $"Day by Type Details: Home Working";
                                        return HomeWorkingDays;

                                    case "Sick Leave":
                                        Title = $"Day by Type Details: Sick Leave";
                                        return SickLeaveDays;

                                    case "Special Leave":
                                        Title = $"Day by Type Details: Special Leave";
                                        return SpecialLeaveDays;

                                    case null:
                                        return null;
                                }
                                break;
                            }

                        case EChartType.Factories:
                            {
                                ObservableCollectionEx<DayEVM> days = new ObservableCollectionEx<DayEVM>();
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
                                        return ItalyTransferDays;

                                    case "No Transfer":
                                        Title = $"No Transfer Factories: Details";
                                        return NoTransferDays;

                                    case "Europe":
                                        Title = $"Factories in Europe: Details";
                                        return EuropeDays;

                                    case "Extra Europe":
                                        Title = $"Factories in Extra Europe: Details";
                                        return ExtraEuropeDays;

                                    case null:
                                        return null;
                                }
                                break;
                            }

                        case EChartType.Tile:
                            {
                                switch (Key.ToLower())
                                {
                                    case "days worked":
                                        Title = $"Days Worked: Details";
                                        return TotalDaysWorked;

                                    case "holidays worked":
                                        Title = $"Holidays Worked: Details";
                                        return TotalHolidaysWorked;

                                    case "saturdays worked":
                                        Title = $"Saturdays Worked: Details";
                                        return TotalSaturdaysWorked;

                                    case "sundays worked":
                                        Title = $"Sundays Worked: Details";
                                        return TotalSundaysWorked;

                                    case null:
                                        return null;
                                }
                                break;
                            }

                        case EChartType.WorldMap:
                            {

                                ObservableCollectionEx<DayEVM> days = new ObservableCollectionEx<DayEVM>();
                                Title = $"Factory {Key}: Details";
                                var values = (from fdl in db.FDLs
                                              from timesheets in fdl.Timesheets
                                              where fdl.Id.Substring(0, 4) == YearStr && fdl.Factory1 != null
                                              && fdl.Factory1.CountryCode == Key
                                              select timesheets.Day);

                                values.ToList().ForEach(x => days.Add(new DayEVM(x)));
                                return days;
                            }
                    }

                    return null;
                }
            }
        }
        #endregion
    }
}
