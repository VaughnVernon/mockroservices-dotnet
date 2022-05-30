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

namespace VaughnVernon.Mockroservices.Journals
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
}