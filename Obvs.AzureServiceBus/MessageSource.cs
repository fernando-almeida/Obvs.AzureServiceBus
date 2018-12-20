using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

using Microsoft.Azure.ServiceBus;

using Obvs.AzureServiceBus.Infrastructure;
using Obvs.MessageProperties;
using Obvs.Serialization;

namespace Obvs.AzureServiceBus {

    public class MessageSource<TMessage> : IMessageSource<TMessage>
        where TMessage : class {
            private IObservable<Message> _Messages;
            private Dictionary<string, IMessageDeserializer<TMessage>> _deserializers;
            private CancellationTokenSource _messageReceiverMessageObservableCancellationTokenSource;
            private IMessageMessageTable _messageMessageTable;

            internal MessageSource(
                IMessagingEntityFactory messagingEntityFactory,
                IEnumerable<IMessageDeserializer<TMessage>> deserializers,
                IMessageMessageTable messageMessageTable) {
                if (messagingEntityFactory == null) throw new ArgumentNullException(nameof(messagingEntityFactory));

                IObservable<Message> Messages = CreateMessageObservableFromMessageReceiver(messagingEntityFactory);

                Initialize(Messages, deserializers, messageMessageTable);
            }

            internal MessageSource(IObservable<Message> Messages, IEnumerable<IMessageDeserializer<TMessage>> deserializers, IMessageMessageTable messageMessageTable) {
                Initialize(Messages, deserializers, messageMessageTable);
            }

            public IObservable<TMessage> Messages {
                get {
                    return _Messages
                        .Where(IsCorrectMessageType)
                        .Select(bm => new {
                            Message = bm,
                                DeserializedMessage = Deserialize(bm)
                        })
                        .Do(messageParts => _messageMessageTable.SetMessageForObject(messageParts.DeserializedMessage, messageParts.Message))
                        .Select(messageParts => messageParts.DeserializedMessage);
                }
            }

            public void Dispose() {
                if (_messageReceiverMessageObservableCancellationTokenSource != null) {
                    _messageReceiverMessageObservableCancellationTokenSource.Cancel();
                    _messageReceiverMessageObservableCancellationTokenSource.Dispose();
                    _messageReceiverMessageObservableCancellationTokenSource = null;
                }
            }

            private IObservable<Message> CreateMessageObservableFromMessageReceiver(IMessagingEntityFactory messagingEntityFactory) {
                _messageReceiverMessageObservableCancellationTokenSource = new CancellationTokenSource();

                return Observable.Create<Message>(async(observer, cancellationToken) => {
                    IMessageReceiver messageReceiver = messagingEntityFactory.CreateMessageReceiver(typeof(TMessage));

                    while (!cancellationToken.IsCancellationRequested &&
                        !_messageReceiverMessageObservableCancellationTokenSource.IsCancellationRequested &&
                        !messageReceiver.IsClosed) {
                        Message nextMessage = await messageReceiver.ReceiveAsync(); // NOTE: could pass the CancellationToken in here if ReceiveAsync is ever updated to accept it

                        if (nextMessage != null) {
                            observer.OnNext(nextMessage);
                        }
                    }

                    return () => messageReceiver.Dispose();
                }).Publish().RefCount();
            }

            private void Initialize(IObservable<Message> Messages, IEnumerable<IMessageDeserializer<TMessage>> deserializers, IMessageMessageTable messageMessageTable) {
                if (Messages == null) throw new ArgumentNullException(nameof(Messages));
                if (deserializers == null) throw new ArgumentNullException(nameof(deserializers));
                if (messageMessageTable == null) throw new ArgumentNullException(nameof(messageMessageTable));

                _Messages = Messages;
                _deserializers = deserializers.ToDictionary(d => d.GetTypeName());
                _messageMessageTable = messageMessageTable;
            }

            private bool IsCorrectMessageType(Message message) {
                object messageTypeName;
                bool messageTypeMatches = message.UserProperties.TryGetValue(MessagePropertyNames.TypeName, out messageTypeName);

                if (messageTypeMatches) {
                    messageTypeMatches = _deserializers.ContainsKey((string) messageTypeName);
                }

                return messageTypeMatches;
            }

            private TMessage Deserialize(Message message) {
                object messageTypeName;
                IMessageDeserializer<TMessage> messageDeserializerForType;

                if (message.UserProperties.TryGetValue(MessagePropertyNames.TypeName, out messageTypeName)) {
                    messageDeserializerForType = _deserializers[(string) messageTypeName];
                } else {
                    try {
                        messageDeserializerForType = _deserializers.Values.Single();
                    } catch (InvalidOperationException exception) {
                        throw new Exception("The message contained no explicit TypeName property. In this scenario there must be a single deserializer provided.", exception);
                    }
                }

                return messageDeserializerForType.Deserialize(message.Body);
            }
        }
}
