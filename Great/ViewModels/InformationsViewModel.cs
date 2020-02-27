using GalaSoft.MvvmLight;
using System.Diagnostics;
using System.Reflection;

namespace Great2.ViewModels
{
    public class InformationsViewModel : ViewModelBase
    {
        private static Assembly assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Gets the Title property.
        /// </summary>
        public static string Title => (assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute).Title;

        /// <summary>
        /// Gets the Description property.
        /// </summary>
        public static string Description => (assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0] as AssemblyDescriptionAttribute).Description;

        /// <summary>
        /// Gets the Company property.
        /// </summary>
        public static string Company => (assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0] as AssemblyCompanyAttribute).Company;

        /// <summary>
        /// Gets the Copyright property.
        /// </summary>
        public static string Copyleft => (assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute).Copyright;

        /// <summary>
        /// Gets the Product property.
        /// </summary>
        public static string Product => (assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0] as AssemblyProductAttribute).Product;

        /// <summary>
        /// Gets the Version property.
        /// </summary>
        public static string Version => FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;

        /// <summary>
        /// Gets the Version property.
        /// </summary>
        public static string AppNameAndVersion => Title + " v" + Version;

        /// <summary>
        /// Gets the ProductDescription property.
        /// </summary>
        public static string ProductDescription
        {
            get;
            internal set;
        }

        /// <summary>
        /// Initializes a new instance of the InformationsViewModel class.
        /// </summary>
        public InformationsViewModel()
        {
            ProductDescription = "This software has been designed to ease the business traveler's tasks.\n" +
                "Keep track of your worked hours, expense accounts, travels and check your stats to be always informed about your activities.\n" +
                "Please feel free to contribute to this project helping us to add new feature and keep everithing up to date, just visit our website for download the source code and start to improve it!\n\n" +
                "A special thanks to Andrea 'Cina' Ghinelli, the original ideator of this very useful software!";
        }
    }
}