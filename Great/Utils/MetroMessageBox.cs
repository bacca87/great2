
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Great2
{
    public sealed class MetroMessageBox
    {
        public static void Show(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                using (var msg = new MetroMessageBoxWindow())
                {
                    msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                    msg.BtnCancel.IsEnabled = false;
                    msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                    msg.BtnYes.IsEnabled = false;
                    msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                    msg.BtnNo.IsEnabled = false;

                    msg.Title = string.Empty;
                    msg.TxtMessage.Text = message;
                    msg.BtnOk.Focus();
                    msg.ShowDialog();
                }
            });

        }

        public static void Show(string message, string title)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                using (var msg = new MetroMessageBoxWindow())
                {
                    msg.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                    msg.BtnCancel.IsEnabled = false;
                    msg.BtnYes.Visibility = System.Windows.Visibility.Collapsed;
                    msg.BtnYes.IsEnabled = false;
                    msg.BtnNo.Visibility = System.Windows.Visibility.Collapsed;
                    msg.BtnNo.IsEnabled = false;

                    msg.Title = string.Empty;
                    msg.TxtMessage.Text = message;
                    msg.BtnOk.Focus();
                    msg.ShowDialog();
                }
            });
        }

        public static MessageBoxResult Show(string message, MessageBoxButton buttons)
        {
            MessageBoxResult result = MessageBoxResult.None;

            Application.Current.Dispatcher.Invoke((Action)delegate
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
                    msg.TxtMessage.Text = message;
                    msg.BtnOk.Focus();
                    msg.ShowDialog();

                    result = msg.Result;
                }
            });

            return result;
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons)
        {
            MessageBoxResult result = MessageBoxResult.None;

            Application.Current.Dispatcher.Invoke((Action)delegate
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
                    msg.TxtMessage.Text = message;
                    msg.BtnOk.Focus();
                    msg.ShowDialog();

                    result = msg.Result;
                }
            });

            return result;
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            MessageBoxResult result = MessageBoxResult.None;

            Application.Current.Dispatcher.Invoke((Action)delegate
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

                if (image == MessageBoxImage.Question) msg.IconImage.Source = new BitmapImage(new Uri("pack://application:,,,/Great2;component/Images/32/MessageBoxQuestion.png"));
                else if (image == MessageBoxImage.Error) msg.IconImage.Source = new BitmapImage(new Uri("pack://application:,,,/Great2;component/Images/32/MessageBoxError.png"));
                else if (image == MessageBoxImage.Information) msg.IconImage.Source = new BitmapImage(new Uri("pack://application:,,,/Great2;component/Images/32/MessageBoxInfo.png"));
                else if (image == MessageBoxImage.Warning) msg.IconImage.Source = new BitmapImage(new Uri("pack://application:,,,/Great2;component/Images/32/MessageBoxWarning.png"));

                    msg.Title = title;
                    msg.TxtMessage.Text = message;
                    msg.BtnOk.Focus();
                    msg.ShowDialog();

                    result = msg.Result;
                }
            });

            return result;
        }
    }

}

