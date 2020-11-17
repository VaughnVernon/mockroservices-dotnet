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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VaughnVernon.Mockroservices.Tests
{
    [TestClass]
    public class RepositoryTest
    {
        [TestMethod]
        public void TestSaveFind()
        {
            var repository = new PersonRepository();
            var person = new PersonES("Joe", 40, DateTime.Now);
            repository.Save(person);
            var joe = repository.PersonOfId(person.Id);
            Console.WriteLine($"PERSON: {person}");
            Console.WriteLine($"JOE: {joe}");
            Assert.AreEqual(person, joe);
        }
        
        [TestMethod]
        public void TestSaveWithStreamPrefixAndFind()
        {
            var repository = new PersonRepository();
            var person = new PersonES("Joe", 40, DateTime.Now);
            repository.Save<PersonES>(person);
            var joe = repository.PersonOfId(StreamNameBuilder.BuildStreamNameFor<PersonES>(person.Id));
            Console.WriteLine($"PERSON: {person}");
            Console.WriteLine($"JOE: {joe}");
            Assert.AreEqual(person, joe);
        }
        
        [TestMethod]
        public void TestSaveFindWithStreamPrefix()
        {
            var repository = new PersonRepository();
            var person = new PersonES("Joe", 40, DateTime.Now);
            repository.Save<PersonES>(person);
            var joe = repository.PersonOfId<PersonES>(person.Id);
            Console.WriteLine($"PERSON: {person}");
            Console.WriteLine($"JOE: {joe}");
            Assert.AreEqual(person, joe);
        }
    }

    public class PersonRepository : Repository
    {
        private readonly Journal journal;
        private readonly EntryStreamReader reader;

        public PersonES PersonOfId(string id)
        {
            var stream = reader.StreamFor(id);

            return new PersonES(ToSourceStream<DomainEvent>(stream.Stream), stream.StreamVersion);
        }
        
        public PersonES PersonOfId<T>(string id)
        {
            var stream = reader.StreamFor<T>(id);

            return new PersonES(ToSourceStream<DomainEvent>(stream.Stream), stream.StreamVersion);
        }

        public void Save(PersonES person) => journal.Write(person.Id, person.CurrentVersion, ToBatch(person.Applied));

        public void Save<T>(T person) where T : PersonES => journal.Write<T>(person.Id, person.CurrentVersion, ToBatch(person.Applied));

        internal PersonRepository()
        {
            journal = Journal.Open("repo-test");
            reader = journal.StreamReader();
        }
    }
}
