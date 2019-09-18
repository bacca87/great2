using GalaSoft.MvvmLight.Messaging;
using Great.Models.Database;
using Great.Utils.Extensions;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Great.Models
{
    public class MSSharepointProvider
    {
        private Thread eventUpdaterThread;
        private Thread eventSenderThred;
        private Int32 sharepointUserId;

        public MSSharepointProvider()
        {
            eventUpdaterThread = new Thread(UpdaterThread);
            eventUpdaterThread.Name = "Sharepoint polling thread";
            eventUpdaterThread.IsBackground = true;
            eventUpdaterThread.Start();

            eventSenderThred = new Thread(SenderThread);
            eventSenderThred.Name = "Sharepoint sender thread";
            eventSenderThred.IsBackground = true;
            eventSenderThred.Start();
        }

        protected void NotifyEventChanged(EventEVM e)
        {
            e.IsChanged = false;
            List<DayEVM> DaysToClear = new List<DayEVM>();
            List<DayEVM> DaysInEvent = new List<DayEVM>();
            e.AddOrUpdateEventRelations(out DaysToClear, out DaysInEvent);
            Messenger.Default.Send(new ItemChangedMessage<EventEVM>(this, e));
            DaysToClear.ForEach(x => { Messenger.Default.Send(new ItemChangedMessage<DayEVM>(this, x)); });
            DaysInEvent.ForEach(x => { Messenger.Default.Send(new ItemChangedMessage<DayEVM>(this, x)); });
        }

        protected void NotifyEventDeleted(EventEVM e)
        {
            List<DayEVM> DaysToClear = new List<DayEVM>();
            List<DayEVM> DaysInEvent = new List<DayEVM>();
            e.AddOrUpdateEventRelations(out DaysToClear, out DaysInEvent);
            Messenger.Default.Send(new DeletedItemMessage<EventEVM>(this, e));
            DaysToClear.ForEach(x => { Messenger.Default.Send(new ItemChangedMessage<DayEVM>(this, x)); });

        }

        protected void NotifyEventImported(EventEVM e)
        {
            e.IsChanged = false;
            List<DayEVM> DaysToClear = new List<DayEVM>();
            List<DayEVM> DaysInEvent = new List<DayEVM>();
            e.AddOrUpdateEventRelations(out DaysToClear, out DaysInEvent);
            Messenger.Default.Send(new NewItemMessage<EventEVM>(this, e));
            DaysInEvent.ForEach(x => { Messenger.Default.Send(new ItemChangedMessage<DayEVM>(this, x)); });
        }

        private void SenderThread()
        {
            bool exit = false;

            while (!exit)
            {
                using (DBArchive db = new DBArchive())
                {
                    foreach (EventEVM ev in db.Events.ToList().Select(v => new EventEVM(v)).Where(e => !e.IsSent))
                    {
                        try
                        {
                            XmlDocument xdoc;
                            XmlNode request = null;

                            if (ev.IsCancelRequested)
                            {
                                if (ev.SharePointId > 0)
                                {
                                    request = GenerateBatchDeletetXML(ev);
                                    using (SharepointReference.Lists l = new SharepointReference.Lists())
                                    {
                                        l.Credentials = new NetworkCredential(UserSettings.Email.Username, UserSettings.Email.EmailPassword);
                                        xdoc = new XmlDocument();

                                        var response = l.UpdateListItems("Vacations ITA", request.FirstChild);
                                        xdoc.LoadXml(response.OuterXml);
                                        var ecode = Convert.ToInt32(xdoc.GetElementsByTagName("ErrorCode")[0].Value);

                                        if (ecode == 0)
                                        {
                                            NotifyEventDeleted(ev);
                                            ev.Delete();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ev.SharePointId > 0)
                                {
                                    request = GenerateBatchUpdateXML(ev);
                                }
                                else
                                {
                                    request = GenerateBatchInsertXML(ev);
                                }

                                using (SharepointReference.Lists l = new SharepointReference.Lists())
                                {
                                    l.Credentials = new NetworkCredential(UserSettings.Email.Username, UserSettings.Email.EmailPassword);
                                    xdoc = new XmlDocument();

                                    var response = l.UpdateListItems("Vacations ITA", request.FirstChild);
                                    xdoc.LoadXml(response.OuterXml);
                                    var eventId = Convert.ToInt32(xdoc.GetElementsByTagName("z:row")[0]?.Attributes["ows_ID"].Value ?? "0");
                                    var ecode = Convert.ToInt32(xdoc.GetElementsByTagName("ErrorCode")[0].Value);

                                    if (eventId > 0 && ecode == 0)
                                    {
                                        ev.SharePointId = eventId;
                                        ev.SendDateTime = DateTime.Now;
                                        ev.IsSent = true;
                                        ev.Save(db);
                                        NotifyEventChanged(ev);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }

                Thread.Sleep(ApplicationSettings.General.WaitForNextEmailCheck);
            }
        }

        private void UpdaterThread()
        {
            bool exit = false;
            XmlDocument xdoc = new XmlDocument();
            HttpWebRequest request = null;

            while (!exit)
            {
                try
                {
                    using (DBArchive db = new DBArchive())
                    {
                        // try to get user Id
                        if (sharepointUserId == 0)
                        {
                            request = (HttpWebRequest)WebRequest.Create($"{ApplicationSettings.General.IntranetAddress}/_api/web/currentuser");
                            request.Credentials = new NetworkCredential(UserSettings.Email.Username, UserSettings.Email.EmailPassword);
                            request.Method = "GET";

                            var webResponse = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();
                            xdoc.LoadXml(webResponse);
                            sharepointUserId = Convert.ToInt32(xdoc.GetElementsByTagName("content")[0].FirstChild.FirstChild.LastChild.Value.ToString());
                        }

                        //take in account only if current user id is retrieved
                        if (sharepointUserId > 0)
                        {
                            // try to get all submitted events
                            string req = $"{ApplicationSettings.General.IntranetAddress}/_api/web/Lists/GetByTitle('Vacations ITA')/Items?$filter=Author/Id eq {sharepointUserId}";

                            request = (HttpWebRequest)WebRequest.Create(req);
                            request.Credentials = new NetworkCredential(UserSettings.Email.Username, UserSettings.Email.EmailPassword);
                            request.Method = "GET";

                            var webResponse = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();
                            xdoc.LoadXml(webResponse);

                            foreach (XmlElement el in xdoc.GetElementsByTagName("entry"))
                            {
                                Int64 shpid = Convert.ToInt64(el.GetElementsByTagName("content")[0]?.FirstChild["d:Id"].InnerText);
                                int status = Convert.ToInt32(el.GetElementsByTagName("content")[0]?.FirstChild["d:OData__ModerationStatus"].InnerText);

                                if (shpid == 0)
                                {
                                    continue;
                                }

                                EventEVM tmp = new EventEVM();
                                tmp.IsSent = true; // the event is on calendar. Not necessary to send it
                                tmp.SharePointId = shpid;
                                tmp.Title = el.GetElementsByTagName("content")[0]?.FirstChild["d:Title"].InnerText.Trim('*');
                                tmp.Location = el.GetElementsByTagName("content")[0]?.FirstChild["d:Location"].InnerText;
                                tmp.Description = el.GetElementsByTagName("content")[0]?.FirstChild["d:Description"].InnerText;
                                tmp.Status = status;
                                tmp.IsAllDay = Convert.ToBoolean(el.GetElementsByTagName("content")[0]?.FirstChild["d:fAllDayEvent"].InnerText);
                                tmp.SendDateTime = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:Created"].InnerText);

                                if (tmp.IsAllDay)
                                {
                                    tmp.StartDate = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:EventDate"].InnerText.TrimEnd('Z'));
                                    tmp.EndDate = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:EndDate"].InnerText.TrimEnd('Z'));
                                }
                                else
                                {
                                    tmp.StartDate = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:EventDate"].InnerText);
                                    tmp.EndDate = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:EndDate"].InnerText);
                                }

                                if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Vacations")
                                {
                                    tmp.EType = EEventType.Vacations;
                                }
                                else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Customer Visit")
                                {
                                    tmp.EType = EEventType.CustomerVisit;
                                }
                                else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Business Trip")
                                {
                                    tmp.EType = EEventType.BusinessTrip;
                                }
                                else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Education")
                                {
                                    tmp.EType = EEventType.Education;
                                }
                                else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Other")
                                {
                                    tmp.EType = EEventType.Other;
                                }
                                else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Old Vacations")
                                {
                                    tmp.EType = EEventType.OldVacations;
                                }

                                if (tmp.EStatus == EEventStatus.Accepted)
                                {
                                    tmp.ApprovationDate = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:Modified"].InnerText);
                                    tmp.Approver = el.GetElementsByTagName("content")[0]?.FirstChild["d:Approver"].InnerText;
                                }



                                if (db.Events.SingleOrDefault(x => x.SharepointId == shpid) == null)
                                {
                                    tmp.Save(db);
                                    NotifyEventImported(tmp);
                                }

                                else
                                {
                                    var existing = new EventEVM(db.Events.SingleOrDefault(x => x.SharepointId == shpid));
                                    if (!tmp.IsEqual(existing) && !existing.IsCancelRequested)
                                    {
                                        tmp.Id = existing.Id;
                                        tmp.Notes = existing.Notes;
                                        //Global.Mapper.Map(tmp, existing);
                                        tmp.Save(db);
                                        NotifyEventChanged(tmp);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }

                Thread.Sleep(ApplicationSettings.General.WaitForNextEventCheck);
            }
        }

        private static XmlDocument GenerateBatchInsertXML(EventEVM ev)
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
                new XElement("Field", new XAttribute("Name", "fAllDayEvent"), ev.IsAllDay ? 1 : 0),
                new XElement("Field", new XAttribute("Name", "EventType"), ev.Type - 1))));

            XmlDocument d = new XmlDocument();
            d.LoadXml(doc.ToString());
            return d;
        }

        private static XmlDocument GenerateBatchUpdateXML(EventEVM ev)
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
                new XAttribute("Cmd", "Update"),
                new XElement("Field", new XAttribute("Name", "ID"), ev.SharePointId),
                new XElement("Field", new XAttribute("Name", "Title"), ev.Description),
                new XElement("Field", new XAttribute("Name", "EventDate"), String.Format("{0:yyyy'-'MM'-'dd HH':'mm':'ss}", s)),
                new XElement("Field", new XAttribute("Name", "EndDate"), String.Format("{0:yyyy'-'MM'-'dd HH':'mm':'ss}", e)),
                new XElement("Field", new XAttribute("Name", "fAllDayEvent"), ev.IsAllDay ? 0 : 1),
                new XElement("Field", new XAttribute("Name", "EventType"), ev.Type - 1))));

            XmlDocument d = new XmlDocument();
            d.LoadXml(doc.ToString());
            return d;
        }

        private static XmlDocument GenerateBatchDeletetXML(EventEVM ev)
        {
            var doc = new XDocument(
            new XElement("Batch",
            new XAttribute("OnError", "Continue"),
                new XElement("Method",
                new XAttribute("ID", "1"),
                new XAttribute("Cmd", "Delete"),
                new XElement("Field", new XAttribute("Name", "ID"), ev.SharePointId))));

            XmlDocument d = new XmlDocument();
            d.LoadXml(doc.ToString());
            return d;
        }

    }

    public class EventChangedEventArgs : EventArgs
    {
        public EventEVM Ev { get; internal set; }

        public EventChangedEventArgs(EventEVM ev)
        {
            Ev = ev;
        }
    }
}
