﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Azure.ServiceBus;

using Moq;

using Obvs.AzureServiceBus.Configuration;
using Obvs.AzureServiceBus.Infrastructure;
using Obvs.Configuration;
using Obvs.MessageProperties;
using Obvs.Serialization;
using Obvs.Types;

using Xunit;

namespace Obvs.AzureServiceBus.Tests {
    public class ConfigurationFacts {
        private readonly Mock<IMessagingFactory> _mockMessagingFactory;
        private readonly Mock<IMessageSerializer> _mockMessageSerializer;
        private readonly Mock<IMessageDeserializerFactory> _mockMessageDeserializerFactory;
        private readonly Mock<IMessagingEntityVerifier> _mockMessagingEntityVerifier;

        public ConfigurationFacts() {
            _mockMessagingFactory = new Mock<IMessagingFactory>();

            _mockMessagingFactory.Setup(mf => mf.CreateMessageReceiver(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<ReceiveMode>()))
                .Returns(new Mock<IMessageReceiver>().Object);
            _mockMessagingFactory.Setup(mf => mf.CreateMessageSender(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns(new Mock<IMessageSender>().Object);

            _mockMessageSerializer = new Mock<IMessageSerializer>();
            _mockMessageDeserializerFactory = new Mock<IMessageDeserializerFactory>();
            _mockMessagingEntityVerifier = new Mock<IMessagingEntityVerifier>();
        }

        public class NamespaceConfigurationFacts : ConfigurationFacts {
            [Fact]
            public void ConfigureAzureServiceBusEndpointWithNullConnectionStringThrows() {
                Action action = () => ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithConnectionString(null);

                action.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void ConfigureAzureServiceBusEndpointWithNullINamespaceManagerThrows() {
                Action action = () => ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithNamespaceManager((INamespaceManager) null);

                action.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void ConfigureAzureServiceBusEndpointWithNullNamespaceManagerThrows() {
                Action action = () => ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithNamespaceManager((INamespaceManager) null);

                action.Should().Throw<ArgumentNullException>();
            }
        }

        public class MessageTypeConfigurationFacts : ConfigurationFacts {
            [Fact]
            public void ConfigureNoMessageTypesShouldThrow() {
                Action action = () => ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer()
                    .CreateServiceBus();

                action.Should().Throw<ArgumentException>()
                    .And.ParamName.Should().Be("messageTypePathMappings");
            }

            [Fact]
            public void ConfigureSameMessageTypeForSameRoleMoreThanOnceShouldThrow() {
                Action action = () => ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("commands")
                    .UsingQueueFor<ICommand>("commandsAgain");

                var exceptionAssertion = action.Should().Throw<MappingAlreadyExistsForMessageTypeException>();

                exceptionAssertion.And.MessageType.Should().Be(typeof(ICommand));
                exceptionAssertion.And.EntityType.Should().Be(MessagingEntityType.Queue);
            }

            [Fact]
            public async Task ConfigureCommandMessageTypeOnlyShouldBeAbleToSendReceiveCommands() {
                IServiceBus<IMessage, ICommand, IEvent, IRequest, IResponse> serviceBus = ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("commands")
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer()
                    .CreateServiceBus();

                serviceBus.Should().NotBeNull();

                await serviceBus.SendAsync(new TestCommand());
            }

            [Fact]
            public void SendingACommandWhenNotConfiguredAsAMessageTypeShouldThrow() {
                IServiceBus<IMessage, ICommand, IEvent, IRequest, IResponse> serviceBus = ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<IEvent>("events")
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer()
                    .CreateServiceBus();

                serviceBus.Should().NotBeNull();

                Action action = () => serviceBus.SendAsync(new TestCommand()).Wait();

                action.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public void PublishingAnEventWhenNotConfiguredAsAMessageTypeShouldThrow() {
                IServiceBus<IMessage, ICommand, IEvent, IRequest, IResponse> serviceBus = ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("commands")
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer()
                    .CreateServiceBus();

                serviceBus.Should().NotBeNull();

                Action action = () => serviceBus.PublishAsync(new TestEvent()).Wait();

                action.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public void ConfigureAllMessageTypes() {
                IServiceBus<IMessage, ICommand, IEvent, IRequest, IResponse> serviceBus = ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("commands")
                    .UsingQueueFor<IRequest>("requests")
                    .UsingQueueFor<IResponse>("responses")
                    .UsingTopicFor<IEvent>("events")
                    .UsingSubscriptionFor<IEvent>("events", "my-event-subscription")
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .FilterMessageTypeAssemblies(assembly => assembly.GetName().Name == "Obvs.AzureServiceBus.Tests")
                    .AsClientAndServer()
                    .CreateServiceBus();

                serviceBus.Should().NotBeNull();
            }
        }

        public class MessageTypeMessagingEntityVerificationFacts : ConfigurationFacts {
            [Fact]
            public void VerifyMessageTypesMappedEntit() {
                IEnumerable<MessageTypeMessagingEntityMappingDetails> configuredMessageTypePathMappings = null;

                _mockMessagingEntityVerifier.Setup(mev => mev.EnsureMessagingEntitiesExist(It.IsAny<IEnumerable<MessageTypeMessagingEntityMappingDetails>>()))
                    .Callback<IEnumerable<MessageTypeMessagingEntityMappingDetails>>(mtpm => configuredMessageTypePathMappings = mtpm);

                ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("commands", MessagingEntityCreationOptions.VerifyAlreadyExists)
                    .UsingQueueFor<IRequest>("requests", MessagingEntityCreationOptions.VerifyAlreadyExists)
                    .UsingQueueFor<IResponse>("responses", MessagingEntityCreationOptions.VerifyAlreadyExists)
                    .UsingTopicFor<IEvent>("events", MessagingEntityCreationOptions.VerifyAlreadyExists)
                    .UsingSubscriptionFor<IEvent>("events", "my-event-subscription", MessagingEntityCreationOptions.VerifyAlreadyExists)
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .FilterMessageTypeAssemblies(assembly => assembly.GetName().Name == "Obvs.AzureServiceBus.Tests")
                    .AsClientAndServer()
                    .CreateServiceBus();

                _mockMessagingEntityVerifier.Verify(mev => mev.EnsureMessagingEntitiesExist(It.IsAny<IEnumerable<MessageTypeMessagingEntityMappingDetails>>()), Times.Once());

                configuredMessageTypePathMappings.Should().NotBeNull();
                configuredMessageTypePathMappings.Should().BeEquivalentTo(new [] {
                    new MessageTypeMessagingEntityMappingDetails(typeof(ICommand), "commands", MessagingEntityType.Queue, MessagingEntityCreationOptions.VerifyAlreadyExists, ReceiveMode.PeekLock),
                        new MessageTypeMessagingEntityMappingDetails(typeof(IRequest), "requests", MessagingEntityType.Queue, MessagingEntityCreationOptions.VerifyAlreadyExists, ReceiveMode.PeekLock),
                        new MessageTypeMessagingEntityMappingDetails(typeof(IResponse), "responses", MessagingEntityType.Queue, MessagingEntityCreationOptions.VerifyAlreadyExists, ReceiveMode.PeekLock),
                        new MessageTypeMessagingEntityMappingDetails(typeof(IEvent), "events", MessagingEntityType.Topic, MessagingEntityCreationOptions.VerifyAlreadyExists, ReceiveMode.PeekLock),
                        new MessageTypeMessagingEntityMappingDetails(typeof(IEvent), "events/subscriptions/my-event-subscription", MessagingEntityType.Subscription, MessagingEntityCreationOptions.VerifyAlreadyExists, ReceiveMode.PeekLock),
                });
            }

            public class MessagingEntityVerifierConfigurationFacts {
                [Fact]
                public void ConfigureNullMessagingEntityVerifierShouldThrow() {
                    Action action = () => ServiceBus.Configure()
                        .WithAzureServiceBusEndpoint<TestMessage>()
                        .Named("Test Service Bus")
                        .WithMessagingEntityVerifier(null);

                    action.Should().Throw<ArgumentNullException>()
                        .And.ParamName.Should().Be("messagingEntityVerifier");
                }
            }
        }

        public class PropertyProviderConfigurationFacts : ConfigurationFacts {
            private class NotATestMessage : IMessage {

            }

            [Fact]
            public void ConfiguringANullProviderInstanceThrows() {
                Action action = () => ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("test")
                    .UsingMessagePropertyProviderFor<TestCommand>(null)
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer();

                action.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void ConfiguringAProviderForAMessageThatIsNotDerivedFromTheServiceMessageTypeThrows() {
                Action action = () => ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("test")
                    .UsingMessagePropertyProviderFor<NotATestMessage>(new FuncMessagePropertyProvider<NotATestMessage>(c => null))
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer();

                action.Should().Throw<ArgumentException>()
                    .And.ParamName.Should().Be("messagePropertyProvider");
            }

            [Fact]
            public void ConfigureProvidersForMultipleMessageTypes() {
                Action action = () => ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("test")
                    .UsingMessagePropertyProviderFor<TestCommand>(new FuncMessagePropertyProvider<TestCommand>(tc => new KeyValuePair<string, object>("CommandProp", "CommandPropValue")))
                    .UsingMessagePropertyProviderFor<TestEvent>(new FuncMessagePropertyProvider<TestEvent>(tc => new KeyValuePair<string, object>("EventProp", "EventPropValue")))
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer();
            }

            [Fact]
            public void ConfigureMultipleProvidersForSameMessageType() {
                Action action = () => ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("test")
                    .UsingMessagePropertyProviderFor<TestCommand>(new FuncMessagePropertyProvider<TestCommand>(tc => new KeyValuePair<string, object>("CommandProp1", "CommandPropValue1")))
                    .UsingMessagePropertyProviderFor<TestCommand>(new FuncMessagePropertyProvider<TestCommand>(tc => new KeyValuePair<string, object>("CommandProp2", "CommandPropValue2")))
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer();
            }

            [Fact]
            public async Task EnsureProvidedPropertiesArePresent() {
                Message Message = null;

                Mock<IMessageSender> mockMessageSender = new Mock<IMessageSender>();
                mockMessageSender.Setup(ms => ms.SendAsync(It.IsAny<Message>()))
                    .Callback<Message>(bm => Message = bm)
                    .Returns(Task.FromResult(true));

                _mockMessagingFactory.Setup(mf => mf.CreateMessageSender(It.IsAny<Type>(), It.IsAny<string>()))
                    .Returns(mockMessageSender.Object);

                CompositeMessagePropertyProvider<TestCommand> propertyProvider = new CompositeMessagePropertyProvider<TestCommand>();
                propertyProvider.Providers.AddRange(
                    c => new KeyValuePair<string, object>("SomeProp", "SomeValue"),
                    c => new KeyValuePair<string, object>("SomeOtherProp", "SomeOtherValue"));

                FuncMessagePropertyProvider<TestCommand> propertyProvider2 = new FuncMessagePropertyProvider<TestCommand>(c => new KeyValuePair<string, object>("SomeThirdProp", "SomeThirdValue"));

                IServiceBus serviceBus = ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .WithMessagingEntityVerifier(_mockMessagingEntityVerifier.Object)
                    .UsingQueueFor<ICommand>("test")
                    .UsingMessagePropertyProviderFor<TestCommand>(propertyProvider)
                    .UsingMessagePropertyProviderFor<TestCommand>(propertyProvider2)
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer()
                    .Create();

                await serviceBus.SendAsync(new TestCommand());

                Message.Should().NotBeNull();

                Message.UserProperties.Should()
                    .Contain(new KeyValuePair<string, object>("SomeProp", "SomeValue"))
                    .And.Contain(new KeyValuePair<string, object>("SomeOtherProp", "SomeOtherValue"))
                    .And.Contain(new KeyValuePair<string, object>("SomeThirdProp", "SomeThirdValue"));
            }
        }

        public class EndpointConfigurationFacts : ConfigurationFacts {
            [Fact]
            public void DefaultMessageReceiveModeIsReceiveAndDelete() {
                Mock<IMessageReceiver> mockMessageReceiver = new Mock<IMessageReceiver>();
                mockMessageReceiver.Setup(mmr => mmr.IsClosed)
                    .Returns(true);

                _mockMessagingFactory.Setup(mf => mf.CreateMessageReceiver(typeof(TestCommand), "test", It.IsAny<ReceiveMode>()))
                    .Returns(mockMessageReceiver.Object);

                IServiceBus serviceBus = ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .UsingQueueFor<TestCommand>("test")
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer()
                    .Create();

                serviceBus.Commands.Subscribe(c => c.ToString());

                _mockMessagingFactory.Verify(mf => mf.CreateMessageReceiver(typeof(TestCommand), "test", ReceiveMode.ReceiveAndDelete));
            }

            [Fact]
            public void ExplicitMessageReceiveModeIsSetCorrectly_ReceiveAndDelete() {
                Mock<IMessageReceiver> mockMessageReceiver = new Mock<IMessageReceiver>();
                mockMessageReceiver.Setup(mmr => mmr.IsClosed)
                    .Returns(true);

                _mockMessagingFactory.Setup(mf => mf.CreateMessageReceiver(typeof(TestCommand), "test", It.IsAny<ReceiveMode>()))
                    .Returns(mockMessageReceiver.Object);

                IServiceBus serviceBus = ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .UsingQueueFor<TestCommand>("test", ReceiveMode.ReceiveAndDelete)
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer()
                    .Create();

                serviceBus.Commands.Subscribe(c => c.ToString());

                _mockMessagingFactory.Verify(mf => mf.CreateMessageReceiver(typeof(TestCommand), "test", ReceiveMode.ReceiveAndDelete));
            }

            [Fact]
            public void ExplicitMessageReceiveModeIsSetCorrectly_PeekLock() {
                Mock<IMessageReceiver> mockMessageReceiver = new Mock<IMessageReceiver>();
                mockMessageReceiver.Setup(mmr => mmr.IsClosed)
                    .Returns(true);

                _mockMessagingFactory.Setup(mf => mf.CreateMessageReceiver(typeof(TestCommand), "test", It.IsAny<ReceiveMode>()))
                    .Returns(mockMessageReceiver.Object);

                IServiceBus serviceBus = ServiceBus.Configure()
                    .WithAzureServiceBusEndpoint<TestMessage>()
                    .Named("Test Service Bus")
                    .WithMessagingFactory(_mockMessagingFactory.Object)
                    .UsingQueueFor<TestCommand>("test", ReceiveMode.PeekLock)
                    .SerializedWith(_mockMessageSerializer.Object, _mockMessageDeserializerFactory.Object)
                    .AsClientAndServer()
                    .Create();

                serviceBus.Commands.Subscribe(c => c.ToString());

                _mockMessagingFactory.Verify(mf => mf.CreateMessageReceiver(typeof(TestCommand), "test", ReceiveMode.PeekLock));
            }
        }

        public class TestMessage : IMessage { }

        public class TestEvent : TestMessage, IEvent {

        }

        public class TestCommand : TestMessage, ICommand {
            public string CommandId {
                get;
                set;
            }
        }

        public class TestRequest : TestMessage, IRequest {
            public string RequestId {
                get;
                set;
            }

            public string RequesterId {
                get;
                set;
            }
        }

        public class TestResponse : TestMessage, IResponse {
            public string RequestId {
                get;
                set;
            }

            public string RequesterId {
                get;
                set;
            }
        }
    }
}
