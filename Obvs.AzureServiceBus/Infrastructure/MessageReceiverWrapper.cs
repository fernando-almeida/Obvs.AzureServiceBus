using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

using Obvs.AzureServiceBus.Configuration;

namespace Obvs.AzureServiceBus.Infrastructure {

    /// <summary>
    /// Wrapper for Azure message receiber
    /// </summary>
    internal sealed class MessageReceiverWrapper : IMessageReceiver {
        private readonly Type _supportedMessageType;
        private readonly MessageReceiver _messageReceiver;

        public MessageReceiverWrapper(Type supportedMessageType, MessageReceiver messageReceiver) {
            _supportedMessageType = supportedMessageType;
            _messageReceiver = messageReceiver;
        }

        public Type SupportedMessageType {
            get {
                return _supportedMessageType;
            }
        }

        public ReceiveMode Mode {
            get {
                return _messageReceiver.ReceiveMode;
            }
        }

        public bool IsClosed {
            get {
                return _messageReceiver.ServiceBusConnection.IsClosedOrClosing;
            }
        }

        public Task<Message> ReceiveAsync() {
            return _messageReceiver.ReceiveAsync();
        }

        public void Dispose() {
            _messageReceiver.ServiceBusConnection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
