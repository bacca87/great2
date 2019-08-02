using GalaSoft.MvvmLight.Messaging;
using Great.Utils;
using Great.Utils.Messages;
using Great.ViewModels.Database;
using NLog;

namespace Great.Models
{
    public class EventManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        MSSharepointProvider provider;

        public EventManager(MSSharepointProvider shProvider)
        {
            provider = shProvider;
            provider.OnEventChanged += provider_OnEventChanged;
        }

        private void provider_OnEventChanged(object sender, EventChangedEventArgs e)
        {
            ProcessEvent(e.Ev);
        }

        private void ProcessEvent(EventEVM ev)
        {
            Messenger.Default.Send(new ItemChangedMessage<EventEVM>(this, ev));
        }

        public bool Add(EventEVM ev)
        {
            if (ev == null)
                return false;

            provider.Add(ev);

            return true;
        }

        public bool Update(EventEVM ev)
        {
            if (ev == null)
                return false;

            provider.Update(ev);

            return true;
        }

        public bool Delete(EventEVM ev)
        {
            if (ev == null)
                return false;

            provider.Delete(ev);

            return true;

        }

        public enum EEventStatus
        {
            Accepted = 0,
            Rejected = 1,
            Pending = 2
        }
    }
}
