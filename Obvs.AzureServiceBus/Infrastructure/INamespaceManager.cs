using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

namespace Obvs.AzureServiceBus.Infrastructure {

    /// <summary>
    /// Namespace manager interface
    /// </summary>
    public interface INamespaceManager {
        /// <summary>
        /// Service bus endpoint address
        /// </summary>
        /// <returns></returns>
        Uri GetAddress();

        // NamespaceManagerSettings Settings {
        //     get;
        // }

        /// <summary>
        /// Check that a queue exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool QueueExists(string path);

        /// <summary>
        /// Create a queue
        /// </summary>
        /// <param name="path"></param>
        void CreateQueue(string path);

        /// <summary>
        /// Delete a queue
        /// </summary>
        /// <param name="path"></param>
        void DeleteQueue(string path);

        /// <summary>
        /// Check if a topic exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool TopicExists(string path);

        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="path"></param>
        void DeleteTopic(string path);

        /// <summary>
        /// Create a topi
        /// </summary>
        /// <param name="path"></param>
        void CreateTopic(string path);

        /// <summary>
        /// Check if a subscription exists for a topi
        /// </summary>
        /// <param name="topicPath"></param>
        /// <param name="subscriptionName"></param>
        /// <returns></returns>
        bool SubscriptionExists(string topicPath, string subscriptionName);

        /// <summary>
        /// Create a subscription for a topic
        /// </summary>
        /// <param name="topicPath"></param>
        /// <param name="subscriptionName"></param>
        void CreateSubscription(string topicPath, string subscriptionName);

        /// <summary>
        /// Delete a subscription for a topic
        /// </summary>
        /// <param name="topicPath"></param>
        /// <param name="subscriptionName"></param>
        void DeleteSubscription(string topicPath, string subscriptionName);
    }
}
