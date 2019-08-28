using Great.Utils;
using Great.Utils.Extensions;
using Great.ViewModels;
using Great.ViewModels.Database;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;


namespace Great.Views.Pages
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

            // scroll 45 unit up for showing current group header
            workingDaysDataGrid.UpdateLayout();
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 45);
        }

        private void scrollToSelectedDay(DayEVM day)
        {
            // hack for scrolling to the selected day 
            scrollViewer.ScrollToHome();
            workingDaysDataGrid.UpdateLayout();
            workingDaysDataGrid.ScrollIntoView(workingDaysDataGrid.SelectedItem);
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
    }
}
