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
        #region Properties
        private DateTime _currentDate;
        public DateTime currentDate
        {
            get
            {
                return _currentDate;
            }

            set
            {
                using (new WaitCursor())
                {
                    _currentDate = value;

                    currentMonthLabel.Content = new DateTime(_currentDate.Year, _currentDate.Month, 1).ToString("y").ToUpper();
                    selectDateCalendar.SelectedDate = (DateTime?)_currentDate;
                    selectDateCalendar.DisplayDate = _currentDate;
                    UpdateWorkingDaysView();
                }
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            currentDate = DateTime.Now;
        }

        private void UpdateWorkingDaysView()
        {
            IList<WorkingDay> days = new List<WorkingDay>();
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            System.Globalization.Calendar cal = dfi.Calendar;

            using (var db = new DBEntities())
            {
                foreach (DateTime day in AllDatesInMonth(currentDate.Year, currentDate.Month))
                {   
                    WorkingDay workingDay = new WorkingDay { 
                                                WeekNr = cal.GetWeekOfYear(day, dfi.CalendarWeekRule, dfi.FirstDayOfWeek), 
                                                Day = day, 
                                                Timesheets = db.Timesheets.SqlQuery("select * from Timesheet where Date = @date", new SQLiteParameter("date", day.ToString("yyyy-MM-dd"))).ToList() 
                                            };
                    days.Add(workingDay);
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
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility =
                      row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
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
                selectMonthButton.IsDropDownOpen = false;
                Mouse.Capture(null);
            }
        }

        private void selectDateCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectDateCalendar.SelectedDate == null)
                return;

            currentDate = selectDateCalendar.SelectedDate.Value;
        }

        private void nextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentDate = currentDate.AddMonths(1);
        }

        private void previousMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentDate = currentDate.AddMonths(-1);
        }
        
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
