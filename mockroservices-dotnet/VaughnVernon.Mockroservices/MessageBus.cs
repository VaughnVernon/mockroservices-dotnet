//   Copyright © 2017 Vaughn Vernon. All rights reserved.
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
        private static Dictionary<string, MessageBus> messageBuses = new Dictionary<string, MessageBus>();

        private Dictionary<string, Topic> topics;

        public static MessageBus Start(string name)
        {
            lock (messageBuses)
            {
                if (messageBuses.ContainsKey(name))
                {
                    return messageBuses[name];
                }

                MessageBus messageBus = new MessageBus(name);
                messageBuses.Add(name, messageBus);

                return messageBus;
            }
        }

        public string Name { get; private set; }

        public Topic OpenTopic(string name)
        {
            lock (topics)
            {
                if (topics.ContainsKey(name))
                {
                    return topics[name];
                }

                Topic topic = new Topic(name);
                topics.Add(name, topic);

                return topic;
            }
        }

        protected MessageBus(string name)
        {
            this.Name = name;
            this.topics = new Dictionary<string, Topic>();
        }
    }

    public class Message
    {
        public string Id { get; private set; }
        public string Payload { get; private set; }
        public string Type { get; private set; }

        public Message(string id, string type, string payload)
        {
            this.Id = id;
            this.Type = type;
            this.Payload = payload;
        }
    }

    public interface ISubscriber
    {
        void Handle(Message message);
    }

    public class Topic
    {
        private volatile bool closed;
        private Thread dispatcherThread;
        private ConcurrentQueue<Message> queue;
        private HashSet<ISubscriber> subscribers;

        public void Close()
        {
            while (queue.Count > 0)
            {
                Thread.Sleep(100);
            }

            closed = true;

            dispatcherThread.Abort();
        }

        public string Name { get; private set; }

        public void Publish(Message message)
        {
            queue.Enqueue(message);
        }

        public void Subscribe(ISubscriber subscriber)
        {
            lock (subscribers)
            {
                subscribers.Add(subscriber);
            }
        }

        internal Topic(string name)
        {
            this.Name = name;
            this.closed = false;
            this.dispatcherThread = new Thread(new ThreadStart(this.DispatchEach));
            this.queue = new ConcurrentQueue<Message>();
            this.subscribers = new HashSet<ISubscriber>();

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
                        Message message;
                        if (queue.TryDequeue(out message))
                        {
                            foreach (ISubscriber subscriber in subscribers)
                            {
                                try
                                {
                                    subscriber.Handle(message);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error dispatching message to handler: " + e.Message);
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
