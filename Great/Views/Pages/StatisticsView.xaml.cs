using Great.ViewModels;
using System.Windows.Controls;

namespace Great.Views.Pages
{
    /// <summary>
    /// Interaction logic for Statistics.xaml
    /// </summary>
    public partial class StatisticsView : Page
    {
        private StatisticsViewModel _viewModel => DataContext as StatisticsViewModel;

        public StatisticsView()
        {
            InitializeComponent();
        }

        private void Page_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue == true) _viewModel.RefreshAllData();
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void GeoMapPlants_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //   geoMapPlants.EnableZoomingAndPanning = true;
        }

        private void GeoMap_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
        }
    }
}