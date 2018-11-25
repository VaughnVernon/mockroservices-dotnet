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

namespace VaughnVernon.Mockroservices.Tests
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void TestSerializationDeserialization()
        {
            Person person1 = new Person("First Middle Last, Jr.", DateTime.Now);
            Assert.IsNotNull(person1);
            string jsonPerson1 = Serialization.Serialize(person1);
            Person person2 = Serialization.Deserialize<Person>(jsonPerson1);
            Assert.IsNotNull(person2);
            Assert.AreEqual(person1, person2);
            string jsonPerson2 = Serialization.Serialize(person2);
            Assert.AreEqual(jsonPerson1, jsonPerson2);
        }

        [TestMethod]
        public void TestDeserializationToClientClass()
        {
            Person person = new Person("First Middle Last, Jr.", DateTime.Now);
            string jsonPerson = Serialization.Serialize(person);
            ClientPerson clientPerson = Serialization.Deserialize<ClientPerson>(jsonPerson);
            Assert.AreEqual(person.Name, clientPerson.Name);
            Assert.AreEqual(person.BirthDate, clientPerson.BirthDate);
        }

        [TestMethod]
        public void TestDeserializationAgainstPrivateSetter()
        {
            Person person = new Person("First Middle Last, Jr.", DateTime.Now);
            string jsonPerson = Serialization.Serialize(person);
            var clientPerson = Serialization.Deserialize<ClientPersonWithPrivateSetter>(jsonPerson);
            Assert.AreEqual(person.Name, clientPerson.Name);
            Assert.AreEqual(person.BirthDate, clientPerson.BirthDate);
        }
    }
}
