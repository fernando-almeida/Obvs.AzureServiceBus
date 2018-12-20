using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

namespace Obvs.AzureServiceBus.Infrastructure {

    /// <summary>
    /// Message sender
    /// </summary>
    public interface IMessageSender : IDisposable {
        Type SupportedMessageType { get; }

        Task SendAsync(Message Message);
    }
}
