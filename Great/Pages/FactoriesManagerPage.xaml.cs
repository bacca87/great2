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
        public const double ZOOM_MARKER = 15;

        private GMapMarker tempMarker;

        public FactoriesManagerPage()
        {
            InitializeComponent();

            mapControl.CacheLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "\\Cache";
            mapControl.MapProvider = GMapProviders.GoogleMap;
            mapControl.ShowCenter = false; //The block of wood display centre cross burns
            mapControl.DragButton = MouseButton.Left; //The key drags left dragging a map
            mapControl.Position = new PointLatLng(0, 0);
        }

        private void SearchLocation()
        {
            PointLatLng? point = GetPointFromAddress(searchEntryTextBox.Text);

            if (point.HasValue)
            {
                if (tempMarker != null)
                    mapControl.Markers.Remove(tempMarker);

                tempMarker = CreateMarker((PointLatLng)point, "New Factory", searchEntryTextBox.Text.Trim(), FactoryMarkerColor.Green);
                mapControl.Markers.Add(tempMarker);

                ZoomOnPoint(point.Value, ZOOM_MARKER);
            }            
        }

        private void ZoomOnPoint(PointLatLng point, double Zoom)
        {
            if (!point.IsEmpty)
            {
                mapControl.Position = point;
                mapControl.Zoom = Zoom;
            }
        }

        private PointLatLng? GetPointFromAddress(string address)
        {
            PointLatLng? point;
            GeoCoderStatusCode status;

            point = GMapProviders.GoogleMap.GetPoint(address, out status);

            //TODO: log errors -> switch(status) case: GeoCoderStatusCode.G_GEO_SUCCESS
            return point;
        }

        private GMapMarker CreateMarker(PointLatLng point, string title, string address, FactoryMarkerColor color)
        {
            GMapMarker marker = new GMapMarker((PointLatLng)point);

            FactoryMarkerShape shape = new FactoryMarkerShape() { Title = title, Address = address, Color = color };
            shape.MouseDoubleClick += marker_MouseDoubleClick;
            shape.MouseUp += marker_MouseUp;
            marker.Shape = shape;

            return marker;
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
                MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "Factories Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            using (var db = new DBEntities())
            {   
                IList<Factory> factories = db.Factories.ToList();

                foreach (Factory factory in factories)
                {
                    PointLatLng? point;

                    if (factory.MapPoint.HasValue)
                        point = factory.MapPoint;
                    else
                        point = GetPointFromAddress(factory.Address);
                    
                    if (point.HasValue)
                    {
                        GMapMarker marker = CreateMarker((PointLatLng)point, factory.Name, factory.Address, FactoryMarkerColor.Red);                        
                        factory.MapMarker = marker;
                        mapControl.Markers.Add(marker);
                    }
                }

                mapControl.ZoomAndCenterMarkers(null);
                factoriesListView.ItemsSource = db.Factories.ToList();
            }
        }

        private void marker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //TEST
            if (e.ChangedButton == MouseButton.Left)
                ((FactoryMarkerShape)sender).PlayBounce();
        }

        private void marker_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void mapControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(mapControl);
            PointLatLng mapPosition = mapControl.FromLocalToLatLng((int)mousePos.X, (int)mousePos.Y);
            List<Placemark> placemarks = null;

            GeoCoderStatusCode status = GMapProviders.GoogleMap.GetPlacemarks(mapPosition, out placemarks);

            if (status == GeoCoderStatusCode.G_GEO_SUCCESS && placemarks != null && placemarks.Count > 0)
            {
                if (tempMarker != null)
                    mapControl.Markers.Remove(tempMarker);

                GMapMarker marker = CreateMarker(mapPosition, "New Factory", placemarks.FirstOrDefault().Address, FactoryMarkerColor.Green);
                tempMarker = marker;
                mapControl.Markers.Add(marker);
            }
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            SearchLocation();
        }

        private void searchEntryTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchLocation();
        }

        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            mapControl.ZoomAndCenterMarkers(null);
        }

        private void factoriesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && factoriesListView.SelectedItem != null)
            {            
                Factory factory = (Factory)factoriesListView.SelectedItem;
                PointLatLng? point;

                if (factory.MapPoint.HasValue)
                    point = factory.MapPoint;
                else
                    point = GetPointFromAddress(factory.Address);

                if (point.HasValue)
                {
                    ZoomOnPoint(point.Value, ZOOM_MARKER);
                    ((FactoryMarkerShape)factory.MapMarker.Shape).PlayBounce();
                }
            }
        }
    }
}
