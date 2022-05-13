//   Copyright © 2022 Vaughn Vernon. All rights reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace VaughnVernon.Mockroservices
{
    public class Journal
    {
        private static readonly Dictionary<string, Journal> Journals = new Dictionary<string, Journal>();

        private readonly Dictionary<string, JournalReader> readers;
        private readonly List<EntryValue> store;

        public static Journal Open(string name)
        {
            if (Journals.ContainsKey(name))
            {
                return Journals[name];
            }

            var eventJournal = new Journal(name);

            Journals.Add(name, eventJournal);

            return eventJournal;
        }

        public void Close()
        {
            lock (((ICollection)store).SyncRoot)
            {
                store.Clear();
            }
            readers.Clear();
            Journals.Remove(Name);
        }

        public string Name { get; }

        public JournalReader Reader(string name)
        {
            if (readers.ContainsKey(name))
            {
                return readers[name];
            }

            var reader = new JournalReader(name, this);

            readers.Add(name, reader);

            return reader;
        }

        public EntryStreamReader StreamReader() => new EntryStreamReader(this);

        public void Write(EntryBatch batch)
        {
            lock (((ICollection)store).SyncRoot)
            {
                if (!TryCanWrite(string.Empty, EntryValue.NoStreamVersion, out var currentVersion))
                {
                    throw new IndexOutOfRangeException($"Cannot write to stream '{string.Empty}' with expected version {EntryValue.NoStreamVersion} because the current version is {currentVersion}");   
                }
                
                var offset = 1;
				foreach (var entry in batch.Entries)
				{
                    store.Add(new EntryValue(string.Empty, 0  + offset, entry.Type, entry.Body, string.Empty));
                    offset++;
                }
			}
		}

        public void Write<T>(string streamName, int streamVersion, EntryBatch batch)
        {
            lock (((ICollection)store).SyncRoot)
            {
                if (!TryCanWrite(streamName, streamVersion, out var currentVersion))
                {
                    throw new IndexOutOfRangeException($"Cannot write to stream '{streamName}' with expected version {streamVersion} because the current version is {currentVersion}");   
                }
                
                var offset = 1;
                foreach (var entry in batch.Entries)
                {
                    store.Add(new EntryValue(StreamNameBuilder.BuildStreamNameFor<T>(streamName), streamVersion + offset, entry.Type, entry.Body, entry.Snapshot));
                    offset++;
                }
            }
        }
        
        public void Write(string streamName, int streamVersion, EntryBatch batch)
        {
            lock (((ICollection)store).SyncRoot)
            {
                if (!TryCanWrite(streamName, streamVersion, out var currentVersion))
                {
                    throw new IndexOutOfRangeException($"Cannot write to stream '{streamName}' with expected version {streamVersion} because the current version is {currentVersion}");   
                }

                var offset = 1;
				foreach (var entry in batch.Entries)
				{
					store.Add(new EntryValue(streamName, streamVersion + offset, entry.Type, entry.Body, entry.Snapshot));
                    offset++;
                }
			}
		}

        protected Journal(string name)
        {
            Name = name;
            readers = new Dictionary<string, JournalReader>();
            store = new List<EntryValue>();
        }

        internal EntryValue EntryValueAt(int id, string streamName)
        {
            lock (((ICollection)store).SyncRoot)
            {
                if (id > GreatestId(streamName))
                {
                    throw new InvalidOperationException($"The id does not exist: {id}");
                }

                var maybeCategoryStreamName = MaybeCategoryStreamName(streamName);
                if (IsCategoryStream(streamName))
                {
                    return store.Where(e => e.StreamName.StartsWith(maybeCategoryStreamName)).ElementAt(id);
                }

                return store.Where(e => e.StreamName == maybeCategoryStreamName).ElementAt(id);
            }
        }

        internal int GreatestId(string streamName)
        {
            lock (((ICollection)store).SyncRoot)
            {
                var maybeCategoryStreamName = MaybeCategoryStreamName(streamName);
                if (IsCategoryStream(streamName))
                {
                    return store.Count(e => e.StreamName.StartsWith(maybeCategoryStreamName)) - 1;
                }
                
                return store.Count(e => e.StreamName == maybeCategoryStreamName) - 1;
            }
        }

        internal EntryStream ReadStream(string streamName)
        {
            lock (((ICollection)store).SyncRoot)
            {
                var values = new List<EntryValue>();
                var storeCopy = new List<EntryValue>(store);
                EntryValue? latestSnapshotValue = null;

                var globalIndex = 0;
                foreach (var value in storeCopy)
                {
                    if (CanRead(value, streamName))
                    {
                        if (value.HasSnapshot())
                        {
                            values.Clear();
                            latestSnapshotValue = value;
                        }
                        else
                        {
                            values.Add(IsCategoryStream(streamName) ? value.WithStreamVersion(globalIndex) : value);
                        }
                    }

                    globalIndex++;
                }

                var snapshotVersion = latestSnapshotValue?.StreamVersion ?? 0;
                var streamVersion = values.Count == 0 ? snapshotVersion : values[values.Count - 1].StreamVersion;

                return new EntryStream(streamName, streamVersion, values, latestSnapshotValue == null ? string.Empty : latestSnapshotValue.Snapshot);
            }
        }
        
        private bool TryCanWrite(string streamName, int expectedStreamVersion, out int currentVersion)
        {
            var lastEntry = store.LastOrDefault(e => e.StreamName == streamName);
            if (lastEntry != null)
            {
                currentVersion = expectedStreamVersion;
                return lastEntry.StreamVersion == expectedStreamVersion;
            }

            if (expectedStreamVersion == EntryValue.NoStreamVersion)
            {
                currentVersion = EntryValue.NoStreamVersion;
                return true;
            }

            currentVersion = EntryValue.NoStreamVersion;
            return false;
        }

        private bool CanRead(EntryValue entryValue, string streamName)
        {
            var maybeCategoryStreamName = MaybeCategoryStreamName(streamName);
            if (IsCategoryStream(streamName))
            {
                return entryValue.StreamName.StartsWith(maybeCategoryStreamName);
            }

            return entryValue.StreamName.Equals(maybeCategoryStreamName);
        }

        private string MaybeCategoryStreamName(string streamName)
        {
            if (IsCategoryStream(streamName))
            {
                return streamName.Split('-')[1];
            }

            return streamName;
        }
        
        private bool IsCategoryStream(string streamName) => streamName.StartsWith("cat-");
    }

    public class JournalPublisher
    {
        private volatile bool closed;
        private readonly Thread dispatcherThread;
        private readonly JournalReader reader;
        private readonly Topic topic;

        public static JournalPublisher From(
            string journalName,
            string messageBusName,
            string topicName) =>
            new JournalPublisher(journalName, messageBusName, topicName);

        public void Close() => closed = true;

        protected JournalPublisher(
            string journalName,
            string messageBusName,
            string topicName)
        {
            reader = Journal.Open(journalName).Reader(topicName);
            topic = MessageBus.Start(messageBusName).OpenTopic(topicName);
            dispatcherThread = new Thread(DispatchEach);

            StartDispatcherThread();
        }

        internal void DispatchEach()
        {
            while (!closed)
            {
                var storedSource = reader.ReadNext();

                if (storedSource.IsValid())
                {
                    var message =
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
        private readonly Journal journal;
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

        public string Name { get; }

        public StoredSource ReadNext()
        {
            if (readSequence <= journal.GreatestId(Name))
            {
                return new StoredSource(readSequence, journal.EntryValueAt(readSequence, Name));
            }

            return new StoredSource(StoredSource.NoId, new EntryValue(string.Empty, EntryValue.NoStreamVersion, string.Empty, string.Empty, string.Empty));
        }

        public void Reset() => readSequence = 0;

        internal JournalReader(string name, Journal eventJournal)
        {
            Name = name;
            journal = eventJournal;
            readSequence = 0;
        }
    }

    public class EntryStream
    {
        public string Snapshot { get; }
        public IEnumerable<EntryValue> Stream { get; }
        public string StreamName { get; }
        public int StreamVersion { get; }

        public bool HasSnapshot() => Snapshot.Length > 0;

        internal EntryStream(string streamName, int streamVersion, IEnumerable<EntryValue> stream, string snapshot)
        {
            StreamName = streamName;
            StreamVersion = streamVersion;
            Stream = stream;
            Snapshot = snapshot;
        }
    }

    public class EntryStreamReader
    {
        private readonly Journal journal;

        public EntryStream StreamFor(string streamName) => journal.ReadStream(streamName);

        public EntryStream StreamFor<T>(string streamName) => journal.ReadStream(StreamNameBuilder.BuildStreamNameFor<T>(streamName));

        internal EntryStreamReader(Journal journal) => this.journal = journal;
    }

    public class EntryValue
    {
        public const int NoStreamVersion = -1;

        public string Body { get; }
        public string Snapshot { get; }
        public string StreamName { get; }
        public int StreamVersion { get; }
        public string Type { get; }

        public bool HasSnapshot() => Snapshot.Length > 0;

        public override int GetHashCode() => StreamName.GetHashCode() + StreamVersion;

        public override bool Equals(object? other)
        {
            if (other == null || other.GetType() != typeof(EntryValue))
            {
                return false;
            }

            var otherEntryValue = (EntryValue)other;

            return StreamName.Equals(otherEntryValue.StreamName) &&
                StreamVersion == otherEntryValue.StreamVersion &&
                Type.Equals(otherEntryValue.Type) &&
                Body.Equals(otherEntryValue.Body) &&
                Snapshot.Equals(otherEntryValue.Snapshot);
        }

        public override string ToString() => $"EntryValue[streamName={StreamName} StreamVersion={StreamVersion} type={Type} body={Body} snapshot={Snapshot}]";

        internal EntryValue(
            string streamName,
            int streamVersion,
            string type,
            string body,
            string snapshot)
        {

            StreamName = streamName;
            StreamVersion = streamVersion;
            Type = type;
            Body = body;
            Snapshot = snapshot;
        }

        internal EntryValue WithStreamVersion(int streamVersion) => new EntryValue(StreamName, streamVersion, Type, Body, Snapshot);
    }

    public class StoredSource
    {
        public const long NoId = -1L;

        public EntryValue EntryValue { get; }
        public long Id { get; }

        public bool IsValid() => Id != NoId;

        public override int GetHashCode() => EntryValue.GetHashCode() + (int)Id;

        public override bool Equals(object? other)
        {
            if (other == null || other.GetType() != typeof(StoredSource))
            {
                return false;
            }

            var otherStoredSource = (StoredSource)other;

            return Id == otherStoredSource.Id && EntryValue.Equals(otherStoredSource.EntryValue);
        }

        public override string ToString() => $"StoredSource[id={Id} entryValue={EntryValue}]";

        internal StoredSource(long id, EntryValue eventValue)
        {
            Id = id;
            EntryValue = eventValue;
        }
    }

	public class EntryBatch
    {
        private readonly List<Entry> entries;

        public IEnumerable<Entry> Entries => entries;

		public static EntryBatch Of(string type, string body) => new EntryBatch(type, body);

        public static EntryBatch Of(string type, string body, string snapshot) => new EntryBatch(type, body, snapshot);

        public EntryBatch()
			: this(2)
		{
		}

		public EntryBatch(int entries) => this.entries = new List<Entry>(entries);

        public EntryBatch(string type, string body)
			: this(type, body, string.Empty)
		{
		}

		public EntryBatch(string type, string body, string snapshot)
			: this() =>
            AddEntry(type, body, snapshot);

        public void AddEntry(string type, string body) => entries.Add(new Entry(type, body));

        public void AddEntry(string type, string body, string snapshot) => entries.Add(new Entry(type, body, snapshot));

        public class Entry
		{
			public string Body { get; }
			public string Type { get; }
			public string Snapshot { get; }

			internal Entry(string type, string body)
				: this(type, body, string.Empty)
			{
			}

			internal Entry(string type, string body, string snapshot)
			{
				Type = type;
				Body = body;
				Snapshot = snapshot;
			}
		}
	}
}