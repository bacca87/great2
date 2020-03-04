using AutoUpdaterDotNET;
using Fluent;
using GalaSoft.MvvmLight.Ioc;
using Great2.Models;
using Great2.ViewModels;
using Great2.Views.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Great2.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : RibbonWindow
    {
        private DispatcherTimer CheckForUpdatesTimer;
        private bool ForceClose = false;

        public MainView()
        {
            InitializeComponent();

            if (!Debugger.IsAttached)
            {
                AutoUpdater.ShowSkipButton = false;
                AutoUpdater.HttpUserAgent = ApplicationSettings.General.UserAgent;
                AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
                AutoUpdater.BasicAuthXML = new BasicAuthentication(ApplicationSettings.General.GithubClientId, ApplicationSettings.General.GithubClientSecret);

                CheckForUpdatesTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
                CheckForUpdatesTimer.Tick += CheckForUpdatesTimer_Tick;
                CheckForUpdatesTimer.Start();
            }
        }

        private void CheckForUpdatesTimer_Tick(object sender, EventArgs e)
        {
            // check for updates
            if (!ApplicationSettings.General.ImportInProgress)
            {
                AutoUpdater.Start(ApplicationSettings.General.GithubReleasesInfoUrl);
                CheckForUpdatesTimer.Interval = TimeSpan.FromMinutes(10); // slow down the update check in order to not overload the github api (max 5000 request per hour)
            }   
            else
                CheckForUpdatesTimer.IsEnabled = false;
        }

        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            if (ApplicationSettings.General.ImportInProgress)
            {
                CheckForUpdatesTimer.IsEnabled = false;
                return;
            }

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
            ForceClose = true;
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
            if (ForceClose)
            {
                CheckEntities();
            }
            else
            {
                Hide();
                e.Cancel = true;
            }
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
            //SimpleIoc.Default.GetInstance<EventsViewModel>().SelectedEvent?.CheckChangedEntity();
        }

        private void AddVirtualFdlButton_Click(object sender, RoutedEventArgs e)
        {
            AddVirtualFDLViewModel virtualFDLVM = SimpleIoc.Default.GetInstance<AddVirtualFDLViewModel>();

            if (virtualFDLVM == null)
                return;

            AddVirtualFDLView view = new AddVirtualFDLView();
            view.Owner = this;
            view.ShowDialog();
        }

        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if(Visibility == Visibility.Hidden)
            {
                Show();
                TimesheetsViewModel timesheetVM = SimpleIoc.Default.GetInstance<TimesheetsViewModel>();
                timesheetVM.OnSelectToday?.Invoke(timesheetVM.SelectedWorkingDay);
            }

            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
        }
    }
}
