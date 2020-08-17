using DotNetX.Repl.Runtime;
using System;
using System.Collections.Generic;

namespace DotNetX.Repl.Builder
{

    public class ReplBuilder : DescribableBuilder<ReplBuilder>
    {
        internal string version = "0.0";
        internal List<object> commands = new List<object>();

        public ReplBuilder WithVersion(string version)
        {
            this.version = version;
            return this;
        }

        public ReplBuilder WithCommand(string name, Action<ReplCommandBuilder> configCommand)
        {
            var commandBuilder = new ReplCommandBuilder().WithName(name);
            configCommand.Invoke(commandBuilder);
            commands.Add(commandBuilder);
            return this;
        }

        public IReplRuntime Build()
        {
            throw new NotImplementedException();
        }
    }
}
