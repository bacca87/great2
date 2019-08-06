using Great.Utils.AttachedProperties;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

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
