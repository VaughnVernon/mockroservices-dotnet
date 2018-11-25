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
    public class RepositoryTest
    {
        [TestMethod]
        public void TestSaveFind()
        {
            PersonRepository repository = new PersonRepository();
            PersonES person = new PersonES("Joe", 40, DateTime.Now);
            repository.Save(person);
            PersonES joe = repository.PersonOfId(person.Id);
            Console.WriteLine("PERSON: " + person);
            Console.WriteLine("JOE: " + joe);
            Assert.AreEqual(person, joe);
        }
    }

    public class PersonRepository : EventSourceRepository
    {
        private EventJournal journal;
        private EventStreamReader reader;

        public PersonES PersonOfId(string id)
        {
            EventStream stream = reader.StreamFor(id);

            return new PersonES(ToEvents(stream.Stream), stream.StreamVersion);
        }

        public void Save(PersonES person)
        {
            journal.Write(person.Id, person.NextVersion, ToBatch(person.Applied));
        }

        internal PersonRepository()
        {
            this.journal = EventJournal.Open("repo-test");
            this.reader = this.journal.StreamReader();
        }
    }
}
