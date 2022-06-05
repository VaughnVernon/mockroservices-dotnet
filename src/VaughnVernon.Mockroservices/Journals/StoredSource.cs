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

public class StoredSource
{
    public const long NoId = -1L;

    public EntryValue EntryValue { get; }
    public long Id { get; }

    public bool IsValid() => Id != NoId;

    public override int GetHashCode() => EntryValue.GetHashCode() + (int)Id;

    public override bool Equals(object? other)
    {
        if (other == null || other.GetType() != typeof(StoredSource))
        {
            return false;
        }

        var otherStoredSource = (StoredSource)other;

        return Id == otherStoredSource.Id && EntryValue.Equals(otherStoredSource.EntryValue);
    }

    public override string ToString() => $"StoredSource[id={Id} entryValue={EntryValue}]";

    internal StoredSource(long id, EntryValue eventValue)
    {
        Id = id;
        EntryValue = eventValue;
    }
}