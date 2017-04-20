using Great.Utils;
using Great.Utils.Extensions;
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
                    string DataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
                    EntityConnectionStringBuilder efBuilder = new EntityConnectionStringBuilder(ConnectionString);
                    SQLiteConnectionStringBuilder sqliteBuilder = new SQLiteConnectionStringBuilder(efBuilder.ProviderConnectionString);                                        
                    return sqliteBuilder.DataSource.Replace("|DataDirectory|", DataDirectory.Remove(DataDirectory.Length - 1)); // remove last \ to the DataDirectory path
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

                    dataDirectoryPath = Environment.ExpandEnvironmentVariables(dataDirectoryPath);
                    return dataDirectoryPath + (!dataDirectoryPath.EndsWith("\\") ? "\\" : "");
                }
            }
            public static string FDL
            {
                get
                {
                    string fdlDirName = ConfigurationManager.AppSettings["FDLDirectoryName"];

                    if (fdlDirName == null || fdlDirName == string.Empty)
                        throw new ConfigurationErrorsException("Missing or invalid FDLDirectory configuration!");

                    return Data + fdlDirName + (!fdlDirName.EndsWith("\\") ? "\\" : "");
                }
            }
            public static string ExpenseAccount
            {
                get
                {
                    string expenseAccountDirName = ConfigurationManager.AppSettings["ExpenseAccountDirectoryName"];

                    if (expenseAccountDirName == null || expenseAccountDirName == string.Empty)
                        throw new ConfigurationErrorsException("Missing or invalid ExpenseAccountDirectory configuration!");

                    return Data + expenseAccountDirName + (!expenseAccountDirName.EndsWith("\\") ? "\\" : "");
                }
            }

            public static string Cache
            {
                get
                {
                    string cacheDirectoryPath = ConfigurationManager.AppSettings["CacheDirectoryPath"];

                    if (cacheDirectoryPath == null || cacheDirectoryPath == string.Empty)
                        throw new ConfigurationErrorsException("Missing or invalid CacheDirectoryPath configuration!");

                    cacheDirectoryPath = Environment.ExpandEnvironmentVariables(cacheDirectoryPath);
                    return cacheDirectoryPath + (!cacheDirectoryPath.EndsWith("\\") ? "\\" : "");
                }
            }
        }
        #endregion
        
        #region FDL
        public static class FDL
        {
            public const int PerformanceDescriptionMaxLength = 575;
            public const int PerformanceDescriptionDetailsMaxLength = 5000;
            public const int FinalTestResultMaxLength = 495;
            public const int OtherNotesMaxLength = 595;

            public const string MIMEType = "application/pdf";

            public const string EmailAddress = "fdl@elettric80.it";

            public const string FDL_Accepted = "FDL RECEIVED";
            public const string FDL_Rejected = "INVALID FDL";
            public const string FDL_Extra = "EXTRA";

            public const string EA_Rejected = "NOTA SPESE RIFIUTATA";
            public const string EA_RejectedResubmission = "Reinvio nota spese";

            public const string Positive = "POSITIVO";
            public const string Negative = "NEGATIVO";
            public const string WithReserve = "CON RISERVA";

            public static class FieldNames
            {
                public const string FDLNumber = "data[0].#subform[0].Campoditesto1[0]";
                public const string Customer = "data[0].#subform[0].Tabella2[0].Riga1[0].Cella2[0]";
                public const string Address = "data[0].#subform[0].Tabella2[0].Riga2[0].Cella2[0]";
                public const string Technician = "data[0].#subform[0].Tabella3[0].Riga1[0].Cella1[0]";
                public const string CID = "data[0].#subform[0].Tabella3[0].Riga2[0].Cella1[0]";
                public const string RequestedBy = "data[0].#subform[0].Tabella5[1].Riga1[0].Cella2[0]";
                public const string Order = "data[0].#subform[0].Tabella5[0].Riga1[0].#field[0]";
                public const string OrderType = "data[0].#subform[0].Tabella5[0].Riga1[1].#field[0]";
                
                public const string Mon_Date = "data[0].#subform[0].Tabella1[0].Riga2[0].DATA[0]";
                public const string Mon_TravelStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga2[0].ORAINIPM[0]";
                public const string Mon_WorkStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga2[0].ORAINILM[0]";
                public const string Mon_WorkEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga2[0].ORAENDLM[0]";
                public const string Mon_TravelEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga2[0].ORAENDAM[0]";
                public const string Mon_TravelStartTimePM = "data[0].#subform[0].Tabella1[0].Riga2[0].ORAINIPP[0]";
                public const string Mon_WorkStartTimePM = "data[0].#subform[0].Tabella1[0].Riga2[0].ORAINILP[0]";
                public const string Mon_WorkEndTimePM = "data[0].#subform[0].Tabella1[0].Riga2[0].ORAENDLP[0]";
                public const string Mon_TravelEndTimePM = "data[0].#subform[0].Tabella1[0].Riga2[0].ORAENDAP[0]";
                       
                public const string Tue_Date = "data[0].#subform[0].Tabella1[0].Riga3[0].DATA[0]";
                public const string Tue_TravelStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga3[0].ORAINIPM[0]";
                public const string Tue_WorkStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga3[0].ORAINILM[0]";
                public const string Tue_WorkEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga3[0].ORAENDLM[0]";
                public const string Tue_TravelEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga3[0].ORAENDAM[0]";
                public const string Tue_TravelStartTimePM = "data[0].#subform[0].Tabella1[0].Riga3[0].ORAINIPP[0]";
                public const string Tue_WorkStartTimePM = "data[0].#subform[0].Tabella1[0].Riga3[0].ORAINILP[0]";
                public const string Tue_WorkEndTimePM = "data[0].#subform[0].Tabella1[0].Riga3[0].ORAENDLP[0]";
                public const string Tue_TravelEndTimePM = "data[0].#subform[0].Tabella1[0].Riga3[0].ORAENDAP[0]";
                       
                public const string Wed_Date = "data[0].#subform[0].Tabella1[0].Riga4[0].DATA[0]";
                public const string Wed_TravelStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga4[0].ORAINIPM[0]";
                public const string Wed_WorkStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga4[0].ORAINILM[0]";
                public const string Wed_WorkEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga4[0].ORAENDLM[0]";
                public const string Wed_TravelEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga4[0].ORAENDAM[0]";
                public const string Wed_TravelStartTimePM = "data[0].#subform[0].Tabella1[0].Riga4[0].ORAINIPP[0]";
                public const string Wed_WorkStartTimePM = "data[0].#subform[0].Tabella1[0].Riga4[0].ORAINILP[0]";
                public const string Wed_WorkEndTimePM = "data[0].#subform[0].Tabella1[0].Riga4[0].ORAENDLP[0]";
                public const string Wed_TravelEndTimePM = "data[0].#subform[0].Tabella1[0].Riga4[0].ORAENDAP[0]";
                       
                public const string Thu_Date = "data[0].#subform[0].Tabella1[0].Riga5[0].DATA[0]";
                public const string Thu_TravelStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga5[0].ORAINIPM[0]";
                public const string Thu_WorkStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga5[0].ORAINILM[0]";
                public const string Thu_WorkEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga5[0].ORAENDLM[0]";
                public const string Thu_TravelEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga5[0].ORAENDAM[0]";
                public const string Thu_TravelStartTimePM = "data[0].#subform[0].Tabella1[0].Riga5[0].ORAINIPP[0]";
                public const string Thu_WorkStartTimePM = "data[0].#subform[0].Tabella1[0].Riga5[0].ORAINILP[0]";
                public const string Thu_WorkEndTimePM = "data[0].#subform[0].Tabella1[0].Riga5[0].ORAENDLP[0]";
                public const string Thu_TravelEndTimePM = "data[0].#subform[0].Tabella1[0].Riga5[0].ORAENDAP[0]";
                       
                public const string Fri_Date = "data[0].#subform[0].Tabella1[0].Riga6[0].DATA[0]";
                public const string Fri_TravelStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga6[0].ORAINIPM[0]";
                public const string Fri_WorkStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga6[0].ORAINILM[0]";
                public const string Fri_WorkEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga6[0].ORAENDLM[0]";
                public const string Fri_TravelEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga6[0].ORAENDAM[0]";
                public const string Fri_TravelStartTimePM = "data[0].#subform[0].Tabella1[0].Riga6[0].ORAINIPP[0]";
                public const string Fri_WorkStartTimePM = "data[0].#subform[0].Tabella1[0].Riga6[0].ORAINILP[0]";
                public const string Fri_WorkEndTimePM = "data[0].#subform[0].Tabella1[0].Riga6[0].ORAENDLP[0]";
                public const string Fri_TravelEndTimePM = "data[0].#subform[0].Tabella1[0].Riga6[0].ORAENDAP[0]";
                       
                public const string Sat_Date = "data[0].#subform[0].Tabella1[0].Riga7[0].DATA[0]";
                public const string Sat_TravelStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga7[0].ORAINIPM[0]";
                public const string Sat_WorkStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga7[0].ORAINILM[0]";
                public const string Sat_WorkEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga7[0].ORAENDLM[0]";
                public const string Sat_TravelEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga7[0].ORAENDAM[0]";
                public const string Sat_TravelStartTimePM = "data[0].#subform[0].Tabella1[0].Riga7[0].ORAINIPP[0]";
                public const string Sat_WorkStartTimePM = "data[0].#subform[0].Tabella1[0].Riga7[0].ORAINILP[0]";
                public const string Sat_WorkEndTimePM = "data[0].#subform[0].Tabella1[0].Riga7[0].ORAENDLP[0]";
                public const string Sat_TravelEndTimePM = "data[0].#subform[0].Tabella1[0].Riga7[0].ORAENDAP[0]";
                       
                public const string Sun_Date = "data[0].#subform[0].Tabella1[0].Riga7[1].DATA[0]";
                public const string Sun_TravelStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga7[1].ORAINIPM[0]";
                public const string Sun_WorkStartTimeAM = "data[0].#subform[0].Tabella1[0].Riga7[1].ORAINILM[0]";
                public const string Sun_WorkEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga7[1].ORAENDLM[0]";
                public const string Sun_TravelEndTimeAM = "data[0].#subform[0].Tabella1[0].Riga7[1].ORAENDAM[0]";
                public const string Sun_TravelStartTimePM = "data[0].#subform[0].Tabella1[0].Riga7[1].ORAINIPP[0]";
                public const string Sun_WorkStartTimePM = "data[0].#subform[0].Tabella1[0].Riga7[1].ORAINILP[0]";
                public const string Sun_WorkEndTimePM = "data[0].#subform[0].Tabella1[0].Riga7[1].ORAENDLP[0]";
                public const string Sun_TravelEndTimePM = "data[0].#subform[0].Tabella1[0].Riga7[1].ORAENDAP[0]";

                public const string Cars1 = "data[0].#subform[0].Tabella1[0].Riga7[3].Campoditesto16[0]";
                public const string Cars2 = "data[0].#subform[0].Tabella1[0].Riga7[4].Campoditesto17[0]";

                public const string OutwardCar = "data[0].#subform[0].Tabella1[0].Riga7[3].Caselladicontrollo1[0]";
                public const string OutwardTaxi = "data[0].#subform[0].Tabella1[0].Riga7[3].Caselladicontrollo3[0]";
                public const string OutwardAircraft = "data[0].#subform[0].Tabella1[0].Riga7[3].Caselladicontrollo5[0]";
                public const string ReturnCar = "data[0].#subform[0].Tabella1[0].Riga7[4].Caselladicontrollo2[0]";
                public const string ReturnTaxi = "data[0].#subform[0].Tabella1[0].Riga7[4].Caselladicontrollo4[0]";
                public const string ReturnAircraft = "data[0].#subform[0].Tabella1[0].Riga7[4].Caselladicontrollo6[0]";

                public const string PerformanceDescription = "data[0].#subform[0].Tabella4[1].Riga1[0].Cella1[0]";
                public const string PerformanceDescriptionDetails = "data[0].#subform[12].Tabella4[3].#subformSet[0].Riga1[1].Cella1[0]";
                public const string AssistantFinalTestResult = "data[0].#subform[0].Tabella4[2].Riga1[0].#field[0]";
                public const string Result = "data[0].#subform[0].Elencoadiscesa1[0]";
                public const string SoftwareVersionsOtherNotes = "data[0].#subform[0].Tabella16[0].Riga2[0].Cella1[0]";
            }
        }
        #endregion

        #region General
        public static class General
        {
            public const int WaitForNextConnectionRetry = 10000;
            public const int WaitForNextEmailCheck = 1000;
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
                    return password.Length > 0 ? password.Decrypt() : string.Empty;
                }
                set
                {
                    Properties.Settings.Default.EmailPassword = value.Encrypt();
                    Properties.Settings.Default.Save();
                }
            }
        }
        #endregion

        #region Advanced
        public static class Advanced
        {
            public static bool AutoAddFactories
            {
                get { return Properties.Settings.Default.AutoAddFactories; }
                set
                {
                    Properties.Settings.Default.AutoAddFactories = value;
                    Properties.Settings.Default.Save();
                }
            }

            public static bool AutoAssignFactories
            {
                get { return Properties.Settings.Default.AutoAssignFactories; }
                set
                {
                    Properties.Settings.Default.AutoAssignFactories = value;
                    Properties.Settings.Default.Save();
                }
            }
        }
        #endregion
    }
    #endregion
}
