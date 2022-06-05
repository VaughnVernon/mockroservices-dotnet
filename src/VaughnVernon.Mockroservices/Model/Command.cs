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
using System.Linq;

namespace VaughnVernon.Mockroservices.Model
{
    public abstract class Command : ISourceType
    {
	    public string Id { get; }
	    
		public DateTimeOffset OccurredOn { get; }
		
		public DateTimeOffset ValidOn { get; }
		
		public int Version { get; }

		public static Command Null = new NullCommand();

		public static IEnumerable<Command> All(params Command[] commands) => All(commands.AsEnumerable());

		public static IEnumerable<Command> All(IEnumerable<Command> commands) => commands.Where(c => !c.IsNull());

		public static IEnumerable<Command> None() => Enumerable.Empty<Command>();

		public virtual bool IsNull() => false;

		protected Command(string id) : this(id, 0)
		{
		}

		protected Command(string id, int version)
		{
			Id = id;
			OccurredOn = DateTimeOffset.Now;
			ValidOn = DateTimeOffset.Now;
			Version = version;
		}
		
		protected Command(string id, DateTimeOffset validOn, int version)
		{
			Id = id;
			OccurredOn = DateTimeOffset.Now;
			ValidOn = validOn;
			Version = version;
		}

		internal class NullCommand : Command
		{
			public NullCommand() : base(string.Empty)
			{
			}
			
			public override bool IsNull() => true;
		}
	}
}
