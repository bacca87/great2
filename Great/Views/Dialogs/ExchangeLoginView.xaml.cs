using GalaSoft.MvvmLight.Ioc;
using Great.Models;
using Great.Models.Interfaces;
using Great.Utils.AttachedProperties;
using MahApps.Metro.Controls;
using System.Windows;

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

            txtEmailAddress.Text = UserSettings.Email.EmailAddress;
            PasswordHelper.SetBoundPassword(txtPassword, UserSettings.Email.EmailPassword);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                IProvider Exchange = SimpleIoc.Default.GetInstance<IProvider>();

                UserSettings.Email.EmailAddress = txtEmailAddress.Text;
                UserSettings.Email.EmailPassword = PasswordHelper.GetBoundPassword(txtPassword);

                Exchange.Disconnect();
                Exchange.Connect();

                Close();
            }
        }
    }
}