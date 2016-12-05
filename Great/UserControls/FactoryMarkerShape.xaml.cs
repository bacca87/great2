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
        public FactoryMarkerColor Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;

                switch (_color)
                {
                    case FactoryMarkerColor.Red:
                        MarkerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/map-marker-red.png"));                        
                        break;
                    case FactoryMarkerColor.Green:
                        MarkerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/map-marker-green.png"));
                        break;
                    case FactoryMarkerColor.Blue:
                        MarkerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/map-marker-blue.png"));
                        break;
                    default:
                        MarkerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/map-marker-red.png"));
                        break;
                }
            }
        }

        private Storyboard BounceAnimation { get; set; }
        private FactoryMarkerColor _color = FactoryMarkerColor.Red;
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

    public enum FactoryMarkerColor
    {
        Red,
        Green,
        Blue
    }
}
