using Great2.Utils.Extensions;
using Great2.ViewModels;
using MahApps.Metro.Controls;

namespace Great2.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for EventsView.xaml
    /// </summary>
    public partial class EventsView : MetroWindow
    {
        public EventsView()
        {
            InitializeComponent();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EventsViewModel _viewModel = (EventsViewModel)DataContext;
            _viewModel.SelectedEvent?.CheckChangedEntity();
        }

        private void yearTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            MaskedTextBoxHelper.PreviewTextInput(sender, e);
        }
    }
}
