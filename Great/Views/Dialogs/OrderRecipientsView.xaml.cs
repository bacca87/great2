using Great2.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

namespace Great2.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for OrderRecipientsView.xaml
    /// </summary>
    public partial class OrderRecipientsView : MetroWindow
    {
        private OrderRecipientsViewModel _viewModel => DataContext as OrderRecipientsViewModel;

        public OrderRecipientsView()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            if (_viewModel != null)
                _viewModel.Close = new System.Action(() => Close());
        }
    }
}
