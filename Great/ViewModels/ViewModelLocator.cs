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

            SimpleIoc.Default.Register<MSExchangeProvider>();
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
            SimpleIoc.Default.Register<CarRentalViewModel>();
        }

        public TimesheetsViewModel Timesheets => SimpleIoc.Default.GetInstance<TimesheetsViewModel>();

        public FactoriesViewModel Factories => SimpleIoc.Default.GetInstance<FactoriesViewModel>();

        public InformationsViewModel Informations => SimpleIoc.Default.GetInstance<InformationsViewModel>();
        public FDLViewModel FDL => SimpleIoc.Default.GetInstance<FDLViewModel>();

        public ExpenseAccountViewModel ExpenseAccount => SimpleIoc.Default.GetInstance<ExpenseAccountViewModel>();
        public SettingsViewModel Settings => SimpleIoc.Default.GetInstance<SettingsViewModel>();
        public NotificationsViewModel Notifications => SimpleIoc.Default.GetInstance<NotificationsViewModel>();

        public ImportExportViewModel ImportExport => SimpleIoc.Default.GetInstance<ImportExportViewModel>();

        public GreatImportWizardViewModel GreatImportWizard => SimpleIoc.Default.GetInstance<GreatImportWizardViewModel>();

        public CarRentalViewModel CarRental => SimpleIoc.Default.GetInstance<CarRentalViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}