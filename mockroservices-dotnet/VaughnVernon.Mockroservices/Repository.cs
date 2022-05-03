//   Copyright © 2017 Vaughn Vernon. All rights reserved.
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

namespace VaughnVernon.Mockroservices.VaughnVernon.Mockroservices
{
    public abstract class Repository
    {
        protected EntryBatch ToBatch<T>(List<T> sources)
        {
            EntryBatch batch = new EntryBatch(sources.Count);
            foreach (T source in sources)
            {
                string eventBody = Serialization.Serialize(source);
                batch.AddEntry(source.GetType().AssemblyQualifiedName, eventBody);
            }
            return batch;
        }

        protected List<T> ToSourceStream<T>(List<EntryValue> stream)
        {
            List<T> sourceStream = new List<T>(stream.Count);
            foreach (EntryValue value in stream)
            {
                Type sourceType = Type.GetType(value.Type);
                T source = (T)Serialization.Deserialize(value.Body, sourceType);
                sourceStream.Add(source);
            }
            return sourceStream;
        }
    }
}
