/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Great"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using Great.Models;
using Great.Models.Database;
using Great.Models.Interfaces;

namespace Great.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            //ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<IProvider,MSExchangeProvider>();
            SimpleIoc.Default.Register<FDLManager>();
            SimpleIoc.Default.Register<DBArchive>();

            SimpleIoc.Default.Register<TimesheetsViewModel>();
            SimpleIoc.Default.Register<FactoriesViewModel>();
            SimpleIoc.Default.Register<InformationsViewModel>();
            SimpleIoc.Default.Register<FDLViewModel>();
            SimpleIoc.Default.Register<ExpenseAccountViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<NotificationsViewModel>();
            SimpleIoc.Default.Register<ImportExportViewModel>();
            SimpleIoc.Default.Register<GreatImportWizardViewModel>();
            SimpleIoc.Default.Register<OrderRecipientsViewModel>();
            SimpleIoc.Default.Register<StatisticsViewModel>();
            SimpleIoc.Default.Register<CarRentalViewModel>();
            SimpleIoc.Default.Register<FDLImportWizardViewModel>();
        }

        public TimesheetsViewModel Timesheets
        {
            get
            {
                return SimpleIoc.Default.GetInstance<TimesheetsViewModel>();
            }
        }

        public FactoriesViewModel Factories
        {
            get
            {
                return SimpleIoc.Default.GetInstance<FactoriesViewModel>();
            }
        }

        public InformationsViewModel Informations
        {
            get
            {
                return SimpleIoc.Default.GetInstance<InformationsViewModel>();
            }
        }

        public FDLViewModel FDL
        {
            get
            {
                return SimpleIoc.Default.GetInstance<FDLViewModel>();
            }
        }

        public ExpenseAccountViewModel ExpenseAccount
        {
            get
            {
                return SimpleIoc.Default.GetInstance<ExpenseAccountViewModel>();
            }
        }

        public SettingsViewModel Settings
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SettingsViewModel>();
            }
        }

        public NotificationsViewModel Notifications
        {
            get
            {
                return SimpleIoc.Default.GetInstance<NotificationsViewModel>();
            }
        }

        public ImportExportViewModel ImportExport
        {
            get
            {
                return SimpleIoc.Default.GetInstance<ImportExportViewModel>();
            }
        }

        public GreatImportWizardViewModel GreatImportWizard
        {
            get
            {
                return SimpleIoc.Default.GetInstance<GreatImportWizardViewModel>();
            }
        }

        public OrderRecipientsViewModel OrderRecipients
        {
            get
            {
                return SimpleIoc.Default.GetInstance<OrderRecipientsViewModel>();
            }
        }

        public StatisticsViewModel Statistics
        {
            get
            {
                return SimpleIoc.Default.GetInstance<StatisticsViewModel>();
            }
        }

        public CarRentalViewModel CarRental
        {
            get
            {
                return SimpleIoc.Default.GetInstance<CarRentalViewModel>();
            }
        }

        public FDLImportWizardViewModel FDLImportWizard
        {
            get
            {
                return SimpleIoc.Default.GetInstance<FDLImportWizardViewModel>();
            }
        }

        public static void Cleanup()
        {
            // Clear the ViewModels
        }
    }
}