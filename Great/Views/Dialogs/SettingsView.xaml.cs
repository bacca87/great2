using Great2.ViewModels;
using MahApps.Metro.Controls;

namespace Great2.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : MetroWindow
    {
        private SettingsViewModel _viewModel => DataContext as SettingsViewModel;

        public SettingsView()
        {
            InitializeComponent();

            if (_viewModel != null)
            {
                _viewModel.Close = new System.Action(() => Close());
            }
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
