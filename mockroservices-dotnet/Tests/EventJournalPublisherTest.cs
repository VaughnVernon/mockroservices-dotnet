//   Copyright © 2017 Vaughn Vernon. All rights reserved.
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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;

namespace VaughnVernon.Mockroservices
{
    [TestClass]
    public class EventJournalPublisherTest
    {
        [TestMethod]
        public void TestEventJournalPublisher()
        {
            EventJournal eventJournal = EventJournal.Open("test-ej");
            MessageBus messageBus = MessageBus.Start("test-bus");
            Topic topic = messageBus.OpenTopic("test-topic");
            EventJournalPublisher journalPublisher =
                EventJournalPublisher.From(eventJournal.Name, messageBus.Name, topic.Name);

            EventJournalPublisherTestSubscriber subscriber = new EventJournalPublisherTestSubscriber();
            topic.Subscribe(subscriber);

            for (int idx = 0; idx < 3; ++idx)
            {
                eventJournal.Write("test1", idx, "test1type", "test1instance" + idx);
            }

            for (int idx = 0; idx < 3; ++idx)
            {
                eventJournal.Write("test2", idx, "test2type", "test2instance" + idx);
            }

            subscriber.WaitForExpectedMessages(6);

            topic.Close();

            journalPublisher.Close();

            Assert.AreEqual(6, subscriber.handledMessages.Count);
        }
    }

    internal class EventJournalPublisherTestSubscriber : ISubscriber
    {
        internal List<Message> handledMessages = new List<Message>();

        public void Handle(Message message)
        {
            handledMessages.Add(message);
        }

        public void WaitForExpectedMessages(int count)
        {
            for (int idx = 0; idx < 100; ++idx)
            {
                if (handledMessages.Count == count)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
