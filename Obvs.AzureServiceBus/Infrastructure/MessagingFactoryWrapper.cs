using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

using Obvs.AzureServiceBus.Configuration;

namespace Obvs.AzureServiceBus.Infrastructure {

    /// <summary>
    /// Messaging factory wrapper
    /// </summary>
    internal sealed class MessagingFactoryWrapper : IMessagingFactory {
        private readonly string _namespaceConnectionString;

        public MessagingFactoryWrapper(string namespaceConnectionString) {
            if (string.IsNullOrEmpty(namespaceConnectionString)) {
                throw new ArgumentNullException(nameof(namespaceConnectionString));
            }
            _namespaceConnectionString = namespaceConnectionString;
        }

        public IMessageSender CreateMessageSender(Type messageType, string entityPath) {
            var messageSender = new MessageSender(_namespaceConnectionString, entityPath);
            return new MessageSenderWrapper(messageType, messageSender);
        }

        public IMessageReceiver CreateMessageReceiver(Type messageType, string entityPath, ReceiveMode receiveMode) {
            var messageReceiver = new MessageReceiver(_namespaceConnectionString, entityPath, receiveMode);
            return new MessageReceiverWrapper(messageType, messageReceiver);
        }
    }
}
