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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VaughnVernon.Mockroservices.Model;

namespace VaughnVernon.Mockroservices.Tests.Model
{
    [TestClass]
    public class EventSourcedRootEntityTest
    {
        [TestMethod]
        public void TestProductDefinedEventKept()
        {
            var id = Guid.NewGuid().ToString();
            var product = new Product(id, "dice-fuz-1", "Fuzzy dice.", 999);
            Assert.AreEqual(1, product.Applied.Count);
            Assert.AreEqual("dice-fuz-1", product.Name);
            Assert.AreEqual("Fuzzy dice.", product.Description);
            Assert.AreEqual(999, product.Price);
            Assert.AreEqual(new ProductDefined(id, "dice-fuz-1", "Fuzzy dice.", 999), product.Applied[0]);
        }

        [TestMethod]
        public void TestProductNameChangedEventKept()
        {
            var product = new Product(Guid.NewGuid().ToString(), "dice-fuz-1", "Fuzzy dice.", 999);

            product.Applied.Clear();

            product.ChangeName("dice-fuzzy-1");
            Assert.AreEqual(1, product.Applied.Count);
            Assert.AreEqual("dice-fuzzy-1", product.Name);
            Assert.AreEqual(new ProductNameChanged("dice-fuzzy-1"), product.Applied[0]);
        }

        [TestMethod]
        public void TestProductDescriptionChangedEventsKept()
        {
            var product = new Product(Guid.NewGuid().ToString(), "dice-fuz-1", "Fuzzy dice.", 999);

            product.Applied.Clear();

            product.ChangeDescription("Fuzzy dice, and all.");
            Assert.AreEqual(1, product.Applied.Count);
            Assert.AreEqual("Fuzzy dice, and all.", product.Description);
            Assert.AreEqual(new ProductDescriptionChanged("Fuzzy dice, and all."), product.Applied[0]);
        }

        [TestMethod]
        public void TestProductPriceChangedEventKept()
        {
            var product = new Product(Guid.NewGuid().ToString(), "dice-fuz-1", "Fuzzy dice.", 999);

            product.Applied.Clear();

            product.ChangePrice(995);
            Assert.AreEqual(1, product.Applied.Count);
            Assert.AreEqual(995, product.Price);
            Assert.AreEqual(new ProductPriceChanged(995), product.Applied[0]);
        }

        [TestMethod]
        public void TestReconstitution()
        {
            var product = new Product(Guid.NewGuid().ToString(), "dice-fuz-1", "Fuzzy dice.", 999);
            product.ChangeName("dice-fuzzy-1");
            product.ChangeDescription("Fuzzy dice, and all.");
            product.ChangePrice(995);

            var productAgain = new Product(product.Applied, product.NextVersion);
            Assert.AreEqual(product, productAgain);
        }
    }

    public class Product : SourcedEntity<DomainEvent>
    {
        public string Id { get; private set; } 
        public string Name { get; private set; }
        public string Description { get; private set; }
        public long Price { get; private set; }

        public Product(string id, string name, string description, long price)
            => Apply(new ProductDefined(id, name, description, price));
        
        public Product(string id, string name, string description, long price, DateTimeOffset validOn)
            => Apply(new ProductDefined(id, name, description, price, validOn));

        public Product(IEnumerable<DomainEvent> eventStream, int streamVersion)
            : base(eventStream, streamVersion)
        {
        }

        public void ChangeDescription(string description) => Apply(new ProductDescriptionChanged(description));
        
        public void ChangeDescription(string description, DateTimeOffset validOn) => Apply(new ProductDescriptionChanged(description, validOn));

        public void ChangeName(string name) => Apply(new ProductNameChanged(name));
        
        public void ChangeName(string name, DateTimeOffset validOn) => Apply(new ProductNameChanged(name, validOn));

        public void ChangePrice(long price) => Apply(new ProductPriceChanged(price));
        
        public void ChangePrice(long price, DateTimeOffset validOn) => Apply(new ProductPriceChanged(price, validOn));

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != typeof(Product))
            {
                return false;
            }

            var otherProduct = (Product)other;

            return Name.Equals(otherProduct.Name) &&
                Description.Equals(otherProduct.Description) &&
                Price == otherProduct.Price;
        }

        public override int GetHashCode() => Id.GetHashCode() + Name.GetHashCode();

        public void When(ProductDefined e)
        {
            Id = e.Id;
            Name = e.Name;
            Description = e.Description;
            Price = e.Price;
        }

        public void When(ProductDescriptionChanged e) => Description = e.Description;

        public void When(ProductNameChanged e) => Name = e.Name;

        public void When(ProductPriceChanged e) => Price = e.Price;
    }

    public class ProductDefined : DomainEvent
    {
        public string Id { get; }
        public string Description { get; }
        public string Name { get; }
        public long Price { get; }

        public ProductDefined(string id, string name, string description, long price)
            : this(id, name, description, price, DateTimeOffset.Now)
        {
        }

        [JsonConstructor]
        public ProductDefined(string id, string name, string description, long price, DateTimeOffset validOn)
            : base(validOn, 0)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
        }

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != typeof(ProductDefined))
            {
                return false;
            }

            var otherProductDefined = (ProductDefined)other;

            return Id.Equals(otherProductDefined.Id) &&
                Name.Equals(otherProductDefined.Name) &&
                Description.Equals(otherProductDefined.Description) &&
                Price == otherProductDefined.Price &&
                Version == otherProductDefined.Version;
        }

        public override int GetHashCode() => Id.GetHashCode() + Name.GetHashCode() + Description.GetHashCode() + (int)Price;
    }

    public class ProductDescriptionChanged : DomainEvent
    {
        public string Description { get; }

        public ProductDescriptionChanged(string description) : this(description, DateTimeOffset.Now)
        {
        }
        
        [JsonConstructor]
        public ProductDescriptionChanged(string description, DateTimeOffset validOn) : base (validOn, 0)
            => Description = description;

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != typeof(ProductDescriptionChanged))
            {
                return false;
            }

            var otherProductDescriptionChanged = (ProductDescriptionChanged)other;

            return Description.Equals(otherProductDescriptionChanged.Description) &&
                Version == otherProductDescriptionChanged.Version;
        }

        public override int GetHashCode() => Description.GetHashCode();
    }

    public class ProductNameChanged : DomainEvent
    {
        public string Name { get; }

        public ProductNameChanged(string name) : this(name, DateTimeOffset.Now)
        {
        }
        
        [JsonConstructor]
        public ProductNameChanged(string name, DateTimeOffset validOn) : base(validOn, 0)
            => Name = name;

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != typeof(ProductNameChanged))
            {
                return false;
            }

            var otherProductNameChanged = (ProductNameChanged)other;

            return Name.Equals(otherProductNameChanged.Name) &&
                Version == otherProductNameChanged.Version;
        }

        public override int GetHashCode() => Name.GetHashCode();
    }

    public class ProductPriceChanged : DomainEvent
    {
        public long Price { get; }

        public ProductPriceChanged(long price) : this(price, DateTimeOffset.Now)
        {
        }
        
        [JsonConstructor]
        public ProductPriceChanged(long price, DateTimeOffset validOn) : base(validOn, 0)
            => Price = price;

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != typeof(ProductPriceChanged))
            {
                return false;
            }

            var otherProductPriceChanged = (ProductPriceChanged)other;

            return Price == otherProductPriceChanged.Price &&
                Version == otherProductPriceChanged.Version;
        }

        public override int GetHashCode() => (int)Price;
    }
}
