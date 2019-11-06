using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Great2.Utils
{
    public static class ToastNotificationHelper
    {
        public static bool Enabled = true;

        public static void SendToastNotification(string Title, string line1, string line2 , ToastTemplateType type)
        {
            if (!Enabled)
                return;

            try
            {
                //windows 10 allows max 3 lines
                var xml = $@"<toast>
                            <visual>
                                <binding template=""{type}"">
                                    <text id=""1"">{Title}</text>
                                    <text id=""2"">{line1}</text>
                                    <text id=""3"">{line2}</text>
                                </binding>
                            </visual>
                        </toast>";
                var toastXml = new XmlDocument();
                toastXml.LoadXml(xml);
                var toast = new ToastNotification(toastXml);

                toast.Failed += (o, args) => {
                    //TODO: add logs
                    var message = args.ErrorCode;
                };

                DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex){ }
        }
    }
}
