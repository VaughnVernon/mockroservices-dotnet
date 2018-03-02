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
    public class EventJournal
    {
        private static Dictionary<string, EventJournal> eventJournals = new Dictionary<string, EventJournal>();

        private Dictionary<string, EventJournalReader> readers;
        private List<EventValue> store;

        public static EventJournal Open(string name)
        {
            if (eventJournals.ContainsKey(name))
            {
                return eventJournals[name];
            }

            EventJournal eventJournal = new EventJournal(name);

            eventJournals.Add(name, eventJournal);

            return eventJournal;
        }

        public void Close()
        {
            store.Clear();
            readers.Clear();
            eventJournals.Remove(Name);
        }

        public string Name { get; private set; }

        public EventJournalReader Reader(string name)
        {
            if (readers.ContainsKey(name))
            {
                return readers[name];
            }

            EventJournalReader reader = new EventJournalReader(name, this);

            readers.Add(name, reader);

            return reader;
        }

        public EventStreamReader StreamReader()
        {
            return new EventStreamReader(this);
        }

        public void Write(EventBatch batch)
        {
            lock (store)
            {
				foreach (EventBatch.Entry entry in batch.Entries)
				{
					store.Add(new EventValue("", 0, entry.Type, entry.Body, ""));
				}
			}
		}

        public void Write(String streamName, int streamVersion, EventBatch batch)
        {
            lock (store)
            {
				foreach (EventBatch.Entry entry in batch.Entries)
				{
					store.Add(new EventValue(streamName, streamVersion, entry.Type, entry.Body, entry.Snapshot));
				}
			}
		}

        protected EventJournal(string name)
        {
            this.Name = name;
            this.readers = new Dictionary<string, EventJournalReader>();
            this.store = new List<EventValue>();
        }

        internal EventValue EventValueAt(int id)
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

        internal EventStream ReadStream(string streamName)
        {
            lock (store)
            {
                List<EventValue> values = new List<EventValue>();
                List<EventValue> storeCopy = new List<EventValue>(store);
                string latestSnapshot = "";

                foreach (EventValue value in storeCopy)
                {
                    if (value.StreamName.Equals(streamName))
                    {
                        if (value.HasSnapshot())
                        {
                            values.Clear();
                            latestSnapshot = value.Snapshot;
                        }
                        values.Add(value);
                    }
                }

                int streamVersion = values.Count == 0 ? 0 : values[values.Count - 1].StreamVersion;

                return new EventStream(streamName, streamVersion, values, latestSnapshot);
            }
        }
    }

    public class EventJournalPublisher
    {
        private volatile bool closed;
        private Thread dispatcherThread;
        private EventJournalReader reader;
        private Topic topic;

        public static EventJournalPublisher From(
            string eventJournalName,
            string messageBusName,
            string topicName)
        {
            return new EventJournalPublisher(eventJournalName, messageBusName, topicName);
        }

        public void Close()
        {
            closed = true;

            dispatcherThread.Abort();
        }

        protected EventJournalPublisher(
            string eventJournalName,
            string messageBusName,
            string topicName)
        {
            this.reader = EventJournal.Open(eventJournalName).Reader("topic-" + topicName + "-reader");
            this.topic = MessageBus.Start(messageBusName).OpenTopic(topicName);
            this.dispatcherThread = new Thread(new ThreadStart(this.DispatchEach));

            StartDispatcherThread();
        }

        internal void DispatchEach()
        {
            while (!closed)
            {
                StoredEvent storedEvent = reader.ReadNext();

                if (storedEvent.IsValid())
                {
                    Message message =
                        new Message(
                            Convert.ToString(storedEvent.Id),
                            storedEvent.EventValue.Type,
                            storedEvent.EventValue.Body);

                    topic.Publish(message);

                    reader.Acknowledge(storedEvent.Id);

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

    public class EventJournalReader
    {
        private EventJournal eventJournal;
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

        public StoredEvent ReadNext()
        {
            if (readSequence <= eventJournal.GreatestId)
            {
                return new StoredEvent(readSequence, eventJournal.EventValueAt(readSequence));
            }

            return new StoredEvent(StoredEvent.NO_ID, new EventValue("", EventValue.NO_STREAM_VERSION, "", "", ""));
        }

        public void Reset()
        {
            readSequence = 0;
        }

        internal EventJournalReader(string name, EventJournal eventJournal)
        {
            this.Name = name;
            this.eventJournal = eventJournal;
            this.readSequence = 0;
        }
    }

    public class EventStream
    {
        public string Snapshot { get; private set; }
        public List<EventValue> Stream { get; private set; }
        public string StreamName { get; private set; }
        public int StreamVersion { get; private set; }

        public bool HasSnapshot()
        {
            return Snapshot.Length > 0;
        }

        internal EventStream(string streamName, int streamVersion, List<EventValue> stream, string snapshot)
        {
            this.StreamName = streamName;
            this.StreamVersion = streamVersion;
            this.Stream = stream;
            this.Snapshot = snapshot;
        }
    }

    public class EventStreamReader
    {
        private EventJournal eventJournal;

        public EventStream StreamFor(string streamName)
        {
            return eventJournal.ReadStream(streamName);
        }

        internal EventStreamReader(EventJournal eventJournal)
        {
            this.eventJournal = eventJournal;
        }
    }

    public class EventValue
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
            if (other == null || other.GetType() != typeof(EventValue))
            {
                return false;
            }

            EventValue otherEventValue = (EventValue)other;

            return this.StreamName.Equals(otherEventValue.StreamName) &&
                this.StreamVersion == otherEventValue.StreamVersion &&
                this.Type.Equals(otherEventValue.Type) &&
                this.Body.Equals(otherEventValue.Body) &&
                this.Snapshot.Equals(otherEventValue.Snapshot);
        }

        public override string ToString()
        {
            return "EventValue[streamName=" + StreamName + " StreamVersion=" + StreamVersion +
                " type=" + Type + " body=" + Body + " snapshot=" + Snapshot + "]";
        }

        internal EventValue(
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

    public class StoredEvent
    {
        public static long NO_ID = -1L;

        public EventValue EventValue { get; private set; }
        public long Id { get; private set; }

        public bool IsValid()
        {
            return Id != NO_ID;
        }

        public override int GetHashCode()
        {
            return EventValue.GetHashCode() + (int)Id;
        }

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != typeof(StoredEvent))
            {
                return false;
            }

            StoredEvent otherStoredEvent = (StoredEvent)other;

            return this.Id == otherStoredEvent.Id && this.EventValue.Equals(otherStoredEvent.EventValue);
        }

        public override string ToString()
        {
            return "StoredEvent[id=" + Id + " eventValue=" + EventValue + "]";
        }

        internal StoredEvent(long id, EventValue eventValue)
        {
            this.Id = id;
            this.EventValue = eventValue;
        }
    }

	public class EventBatch
	{
		public List<Entry> Entries { get; private set; }

		public static EventBatch Of(String type, String body)
		{
			return new EventBatch(type, body);
		}

		public static EventBatch Of(String type, String body, String snapshot)
		{
			return new EventBatch(type, body, snapshot);
		}

		public EventBatch()
			: this(2)
		{
		}

		public EventBatch(int entries)
		{
			this.Entries = new List<Entry>(entries);
		}

		public EventBatch(String type, String body)
			: this(type, body, "")
		{
		}

		public EventBatch(String type, String body, String snapshot)
			: this()
		{
			this.AddEntry(type, body, snapshot);
		}

		public void AddEntry(String type, String body)
		{
			this.Entries.Add(new Entry(type, body));
		}

		public void AddEntry(String type, String body, String snapshot)
		{
			this.Entries.Add(new Entry(type, body, snapshot));
		}

		public class Entry
		{
			public String Body { get; private set; }
			public String Type { get; private set; }
			public String Snapshot { get; private set; }

			internal Entry(String type, String body)
				: this(type, body, "")
			{
			}

			internal Entry(String type, String body, String snapshot)
			{
				this.Type = type;
				this.Body = body;
				this.Snapshot = snapshot;
			}
		}
	}

	public abstract class EventSourceRepository
	{
		protected EventBatch ToBatch(List<DomainEvent> domainEvents)
		{
			EventBatch batch = new EventBatch(domainEvents.Count);
			foreach (DomainEvent domainEvent in domainEvents)
			{
				string eventBody = Serialization.Serialize(domainEvent);
				batch.AddEntry(domainEvent.GetType().AssemblyQualifiedName, eventBody);
			}
			return batch;
		}

		protected List<DomainEvent> ToEvents(List<EventValue> stream)
		{
			List<DomainEvent> eventStream = new List<DomainEvent>(stream.Count);
			foreach (EventValue value in stream)
			{
                Type eventType = Type.GetType(value.Type);
				DomainEvent domainEvent = (DomainEvent)Serialization.Deserialize(value.Body, eventType);
				eventStream.Add(domainEvent);
			}
			return eventStream;
		}
	}
}
