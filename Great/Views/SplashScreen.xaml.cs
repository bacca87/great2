using MahApps.Metro.Controls;
using System.Diagnostics;
using System.Reflection;

namespace Great2.Views
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : MetroWindow
    {
        public SplashScreen()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            InitializeComponent();

            VersionTextBlock.Text = "v" + FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        }
    }
}
