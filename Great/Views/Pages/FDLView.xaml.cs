using Great.ViewModels;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Great.Views.Pages
{
    /// <summary>
    /// Interaction logic for FDLView.xaml
    /// </summary>
    public partial class FDLView : Page
    {
        FDLViewModel _viewModel;

        public FDLView()
        {
            InitializeComponent();
            _viewModel = DataContext as FDLViewModel;
        }
        
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // hack for selecting the first datagrid row by default in a hidden page
            if (fdlDataGridView.SelectedIndex == -1 && fdlDataGridView.Items.Count > 0)
                fdlDataGridView.SelectedIndex = 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_viewModel?.SelectedFDLClone != null)
                _viewModel.SelectedFDLClone.NotifyFDLPropertiesChanged();
        }

        private void FactoryHyperlink_OnClick(object sender, RequestNavigateEventArgs e)
        {
            Window wnd = Window.GetWindow(this);
            FactoriesViewModel factoriesVM = ServiceLocator.Current.GetInstance<FactoriesViewModel>();

            if (wnd is MainView && factoriesVM != null)
            {
                MainView mainView = wnd as MainView;
                TabItem factoriesTabItem = mainView.NavigationTabControl.Items.Cast<TabItem>().SingleOrDefault(item => (string)item.Header == "Factories");

                if (factoriesTabItem != null)
                {
                    factoriesVM.SelectedFactory = factoriesVM.Factories.SingleOrDefault(f => f.Id == _viewModel.SelectedFDL.Factory);
                    factoriesVM.ZoomOnFactoryRequest(factoriesVM.SelectedFactory);
                    factoriesTabItem.IsSelected = true;
                }

                e.Handled = true;
            }
        }
    }
}
