using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Obvs.AzureServiceBus.Infrastructure;
using Obvs.Configuration;
using Obvs.MessageProperties;
using Obvs.Serialization;

namespace Obvs.AzureServiceBus.Configuration {

    /// <summary>
    /// Azure service bus endpoint provider
    /// </summary>
    /// <typeparam name="TServiceMessage"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class AzureServiceBusEndpointProvider<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse> : ServiceEndpointProviderBase<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage
    where TServiceMessage : class {
        private readonly IMessageSerializer _messageSerializer;
        private readonly IMessageDeserializerFactory _messageDeserializerFactory;
        private readonly Func<Assembly, bool> _assemblyFilter;
        private readonly Func<Type, bool> _typeFilter;
        private readonly List<MessageTypeMessagingEntityMappingDetails> _messageTypePathMappings;
        private readonly MessagingEntityFactory _messagingEntityFactory;
        private readonly IMessageOutgoingPropertiesTable _messageOutgoingPropertiesTable;
        private readonly MessagePropertyProviderManager<TMessage> _messagePropertyProviderManager;

        public AzureServiceBusEndpointProvider(string serviceName, IMessagingFactory messagingFactory, IMessageSerializer messageSerializer, IMessageDeserializerFactory messageDeserializerFactory, List<MessageTypeMessagingEntityMappingDetails> messageTypePathMappings, Func<Assembly, bool> assemblyFilter, Func<Type, bool> typeFilter, MessagePropertyProviderManager<TMessage> messagePropertyProviderManager, IMessageOutgoingPropertiesTable messageOutgoingPropertiesTable) : base(serviceName) {
            if (messagingFactory == null) throw new ArgumentNullException(nameof(messagingFactory));
            if (messageSerializer == null) throw new ArgumentNullException(nameof(messageSerializer));
            if (messageDeserializerFactory == null) throw new ArgumentNullException(nameof(messageDeserializerFactory));
            if (messageTypePathMappings == null) throw new ArgumentNullException(nameof(messageTypePathMappings));
            if (messageTypePathMappings.Count == 0) throw new ArgumentException("An empty set of path mappings was specified.", nameof(messageTypePathMappings));
            if (messagePropertyProviderManager == null) throw new ArgumentNullException(nameof(messagePropertyProviderManager));
            if (messageOutgoingPropertiesTable == null) throw new ArgumentNullException(nameof(messageOutgoingPropertiesTable));

            _messageSerializer = messageSerializer;
            _messageDeserializerFactory = messageDeserializerFactory;
            _assemblyFilter = assemblyFilter;
            _typeFilter = typeFilter;
            _messageTypePathMappings = messageTypePathMappings;
            _messagePropertyProviderManager = messagePropertyProviderManager;
            _messageOutgoingPropertiesTable = messageOutgoingPropertiesTable;

            _messagingEntityFactory = new MessagingEntityFactory(messagingFactory, messageTypePathMappings);
        }

        public override IServiceEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> CreateEndpoint() {
            return new ServiceEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse>(
                GetMessageSource<TRequest>(),
                GetMessageSource<TCommand>(),
                new MessagePublisher<TEvent>(_messagingEntityFactory, _messageSerializer, _messagePropertyProviderManager.GetMessagePropertyProviderFor<TEvent>(), _messageOutgoingPropertiesTable),
                new MessagePublisher<TResponse>(_messagingEntityFactory, _messageSerializer, _messagePropertyProviderManager.GetMessagePropertyProviderFor<TResponse>(), _messageOutgoingPropertiesTable),
                typeof(TServiceMessage));
        }

        public override IServiceEndpointClient<TMessage, TCommand, TEvent, TRequest, TResponse> CreateEndpointClient() {

            return new ServiceEndpointClient<TMessage, TCommand, TEvent, TRequest, TResponse>(
                GetMessageSource<TEvent>(),
                GetMessageSource<TResponse>(),
                new MessagePublisher<TRequest>(_messagingEntityFactory, _messageSerializer, _messagePropertyProviderManager.GetMessagePropertyProviderFor<TRequest>(), _messageOutgoingPropertiesTable),
                new MessagePublisher<TCommand>(_messagingEntityFactory, _messageSerializer, _messagePropertyProviderManager.GetMessagePropertyProviderFor<TCommand>(), _messageOutgoingPropertiesTable),
                typeof(TServiceMessage));
        }

        private IMessageSource<TSourceMessage> GetMessageSource<TSourceMessage>() where TSourceMessage : class, TMessage {
            // Find mappings for source types thare are assignable from the target type
            var sourceMessageTypePathMappings = (from mtpm in _messageTypePathMappings where(mtpm.MessagingEntityType == MessagingEntityType.Queue ||
                    mtpm.MessagingEntityType == MessagingEntityType.Subscription) &&
                typeof(TSourceMessage).IsAssignableFrom(mtpm.MessageType) select mtpm);

            IMessageSource<TSourceMessage> result;

            // If there's only one target path mapping for this message type then just return a single MessageSource<T> instance (avoid overhead of MergedMessageSource)
            if (sourceMessageTypePathMappings.Count() == 1) {
                result = CreateMessageSource<TSourceMessage>(sourceMessageTypePathMappings.First());
            } else {
                result = new MergedMessageSource<TSourceMessage>(sourceMessageTypePathMappings.Select(mtpm => CreateMessageSource<TSourceMessage>(mtpm)));
            }

            return result;
        }

        private IMessageSource<TSourceMessage> CreateMessageSource<TSourceMessage>(MessageTypeMessagingEntityMappingDetails messageTypeMessageingEntityMappingDetails) where TSourceMessage : class, TMessage {
            Type messageType = messageTypeMessageingEntityMappingDetails.MessageType;
            Type messageSourceType = typeof(MessageSource<>).MakeGenericType(messageType);
            Type messageSourceDeserializerType = typeof(IMessageDeserializer<>).MakeGenericType(messageType);

            return Expression.Lambda<Func<IMessageSource<TSourceMessage>>>(
                Expression.New(messageSourceType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(IMessagingEntityFactory), typeof(IEnumerable<>).MakeGenericType(messageSourceDeserializerType), typeof(IMessageMessageTable) }, null),
                    Expression.Constant(_messagingEntityFactory),
                    Expression.Call(
                        Expression.Constant(_messageDeserializerFactory),
                        typeof(IMessageDeserializerFactory).GetMethod("Create").MakeGenericMethod(messageType, typeof(TServiceMessage)),
                        Expression.Constant(_assemblyFilter, typeof(Func<Assembly, bool>)),
                        Expression.Constant(_typeFilter, typeof(Func<Type, bool>))),
                    Expression.Constant(MessageMessageTable.ConfiguredInstance, typeof(IMessageMessageTable)))).Compile().Invoke();
        }
    }
}
