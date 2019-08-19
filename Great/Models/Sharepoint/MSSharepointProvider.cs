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
            Messenger.Default.Send(new ItemChangedMessage<EventEVM>(this, e));
        }

        protected void NotifyEventImported(EventEVM e)
        {
            Messenger.Default.Send(new NewItemMessage<EventEVM>(this, e));
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

                            switch (ev.EStatus)
                            {
                                case EEventStatus.Pending:

                                    if (ev.SharePointId > 0) request = GenerateBatchUpdateXML(ev);
                                    else request = GenerateBatchInsertXML(ev);

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
                                    break;

                                case EEventStatus.Rejected:

                                    if (ev.SharePointId == 0)
                                    {
                                        ev.IsSent = true;
                                        ev.Save(db);
                                        continue;
                                    }
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
                                            ev.IsSent = true;
                                            ev.Save(db);
                                            NotifyEventChanged(ev);
                                        }
                                    }

                                    break;
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
                            string req = string.Format($"{ApplicationSettings.General.IntranetAddress}/_api/web/Lists/GetByTitle('Vacations ITA')/Items?$filter=Author/Id eq {0}", sharepointUserId);

                            request = (HttpWebRequest)WebRequest.Create(req);
                            request.Credentials = new NetworkCredential(UserSettings.Email.Username, UserSettings.Email.EmailPassword);
                            request.Method = "GET";

                            var webResponse = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();
                            xdoc.LoadXml(webResponse);

                            foreach (XmlElement el in xdoc.GetElementsByTagName("entry"))
                            {
                                Int64 shpid = Convert.ToInt64(el.GetElementsByTagName("content")[0]?.FirstChild["d:Id"].InnerText);
                                int status = Convert.ToInt32(el.GetElementsByTagName("content")[0]?.FirstChild["d:OData__ModerationStatus"].InnerText);

                                var existing = db.Events.SingleOrDefault(x => x.SharepointId == shpid);

                                if (shpid == 0) continue;
                                if (existing == null)
                                {
                                    //manually added to calendar. Import it!
                                    EventEVM tmp = new EventEVM();
                                    tmp.IsSent = true; // the event is on calendar. Not necessary to send it
                                    tmp.SharePointId = shpid;
                                    tmp.Title = el.GetElementsByTagName("content")[0]?.FirstChild["d:Title"].InnerText.Trim('*');
                                    tmp.Location = el.GetElementsByTagName("content")[0]?.FirstChild["d:Location"].InnerText;
                                    tmp.StartDate = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:EventDate"].InnerText);
                                    tmp.EndDate = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:EndDate"].InnerText);
                                    tmp.Description = el.GetElementsByTagName("content")[0]?.FirstChild["d:Description"].InnerText;
                                    tmp.Status = status;
                                    tmp.IsAllDay = Convert.ToBoolean(el.GetElementsByTagName("content")[0]?.FirstChild["d:fAllDayEvent"].InnerText);
                                    tmp.SendDateTime = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:Created"].InnerText);

                                    //FIx 2:00 am on sharepoint calendar that broke timestamp relation!
                                    if (tmp.IsAllDay)
                                    {
                                        tmp.StartDate = new DateTime(tmp.StartDate.Year, tmp.StartDate.Month, tmp.StartDate.Day, 0, 0, 0);
                                        tmp.EndDate = new DateTime(tmp.EndDate.Year, tmp.EndDate.Month, tmp.EndDate.Day, 0, 0, 0);
                                    }

                                    if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Vacations") tmp.EType = EEventType.Vacations;
                                    else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Customer Visit") tmp.EType = EEventType.CustomerVisit;
                                    else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Business Trip") tmp.EType = EEventType.BusinessTrip;
                                    else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Education") tmp.EType = EEventType.Education;
                                    else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Other") tmp.EType = EEventType.Other;
                                    else if (el.GetElementsByTagName("content")[0]?.FirstChild["d:Category"].InnerText == "Old Vacations") tmp.EType = EEventType.OldVacations;

                                    if (tmp.EStatus == EEventStatus.Accepted)
                                    {
                                        tmp.ApprovationDate = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:Modified"].InnerText);
                                        tmp.Approver = el.GetElementsByTagName("content")[0]?.FirstChild["d:Approver"].InnerText;
                                    }

                                    tmp.Save(db);

                                    if (tmp.EStatus != EEventStatus.Rejected)
                                        NotifyEventImported(tmp);
                                }
                                else
                                {
                                    EventEVM tmp = new EventEVM(existing);

                                    //if event does not have relations create it this is to patch error on previous releases
                                    if (!db.DayEvents.Any(x => x.EventId == tmp.Id))


                                    if (tmp.EStatus != EEventStatus.Pending || tmp.EType != EEventType.Vacations) continue;

                                    if ((EEventStatus)status < tmp.EStatus)
                                    {
                                        tmp.EStatus = (EEventStatus)status;
                                        tmp.ApprovationDate = Convert.ToDateTime(el.GetElementsByTagName("content")[0]?.FirstChild["d:Modified"].InnerText);

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

                Thread.Sleep(ApplicationSettings.General.WaitForNextEventChek);
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

        //public void AddOrUpdateEventRelations(EventEVM ev, DBArchive db)
        //{
        //    List<DayEVM> currentDayEVM;
        //    List<DayEVM> newDays = new List<DayEVM>();

        //    List<DayEventEVM> relationsToAdd = new List<DayEventEVM>();
        //    IEnumerable<Day> currentDays = (from d in db.Days join e in db.DayEvents on d.Timestamp equals e.Timestamp where e.EventId == ev.Id select d);
        //    currentDayEVM = currentDays.Select(x => new DayEVM(x)).ToList();

        //    foreach (DateTime d in AllDatesInRange(ev.StartDate, ev.EndDate))
        //    {
        //        long timestamp = d.ToUnixTimestamp();
        //        Day currentDay = db.Days.SingleOrDefault(x => x.Timestamp == timestamp);

        //        if (currentDay == null)
        //            newDays.Add(new DayEVM { Date = d });

        //        else newDays.Add(new DayEVM(currentDay));

        //        relationsToAdd.Add(new DayEventEVM { TimeStamp = timestamp, EventId = ev.Id });
        //    }

        //    db.DayEvents.RemoveRange(db.DayEvents.Where(x => x.EventId == ev.Id));
        //    newDays.ToList().ForEach(d => d.Save(db));
        //    relationsToAdd.ToList().ForEach(r => r.Save(db));


        //    if (ev.EType == EEventType.Vacations)
        //    {
        //        if (ev.EStatus == EEventStatus.Accepted) newDays.ToList().ForEach(d => { if (d.TotalTime == null && !d.IsHoliday) d.EType = EDayType.VacationDay; d.Save(db); });
        //        if (ev.EStatus == EEventStatus.Rejected) newDays.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday) d.EType = EDayType.WorkDay; d.Save(db); });
        //        if (ev.EStatus == EEventStatus.Pending) newDays.ToList().ForEach(d => { if (d.WorkTime == null && !d.IsHoliday) d.EType = EDayType.SpecialLeave; d.Save(db); });
        //    }

        //    //var commonDays = newDays.Intersect(currentDayEVM).ToList();
        //    //var toReset = currentDayEVM.Except(commonDays).ToList();
        //    //toReset.ForEach(x => x.EType = EDayType.WorkDay);
        //    //var toadd = newDays.Except(commonDays).ToList();

        //    //var daysChanged = commonDays.Union(toReset).Union(toadd).ToList();
        //    //daysChanged.ForEach(d => { Messenger.Default.Send(new ItemChangedMessage<DayEVM>(this, d)); });

        //}

        //public static IEnumerable<DateTime> AllDatesInRange(DateTime startDate, DateTime endDate)
        //{
        //    List<DateTime> dates = new List<DateTime>();

        //    DateTime pointer = startDate;

        //    do
        //    {
        //        dates.Add(new DateTime(pointer.Date.Year, pointer.Date.Month, pointer.Date.Day, pointer.Hour, pointer.Minute, pointer.Second));
        //        pointer = pointer.AddDays(1);
        //    }
        //    while (pointer <= endDate);


        //    return dates;
        //}

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
