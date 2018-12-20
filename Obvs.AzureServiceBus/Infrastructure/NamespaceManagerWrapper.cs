using System;
using System.Threading.Tasks;

using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent.Models;
using Microsoft.Azure.Management.ServiceBus.Models;
using Microsoft.Azure.ServiceBus;

namespace Obvs.AzureServiceBus.Infrastructure {

    /// <summary>
    /// Namespace manager wrapper
    /// </summary>
    internal sealed class NamespaceManagerWrapper : INamespaceManager {
        private readonly string _resourceGroupName;
        private readonly string _namespaceName;

        private readonly IServiceBusManagementClient _serviceBusManagementClient;

        public Uri GetAddress() {
            var @namespace = _serviceBusManagementClient.Namespaces
                .GetAsync(_resourceGroupName, _namespaceName)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return new Uri(@namespace.ServiceBusEndpoint);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceGroupName"></param>
        /// <param name="namespaceName"></param>
        /// <param name="serviceBusManagementClient"></param>
        public NamespaceManagerWrapper(
            string resourceGroupName,
            string namespaceName,
            IServiceBusManagementClient serviceBusManagementClient
        ) {
            if (string.IsNullOrEmpty(resourceGroupName)) {
                throw new NullReferenceException(nameof(resourceGroupName));
            }
            if (string.IsNullOrEmpty(namespaceName)) {
                throw new NullReferenceException(nameof(namespaceName));
            }
            if (serviceBusManagementClient == null) {
                throw new NullReferenceException(nameof(serviceBusManagementClient));
            }
            _resourceGroupName = resourceGroupName;
            _namespaceName = namespaceName;
            _serviceBusManagementClient = serviceBusManagementClient;
        }

        // public NamespaceManagerSettings Settings {
        //     get {
        //         return _serviceBusManagementClient.Settings;
        //     }
        // }

        public bool QueueExists(string path) {
            var queue = _serviceBusManagementClient.Queues.GetAsync(
                    _resourceGroupName,
                    _namespaceName,
                    path
                ).ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return queue != null;
        }

        public void CreateQueue(string path) {
            // TODO Parse components from URI
            var queueParams = new QueueInner {
                EnablePartitioning = true
            };
            _serviceBusManagementClient.Queues.CreateOrUpdateAsync(
                    _resourceGroupName,
                    _namespaceName,
                    path,
                    queueParams
                ).ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public void DeleteQueue(string path) {
            _serviceBusManagementClient.Queues.DeleteAsync(
                    _resourceGroupName,
                    _namespaceName,
                    path
                ).ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public bool TopicExists(string path) {
            var topic = _serviceBusManagementClient.Topics.GetAsync(
                    _resourceGroupName,
                    _namespaceName,
                    path
                ).ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return topic != null;
        }

        public void CreateTopic(string path) {
            var topicInner = new TopicInner {};
            _serviceBusManagementClient.Topics
                .CreateOrUpdateAsync(_resourceGroupName, _namespaceName, path, topicInner)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public void DeleteTopic(string path) {
            _serviceBusManagementClient.Topics.DeleteAsync(
                    _resourceGroupName,
                    _namespaceName,
                    path
                ).ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public bool SubscriptionExists(string topicPath, string subscriptionName) {
            var subscription = _serviceBusManagementClient.Subscriptions.GetAsync(
                    _resourceGroupName,
                    _namespaceName,
                    topicPath,
                    subscriptionName
                ).ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return subscription != null;
        }

        public void CreateSubscription(string topicPath, string subscriptionName) {
            var subscriptionInner = new SubscriptionInner {
            };
            _serviceBusManagementClient.Subscriptions.CreateOrUpdateAsync(
                    _resourceGroupName,
                    _namespaceName,
                    topicPath,
                    subscriptionName,
                    subscriptionInner
                ).ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public void DeleteSubscription(string topicPath, string subscriptionName) {
            _serviceBusManagementClient.Subscriptions.DeleteAsync(
                    _resourceGroupName,
                    _namespaceName,
                    topicPath,
                    subscriptionName
                ).ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Create an instance from a connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static NamespaceManagerWrapper CreateFromConnectionString(string connectionString) {
            throw new NotImplementedException();
        }
    }
}