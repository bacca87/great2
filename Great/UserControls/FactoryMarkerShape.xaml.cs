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
using System.Windows.Media.Animation;
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
        public string Title { get; set; }
        public string Address { get; set; }

        private Storyboard BounceAnimation { get; set; }
        #endregion

        public FactoryMarkerShape()
        {
            InitializeComponent();

            BounceAnimation = this.FindResource("Bounce") as Storyboard;
            Storyboard.SetTarget(BounceAnimation, this.rectangle);
        }

        public void PlayBounce()
        {
            BounceAnimation.Begin();
        }
    }
}
