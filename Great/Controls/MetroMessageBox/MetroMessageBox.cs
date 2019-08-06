
using System.Windows;


namespace Great.Controls
{
    public sealed class MetroMessageBox
    {
        public static void Show(string message)
        {
            using (var msg = new MetroMessageBoxWindow())
            {
                // simplest case: only ok button available
                msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                msg.BtnCancel.IsEnabled = false;
                msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                msg.BtnYes.IsEnabled = false;
                msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                msg.BtnNo.IsEnabled = false;

                msg.Title = string.Empty;
                msg.TxtTitle.Text = string.Empty;
                msg.TxtMessage.Text = message;
                msg.BtnOk.Focus();
                msg.ShowDialog();
            }
        }

        public static void Show(string message, string title)
        {
            using (var msg = new MetroMessageBoxWindow())
            {
                // simplest case: only ok button available
                msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                msg.BtnCancel.IsEnabled = false;
                msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                msg.BtnYes.IsEnabled = false;
                msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                msg.BtnNo.IsEnabled = false;

                msg.Title = string.Empty;
                msg.TxtTitle.Text = title;
                msg.TxtMessage.Text = message;
                msg.BtnOk.Focus();
                msg.ShowDialog();
            }
        }

        public static MessageBoxResult Show(string message, MessageBoxButton buttons)
        {
            using (var msg = new MetroMessageBoxWindow())
            {
                switch (buttons)
                {
                    case MessageBoxButton.OK:
                        msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnCancel.IsEnabled = false;
                        msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnYes.IsEnabled = false;
                        msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnNo.IsEnabled = false;
                        break;
                    case MessageBoxButton.YesNo:
                        msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnCancel.IsEnabled = false;
                        msg.BtnOk.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnOk.IsEnabled = false;
                        break;
                    case MessageBoxButton.YesNoCancel:
                        msg.BtnOk.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnOk.IsEnabled = false;
                        break;
                    case MessageBoxButton.OKCancel:

                        msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnYes.IsEnabled = false;
                        msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnNo.IsEnabled = false;
                        break;
                }
                msg.Title = string.Empty;
                msg.TxtTitle.Text = string.Empty;
                msg.TxtMessage.Text = message;
                msg.BtnOk.Focus();
                msg.ShowDialog();

                return msg.Result;
            }
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons)
        {
            using (var msg = new MetroMessageBoxWindow())
            {
                switch (buttons)
                {
                    case MessageBoxButton.OK:
                        msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnCancel.IsEnabled = false;
                        msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnYes.IsEnabled = false;
                        msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnNo.IsEnabled = false;
                        break;
                    case MessageBoxButton.YesNo:
                        msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnCancel.IsEnabled = false;
                        msg.BtnOk.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnOk.IsEnabled = false;
                        break;
                    case MessageBoxButton.YesNoCancel:
                        msg.BtnOk.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnOk.IsEnabled = false;
                        break;
                    case MessageBoxButton.OKCancel:

                        msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnYes.IsEnabled = false;
                        msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnNo.IsEnabled = false;
                        break;
                }
                msg.Title = title;
                msg.TxtTitle.Text = title;
                msg.TxtMessage.Text = message;
                msg.BtnOk.Focus();
                msg.ShowDialog();

                return msg.Result;
            }
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            using (var msg = new MetroMessageBoxWindow())
            {
                switch (buttons)
                {
                    case MessageBoxButton.OK:
                        msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnCancel.IsEnabled = false;
                        msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnYes.IsEnabled = false;
                        msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnNo.IsEnabled = false;
                        break;
                    case MessageBoxButton.YesNo:
                        msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnCancel.IsEnabled = false;
                        msg.BtnOk.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnOk.IsEnabled = false;
                        break;
                    case MessageBoxButton.YesNoCancel:
                        msg.BtnOk.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnOk.IsEnabled = false;
                        break;
                    case MessageBoxButton.OKCancel:

                        msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnYes.IsEnabled = false;
                        msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                        msg.BtnNo.IsEnabled = false;
                        break;
                }
                msg.Title = title;
                msg.TxtTitle.Text = title;
                msg.TxtMessage.Text = message;
                msg.BtnOk.Focus();
                msg.ShowDialog();

                return msg.Result;
            }
        }

    }
}
