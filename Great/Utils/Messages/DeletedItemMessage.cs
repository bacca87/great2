using GalaSoft.MvvmLight.Messaging;

namespace Great.Utils.Messages
{
    public class DeletedItemMessage<T> : GenericMessage<T>
    {
        //
        // Summary:
        //     Initializes a new instance of the DeletedItemMessage class.
        //
        // Parameters:
        //   content:
        //     A value to be passed to recipient(s).
        //       
        public DeletedItemMessage(T content) : base(content) { }
        //
        // Summary:
        //     Initializes a new instance of the DeletedItemMessage class.
        //
        // Parameters:
        //   sender:
        //     The message's sender.
        //
        //   content:
        //     A value to be passed to recipient(s).
        //        
        public DeletedItemMessage(object sender, T content) : base(sender, content) { }
    }
}
