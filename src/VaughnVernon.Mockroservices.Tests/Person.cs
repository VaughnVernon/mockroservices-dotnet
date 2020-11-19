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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VaughnVernon.Mockroservices.Tests
{
    public class Person
    {
        public DateTime BirthDate { get; }
        public string Name { get; }

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != typeof(Person))
            {
                return false;
            }

            var otherPerson = (Person)other;

            return Name.Equals(otherPerson.Name) && BirthDate.Equals(otherPerson.BirthDate);
        }

        public override int GetHashCode() => Name.GetHashCode() + BirthDate.GetHashCode();

        public Person(string name, DateTime birthDate)
        {
            Name = name;
            BirthDate = birthDate;
        }
    }

    public class ClientPerson
    {
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
    }

    public class ClientPersonWithPrivateSetter
    {
        public string Name { get; private set; }
        public DateTime BirthDate { get; private set; }

        public ClientPersonWithPrivateSetter() { }

        public static ClientPersonWithPrivateSetter Instance(
            string name, DateTime birthDate) =>
            new ClientPersonWithPrivateSetter(
                name, birthDate);

        private ClientPersonWithPrivateSetter(string name, DateTime birthDate)
        {
            Name = name;
            BirthDate = birthDate;
        }
    }

    public class PersonES : SourcedEntity<DomainEvent>
    {
        [JsonConstructor]
        public PersonES(string name, int age, DateTime birthDate) => Apply(new PersonNamed(Guid.NewGuid().ToString(), name, age, birthDate));

        public PersonES(IEnumerable<DomainEvent> stream, int streamVersion)
            : base(stream, streamVersion)
        {
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public int Age { get; private set; }
        public DateTime BirthDate { get; private set; }

        public void When(PersonNamed personNamed)
        {
            Id = personNamed.Id;
            Name = personNamed.Name;
            Age = personNamed.Age;
        }

        public override bool Equals(object other)
        {
            if (other != null && other.GetType() != typeof(PersonES))
            {
                return false;
            }

            var otherPerson = (PersonES)other;

            return Id.Equals(otherPerson.Id) && Name.Equals(otherPerson.Name) && Age == otherPerson.Age;
        }

        public override int GetHashCode() => 31 * Id.GetHashCode() * Name.GetHashCode() * Age.GetHashCode();

        public override string ToString() => $"PersonES [Id={Id} Name={Name} Age={Age}]";
    }

    public class PersonNamed : DomainEvent
    {
        public string Id { get; }
        public string Name { get; }
        public int Age { get; }
        public DateTime BirthDate { get; }

        public PersonNamed(string id, string name, int age, DateTime birthDate)
        {
            Id = id;
            Name = name;
            Age = age;
            BirthDate = birthDate;
        }
    }
}
