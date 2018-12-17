using Fluent;
using GalaSoft.MvvmLight.Ioc;
using Great.ViewModels;
using Great.Views.Dialogs;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Great.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : RibbonWindow
    {
        public MainView()
        {
            InitializeComponent();
        }
        
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void yearTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void mEditRecipients_Click(object sender, RoutedEventArgs e)
        {
            OrderRecipientsViewModel recipientsVM = SimpleIoc.Default.GetInstance<OrderRecipientsViewModel>();
            FDLViewModel FdlVM = SimpleIoc.Default.GetInstance<FDLViewModel>();

            if (recipientsVM == null || FdlVM == null || FdlVM.SelectedFDL == null)
                return;

            recipientsVM.Order = FdlVM.SelectedFDL.Order;
            
            OrderRecipientsView view = new OrderRecipientsView();
            view.ShowDialog();
        }
    }
}
