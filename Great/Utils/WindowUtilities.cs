using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Great2.Utils
{
    public static class WindowUtilities
    {
        public static readonly DependencyProperty CloseOnEscapeProperty = DependencyProperty.RegisterAttached(
           "CloseOnEscape",
           typeof(bool),
           typeof(WindowUtilities),
           new FrameworkPropertyMetadata(false, CloseOnEscapeChanged));

        public static bool GetCloseOnEscape(DependencyObject d)
        {
            return (bool)d.GetValue(CloseOnEscapeProperty);
        }

        public static void SetCloseOnEscape(DependencyObject d, bool value)
        {
            d.SetValue(CloseOnEscapeProperty, value);
        }

        private static void CloseOnEscapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window target = d as Window;
            if (target != null)
            {
                if ((bool)e.NewValue)
                {
                    target.PreviewKeyDown += Window_PreviewKeyDown;
                }
                else
                {
                    target.PreviewKeyDown -= Window_PreviewKeyDown;
                }
            }
        }

        private static void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Window target = sender as Window;

            if (target != null)
            {
                if (e.Key == Key.Escape)
                {
                    target.Close();
                }
            }
        }
    }
}
