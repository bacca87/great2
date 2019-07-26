using GalaSoft.MvvmLight.Messaging;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models
{
    public class EventManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        MSSharepointProvider provider;

        public EventManager(MSSharepointProvider exProvider)
        {
            provider = exProvider;
            provider.OnEventChanged += provider_OnEventChanged;
        }

        private void provider_OnEventChanged(object sender, EventDTO e)
        {
            ProcessEvent(e);
        }

        private void ProcessEvent(EventDTO ev)
        {
            using (DBArchive db = new DBArchive())
            {
                Event evnt = db.Events.SingleOrDefault(x => x.SharepointId == ev.SharepointId);
                if (evnt != null)
                {
                    evnt.Status = (long)ev.EStatus;
                    db.SaveChanges();
                    Messenger.Default.Send(new ItemChangedMessage<EventEVM>(this, new EventEVM(evnt)));
                }
            }

        }

        public bool Send(EventEVM ev)
        {
            if (ev == null)
                return false;

            using (new WaitCursor())
            {
                provider.Send(ev);
            }
            return true;
        }

        public enum EEventStatus
        {
            Accepted = 0,
            Rejected = 1,
            Pending = 2,
            New = 3,
            Cancelled = 4
        }
    }
}
