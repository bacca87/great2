using Fluent;
using Great.DB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            IList<WorkingDay> days = new List<WorkingDay>();
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            System.Globalization.Calendar cal = dfi.Calendar;

            using (var db = new DBEntities())
            {
                foreach (DateTime date in AllDatesInMonth(2016, 11))
                {
                    try
                    {
                        WorkingDay day = new WorkingDay { WeekNr = cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek), Day = date, Timesheets = db.Timesheet.Where(t => t.Date == date.Date).ToList() };
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
    }
}
