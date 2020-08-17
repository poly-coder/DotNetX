namespace DotNetX.Repl.Builder
{
    public class ReplCommandPositionalParameterBuilder :
        ReplCommandParameterBuilder<ReplCommandPositionalParameterBuilder>
    {
        internal bool rest;

        public ReplCommandPositionalParameterBuilder WithRest()
        {
            this.rest = true;
            return this;
        }
    }
}
