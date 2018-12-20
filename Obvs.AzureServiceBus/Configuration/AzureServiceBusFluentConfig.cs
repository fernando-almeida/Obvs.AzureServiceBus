using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;

using Obvs.AzureServiceBus.Infrastructure;
using Obvs.Configuration;
using Obvs.MessageProperties;
using Obvs.Serialization;

namespace Obvs.AzureServiceBus.Configuration {
    /// <summary>
    /// ICanAddAzureServiceBusServiceName
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface ICanAddAzureServiceBusServiceName<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {
        ICanSpecifyAzureServiceBusNamespace<TMessage, TCommand, TEvent, TRequest, TResponse> Named(string serviceName);
    }

    /// <summary>
    /// ICanSpecifyAzureServiceBusNamespace
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface ICanSpecifyAzureServiceBusNamespace<TMessage, TCommand, TEvent, TRequest, TResponse> : ICanSpecifyAzureServiceBusMessagingFactory<TMessage, TCommand, TEvent, TRequest, TResponse>, ICanSpecifyAzureServiceBusMessagingEntityVerifier<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {
        ICanSpecifyAzureServiceBusMessagingFactory<TMessage, TCommand, TEvent, TRequest, TResponse> WithConnectionString(string connectionString);
        ICanSpecifyAzureServiceBusMessagingFactory<TMessage, TCommand, TEvent, TRequest, TResponse> WithNamespaceManager(INamespaceManager namespaceManager);
        ICanSpecifyAzureServiceBusMessagingFactory<TMessage, TCommand, TEvent, TRequest, TResponse> WithNamespaceManager(
            string resourceGroupName,
            string namespaceName,
            IServiceBusManagementClient serviceBusManagementClient);
    }

    /// <summary>
    /// ICanSpecifyAzureServiceBusMessagingFactory
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface ICanSpecifyAzureServiceBusMessagingFactory<TMessage, TCommand, TEvent, TRequest, TResponse> : ICanSpecifyAzureServiceBusMessagingEntityVerifier<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {
        ICanSpecifyAzureServiceBusMessagingEntityVerifier<TMessage, TCommand, TEvent, TRequest, TResponse> WithMessagingFactory(IMessagingFactory messagingFactory);
        ICanSpecifyAzureServiceBusMessagingEntityVerifier<TMessage, TCommand, TEvent, TRequest, TResponse> WithMessagingFactory(string namespaceConnectionString);
    }

    /// <summary>
    /// ICanSpecifyAzureServiceBusMessagingEntityVerifier
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface ICanSpecifyAzureServiceBusMessagingEntityVerifier<TMessage, TCommand, TEvent, TRequest, TResponse> : ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> WithMessagingEntityVerifier(IMessagingEntityVerifier messagingEntityVerifier);
    }

    /// <summary>
    /// ICanSpecifyAzureServiceBusMessagingEntity
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> : ICanSpecifyPropertyProviders<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingQueueFor<T>(string queuePath) where T : class, TMessage;
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingQueueFor<T>(string queuePath, ReceiveMode receiveMode) where T : class, TMessage;
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingQueueFor<T>(string queuePath, MessagingEntityCreationOptions creationOptions) where T : class, TMessage;
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingQueueFor<T>(string queuePath, ReceiveMode receiveMode, MessagingEntityCreationOptions creationOptions) where T : class, TMessage;
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingTopicFor<T>(string topicPath) where T : class, TMessage;
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingTopicFor<T>(string topicPath, MessagingEntityCreationOptions creationOptions) where T : class, TMessage;
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingSubscriptionFor<T>(string topicPath, string subscriptionName) where T : class, TMessage;
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingSubscriptionFor<T>(string topicPath, string subscriptionName, ReceiveMode receiveMode) where T : class, TMessage;
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingSubscriptionFor<T>(string topicPath, string subscriptionName, MessagingEntityCreationOptions creationOptions) where T : class, TMessage;
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingSubscriptionFor<T>(string topicPath, string subscriptionName, ReceiveMode receiveMode, MessagingEntityCreationOptions creationOptions) where T : class, TMessage;
    }

    /// <summary>
    /// AzureServiceBusFluentConfig
    /// </summary>
    /// <typeparam name="TServiceMessage"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    internal class AzureServiceBusFluentConfig<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse>:
        ICanAddAzureServiceBusServiceName<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanSpecifyAzureServiceBusNamespace<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanSpecifyAzureServiceBusMessagingFactory<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanCreateEndpointAsClientOrServer<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanSpecifyEndpointSerializers<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanSpecifyAzureServiceBusMessagingEntityVerifier<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanSpecifyPropertyProviders<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage
    where TServiceMessage : class {
        private readonly ICanAddEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> _canAddEndpoint;
        private string _serviceName;
        private IMessageSerializer _serializer;
        private IMessageDeserializerFactory _deserializerFactory;
        private Func<Assembly, bool> _assemblyFilter;
        private Func<Type, bool> _typeFilter;
        private readonly List<MessageTypeMessagingEntityMappingDetails> _messageTypePathMappings = new List<MessageTypeMessagingEntityMappingDetails>();
        private IMessagingFactory _messagingFactory;
        private INamespaceManager _namespaceManager;
        private MessagePropertyProviderManager<TMessage> _messagePropertyProviderManager = new MessagePropertyProviderManager<TMessage>();
        private IMessagingEntityVerifier _messagingEntityVerifier;
        private IMessageOutgoingPropertiesTable _messageOutgoingPropertiesTable = MessageOutgoingPropertiesTable.ConfiguredInstance;

        public AzureServiceBusFluentConfig(ICanAddEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> canAddEndpoint) {
            _canAddEndpoint = canAddEndpoint;
        }

        public ICanSpecifyAzureServiceBusNamespace<TMessage, TCommand, TEvent, TRequest, TResponse> Named(string serviceName) {
            _serviceName = serviceName;

            return this;
        }

        public ICanAddEndpointOrLoggingOrCorrelationOrCreate<TMessage, TCommand, TEvent, TRequest, TResponse> AsClient() {
            return _canAddEndpoint.WithClientEndpoints(CreateProvider());
        }

        public ICanAddEndpointOrLoggingOrCorrelationOrCreate<TMessage, TCommand, TEvent, TRequest, TResponse> AsServer() {
            return _canAddEndpoint.WithServerEndpoints(CreateProvider());
        }

        public ICanAddEndpointOrLoggingOrCorrelationOrCreate<TMessage, TCommand, TEvent, TRequest, TResponse> AsClientAndServer() {
            return _canAddEndpoint.WithEndpoints(CreateProvider());
        }

        private AzureServiceBusEndpointProvider<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse> CreateProvider() {
            if (_messagingFactory == null) {
                string namespaceConnectionString = null; // TODO
                throw new NotImplementedException(nameof(namespaceConnectionString));
                _messagingFactory = new MessagingFactoryWrapper(namespaceConnectionString);
            }

            if (_messagingEntityVerifier == null) {
                _messagingEntityVerifier = new MessagingEntityVerifier(_namespaceManager);
            }

            _messagingEntityVerifier.EnsureMessagingEntitiesExist(_messageTypePathMappings);

            return new AzureServiceBusEndpointProvider<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse>(_serviceName, _messagingFactory, _serializer, _deserializerFactory, _messageTypePathMappings, _assemblyFilter, _typeFilter, _messagePropertyProviderManager, _messageOutgoingPropertiesTable);
        }

        public ICanSpecifyAzureServiceBusMessagingFactory<TMessage, TCommand, TEvent, TRequest, TResponse> WithConnectionString(string connectionString) {
            if (connectionString == null) {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return WithNamespaceManager(NamespaceManagerWrapper.CreateFromConnectionString(connectionString));
        }

        public ICanSpecifyAzureServiceBusMessagingFactory<TMessage, TCommand, TEvent, TRequest, TResponse> WithNamespaceManager(INamespaceManager namespaceManager) {
            if (namespaceManager == null) throw new ArgumentNullException("namespaceManager");

            _namespaceManager = namespaceManager;

            return this;
        }

        public ICanSpecifyAzureServiceBusMessagingFactory<TMessage, TCommand, TEvent, TRequest, TResponse> WithNamespaceManager(
            string resourceGroupName,
            string namespaceName,
            IServiceBusManagementClient serviceBusManagementClient
        ) {
            if (string.IsNullOrEmpty(resourceGroupName)) {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (string.IsNullOrEmpty(namespaceName)) {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (serviceBusManagementClient == null) {
                throw new ArgumentNullException(nameof(serviceBusManagementClient));
            }
            var namespaceManager = new NamespaceManagerWrapper(resourceGroupName, namespaceName, serviceBusManagementClient);
            return WithNamespaceManager(namespaceManager);
        }

        public ICanSpecifyAzureServiceBusMessagingEntityVerifier<TMessage, TCommand, TEvent, TRequest, TResponse> WithMessagingFactory(IMessagingFactory messagingFactory) {
            if (messagingFactory == null) throw new ArgumentNullException("messagingFactory");

            _messagingFactory = messagingFactory;

            return this;
        }

        public ICanSpecifyAzureServiceBusMessagingEntityVerifier<TMessage, TCommand, TEvent, TRequest, TResponse> WithMessagingFactory(string namespaceConnectionString) {
            return WithMessagingFactory(new MessagingFactoryWrapper(namespaceConnectionString));
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> WithMessagingEntityVerifier(IMessagingEntityVerifier messagingEntityVerifier) {
            if (messagingEntityVerifier == null) throw new ArgumentNullException("messagingEntityVerifier");

            _messagingEntityVerifier = messagingEntityVerifier;

            return this;
        }

        public ICanCreateEndpointAsClientOrServer<TMessage, TCommand, TEvent, TRequest, TResponse> SerializedWith(IMessageSerializer serializer, IMessageDeserializerFactory deserializerFactory) {
            _serializer = serializer;
            _deserializerFactory = deserializerFactory;

            return this;
        }

        public ICanCreateEndpointAsClientOrServer<TMessage, TCommand, TEvent, TRequest, TResponse> FilterMessageTypeAssemblies(Func<Assembly, bool> assemblyFilter = null, Func<Type, bool> typeFilter = null) {
            _assemblyFilter = assemblyFilter;
            _typeFilter = typeFilter;

            return this;
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingQueueFor<T>(string queuePath) where T : class, TMessage {
            return UsingQueueFor<T>(queuePath, ReceiveMode.ReceiveAndDelete);
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingQueueFor<T>(string queuePath, ReceiveMode receiveMode) where T : class, TMessage {
            return UsingQueueFor<T>(queuePath, receiveMode, MessagingEntityCreationOptions.None);
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingQueueFor<T>(string queuePath, MessagingEntityCreationOptions creationOptions) where T : class, TMessage {
            return UsingQueueFor<T>(queuePath, ReceiveMode.ReceiveAndDelete, creationOptions);
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingQueueFor<T>(string queuePath, ReceiveMode receiveMode, MessagingEntityCreationOptions creationOptions) where T : class, TMessage {
            AddMessageTypePathMapping(new MessageTypeMessagingEntityMappingDetails(typeof(T), queuePath, MessagingEntityType.Queue, creationOptions, receiveMode));

            return this;
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingTopicFor<T>(string topicPath) where T : class, TMessage {
            return UsingTopicFor<T>(topicPath, MessagingEntityCreationOptions.None);
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingTopicFor<T>(string topicPath, MessagingEntityCreationOptions creationOptions) where T : class, TMessage {
            AddMessageTypePathMapping(new MessageTypeMessagingEntityMappingDetails(typeof(T), topicPath, MessagingEntityType.Topic, creationOptions));

            return this;
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingSubscriptionFor<T>(string topicPath, string subscriptionName) where T : class, TMessage {
            return UsingSubscriptionFor<T>(topicPath, subscriptionName, MessagingEntityCreationOptions.None);
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingSubscriptionFor<T>(string topicPath, string subscriptionName, ReceiveMode receiveMode) where T : class, TMessage {
            return UsingSubscriptionFor<T>(topicPath, subscriptionName, receiveMode, MessagingEntityCreationOptions.None);
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingSubscriptionFor<T>(string topicPath, string subscriptionName, MessagingEntityCreationOptions creationOptions) where T : class, TMessage {
            return UsingSubscriptionFor<T>(topicPath, subscriptionName, ReceiveMode.ReceiveAndDelete, creationOptions);
        }

        public ICanSpecifyAzureServiceBusMessagingEntity<TMessage, TCommand, TEvent, TRequest, TResponse> UsingSubscriptionFor<T>(string topicPath, string subscriptionName, ReceiveMode receiveMode, MessagingEntityCreationOptions creationOptions) where T : class, TMessage {
            AddMessageTypePathMapping(new MessageTypeMessagingEntityMappingDetails(typeof(T), topicPath + "/subscriptions/" + subscriptionName, MessagingEntityType.Subscription, creationOptions, receiveMode));

            return this;
        }

        public ICanSpecifyPropertyProviders<TMessage, TCommand, TEvent, TRequest, TResponse> UsingMessagePropertyProviderFor<T>(IMessagePropertyProvider<T> messagePropertyProvider) where T : class, TMessage {
            if (messagePropertyProvider == null) throw new ArgumentNullException("provider");

            if (!typeof(TServiceMessage).IsAssignableFrom(typeof(T))) throw new ArgumentException(string.Format("{0} is not a subclass of {1}.", typeof(T).FullName, typeof(TServiceMessage).FullName), "messagePropertyProvider");

            _messagePropertyProviderManager.Add(messagePropertyProvider);

            return this;
        }

        private void AddMessageTypePathMapping(MessageTypeMessagingEntityMappingDetails messageTypePathMappingDetails) {
            MessageTypeMessagingEntityMappingDetails existingMessageTypePathMapping = _messageTypePathMappings.FirstOrDefault(mtpm => mtpm.MessageType == messageTypePathMappingDetails.MessageType && mtpm.MessagingEntityType == messageTypePathMappingDetails.MessagingEntityType);

            if (existingMessageTypePathMapping != null) {
                throw new MappingAlreadyExistsForMessageTypeException(existingMessageTypePathMapping.MessageType, existingMessageTypePathMapping.MessagingEntityType);
            }

            _messageTypePathMappings.Add(messageTypePathMappingDetails);
        }
    }
}
