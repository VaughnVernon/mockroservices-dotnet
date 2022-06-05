//   Copyright © 2022 Vaughn Vernon. All rights reserved.
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
using System.Diagnostics.CodeAnalysis;

namespace VaughnVernon.Mockroservices
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class KVStore
	{
		private static readonly Dictionary<string, KVStore> stores = new();

		public string Name { get; }
		private Dictionary<string, string> Store { get; }

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

		public string Get(string key) => Store[key];

		public void Put(string key, string value) => Store.Add(key, value);

		private KVStore(string name)
		{
			Name = name;
			Store = new Dictionary<string, string>();
		}
	}
}
