using System;
using System.Collections.Generic;

namespace DotNetX.Repl.Builder
{
    public abstract class NamedBuilder<TBuilder> :
        DescribableBuilder<TBuilder>
        where TBuilder: NamedBuilder<TBuilder>
    {
        internal List<string> names = new List<string>();

        public TBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            this.names.Add(name.Trim());
            return (TBuilder)this;
        }
    }
}
