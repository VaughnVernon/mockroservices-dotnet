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

namespace VaughnVernon.Mockroservices.Journals;

public class EntryStreamReader
{
    private readonly Journal journal;

    public EntryStream StreamFor(string streamName) => journal.ReadStream(streamName);

    public EntryStream StreamFor<T>(string streamName) => journal.ReadStream(StreamNameBuilder.BuildStreamNameFor<T>(streamName));

    internal EntryStreamReader(Journal journal) => this.journal = journal;
}