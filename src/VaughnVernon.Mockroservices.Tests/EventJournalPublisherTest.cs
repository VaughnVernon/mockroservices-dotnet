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
            var topic = messageBus.OpenTopic("test-topic");
            var journalPublisher =
                JournalPublisher.From(eventJournal.Name, messageBus.Name, topic.Name);

            var subscriber = new JournalPublisherTestSubscriber();
            topic.Subscribe(subscriber);

			var batch1 = new EntryBatch();
			for (var idx = 0; idx < 3; ++idx)
            {
				batch1.AddEntry("test1type", $"test1instance{idx}");
            }
			eventJournal.Write("test1", 0, batch1);

			var batch2 = new EntryBatch();
			for (var idx = 0; idx < 3; ++idx)
            {
				batch2.AddEntry("test2type", $"test2instance{idx}");
            }
			eventJournal.Write("test2", 0, batch2);

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
