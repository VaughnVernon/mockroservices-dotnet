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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using VaughnVernon.Mockroservices.Journals;

namespace VaughnVernon.Mockroservices.Tests
{
    [TestClass]
    public class EventJournalPublisherTest
    {
        [TestMethod]
        public void TestEventJournalPublisher()
        {
            var eventJournal = Journal.Open("test-ej");
            var messageBus = MessageBus.Start("test-bus");
            var topic1 = messageBus.OpenTopic("test1");
            var topic2 = messageBus.OpenTopic("test2");
            var journalPublisher1 = JournalPublisher.From(eventJournal.Name, messageBus.Name, topic1.Name);
            var journalPublisher2 = JournalPublisher.From(eventJournal.Name, messageBus.Name, topic2.Name);

            var subscriber1 = new JournalPublisherTestSubscriber();
            topic1.Subscribe(subscriber1);

            var batch1 = new EntryBatch();
			for (var idx = 0; idx < 3; ++idx)
            {
				batch1.AddEntry("test1type", $"test1instance{idx}");
            }
			eventJournal.Write("test1", EntryValue.NoStreamVersion, batch1);
            
            subscriber1.WaitForExpectedMessages(3);
            topic1.Close();
            journalPublisher1.Close();

            Assert.AreEqual(3, subscriber1.HandledMessages.Count);

            var subscriber2 = new JournalPublisherTestSubscriber();
            topic2.Subscribe(subscriber2);
            
			var batch2 = new EntryBatch();
			for (var idx = 0; idx < 3; ++idx)
            {
				batch2.AddEntry("test2type", $"test2instance{idx}");
            }
			eventJournal.Write("test2", EntryValue.NoStreamVersion, batch2);

			subscriber2.WaitForExpectedMessages(3);
            topic2.Close();
            journalPublisher2.Close();

            Assert.AreEqual(3, subscriber2.HandledMessages.Count);
        }
        
        [TestMethod]
        public void TestEventJournalPublisherForCategory()
        {
            var eventJournal = Journal.Open("test-ej-cat");
            var messageBus = MessageBus.Start("test-bus-cat");
            var topic = messageBus.OpenTopic("cat-test");
            var journalPublisher = JournalPublisher.From(eventJournal.Name, messageBus.Name, topic.Name);

            var subscriber = new JournalPublisherTestSubscriber();
            topic.Subscribe(subscriber);

            var batch1 = new EntryBatch();
            for (var idx = 0; idx < 3; ++idx)
            {
                batch1.AddEntry("test1type", $"test1instance{idx}");
            }
            eventJournal.Write($"test_1", EntryValue.NoStreamVersion, batch1);

            var batch2 = new EntryBatch();
            for (var idx = 0; idx < 3; ++idx)
            {
                batch2.AddEntry("test2type", $"test2instance{idx}");
            }
            eventJournal.Write("test_2", EntryValue.NoStreamVersion, batch2);

            subscriber.WaitForExpectedMessages(6);
            topic.Close();
            journalPublisher.Close();

            Assert.AreEqual(6, subscriber.HandledMessages.Count);
        }
    }

    internal class JournalPublisherTestSubscriber : ISubscriber
    {
        internal readonly List<Message> HandledMessages = new List<Message>();

        public void Handle(Message message) => HandledMessages.Add(message);

        public void WaitForExpectedMessages(int count)
        {
            for (var idx = 0; idx < 100; ++idx)
            {
                if (HandledMessages.Count == count)
                {
                    break;
                }

                Thread.Sleep(100);
            }
        }
    }
}
