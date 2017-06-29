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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Great.Models;
using Microsoft.Practices.ServiceLocation;

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
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

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
            
            SimpleIoc.Default.Register<MSExchangeProvider>();
            SimpleIoc.Default.Register<FDLManager>();
            SimpleIoc.Default.Register<DBEntities>();

            SimpleIoc.Default.Register<TimesheetsViewModel>();
            SimpleIoc.Default.Register<FactoriesViewModel>();
            SimpleIoc.Default.Register<InformationsViewModel>();
            SimpleIoc.Default.Register<FDLViewModel>();
            SimpleIoc.Default.Register<ExpenseAccountViewModel>();            
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<NotificationsViewModel>();
            SimpleIoc.Default.Register<ImportExportViewModel>();
            SimpleIoc.Default.Register<GreatImportWizardViewModel>();
        }

        public TimesheetsViewModel Timesheets
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TimesheetsViewModel>();
            }
        }

        public FactoriesViewModel Factories
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FactoriesViewModel>();
            }
        }

        public InformationsViewModel Informations
        {
            get
            {
                return ServiceLocator.Current.GetInstance<InformationsViewModel>();
            }
        }

        public FDLViewModel FDL
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FDLViewModel>();
            }
        }

        public ExpenseAccountViewModel ExpenseAccount
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ExpenseAccountViewModel>();
            }
        }

        public SettingsViewModel Settings
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsViewModel>();
            }
        }

        public NotificationsViewModel Notifications
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NotificationsViewModel>();
            }
        }

        public ImportExportViewModel ImportExport
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ImportExportViewModel>();
            }
        }

        public GreatImportWizardViewModel GreatImportWizard
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GreatImportWizardViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}