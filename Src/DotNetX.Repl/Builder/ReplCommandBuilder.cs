using System.Collections.Generic;

namespace DotNetX.Repl.Builder
{
    public class ReplCommandBuilder : NamedBuilder<ReplCommandBuilder>
    {
        internal string category;
        internal List<object> parameters = new List<object>();

        public ReplCommandBuilder WithFlag(string name)
        {
            var parameterBuilder = new ReplCommandFlagParameterBuilder().WithName(name);
            this.names.Add(name.Trim());
            return this;
        }

        public ReplCommandBuilder WithCategory(string category)
        {
            this.category = category;
            return this;
        }
    }
}
