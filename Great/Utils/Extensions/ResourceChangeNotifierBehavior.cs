using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Great.Utils.Extensions
{
    public class ResourceChangeNotifierBehavior : System.Windows.Interactivity.Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty ResourceProperty
                = DependencyProperty.Register("Resource",typeof(object), typeof(ResourceChangeNotifierBehavior), new PropertyMetadata(default(object), ResourceChangedCallback));

        public event EventHandler ResourceChanged;

        public object Resource
        {
            get { return GetValue(ResourceProperty); }
            set { SetValue(ResourceProperty, value);}
        }

        private static void ResourceChangedCallback(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs args)

        {
            var resourceChangeNotifier = dependencyObject as ResourceChangeNotifierBehavior;
            if (resourceChangeNotifier == null)
                return;

            resourceChangeNotifier.OnResourceChanged();
        }


        private void OnResourceChanged()
        {
            EventHandler handler = ResourceChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }


    }
}
