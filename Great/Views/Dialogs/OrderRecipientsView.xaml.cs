using MahApps.Metro.Controls;
using System.Windows;

namespace Great2.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for OrderRecipientsView.xaml
    /// </summary>
    public partial class OrderRecipientsView : MetroWindow
    {
        public OrderRecipientsView()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
