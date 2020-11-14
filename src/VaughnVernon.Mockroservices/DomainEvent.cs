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

namespace VaughnVernon.Mockroservices
{
    public abstract class DomainEvent : ISourceType
    {
		public DateTimeOffset OccurredOn { get; }
		
		public DateTimeOffset ValidOn { get; }
		
		public int Version { get; }

		public static DomainEvent Null = new NullDomainEvent();

		public static List<DomainEvent> All(params DomainEvent[] domainEvents) => All(new List<DomainEvent>(domainEvents));

		public static List<DomainEvent> All(List<DomainEvent> domainEvents)
		{
			var all = new List<DomainEvent>(domainEvents.Count);

			foreach (var domainEvent in domainEvents)
			{
				if (!domainEvent.IsNull())
				{
					all.Add(domainEvent);
				}
			}
			return all;
		}

		public static List<DomainEvent> None() => new List<DomainEvent>(0);

		public virtual bool IsNull() => false;

		protected DomainEvent()
			: this(0)
		{
		}

		protected DomainEvent(int version)
		{
			OccurredOn = DateTimeOffset.Now;
			ValidOn = DateTimeOffset.Now;
			Version = version;
		}
		
		protected DomainEvent(DateTimeOffset validOn, int version)
		{
			OccurredOn = DateTimeOffset.Now;
			ValidOn = validOn;
			Version = version;
		}

		internal class NullDomainEvent : DomainEvent
		{
			public override bool IsNull() => true;
		}
	}
}