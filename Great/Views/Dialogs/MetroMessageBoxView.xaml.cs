using System;
using System.Windows;

namespace Great
{
    /// <summary>
    /// Interaction logic for MetroMessageBox.xaml
    /// </summary>
    public partial class MetroMessageBoxWindow : IDisposable
    {
        public MessageBoxResult Result { get; set; }

        public MetroMessageBoxWindow()
        {
            InitializeComponent();
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        public void Dispose()
        {
            Close();
        }

        private void BtnCopyMessage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtMessage.Text);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void BtnNo_OnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }
    }
}