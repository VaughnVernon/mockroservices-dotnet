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
using System.Threading;
using VaughnVernon.Mockroservices.MessageBuses;

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