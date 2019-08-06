using GalaSoft.MvvmLight.Ioc;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Great.Controls;
using Great.Models;
using Great.ViewModels;
using Great.ViewModels.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Great.Views
{
    /// <summary>
    /// Interaction logic for FactoriesManagerPage.xaml
    /// </summary>
    public partial class FactoriesView : Page
    {
        private GMapMarker tempMarker;
        private GMapMarker tempPosMarker;

        private FactoriesViewModel _viewModel { get { return DataContext as FactoriesViewModel; } }

        private bool IsLatLngSelectionMode = false;
        private GridLength lastGridHeight;
        private Cursor lastCursor;

        public FactoriesView()
        {
            InitializeComponent();

            factoriesMapControl.CacheLocation = ApplicationSettings.Directories.Cache;
            factoriesMapControl.MapProvider = GMapProviders.OpenStreetMap;
            factoriesMapControl.ShowCenter = false; //The block of wood display centre cross burns
            factoriesMapControl.DragButton = MouseButton.Right; //The key drags left dragging a map
            factoriesMapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            factoriesMapControl.Position = new PointLatLng(0, 0);

            zoomSlider.Maximum = factoriesMapControl.MaxZoom;
            zoomSlider.Minimum = factoriesMapControl.MinZoom;
            zoomSlider.Value = factoriesMapControl.Zoom;

            _viewModel.PropertyChanged += FactoriesView_PropertyChangedEventHandler;
            _viewModel.OnZoomOnFactoryRequest += OnZoomOnFactoryRequest;

            factoriesMapControl.ZoomAndCenterMarkers(null);
        }

        private void LatLngSelectionMode(bool enable)
        {
            IsLatLngSelectionMode = enable;

            if (enable)
            {
                lastCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Cross;

                lastGridHeight = layoutGrid.RowDefinitions[2].Height;
                layoutGrid.RowDefinitions[2].Height = new GridLength(0);
            }
            else
            {
                Mouse.OverrideCursor = lastCursor;
                layoutGrid.RowDefinitions[2].Height = lastGridHeight;
            }

            splitter.IsEnabled = !enable;
            searchEntryTextBox.IsEnabled = !enable;
            goButton.IsEnabled = !enable;

            latlngButton.IsChecked = enable;
        }

        private void SearchLocation()
        {
            PointLatLng? point = GetPointFromAddress(searchEntryTextBox.Text.Trim());

            if (point.HasValue)
            {
                if (tempMarker != null)
                    factoriesMapControl.Markers.Remove(tempMarker);

                tempMarker = CreateMarker(point.Value, new FactoryEVM() { Name = ApplicationSettings.Map.NewFactoryName, Address = searchEntryTextBox.Text.Trim(), Latitude = point.Value.Lat, Longitude = point.Value.Lng }, FactoryMarkerColor.Green);
                factoriesMapControl.Markers.Add(tempMarker);

                ZoomOnPoint(point.Value, ApplicationSettings.Map.ZoomMarker);
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

        private void ZoomOnFactory(FactoryEVM factory)
        {
            PointLatLng? point = GetFactoryCoordsAsync(factory).Result;

            if (point.HasValue)
            {
                ZoomOnPoint(point.Value, ApplicationSettings.Map.ZoomMarker);
                FactoryMarker marker = factoriesMapControl.Markers.Where(m => ((FactoryEVM)((FactoryMarker)m.Shape).DataContext).Id == factory.Id).Select(m => m.Shape as FactoryMarker).FirstOrDefault();

                if (marker != null)
                    marker.PlayBounce();
            }
        }

        public async Task<PointLatLng?> GetFactoryCoordsAsync(FactoryEVM factory)
        {
            if (factory.Latitude.HasValue && factory.Longitude.HasValue)
                return new PointLatLng(factory.Latitude.Value, factory.Longitude.Value);
            else
            {
                var point = await GetCoordsFromAddressAsync(factory.Address);

                if (point.HasValue)
                {
                    factory.Latitude = point.Value.Lat;
                    factory.Longitude = point.Value.Lng;
                    factory.Save();
                }

                return point;
            }
        }

        private PointLatLng? GetPointFromAddress(string address)
        {
            //The GetPoint method of Gmap.NET run on GUI thread freezing everything until the end of the computation
            //use this methon only for one time call. For big bulk of data use GetCoordsFromAddressAsync()
            PointLatLng? point;
            GeoCoderStatusCode status;

            point = (factoriesMapControl.MapProvider as GeocodingProvider).GetPoint(address, out status);
            //TODO: log errors -> switch(status) case: GeoCoderStatusCode.G_GEO_SUCCESS
            return point;
        }

        public static async Task<PointLatLng?> GetCoordsFromAddressAsync(string address)
        {
            try
            {
                // we call directly the OSM web api in order to prevent GUI freeze. The GetPoint method of Gmap.NET run on GUI thread freezing everything until the end of the computation
                HttpClient httpClient = new HttpClient { BaseAddress = new Uri("http://nominatim.openstreetmap.org") };
                httpClient.DefaultRequestHeaders.Add("User-Agent", ApplicationSettings.General.UserAgent);

                HttpResponseMessage httpResult = await httpClient.GetAsync($"search.php?q={address}&format=json&polygon=1&addressdetails=1");

                var result = await httpResult.Content.ReadAsStringAsync();

                var r = (JArray)JsonConvert.DeserializeObject(result);

                if (r.HasValues)
                {
                    var latString = ((JValue)r[0]["lat"]).Value as string;
                    var lngString = ((JValue)r[0]["lon"]).Value as string;

                    if (latString != string.Empty && lngString != string.Empty)
                    {
                        double lat = double.Parse(latString, CultureInfo.InvariantCulture);
                        double lng = double.Parse(lngString, CultureInfo.InvariantCulture);

                        return new PointLatLng(lat, lng);
                    }
                }
            }
            catch { }

            return null;
        }

        private GMapMarker CreateMarker(PointLatLng point, FactoryEVM factory, FactoryMarkerColor color)
        {
            FactoryMarker shape = new FactoryMarker() { DataContext = factory, Color = color };

            GMapMarker marker = new GMapMarker(point);
            marker.Tag = factory;
            marker.Shape = shape;
            marker.Offset = new Point(-(shape.Rectangle.Width / 2), -shape.Rectangle.Height);

            factory.Latitude = point.Lat;
            factory.Longitude = point.Lng;
            factory.Save();

            return marker;
        }

        public async void RefreshMarkersAsync(IList<FactoryEVM> factories)
        {
            tempMarker = null;
            tempPosMarker = null;

            var tasks = new Dictionary<FactoryEVM, Task<PointLatLng?>>();

            var factoriesToAdd = factories.Where(f => !factoriesMapControl.Markers.Any(m => m.Tag != null && (m.Tag as FactoryEVM).Id == f.Id));

            foreach (FactoryEVM factory in factoriesToAdd)
                tasks.Add(factory, GetFactoryCoordsAsync(factory));

            await Task.WhenAll(tasks.Values);

            foreach (var task in tasks)
            {
                FactoryEVM factory = task.Key;
                PointLatLng? point = task.Value.Result;

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
                System.Net.IPHostEntry test = System.Net.Dns.GetHostEntry(new Uri("http://nominatim.openstreetmap.org").Host);
            }
            catch
            {
                factoriesMapControl.Manager.Mode = AccessMode.CacheOnly;
                MetroMessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "Factories Map", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void FactoriesView_PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_viewModel.SelectedFactory):
                    if (tempPosMarker != null)
                    {
                        factoriesMapControl.Markers.Remove(tempPosMarker);
                        tempPosMarker = null;
                    }
                    break;
            }
        }

        private void marker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                FactoryMarker marker = sender as FactoryMarker;
                _viewModel.SelectedFactory = marker.DataContext as FactoryEVM;
                marker.PlayBounce();
            }
        }

        private void marker_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void FactoriesMapControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Point mousePos = e.GetPosition(factoriesMapControl);
                PointLatLng mapPosition = factoriesMapControl.FromLocalToLatLng((int)mousePos.X, (int)mousePos.Y);

                GeoCoderStatusCode status;
                Placemark? placemark = (factoriesMapControl.MapProvider as GeocodingProvider).GetPlacemark(mapPosition, out status);

                if (status == GeoCoderStatusCode.OK && placemark.HasValue)
                {
                    if (tempMarker != null)
                        factoriesMapControl.Markers.Remove(tempMarker);

                    FactoryEVM factory = new FactoryEVM() { Name = ApplicationSettings.Map.NewFactoryName, Address = placemark.Value.Address.Trim(), Latitude = mapPosition.Lat, Longitude = mapPosition.Lng };
                    GMapMarker marker = CreateMarker(mapPosition, factory, FactoryMarkerColor.Green);
                    tempMarker = marker;
                    factoriesMapControl.Markers.Add(marker);

                    _viewModel.SelectedFactory = factory;
                }
            }
        }

        private void factoriesMapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsLatLngSelectionMode)
            {
                Point mousePos = e.GetPosition(factoriesMapControl);
                PointLatLng mapPosition = factoriesMapControl.FromLocalToLatLng((int)mousePos.X, (int)mousePos.Y);

                FactoryEVM factory = _viewModel.SelectedFactory;
                factory.Latitude = mapPosition.Lat;
                factory.Longitude = mapPosition.Lng;

                if (tempPosMarker != null)
                    factoriesMapControl.Markers.Remove(tempPosMarker);

                GMapMarker marker = CreateMarker(mapPosition, factory, FactoryMarkerColor.Blue);
                tempPosMarker = marker;
                factoriesMapControl.Markers.Add(marker);

                LatLngSelectionMode(false);
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
            factoriesMapControl.ZoomAndCenterMarkers(null);
        }

        private void factoryListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && factoriesListView.SelectedItem != null)
            {
                FactoryEVM factory = (FactoryEVM)factoriesListView.SelectedItem;
                ZoomOnFactory(factory);
            }
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            factoriesMapControl.Zoom = e.NewValue;
        }

        private void factoriesMapControl_OnMapZoomChanged()
        {
            zoomSlider.Value = factoriesMapControl.Zoom;
            zoomLabel.Content = factoriesMapControl.Zoom + "x";
        }

        private void factoriesMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            //hack for updating the map zoom when it is changed before map is fully loaded
            factoriesMapControl_OnMapZoomChanged();
        }

        private void latlngButton_Click(object sender, RoutedEventArgs e)
        {
            LatLngSelectionMode(true);
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false && IsLatLngSelectionMode)
                LatLngSelectionMode(false);

            if ((bool)e.NewValue)
                RefreshMarkersAsync(new List<FactoryEVM>(_viewModel.Factories));
        }

        private void OnZoomOnFactoryRequest(FactoryEVM factory)
        {
            ZoomOnFactory(factory);
        }
    }
}
