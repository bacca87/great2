using GalaSoft.MvvmLight;
using System.Diagnostics;
using System.Reflection;

namespace Great.ViewModels
{
    public class InformationsViewModel : ViewModelBase
    {
        private Assembly assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Gets the Title property.
        /// </summary>
        public string Title
        {
            get => (assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute).Title;
        }

        /// <summary>
        /// Gets the Description property.
        /// </summary>
        public string Description
        {
            get => (assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0] as AssemblyDescriptionAttribute).Description;
        }

        /// <summary>
        /// Gets the Company property.
        /// </summary>
        public string Company
        {
            get => (assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0] as AssemblyCompanyAttribute).Company;
        }

        /// <summary>
        /// Gets the Copyright property.
        /// </summary>
        public string Copyleft
        {
            get => (assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute).Copyright;
        }

        /// <summary>
        /// Gets the Product property.
        /// </summary>
        public string Product
        {
            get => (assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0] as AssemblyProductAttribute).Product;
        }

        /// <summary>
        /// Gets the Version property.
        /// </summary>
        public string Version
        {
            get => FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        }

        /// <summary>
        /// Gets the Version property.
        /// </summary>
        public string AppNameAndVersion
        {
            get => Title + " v" + Version;
        }

        /// <summary>
        /// Gets the ProductDescription property.
        /// </summary>
        public string ProductDescription
        {
            get;
            internal set;
        }

        /// <summary>
        /// Initializes a new instance of the InformationsViewModel class.
        /// </summary>
        public InformationsViewModel()
        {
            ProductDescription = "This software has been designed to facilitate the business traveler's tasks.\n" +
                "Keep track of you worked hours, expense accounts, travels and check your stats to be always informed about your activities.\n" +
                "All of this in automatic way!" +
                "Please feel free to contribute to this project helping us to add new feature and keep everithing up to date, just visit our website for download the source code and start to improve it!\n\n" +
                "A special thanks to Andrea 'Cina' Ghinelli, the original author of this very useful software!";
        }
    }
}