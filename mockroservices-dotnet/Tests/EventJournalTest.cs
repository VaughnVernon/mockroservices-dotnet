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

namespace VaughnVernon.Mockroservices
{
    [TestClass]
    public class EventJournalTest
    {
        [TestMethod]
        public void TestEventJournalOpenClose()
        {
            EventJournal journal = EventJournal.Open("test");

            Assert.IsNotNull(journal);
            Assert.AreEqual("test", journal.Name);

            EventJournal journalPreOpened = EventJournal.Open("test");
            Assert.AreSame(journal, journalPreOpened);

            journal.Close();
        }

        [TestMethod]
        public void TestOpenEventJournalReader()
        {
            EventJournal journal = EventJournal.Open("test");
            EventJournalReader reader = journal.Reader("test_reader");
            Assert.IsNotNull(reader);
            Assert.AreEqual("test_reader", reader.Name);
            EventJournalReader readerPreOpened = journal.Reader("test_reader");
            Assert.AreSame(reader, readerPreOpened);

            journal.Close();
        }

        [TestMethod]
        public void TestWriteRead()
        {
            EventJournal journal = EventJournal.Open("test");
			journal.Write("name123", 1, EventBatch.Of("type1", "type1_instance1"));
			journal.Write("name456", 1, EventBatch.Of("type2", "type2_instance1"));
			EventJournalReader reader = journal.Reader("test_reader");
            Assert.AreEqual(new StoredEvent(0, new EventValue("name123", 1, "type1", "type1_instance1", "")), reader.ReadNext());
            reader.Acknowledge(0);
            Assert.AreEqual(new StoredEvent(1, new EventValue("name456", 1, "type2", "type2_instance1", "")), reader.ReadNext());
            reader.Acknowledge(1);
            Assert.AreEqual(new StoredEvent(-1, new EventValue("", -1, "", "", "")), reader.ReadNext());

            journal.Close();
        }

        [TestMethod]
        public void TestWriteReadStream()
        {
			EventJournal journal = EventJournal.Open("test");
			journal.Write("name123", 1, EventBatch.Of("type1", "type1_instance1"));
			journal.Write("name456", 1, EventBatch.Of("type2", "type2_instance1"));
			journal.Write("name123", 2, EventBatch.Of("type1-1", "type1-1_instance1"));
			journal.Write("name123", 3, EventBatch.Of("type1-2", "type1-2_instance1"));
			journal.Write("name456", 2, EventBatch.Of("type2-1", "type2-1_instance1"));

			EventStreamReader streamReader = journal.StreamReader();

            EventStream eventStream123 = streamReader.StreamFor("name123");
            Assert.AreEqual(3, eventStream123.StreamVersion);
            Assert.AreEqual(3, eventStream123.Stream.Count);
            Assert.AreEqual(new EventValue("name123", 1, "type1", "type1_instance1", ""), eventStream123.Stream[0]);
            Assert.AreEqual(new EventValue("name123", 2, "type1-1", "type1-1_instance1", ""), eventStream123.Stream[1]);
            Assert.AreEqual(new EventValue("name123", 3, "type1-2", "type1-2_instance1", ""), eventStream123.Stream[2]);

            EventStream eventStream456 = streamReader.StreamFor("name456");
            Assert.AreEqual(2, eventStream456.StreamVersion);
            Assert.AreEqual(2, eventStream456.Stream.Count);
            Assert.AreEqual(new EventValue("name456", 1, "type2", "type2_instance1", ""), eventStream456.Stream[0]);
            Assert.AreEqual(new EventValue("name456", 2, "type2-1", "type2-1_instance1", ""), eventStream456.Stream[1]);

            journal.Close();
        }

        [TestMethod]
        public void TestWriteReadStreamSnapshot()
        {
            EventJournal journal = EventJournal.Open("test");
			journal.Write("name123", 1, EventBatch.Of("type1", "type1_instance1", "SNAPSHOT123-1"));
			journal.Write("name456", 1, EventBatch.Of("type2", "type2_instance1", "SNAPSHOT456-1"));
			journal.Write("name123", 2, EventBatch.Of("type1-1", "type1-1_instance1", "SNAPSHOT123-2"));
			journal.Write("name123", 3, EventBatch.Of("type1-2", "type1-2_instance1"));
			journal.Write("name456", 2, EventBatch.Of("type2-1", "type2-1_instance1", "SNAPSHOT456-2"));

			EventStreamReader streamReader = journal.StreamReader();

            EventStream eventStream123 = streamReader.StreamFor("name123");
            Assert.AreEqual("name123", eventStream123.StreamName);
            Assert.AreEqual(3, eventStream123.StreamVersion);
            Assert.AreEqual(2, eventStream123.Stream.Count);
            Assert.AreEqual("SNAPSHOT123-2", eventStream123.Snapshot);
            Assert.AreEqual(new EventValue("name123", 2, "type1-1", "type1-1_instance1", "SNAPSHOT123-2"), eventStream123.Stream[0]);
            Assert.AreEqual(new EventValue("name123", 3, "type1-2", "type1-2_instance1", ""), eventStream123.Stream[1]);

            EventStream eventStream456 = streamReader.StreamFor("name456");
            Assert.AreEqual("name456", eventStream456.StreamName);
            Assert.AreEqual(2, eventStream456.StreamVersion);
            Assert.AreEqual(1, eventStream456.Stream.Count);
            Assert.AreEqual("SNAPSHOT456-2", eventStream456.Snapshot);
            Assert.AreEqual(new EventValue("name456", 2, "type2-1", "type2-1_instance1", "SNAPSHOT456-2"), eventStream456.Stream[0]);

            journal.Close();
        }
    }
}
