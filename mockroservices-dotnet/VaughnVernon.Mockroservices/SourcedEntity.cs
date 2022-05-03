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
    public abstract class SourcedEntity<Source>
    {
        private readonly List<Source> applied;
        private readonly int currentVersion;

        public List<Source> Applied
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
            this.applied = new List<Source>();
            this.currentVersion = 0;
        }

        protected SourcedEntity(List<Source> stream, int streamVersion)
            : this()
        {
            foreach (Source source in stream)
            {
                DispatchWhen(source);
            }

            this.currentVersion = streamVersion;
        }

        protected void Apply(Source source)
        {
            applied.Add(source);
            DispatchWhen(source);
        }

        protected void DispatchWhen(Source source)
        {
            ((dynamic) this).When((dynamic)source);
        }
    }
}
