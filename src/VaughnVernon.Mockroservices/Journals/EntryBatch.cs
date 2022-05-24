using System.Collections.Generic;

namespace VaughnVernon.Mockroservices.Journals;

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