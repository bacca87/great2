using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Great.Utils.Converters
{
    public class DataGridItemConverter : MarkupExtension, IValueConverter
    {
        static DataGridItemConverter converter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null && value.GetType() == targetType) ? value : null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (converter == null)
                converter = new DataGridItemConverter();
            return converter;
        }
    }
}
