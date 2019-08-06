namespace Great.Models
{
    public enum EEventStatus
    {
        Accepted = 0,
        Rejected = 1,
        Pending = 2
    }

    public enum EEventType
    {
        Vacations = 1,
        CustomerVisit = 2,
        BusinessTrip = 3,
        Education = 4,
        Other = 5,
        OldVacations = 6
    }

    //public class EventManager
    //{
    //    private static Logger log = LogManager.GetCurrentClassLogger();

    //    MSSharepointProvider provider;

    //    public EventManager(MSSharepointProvider shProvider)
    //    {
    //        provider = shProvider;
    //        provider.OnEventChanged += provider_OnEventChanged;
    //    }

    //    private void provider_OnEventChanged(object sender, EventChangedEventArgs e)
    //    {
    //        ProcessEvent(e.Ev);
    //    }

    //    private void ProcessEvent(EventEVM ev)
    //    {
    //        Messenger.Default.Send(new ItemChangedMessage<EventEVM>(this, ev));
    //    }

    //    public bool Add(EventEVM ev)
    //    {
    //        if (ev == null)
    //            return false;

    //        provider.Add(ev);

    //        return true;
    //    }

    //    public bool Update(EventEVM ev)
    //    {
    //        if (ev == null)
    //            return false;

    //        provider.Update(ev);

    //        return true;
    //    }

    //    public bool Delete(EventEVM ev)
    //    {
    //        if (ev == null)
    //            return false;

    //        provider.Delete(ev);

    //        return true;

    //    }


    //}
}
