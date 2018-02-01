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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VaughnVernon.Mockroservices
{
    [TestClass]
    public class MessageExchangeReaderTest
    {
        [TestMethod]
        public void TestExchangeRoundTrip()
        {
            TestableDomainEvent domainEvent = new TestableDomainEvent(1, "something");
            string serializedDomainEvent = Serialization.Serialize(domainEvent);
            Message eventMessage = new Message(Convert.ToString(domainEvent.Id), domainEvent.GetType().Name, serializedDomainEvent);
            MessageExchangeReader reader = MessageExchangeReader.From(eventMessage);
            Assert.AreEqual(eventMessage.Id, reader.Id);
            Assert.AreEqual(eventMessage.Type, reader.Type);
            Assert.AreEqual(domainEvent.Id, reader.IdAsLong);
            Assert.AreEqual(domainEvent.Name, reader.PayloadStringValue("name"));
            Assert.AreEqual(domainEvent.EventVersion, reader.PayloadIntegerValue("eventVersion"));
            Assert.AreEqual(domainEvent.OccurredOn, reader.PayloadLongValue("occurredOn"));
        }
    }
}
