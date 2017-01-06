using Fluent;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Great.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : RibbonWindow
    {
        public MainView()
        {
            InitializeComponent();
        }
        
        void ShowHideDetails(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
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
        
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
