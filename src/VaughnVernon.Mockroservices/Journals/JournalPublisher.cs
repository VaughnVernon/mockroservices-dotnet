using System;
using System.Threading;

namespace VaughnVernon.Mockroservices.Journals;

public class JournalPublisher
{
    private volatile bool closed;
    private readonly Thread dispatcherThread;
    private readonly JournalReader reader;
    private readonly Topic topic;

    public static JournalPublisher From(string journalName, string messageBusName, string topicName) =>
        new(journalName, messageBusName, topicName);

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