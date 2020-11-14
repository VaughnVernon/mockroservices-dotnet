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
			Assert.AreEqual(value, store.Get(key));
		}
	}
}
