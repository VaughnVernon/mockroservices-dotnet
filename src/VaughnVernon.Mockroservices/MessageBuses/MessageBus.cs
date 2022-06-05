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

using System.Collections.Generic;

namespace VaughnVernon.Mockroservices.MessageBuses
{
    public class MessageBus
    {
        private static readonly Dictionary<string, MessageBus> MessageBuses = new();

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
}
