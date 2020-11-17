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
using System.Linq;

namespace VaughnVernon.Mockroservices
{
    public abstract class Repository
    {
        protected EntryBatch ToBatch<T>(IEnumerable<T> sources)
        {
            var localSources = sources.ToList();
            var batch = new EntryBatch(localSources.Count);
            foreach (var source in localSources)
            {
                var eventBody = Serialization.Serialize(source);
                var assemblyQualifiedName = source?.GetType().AssemblyQualifiedName;
                if (!string.IsNullOrEmpty(assemblyQualifiedName))
                {
                    batch.AddEntry(assemblyQualifiedName!, eventBody);   
                }
            }
            return batch;
        }

        protected List<T> ToSourceStream<T>(IEnumerable<EntryValue> stream)
        {
            var entryValues = stream.ToList();
            var sourceStream = new List<T>(entryValues.Count);
            foreach (var value in entryValues)
            {
                var sourceType = Type.GetType(value.Type);
                if (sourceType != null)
                {
                    var source = (T)Serialization.Deserialize(value.Body, sourceType);
                    sourceStream.Add(source);   
                }
            }
            return sourceStream;
        }
    }
}
