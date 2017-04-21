using System.Windows;

namespace Great.Utils.AttachedProperties
{
    public static class NotificationHelper
    {
        public static readonly DependencyProperty NotificationCount = 
            DependencyProperty.RegisterAttached("NotificationCount", typeof(int), typeof(NotificationHelper), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.Inherits));
        
        public static void SetNotificationCount(UIElement element, int value)
        {
            element.SetValue(NotificationCount, value);
        }
        
        public static int GetNotificationCount(UIElement element)
        {
            return (int)element.GetValue(NotificationCount);
        }
    }
}
