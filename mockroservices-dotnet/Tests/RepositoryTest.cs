using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace VaughnVernon.Mockroservices.Tests
{
    [TestClass]
    public class RepositoryTest
    {
        [TestMethod]
        public void TestSaveFind()
        {
            PersonRepository repository = new PersonRepository();
            Person person = new Person("Joe", 30);
            repository.Save(person);
            Person joe = repository.PersonOfId(person.Id);
            Console.WriteLine("PERSON: " + person);
            Console.WriteLine("JOE: " + joe);
            Assert.AreEqual(person, joe);
        }
    }

    public class Person : EventSourcedRootEntity
    {
        public Person(string name, int age)
        {
            Apply(new PersonNamed(Guid.NewGuid().ToString(), name, age));
        }

        public Person(List<DomainEvent> stream, int streamVersion)
            : base(stream, streamVersion)
        {
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public int Age { get; private set; }

        public void When(PersonNamed personNamed)
        {
            Id = personNamed.Id;
            Name = personNamed.Name;
            Age = personNamed.Age;
        }

        public override bool Equals(object other)
        {
            if (other != null && other.GetType() != typeof(Person))
            {
                return false;
            }

            Person otherPerson = (Person)other;

            return this.Id.Equals(otherPerson.Id) && this.Name.Equals(otherPerson.Name) && this.Age == otherPerson.Age;
        }

        public override string ToString()
        {
            return "Person [Id=" + Id + " Name=" + Name + " Age=" + Age + "]";
        }
    }

    public class PersonNamed : DomainEvent
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public int Age { get; private set; }

        public PersonNamed(string id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }
    }

    public class PersonRepository : EventSourceRepository
    {
        private EventJournal journal;
        private EventStreamReader reader;

        public Person PersonOfId(string id)
        {
            EventStream stream = reader.StreamFor(id);

            return new Person(ToEvents(stream.Stream), stream.StreamVersion);
        }

        public void Save(Person person)
        {
            journal.Write(person.Id, person.MutatedVersion, ToBatch(person.MutatingEvents));
        }

        internal PersonRepository()
        {
            this.journal = EventJournal.Open("repo-test");
            this.reader = this.journal.StreamReader();
        }
    }
}
