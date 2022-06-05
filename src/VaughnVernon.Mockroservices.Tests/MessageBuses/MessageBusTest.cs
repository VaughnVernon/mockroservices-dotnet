//   Copyright © 2020 Vaughn Vernon. All rights reserved.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VaughnVernon.Mockroservices.MessageBuses;

namespace VaughnVernon.Mockroservices.Tests.MessageBuses
{
    [TestClass]
    public class MessageBusTest
    {
        [TestMethod]
        public void TestMessageBusStart()
        {
            var messageBus = MessageBus.Start("test_bus");
            Assert.IsNotNull(messageBus);
        }

        [TestMethod]
        public void TestTopicOpen()
        {
            var messageBus = MessageBus.Start("test_bus");
            Assert.IsNotNull(messageBus);
            var topic = messageBus.OpenTopic("test_topic");
            Assert.IsNotNull(topic);
            var topicAgain = messageBus.OpenTopic("test_topic");
            Assert.AreSame(topic, topicAgain);
            topic.Close();
        }

        [TestMethod]
        public void TestTopicPubSub()
        {
            var messageBus = MessageBus.Start("test_bus2");
            var topic = messageBus.OpenTopic("test_topic2");
            var subscriber = new MessageBusTestSubscriber();
            topic.Subscribe(subscriber);
            topic.Publish(new Message("1", "type1", "test1"));
            topic.Publish(new Message("2", "type2", "test2"));
            topic.Publish(new Message("3", "type3", "test3"));

            topic.Close();

            Assert.AreEqual(3, subscriber.HandledMessages.Count);
            Assert.AreEqual("1", subscriber.HandledMessages[0].Id);
            Assert.AreEqual("type1", subscriber.HandledMessages[0].Type);
            Assert.AreEqual("test1", subscriber.HandledMessages[0].Payload);
            Assert.AreEqual("2", subscriber.HandledMessages[1].Id);
            Assert.AreEqual("type2", subscriber.HandledMessages[1].Type);
            Assert.AreEqual("test2", subscriber.HandledMessages[1].Payload);
            Assert.AreEqual("3", subscriber.HandledMessages[2].Id);
            Assert.AreEqual("type3", subscriber.HandledMessages[2].Type);
            Assert.AreEqual("test3", subscriber.HandledMessages[2].Payload);
        }
    }

    internal class MessageBusTestSubscriber : ISubscriber
    {
        internal readonly List<Message> HandledMessages = new List<Message>();

        public void Handle(Message message) => HandledMessages.Add(message);
    }
}
