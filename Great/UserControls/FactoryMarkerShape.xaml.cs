using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Great
{
    /// <summary>
    /// Interaction logic for GMapFactoryMarker.xaml
    /// </summary>
    public partial class FactoryMarkerShape : UserControl
    {
        #region Properties
        public int FactoryId { get; set; }
        public string FactoryName { get; set; }
        public string Address { get; set; }
        #endregion

        public FactoryMarkerShape()
        {
            InitializeComponent();
        }
    }
}
