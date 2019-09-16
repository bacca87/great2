using GalaSoft.MvvmLight.Ioc;
using Great.Utils.Extensions;
using Great.ViewModels;
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
            if (e.Source is ComboBox)
            {
                ComboBox cb = (ComboBox)e.Source;
                cb.Text = cb.Text?.ToUpper();

            }
        }


        private void MaskedTextBox_PreviewLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            MaskedTextBoxHelper.PreviewLostKeyboardFocus(sender, e);
        }

        private void MaskedTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            MaskedTextBoxHelper.PreviewTextInput(sender, e);
        }

    }
}
