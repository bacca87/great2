using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Great.Models.Database;
using Great.Utils.Extensions;
using Great.ViewModels.Database;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Great.ViewModels
{
    public class StatisticsViewModel : ViewModelBase
    {
        #region Properties
        private Func<ChartPoint, string> HoursLabel { get; set; }

        private int _WorkedDays;
        public int WorkedDays
        {
            get => _WorkedDays;
            set => Set(ref _WorkedDays, value);
        }

        private int _WorkedSaturdays;
        public int WorkedSaturdays
        {
            get => _WorkedSaturdays;
            set => Set(ref _WorkedSaturdays, value);
        }

        private int _WorkedSundays;
        public int WorkedSundays
        {
            get => _WorkedSundays;
            set => Set(ref _WorkedSundays, value);
        }

        private int _WorkedHolidays;
        public int WorkedHolidays
        {
            get => _WorkedHolidays;
            set => Set(ref _WorkedHolidays, value);
        }

        private int _TravelCount;
        public int TravelCount
        {
            get => _TravelCount;
            set => Set(ref _TravelCount, value);
        }


        private SeriesCollection _Factories;
        public SeriesCollection Factories
        {
            get => _Factories;
            set => Set(ref _Factories, value);
        }

        private Dictionary<string, double> _FactoryCountries;
        public Dictionary<string, double> FactoryCountries
        {
            get => _FactoryCountries;
            set => Set(ref _FactoryCountries, value);
        }

        private SeriesCollection _Km;
        public SeriesCollection Km
        {
            get => _Km;
            set => Set(ref _Km, value);
        }

        private SeriesCollection _Hours;
        public SeriesCollection Hours
        {
            get => _Hours;
            set => Set(ref _Hours, value);
        }

        private SeriesCollection _HourTypes;
        public SeriesCollection HourTypes
        {
            get => _HourTypes;
            set => Set(ref _HourTypes, value);
        }

        private SeriesCollection _days;
        public SeriesCollection Days
        {
            get => _days;
            set => Set(ref _days, value);
        }

        private SeriesCollection _FactoryTypes;
        public SeriesCollection FactoryTypes
        {
            get => _FactoryTypes;
            set => Set(ref _FactoryTypes, value);
        }

        private int _SelectedYear;
        public int SelectedYear
        {
            get => _SelectedYear;
            set
            {
                Set(ref _SelectedYear, value);
                RefreshAllData();
            }
        }

        private string[] _MonthsLabels;
        public string[] MonthsLabels
        {
            get => _MonthsLabels;
            set => Set(ref _MonthsLabels, value);
        }

        private bool IsRefreshEnabled = false;
        #endregion

        #region Commands Definitions
        public RelayCommand NextYearCommand { get; set; }
        public RelayCommand PreviousYearCommand { get; set; }
        #endregion

        public StatisticsViewModel()
        {
            Factories = new SeriesCollection();
            FactoryTypes = new SeriesCollection();
            Hours = new SeriesCollection();
            HourTypes = new SeriesCollection();
            Days = new SeriesCollection();
            Km = new SeriesCollection();
            FactoryCountries = new Dictionary<string, double>();

            NextYearCommand = new RelayCommand(() => SelectedYear++);
            PreviousYearCommand = new RelayCommand(() => SelectedYear--);

            HoursLabel = chartPoint => chartPoint.Y.ToString("N2") + "h";

            SelectedYear = DateTime.Now.Year;
            IsRefreshEnabled = true;

            MonthsLabels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        }

        public void RefreshAllData()
        {
            if (!IsRefreshEnabled)
                return;

            using (new WaitCursor())
            {
                LoadFactoriesData();
                LoadHoursData();
                LoadDayStatistic();
                LoadCarStatistics();
            }
        }

        private void LoadFactoriesData()
        {
            Dictionary<string, int> factoriesData = new Dictionary<string, int>();
            Factories.Clear();
            FactoryCountries.Clear();

            using (DBArchive db = new DBArchive())
            {
                string YearStr = SelectedYear.ToString();

                factoriesData = (from fdl in db.FDLs
                                 from timesheets in fdl.Timesheets
                                 where fdl.Id.Substring(0, 4) == YearStr && fdl.Factory1 != null
                                 group fdl.Factory1 by fdl.Factory1.Name into factories
                                 select factories).ToDictionary(x => x.Key, x => x.Count());

                foreach (KeyValuePair<string, int> entry in factoriesData)
                {
                    PieSeries factory = new PieSeries
                    {
                        Title = entry.Key,
                        Values = new ChartValues<int> { entry.Value },
                        DataLabels = true,

                    };

                    Factories.Add(factory);

                    var f = db.Factories.SingleOrDefault(x => x.Name == entry.Key);
                    if (f != null && f?.CountryCode != null)
                    {
                        if (FactoryCountries.Any(x => x.Key == f.CountryCode))
                            FactoryCountries[f.CountryCode] = FactoryCountries[f.CountryCode] + entry.Value;
                        else 
                            FactoryCountries.Add(f.CountryCode, entry.Value);
                    }

                }


            }
        }

        private void LoadHoursData()
        {

            Dictionary<string, int> factoriesData = new Dictionary<string, int>();

            using (DBArchive db = new DBArchive())
            {
                long startDate = new DateTime(SelectedYear, 1, 1).ToUnixTimestamp();
                long endDate = new DateTime(SelectedYear, 12, 31).ToUnixTimestamp();
                var WorkingDays = db.Days.Where(day => day.Timestamp >= startDate && day.Timestamp <= endDate && day.Timesheets.Count() > 0).ToList().Select(d => new DayEVM(d));
                var AllDays = db.Days.Where(day => day.Timestamp >= startDate && day.Timestamp <= endDate).ToList().Select(d => new DayEVM(d));




                var MontlyHours = WorkingDays?.GroupBy(d => d.Date.Month)
                                       .Select(g => new
                                       {
                                           TotalTime = g.Sum(x => (x.TotalTime ?? 0)),
                                           Ordinary = g.Sum(x => (x.TotalTime ?? 0) - (x.Overtime34 ?? 0) - (x.Overtime35 ?? 0) - (x.Overtime50 ?? 0) - (x.Overtime100 ?? 0)),
                                           Overtime34 = g.Sum(x => x.Overtime34 ?? 0),
                                           Overtime35 = g.Sum(x => x.Overtime35 ?? 0),
                                           Overtime50 = g.Sum(x => x.Overtime50 ?? 0),
                                           Overtime100 = g.Sum(x => x.Overtime100 ?? 0)
                                       });

                var AllDayMonthlyHours = AllDays?.GroupBy(d => d.Date.Month)
                       .Select(g => new
                       {
                           HomeWoring = g.Sum(x => x.HoursOfHomeWorking ?? 0),
                           Vacations = g.Sum(x => x.HoursOfVacation ?? 0),
                           Leave = g.Sum(x => x.HoursOfLeave ?? 0),
                           SpecialLeave = g.Sum(x => x.HoursOfSpecialLeave ?? 0),
                           SickLeave = g.Sum(x => x.HoursOfSicklLeave ?? 0)
                       });


                ChartValues<float> TotalTimeValues = new ChartValues<float>();
                ChartValues<float> OrdinaryValues = new ChartValues<float>();
                ChartValues<float> Overtime34Values = new ChartValues<float>();
                ChartValues<float> Overtime35Values = new ChartValues<float>();
                ChartValues<float> Overtime50Values = new ChartValues<float>();
                ChartValues<float> Overtime100Values = new ChartValues<float>();

                ChartValues<float> SpecialLeaveValues = new ChartValues<float>();
                ChartValues<float> VacationsValues = new ChartValues<float>();
                ChartValues<float> LeaveValues = new ChartValues<float>();
                ChartValues<float> SickLeaveValues = new ChartValues<float>();
                ChartValues<float> HomeWorkValues = new ChartValues<float>();



                foreach (var month in MontlyHours)
                {
                    TotalTimeValues.Add(month.TotalTime);
                    OrdinaryValues.Add(month.Ordinary);
                    Overtime34Values.Add(month.Overtime34);
                    Overtime35Values.Add(month.Overtime35);
                    Overtime50Values.Add(month.Overtime50);
                    Overtime100Values.Add(month.Overtime100);
                }

                foreach (var month in AllDayMonthlyHours)
                {
                    HomeWorkValues.Add(month.HomeWoring);
                    SickLeaveValues.Add(month.SickLeave);
                    LeaveValues.Add(month.Leave);
                    VacationsValues.Add(month.Vacations);
                    SpecialLeaveValues.Add(month.SpecialLeave);
                }

                Hours = new SeriesCollection()
                {
                    new StackedColumnSeries()
                    {
                        Title = "Ordinary Hours",
                        Values = OrdinaryValues,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new StackedColumnSeries()
                    {
                        Title = "Overtime 34%",
                        Values = Overtime34Values,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new StackedColumnSeries()
                    {
                        Title = "Overtime 35%",
                        Values = Overtime35Values,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new StackedColumnSeries()
                    {
                        Title = "Overtime 50%",
                        Values = Overtime50Values,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new StackedColumnSeries()
                    {
                        Title = "Overtime 100%",
                        Values = Overtime100Values,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new LineSeries
                    {
                        Title = "Total",
                        Values = TotalTimeValues,
                        DataLabels = true,
                        LabelPoint = HoursLabel
                    }
                };

                HourTypes = new SeriesCollection()
                {
                    new StackedColumnSeries()
                    {
                        Title = "Office work hours",
                        Values = TotalTimeValues,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new StackedColumnSeries()
                    {
                        Title = "Home working hours",
                        Values = HomeWorkValues,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new StackedColumnSeries()
                    {
                        Title = "Vacation Hours",
                        Values = VacationsValues,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new StackedColumnSeries()
                    {
                        Title = "Leave Hours",
                        Values = LeaveValues,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new StackedColumnSeries()
                    {
                        Title = "Special Leave Hours",
                        Values = SpecialLeaveValues,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    },
                    new StackedColumnSeries
                    {
                        Title = "Sick Leave Hours",
                        Values = SickLeaveValues,
                        DataLabels = true,
                        LabelPoint = HoursLabel
                    }
                };

                WorkedDays = WorkingDays?.Count() ?? 0;
                TravelCount = WorkingDays?.Where(x => x.Timesheets.Any(d => d.FDL1 != null)).Count() ?? 0;
                WorkedHolidays = WorkingDays?.Where(x => x.Timesheets.Count > 0 && x.IsHoliday).Count() ?? 0;
                WorkedSaturdays = WorkingDays?.Where(x => x.Timesheets.Count > 0 && x.Date.DayOfWeek == DayOfWeek.Saturday).Count() ?? 0;
                WorkedSundays = WorkingDays?.Where(x => x.Timesheets.Count > 0 && x.Date.DayOfWeek == DayOfWeek.Sunday).Count() ?? 0;
                TravelCount = WorkingDays?.Where(x => x.Timesheets.Count() > 0 && x.Timesheets.Any(y => y.FDL1 != null)).Count() ?? 0;
            }
        }

        private void LoadDayStatistic()
        {
            Days.Clear();
            FactoryTypes.Clear();

            using (DBArchive db = new DBArchive())
            {
                string YearStr = SelectedYear.ToString();
                long startDate = new DateTime(SelectedYear, 1, 1).ToUnixTimestamp();
                long endDate = new DateTime(SelectedYear, 12, 31).ToUnixTimestamp();

                //count all trip days
                var businessTripDays = (from d in db.Days
                                        from ts in d.Timesheets
                                        where ts.FDL1 != null
                                        && ts.FDL1.Id.Substring(0, 4) == YearStr
                                        select d).Distinct().Count();

                //count all office days without fdl
                var officeDays = (from d in db.Days
                                  from ts in d.Timesheets
                                  where d.DayType.Id == (long)EDayType.WorkDay && ts.FDL1 == null && d.Timestamp >= startDate && d.Timestamp <= endDate
                                  select d).Distinct().Count();

                //count all home working days 
                var homeWorkingDays = (from d in db.Days
                                       where d.DayType.Id == (long)EDayType.HomeWorkDay && d.Timestamp >= startDate && d.Timestamp <= endDate
                                       select d).Distinct().Count();

                //count all sick leaves 
                var sickDays = (from d in db.Days
                                where d.DayType.Id == (long)EDayType.SickLeave && d.Timestamp >= startDate && d.Timestamp <= endDate
                                select d).Distinct().Count();

                //count all vacations days
                var vacationDays = (from d in db.Days
                                    where d.DayType.Id == (long)EDayType.VacationDay && d.Timestamp >= startDate && d.Timestamp <= endDate
                                    select d).Distinct().Count();

                //count all special leaves
                var specialDays = (from d in db.Days
                                   where d.DayType.Id == (long)EDayType.SpecialLeave && d.Timestamp >= startDate && d.Timestamp <= endDate
                                   select d).Distinct().Count();

                //days in italy
                var transferITA = (from d in db.Days
                                   from ts in d.Timesheets
                                   where ts.FDL1 != null
                                   && ts.FDL1.Id.Substring(0, 4) == YearStr
                                   && ts.FDL1.Factory1.TransferType1.Id == 1
                                   select d).Distinct().Count();

                //days in italy
                var transferEU = (from d in db.Days
                                  from ts in d.Timesheets
                                  where ts.FDL1 != null
                                  && ts.FDL1.Id.Substring(0, 4) == YearStr
                                  && ts.FDL1.Factory1.TransferType1.Id == 2
                                  select d).Distinct().Count();

                //days in italy
                var transferExEU = (from d in db.Days
                                    from ts in d.Timesheets
                                    where ts.FDL1 != null
                                    && ts.FDL1.Id.Substring(0, 4) == YearStr
                                    && ts.FDL1.Factory1.TransferType1.Id == 3
                                    select d).Distinct().Count();

                //days no transf
                var noTransfer = (from d in db.Days
                                  from ts in d.Timesheets
                                  where ts.FDL1 != null
                                  && ts.FDL1.Id.Substring(0, 4) == YearStr
                                  && ts.FDL1.Factory1.TransferType1.Id == 0
                                  select d).Distinct().Count();

                if (officeDays > 0)
                {
                    Days.Add(new PieSeries
                    {
                        Title = "Office",
                        Values = new ChartValues<int> { officeDays },
                        DataLabels = true
                    });
                }

                if (homeWorkingDays > 0)
                {
                    Days.Add(new PieSeries
                    {
                        Title = "Home Work",
                        Values = new ChartValues<int> { homeWorkingDays },
                        DataLabels = true
                    });
                }

                if (businessTripDays > 0)
                {
                    Days.Add(new PieSeries
                    {
                        Title = "Business Trip",
                        Values = new ChartValues<int> { businessTripDays },
                        DataLabels = true
                    });
                }

                if (sickDays > 0)
                {
                    Days?.Add(new PieSeries
                    {
                        Title = "Sick Leave",
                        Values = new ChartValues<int> { sickDays },
                        DataLabels = true
                    });
                }

                if (vacationDays > 0)
                {
                    Days?.Add(new PieSeries
                    {
                        Title = "Vacations",
                        Values = new ChartValues<int> { vacationDays },
                        DataLabels = true
                    });
                }

                if (specialDays > 0)
                {
                    Days?.Add(new PieSeries
                    {
                        Title = "Special Leave",
                        Values = new ChartValues<int> { specialDays },
                        DataLabels = true
                    });
                }

                if (noTransfer > 0)
                {
                    FactoryTypes?.Add(new PieSeries
                    {
                        Title = "No Transfer",
                        Values = new ChartValues<int> { noTransfer },
                        DataLabels = true
                    });
                }

                if (transferITA > 0)
                {
                    FactoryTypes?.Add(new PieSeries
                    {
                        Title = "Italy",
                        Values = new ChartValues<int> { transferITA },
                        DataLabels = true
                    });
                }

                if (transferEU > 0)
                {
                    FactoryTypes?.Add(new PieSeries
                    {
                        Title = "Europe",
                        Values = new ChartValues<int> { transferEU },
                        DataLabels = true
                    });
                }

                if (transferExEU > 0)
                {
                    FactoryTypes?.Add(new PieSeries
                    {
                        Title = "Extra Europe",
                        Values = new ChartValues<int> { transferExEU },
                        DataLabels = true
                    });
                }
            }
        }

        private void LoadCarStatistics()
        {
            using (DBArchive db = new DBArchive())
            {
                long startDate = new DateTime(SelectedYear, 1, 1).ToUnixTimestamp();
                long endDate = new DateTime(SelectedYear, 12, 31).ToUnixTimestamp();
                var Rents = db.CarRentalHistories.Where(cr => cr.StartDate >= startDate && cr.EndDate <= endDate && cr.Car1 != null).ToList().Select(cr => new CarRentalHistoryEVM(cr));

                ChartValues<float> TotalKm = new ChartValues<float>();

                var MonthlyKm = Rents?.GroupBy(d => d.RentStartDate.Month)
                                       .Select(g => new
                                       {
                                           Km = g.Sum(x => (x.EndKm - x.StartKm)),
                                       });

                foreach (var month in MonthlyKm)
                {
                    TotalKm.Add(month.Km);
                }

                Km = new SeriesCollection()
                {
                    new LineSeries()
                    {
                        Title = "Driven Km",
                        Values = TotalKm,
                        DataLabels = false,
                        LabelPoint = HoursLabel
                    }
                };
            }
        }
    }
}
