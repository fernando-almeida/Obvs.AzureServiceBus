using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
    public class MessageSourceFacts {
        public class ConstructorFacts {
            [Fact]
            public void CreatingWithNullMessageReceiverThrows() {
                Action action = () => {
                    new MessageSource<TestMessage>((IMessagingEntityFactory) null, new [] {
                        Mock.Of<IMessageDeserializer<TestMessage>>()
                    }, Mock.Of<IMessageMessageTable>());
                };

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("messagingEntityFactory");
            }

            [Fact]
            public void CreatingWithNullMessageObservableThrows() {
                Action action = () => {
                    new MessageSource<TestMessage>((IObservable<Message>) null, new [] { Mock.Of<IMessageDeserializer<TestMessage>>() }, Mock.Of<IMessageMessageTable>());
                };

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("Messages");
            }

            [Fact]
            public void CreatingWithNullDeserializersThrows() {
                Action action = () => {
                    new MessageSource<TestMessage>(Mock.Of<IObservable<Message>>(), null, Mock.Of<IMessageMessageTable>());
                };

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("deserializers");
            }

            [Fact]
            public void CreatingWithNullMessageMessageTableThrows() {
                Action action = () => {
                    new MessageSource<TestMessage>(Mock.Of<IObservable<Message>>(), new [] { Mock.Of<IMessageDeserializer<TestMessage>>() }, null);
                };

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("messageMessageTable");
            }
        }

        public class MessageProcessingFacts {

            [Fact]
            public void ReceivesAndDeserializesSingleMessage() {
                Mock<IMessageDeserializer<TestMessage>> mockTestMessageDeserializer = new Mock<IMessageDeserializer<TestMessage>>();
                mockTestMessageDeserializer.Setup(md => md.GetTypeName())
                    .Returns(typeof(TestMessage).Name);

                TestMessage testMessage = new TestMessage();

                mockTestMessageDeserializer.Setup(md => md.Deserialize(It.IsAny<Stream>()))
                    .Returns(testMessage);

                Message message = new Message() {
                    UserProperties = {
                        { MessagePropertyNames.TypeName, typeof(TestMessage).Name }
                    }
                };

                IObservable<TestMessage> Messages = Observable.Create<TestMessage>(o => {
                    o.OnNext(testMessage);

                    o.OnCompleted();

                    return Disposable.Empty;
                });

                throw new NotImplementedException("ERROR in line below");
                // MessageSource<TestMessage> messageSource = new MessageSource<TestMessage>(
                //     Messages,
                //     new [] {
                //         mockTestMessageDeserializer.Object
                //     },
                //     Mock.Of<IMessageMessageTable>());

                // TestMessage receiveMessage = await messageSource.Messages.SingleOrDefaultAsync();

                // message.Should().BeEquivalentTo(receiveMessage);

                // // NOTE: Would be great to be able to verify that testMessage.CompleteAsync() was called here, but I would have to build abstraction around Message for that because it can't be mocked (since it's sealed)

                // mockTestMessageDeserializer.Verify(md => md.Deserialize(It.IsAny<Stream>()), Times.Once());
            }

            [Fact]
            public async Task ReceivesAndDeserializesMultipleMessagesInCorrectOrder() {
                Mock<IMessageDeserializer<TestMessage>> mockTestMessageDeserializer = new Mock<IMessageDeserializer<TestMessage>>();
                mockTestMessageDeserializer.Setup(md => md.GetTypeName())
                    .Returns(typeof(TestMessage).Name);

                const int NumberOfMessagesToGenerate = 5;
                int messageCounter = 0;

                mockTestMessageDeserializer.Setup(md => md.Deserialize(It.IsAny<Stream>()))
                    .Returns(() => new TestMessage {
                        TestId = messageCounter++
                    });

                IObservable<Message> Messages = Observable.Create<Message>(o => {
                    for (int messageIndex = 0; messageIndex < NumberOfMessagesToGenerate; messageIndex++) {
                        o.OnNext(new Message {
                            UserProperties = { { MessagePropertyNames.TypeName, typeof(TestMessage).Name }
                            }
                        });
                    }

                    o.OnCompleted();

                    return Disposable.Empty;
                });

                MessageSource<TestMessage> messageSource = new MessageSource<TestMessage>(Messages, new [] { mockTestMessageDeserializer.Object }, Mock.Of<IMessageMessageTable>());

                IList<TestMessage> messages = await messageSource.Messages.ToList();

                messages.Count.Should().Be(NumberOfMessagesToGenerate);

                for (int messageIndex = 0; messageIndex < NumberOfMessagesToGenerate; messageIndex++) {
                    messages[messageIndex].TestId.Should().Be(messageIndex);
                }
            }

            [Fact]
            public async Task OnlyDeliversMessagesOfTheCorrectType() {
                Mock<IMessageDeserializer<TestMessage>> mockTestMessageDeserializer = new Mock<IMessageDeserializer<TestMessage>>();
                mockTestMessageDeserializer.Setup(md => md.GetTypeName())
                    .Returns(typeof(TestMessage).Name);

                TestMessage testMessage = new TestMessage();

                mockTestMessageDeserializer.Setup(md => md.Deserialize(It.IsAny<Stream>()))
                    .Returns(testMessage);

                Message MessageThatShouldBeIgnored = new Message() {
                    UserProperties = { { MessagePropertyNames.TypeName, "SomeMessageTypeThatIDontWant" }
                    }
                };

                Message MessageThatShouldBeReceived = new Message() {
                    UserProperties = { { MessagePropertyNames.TypeName, typeof(TestMessage).Name }
                    }
                };

                IObservable<Message> Messages = Observable.Create<Message>(o => {
                    o.OnNext(MessageThatShouldBeIgnored);

                    o.OnNext(MessageThatShouldBeReceived);

                    o.OnCompleted();

                    return Disposable.Empty;
                });

                MessageSource<TestMessage> messageSource = new MessageSource<TestMessage>(Messages, new [] { mockTestMessageDeserializer.Object }, Mock.Of<IMessageMessageTable>());

                TestMessage message = await messageSource.Messages.SingleOrDefaultAsync();

                message.Should().NotBeNull();

                // NOTE: Would be great to be able to verify that testMessage.CompleteAsync() wasn't called here, but I would have to build abstraction around Message for that because it can't be mocked (since it's sealed)

                mockTestMessageDeserializer.Verify(md => md.Deserialize(It.IsAny<Stream>()), Times.Once());
            }
        }

        public class PeekLockMessageFacts {
            [Fact]
            public async Task CompletingPeekLockMessageCompletesTheAssociatedMessage() {
                Mock<IMessageDeserializer<TestPeekLockMessage>> mockTestPeekLockMessageDeserializer = new Mock<IMessageDeserializer<TestPeekLockMessage>>();
                mockTestPeekLockMessageDeserializer.Setup(md => md.GetTypeName())
                    .Returns(typeof(TestPeekLockMessage).Name);

                TestPeekLockMessage testPeekLockMessage = new TestPeekLockMessage();

                mockTestPeekLockMessageDeserializer.Setup(md => md.Deserialize(It.IsAny<Stream>()))
                    .Returns(testPeekLockMessage);

                IObservable<Message> Messages = Observable.Create<Message>(o => {
                    o.OnNext(new Message {
                        UserProperties = { { MessagePropertyNames.TypeName, typeof(TestPeekLockMessage).Name }
                        }
                    });

                    o.OnCompleted();

                    return Disposable.Empty;
                });

                Mock<IMessagePeekLockControl> mockMessagePeekLockControl = new Mock<IMessagePeekLockControl>();

                Mock<IMessagePeekLockControlProvider> mockPeekLockControlProvider = new Mock<IMessagePeekLockControlProvider>();

                mockPeekLockControlProvider.Setup(bmplcp => bmplcp.GetMessagePeekLockControl(testPeekLockMessage))
                    .Returns(mockMessagePeekLockControl.Object);

                MessagePeekLockControlProvider.Use(mockPeekLockControlProvider.Object);

                MessageSource<TestPeekLockMessage> messageSource = new MessageSource<TestPeekLockMessage>(Messages, new [] { mockTestPeekLockMessageDeserializer.Object }, Mock.Of<IMessageMessageTable>());

                TestPeekLockMessage message = await messageSource.Messages.SingleOrDefaultAsync();

                IMessagePeekLockControl messagePeekLockControl = message.GetPeekLockControl();

                messagePeekLockControl.Should().NotBeNull();

                await messagePeekLockControl.CompleteAsync();

                mockMessagePeekLockControl.Verify(bmplc => bmplc.CompleteAsync(), Times.Once());
            }
        }

        public class TestMessage : IMessage {
            public int TestId {
                get;
                set;
            }
        }

        public class TestPeekLockMessage {
            public int TestId {
                get;
                set;
            }
        }
    }
}
