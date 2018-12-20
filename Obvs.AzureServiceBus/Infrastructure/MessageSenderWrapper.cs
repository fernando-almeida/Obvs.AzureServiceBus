using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Obvs.AzureServiceBus.Infrastructure {
    internal sealed class MessageSenderWrapper : IMessageSender {
        MessageSender _messageSender;
        Type _supportedMessageType;

        public MessageSenderWrapper(Type supportedMessageType, MessageSender messageSender) {
            if (supportedMessageType == null) throw new ArgumentNullException("supportedMessageType");
            if (messageSender == null) throw new ArgumentNullException("messageSender");

            _supportedMessageType = supportedMessageType;
            _messageSender = messageSender;
        }

        public Type SupportedMessageType {
            get {
                return _supportedMessageType;
            }
        }

        public Task SendAsync(Message message) {
            return _messageSender.SendAsync(message);
        }

        public void Dispose() {
            _messageSender.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

}
