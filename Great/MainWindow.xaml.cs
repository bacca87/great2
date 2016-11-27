using Fluent;
using Great.DB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Great
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateWorkingDaysView(DateTime.Now.Year, DateTime.Now.Month);
        }

        private void UpdateWorkingDaysView(int year, int month)
        {
            IList<WorkingDay> days = new List<WorkingDay>();
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            System.Globalization.Calendar cal = dfi.Calendar;

            using (var db = new DBEntities())
            {
                foreach (DateTime date in AllDatesInMonth(year, month))
                {
                    try
                    {
                        WorkingDay day = new WorkingDay { WeekNr = cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek), Day = date, Timesheets = db.Timesheet.SqlQuery("select * from Timesheet where Date = @date", new SQLiteParameter("date", date.ToString("yyyy-MM-dd"))).ToList() };
                        days.Add(day);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
            }

            workingDaysDataGrid.ItemsSource = days;
        }

        public static IEnumerable<DateTime> AllDatesInMonth(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= days; day++)
            {
                yield return new DateTime(year, month, day);
            }
        }

        void ShowHideDetails(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility =
                      row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        void selectDateCalendar_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            // hack for using the calendar component just for selecting month and year
            if (selectDateCalendar.DisplayMode == CalendarMode.Month)
            {
                if (selectDateCalendar.DisplayDate != null)
                    selectDateCalendar.SelectedDate = new DateTime(selectDateCalendar.DisplayDate.Year, selectDateCalendar.DisplayDate.Month, 1);

                selectDateCalendar.DisplayMode = CalendarMode.Year;
                Mouse.Capture(null);
            }
        }

        private void selectDateCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateWorkingDaysView(selectDateCalendar.SelectedDate.Value.Year, selectDateCalendar.SelectedDate.Value.Month);
        }
    }
}
