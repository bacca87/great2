using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Great.Controls
{
    /// <summary>
    /// Interaction logic for GMapFactoryMarker.xaml
    /// </summary>
    public partial class FactoryMarker : UserControl
    {
        #region Properties
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
                        MarkerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Great2;component/Images/24/map-marker-red.png"));                        
                        break;
                    case FactoryMarkerColor.Green:
                        MarkerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Great2;component/Images/24/map-marker-green.png"));
                        break;
                    case FactoryMarkerColor.Blue:
                        MarkerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Great2;component/Images/24/map-marker-blue.png"));
                        break;
                    default:
                        break;
                }
            }
        }

        private Storyboard BounceAnimation { get; set; }
        private FactoryMarkerColor _color = FactoryMarkerColor.Red;
        #endregion

        public FactoryMarker()
        {
            InitializeComponent();
            
            Color = FactoryMarkerColor.Red;

            BounceAnimation = this.FindResource("Bounce") as Storyboard;
            Storyboard.SetTarget(BounceAnimation, this.Rectangle);
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
