//   Copyright © 2020 Vaughn Vernon. All rights reserved.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace VaughnVernon.Mockroservices
{
    public class MessageBus
    {
        private static readonly Dictionary<string, MessageBus> MessageBuses = new Dictionary<string, MessageBus>();

        private readonly Dictionary<string, Topic> topics;

        public static MessageBus Start(string name)
        {
            lock (MessageBuses)
            {
                if (MessageBuses.ContainsKey(name))
                {
                    return MessageBuses[name];
                }

                var messageBus = new MessageBus(name);
                MessageBuses.Add(name, messageBus);

                return messageBus;
            }
        }

        public string Name { get; }

        public Topic OpenTopic(string name)
        {
            lock (topics)
            {
                if (topics.ContainsKey(name))
                {
                    return topics[name];
                }

                var topic = new Topic(name);
                topics.Add(name, topic);

                return topic;
            }
        }

        protected MessageBus(string name)
        {
            Name = name;
            topics = new Dictionary<string, Topic>();
        }
    }

    public class Message
    {
        public string Id { get; }
        public string Payload { get; }
        public string Type { get; }

        public Message(string id, string type, string payload)
        {
            Id = id;
            Type = type;
            Payload = payload;
        }

        public string SimpleTypeName() => Type.Split(',')[0];
    }

    public interface ISubscriber
    {
        void Handle(Message message);
    }

    public class Topic
    {
        private volatile bool closed;
        private readonly Thread dispatcherThread;
        private readonly ConcurrentQueue<Message> queue;
        private readonly HashSet<ISubscriber> subscribers;

        public void Close()
        {
            while (queue.Count > 0)
            {
                Thread.Sleep(100);
            }

            closed = true;
        }

        public string Name { get; }

        public void Publish(Message message) => queue.Enqueue(message);

        public void Subscribe(ISubscriber subscriber)
        {
            lock (subscribers)
            {
                subscribers.Add(subscriber);
            }
        }

        internal Topic(string name)
        {
            Name = name;
            closed = false;
            dispatcherThread = new Thread(DispatchEach);
            queue = new ConcurrentQueue<Message>();
            subscribers = new HashSet<ISubscriber>();

            StartDispatcherThread();
        }

        internal void DispatchEach()
        {
            while (!closed)
            {
                lock (subscribers)
                {
                    if (subscribers.Count > 0)
                    {
                        if (queue.TryDequeue(out Message message))
                        {
                            foreach (var subscriber in subscribers)
                            {
                                try
                                {
                                    subscriber.Handle(message);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"Error dispatching message to handler: {e.Message}");
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(100);
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
}
