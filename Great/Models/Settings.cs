using Great.Properties;
using Great.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
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
                    string connectionString = ConfigurationManager.ConnectionStrings["DBArchive"].ConnectionString;

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
                    SQLiteConnectionStringBuilder sqliteBuilder = new SQLiteConnectionStringBuilder(ConnectionString);                                        
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
                    return Data + "FDL\\";
                }
            }
            public static string ExpenseAccount
            {
                get
                {
                    return Data + "Expense Account\\";
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

            public static string Log
            {
                get
                {
                    return Data + "Log\\";
                }
            }
        }
        #endregion

        #region Email Recipients
        public static class EmailRecipients
        {
            public const int MaxRecentRecipientsCount = 10;

            public const string FDLSystem = "fdl@elettric80.it";
            public const string HR = "hr@elettric80.it";
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
            
            public const string FDL_Accepted = "FDL RECEIVED";
            public const string FDL_Rejected = "INVALID FDL";
            public const string FDL_Extra = "EXTRA";

            public const string Reminder = "REMINDER:";

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
                public const string AssistanceDescription = "data[0].#subform[0].Tabella4[0].Riga1[0].Cella1[0]";
                
                public const string FDLNumber2 = "data[0].#subform[12].Campoditesto1[1]";
                public const string Customer2 = "data[0].#subform[12].Tabella2[1].Riga1[0].Cella2[0]";
                public const string Address2 = "data[0].#subform[12].Tabella2[1].Riga2[0].Cella2[0]";
                public const string Technician2 = "data[0].#subform[12].Tabella3[1].Riga1[0].Cella1[0]";
                public const string CID2 = "data[0].#subform[12].Tabella3[1].Riga2[0].Cella1[0]";
                public const string RequestedBy2 = "data[0].#subform[12].Tabella5[6].Riga1[0].REQMR[0]";
                public const string Order2 = "data[0].#subform[12].Tabella5[3].Riga1[0].#field[0]";
                public const string OrderType2 = "data[0].#subform[12].Tabella5[3].Riga1[1].#field[0]";
                public const string AssistanceDescription2 = "data[0].#subform[12].Tabella4[3].Riga1[0].Cella1[0]";

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
                public const string PerformanceDescriptionDetails_old = "data[0].#subform[10].Tabella4[3].#subformSet[0].Riga1[1].Cella1[0]";
                public const string AssistantFinalTestResult = "data[0].#subform[0].Tabella4[2].Riga1[0].#field[0]";
                public const string Result = "data[0].#subform[0].Elencoadiscesa1[0]";
                public const string SoftwareVersionsOtherNotes = "data[0].#subform[0].Tabella16[0].Riga2[0].Cella1[0]";

                // Helpers
                #region TimesMatrix
                public static readonly Dictionary<DayOfWeek, Dictionary<string, string>> TimesMatrix = new Dictionary<DayOfWeek, Dictionary<string, string>>()
                {
                    {
                        DayOfWeek.Monday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Mon_Date },
                            { "TravelStartTimeAM", Mon_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Mon_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Mon_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Mon_TravelEndTimeAM },
                            { "TravelStartTimePM", Mon_TravelStartTimePM },
                            { "WorkStartTimePM",   Mon_WorkStartTimePM },
                            { "WorkEndTimePM",     Mon_WorkEndTimePM },
                            { "TravelEndTimePM",   Mon_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Tuesday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Tue_Date },
                            { "TravelStartTimeAM", Tue_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Tue_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Tue_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Tue_TravelEndTimeAM },
                            { "TravelStartTimePM", Tue_TravelStartTimePM },
                            { "WorkStartTimePM",   Tue_WorkStartTimePM },
                            { "WorkEndTimePM",     Tue_WorkEndTimePM },
                            { "TravelEndTimePM",   Tue_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Wednesday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Wed_Date },
                            { "TravelStartTimeAM", Wed_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Wed_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Wed_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Wed_TravelEndTimeAM },
                            { "TravelStartTimePM", Wed_TravelStartTimePM },
                            { "WorkStartTimePM",   Wed_WorkStartTimePM },
                            { "WorkEndTimePM",     Wed_WorkEndTimePM },
                            { "TravelEndTimePM",   Wed_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Thursday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Thu_Date },
                            { "TravelStartTimeAM", Thu_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Thu_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Thu_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Thu_TravelEndTimeAM },
                            { "TravelStartTimePM", Thu_TravelStartTimePM },
                            { "WorkStartTimePM",   Thu_WorkStartTimePM },
                            { "WorkEndTimePM",     Thu_WorkEndTimePM },
                            { "TravelEndTimePM",   Thu_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Friday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Fri_Date },
                            { "TravelStartTimeAM", Fri_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Fri_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Fri_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Fri_TravelEndTimeAM },
                            { "TravelStartTimePM", Fri_TravelStartTimePM },
                            { "WorkStartTimePM",   Fri_WorkStartTimePM },
                            { "WorkEndTimePM",     Fri_WorkEndTimePM },
                            { "TravelEndTimePM",   Fri_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Saturday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Sat_Date },
                            { "TravelStartTimeAM", Sat_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Sat_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Sat_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Sat_TravelEndTimeAM },
                            { "TravelStartTimePM", Sat_TravelStartTimePM },
                            { "WorkStartTimePM",   Sat_WorkStartTimePM },
                            { "WorkEndTimePM",     Sat_WorkEndTimePM },
                            { "TravelEndTimePM",   Sat_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Sunday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Sun_Date },
                            { "TravelStartTimeAM", Sun_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Sun_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Sun_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Sun_TravelEndTimeAM },
                            { "TravelStartTimePM", Sun_TravelStartTimePM },
                            { "WorkStartTimePM",   Sun_WorkStartTimePM },
                            { "WorkEndTimePM",     Sun_WorkEndTimePM },
                            { "TravelEndTimePM",   Sun_TravelEndTimePM }
                        }
                    }
                };
                #endregion
            }

            public static class XFAFieldNames
            {
                public const string FDLNumber = "data[0].ZFDLK[0].NUM_FOG[0]";
                public const string Customer = "data[0].ZFDLK[0].NAME[0]";
                public const string Address = "data[0].ZFDLK[0].ADDRESS[0]";
                public const string Technician = "data[0].ZFDLK[0].SNAME[0]";
                public const string CID = "data[0].ZFDLK[0].NUMCID[0]";
                public const string RequestedBy = "data[0].ZFDLK[0].REQMR[0]";
                public const string Order = "data[0].ZFDLK[0].ORD_PROD[0]";
                public const string OrderType = "data[0].ZFDLK[0].LTXA1[0]";
                public const string AssistanceDescription = "data[0].P_TEXT[0]";

                public const string Mon_Date = "data[0].ZFDLP1[0].DATA[0]";
                public const string Mon_TravelStartTimeAM = "data[0].ZFDLP1[0].ORAINIPM[0]";
                public const string Mon_WorkStartTimeAM = "data[0].ZFDLP1[0].ORAINILM[0]";
                public const string Mon_WorkEndTimeAM = "data[0].ZFDLP1[0].ORAENDLM[0]";
                public const string Mon_TravelEndTimeAM = "data[0].ZFDLP1[0].ORAENDAM[0]";
                public const string Mon_TravelStartTimePM = "data[0].ZFDLP1[0].ORAINIPP[0]";
                public const string Mon_WorkStartTimePM = "data[0].ZFDLP1[0].ORAINILP[0]";
                public const string Mon_WorkEndTimePM = "data[0].ZFDLP1[0].ORAENDLP[0]";
                public const string Mon_TravelEndTimePM = "data[0].ZFDLP1[0].ORAENDAP[0]";

                public const string Tue_Date = "data[0].ZFDLP2[0].DATA[0]";
                public const string Tue_TravelStartTimeAM = "data[0].ZFDLP2[0].ORAINIPM[0]";
                public const string Tue_WorkStartTimeAM = "data[0].ZFDLP2[0].ORAINILM[0]";
                public const string Tue_WorkEndTimeAM = "data[0].ZFDLP2[0].ORAENDLM[0]";
                public const string Tue_TravelEndTimeAM = "data[0].ZFDLP2[0].ORAENDAM[0]";
                public const string Tue_TravelStartTimePM = "data[0].ZFDLP2[0].ORAINIPP[0]";
                public const string Tue_WorkStartTimePM = "data[0].ZFDLP2[0].ORAINILP[0]";
                public const string Tue_WorkEndTimePM = "data[0].ZFDLP2[0].ORAENDLP[0]";
                public const string Tue_TravelEndTimePM = "data[0].ZFDLP2[0].ORAENDAP[0]";

                public const string Wed_Date = "data[0].ZFDLP3[0].DATA[0]";
                public const string Wed_TravelStartTimeAM = "data[0].ZFDLP3[0].ORAINIPM[0]";
                public const string Wed_WorkStartTimeAM = "data[0].ZFDLP3[0].ORAINILM[0]";
                public const string Wed_WorkEndTimeAM = "data[0].ZFDLP3[0].ORAENDLM[0]";
                public const string Wed_TravelEndTimeAM = "data[0].ZFDLP3[0].ORAENDAM[0]";
                public const string Wed_TravelStartTimePM = "data[0].ZFDLP3[0].ORAINIPP[0]";
                public const string Wed_WorkStartTimePM = "data[0].ZFDLP3[0].ORAINILP[0]";
                public const string Wed_WorkEndTimePM = "data[0].ZFDLP3[0].ORAENDLP[0]";
                public const string Wed_TravelEndTimePM = "data[0].ZFDLP3[0].ORAENDAP[0]";

                public const string Thu_Date = "data[0].ZFDLP4[0].DATA[0]";
                public const string Thu_TravelStartTimeAM = "data[0].ZFDLP4[0].ORAINIPM[0]";
                public const string Thu_WorkStartTimeAM = "data[0].ZFDLP4[0].ORAINILM[0]";
                public const string Thu_WorkEndTimeAM = "data[0].ZFDLP4[0].ORAENDLM[0]";
                public const string Thu_TravelEndTimeAM = "data[0].ZFDLP4[0].ORAENDAM[0]";
                public const string Thu_TravelStartTimePM = "data[0].ZFDLP4[0].ORAINIPP[0]";
                public const string Thu_WorkStartTimePM = "data[0].ZFDLP4[0].ORAINILP[0]";
                public const string Thu_WorkEndTimePM = "data[0].ZFDLP4[0].ORAENDLP[0]";
                public const string Thu_TravelEndTimePM = "data[0].ZFDLP4[0].ORAENDAP[0]";

                public const string Fri_Date = "data[0].ZFDLP5[0].DATA[0]";
                public const string Fri_TravelStartTimeAM = "data[0].ZFDLP5[0].ORAINIPM[0]";
                public const string Fri_WorkStartTimeAM = "data[0].ZFDLP5[0].ORAINILM[0]";
                public const string Fri_WorkEndTimeAM = "data[0].ZFDLP5[0].ORAENDLM[0]";
                public const string Fri_TravelEndTimeAM = "data[0].ZFDLP5[0].ORAENDAM[0]";
                public const string Fri_TravelStartTimePM = "data[0].ZFDLP5[0].ORAINIPP[0]";
                public const string Fri_WorkStartTimePM = "data[0].ZFDLP5[0].ORAINILP[0]";
                public const string Fri_WorkEndTimePM = "data[0].ZFDLP5[0].ORAENDLP[0]";
                public const string Fri_TravelEndTimePM = "data[0].ZFDLP5[0].ORAENDAP[0]";

                public const string Sat_Date = "data[0].ZFDLP6[0].DATA[0]";
                public const string Sat_TravelStartTimeAM = "data[0].ZFDLP6[0].ORAINIPM[0]";
                public const string Sat_WorkStartTimeAM = "data[0].ZFDLP6[0].ORAINILM[0]";
                public const string Sat_WorkEndTimeAM = "data[0].ZFDLP6[0].ORAENDLM[0]";
                public const string Sat_TravelEndTimeAM = "data[0].ZFDLP6[0].ORAENDAM[0]";
                public const string Sat_TravelStartTimePM = "data[0].ZFDLP6[0].ORAINIPP[0]";
                public const string Sat_WorkStartTimePM = "data[0].ZFDLP6[0].ORAINILP[0]";
                public const string Sat_WorkEndTimePM = "data[0].ZFDLP6[0].ORAENDLP[0]";
                public const string Sat_TravelEndTimePM = "data[0].ZFDLP6[0].ORAENDAP[0]";

                public const string Sun_Date = "data[0].ZFDLP7[0].DATA[0]";
                public const string Sun_TravelStartTimeAM = "data[0].ZFDLP7[0].ORAINIPM[0]";
                public const string Sun_WorkStartTimeAM = "data[0].ZFDLP7[0].ORAINILM[0]";
                public const string Sun_WorkEndTimeAM = "data[0].ZFDLP7[0].ORAENDLM[0]";
                public const string Sun_TravelEndTimeAM = "data[0].ZFDLP7[0].ORAENDAM[0]";
                public const string Sun_TravelStartTimePM = "data[0].ZFDLP7[0].ORAINIPP[0]";
                public const string Sun_WorkStartTimePM = "data[0].ZFDLP7[0].ORAINILP[0]";
                public const string Sun_WorkEndTimePM = "data[0].ZFDLP7[0].ORAENDLP[0]";
                public const string Sun_TravelEndTimePM = "data[0].ZFDLP7[0].ORAENDAP[0]";

                public const string Cars1 = "not implemented";
                public const string Cars2 = "not implemented";

                public const string OutwardCar = "not implemented";
                public const string OutwardTaxi = "not implemented";
                public const string OutwardAircraft = "not implemented";
                public const string ReturnCar = "not implemented";
                public const string ReturnTaxi = "not implemented";
                public const string ReturnAircraft = "not implemented";

                public const string PerformanceDescription = "not implemented";
                public const string PerformanceDescriptionDetails = "not implemented";
                public const string AssistantFinalTestResult = "not implemented";
                public const string Result = "not implemented";
                public const string SoftwareVersionsOtherNotes = "not implemented";

                // Helpers
                #region TimesMatrix
                public static readonly Dictionary<DayOfWeek, Dictionary<string, string>> TimesMatrix = new Dictionary<DayOfWeek, Dictionary<string, string>>()
                {
                    {
                        DayOfWeek.Monday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Mon_Date },
                            { "TravelStartTimeAM", Mon_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Mon_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Mon_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Mon_TravelEndTimeAM },
                            { "TravelStartTimePM", Mon_TravelStartTimePM },
                            { "WorkStartTimePM",   Mon_WorkStartTimePM },
                            { "WorkEndTimePM",     Mon_WorkEndTimePM },
                            { "TravelEndTimePM",   Mon_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Tuesday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Tue_Date },
                            { "TravelStartTimeAM", Tue_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Tue_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Tue_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Tue_TravelEndTimeAM },
                            { "TravelStartTimePM", Tue_TravelStartTimePM },
                            { "WorkStartTimePM",   Tue_WorkStartTimePM },
                            { "WorkEndTimePM",     Tue_WorkEndTimePM },
                            { "TravelEndTimePM",   Tue_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Wednesday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Wed_Date },
                            { "TravelStartTimeAM", Wed_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Wed_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Wed_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Wed_TravelEndTimeAM },
                            { "TravelStartTimePM", Wed_TravelStartTimePM },
                            { "WorkStartTimePM",   Wed_WorkStartTimePM },
                            { "WorkEndTimePM",     Wed_WorkEndTimePM },
                            { "TravelEndTimePM",   Wed_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Thursday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Thu_Date },
                            { "TravelStartTimeAM", Thu_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Thu_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Thu_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Thu_TravelEndTimeAM },
                            { "TravelStartTimePM", Thu_TravelStartTimePM },
                            { "WorkStartTimePM",   Thu_WorkStartTimePM },
                            { "WorkEndTimePM",     Thu_WorkEndTimePM },
                            { "TravelEndTimePM",   Thu_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Friday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Fri_Date },
                            { "TravelStartTimeAM", Fri_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Fri_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Fri_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Fri_TravelEndTimeAM },
                            { "TravelStartTimePM", Fri_TravelStartTimePM },
                            { "WorkStartTimePM",   Fri_WorkStartTimePM },
                            { "WorkEndTimePM",     Fri_WorkEndTimePM },
                            { "TravelEndTimePM",   Fri_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Saturday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Sat_Date },
                            { "TravelStartTimeAM", Sat_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Sat_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Sat_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Sat_TravelEndTimeAM },
                            { "TravelStartTimePM", Sat_TravelStartTimePM },
                            { "WorkStartTimePM",   Sat_WorkStartTimePM },
                            { "WorkEndTimePM",     Sat_WorkEndTimePM },
                            { "TravelEndTimePM",   Sat_TravelEndTimePM }
                        }
                    },
                    {
                        DayOfWeek.Sunday,
                        new Dictionary<string, string>
                        {
                            { "Date",              Sun_Date },
                            { "TravelStartTimeAM", Sun_TravelStartTimeAM },
                            { "WorkStartTimeAM",   Sun_WorkStartTimeAM },
                            { "WorkEndTimeAM",     Sun_WorkEndTimeAM },
                            { "TravelEndTimeAM",   Sun_TravelEndTimeAM },
                            { "TravelStartTimePM", Sun_TravelStartTimePM },
                            { "WorkStartTimePM",   Sun_WorkStartTimePM },
                            { "WorkEndTimePM",     Sun_WorkEndTimePM },
                            { "TravelEndTimePM",   Sun_TravelEndTimePM }
                        }
                    }
                };
                #endregion
            }

        }
        #endregion

        #region Expense Account
        public static class ExpenseAccount
        {
            public const int NotesMaxLength = 352;

            public const string EA_Accepted = "NOTA SPESE ACCETTATA";
            public const string EA_Rejected = "NOTA SPESE RIFIUTATA";
            public const string EA_RejectedResubmission = "Reinvio nota spese";

            public static class FieldNames
            {
                public const string Technician = "data[0].#subform[0].W_SNAME[0]";
                public const string DateFrom = "data[0].#subform[0].DATA[0]";
                public const string DateTo = "data[0].#subform[0].DATA[1]";
                public const string Customer = "data[0].#subform[0].W_CLIENT[0]";
                public const string Address = "data[0].#subform[0].W_ADRESS[0]";
                public const string Currency = "data[0].#subform[0].Elencoadiscesa1[0]";
                public const string CdC = "data[0].#subform[0].W_CdC[0]";
                public const string FDLNumber = "data[0].#subform[0].Sottomodulo1[0].NUM_FOG[0]";
                public const string PSPNR = "data[0].#subform[0].Sottomodulo1[0].PSPNR[0]";
                public const string Order = "data[0].#subform[0].Sottomodulo1[0].COMMESSA[0]";
                public const string InternalOrder = "data[0].#subform[0].Sottomodulo1[0].ORD_INTE[0]";
                public const string PAG_CC1 = "data[0].#subform[0].Sottomodulo1[0].PAG_CC[0]";
                public const string PAG_CC2 = "data[0].#subform[0].Sottomodulo1[0].PAG_CC[1]";
                public const string SignatureDate = "data[0].#subform[0].data[0]";
                public const string Notes = "data[0].#subform[0].Campoditesto1[0]";

                public const string Mon_Date = "data[0].#subform[0].Tabella6[0].RigaIntestazione[0].Cella4[0]";
                public const string Tue_Date = "data[0].#subform[0].Tabella6[0].RigaIntestazione[0].Cella5[0]";
                public const string Wed_Date = "data[0].#subform[0].Tabella6[0].RigaIntestazione[0].Cella6[0]";
                public const string Thu_Date = "data[0].#subform[0].Tabella6[0].RigaIntestazione[0].Cella7[0]";
                public const string Fri_Date = "data[0].#subform[0].Tabella6[0].RigaIntestazione[0].Cella8[0]";
                public const string Sat_Date = "data[0].#subform[0].Tabella6[0].RigaIntestazione[0].Cella9[0]";
                public const string Sun_Date = "data[0].#subform[0].Tabella6[0].RigaIntestazione[0].Cella10[0]";

                public const string Total_Mon = "data[0].#subform[0].Tabella6[0].Riga8[1].totday1[0]";
                public const string Total_Tue = "data[0].#subform[0].Tabella6[0].Riga8[1].totday2[0]";
                public const string Total_Wed = "data[0].#subform[0].Tabella6[0].Riga8[1].totday3[0]";
                public const string Total_Thu = "data[0].#subform[0].Tabella6[0].Riga8[1].totday4[0]";
                public const string Total_Fri = "data[0].#subform[0].Tabella6[0].Riga8[1].totday5[0]";
                public const string Total_Sat = "data[0].#subform[0].Tabella6[0].Riga8[1].totday6[0]";
                public const string Total_Sun = "data[0].#subform[0].Tabella6[0].Riga8[1].totday7[0]";
                public const string SubTotal = "data[0].#subform[0].Tabella6[0].Riga8[1].tottotale[0]";
                public const string Total = "data[0].#subform[0].Tabella6[0].Riga8[2].Totalespese[0]";

                public const string EX1_Type = "data[0].#subform[0].Tabella6[0].Riga1[0].r1Spesa[0]";
                public const string EX1_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga1[0].r1day1[0]";
                public const string EX1_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga1[0].r1day2[0]";
                public const string EX1_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga1[0].r1day3[0]";
                public const string EX1_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga1[0].r1day4[0]";
                public const string EX1_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga1[0].r1day5[0]";
                public const string EX1_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga1[0].r1day6[0]";
                public const string EX1_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga1[0].r1day7[0]";
                public const string EX1_Total = "data[0].#subform[0].Tabella6[0].Riga1[0].r1totale[0]";

                public const string EX2_Type = "data[0].#subform[0].Tabella6[0].Riga2[0].r2spesa[0]";
                public const string EX2_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga2[0].r2day1[0]";
                public const string EX2_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga2[0].r2day2[0]";
                public const string EX2_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga2[0].r2day3[0]";
                public const string EX2_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga2[0].r2day4[0]";
                public const string EX2_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga2[0].r2day5[0]";
                public const string EX2_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga2[0].r2day6[0]";
                public const string EX2_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga2[0].r2day7[0]";
                public const string EX2_Total = "data[0].#subform[0].Tabella6[0].Riga2[0].r2totale[0]";

                public const string EX3_Type = "data[0].#subform[0].Tabella6[0].Riga3[0].r3spesa[0]";
                public const string EX3_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga3[0].r3day1[0]";
                public const string EX3_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga3[0].r3day2[0]";
                public const string EX3_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga3[0].r3day3[0]";
                public const string EX3_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga3[0].r3day4[0]";
                public const string EX3_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga3[0].r3day5[0]";
                public const string EX3_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga3[0].r3day6[0]";
                public const string EX3_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga3[0].r3day7[0]";
                public const string EX3_Total = "data[0].#subform[0].Tabella6[0].Riga3[0].r3totale[0]";

                public const string EX4_Type = "data[0].#subform[0].Tabella6[0].Riga4[0].r4spesa[0]";
                public const string EX4_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga4[0].r4day1[0]";
                public const string EX4_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga4[0].r4day2[0]";
                public const string EX4_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga4[0].r4day3[0]";
                public const string EX4_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga4[0].r4day4[0]";
                public const string EX4_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga4[0].r4day5[0]";
                public const string EX4_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga4[0].r4day6[0]";
                public const string EX4_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga4[0].r4day7[0]";
                public const string EX4_Total = "data[0].#subform[0].Tabella6[0].Riga4[0].r4totale[0]";

                public const string EX5_Type = "data[0].#subform[0].Tabella6[0].Riga5[0].r5spesa[0]";
                public const string EX5_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga5[0].r5day1[0]";
                public const string EX5_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga5[0].r5day2[0]";
                public const string EX5_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga5[0].r5day3[0]";
                public const string EX5_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga5[0].r5day4[0]";
                public const string EX5_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga5[0].r5day5[0]";
                public const string EX5_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga5[0].r5day6[0]";
                public const string EX5_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga5[0].r5day7[0]";
                public const string EX5_Total = "data[0].#subform[0].Tabella6[0].Riga5[0].r5totale[0]";

                public const string EX6_Type = "data[0].#subform[0].Tabella6[0].Riga6[0].r6spesa[0]";
                public const string EX6_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga6[0].r6day1[0]";
                public const string EX6_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga6[0].r6day2[0]";
                public const string EX6_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga6[0].r6day3[0]";
                public const string EX6_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga6[0].r6day4[0]";
                public const string EX6_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga6[0].r6day5[0]";
                public const string EX6_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga6[0].r6day6[0]";
                public const string EX6_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga6[0].r6day7[0]";
                public const string EX6_Total = "data[0].#subform[0].Tabella6[0].Riga6[0].r6totale[0]";

                public const string EX7_Type = "data[0].#subform[0].Tabella6[0].Riga7[0].r7spesa[0]";
                public const string EX7_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga7[0].r7day1[0]";
                public const string EX7_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga7[0].r7day2[0]";
                public const string EX7_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga7[0].r7day3[0]";
                public const string EX7_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga7[0].r7day4[0]";
                public const string EX7_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga7[0].r7day5[0]";
                public const string EX7_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga7[0].r7day6[0]";
                public const string EX7_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga7[0].r7day7[0]";
                public const string EX7_Total = "data[0].#subform[0].Tabella6[0].Riga7[0].r7totale[0]";

                public const string EX8_Type = "data[0].#subform[0].Tabella6[0].Riga8[0].r8spesa[0]";
                public const string EX8_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga8[0].r8day1[0]";
                public const string EX8_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga8[0].r8day2[0]";
                public const string EX8_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga8[0].r8day3[0]";
                public const string EX8_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga8[0].r8day4[0]";
                public const string EX8_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga8[0].r8day5[0]";
                public const string EX8_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga8[0].r8day6[0]";
                public const string EX8_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga8[0].r8day7[0]";
                public const string EX8_Total = "data[0].#subform[0].Tabella6[0].Riga8[0].r8totale[0]";

                public const string EX9_Type = "data[0].#subform[0].Tabella6[0].Riga9[0].r9spesa[0]";
                public const string EX9_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga9[0].r9day1[0]";
                public const string EX9_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga9[0].r9day2[0]";
                public const string EX9_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga9[0].r9day3[0]";
                public const string EX9_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga9[0].r9day4[0]";
                public const string EX9_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga9[0].r9day5[0]";
                public const string EX9_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga9[0].r9day6[0]";
                public const string EX9_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga9[0].r9day7[0]";
                public const string EX9_Total = "data[0].#subform[0].Tabella6[0].Riga9[0].r9totale[0]";

                public const string EX10_Type = "data[0].#subform[0].Tabella6[0].Riga0[0].r0spesa[0]";
                public const string EX10_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga0[0].r0day1[0]";
                public const string EX10_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga0[0].r0day2[0]";
                public const string EX10_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga0[0].r0day3[0]";
                public const string EX10_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga0[0].r0day4[0]";
                public const string EX10_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga0[0].r0day5[0]";
                public const string EX10_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga0[0].r0day6[0]";
                public const string EX10_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga0[0].r0day7[0]";
                public const string EX10_Total = "data[0].#subform[0].Tabella6[0].Riga0[0].r0totale[0]";

                public const string EX11_Type = "data[0].#subform[0].Tabella6[0].Riga11[0].r11spesa[0]";
                public const string EX11_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga11[0].r11day1[0]";
                public const string EX11_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga11[0].r11day2[0]";
                public const string EX11_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga11[0].r11day3[0]";
                public const string EX11_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga11[0].r11day4[0]";
                public const string EX11_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga11[0].r11day5[0]";
                public const string EX11_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga11[0].r11day6[0]";
                public const string EX11_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga11[0].r11day7[0]";
                public const string EX11_Total = "data[0].#subform[0].Tabella6[0].Riga11[0].r11totale[0]";

                public const string EX12_Type = "data[0].#subform[0].Tabella6[0].Riga12[0].r12spesa[0]";
                public const string EX12_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga12[0].r12day1[0]";
                public const string EX12_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga12[0].r12day2[0]";
                public const string EX12_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga12[0].r12day3[0]";
                public const string EX12_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga12[0].r12day4[0]";
                public const string EX12_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga12[0].r12day5[0]";
                public const string EX12_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga12[0].r12day6[0]";
                public const string EX12_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga12[0].r12day7[0]";
                public const string EX12_Total = "data[0].#subform[0].Tabella6[0].Riga12[0].r12totale[0]";

                public const string EX13_Type = "data[0].#subform[0].Tabella6[0].Riga13[0].r13spesa[0]";
                public const string EX13_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga13[0].r13day1[0]";
                public const string EX13_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga13[0].r13day2[0]";
                public const string EX13_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga13[0].r13day3[0]";
                public const string EX13_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga13[0].r13day4[0]";
                public const string EX13_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga13[0].r13day5[0]";
                public const string EX13_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga13[0].r13day6[0]";
                public const string EX13_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga13[0].r13day7[0]";
                public const string EX13_Total = "data[0].#subform[0].Tabella6[0].Riga13[0].r13totale[0]";

                public const string EX14_Type = "data[0].#subform[0].Tabella6[0].Riga14[0].r14spesa[0]";
                public const string EX14_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga14[0].r14day1[0]";
                public const string EX14_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga14[0].r14day2[0]";
                public const string EX14_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga14[0].r14day3[0]";
                public const string EX14_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga14[0].r14day4[0]";
                public const string EX14_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga14[0].r14day5[0]";
                public const string EX14_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga14[0].r14day6[0]";
                public const string EX14_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga14[0].r14day7[0]";
                public const string EX14_Total = "data[0].#subform[0].Tabella6[0].Riga14[0].r14totale[0]";

                public const string EX15_Type = "data[0].#subform[0].Tabella6[0].Riga15[0].r15spesa[0]";
                public const string EX15_Mon_Amount = "data[0].#subform[0].Tabella6[0].Riga15[0].r15day1[0]";
                public const string EX15_Tue_Amount = "data[0].#subform[0].Tabella6[0].Riga15[0].r15day2[0]";
                public const string EX15_Wed_Amount = "data[0].#subform[0].Tabella6[0].Riga15[0].r15day3[0]";
                public const string EX15_Thu_Amount = "data[0].#subform[0].Tabella6[0].Riga15[0].r15day4[0]";
                public const string EX15_Fri_Amount = "data[0].#subform[0].Tabella6[0].Riga15[0].r15day5[0]";
                public const string EX15_Sat_Amount = "data[0].#subform[0].Tabella6[0].Riga15[0].r15day6[0]";
                public const string EX15_Sun_Amount = "data[0].#subform[0].Tabella6[0].Riga15[0].r15day7[0]";
                public const string EX15_Total = "data[0].#subform[0].Tabella6[0].Riga15[0].r15totale[0]";

                // Helpers
                #region ExpenseMatrix
                public static readonly Dictionary<string, string>[] ExpenseMatrix = new Dictionary<string, string>[]
                {   
                    new Dictionary<string, string>
                    {
                        { "Type",       EX1_Type},
                        { "Mon_Amount", EX1_Mon_Amount},
                        { "Tue_Amount", EX1_Tue_Amount},
                        { "Wed_Amount", EX1_Wed_Amount},
                        { "Thu_Amount", EX1_Thu_Amount},
                        { "Fri_Amount", EX1_Fri_Amount},
                        { "Sat_Amount", EX1_Sat_Amount},
                        { "Sun_Amount", EX1_Sun_Amount},
                        { "Total",      EX1_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX2_Type},
                        { "Mon_Amount", EX2_Mon_Amount},
                        { "Tue_Amount", EX2_Tue_Amount},
                        { "Wed_Amount", EX2_Wed_Amount},
                        { "Thu_Amount", EX2_Thu_Amount},
                        { "Fri_Amount", EX2_Fri_Amount},
                        { "Sat_Amount", EX2_Sat_Amount},
                        { "Sun_Amount", EX2_Sun_Amount},
                        { "Total",      EX2_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX3_Type},
                        { "Mon_Amount", EX3_Mon_Amount},
                        { "Tue_Amount", EX3_Tue_Amount},
                        { "Wed_Amount", EX3_Wed_Amount},
                        { "Thu_Amount", EX3_Thu_Amount},
                        { "Fri_Amount", EX3_Fri_Amount},
                        { "Sat_Amount", EX3_Sat_Amount},
                        { "Sun_Amount", EX3_Sun_Amount},
                        { "Total",      EX3_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX4_Type},
                        { "Mon_Amount", EX4_Mon_Amount},
                        { "Tue_Amount", EX4_Tue_Amount},
                        { "Wed_Amount", EX4_Wed_Amount},
                        { "Thu_Amount", EX4_Thu_Amount},
                        { "Fri_Amount", EX4_Fri_Amount},
                        { "Sat_Amount", EX4_Sat_Amount},
                        { "Sun_Amount", EX4_Sun_Amount},
                        { "Total",      EX4_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX5_Type},
                        { "Mon_Amount", EX5_Mon_Amount},
                        { "Tue_Amount", EX5_Tue_Amount},
                        { "Wed_Amount", EX5_Wed_Amount},
                        { "Thu_Amount", EX5_Thu_Amount},
                        { "Fri_Amount", EX5_Fri_Amount},
                        { "Sat_Amount", EX5_Sat_Amount},
                        { "Sun_Amount", EX5_Sun_Amount},
                        { "Total",      EX5_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX6_Type},
                        { "Mon_Amount", EX6_Mon_Amount},
                        { "Tue_Amount", EX6_Tue_Amount},
                        { "Wed_Amount", EX6_Wed_Amount},
                        { "Thu_Amount", EX6_Thu_Amount},
                        { "Fri_Amount", EX6_Fri_Amount},
                        { "Sat_Amount", EX6_Sat_Amount},
                        { "Sun_Amount", EX6_Sun_Amount},
                        { "Total",      EX6_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX7_Type},
                        { "Mon_Amount", EX7_Mon_Amount},
                        { "Tue_Amount", EX7_Tue_Amount},
                        { "Wed_Amount", EX7_Wed_Amount},
                        { "Thu_Amount", EX7_Thu_Amount},
                        { "Fri_Amount", EX7_Fri_Amount},
                        { "Sat_Amount", EX7_Sat_Amount},
                        { "Sun_Amount", EX7_Sun_Amount},
                        { "Total",      EX7_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX8_Type},
                        { "Mon_Amount", EX8_Mon_Amount},
                        { "Tue_Amount", EX8_Tue_Amount},
                        { "Wed_Amount", EX8_Wed_Amount},
                        { "Thu_Amount", EX8_Thu_Amount},
                        { "Fri_Amount", EX8_Fri_Amount},
                        { "Sat_Amount", EX8_Sat_Amount},
                        { "Sun_Amount", EX8_Sun_Amount},
                        { "Total",      EX8_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX9_Type},
                        { "Mon_Amount", EX9_Mon_Amount},
                        { "Tue_Amount", EX9_Tue_Amount},
                        { "Wed_Amount", EX9_Wed_Amount},
                        { "Thu_Amount", EX9_Thu_Amount},
                        { "Fri_Amount", EX9_Fri_Amount},
                        { "Sat_Amount", EX9_Sat_Amount},
                        { "Sun_Amount", EX9_Sun_Amount},
                        { "Total",      EX9_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX10_Type},
                        { "Mon_Amount", EX10_Mon_Amount},
                        { "Tue_Amount", EX10_Tue_Amount},
                        { "Wed_Amount", EX10_Wed_Amount},
                        { "Thu_Amount", EX10_Thu_Amount},
                        { "Fri_Amount", EX10_Fri_Amount},
                        { "Sat_Amount", EX10_Sat_Amount},
                        { "Sun_Amount", EX10_Sun_Amount},
                        { "Total",      EX10_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX11_Type},
                        { "Mon_Amount", EX11_Mon_Amount},
                        { "Tue_Amount", EX11_Tue_Amount},
                        { "Wed_Amount", EX11_Wed_Amount},
                        { "Thu_Amount", EX11_Thu_Amount},
                        { "Fri_Amount", EX11_Fri_Amount},
                        { "Sat_Amount", EX11_Sat_Amount},
                        { "Sun_Amount", EX11_Sun_Amount},
                        { "Total",      EX11_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX12_Type},
                        { "Mon_Amount", EX12_Mon_Amount},
                        { "Tue_Amount", EX12_Tue_Amount},
                        { "Wed_Amount", EX12_Wed_Amount},
                        { "Thu_Amount", EX12_Thu_Amount},
                        { "Fri_Amount", EX12_Fri_Amount},
                        { "Sat_Amount", EX12_Sat_Amount},
                        { "Sun_Amount", EX12_Sun_Amount},
                        { "Total",      EX12_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX13_Type},
                        { "Mon_Amount", EX13_Mon_Amount},
                        { "Tue_Amount", EX13_Tue_Amount},
                        { "Wed_Amount", EX13_Wed_Amount},
                        { "Thu_Amount", EX13_Thu_Amount},
                        { "Fri_Amount", EX13_Fri_Amount},
                        { "Sat_Amount", EX13_Sat_Amount},
                        { "Sun_Amount", EX13_Sun_Amount},
                        { "Total",      EX13_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX14_Type},
                        { "Mon_Amount", EX14_Mon_Amount},
                        { "Tue_Amount", EX14_Tue_Amount},
                        { "Wed_Amount", EX14_Wed_Amount},
                        { "Thu_Amount", EX14_Thu_Amount},
                        { "Fri_Amount", EX14_Fri_Amount},
                        { "Sat_Amount", EX14_Sat_Amount},
                        { "Sun_Amount", EX14_Sun_Amount},
                        { "Total",      EX14_Total}
                    },
                    new Dictionary<string, string>
                    {
                        { "Type",       EX15_Type},
                        { "Mon_Amount", EX15_Mon_Amount},
                        { "Tue_Amount", EX15_Tue_Amount},
                        { "Wed_Amount", EX15_Wed_Amount},
                        { "Thu_Amount", EX15_Thu_Amount},
                        { "Fri_Amount", EX15_Fri_Amount},
                        { "Sat_Amount", EX15_Sat_Amount},
                        { "Sun_Amount", EX15_Sun_Amount},
                        { "Total",      EX15_Total}
                    }
                };
                #endregion
            }
        }
        #endregion

        #region General
        public static class General
        {
            public const int WaitForNextConnectionRetry = 10000;
            public const int WaitForNextEmailCheck = 1000;
            public const int WaitForCredentialsCheck = 1000;
        }
        #endregion

        #region Map
        public static class Map
        {
            public const double ZoomMarker = 15;
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

        #region Factories
        public static class Factories
        {
            public const int FactoryNameMaxLength = 50;
            public const int CompanyNameMaxLength = 100;
            public const int AddressMaxLength = 255;
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
            Settings.Default.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
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
                get { return Settings.Default.EmailAddress; }
                set
                {
                    Settings.Default.EmailAddress = value;
                    Settings.Default.Save();
                }
            }

            public static string EmailPassword
            {
                get
                {
                    string password = Settings.Default.EmailPassword;
                    return password.Length > 0 ? password.Decrypt() : string.Empty;
                }
                set
                {
                    Settings.Default.EmailPassword = value.Encrypt();
                    Settings.Default.Save();
                }
            }

            public static class Recipients
            {
                public static StringCollection RecentEmailRecipients
                {
                    get
                    {
                        return Settings.Default.RecentEmailRecipients;
                    }
                    set
                    {
                        Settings.Default.RecentEmailRecipients = value;
                        Settings.Default.Save();
                    }
                }

                public static StringCollection FDLCancelRequest
                {
                    get
                    {
                        return Settings.Default.FDLCancelRequestRecipients;
                    }
                    set
                    {
                        Settings.Default.FDLCancelRequestRecipients = value;
                        Settings.Default.Save();
                    }
                }
            }
        }
        #endregion

        #region Advanced
        public static class Advanced
        {
            public static bool AutoAddFactories
            {
                get { return Settings.Default.AutoAddFactories; }
                set
                {
                    Settings.Default.AutoAddFactories = value;
                    Settings.Default.Save();
                }
            }

            public static bool AutoAssignFactories
            {
                get { return Settings.Default.AutoAssignFactories; }
                set
                {
                    Settings.Default.AutoAssignFactories = value;
                    Settings.Default.Save();
                }
            }
        }
        #endregion
    }
    #endregion
}
