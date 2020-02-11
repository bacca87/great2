using GalaSoft.MvvmLight.Ioc;
using Great2.ViewModels;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Great2.Views.Pages
{
    /// <summary>
    /// Interaction logic for ExpenseAccount.xaml
    /// </summary>
    public partial class ExpenseAccountView : Page
    {
        private ExpenseAccountViewModel _viewModel => DataContext as ExpenseAccountViewModel;

        public ExpenseAccountView()
        {
            InitializeComponent();

            _viewModel.OnFactoryLink += OnFactoryLink;
        }

        private void FactoryHyperlink_OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            e.Handled = true;
        }

        private void OnFactoryLink(long factoryId)
        {
            Window wnd = Window.GetWindow(this);
            FactoriesViewModel factoriesVM = SimpleIoc.Default.GetInstance<FactoriesViewModel>();

            if (wnd is MainView && factoriesVM != null)
            {
                MainView mainView = wnd as MainView;
                TabItem factoriesTabItem = mainView.NavigationTabControl.Items.Cast<TabItem>().SingleOrDefault(item => (string)item.Header == "Factories");

                if (factoriesTabItem != null)
                {
                    factoriesVM.SelectedFactory = factoriesVM.Factories.SingleOrDefault(f => f.Id == factoryId);
                    factoriesVM.ZoomOnFactoryRequest(factoriesVM.SelectedFactory);
                    factoriesTabItem.IsSelected = true;
                }
            }
        }
    }
}
