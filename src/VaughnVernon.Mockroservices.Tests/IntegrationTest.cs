using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VaughnVernon.Mockroservices.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void FromRepositoryToProjection()
        {
            var messageBus = MessageBus.Start("test-bus-product");
            var topic = messageBus.OpenTopic("cat-product");
            var journalPublisher = JournalPublisher.From("product-journal", messageBus.Name, topic.Name);
            var subscriber = new JournalPublisherTestSubscriber();
            topic.Subscribe(subscriber);
            
            var repository = new ProductRepository();
            
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
        
        public Product ProductOfId<T>(string id)
        {
            var stream = reader.StreamFor<T>(id);
            return new Product(ToSourceStream<DomainEvent>(stream.Stream), stream.StreamVersion);
        }

        public void Save(Product product) => journal.Write(product.Id, product.CurrentVersion, ToBatch(product.Applied));

        public void Save<T>(T product) where T : Product => journal.Write<T>(product.Id, product.CurrentVersion, ToBatch(product.Applied));

        internal ProductRepository()
        {
            journal = Journal.Open("product-journal");
            reader = journal.StreamReader();
        }
    }
}