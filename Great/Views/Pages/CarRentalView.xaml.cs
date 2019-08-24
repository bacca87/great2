using System.Windows.Controls;

namespace Great.Views.Pages
{
    /// <summary>
    /// Interaction logic for CarRental.xaml
    /// </summary>
    public partial class CarRental : Page
    {

        public CarRental()
        {
            InitializeComponent();

        }

        private void ComboBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void CmbLicenxePlate_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            cmbLicenxePlate.Text = cmbLicenxePlate.Text?.ToUpper();
        }
    }
}
