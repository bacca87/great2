using Great.Utils;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SQLite;

namespace Great.Models
{
    #region Application Settings
    public static class ApplicationSettings
    {
        #region Database
        public static class Database
        {
            public static string ConnectionString
            {
                get
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["DBEntities"].ConnectionString;

                    if (connectionString == null || connectionString == string.Empty)
                        throw new ConfigurationErrorsException("Missing DBEntities connection string!");

                    return connectionString;
                }
            }
            public static string DBFileName
            {
                get
                {
                    EntityConnectionStringBuilder efBuilder = new EntityConnectionStringBuilder(ConnectionString);
                    SQLiteConnectionStringBuilder sqliteBuilder = new SQLiteConnectionStringBuilder(efBuilder.ProviderConnectionString);
                    return sqliteBuilder.DataSource.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
                }
            }
        }
        #endregion

        #region Directories
        public static class Directories
        {
            public static string Data
            {
                get
                {
                    string dataDirectoryPath = ConfigurationManager.AppSettings["DataDirectoryPath"];

                    if (dataDirectoryPath == null || dataDirectoryPath == string.Empty)
                        throw new ConfigurationErrorsException("Missing or invalid DataDirectoryPath configuration!");

                    return Environment.ExpandEnvironmentVariables(dataDirectoryPath);
                }
            }
            public static string FDL
            {
                get
                {
                    string fdlDirName = ConfigurationManager.AppSettings["FDLDirectoryName"];

                    if (fdlDirName == null || fdlDirName == string.Empty)
                        throw new ConfigurationErrorsException("Missing or invalid FDLDirectory configuration!");

                    return Data + "\\" + fdlDirName;
                }
            }
            public static string ExpenseAccount
            {
                get
                {
                    string expenseAccountDirName = ConfigurationManager.AppSettings["ExpenseAccountDirectoryName"];

                    if (expenseAccountDirName == null || expenseAccountDirName == string.Empty)
                        throw new ConfigurationErrorsException("Missing or invalid ExpenseAccountDirectory configuration!");

                    return Data + "\\" + expenseAccountDirName;
                }
            }

            public static string Cache
            {
                get
                {
                    string cacheDirectoryPath = ConfigurationManager.AppSettings["CacheDirectoryPath"];

                    if (cacheDirectoryPath == null || cacheDirectoryPath == string.Empty)
                        throw new ConfigurationErrorsException("Missing or invalid CacheDirectoryPath configuration!");
                    
                    return Environment.ExpandEnvironmentVariables(cacheDirectoryPath);
                }
            }
        }
        #endregion
        
        #region FDL
        public static class FDL
        {
            public const int PerformanceDescriptionMaxLength = 575;
            public const int FinalTestResultMaxLength = 495;
            public const int OtherNotesMaxLength = 595;

            public const string MIMEType = "application/pdf";

            public const string FDLEmailAddress = "fdl@elettric80.it";
        }
        #endregion

        #region General
        public static class General
        {
            public const int WaitForNextConnectionRetry = 10000;
        }
        #endregion

        #region Google Map
        public static class GoogleMap
        {
            public const double ZoomMarker = 15;
            public const string GoogleUrl = "google.com";

            public const string NewFactoryName = "New Factory";
        }
        #endregion

        #region Timesheets
        public static class Timesheets
        {
            public const int MinYear = 1900;
            public const int MaxYear = 2100;
        }
        #endregion
    }
    #endregion

    #region User Settings
    public static class UserSettings
    {
        #region Events
        public static Action<string> SettingsChanged;
        #endregion

        #region Constructor
        static UserSettings()
        {
            Properties.Settings.Default.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                SettingsChanged?.Invoke(e.PropertyName);
            };
        }
        #endregion

        #region Email
        public static class Email
        {
            public static string EmailAddress
            {
                get { return Properties.Settings.Default.EmailAddress; }
                set
                {
                    Properties.Settings.Default.EmailAddress = value;
                    Properties.Settings.Default.Save();
                }
            }
            public static string EmailPassword
            {
                get
                {
                    string password = Properties.Settings.Default.EmailPassword;
                    return password.Length > 0 ? SecureCrypt.Decrypt(password) : string.Empty;
                }
                set
                {
                    Properties.Settings.Default.EmailPassword = SecureCrypt.Encrypt(value);
                    Properties.Settings.Default.Save();
                }
            }
        }
        #endregion
    }
    #endregion
}
