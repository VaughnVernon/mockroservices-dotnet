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
using System.Collections.Generic;
using System.Threading;

namespace VaughnVernon.Mockroservices
{
    public class Journal
    {
        private static Dictionary<string, Journal> journals = new Dictionary<string, Journal>();

        private Dictionary<string, JournalReader> readers;
        private List<EntryValue> store;

        public static Journal Open(string name)
        {
            if (journals.ContainsKey(name))
            {
                return journals[name];
            }

            Journal eventJournal = new Journal(name);

            journals.Add(name, eventJournal);

            return eventJournal;
        }

        public void Close()
        {
            store.Clear();
            readers.Clear();
            journals.Remove(Name);
        }

        public string Name { get; private set; }

        public JournalReader Reader(string name)
        {
            if (readers.ContainsKey(name))
            {
                return readers[name];
            }

            JournalReader reader = new JournalReader(name, this);

            readers.Add(name, reader);

            return reader;
        }

        public EntryStreamReader StreamReader()
        {
            return new EntryStreamReader(this);
        }

        public void Write(EntryBatch batch)
        {
            lock (store)
            {
				foreach (EntryBatch.Entry entry in batch.Entries)
				{
					store.Add(new EntryValue("", 0, entry.Type, entry.Body, ""));
				}
			}
		}

        public void Write(string streamName, int streamVersion, EntryBatch batch)
        {
            lock (store)
            {
				foreach (EntryBatch.Entry entry in batch.Entries)
				{
					store.Add(new EntryValue(streamName, streamVersion, entry.Type, entry.Body, entry.Snapshot));
				}
			}
		}

        protected Journal(string name)
        {
            this.Name = name;
            this.readers = new Dictionary<string, JournalReader>();
            this.store = new List<EntryValue>();
        }

        internal EntryValue EntryValueAt(int id)
        {
            lock (store)
            {
                if (id >= store.Count)
                {
                    throw new InvalidOperationException("The id does not exist: " + id);
                }

                return store[id];
            }
        }

        internal int GreatestId { get { return store.Count - 1; } }

        internal EntryStream ReadStream(string streamName)
        {
            lock (store)
            {
                List<EntryValue> values = new List<EntryValue>();
                List<EntryValue> storeCopy = new List<EntryValue>(store);
                EntryValue latestSnapshotValue = null;

                foreach (EntryValue value in storeCopy)
                {
                    if (value.StreamName.Equals(streamName))
                    {
                        if (value.HasSnapshot())
                        {
                            values.Clear();
                            latestSnapshotValue = value;
                        }
                        else
                        {
                            values.Add(value);
                        }
                    }
                }

                int snapshotVersion = latestSnapshotValue == null ? 0 : latestSnapshotValue.StreamVersion;
                int streamVersion = values.Count == 0 ? snapshotVersion : values[values.Count - 1].StreamVersion;

                return new EntryStream(streamName, streamVersion, values, latestSnapshotValue == null ? "" : latestSnapshotValue.Snapshot);
            }
        }
    }

    public class JournalPublisher
    {
        private volatile bool closed;
        private Thread dispatcherThread;
        private JournalReader reader;
        private Topic topic;

        public static JournalPublisher From(
            string journalName,
            string messageBusName,
            string topicName)
        {
            return new JournalPublisher(journalName, messageBusName, topicName);
        }

        public void Close()
        {
            closed = true;
        }

        protected JournalPublisher(
            string journalName,
            string messageBusName,
            string topicName)
        {
            this.reader = Journal.Open(journalName).Reader("topic-" + topicName + "-reader");
            this.topic = MessageBus.Start(messageBusName).OpenTopic(topicName);
            this.dispatcherThread = new Thread(new ThreadStart(this.DispatchEach));

            StartDispatcherThread();
        }

        internal void DispatchEach()
        {
            while (!closed)
            {
                StoredSource storedSource = reader.ReadNext();

                if (storedSource.IsValid())
                {
                    Message message =
                        new Message(
                            Convert.ToString(storedSource.Id),
                            storedSource.EntryValue.Type,
                            storedSource.EntryValue.Body);

                    topic.Publish(message);

                    reader.Acknowledge(storedSource.Id);

                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void StartDispatcherThread()
        {
            dispatcherThread.Start();

            while (!dispatcherThread.IsAlive)
            {
                Thread.Sleep(1);
            }
        }
    }

    public class JournalReader
    {
        private Journal journal;
        private int readSequence;

        public void Acknowledge(long id)
        {
            if (id == readSequence)
            {
                ++readSequence;
            }
            else if (id > readSequence)
            {
                throw new InvalidOperationException("The id is out of range.");
            }
            else if (id < readSequence)
            {
                // allow for this case; don't throw IllegalArgumentException("The id has already been acknowledged.");
            }
        }

        public string Name { get; private set; }

        public StoredSource ReadNext()
        {
            if (readSequence <= journal.GreatestId)
            {
                return new StoredSource(readSequence, journal.EntryValueAt(readSequence));
            }

            return new StoredSource(StoredSource.NO_ID, new EntryValue("", EntryValue.NO_STREAM_VERSION, "", "", ""));
        }

        public void Reset()
        {
            readSequence = 0;
        }

        internal JournalReader(string name, Journal eventJournal)
        {
            this.Name = name;
            this.journal = eventJournal;
            this.readSequence = 0;
        }
    }

    public class EntryStream
    {
        public string Snapshot { get; private set; }
        public List<EntryValue> Stream { get; private set; }
        public string StreamName { get; private set; }
        public int StreamVersion { get; private set; }

        public bool HasSnapshot()
        {
            return Snapshot.Length > 0;
        }

        internal EntryStream(string streamName, int streamVersion, List<EntryValue> stream, string snapshot)
        {
            this.StreamName = streamName;
            this.StreamVersion = streamVersion;
            this.Stream = stream;
            this.Snapshot = snapshot;
        }
    }

    public class EntryStreamReader
    {
        private Journal journal;

        public EntryStream StreamFor(string streamName)
        {
            return journal.ReadStream(streamName);
        }

        internal EntryStreamReader(Journal journal)
        {
            this.journal = journal;
        }
    }

    public class EntryValue
    {
        public static int NO_STREAM_VERSION = -1;

        public string Body { get; private set; }
        public string Snapshot { get; private set; }
        public string StreamName { get; private set; }
        public int StreamVersion { get; private set; }
        public string Type { get; private set; }

        public bool HasSnapshot()
        {
            return Snapshot.Length > 0;
        }

        public override int GetHashCode()
        {
            return this.StreamName.GetHashCode() + this.StreamVersion;
        }

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != typeof(EntryValue))
            {
                return false;
            }

            EntryValue otherEntryValue = (EntryValue)other;

            return this.StreamName.Equals(otherEntryValue.StreamName) &&
                this.StreamVersion == otherEntryValue.StreamVersion &&
                this.Type.Equals(otherEntryValue.Type) &&
                this.Body.Equals(otherEntryValue.Body) &&
                this.Snapshot.Equals(otherEntryValue.Snapshot);
        }

        public override string ToString()
        {
            return "EntryValue[streamName=" + StreamName + " StreamVersion=" + StreamVersion +
                " type=" + Type + " body=" + Body + " snapshot=" + Snapshot + "]";
        }

        internal EntryValue(
            string streamName,
            int streamVersion,
            string type,
            string body,
            string snapshot)
        {

            this.StreamName = streamName;
            this.StreamVersion = streamVersion;
            this.Type = type;
            this.Body = body;
            this.Snapshot = snapshot;
        }
    }

    public class StoredSource
    {
        public static long NO_ID = -1L;

        public EntryValue EntryValue { get; private set; }
        public long Id { get; private set; }

        public bool IsValid()
        {
            return Id != NO_ID;
        }

        public override int GetHashCode()
        {
            return EntryValue.GetHashCode() + (int)Id;
        }

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != typeof(StoredSource))
            {
                return false;
            }

            StoredSource otherStoredSource = (StoredSource)other;

            return this.Id == otherStoredSource.Id && this.EntryValue.Equals(otherStoredSource.EntryValue);
        }

        public override string ToString()
        {
            return "StoredSource[id=" + Id + " entryValue=" + EntryValue + "]";
        }

        internal StoredSource(long id, EntryValue eventValue)
        {
            this.Id = id;
            this.EntryValue = eventValue;
        }
    }

	public class EntryBatch
	{
		public List<Entry> Entries { get; private set; }

		public static EntryBatch Of(string type, string body)
		{
			return new EntryBatch(type, body);
		}

		public static EntryBatch Of(string type, string body, string snapshot)
		{
			return new EntryBatch(type, body, snapshot);
		}

		public EntryBatch()
			: this(2)
		{
		}

		public EntryBatch(int entries)
		{
			this.Entries = new List<Entry>(entries);
		}

		public EntryBatch(string type, string body)
			: this(type, body, "")
		{
		}

		public EntryBatch(string type, string body, string snapshot)
			: this()
		{
			this.AddEntry(type, body, snapshot);
		}

		public void AddEntry(string type, string body)
		{
			this.Entries.Add(new Entry(type, body));
		}

		public void AddEntry(string type, string body, string snapshot)
		{
			this.Entries.Add(new Entry(type, body, snapshot));
		}

		public class Entry
		{
			public string Body { get; private set; }
			public string Type { get; private set; }
			public string Snapshot { get; private set; }

			internal Entry(string type, string body)
				: this(type, body, "")
			{
			}

			internal Entry(string type, string body, string snapshot)
			{
				this.Type = type;
				this.Body = body;
				this.Snapshot = snapshot;
			}
		}
	}
}
