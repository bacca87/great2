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
            float f;
            if (!float.TryParse(e.Text, out f)) e.Handled = true;
        }
    }
}
