using System;
using System.Collections.Generic;

namespace DotNetX.Repl.Builder
{
    public interface IDescribableBuilder
    {
        string Caption { get; }
        string Description { get; }
        IEnumerable<ReplExampleBuilder> Examples { get; }
    }

    public abstract class DescribableBuilder<TBuilder> :
        IDescribableBuilder
        where TBuilder: DescribableBuilder<TBuilder>
    {
        internal string caption;
        internal string description;
        internal List<ReplExampleBuilder> examples = new List<ReplExampleBuilder>();

        public string Caption => caption;

        public string Description => description;

        public IEnumerable<ReplExampleBuilder> Examples => examples;

        public TBuilder WithCaption(string caption)
        {
            this.caption = caption;
            return (TBuilder)this;
        }

        public TBuilder WithDescription(string description)
        {
            this.description = description;
            return (TBuilder)this;
        }

        public TBuilder WithExample(string command, Action<ReplExampleBuilder> configExample = null)
        {
            var exampleBuilder = new ReplExampleBuilder().WithCommand(command);
            configExample?.Invoke(exampleBuilder);
            examples.Add(exampleBuilder);
            return (TBuilder)this;
        }
    }
}
