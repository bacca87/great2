using GalaSoft.MvvmLight.Messaging;

namespace Great.Utils.Messages
{
    public class ItemChangedMessage<T> : GenericMessage<T>
    {
        //
        // Summary:
        //     Initializes a new instance of the ItemChangedMessage class.
        //
        // Parameters:
        //   content:
        //     A value to be passed to recipient(s).
        //       
        public ItemChangedMessage(T content) : base(content)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the ItemChangedMessage class.
        //
        // Parameters:
        //   sender:
        //     The message's sender.
        //
        //   content:
        //     A value to be passed to recipient(s).
        //        
        public ItemChangedMessage(object sender, T content) : base(sender, content)
        {
        }
    }
}