using Fluent;
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
    }
}
