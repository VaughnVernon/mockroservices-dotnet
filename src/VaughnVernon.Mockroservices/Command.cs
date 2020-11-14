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
    public abstract class Command : ISourceType
    {
		public long OccurredOn { get; }
		public int CommandVersion { get; }

		public static Command Null = new NullCommand();

		public static List<Command> All(params Command[] commands) => All(new List<Command>(commands));

		public static List<Command> All(List<Command> commands)
		{
			var all = new List<Command>(commands.Count);

			foreach (var command in commands)
			{
				if (!command.IsNull())
				{
					all.Add(command);
				}
			}
			return all;
		}

		public static List<Command> None() => new List<Command>(0);

		public virtual bool IsNull() => false;

		protected Command()
			: this(1)
		{
		}

		protected Command(int commandVersion)
		{
			OccurredOn = DateTime.Now.Ticks;
			CommandVersion = commandVersion;
		}

		internal class NullCommand : Command
		{
			public override bool IsNull() => true;
		}
	}
}
