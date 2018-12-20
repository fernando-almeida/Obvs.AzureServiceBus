using System;
using System.Runtime.CompilerServices;

using Microsoft.Azure.ServiceBus;

namespace Obvs.AzureServiceBus.Infrastructure {
    internal interface IMessageMessageTable {
        void SetMessageForObject(object obj, Message message);

        Message GetMessageForObject(object message);

        void RemoveMessageForMessage(object message);
    }

    internal sealed class MessageMessageTable {
        private static readonly IMessageMessageTable Instance = new DefaultMessageMessageTable();

        public static IMessageMessageTable ConfiguredInstance {
            get {
                return Instance;
            }
        }
    }

    internal sealed class DefaultMessageMessageTable : IMessageMessageTable {
        ConditionalWeakTable<object, Message> _innerTable = new ConditionalWeakTable<object, Message>();

        public void SetMessageForObject(object obj, Message message) {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (message == null) throw new ArgumentNullException(nameof(message));

            _innerTable.Add(obj, message);
        }

        public Message GetMessageForObject(object message) {
            Message result;

            _innerTable.TryGetValue(message, out result);

            return result;
        }

        public void RemoveMessageForMessage(object message) => _innerTable.Remove(message);
    }
}
