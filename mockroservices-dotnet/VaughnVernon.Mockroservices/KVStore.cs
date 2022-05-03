using System;
using System.Collections.Generic;

namespace VaughnVernon.Mockroservices
{
	public class KVStore
	{
		private static Dictionary<string, KVStore> stores = new Dictionary<string, KVStore>();

		public string Name { get; private set; }
		private Dictionary<string, string> Store { get; set; }

		public static KVStore Open(string name)
		{
			try
			{
				KVStore openStore = stores[name];
				return openStore;
			}
			catch (Exception)
			{
				// ignore
			}

			KVStore store = new KVStore(name);

			stores.Add(name, store);

			return store;
		}

		public string get(string key)
		{
			return Store[key];
		}

		public void Put(string key, string value)
		{
			Store.Add(key, value);
		}

		private KVStore(string name)
		{
			this.Name = name;
			this.Store = new Dictionary<string, string>();
		}
	}
}
