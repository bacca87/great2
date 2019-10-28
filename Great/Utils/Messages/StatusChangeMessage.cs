using GalaSoft.MvvmLight.Messaging;

namespace Great2.Utils.Messages
{
    public class StatusChangeMessage<T> : GenericMessage<T>
    {
        //
        // Summary:
        //     Initializes a new instance of the StatusChangeMessage class.
        //
        // Parameters:
        //   content:
        //     A value to be passed to recipient(s).
        //       
        public StatusChangeMessage(T content) : base(content) { }
        //
        // Summary:
        //     Initializes a new instance of the StatusChangeMessage class.
        //
        // Parameters:
        //   sender:
        //     The message's sender.
        //
        //   content:
        //     A value to be passed to recipient(s).
        //        
        public StatusChangeMessage(object sender, T content) : base(sender, content) { }
    }
}
