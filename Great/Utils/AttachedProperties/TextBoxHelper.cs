﻿using System.Windows;
using System.Windows.Controls;

namespace Great.Utils.AttachedProperties
{
    public class TextBoxHelper
    {
        public static bool GetAutoScrollToEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToEndProperty);
        }

        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToEndProperty, value);
        }

        // Using a DependencyProperty as the backing store for AutoScrollToEnd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoScrollToEndProperty =
        DependencyProperty.RegisterAttached("AutoScrollToEnd", typeof(bool), typeof(TextBoxHelper), new PropertyMetadata(false, AutoScrollToEndPropertyChanged));

        private static void AutoScrollToEndPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textbox && e.NewValue is bool mustAutoScroll) textbox.TextChanged += (s, ee) => AutoScrollToEnd(s, ee, textbox);
        }

        private static void AutoScrollToEnd(object sender, TextChangedEventArgs e, TextBox textbox)
        {
            textbox.ScrollToEnd();
        }
    }
}
