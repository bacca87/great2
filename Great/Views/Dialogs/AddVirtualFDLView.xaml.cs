using Great2.ViewModels;
using MahApps.Metro.Controls;
using System.Text.RegularExpressions;
using System.Windows;

namespace Great2.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ExchangeLoginView.xaml
    /// </summary>
    public partial class AddVirtualFDLView : MetroWindow
    {
        private AddVirtualFDLViewModel _viewModel => DataContext as AddVirtualFDLViewModel;

        public AddVirtualFDLView()
        {
            InitializeComponent();
            _viewModel.OnFDLSaved += () => Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OrderTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
