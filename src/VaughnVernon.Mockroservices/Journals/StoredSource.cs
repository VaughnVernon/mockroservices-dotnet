namespace VaughnVernon.Mockroservices.Journals;

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