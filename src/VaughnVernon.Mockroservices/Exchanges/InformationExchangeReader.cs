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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace VaughnVernon.Mockroservices.Exchanges
{
    public abstract class InformationExchangeReader
    {
        public InformationExchangeReader(string representation) => Representation = Parse(representation);

        protected dynamic? Parse(string representation)
        {
            return JsonConvert.DeserializeObject(representation, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        protected dynamic? Representation { get; }

        public long LongValue(string key) => Representation?[key];

        protected string? StringValue(string key) => Representation?[key];
    }

    public class MessageExchangeReader : InformationExchangeReader
    {
        public static MessageExchangeReader From(Message message) => new(message);

        public MessageExchangeReader(Message message)
            : base(message.Payload)
        {
            DynamicPayload = JsonConvert.DeserializeObject(message.Payload, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            Message = message;
        }

        public string Id => Message.Id;

        public long IdAsLong => long.Parse(Message.Id);

        public string Type => Message.Type;

        public bool PayloadBooleanValue(string key)
        {
            var stringValue = PayloadStringValue(key);
            return stringValue == null ? false : Convert.ToBoolean(stringValue);
        }

        public DateTime PayloadDateTimeValue(string key) => new(PayloadLongValue(key));

        public double PayloadDoubleValue(string key) => DynamicPayload?[key];

        public int PayloadIntegerValue(string key) => DynamicPayload?[key];

        public long PayloadLongValue(string key) => DynamicPayload?[key];
        
        public DateTimeOffset PayloadDateTimeOffsetValue(string key) => DynamicPayload?[key];

        public string? PayloadStringValue(string key) => DynamicPayload?[key];

        public string[] PayloadStringArrayValue(string key)
        {
            if (!(DynamicPayload?[key] is JArray serializedArray) || serializedArray.Count == 0)
            {
                return new string[0];
            }

            var result = new string[serializedArray.Count];

            for (var i = 0; i < serializedArray.Count; i++)
            {
                result[i] = serializedArray[i].Value<string>();
            }
            return result;
        }

        protected dynamic? DynamicPayload { get; }
        protected Message Message { get; }
    }
}