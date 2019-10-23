using GalaSoft.MvvmLight.Ioc;
using Great2.Models;
using Great2.Models.Database;
using Great2.ViewModels.Database;
using NLog;
using System;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading;

namespace Great2.Utils
{
    public class FDLImport : BaseImport
    {
        #region Constants

        public const string sGreatDefaultInstallationFolder = @"C:\Program Files (x86)\GREAT";
        const string sAccessConnectionstring = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";
        const string sSqliteConnectionString = @"data source={0}";
        const string sGreatIniFilePath = @"Ini\Setting.ini";
        string[] sPathToCheck = new string[] { @"C:\Program Files" };

        #endregion

        #region Properties
        private FDLManager FDLManager = SimpleIoc.Default.GetInstance<FDLManager>();
        private volatile bool stopImport;
        private Thread thrd;

        public string FDLFolder { get; set; }
        #endregion

        public FDLImport() : base(LogManager.GetLogger("FDLImport")) { }

        public override void Start()
        {
            stopImport = false;

            StatusChanged("Import Started...");

            if (Directory.Exists(FDLFolder))
            {
                thrd = new Thread(new ThreadStart(ImportThread));
                thrd.Start();
                return;
            }
            else Error($"Wrong FDL directory path: {FDLFolder}");

            StatusChanged("Import failed!");
            Finished(false);
        }

        public override void Cancel()
        {
            stopImport = true;
        }

        public override void Close()
        {
        }

        private void ImportThread()
        {
            ImportFDLFiles();
            ImportEAFiles();

            if (!stopImport)
            {
                StatusChanged("Operation Completed!");
                Finished();
            }
            else
            {
                Message("Import stopped by user.");
                Finished(false);
            }
        }

        private bool ImportFDLFiles()
        {
            bool result = false;

            if (stopImport)
                return result;

            StatusChanged("Importing FDL files...");

            try
            {
                foreach (FileInfo file in new DirectoryInfo(FDLFolder).GetFiles("*.pdf", SearchOption.AllDirectories))
                {
                    if (stopImport)
                        break;

                    if (FDLManager.GetFileType(file.Name) != EFileType.FDL)
                        continue;

                    FDLEVM fdl = null;

                    try
                    {
                        fdl = FDLManager.ImportFDLFromFile(file.FullName, false, false, false, false, true);

                        // try with XFA format
                        if (fdl == null)
                            fdl = FDLManager.ImportFDLFromFile(file.FullName, true, false, false, false, false);

                        if (fdl != null)
                        {
                            File.Copy(file.FullName, Path.Combine(ApplicationSettings.Directories.FDL, file.Name), true);

                            using (DBArchive db = new DBArchive())
                            {
                                // we must override recived fdl with the same of current dbcontext istance
                                FDL currentFdl = db.FDLs.SingleOrDefault(f => f.Id == fdl.Id);

                                if (currentFdl != null)
                                {
                                    currentFdl.Status = (long)EFDLStatus.Accepted;

                                    db.FDLs.AddOrUpdate(currentFdl);
                                    db.SaveChanges();
                                }
                                else
                                    Error("Missing FDL on database. Should never happen.");
                            }

                            Message($"FDL {fdl.Id} OK");
                        }
                        else
                            Error($"Failed to import FDL from file: {file.FullName}");
                        }
                    catch (Exception ex)
                    {
                        Error($"Failed importing FDL {fdl?.Id}. {ex}", ex);
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                Error($"Failed importing FDLs. {ex}", ex);
            }

            return result;
        }

        private bool ImportEAFiles()
        {
            bool result = false;

            if (stopImport)
                return result;

            StatusChanged("Importing Expense Account files...");

            try
            {
                foreach (FileInfo file in new DirectoryInfo(FDLFolder).GetFiles("*.pdf", SearchOption.AllDirectories))
                {
                    if (stopImport)
                        break;

                    if (FDLManager.GetFileType(file.Name) != EFileType.ExpenseAccount)
                        continue;

                    ExpenseAccountEVM ea = null;

                    try
                    {
                        ea = FDLManager.ImportEAFromFile(file.FullName, false, false, true);

                        if (ea != null)
                        {
                            File.Copy(file.FullName, Path.Combine(ApplicationSettings.Directories.ExpenseAccount, file.Name), true);

                            using (DBArchive db = new DBArchive())
                            {
                                // we must override recived EA with the same of current dbcontext istance
                                ExpenseAccount currentEA = db.ExpenseAccounts.SingleOrDefault(e => e.Id == ea.Id);

                                if (currentEA != null)
                                {
                                    currentEA.Status = (long)EFDLStatus.Accepted;
                                    currentEA.IsRefunded = true;

                                    db.ExpenseAccounts.AddOrUpdate(currentEA);
                                    db.SaveChanges();
                                }
                                else
                                    Error("Missing EA on database. Should never happen.");
                                }

                            Message($"Expense Account {ea.FDL} OK");
                        }
                        else
                            Error($"Failed to import EA from file: {file.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Error($"Failed importing EA {ea?.FDL}. {ex}", ex);
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                Error($"Failed importing expense accounts. {ex}", ex);
            }

            return result;
        }
    }
}
