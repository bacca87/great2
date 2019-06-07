using Great.Models;
using Great.Models.Database;
using Great.Utils;
using NLog;
using System;
using System.Data.Entity;
using System.IO;
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
            GlobalDiagnosticsContext.Set("logDirectory", ApplicationSettings.Directories.Log);
            InitializeDirectoryTree();
            InitializeDatabase();

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
            
            if(!File.Exists(dbFileName))
                File.WriteAllBytes(dbFileName, Great.Properties.Resources.EmptyDatabaseFile);
        }
    }
}
