using System;
using System.Collections.Generic;

namespace DotNetX.Repl.Builder
{
    public interface INamedBuilder : IDescribableBuilder
    {
        IEnumerable<string> Names { get; }
    }

    public abstract class NamedBuilder<TBuilder> :
        DescribableBuilder<TBuilder>,
        INamedBuilder
        where TBuilder: NamedBuilder<TBuilder>
    {
        internal List<string> names = new List<string>();

        public IEnumerable<string> Names => names;

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
