using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Great.Models;
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
using System.Diagnostics;
using System.Collections.Specialized;
using Great.ViewModels;
using System.ComponentModel;
using Great.Controls;

namespace Great.Views
{
    /// <summary>
    /// Interaction logic for FactoriesManagerPage.xaml
    /// </summary>
    public partial class FactoriesView : Page
    {
        public const double ZOOM_MARKER = 15;

        private GMapMarker tempMarker;

        private FactoriesViewModel _viewModel { get { return DataContext as FactoriesViewModel; } }

        public FactoriesView()
        {
            InitializeComponent();

            factoriesMapControl.CacheLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "\\Cache";
            factoriesMapControl.MapProvider = GMapProviders.GoogleMap;
            factoriesMapControl.ShowCenter = false; //The block of wood display centre cross burns
            factoriesMapControl.DragButton = MouseButton.Left; //The key drags left dragging a map
            factoriesMapControl.Position = new PointLatLng(0, 0);
            
            _viewModel.PropertyChanged += FactoriesView_PropertyChangedEventHandler;
        }

        private void SearchLocation()
        {
            PointLatLng? point = GetPointFromAddress(searchEntryTextBox.Text.Trim());

            if (point.HasValue)
            {
                if (tempMarker != null)
                    factoriesMapControl.Markers.Remove(tempMarker);

                tempMarker = CreateMarker((PointLatLng)point, new Factory() { Name = "New Factory", Address = searchEntryTextBox.Text.Trim() }, FactoryMarkerColor.Green);
                factoriesMapControl.Markers.Add(tempMarker);

                ZoomOnPoint(point.Value, ZOOM_MARKER);
            }
        }

        private void ZoomOnPoint(PointLatLng point, double Zoom)
        {
            if (!point.IsEmpty)
            {
                factoriesMapControl.Position = point;
                factoriesMapControl.Zoom = Zoom;
            }
        }

        public PointLatLng? GetFactoryPosition(Factory factory)
        {
            PointLatLng? point;

            if (factory.Latitude.HasValue && factory.Longitude.HasValue)
                point = new PointLatLng(factory.Latitude.Value, factory.Longitude.Value);
            else
                point = GetPointFromAddress(factory.Address);

            return point;
        }

        private PointLatLng? GetPointFromAddress(string address)
        {
            PointLatLng? point;
            GeoCoderStatusCode status;

            point = GMapProviders.GoogleMap.GetPoint(address, out status);

            //TODO: log errors -> switch(status) case: GeoCoderStatusCode.G_GEO_SUCCESS
            return point;
        }

        private GMapMarker CreateMarker(PointLatLng point, Factory factory, FactoryMarkerColor color)
        {
            FactoryMarker shape = new FactoryMarker() { DataContext = factory, Color = color };

            GMapMarker marker = new GMapMarker((PointLatLng)point);
            marker.Shape = shape;
            marker.Offset = new Point(-(shape.Rectangle.Width / 2), -shape.Rectangle.Height);

            return marker;
        }

        public void RefreshMarkers()
        {
            factoriesMapControl.Markers.Clear();

            foreach (Factory factory in _viewModel.Factories)
            {
                PointLatLng? point = GetFactoryPosition(factory);

                if (point.HasValue)
                {   
                    GMapMarker marker = CreateMarker((PointLatLng)point, factory, FactoryMarkerColor.Red);
                    factoriesMapControl.Markers.Add(marker);
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Net.IPHostEntry test = System.Net.Dns.GetHostEntry("google.com");
            }
            catch
            {
                factoriesMapControl.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "Factories Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            RefreshMarkers();
            factoriesMapControl.ZoomAndCenterMarkers(null);
        }
        
        private void FactoriesView_PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Factories")
            {
                RefreshMarkers();
            }
        }

        private void marker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                FactoryMarker marker = sender as FactoryMarker;
                _viewModel.SelectedFactory = marker.DataContext as Factory;
                marker.PlayBounce();
            }
        }

        private void marker_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FactoriesMapControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(factoriesMapControl);
            PointLatLng mapPosition = factoriesMapControl.FromLocalToLatLng((int)mousePos.X, (int)mousePos.Y);
            List<Placemark> placemarks = null;

            GeoCoderStatusCode status = GMapProviders.GoogleMap.GetPlacemarks(mapPosition, out placemarks);

            if (status == GeoCoderStatusCode.G_GEO_SUCCESS && placemarks != null && placemarks.Count > 0)
            {
                if (tempMarker != null)
                    factoriesMapControl.Markers.Remove(tempMarker);

                Factory factory = new Factory() { Name = "New Factory", Address = placemarks.FirstOrDefault().Address.Trim() };
                GMapMarker marker = CreateMarker(mapPosition, factory, FactoryMarkerColor.Green);
                tempMarker = marker;
                factoriesMapControl.Markers.Add(marker);
                
                _viewModel.SelectedFactory = factory;
            }
        }

        private void FactoriesMapControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            
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
            factoriesMapControl.ZoomAndCenterMarkers(null);
        }

        private void factoryListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && factoriesListView.SelectedItem != null)
            {            
                Factory factory = (Factory)factoriesListView.SelectedItem;
                PointLatLng? point = GetFactoryPosition(factory);

                if (point.HasValue)
                {
                    ZoomOnPoint(point.Value, ZOOM_MARKER);
                    FactoryMarker marker = factoriesMapControl.Markers.Where(m => ((Factory)((FactoryMarker)m.Shape).DataContext).Id == factory.Id).Select(m => m.Shape as FactoryMarker).FirstOrDefault();
                    
                    if(marker != null)
                        marker.PlayBounce();
                }
            }
        }
    }
}
