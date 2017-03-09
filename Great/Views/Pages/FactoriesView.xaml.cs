using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Great.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            factoriesMapControl.MapProvider = GMapProviders.GoogleMap;
            factoriesMapControl.ShowCenter = false; //The block of wood display centre cross burns
            factoriesMapControl.DragButton = MouseButton.Right; //The key drags left dragging a map
            factoriesMapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            factoriesMapControl.Position = new PointLatLng(0, 0);

            zoomSlider.Maximum = factoriesMapControl.MaxZoom;
            zoomSlider.Minimum = factoriesMapControl.MinZoom;
            zoomSlider.Value = factoriesMapControl.Zoom;
            
            _viewModel.PropertyChanged += FactoriesView_PropertyChangedEventHandler;
            _viewModel.Factories.ListChanged += Factories_ListChanged;
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

                tempMarker = CreateMarker(point.Value, new Factory() { Name = ApplicationSettings.GoogleMap.NewFactoryName, Address = searchEntryTextBox.Text.Trim(), Latitude = point.Value.Lat, Longitude = point.Value.Lng }, FactoryMarkerColor.Green);
                factoriesMapControl.Markers.Add(tempMarker);

                ZoomOnPoint(point.Value, ApplicationSettings.GoogleMap.ZoomMarker);
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

        private void ZoomOnFactory(Factory factory)
        {   
            PointLatLng? point = GetFactoryPosition(factory);

            if (point.HasValue)
            {
                ZoomOnPoint(point.Value, ApplicationSettings.GoogleMap.ZoomMarker);
                FactoryMarker marker = factoriesMapControl.Markers.Where(m => ((Factory)((FactoryMarker)m.Shape).DataContext).Id == factory.Id).Select(m => m.Shape as FactoryMarker).FirstOrDefault();

                if (marker != null)
                    marker.PlayBounce();
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
            tempMarker = null;
            tempPosMarker = null;
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
                System.Net.IPHostEntry test = System.Net.Dns.GetHostEntry(ApplicationSettings.GoogleMap.GoogleUrl);
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
            if (e.PropertyName == "SelectedFactory")
            {
                if (tempPosMarker != null)
                {   
                    factoriesMapControl.Markers.Remove(tempPosMarker);
                    tempPosMarker = null;                    
                }
            }
        }
        
        private void Factories_ListChanged(object sender, ListChangedEventArgs e)
        {
            RefreshMarkers();
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
            
        }

        private void FactoriesMapControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Point mousePos = e.GetPosition(factoriesMapControl);
                PointLatLng mapPosition = factoriesMapControl.FromLocalToLatLng((int)mousePos.X, (int)mousePos.Y);
                List<Placemark> placemarks = null;

                GeoCoderStatusCode status = GMapProviders.GoogleMap.GetPlacemarks(mapPosition, out placemarks);

                if (status == GeoCoderStatusCode.G_GEO_SUCCESS && placemarks != null && placemarks.Count > 0)
                {
                    if (tempMarker != null)
                        factoriesMapControl.Markers.Remove(tempMarker);

                    Factory factory = new Factory() { Name = ApplicationSettings.GoogleMap.NewFactoryName, Address = placemarks.FirstOrDefault().Address.Trim(), Latitude = mapPosition.Lat, Longitude = mapPosition.Lng };
                    GMapMarker marker = CreateMarker(mapPosition, factory, FactoryMarkerColor.Green);
                    tempMarker = marker;
                    factoriesMapControl.Markers.Add(marker);

                    _viewModel.SelectedFactory = factory;
                }
            }
        }

        private void factoriesMapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(IsLatLngSelectionMode)
            { 
                Point mousePos = e.GetPosition(factoriesMapControl);
                PointLatLng mapPosition = factoriesMapControl.FromLocalToLatLng((int)mousePos.X, (int)mousePos.Y);

                Factory factory = _viewModel.SelectedFactoryClone;
                factory.Latitude = mapPosition.Lat;
                factory.Longitude = mapPosition.Lng;

                _viewModel.SelectedFactoryClone = factory; //raise property changed

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
                Factory factory = (Factory)factoriesListView.SelectedItem;
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
        }
    }
}
