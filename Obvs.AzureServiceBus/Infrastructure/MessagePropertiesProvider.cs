using System;
using System.Runtime.CompilerServices;

using Microsoft.Azure.ServiceBus;

namespace Obvs.AzureServiceBus.Infrastructure {
    public interface IMessagePropertiesProvider {
        IIncomingMessageProperties GetIncomingMessageProperties(object message);
        IOutgoingMessageProperties GetOutgoingMessageProperties(object message);
    }

    internal static class MessagePropertiesProvider {
        private static IMessagePropertiesProvider Instance;

        public static IMessagePropertiesProvider ConfiguredInstance {
            get {
                if (Instance == null) {
                    UseDefault();
                }

                return Instance;
            }
        }

        public static void Use(IMessagePropertiesProvider messagePropertiesProvider) {
            Instance = messagePropertiesProvider;
        }

        public static void UseDefault() => Use(new DefaultMessagePropertiesProvider(MessageMessageTable.ConfiguredInstance, MessageOutgoingPropertiesTable.ConfiguredInstance));

        public static void UseFakeMessagePropertiesProvider() => Use(new FakeMessagePropertiesProvider());
    }

    internal class FakeMessagePropertiesProvider : IMessagePropertiesProvider {
        private static readonly ConditionalWeakTable<object, Tuple<FakeIncomingMessageProperties, FakeOutgoingMessageProperties>> _trackedMessagePropertiesTable = new ConditionalWeakTable<object, Tuple<FakeIncomingMessageProperties, FakeOutgoingMessageProperties>>();

        public IIncomingMessageProperties GetIncomingMessageProperties(object message) => GetTrackedMessageProperties(message).Item1;

        public IOutgoingMessageProperties GetOutgoingMessageProperties(object message) => GetTrackedMessageProperties(message).Item2;

        private Tuple<FakeIncomingMessageProperties, FakeOutgoingMessageProperties> GetTrackedMessageProperties(object message) {
            Tuple<FakeIncomingMessageProperties, FakeOutgoingMessageProperties> messageProperties;

            if (!_trackedMessagePropertiesTable.TryGetValue(message, out messageProperties)) {
                messageProperties = new Tuple<FakeIncomingMessageProperties, FakeOutgoingMessageProperties>(new FakeIncomingMessageProperties(), new FakeOutgoingMessageProperties());

                _trackedMessagePropertiesTable.Add(message, messageProperties);
            }

            return messageProperties;
        }

        private sealed class FakeOutgoingMessageProperties : IOutgoingMessageProperties {
            public DateTime ScheduledEnqueueTimeUtc {
                get;
                set;
            }

            public TimeSpan TimeToLive {
                get;
                set;
            }
        }

        private sealed class FakeIncomingMessageProperties : IIncomingMessageProperties {
            public int DeliveryCount {
                get {
                    return 1;
                }
            }
        }
    }

    internal sealed class DefaultMessagePropertiesProvider : IMessagePropertiesProvider {
        private IMessageMessageTable _messageMessageTable;
        private IMessageOutgoingPropertiesTable _messageOutgoingPropertiesTable;

        public DefaultMessagePropertiesProvider(IMessageMessageTable messageMessageTable, IMessageOutgoingPropertiesTable messageOutgoingPropertiesTable) {
            _messageMessageTable = messageMessageTable;
            _messageOutgoingPropertiesTable = messageOutgoingPropertiesTable;
        }

        public IIncomingMessageProperties GetIncomingMessageProperties(object message) => new DefaultMessageIncomingMessageProperties(_messageMessageTable.GetMessageForObject(message));

        public IOutgoingMessageProperties GetOutgoingMessageProperties(object message) {
            IOutgoingMessageProperties result;

            result = _messageOutgoingPropertiesTable.GetOutgoingPropertiesForMessage(message);

            if (result == null) {
                result = new DefaultMessageOutgoingMessageProperties();

                _messageOutgoingPropertiesTable.SetOutgoingPropertiesForMessage(message, result);
            }

            return result;
        }
    }

    public interface IIncomingMessageProperties {
        int DeliveryCount {
            get;
        }
    }

    public interface IOutgoingMessageProperties {
        DateTime ScheduledEnqueueTimeUtc {
            get;
            set;
        }

        TimeSpan TimeToLive {
            get;
            set;
        }
    }

    internal sealed class DefaultMessageIncomingMessageProperties : IIncomingMessageProperties {
        private readonly Message _message;

        public DefaultMessageIncomingMessageProperties(Message message) {
            _message = message;
        }

        public int DeliveryCount {
            get {
                return _message.SystemProperties.DeliveryCount;
            }
        }
    }

    internal sealed class DefaultMessageOutgoingMessageProperties : IOutgoingMessageProperties {
        public DefaultMessageOutgoingMessageProperties() {

        }

        public DateTime ScheduledEnqueueTimeUtc {
            get;
            set;
        }

        public TimeSpan TimeToLive {
            get;
            set;
        }
    }
}
