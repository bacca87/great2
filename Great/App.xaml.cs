using Great.Models;
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
            InitializeDirectoryTree();
            InitializeDatabase();
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
            Database.SetInitializer<DBEntities>(null);
            
            string dbDirectory = Path.GetDirectoryName(ApplicationSettings.Database.DBFileName);
            string dbFileName = Path.GetFileName(ApplicationSettings.Database.DBFileName);

            Directory.CreateDirectory(dbDirectory);
            
            if(!File.Exists(dbFileName))
                File.WriteAllBytes(dbFileName, Great.Properties.Resources.EmptyDatabaseFile);
        }
    }
}
