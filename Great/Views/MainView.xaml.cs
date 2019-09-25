using AutoUpdaterDotNET;
using Fluent;
using GalaSoft.MvvmLight.Ioc;
using Great.Models;
using Great.ViewModels;
using Great.Views.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Great.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : RibbonWindow
    {
        DispatcherTimer CheckForUpdatesTimer;

        public MainView()
        {
            InitializeComponent();

            if (!Debugger.IsAttached)
            {
                AutoUpdater.HttpUserAgent = ApplicationSettings.General.UserAgent;
                AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;

                CheckForUpdatesTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
                CheckForUpdatesTimer.Tick += CheckForUpdatesTimer_Tick;
                CheckForUpdatesTimer.Start();
            }
        }

        private void CheckForUpdatesTimer_Tick(object sender, EventArgs e)
        {
            // check for updates            
            AutoUpdater.Start(ApplicationSettings.General.ReleasesInfoAddress);
        }

        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            var json = (JArray)JsonConvert.DeserializeObject(args.RemoteData);

            Func<string, string> GetVersionFromTag = (tag) => { return tag.Remove(0, tag.IndexOf('v') + 1); };

            var CurrentVersion = new Version(GetVersionFromTag(((JValue)json[0]["tag_name"]).Value as string));
            var ChangelogUrl = string.Empty;
            var DownloadUrl = ((JValue)json[0]["assets"][0]["browser_download_url"]).Value as string;

            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = CurrentVersion,
                ChangelogURL = ChangelogUrl,
                Mandatory = false,
                DownloadURL = DownloadUrl
            };
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

        private void mEventPage_Click(object sender, RoutedEventArgs e)
        {
            EventsViewModel eventVM = SimpleIoc.Default.GetInstance<EventsViewModel>();

            if (eventVM == null)
                return;

            EventsView view = new EventsView();
            view.Owner = this;
            view.ShowDialog();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsViewModel settingsVM = SimpleIoc.Default.GetInstance<SettingsViewModel>();

            if (settingsVM == null)
                return;

            SettingsView view = new SettingsView();
            view.Owner = this;
            view.ShowDialog();
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheckEntities();
        }



        private void NavigationTabControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var test = e.Source;
            if (e.Source is TabItem)
            {
                TabItem t = (TabItem)e.Source;
                this.Dispatcher.Invoke(new Action(() => { CheckEntities(); }), null);
                t.Focus();
            }

        }

        private void CheckEntities()
        {
            SimpleIoc.Default.GetInstance<FDLViewModel>().SelectedFDL?.CheckChangedEntity();
            SimpleIoc.Default.GetInstance<ExpenseAccountViewModel>().SelectedEA?.CheckChangedEntity();
            SimpleIoc.Default.GetInstance<FactoriesViewModel>().SelectedFactory?.CheckChangedEntity();
            SimpleIoc.Default.GetInstance<CarRentalViewModel>().SelectedRent?.CheckChangedEntity();
            SimpleIoc.Default.GetInstance<EventsViewModel>().SelectedEvent?.CheckChangedEntity();
        }


    }

}
}
