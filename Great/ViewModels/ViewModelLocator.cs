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
            // Prioritary
            SimpleIoc.Default.Register<NotificationsViewModel>(true);

            // Indipendent
            SimpleIoc.Default.Register<TimesheetsViewModel>(true);
            SimpleIoc.Default.Register<FactoriesViewModel>(true);
            SimpleIoc.Default.Register<InformationsViewModel>(true);            
            SimpleIoc.Default.Register<ImportExportViewModel>(true);
            SimpleIoc.Default.Register<GreatImportWizardViewModel>(true);
            SimpleIoc.Default.Register<OrderRecipientsViewModel>(true);
            SimpleIoc.Default.Register<StatisticsViewModel>(true);
            SimpleIoc.Default.Register<CarRentalViewModel>(true);
            SimpleIoc.Default.Register<FDLImportWizardViewModel>(true);

            // Exchange
            SimpleIoc.Default.Register<IProvider, MSExchangeProvider>(true);
            SimpleIoc.Default.Register<FDLManager>(true);
            SimpleIoc.Default.Register<FDLViewModel>(true);
            SimpleIoc.Default.Register<ExpenseAccountViewModel>(true);
            SimpleIoc.Default.Register<SettingsViewModel>(true);

            // Sharepoint
            SimpleIoc.Default.Register<MSSharepointProvider>(true);
            SimpleIoc.Default.Register<EventsViewModel>(true);
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

        public EventsViewModel Events
        {
            get
            {
                return SimpleIoc.Default.GetInstance<EventsViewModel>();
            }
        }

        public static void Cleanup()
        {
            // Clear the ViewModels
        }
    }
}