using Great.Models;
using Great.Models.Database;
using Great.Views.Skins;
using NLog;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Great
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

         private void Application_Startup(object sender, StartupEventArgs e)
        {   
            // Multiple istance check
            if(!e.Args.Contains("-m") && !e.Args.Contains("/m"))
            {
                Process proc = Process.GetCurrentProcess();
                int count = Process.GetProcesses().Where(p =>
                    p.ProcessName == proc.ProcessName).Count();

                if (count > 1)
                    Current.Shutdown();
            }

            GlobalDiagnosticsContext.Set("logDirectory", ApplicationSettings.Directories.Log);
            InitializeDirectoryTree();
            InitializeDatabase();

            ApplySkin(UserSettings.Themes.Skin);
            // TODO: Auto Updater (https://github.com/ravibpatel/AutoUpdater.NET)
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
                    }
                }
            }
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
