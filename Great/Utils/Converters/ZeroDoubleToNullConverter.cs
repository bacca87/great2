﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Great2.Utils.Converters
{
    public class ZeroDoubleToNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is double && (double)value == 0)
                return null;
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
