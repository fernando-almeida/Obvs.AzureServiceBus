using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

namespace Obvs.AzureServiceBus.Infrastructure {
    public static class MessagePeekLockControlProvider {
        private static IMessagePeekLockControlProvider Instance;

        internal static IMessagePeekLockControlProvider ConfiguredInstance {
            get {
            //     if (Instance == null) {
            //         UseDefault();
            //     }

                return Instance;
            }
        }

        public static void Use(IMessagePeekLockControlProvider messagePeekLockControlProvider) {
            MessagePeekLockControlProvider.Instance = messagePeekLockControlProvider;
        }

        public static void UseDefault(IMessageSession messageSession) => Use(new DefaultMessagePeekLockControlProvider(messageSession, MessageMessageTable.ConfiguredInstance));

        public static void UseFakeMessagePeekLockControlProvider() => Use(new FakeMessagePeekLockControlProvider());
    }

    public interface IMessagePeekLockControl {
        Task AbandonAsync();
        Task CompleteAsync();
        Task RejectAsync(string reasonCode, string description);
        Task RenewLockAsync();
    }

    public interface IMessagePeekLockControlProvider {
        IMessagePeekLockControl GetMessagePeekLockControl<TMessage>(TMessage message);
    }

    internal sealed class DefaultMessagePeekLockControlProvider : IMessagePeekLockControlProvider {
        private readonly IMessageSession _messageSession;
        private IMessageMessageTable _messageMessageTable;

        public DefaultMessagePeekLockControlProvider(IMessageSession messageSession, IMessageMessageTable messageMessageTable) {
            _messageSession = messageSession;
            _messageMessageTable = messageMessageTable;
        }

        public IMessagePeekLockControl GetMessagePeekLockControl<TMessage>(TMessage message) => new DefaultMessagePeekLockControl(_messageSession, _messageMessageTable.GetMessageForObject(message));

        private sealed class DefaultMessagePeekLockControl : IMessagePeekLockControl {
            private readonly IMessageSession _messageSession;
            private Message _message;

            public DefaultMessagePeekLockControl(IMessageSession messageSession, Message message) {
                _messageSession = messageSession;
                _message = message;
            }

            public async Task AbandonAsync() => await PerformMessageActionAndDisposeAsync(async bm => await _messageSession.AbandonAsync(bm.SystemProperties.LockToken));

            public async Task CompleteAsync() => await PerformMessageActionAndDisposeAsync(async bm => await _messageSession.CompleteAsync(bm.SystemProperties.LockToken));

            public async Task RejectAsync(string reasonCode, string description) => await PerformMessageActionAndDisposeAsync(async bm => await _messageSession.DeadLetterAsync(bm.SystemProperties.LockToken));

            public Task RenewLockAsync() {
                EnsureMessageNotAlreadyProcessed();

                return _messageSession.RenewLockAsync(_message.SystemProperties.LockToken);
            }

            private async Task PerformMessageActionAndDisposeAsync(Func<Message, Task> action) {
                EnsureMessageNotAlreadyProcessed();

                await action(_message);

                _message = null;
            }

            private void EnsureMessageNotAlreadyProcessed() {
                if (_message == null) {
                    throw new InvalidOperationException("The message has already been abandoned, completed or rejected.");
                }
            }
        }
    }

    internal sealed class FakeMessagePeekLockControlProvider : IMessagePeekLockControlProvider {
        public IMessagePeekLockControl GetMessagePeekLockControl<TMessage>(TMessage message) => FakeMessagePeekLockControl.Default;

        private sealed class FakeMessagePeekLockControl : IMessagePeekLockControl {
            public static readonly FakeMessagePeekLockControl Default = new FakeMessagePeekLockControl();
            private static readonly Task CompletedPeekLockOperationTask = Task.FromResult<object>(null);

            public Task AbandonAsync() => FakeMessagePeekLockControl.CompletedPeekLockOperationTask;

            public Task CompleteAsync() => FakeMessagePeekLockControl.CompletedPeekLockOperationTask;

            public Task RejectAsync(string reasonCode, string description) => FakeMessagePeekLockControl.CompletedPeekLockOperationTask;

            public Task RenewLockAsync() => FakeMessagePeekLockControl.CompletedPeekLockOperationTask;
        }
    }

}
