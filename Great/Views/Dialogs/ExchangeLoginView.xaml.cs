﻿using Great.Utils.AttachedProperties;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Great.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ExchangeLoginView.xaml
    /// </summary>
    public partial class ExchangeLoginView : MetroWindow
    {
        public ExchangeLoginView()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            txtEmailAddress.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtPassword.GetBindingExpression(PasswordHelper.BoundPassword).UpdateSource();
            Close();
        }
    }
}