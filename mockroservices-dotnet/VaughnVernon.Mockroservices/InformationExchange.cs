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
using System.Web.Script.Serialization;

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
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<dynamic>(representation);
        }

        protected dynamic Representation { get; private set; }

        protected string StringValue(string key)
        {
            return Representation[key];
        }
    }

    public class MessageExchangeReader : InformationExchangeReader
    {
        public static MessageExchangeReader From(string representation)
        {
            return new MessageExchangeReader(representation);
        }

        public MessageExchangeReader(string representation)
            : base(representation)
        {
            DynamicPayload = new JavaScriptSerializer().Deserialize<dynamic>(Payload);
        }

        public string Id
        {
            get
            {
                string id = StringValue("Id");
                return id;
            }
        }

        public long IdAsLong
        {
            get
            {
                string id = StringValue("Id");
                return Convert.ToInt64(id);
            }
        }

        public string Type
        {
            get
            {
                string type = StringValue("Type");
                return type;
            }
        }

        public bool PayloadBooleanValue(string key)
        {
            string stringValue = PayloadStringValue(key);
            return stringValue == null ? false : Convert.ToBoolean(stringValue);
        }

        public DateTime PayloadDateTimeValue(string key)
        {
            string stringValue = PayloadStringValue(key);
            return DateTime.Parse(stringValue);
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

        protected string Payload
        {
            get
            {
                string type = StringValue("Payload");
                return type;
            }
        }
    }
}
