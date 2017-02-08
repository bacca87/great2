using Great.Models;
using Great.Utils;
using Great.ViewModels;
using System;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace Great.Views.Pages
{
    /// <summary>
    /// Interaction logic for Timesheet.xaml
    /// </summary>
    public partial class TimesheetView : Page
    {
        TimesheetsViewModel _viewModel;
        ScrollViewer scrollViewer;

        public TimesheetView()
        {
            InitializeComponent();
            
            _viewModel = DataContext as TimesheetsViewModel;
            _viewModel.OnSelectFirstDayInMonth += scrollToSelectedDay;
        }

        private void scrollToSelectedDay(WorkingDay day)
        {
            if (scrollViewer == null) // run once
                scrollViewer = WPFTools.GetVisualChild<ScrollViewer>(workingDaysDataGrid);

            // hack for scrolling to the selected item 
            scrollViewer.ScrollToBottom();
            workingDaysDataGrid.UpdateLayout();
            workingDaysDataGrid.ScrollIntoView(workingDaysDataGrid.SelectedItem);

            // scroll 1 unit up for showing current group header
            workingDaysDataGrid.UpdateLayout();
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 1);
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // hack for preselecting the workingDaysDataGrid at startup
            _viewModel.SelectFirstDayInMonth(DateTime.Now.Month);
            _viewModel.SelectToday();
        }
        
        private void MaskedTextBox_PreviewLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            //MaskedTextBox textbox = (e.Source as MaskedTextBox);

            //if (textbox.Text.Length < 5)
            //    return;

            //string[] digits = textbox.Text.Split(':');

            //if (digits.Length > 2)
            //    return;

            //string hours = digits[0].Replace("_", string.Empty);
            //string minutes = digits[1].Replace("_", string.Empty);




        }
    }
}
