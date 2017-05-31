using Great.Models;
using Great.Utils;
using Great.ViewModels;
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

        private void scrollToFirstDayInMonth(Day day)
        {
            // hack for scrolling to the first day in month displaying the group header 
            scrollViewer.ScrollToBottom();
            workingDaysDataGrid.UpdateLayout();
            workingDaysDataGrid.ScrollIntoView(workingDaysDataGrid.SelectedItem);

            // scroll 1 unit up for showing current group header
            workingDaysDataGrid.UpdateLayout();
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 1);
        }

        private void scrollToSelectedDay(Day day)
        {
            // hack for scrolling to the selected day 
            scrollViewer.ScrollToHome();
            workingDaysDataGrid.UpdateLayout();
            workingDaysDataGrid.ScrollIntoView(workingDaysDataGrid.SelectedItem);
        }

        #region Time MaskedTextBox Autocomplete Methods 
        private void MaskedTextBox_PreviewLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            int? hours;
            int? minutes;

            MaskedTextBox textbox = (sender as MaskedTextBox);

            if (!ParseTimeSpanMask(textbox.Text, textbox.PromptChar, out hours, out minutes))
                return;
            
            if (hours.HasValue && hours.Value < 10)
                textbox.Text = hours.Value.ToString().PadLeft(2, '0') + textbox.Text.Substring(2);

            if (minutes.HasValue && minutes.Value < 10)
                textbox.Text = textbox.Text.Substring(0, 3) + minutes.Value.ToString().PadLeft(2, '0');

            if(hours.HasValue || minutes.HasValue)
                textbox.Text = textbox.Text.Replace(textbox.PromptChar, '0');
        }

        private void MaskedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int? hours;
            int? minutes;

            MaskedTextBox textbox = (sender as MaskedTextBox);

            if (e.Text == ":" || e.Text == "." )
            {
                if (!ParseTimeSpanMask(textbox.Text, textbox.PromptChar, out hours, out minutes))
                    return;

                if (hours.HasValue && hours.Value < 10)
                    textbox.Text = hours.Value.ToString().PadLeft(2, '0') + textbox.Text.Substring(2);

                return;
            }

            if (!ParseTimeSpanMask(textbox.Text.Remove(textbox.CaretIndex, 1).Insert(textbox.CaretIndex, e.Text), textbox.PromptChar, out hours, out minutes))
                return;

            if (hours.HasValue && (hours.Value < 0 || hours.Value > 23))
                e.Handled = true;

            if (minutes.HasValue && (minutes.Value < 0 || minutes.Value > 59))
                e.Handled = true;
        }

        private bool ParseTimeSpanMask(string mask, char promptChar, out int? hours, out int? minutes)
        {
            hours = null;
            minutes = null;
            
            try
            {
                if (mask.Length < 5)
                    return false;

                string[] digits = mask.Split(':');

                if (digits.Length != 2)
                    return false;
                
                string hoursString = digits[0].Replace(promptChar.ToString(), string.Empty);
                string minutesString = digits[1].Replace(promptChar.ToString(), string.Empty);

                if (hoursString != string.Empty)
                    hours = Convert.ToInt32(hoursString);

                if (minutesString != string.Empty)
                    minutes = Convert.ToInt32(minutesString);

                return true;
            }
            catch { }

            return false;
        }
        #endregion
    }
}
