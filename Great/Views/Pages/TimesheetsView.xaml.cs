using Great2.Utils;
using Great2.Utils.Extensions;
using Great2.ViewModels;
using Great2.ViewModels.Database;
using System.Windows.Controls;
using System.Windows.Input;


namespace Great2.Views.Pages
{
    /// <summary>
    /// Interaction logic for Timesheet.xaml
    /// </summary>
    public partial class TimesheetView : Page
    {
        private TimesheetsViewModel _viewModel;
        private ScrollViewer scrollViewer;
        private bool startup = true;

        public TimesheetView()
        {
            InitializeComponent();

            _viewModel = DataContext as TimesheetsViewModel;
            _viewModel.OnSelectFirstDayInMonth += scrollToFirstDayInMonth;
            _viewModel.OnSelectToday += scrollToSelectedDay;
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (startup) // run once
            {
                scrollViewer = WPFTools.GetVisualChild<ScrollViewer>(workingDaysDataGrid);

                // hack for preselecting the workingDaysDataGrid at startup
                _viewModel.SelectToday();
                startup = false;
            }
        }

        private void scrollToFirstDayInMonth(DayEVM day)
        {
            // hack for scrolling to the first day in month displaying the group header 
            scrollViewer.ScrollToBottom();
            workingDaysDataGrid.UpdateLayout();
            workingDaysDataGrid.ScrollIntoView(workingDaysDataGrid.SelectedItem);

            // scroll 1 unit up for showing current group header
            workingDaysDataGrid.UpdateLayout();
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 1);
        }

        private void scrollToSelectedDay(DayEVM day)
        {
            // hack for scrolling to the selected day 
            scrollViewer.ScrollToHome();
            workingDaysDataGrid.UpdateLayout();
            workingDaysDataGrid.ScrollIntoView(workingDaysDataGrid.SelectedItem);

            // scroll down for centering in the middle of the grid the selected day
            workingDaysDataGrid.UpdateLayout();
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 15);
        }

        #region Time MaskedTextBox Autocomplete Methods 
        private void MaskedTextBox_PreviewLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            MaskedTextBoxHelper.PreviewLostKeyboardFocus(sender, e);
        }

        private void MaskedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            MaskedTextBoxHelper.PreviewTextInput(sender, e);

        }

        #endregion

        private void WorkingDaysDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            timesheetPanel.IsExpanded = true;
            editTimesheetPanel.IsExpanded = true;
        }

        private void TimesheetPanel_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!timesheetPanel.IsExpanded)
                return;

            scrollToSelectedDay((DayEVM)workingDaysDataGrid.SelectedItem);
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                timesheetPanel.IsExpanded = false;
        }
    }
}
