using Great2.Wizards.Views;
using System.Windows;
using System.Windows.Controls;

namespace Great2.Views.Pages
{
    /// <summary>
    /// Interaction logic for ImportExportView.xaml
    /// </summary>
    public partial class ImportExportView : Page
    {
        public ImportExportView()
        {
            InitializeComponent();
        }

        private void GreatImportButton_Click(object sender, RoutedEventArgs e)
        {
            GreatImportWizardView wizard = new GreatImportWizardView();
            wizard.Owner = Window.GetWindow(this);
            wizard.ShowDialog();
        }

        private void FDLImportButton_Click(object sender, RoutedEventArgs e)
        {
            FDLImportWizardView wizard = new FDLImportWizardView();
            wizard.Owner = Window.GetWindow(this);
            wizard.ShowDialog();
        }
    }
}
