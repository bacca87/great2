using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Great.Utils
{
    public static class ToastNotificationHelper
    {
        public static void SendToastNotification(string Title, string line1, string line2 , ToastTemplateType type)
        {
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
                ToastNotificationManager.CreateToastNotifier("Great").Show(toast);
            }
            catch (Exception ex){ }
        }
    }
}
