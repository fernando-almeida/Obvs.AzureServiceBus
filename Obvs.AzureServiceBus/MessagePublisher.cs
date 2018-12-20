using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

using Obvs.AzureServiceBus.Infrastructure;
using Obvs.MessageProperties;
using Obvs.Serialization;
using Obvs.Types;

namespace Obvs.AzureServiceBus {

    /// <summary>
    /// Message publisher
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class MessagePublisher<TMessage> : IMessagePublisher<TMessage>
        where TMessage : class {
            private readonly IMessagingEntityFactory _messagingEntityFactory;
            private readonly IMessageSerializer _serializer;
            private readonly IMessagePropertyProvider<TMessage> _propertyProvider;
            private readonly IMessageOutgoingPropertiesTable _messageOutgoingPropertiesTable;
            private readonly ConcurrentDictionary<Type, IMessageSender> _messageTypeMessageSenderMap;

            internal MessagePublisher(IMessagingEntityFactory messagingEntityFactory, IMessageSerializer serializer, IMessagePropertyProvider<TMessage> propertyProvider, IMessageOutgoingPropertiesTable messageOutgoingPropertiesTable) {
                if (messagingEntityFactory == null) throw new ArgumentNullException(nameof(messagingEntityFactory));
                if (serializer == null) throw new ArgumentNullException(nameof(serializer));
                if (propertyProvider == null) throw new ArgumentNullException(nameof(propertyProvider));
                if (messageOutgoingPropertiesTable == null) throw new ArgumentNullException(nameof(messageOutgoingPropertiesTable));

                _messagingEntityFactory = messagingEntityFactory;
                _serializer = serializer;
                _propertyProvider = propertyProvider;
                _messageOutgoingPropertiesTable = messageOutgoingPropertiesTable;

                _messageTypeMessageSenderMap = new ConcurrentDictionary<Type, IMessageSender>();
            }

            public Task PublishAsync(TMessage message) {
                IEnumerable<KeyValuePair<string, object>> properties = _propertyProvider.GetProperties(message);

                return PublishAsync(message, properties);
            }

            /// <inheritdoc />
            public void Dispose() {
                foreach (IMessageSender messageSender in _messageTypeMessageSenderMap.Values) {
                    messageSender.Dispose();
                }
            }

            private async Task PublishAsync(TMessage tmessage, IEnumerable<KeyValuePair<string, object>> properties) {
                // NOTE: we don't dispose of the MemoryStream here because Message assumes ownership of it's lifetime
                MemoryStream messageBodyStream = new MemoryStream();

                _serializer.Serialize(messageBodyStream, tmessage);

                messageBodyStream.Position = 0;

                Message message = new Message(messageBodyStream.ToArray());

                ApplyAnyOutgoingProperties(tmessage, message);

                SetSessionAndCorrelationIdentifiersIfApplicable(tmessage, message);

                SetProperties(tmessage, properties, message);

                IMessageSender messageSenderForMessageType = GetMessageSenderForMessageType(tmessage);

                await messageSenderForMessageType.SendAsync(message);
            }

            private static void SetProperties(
                TMessage tmessage,
                IEnumerable<KeyValuePair<string, object>> properties,
                Message message) {
                message.UserProperties.Add(MessagePropertyNames.TypeName, message.GetType().Name);

                foreach (KeyValuePair<string, object> property in properties) {
                    message.UserProperties.Add(property);
                }
            }

            private void SetSessionAndCorrelationIdentifiersIfApplicable(TMessage tmessage, Message message) {
                IRequest requestMessage = message as IRequest;

                if (requestMessage != null) {
                    SetRequestSessionAndCorrelationIdentifiers(message, requestMessage);
                } else {
                    IResponse responseMessage = message as IResponse;

                    if (responseMessage != null) {
                        SetResponseSessionAndCorrelationIdentifiers(message, responseMessage);
                    }
                }
            }

            private static void SetRequestSessionAndCorrelationIdentifiers(Message message, IRequest requestMessage) {
                string requesterId = requestMessage.RequesterId;

                if (!string.IsNullOrEmpty(requesterId)) {
                    message.ReplyToSessionId = requesterId;
                }

                message.CorrelationId = requestMessage.RequestId;
            }

            private static void SetResponseSessionAndCorrelationIdentifiers(Message message, IResponse responseMessage) {
                string requesterId = responseMessage.RequesterId;

                if (!string.IsNullOrEmpty(requesterId)) {
                    message.SessionId = requesterId;
                }

                message.CorrelationId = responseMessage.RequestId;
            }

            private IMessageSender GetMessageSenderForMessageType(TMessage message) {
                Type messageType = message.GetType();

                return _messageTypeMessageSenderMap.GetOrAdd(
                    messageType,
                    CreateMessageSenderForMessageType);
            }

            private IMessageSender CreateMessageSenderForMessageType(Type messageType) {
                return _messagingEntityFactory.CreateMessageSender(messageType);
            }

            private void ApplyAnyOutgoingProperties(TMessage tmessage, Message message) {
                IOutgoingMessageProperties outgoingProperties = _messageOutgoingPropertiesTable.GetOutgoingPropertiesForMessage(message);

                // Check if there were even any outgoing properties set for this message
                if (outgoingProperties != null) {
                    message.ScheduledEnqueueTimeUtc = outgoingProperties.ScheduledEnqueueTimeUtc;
                    message.TimeToLive = outgoingProperties.TimeToLive;

                    // Remove the properties for the message from the table now that we've mapped them as they'll have no further use beyond this point
                    _messageOutgoingPropertiesTable.RemoveOutgoingPropertiesForMessage(tmessage);
                }
            }
        }
}
