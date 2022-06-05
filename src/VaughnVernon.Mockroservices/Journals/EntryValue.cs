namespace VaughnVernon.Mockroservices.Journals;

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

    internal EntryValue WithStreamVersion(int streamVersion) => new(StreamName, streamVersion, Type, Body, Snapshot);
}