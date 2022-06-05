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

using System.Collections.Generic;

namespace VaughnVernon.Mockroservices.Journals;

public class EntryBatch
{
    private readonly List<Entry> entries;

    public IEnumerable<Entry> Entries => entries;

    public static EntryBatch Of(string type, string body) => new(type, body);

    public static EntryBatch Of(string type, string body, string snapshot) => new(type, body, snapshot);

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