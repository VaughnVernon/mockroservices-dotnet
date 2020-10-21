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

using System.Collections.Generic;

namespace VaughnVernon.Mockroservices
{
    public abstract class SourcedEntity<TSource>
    {
        private readonly List<TSource> applied;
        private readonly int currentVersion;

        public List<TSource> Applied
        {
            get { return this.applied; }
        }

        public int NextVersion
        {
            get { return this.currentVersion + 1; }
        }

        public int CurrentVersion
        {
            get { return this.currentVersion; }
        }

        protected SourcedEntity()
        {
            this.applied = new List<TSource>();
            this.currentVersion = 0;
        }

        protected SourcedEntity(List<TSource> stream, int streamVersion)
            : this()
        {
            foreach (TSource source in stream)
            {
                DispatchWhen(source);
            }

            this.currentVersion = streamVersion;
        }

        protected void Apply(TSource source)
        {
            applied.Add(source);
            DispatchWhen(source);
        }

        protected void DispatchWhen(TSource source)
        {
            ((dynamic) this).When((dynamic)source);
        }
    }
}
