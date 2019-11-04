using GalaSoft.MvvmLight.Ioc;
using Great2.Utils;
using Great2.ViewModels;
using Great2.Views.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Great2.Views.Pages
{
    /// <summary>
    /// Interaction logic for Statistics.xaml
    /// </summary>
    public partial class StatisticsView : Page
    {
        private StatisticsViewModel _viewModel => DataContext as StatisticsViewModel;
        private ChartDataPopupViewModel _dataViewModel;

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

        private void DaysByType_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            _dataViewModel = SimpleIoc.Default.GetInstance<ChartDataPopupViewModel>();

            if (_dataViewModel == null)
                return;

            _dataViewModel.ChartType = EChartType.DayByType;
            _dataViewModel.Key = chartPoint.SeriesView.Title;
            _dataViewModel.Year = _viewModel.SelectedYear;

            ChartDataPopupView view = new ChartDataPopupView();
            view.Owner = Window.GetWindow(this);
            view.ShowDialog();

        }

        private void FactoriesByType_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            _dataViewModel = SimpleIoc.Default.GetInstance<ChartDataPopupViewModel>();

            if (_dataViewModel == null)
                return;

            _dataViewModel.ChartType = EChartType.FactoriesByType;
            _dataViewModel.Key = chartPoint.SeriesView.Title;
            _dataViewModel.Year = _viewModel.SelectedYear;

            ChartDataPopupView view = new ChartDataPopupView();
            view.Owner = Window.GetWindow(this);
            view.ShowDialog();

        }

        private void Factories_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            _dataViewModel = SimpleIoc.Default.GetInstance<ChartDataPopupViewModel>();

            if (_dataViewModel == null)
                return;

            _dataViewModel.ChartType = EChartType.Factories;
            _dataViewModel.Key = chartPoint.SeriesView.Title;
            _dataViewModel.Year = _viewModel.SelectedYear;

            ChartDataPopupView view = new ChartDataPopupView();
            view.Owner = Window.GetWindow(this);
            view.ShowDialog();

        }
    }
}
