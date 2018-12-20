using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

using Obvs.AzureServiceBus.Configuration;

namespace Obvs.AzureServiceBus.Infrastructure {
    public interface IMessageReceiver : IDisposable {
        ReceiveMode Mode {
            get;
        }

        bool IsClosed {
            get;
        }

        Task<Message> ReceiveAsync();
    }
}
