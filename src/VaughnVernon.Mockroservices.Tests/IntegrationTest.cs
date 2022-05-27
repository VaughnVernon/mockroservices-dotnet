using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VaughnVernon.Mockroservices.Journals;

namespace VaughnVernon.Mockroservices.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void WriteReadBiTemporalEvents()
        {
            var repository = new ProductRepository("bi-product-journal");
            
            var y2018 = new DateTimeOffset(2018, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var y2019 = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var y2020 = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var id = Guid.NewGuid().ToString();
            var product = new Product(id, "dice-fuz-1", "Fuzzy dice.", 999, y2018);
            product.ChangeName("dice-fuzzy-1", y2020);
            product.ChangeDescription("Fuzzy dice, and all.", y2020);
            product.ChangePrice(995, y2019);
            product.ChangeName("dice-fizzy-1", y2019);
            
            repository.Save<Product>(product);

            var productAt2018 = repository.ProductOfId<Product>(id, y2018);
            
            Assert.AreEqual(4, productAt2018.CurrentVersion);
            Assert.AreEqual("dice-fuz-1", productAt2018.Name);
            Assert.AreEqual("Fuzzy dice.", productAt2018.Description);
            Assert.AreEqual(999, productAt2018.Price);
            
            var productAt2019 = repository.ProductOfId<Product>(id, y2019);
            
            Assert.AreEqual(4, productAt2019.CurrentVersion);
            Assert.AreEqual("dice-fizzy-1", productAt2019.Name);
            Assert.AreEqual("Fuzzy dice.", productAt2019.Description);
            Assert.AreEqual(995, productAt2019.Price);
            
            var productAt2020 = repository.ProductOfId<Product>(id, y2020);
            
            Assert.AreEqual(4, productAt2020.CurrentVersion);
            Assert.AreEqual("dice-fuzzy-1", productAt2020.Name);
            Assert.AreEqual("Fuzzy dice, and all.", productAt2020.Description);
            Assert.AreEqual(995, productAt2020.Price);
        }
        
        [TestMethod]
        public void FromRepositoryToProjection()
        {
            var messageBus = MessageBus.Start("test-bus-product");
            var topic = messageBus.OpenTopic("cat-product");
            var journalName = "product-journal";
            var journalPublisher = JournalPublisher.From(journalName, messageBus.Name, topic.Name);
            var subscriber = new JournalPublisherTestSubscriber();
            topic.Subscribe(subscriber);
            
            var repository = new ProductRepository(journalName);
            
            var product1 = new Product(Guid.NewGuid().ToString(), "dice-fuz-1", "Fuzzy dice.", 999);
            product1.ChangeName("dice-fuzzy-1");
            product1.ChangeDescription("Fuzzy dice, and all.");
            product1.ChangePrice(995);
            
            repository.Save<Product>(product1);
            
            var product2 = new Product(Guid.NewGuid().ToString(), "dice-fuz-2", "Fuzzy dice.", 999);
            product2.ChangeName("dice-fuzzy-2");
            product2.ChangeDescription("Fuzzy dice, and all 2.");
            product2.ChangePrice(1000);
            
            repository.Save<Product>(product2);
            
            subscriber.WaitForExpectedMessages(8);
            topic.Close();
            journalPublisher.Close();

            Assert.AreEqual(8, subscriber.HandledMessages.Count);
        }
    }
    
    public class ProductRepository : Repository
    {
        private readonly Journal journal;
        private readonly EntryStreamReader reader;

        public Product ProductOfId(string id)
        {
            var stream = reader.StreamFor(id);
            return new Product(ToSourceStream<DomainEvent>(stream.Stream), stream.StreamVersion);
        }
        
        public Product ProductOfId<T>(string id, DateTimeOffset validOn)
        {
            var stream = reader.StreamFor<T>(id);
            return new Product(ToSourceStream<DomainEvent>(stream.Stream, validOn), stream.StreamVersion);
        }

        public void Save(Product product) => journal.Write(product.Id, product.CurrentVersion, ToBatch(product.Applied));

        public void Save<T>(T product) where T : Product => journal.Write<T>(product.Id, product.CurrentVersion, ToBatch(product.Applied));

        internal ProductRepository(string journalName)
        {
            journal = Journal.Open(journalName);
            reader = journal.StreamReader();
        }
    }
}