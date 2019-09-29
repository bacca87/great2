using Great.ViewModels;
using System;
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

            _viewModel.OnTabIndexSelected += OnTabSelected;
        }

        private void OnTabSelected(int obj)
        {
            ChartTab.SelectedIndex = obj;
        }

        private void Page_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                _viewModel.RefreshAllData();
            }
        }


    }
}
