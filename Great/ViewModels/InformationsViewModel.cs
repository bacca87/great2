using GalaSoft.MvvmLight;
using System.Collections.Generic;
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
            get
            {
                return (assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute).Title;
            }
        }

        /// <summary>
        /// Gets the Description property.
        /// </summary>
        public string Description
        {
            get
            {
                return (assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0] as AssemblyDescriptionAttribute).Description;
            }
        }

        /// <summary>
        /// Gets the Company property.
        /// </summary>
        public string Company
        {
            get
            {
                return (assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0] as AssemblyCompanyAttribute).Company;
            }
        }

        /// <summary>
        /// Gets the Copyright property.
        /// </summary>
        public string Copyright
        {
            get
            {
                return (assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute).Copyright;
            }
        }

        /// <summary>
        /// Gets the Product property.
        /// </summary>
        public string Product
        {
            get
            {
                return (assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0] as AssemblyProductAttribute).Product;
            }
        }

        /// <summary>
        /// Gets the Version property.
        /// </summary>
        public string Version
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
            }
        }
                
        /// <summary>
        /// Initializes a new instance of the InformationsViewModel class.
        /// </summary>
        public InformationsViewModel()
        {
        }
    }
}