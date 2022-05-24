using System.Collections.Generic;

namespace VaughnVernon.Mockroservices.Journals;

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