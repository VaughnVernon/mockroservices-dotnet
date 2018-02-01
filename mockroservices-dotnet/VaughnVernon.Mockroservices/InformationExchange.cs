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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VaughnVernon.Mockroservices
{
    public abstract class InformationExchangeReader
    {
        public InformationExchangeReader(string representation)
        {
            this.Representation = Parse(representation);
        }

        protected dynamic Parse(string representation)
        {
            return JsonConvert.DeserializeObject(representation, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        protected dynamic Representation { get; private set; }

        public long LongValue(string key)
        {
            return Representation[key];
        }

        protected string StringValue(string key)
        {
            return Representation[key];
        }
    }

    public class MessageExchangeReader : InformationExchangeReader
    {
        public static MessageExchangeReader From(Message message)
        {
            return new MessageExchangeReader(message);
        }

        public MessageExchangeReader(Message message)
            : base(message.Payload)
        {
            DynamicPayload = JsonConvert.DeserializeObject(message.Payload, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            Message = message;
        }

        public string Id
        {
            get
            {
                return Message.Id;
            }
        }

        public long IdAsLong
        {
            get
            {
                return long.Parse(Message.Id);
            }
        }

        public string Type
        {
            get
            {
                return Message.Type;
            }
        }

        public bool PayloadBooleanValue(string key)
        {
            string stringValue = PayloadStringValue(key);
            return stringValue == null ? false : Convert.ToBoolean(stringValue);
        }

        public DateTime PayloadDateTimeValue(string key)
        {
            long longValue = PayloadLongValue(key);
            return new DateTime(longValue);
        }

        public double PayloadDoubleValue(string key)
        {
            return DynamicPayload[key];
        }

        public int PayloadIntegerValue(string key)
        {
            return DynamicPayload[key];
        }

        public long PayloadLongValue(string key)
        {
            return DynamicPayload[key];
        }

        public string PayloadStringValue(string key)
        {
            return DynamicPayload[key];
        }

        protected dynamic DynamicPayload { get; private set; }
        protected Message Message { get; private set; }
    }
}
