using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Great.Utils.Converters
{
    class MessageBoxImageToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Icon icon = (Icon)typeof(SystemIcons).GetProperty(parameter.ToString(), BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            BitmapSource bs = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bs;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
