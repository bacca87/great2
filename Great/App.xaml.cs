using Great.Controls;
using Great.Models;
using Great.Models.Database;
using Great.Properties;
using Great.Utils;
using Great.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SplashScreen = Great.Views.SplashScreen;

namespace Great
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // set the current thread culture to force the AutoUpdater.NET language in english
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en");

            SplashScreen splash = new SplashScreen();
            MainWindow = splash;
            splash.Show();

            //in order to ensure the UI stays responsive, we need to
            //do the work on a different thread
            Task.Factory.StartNew(() =>
            {
                // Multiple istance check
                if (!e.Args.Contains("-m") && !e.Args.Contains("/m"))
                {
                    Process proc = Process.GetCurrentProcess();
                    int count = Process.GetProcesses().Where(p =>
                        p.ProcessName == proc.ProcessName).Count();

                    if (count > 1)
                    {
                        Environment.Exit(1);
                        return;
                    }
                }

                // Upgrade Settings
                if (Settings.Default.UpgradeSettings)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeSettings = false;
                    Settings.Default.Save();
                }

                ApplySkin(UserSettings.Themes.Skin);

                GlobalDiagnosticsContext.Set("logDirectory", ApplicationSettings.Directories.Log);
                InitializeDirectoryTree();
                InitializeDatabase();

                //once we're done we need to use the Dispatcher
                //to create and show the main window
                Dispatcher.Invoke(() =>
                {
                    //initialize the main window, set it as the application main window
                    //and close the splash screen
                    MainView window = new MainView();
                    window.ContentRendered += (s, args) => splash.Close();
                    MainWindow = window;
                    window.Show();
                });
            });            
        }

        private void InitializeDirectoryTree()
        {
            // Init the Data Directory
            Directory.CreateDirectory(ApplicationSettings.Directories.Data);
            AppDomain.CurrentDomain.SetData("DataDirectory", ApplicationSettings.Directories.Data);

            // Init the Directory tree
            Directory.CreateDirectory(ApplicationSettings.Directories.FDL);
            Directory.CreateDirectory(ApplicationSettings.Directories.ExpenseAccount);
            Directory.CreateDirectory(ApplicationSettings.Directories.Cache);
        }

        private void InitializeDatabase()
        {
            // disable EF database initializer
            Database.SetInitializer<DBArchive>(null);

            string dbDirectory = Path.GetDirectoryName(ApplicationSettings.Database.DBFileName);
            string dbFileName = dbDirectory + "\\" + Path.GetFileName(ApplicationSettings.Database.DBFileName);

            Directory.CreateDirectory(dbDirectory);

            if (!File.Exists(dbFileName))
                File.WriteAllBytes(dbFileName, Great.Properties.Resources.EmptyDatabaseFile);
            else
                DoBackup(dbFileName, dbDirectory); //Backup before migrations

            ApplyMigrations(); //Apply only if exist. Otherwise must be updated from installation
        }

        private void ApplyMigrations()
        {
            using (DBArchive db = new DBArchive())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var db_version = db.Database.SqlQuery<int>("PRAGMA user_version").First();

                        foreach (var f in Directory.GetFiles("UpgradeScripts/", "*.sql").OrderBy(f => Int32.Parse(Regex.Match(f, @"\d+").Value)))
                        {
                            if (!int.TryParse(Path.GetFileName(f).Split('_').First(), out int scriptVersion))
                                continue;

                            if (db_version >= scriptVersion)
                                continue;

                            db.Database.ExecuteSqlCommand(File.ReadAllText(f));
                            db.SaveChanges();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        MetroMessageBox.Show($"Error upgrading the database.\nException: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        Environment.Exit(2);
                    }
                }
            }
        }

        private void DoBackup(string dbFileName, string dbDirectory)
        {

            File.Copy(dbFileName, dbDirectory + "\\" + "archive_" + DateTime.Now.ToString("yyyyMMdd") + ".db3", true);

            // get files ordered by creationdatetime
            IList<FileInfo> files = new DirectoryInfo(dbDirectory).GetFiles("*.db3")
                                                                  .Where(x => x.FullName != dbFileName)
                                                                  .OrderByDescending(x => x.CreationTime)
                                                                  .ToList();

            files.Except(files.Take(ApplicationSettings.Database.MaxBackupCount))
                  .ToList()
                  .ForEach(x => x.Delete());
        }

        public void ApplySkin(ESkin newSkin)
        {
            foreach (ResourceDictionary dict in Resources.MergedDictionaries)
            {
                if (dict is SkinResourceDictionary skinDict)
                    skinDict.UpdateSource();
                else
                    dict.Source = dict.Source;
            }

        }
    }
}
