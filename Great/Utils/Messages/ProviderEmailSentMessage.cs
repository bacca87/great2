using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great2.Utils.Messages
{
    class ProviderEmailSentMessage<T>:GenericMessage<T>
    {
        //
        // Summary:
        //     Initializes a new instance of the ProviderEmailSentMessage class.
        //
        // Parameters:
        //   content:
        //     A value to be passed to recipient(s).
        //       
        public ProviderEmailSentMessage(T content) : base(content) { }
        //
        // Summary:
        //     Initializes a new instance of the ProviderEmailSentMessage class.
        //
        // Parameters:
        //   sender:
        //     The message's sender.
        //
        //   content:
        //     A value to be passed to recipient(s).
        //        
        public ProviderEmailSentMessage(object sender, T content) : base(sender, content) { }
    }
}
