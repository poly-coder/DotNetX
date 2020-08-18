namespace DotNetX.Repl.Builder
{
    public class ReplCommandPositionalParameterBuilder :
        ReplCommandParameterBuilder<ReplCommandPositionalParameterBuilder>
    {
        internal string typeName;
        internal bool isRequired;
        internal bool isRepeated;

        public string TypeName => typeName;

        public bool IsRequired => isRequired;

        public bool IsRepeated => isRepeated;

        public ReplCommandPositionalParameterBuilder WithTypeName(string typeName)
        {
            this.typeName = typeName;
            return this;
        }

        public ReplCommandPositionalParameterBuilder WithIsRequired()
        {
            this.isRequired = true;
            return this;
        }

        public ReplCommandPositionalParameterBuilder WithIsRepeated()
        {
            this.isRepeated = true;
            return this;
        }
    }
}
