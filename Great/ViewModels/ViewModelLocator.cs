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

            SimpleIoc.Default.Register<DBEntities>();

            SimpleIoc.Default.Register<ExchangeProvider>();

            SimpleIoc.Default.Register<TimesheetsViewModel>();
            SimpleIoc.Default.Register<FactoriesViewModel>();
            SimpleIoc.Default.Register<InformationsViewModel>();
            SimpleIoc.Default.Register<EmailViewModel>();
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

        public EmailViewModel Email
        {
            get
            {
                return ServiceLocator.Current.GetInstance<EmailViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}