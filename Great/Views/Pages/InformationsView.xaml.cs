using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Great2.Views
{
    /// <summary>
    /// Interaction logic for InformationsPage.xaml
    /// </summary>
    public partial class InformationsView : Page
    {
        public InformationsView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
