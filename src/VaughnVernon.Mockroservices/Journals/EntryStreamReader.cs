namespace VaughnVernon.Mockroservices.Journals;

public class EntryStreamReader
{
    private readonly Journal journal;

    public EntryStream StreamFor(string streamName) => journal.ReadStream(streamName);

    public EntryStream StreamFor<T>(string streamName) => journal.ReadStream(StreamNameBuilder.BuildStreamNameFor<T>(streamName));

    internal EntryStreamReader(Journal journal) => this.journal = journal;
}