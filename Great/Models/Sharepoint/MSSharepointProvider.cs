using GalaSoft.MvvmLight.Messaging;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Models.Interfaces;
using Great.Utils.Extensions;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;
using static Great.Models.EventManager;
using static Great.Models.ExchangeTraceListener;

namespace Great.Models
{

    public class MSSharepointProvider
    {
        private ConcurrentQueue<EventEVM> eventQueue = new ConcurrentQueue<EventEVM>();
        public event EventHandler<EventDTO> OnEventChanged;
        private Thread eventUpdaterThread;
        private Thread eventSenderThred;

        public MSSharepointProvider()
        {
            //Load all events
            using (DBArchive db = new DBArchive())
                eventQueue = new ConcurrentQueue<EventEVM>(db.Events.ToList().Select(e => new EventEVM(e)));


            eventUpdaterThread = new Thread(UpdaterThread);
            eventUpdaterThread.Name = "Sharepoint polling thread";
            eventUpdaterThread.IsBackground = true;
            eventUpdaterThread.Start();

            eventSenderThred = new Thread(SenderThread);
            eventSenderThred.Name = "Sharepoint sender thread";
            eventSenderThred.IsBackground = true;
            eventSenderThred.Start();
        }

        private void SenderThread()
        {
            bool exit = false;

            while (!exit)
            {
                while (eventQueue.Any(e => e.EStatus == EEventStatus.New && e.EStatus != EEventStatus.Cancelled))
                {
                    EventEVM ev;
                    bool IsSent = false;

                    if (!eventQueue.TryDequeue(out ev))
                        continue;

                    do
                    {
                        try
                        {
                            var request = GenerateBatchInsertXML(ev);
                            using (SharepointReference.Lists l = new SharepointReference.Lists())
                            {
                                l.Credentials = new NetworkCredential(UserSettings.Email.Username, UserSettings.Email.EmailPassword);
                                XmlDocument xdoc = new XmlDocument();

                                //=== Temp bypass, to be tested better! ====
                                //     var response = l.UpdateListItems("Vacations ITA",request.FirstChild);
                                //      xdoc.LoadXml(response.OuterXml);
                                var eventId = Convert.ToInt32(xdoc.GetElementsByTagName("z:row")[0]?.Attributes["ows_ID"].Value ?? "0");

                                if (eventId>0)
                                {
                                    IsSent = true;
                                    ev.SharePointId = eventId;
                                    ev.EStatus = EEventStatus.Pending;
                                    ev.Save();
                                    eventQueue.Enqueue(ev);
                                    Messenger.Default.Send(new ItemChangedMessage<EventEVM>(this,ev));

                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    while (!IsSent);
                }

                Thread.Sleep(ApplicationSettings.General.WaitForNextEmailCheck);
            }

        }

        private void UpdaterThread()
        {
            bool exit = false;

            while (!exit)
            {
                while (eventQueue.Any(e => e.EStatus == EEventStatus.Pending))
                {
                    foreach (EventEVM e in eventQueue)
                    {
                        if (e.EStatus != EEventStatus.Pending) continue;

                        var camlQueryString = string.Format("<Query><Where><Eq><FieldRef Name='ID' /><Value Type='Number'>{0}</Value></Eq></Where></Query>", e.SharePointId);
                        //var camlQueryString = string.Format("<Query><Where><Contains><FieldRef Name='Title' /><Value Type='Text'>{0}</Value></Contains></Where></Query>", e.SharePointId);

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(camlQueryString);
                        XmlNode n = doc.DocumentElement;

                        using (SharepointReference.Lists l = new SharepointReference.Lists())
                        {
                            l.Credentials = new NetworkCredential(UserSettings.Email.Username, UserSettings.Email.EmailPassword);
                            var response = l.GetListItems("Vacations ITA", null, n, null, null, null, null);

                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(response.OuterXml);
                            var newStatus = Convert.ToInt32(xdoc.GetElementsByTagName("z:row")[0]?.Attributes["ows__ModerationStatus"].Value ?? "1"); //default rejected

                            if (newStatus != e.Status)
                            {
                                e.Status = newStatus;
                                e.Save();
                                Messenger.Default.Send(new ItemChangedMessage<EventEVM>(this, e));
                            }
                        }
                    }
                }

                Thread.Sleep(ApplicationSettings.General.WaitForNextEmailCheck);
            }
        }

        protected void NotifyEventChanged(EventDTO e)
        {
            OnEventChanged?.Invoke(this, e);
        }

        public void Send(EventEVM e)
        {
            eventQueue.Enqueue(e);
        }

        private static XmlDocument  GenerateBatchInsertXML(EventEVM ev)
        {
            var s = DateTime.Now.FromUnixTimestamp(ev.StartDateTimeStamp);
            var e = DateTime.Now.FromUnixTimestamp(ev.EndDateTimeStamp);

            var doc = new XDocument(
              new XElement("Batch",
              new XAttribute("OnError", "Continue"),
              new XAttribute("ListVersion", "1"),
              new XAttribute("ViewName", "Vacations ITA"),
              new XElement("Method",
                new XAttribute("ID", "1"),
                new XAttribute("Cmd", "New"),
                new XElement("Field", new XAttribute("Name", "Title"), ev.Description),
                new XElement("Field", new XAttribute("Name", "EventDate"), String.Format("{0:yyyy'-'MM'-'dd HH':'mm':'ss}", s)),
                new XElement("Field", new XAttribute("Name", "EndDate"), String.Format("{0:yyyy'-'MM'-'dd HH':'mm':'ss}", e)),
                new XElement("Field", new XAttribute("Name", "fAllDayEvent"), ev.IsAllDay?0:1),
                new XElement("Field", new XAttribute("Name", "EventType"), ev.Type-1))));

            XmlDocument d = new XmlDocument();
            d.LoadXml(doc.ToString());
            return d;


        }

        private static XDocument GenerateBatchDeletetXML(EventEVM ev)
        {
            var vis = new XDocument(
              new XElement("Batch",
              new XAttribute("OnError", "Continue"),
              new XAttribute("ListVersion", "1"),
              new XAttribute("ViewName", "Vacations ITA"),
              new XElement("Method",
                new XAttribute("ID", "1"),
                new XAttribute("Cmd", "Delete"),
                new XElement("Field", new XAttribute("Name", "ID"), ev.SharePointId))));
            return vis;


        }
    }
}
