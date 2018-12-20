﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Azure.ServiceBus;

using Moq;

using Obvs.AzureServiceBus.Infrastructure;
using Obvs.MessageProperties;
using Obvs.Serialization;
using Obvs.Types;

using Xunit;

namespace Obvs.AzureServiceBus.Tests {
    public class MessagePublisherFacts {
        public class ConstructorFacts {
            [Fact]
            public void CreatingWithNullIMessageClientEntityFactorySenderThrows() {
                Action action = () => {
                    new MessagePublisher<TestMessage>((IMessagingEntityFactory) null, Mock.Of<IMessageSerializer>(), Mock.Of<IMessagePropertyProvider<TestMessage>>(), Mock.Of<IMessageOutgoingPropertiesTable>());
                };

                action.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "messagingEntityFactory");
            }

            [Fact]
            public void CreatingWithNullSerializerThrows() {
                Action action = () => {
                    new MessagePublisher<TestMessage>(Mock.Of<IMessagingEntityFactory>(), null, Mock.Of<IMessagePropertyProvider<TestMessage>>(), Mock.Of<IMessageOutgoingPropertiesTable>());
                };

                action.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "serializer");
            }

            [Fact]
            public void CreatingWithNullPropertyProviderThrows() {
                Action action = () => {
                    new MessagePublisher<TestMessage>(Mock.Of<IMessagingEntityFactory>(), Mock.Of<IMessageSerializer>(), null, Mock.Of<IMessageOutgoingPropertiesTable>());
                };

                action.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "propertyProvider");
            }

            [Fact]
            public void CreatingWithNullMessageOutgoingPropertiesTableThrows() {
                Action action = () => {
                    new MessagePublisher<TestMessage>(Mock.Of<IMessagingEntityFactory>(), Mock.Of<IMessageSerializer>(), Mock.Of<IMessagePropertyProvider<TestMessage>>(), null);
                };

                action.Should().Throw<ArgumentNullException>().Where(e => e.ParamName == "messageOutgoingPropertiesTable");
            }
        }

        public class MessagePublishingFacts {
            [Fact]
            public void SerializesMessage() {
                Mock<IMessageSerializer> mockMessageSerializer = new Mock<IMessageSerializer>();

                MessagePublisher<TestMessage> messagePublisher = new MessagePublisher<TestMessage>(Mock.Of<IMessagingEntityFactory>(), mockMessageSerializer.Object, Mock.Of<IMessagePropertyProvider<TestMessage>>(), Mock.Of<IMessageOutgoingPropertiesTable>());

                TestMessage message = new TestMessage();

                messagePublisher.PublishAsync(message);

                mockMessageSerializer.Verify(ms => ms.Serialize(It.IsAny<Stream>(), It.Is<TestMessage>(it => Object.ReferenceEquals(it, message))), Times.Once());
            }

            [Fact]
            public async Task GetsMessagePropertiesFromPropertyProviderAndAppliesThemToTheMessage() {
                Mock<IMessageSender> mockMessageSender = new Mock<IMessageSender>();
                Message MessageSent = null;

                mockMessageSender.Setup(ms => ms.SendAsync(It.IsAny<Message>()))
                    .Callback<Message>(bm => {
                        MessageSent = bm;
                    })
                    .Returns(Task.FromResult<object>(null));

                Mock<IMessagingEntityFactory> mockMessageClientEntityFactory = new Mock<IMessagingEntityFactory>();
                mockMessageClientEntityFactory.Setup(mcef => mcef.CreateMessageSender(It.IsAny<Type>()))
                    .Returns(mockMessageSender.Object);

                Mock<IMessageSerializer> mockMessageSerializer = new Mock<IMessageSerializer>();

                Mock<IMessagePropertyProvider<TestMessage>> mockMessagePropertyProvider = new Mock<IMessagePropertyProvider<TestMessage>>();

                KeyValuePair<string, object>[] properties = new [] {
                    new KeyValuePair<string, object>("Prop1", 1),
                    new KeyValuePair<string, object>("Prop2", "two"),
                };

                mockMessagePropertyProvider.Setup(mpp => mpp.GetProperties(It.IsAny<TestMessage>()))
                    .Returns(properties);

                MessagePublisher<TestMessage> messagePublisher = new MessagePublisher<TestMessage>(mockMessageClientEntityFactory.Object, mockMessageSerializer.Object, mockMessagePropertyProvider.Object, Mock.Of<IMessageOutgoingPropertiesTable>());

                TestMessage message = new TestMessage();

                await messagePublisher.PublishAsync(message);

                mockMessagePropertyProvider.Verify(mpp => mpp.GetProperties(It.Is<TestMessage>(it => Object.ReferenceEquals(it, message))), Times.Once());

                MessageSent.Should().NotBeNull();

                // TODO: should be able to do this cleaner
                MessageSent.UserProperties.Should().ContainKeys(properties.Select(kvp => kvp.Key), "Should have translated all properties provided by the property provider to the Message.");
                MessageSent.UserProperties.Should().ContainValues(properties.Select(kvp => kvp.Value), "Should have translated all properties provided by the property provider to the Message.");
            }

            [Fact]
            public async Task AppliesMessageTypeNamePropertyToTheMessage() {
                Mock<IMessageSender> mockMessageSender = new Mock<IMessageSender>();
                Message MessageSent = null;

                mockMessageSender.Setup(ms => ms.SendAsync(It.IsAny<Message>()))
                    .Callback<Message>(bm => {
                        MessageSent = bm;
                    })
                    .Returns(Task.FromResult<object>(null));

                Mock<IMessagingEntityFactory> mockMessageClientEntityFactory = new Mock<IMessagingEntityFactory>();
                mockMessageClientEntityFactory.Setup(mcef => mcef.CreateMessageSender(It.IsAny<Type>()))
                    .Returns(mockMessageSender.Object);

                Mock<IMessageSerializer> mockMessageSerializer = new Mock<IMessageSerializer>();

                Mock<IMessagePropertyProvider<TestMessage>> mockMessagePropertyProvider = new Mock<IMessagePropertyProvider<TestMessage>>();

                MessagePublisher<TestMessage> messagePublisher = new MessagePublisher<TestMessage>(mockMessageClientEntityFactory.Object, mockMessageSerializer.Object, mockMessagePropertyProvider.Object, Mock.Of<IMessageOutgoingPropertiesTable>());

                TestMessage message = new TestMessage();

                await messagePublisher.PublishAsync(message);

                MessageSent.Should().NotBeNull();

                MessageSent.UserProperties.Should().Contain(new KeyValuePair<string, object>(MessagePropertyNames.TypeName, typeof(TestMessage).Name), "Should have applied the mesage type name property to the Message.");
            }

            [Fact]
            public async Task SendsMessage() {
                Mock<IMessageSender> mockMessageSender = new Mock<IMessageSender>();
                Mock<IMessagingEntityFactory> mockMessageClientEntityFactory = new Mock<IMessagingEntityFactory>();
                mockMessageClientEntityFactory.Setup(mcef => mcef.CreateMessageSender(It.IsAny<Type>()))
                    .Returns(mockMessageSender.Object);

                MessagePublisher<TestMessage> messagePublisher = new MessagePublisher<TestMessage>(mockMessageClientEntityFactory.Object, Mock.Of<IMessageSerializer>(), Mock.Of<IMessagePropertyProvider<TestMessage>>(), Mock.Of<IMessageOutgoingPropertiesTable>());

                TestMessage message = new TestMessage();

                await messagePublisher.PublishAsync(message);

                mockMessageSender.Verify(ms => ms.SendAsync(It.IsAny<Message>()), Times.Once());
            }

            [Fact]
            public async Task HandlesNoOutgoingPropertiesForMessage() {
                Mock<IMessageSender> mockMessageSender = new Mock<IMessageSender>();
                Mock<IMessagingEntityFactory> mockMessageClientEntityFactory = new Mock<IMessagingEntityFactory>();
                mockMessageClientEntityFactory.Setup(mcef => mcef.CreateMessageSender(It.IsAny<Type>()))
                    .Returns(mockMessageSender.Object);

                Mock<IMessageOutgoingPropertiesTable> mockMessageOutgoingPropertiesTable = new Mock<IMessageOutgoingPropertiesTable>();

                TestMessage testMessage = new TestMessage();
                MessagePublisher<TestMessage> messagePublisher = new MessagePublisher<TestMessage>(mockMessageClientEntityFactory.Object, Mock.Of<IMessageSerializer>(), Mock.Of<IMessagePropertyProvider<TestMessage>>(), mockMessageOutgoingPropertiesTable.Object);

                await messagePublisher.PublishAsync(testMessage);

                mockMessageOutgoingPropertiesTable.Verify(mopt => mopt.GetOutgoingPropertiesForMessage(testMessage), Times.Once());
            }

            [Fact]
            public async Task AppliesOutgoingPropertiesToMessage() {
                Mock<IMessageSender> mockMessageSender = new Mock<IMessageSender>();
                Mock<IMessagingEntityFactory> mockMessageClientEntityFactory = new Mock<IMessagingEntityFactory>();
                mockMessageClientEntityFactory.Setup(mcef => mcef.CreateMessageSender(It.IsAny<Type>()))
                    .Returns(mockMessageSender.Object);

                Message sentMessage = null;

                mockMessageSender.Setup(ms => ms.SendAsync(It.IsAny<Message>()))
                    .Callback<Message>(bm => sentMessage = bm)
                    .Returns(Task.FromResult<object>(null));

                TestMessage testMessage = new TestMessage();

                DateTime testScheduledEnqueueTime = new DateTime(2577, 1, 31);
                TimeSpan testTimeToLive = TimeSpan.FromMinutes(13177);

                Mock<IOutgoingMessageProperties> mockOutgoingMessageProperties = new Mock<IOutgoingMessageProperties>();
                mockOutgoingMessageProperties.Setup(omp => omp.ScheduledEnqueueTimeUtc)
                    .Returns(testScheduledEnqueueTime);

                mockOutgoingMessageProperties.Setup(omp => omp.TimeToLive)
                    .Returns(testTimeToLive);

                Mock<IMessageOutgoingPropertiesTable> mockMessageOutgoingPropertiesTable = new Mock<IMessageOutgoingPropertiesTable>();
                mockMessageOutgoingPropertiesTable.Setup(mopt => mopt.GetOutgoingPropertiesForMessage(testMessage))
                    .Returns(mockOutgoingMessageProperties.Object);

                MessagePublisher<TestMessage> messagePublisher = new MessagePublisher<TestMessage>(mockMessageClientEntityFactory.Object, Mock.Of<IMessageSerializer>(), Mock.Of<IMessagePropertyProvider<TestMessage>>(), mockMessageOutgoingPropertiesTable.Object);

                await messagePublisher.PublishAsync(testMessage);

                mockMessageOutgoingPropertiesTable.Verify(mopt => mopt.GetOutgoingPropertiesForMessage(testMessage), Times.Once());
                mockOutgoingMessageProperties.Verify(omp => omp.ScheduledEnqueueTimeUtc, Times.Once());
                mockOutgoingMessageProperties.Verify(omp => omp.TimeToLive, Times.Once());

                sentMessage.ScheduledEnqueueTimeUtc.Should().Be(testScheduledEnqueueTime);
                sentMessage.TimeToLive.Should().Be(testTimeToLive);
            }
        }

        public class RequestResponseFacts {
            [Fact]
            public async Task RequestWithNoRequestorIdOrRequestIdEndsUpWithNoCorrelationData() {
                Mock<IMessageSender> mockMessageSender = new Mock<IMessageSender>();

                Message MessageSent = null;

                mockMessageSender.Setup(ms => ms.SendAsync(It.IsAny<Message>()))
                    .Callback<Message>(bm => {
                        MessageSent = bm;
                    })
                    .Returns(Task.FromResult<object>(null));

                Mock<IMessagingEntityFactory> mockMessageClientEntityFactory = new Mock<IMessagingEntityFactory>();
                mockMessageClientEntityFactory.Setup(mcef => mcef.CreateMessageSender(It.IsAny<Type>()))
                    .Returns(mockMessageSender.Object);

                MessagePublisher<TestMessage> messagePublisher = new MessagePublisher<TestMessage>(mockMessageClientEntityFactory.Object, Mock.Of<IMessageSerializer>(), Mock.Of<IMessagePropertyProvider<TestMessage>>(), Mock.Of<IMessageOutgoingPropertiesTable>());

                TestRequest request = new TestRequest();

                await messagePublisher.PublishAsync(request);

                mockMessageSender.Verify(ms => ms.SendAsync(It.IsAny<Message>()), Times.Once());

                MessageSent.Should().NotBeNull();
                MessageSent.CorrelationId.Should().BeNull();
                MessageSent.ReplyToSessionId.Should().BeNull();
            }

            [Fact]
            public async Task RequestWithRequestIdEndsUpWithCorrelationId() {
                Mock<IMessageSender> mockMessageSender = new Mock<IMessageSender>();

                Message MessageSent = null;

                mockMessageSender.Setup(ms => ms.SendAsync(It.IsAny<Message>()))
                    .Callback<Message>(bm => {
                        MessageSent = bm;
                    })
                    .Returns(Task.FromResult<object>(null));

                Mock<IMessagingEntityFactory> mockMessageClientEntityFactory = new Mock<IMessagingEntityFactory>();
                mockMessageClientEntityFactory.Setup(mcef => mcef.CreateMessageSender(It.IsAny<Type>()))
                    .Returns(mockMessageSender.Object);

                MessagePublisher<TestMessage> messagePublisher = new MessagePublisher<TestMessage>(mockMessageClientEntityFactory.Object, Mock.Of<IMessageSerializer>(), Mock.Of<IMessagePropertyProvider<TestMessage>>(), Mock.Of<IMessageOutgoingPropertiesTable>());

                TestRequest request = new TestRequest {
                    RequestId = "TestRequestId"
                };

                await messagePublisher.PublishAsync(request);

                mockMessageSender.Verify(ms => ms.SendAsync(It.IsAny<Message>()), Times.Once());

                MessageSent.Should().NotBeNull();
                MessageSent.CorrelationId.Should().Be(request.RequestId);
            }

            [Fact]
            public async Task RequestRequesterIdEndsUpAsReplyToSessionId() {
                Mock<IMessageSender> mockMessageSender = new Mock<IMessageSender>();

                Message MessageSent = null;

                mockMessageSender.Setup(ms => ms.SendAsync(It.IsAny<Message>()))
                    .Callback<Message>(bm => {
                        MessageSent = bm;
                    })
                    .Returns(Task.FromResult<object>(null));

                Mock<IMessagingEntityFactory> mockMessageClientEntityFactory = new Mock<IMessagingEntityFactory>();
                mockMessageClientEntityFactory.Setup(mcef => mcef.CreateMessageSender(It.IsAny<Type>()))
                    .Returns(mockMessageSender.Object);

                MessagePublisher<TestMessage> messagePublisher = new MessagePublisher<TestMessage>(mockMessageClientEntityFactory.Object, Mock.Of<IMessageSerializer>(), Mock.Of<IMessagePropertyProvider<TestMessage>>(), Mock.Of<IMessageOutgoingPropertiesTable>());

                TestRequest request = new TestRequest {
                    RequesterId = "TestRequesterId"
                };

                await messagePublisher.PublishAsync(request);

                mockMessageSender.Verify(ms => ms.SendAsync(It.IsAny<Message>()), Times.Once());

                MessageSent.Should().NotBeNull();
                MessageSent.ReplyToSessionId.Should().Be(request.RequesterId);
            }
        }

        public class TestMessage : IMessage {

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
    }
}
