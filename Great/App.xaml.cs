using Great2.Models;
using Great2.Models.Database;
using Great2.Properties;
using Great2.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SplashScreen = Great2.Views.SplashScreen;

namespace Great2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            GlobalDiagnosticsContext.Set("logDirectory", ApplicationSettings.Directories.Log);
            AppDomain.CurrentDomain.UnhandledException += Great2_UnhandledException;

            // Register AUMID, COM server, and activator
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<Great2NotificationActivator>(ApplicationSettings.General.AUMID);
            DesktopNotificationManagerCompat.RegisterActivator<Great2NotificationActivator>();

            // set the current thread culture to force the AutoUpdater.NET language in english
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en");

            // Multiple istance check
            if (!e.Args.Contains("-m") && !e.Args.Contains("/m"))
            {
                Process proc = Process.GetCurrentProcess();
                int count = Process.GetProcesses().Where(p =>
                    p.ProcessName == proc.ProcessName).Count();

                if (count > 1)
                {
                    MessageBox.Show("Cannot start Great2, another istance is already running!", "Great2", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Environment.Exit(1);
                    return;
                }
            }

            SplashScreen splash = null;

            if (!Debugger.IsAttached)
            {
                splash = new SplashScreen();
                MainWindow = splash;
                splash.Show();
            }

            // Upgrade Settings
            if (Settings.Default.UpgradeSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeSettings = false;
                Settings.Default.Save();
            }

            // themes
            UserSettings.Themes.AttachCustomThemes();
            UserSettings.Themes.ApplyThemeAccent(UserSettings.Themes.Theme, UserSettings.Themes.AccentColor);
            UserSettings.Themes.ApplyAllColors();

            // in order to ensure the UI stays responsive, we need to
            // do the work on a different thread
            Task.Factory.StartNew(() =>
            {
                MigrateDataFolder();
                InitializeDirectoryTree();
                InitializeDatabase();

                //once we're done we need to use the Dispatcher
                //to create and show the main window
                Dispatcher.Invoke(() =>
                {
                    //initialize the main window, set it as the application main window
                    //and close the splash screen
                    MainView window = new MainView();
                    window.ContentRendered += (s, args) =>
                    {
                        if (splash != null)
                            splash.Close();
                    };

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
                File.WriteAllBytes(dbFileName, Great2.Properties.Resources.EmptyDatabaseFile);
            else
                BackupDatabase(dbFileName, dbDirectory); //Backup before migrations

            ApplyMigrations(); //Apply only if exist. Otherwise must be updated from installation
        }

        private void ApplyMigrations()
        {
            string connectionString = ApplicationSettings.Database.ConnectionString.Replace("foreign keys=true;", string.Empty);

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteTransaction tr = connection.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = connection.CreateCommand())
                        {
                            int user_version = 0;

                            cmd.CommandText = "PRAGMA user_version";

                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                    user_version = Convert.ToInt32(reader.GetValue(0));
                            }

                            foreach (var f in Directory.GetFiles("UpgradeScripts/", "*.sql").OrderBy(f => Int32.Parse(Regex.Match(f, @"\d+").Value)))
                            {
                                if (!int.TryParse(Path.GetFileName(f).Split('_').First(), out int scriptVersion))
                                    continue;

                                if (user_version >= scriptVersion)
                                    continue;

                                cmd.CommandText = File.ReadAllText(f);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();

                        MetroMessageBox.Show($"Error upgrading the database.\nException: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(2);
                    }
                }
            }
        }

        private void BackupDatabase(string dbFileName, string dbDirectory)
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

        private void MigrateDataFolder()
        {
            try
            {
                if (UserSettings.Advanced.MigrationDataFolder == string.Empty)
                    return;

                string SourcePath = ApplicationSettings.Directories.Data.TrimEnd('\\');
                string DestinationPath = UserSettings.Advanced.MigrationDataFolder.TrimEnd('\\');

                // Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

                // Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);

                ApplicationSettings.Directories.Data = UserSettings.Advanced.MigrationDataFolder;
                    
                // Delete old folder and its contents
                Directory.Delete(SourcePath, true);

                UserSettings.Advanced.MigrationDataFolder = string.Empty;
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show($"Migration Failed!\nException: {ex.Message}", "Migration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Great2_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;      
            log.Error(ex, "Great2_UnhandledException event");
            log.Info("Is Terminating: {0}", e.IsTerminating);
        }
    }
}
