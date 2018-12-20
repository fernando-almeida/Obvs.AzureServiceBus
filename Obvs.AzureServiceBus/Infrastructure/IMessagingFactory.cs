using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Azure.ServiceBus;

using Obvs.AzureServiceBus.Configuration;

namespace Obvs.AzureServiceBus.Infrastructure {

    /// <summary>
    /// Messaging factory interface
    /// </summary>
    public interface IMessagingFactory {

        /// <summary>
        /// Create message sender
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="entityPath"></param>
        /// <returns></returns>
        IMessageSender CreateMessageSender(Type messageType, string entityPath);

        /// <summary>
        /// Create message receiver
        /// /// </summary>
        /// <param name="messageType"></param>
        /// <param name="entityPath"></param>
        /// <param name="receiveMode"></param>
        /// <returns></returns>
        IMessageReceiver CreateMessageReceiver(Type messageType, string entityPath, ReceiveMode receiveMode);
    }
}
