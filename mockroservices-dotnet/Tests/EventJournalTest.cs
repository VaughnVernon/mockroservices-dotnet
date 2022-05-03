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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VaughnVernon.Mockroservices.Tests
{
    [TestClass]
    public class EventJournalTest
    {
        [TestMethod]
        public void TestEventJournalOpenClose()
        {
            Journal journal = Journal.Open("test");

            Assert.IsNotNull(journal);
            Assert.AreEqual("test", journal.Name);

            Journal journalPreOpened = Journal.Open("test");
            Assert.AreSame(journal, journalPreOpened);

            journal.Close();
        }

        [TestMethod]
        public void TestOpenEventJournalReader()
        {
            Journal journal = Journal.Open("test");
            JournalReader reader = journal.Reader("test_reader");
            Assert.IsNotNull(reader);
            Assert.AreEqual("test_reader", reader.Name);
            JournalReader readerPreOpened = journal.Reader("test_reader");
            Assert.AreSame(reader, readerPreOpened);

            journal.Close();
        }

        [TestMethod]
        public void TestWriteRead()
        {
            Journal journal = Journal.Open("test");
			journal.Write("name123", 1, EntryBatch.Of("type1", "type1_instance1"));
			journal.Write("name456", 1, EntryBatch.Of("type2", "type2_instance1"));
			JournalReader reader = journal.Reader("test_reader");
            Assert.AreEqual(new StoredSource(0, new EntryValue("name123", 1, "type1", "type1_instance1", "")), reader.ReadNext());
            reader.Acknowledge(0);
            Assert.AreEqual(new StoredSource(1, new EntryValue("name456", 1, "type2", "type2_instance1", "")), reader.ReadNext());
            reader.Acknowledge(1);
            Assert.AreEqual(new StoredSource(-1, new EntryValue("", -1, "", "", "")), reader.ReadNext());

            journal.Close();
        }

        [TestMethod]
        public void TestWriteReadStream()
        {
			Journal journal = Journal.Open("test");
			journal.Write("name123", 1, EntryBatch.Of("type1", "type1_instance1"));
			journal.Write("name456", 1, EntryBatch.Of("type2", "type2_instance1"));
			journal.Write("name123", 2, EntryBatch.Of("type1-1", "type1-1_instance1"));
			journal.Write("name123", 3, EntryBatch.Of("type1-2", "type1-2_instance1"));
			journal.Write("name456", 2, EntryBatch.Of("type2-1", "type2-1_instance1"));

			EntryStreamReader streamReader = journal.StreamReader();

            EntryStream eventStream123 = streamReader.StreamFor("name123");
            Assert.AreEqual(3, eventStream123.StreamVersion);
            Assert.AreEqual(3, eventStream123.Stream.Count);
            Assert.AreEqual(new EntryValue("name123", 1, "type1", "type1_instance1", ""), eventStream123.Stream[0]);
            Assert.AreEqual(new EntryValue("name123", 2, "type1-1", "type1-1_instance1", ""), eventStream123.Stream[1]);
            Assert.AreEqual(new EntryValue("name123", 3, "type1-2", "type1-2_instance1", ""), eventStream123.Stream[2]);

            EntryStream eventStream456 = streamReader.StreamFor("name456");
            Assert.AreEqual(2, eventStream456.StreamVersion);
            Assert.AreEqual(2, eventStream456.Stream.Count);
            Assert.AreEqual(new EntryValue("name456", 1, "type2", "type2_instance1", ""), eventStream456.Stream[0]);
            Assert.AreEqual(new EntryValue("name456", 2, "type2-1", "type2-1_instance1", ""), eventStream456.Stream[1]);

            journal.Close();
        }

        [TestMethod]
        public void TestWriteReadStreamSnapshot()
        {
            Journal journal = Journal.Open("test");
			journal.Write("name123", 1, EntryBatch.Of("type1", "type1_instance1", "SNAPSHOT123-1"));
			journal.Write("name456", 1, EntryBatch.Of("type2", "type2_instance1", "SNAPSHOT456-1"));
			journal.Write("name123", 2, EntryBatch.Of("type1-1", "type1-1_instance1", "SNAPSHOT123-2"));
			journal.Write("name123", 3, EntryBatch.Of("type1-2", "type1-2_instance1"));
			journal.Write("name456", 2, EntryBatch.Of("type2-1", "type2-1_instance1", "SNAPSHOT456-2"));

			EntryStreamReader streamReader = journal.StreamReader();

            EntryStream eventStream123 = streamReader.StreamFor("name123");
            Assert.AreEqual("name123", eventStream123.StreamName);
            Assert.AreEqual(3, eventStream123.StreamVersion);
            Assert.AreEqual(1, eventStream123.Stream.Count);
            Assert.AreEqual("SNAPSHOT123-2", eventStream123.Snapshot);

            EntryStream eventStream456 = streamReader.StreamFor("name456");
            Assert.AreEqual("name456", eventStream456.StreamName);
            Assert.AreEqual(2, eventStream456.StreamVersion);
            Assert.AreEqual(0, eventStream456.Stream.Count);
            Assert.AreEqual("SNAPSHOT456-2", eventStream456.Snapshot);

            journal.Close();
        }
    }
}
