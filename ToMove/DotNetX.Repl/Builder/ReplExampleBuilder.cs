namespace DotNetX.Repl.Builder
{
    public class ReplExampleBuilder
    {
        internal string? caption;
        internal string? command;
        internal string? description;

        public ReplExampleBuilder WithCommand(string? command)
        {
            this.command = command;
            return this;
        }

        public ReplExampleBuilder WithCaption(string? caption)
        {
            this.caption = caption;
            return this;
        }

        public ReplExampleBuilder WithDescription(string? description)
        {
            this.description = description;
            return this;
        }
    }
}
