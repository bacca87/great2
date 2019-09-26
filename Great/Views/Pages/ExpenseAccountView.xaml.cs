using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Great.Views.Pages
{
    /// <summary>
    /// Interaction logic for ExpenseAccount.xaml
    /// </summary>
    public partial class ExpenseAccountView : Page
    {
        public ExpenseAccountView()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            if (!regex.Match(e.Text).Success) e.Handled = true;
        }
    }
}
