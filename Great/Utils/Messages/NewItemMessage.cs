using GalaSoft.MvvmLight.Messaging;

namespace Great.Utils.Messages
{
    public class NewItemMessage<T> : GenericMessage<T>
    {
        //
        // Summary:
        //     Initializes a new instance of the NewItemMessage class.
        //
        // Parameters:
        //   content:
        //     A value to be passed to recipient(s).
        //       
        public NewItemMessage(T content) : base(content) { }
        //
        // Summary:
        //     Initializes a new instance of the NewItemMessage class.
        //
        // Parameters:
        //   sender:
        //     The message's sender.
        //
        //   content:
        //     A value to be passed to recipient(s).
        //        
        public NewItemMessage(object sender, T content) : base(sender, content) { }
    }
}
