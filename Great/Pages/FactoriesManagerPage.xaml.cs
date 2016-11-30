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
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Great.DB;
using System.Diagnostics;
using System.Collections;

namespace Great.Pages
{
    /// <summary>
    /// Interaction logic for FactoriesManagerPage.xaml
    /// </summary>
    public partial class FactoriesManagerPage : Page
    {
        public FactoriesManagerPage()
        {
            InitializeComponent();

            mapControl.CacheLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "\\Cache";
            mapControl.MapProvider = GMapProviders.GoogleMap;
            mapControl.ShowCenter = false; //The block of wood display centre cross burns
            mapControl.DragButton = MouseButton.Left; //The key drags left dragging a map
            mapControl.Position = new PointLatLng(0, 0);            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Net.IPHostEntry test = System.Net.Dns.GetHostEntry("google.com");
            }
            catch
            {
                mapControl.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "GMap.NET Demo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            using (var db = new DBEntities())
            {
                IList<Factory> factories = db.Factory.ToList();

                foreach (Factory factory in factories)
                {
                    GeoCoderStatusCode status;
                    PointLatLng? point = GMapProviders.GoogleMap.GetPoint(factory.Address, out status);
                    
                    if (status == GeoCoderStatusCode.G_GEO_SUCCESS && point != null)
                    {
                        GMapMarker marker = new GMapMarker((PointLatLng)point);
                        marker.Shape = new FactoryMarkerShape() { FactoryId = factory.Id, FactoryName = factory.Name, Address = factory.Address };
                        mapControl.Markers.Add(marker);
                    }
                }

                mapControl.ZoomAndCenterMarkers(null);
                factoriesListView.ItemsSource = db.Factory.ToList();
            }
        }

        private void mapControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(mapControl);
            PointLatLng mapPosition = mapControl.FromLocalToLatLng((int)mousePos.X, (int)mousePos.Y);

            List<Placemark> plc = null;
            var st = GMapProviders.GoogleMap.GetPlacemarks(mapPosition, out plc);
            if (st == GeoCoderStatusCode.G_GEO_SUCCESS && plc != null)
            {
                foreach (var pl in plc)
                {
                    if (!string.IsNullOrEmpty(pl.PostalCodeNumber))
                    {
                        Debug.WriteLine("Accuracy: " + pl.Accuracy + ", " + pl.Address + ", PostalCodeNumber: " + pl.PostalCodeNumber);
                    }
                }
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchLocation();
        }

        private void searchEntryTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchLocation();
        }

        private void SearchLocation()
        {
            GeoCoderStatusCode status;
            PointLatLng? point = GMapProviders.GoogleMap.GetPoint(searchEntryTextBox.Text, out status);

            if (status == GeoCoderStatusCode.G_GEO_SUCCESS && point != null)
            {
                mapControl.Position = (PointLatLng)point;
                mapControl.Zoom = 17;
            }
        }

        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            mapControl.ZoomAndCenterMarkers(null);
        }
    }
}
