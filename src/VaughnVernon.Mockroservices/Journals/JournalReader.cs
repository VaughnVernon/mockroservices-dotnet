using System;

namespace VaughnVernon.Mockroservices.Journals;

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