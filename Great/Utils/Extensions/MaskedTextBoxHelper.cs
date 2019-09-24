using System;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace Great.Utils.Extensions
{
    public static class MaskedTextBoxHelper
    {
        public static void PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            int? hours;
            int? minutes;

            MaskedTextBox textbox = sender as MaskedTextBox;

            if (!ParseTimeSpanMask(textbox.Text, textbox.PromptChar, out hours, out minutes)) return;

            if (hours.HasValue && hours.Value < 10) textbox.Text = hours.Value.ToString().PadLeft(2, '0') + textbox.Text.Substring(2);

            if (minutes.HasValue && minutes.Value < 10) textbox.Text = textbox.Text.Substring(0, 3) + minutes.Value.ToString().PadLeft(2, '0');

            if (hours.HasValue || minutes.HasValue) textbox.Text = textbox.Text.Replace(textbox.PromptChar, '0');
        }

        public static void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int? hours;
            int? minutes;

            MaskedTextBox textbox = sender as MaskedTextBox;

            if (e.Text == ":" || e.Text == ".")
            {
                if (!ParseTimeSpanMask(textbox.Text, textbox.PromptChar, out hours, out minutes)) return;

                if (hours.HasValue && hours.Value < 10) textbox.Text = hours.Value.ToString().PadLeft(2, '0') + textbox.Text.Substring(2);

                return;
            }

            if (textbox.CaretIndex >= textbox.MaxLength) return;

            if (!ParseTimeSpanMask(textbox.Text.Remove(textbox.CaretIndex, 1).Insert(textbox.CaretIndex, e.Text), textbox.PromptChar, out hours, out minutes)) return;

            if (hours.HasValue && (hours.Value < 0 || hours.Value > 23)) e.Handled = true;

            if (minutes.HasValue && (minutes.Value < 0 || minutes.Value > 59)) e.Handled = true;
        }

        private static bool ParseTimeSpanMask(string mask, char promptChar, out int? hours, out int? minutes)
        {
            hours = null;
            minutes = null;

            try
            {
                if (mask.Length < 5) return false;

                string[] digits = mask.Split(':');

                if (digits.Length != 2) return false;

                string hoursString = digits[0].Replace(promptChar.ToString(), string.Empty);
                string minutesString = digits[1].Replace(promptChar.ToString(), string.Empty);

                if (hoursString != string.Empty) hours = Convert.ToInt32(hoursString);

                if (minutesString != string.Empty) minutes = Convert.ToInt32(minutesString);

                return true;
            }
            catch { }

            return false;
        }

    }
}
