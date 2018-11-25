using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VaughnVernon.Mockroservices.Tests
{
	[TestClass]
	public class KVStoreTest
	{
		[TestMethod]
		public void TestPutGet()
		{
			string key = "k1";
			string value = "v1";

			string name = "test";
			KVStore store = KVStore.Open(name);

			store.Put(key, value);

			Assert.AreEqual(name, store.Name);
			Assert.AreEqual(value, store.get(key));
		}
	}
}
