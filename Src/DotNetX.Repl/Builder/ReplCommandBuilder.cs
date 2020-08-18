using System;
using System.Collections.Generic;

namespace DotNetX.Repl.Builder
{
    public class ReplCommandBuilder : NamedBuilder<ReplCommandBuilder>
    {
        internal string category;
        internal List<IReplCommandParameterBuilder> parameters = new List<IReplCommandParameterBuilder>();

        public string Category => category;
        public IEnumerable<IReplCommandParameterBuilder> Parameters => parameters;

        public ReplCommandBuilder WithFlag(string name, Action<ReplCommandFlagParameterBuilder>? config = null)
        {
            var parameterBuilder = new ReplCommandFlagParameterBuilder().WithName(name);
            config?.Invoke(parameterBuilder);
            this.parameters.Add(parameterBuilder);
            return this;
        }

        public ReplCommandBuilder WithPositional(string name, Action<ReplCommandPositionalParameterBuilder>? config = null)
        {
            var parameterBuilder = new ReplCommandPositionalParameterBuilder().WithName(name);
            config?.Invoke(parameterBuilder);
            this.parameters.Add(parameterBuilder);
            return this;
        }

        public ReplCommandBuilder WithOption(string name, Action<ReplCommandOptionParameterBuilder>? config = null)
        {
            var parameterBuilder = new ReplCommandOptionParameterBuilder().WithName(name);
            config?.Invoke(parameterBuilder);
            this.parameters.Add(parameterBuilder);
            return this;
        }

        public ReplCommandBuilder WithCategory(string category)
        {
            this.category = category;
            return this;
        }
    }
}
