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

public class EntryValue
{
    public const int NoStreamVersion = -1;

    public string Body { get; }
    public string Snapshot { get; }
    public string StreamName { get; }
    public int StreamVersion { get; }
    public string Type { get; }

    public bool HasSnapshot() => Snapshot.Length > 0;

    public override int GetHashCode() => StreamName.GetHashCode() + StreamVersion;

    public override bool Equals(object? other)
    {
        if (other == null || other.GetType() != typeof(EntryValue))
        {
            return false;
        }

        var otherEntryValue = (EntryValue)other;

        return StreamName.Equals(otherEntryValue.StreamName) &&
               StreamVersion == otherEntryValue.StreamVersion &&
               Type.Equals(otherEntryValue.Type) &&
               Body.Equals(otherEntryValue.Body) &&
               Snapshot.Equals(otherEntryValue.Snapshot);
    }

    public override string ToString() => $"EntryValue[streamName={StreamName} StreamVersion={StreamVersion} type={Type} body={Body} snapshot={Snapshot}]";

    internal EntryValue(
        string streamName,
        int streamVersion,
        string type,
        string body,
        string snapshot)
    {

        StreamName = streamName;
        StreamVersion = streamVersion;
        Type = type;
        Body = body;
        Snapshot = snapshot;
    }

    internal EntryValue WithStreamVersion(int streamVersion) => new(StreamName, streamVersion, Type, Body, Snapshot);
}