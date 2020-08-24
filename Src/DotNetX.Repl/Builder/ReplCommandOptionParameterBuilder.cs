using System;

namespace DotNetX.Repl.Builder
{
    public class ReplCommandOptionParameterBuilder : 
        ReplCommandParameterBuilder<ReplCommandOptionParameterBuilder>
    {
        private string typeName;
        private bool isRequired;
        private bool isRepeated;
        private int valueCount = 1;

        public string TypeName => typeName;
        public bool IsRequired => isRequired;
        public bool IsRepeated => isRepeated;
        public int ValueCount => valueCount;

        public ReplCommandOptionParameterBuilder WithTypeName(string typeName)
        {
            this.typeName = typeName;
            return this;
        }

        public ReplCommandOptionParameterBuilder WithIsRequired()
        {
            this.isRequired = true;
            return this;
        }

        public ReplCommandOptionParameterBuilder WithIsRepeated()
        {
            this.isRepeated = true;
            return this;
        }

        public ReplCommandOptionParameterBuilder WithValueCount(int valueCount)
        {
            if (valueCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(valueCount), "Options value count must be a positive integer");
            }

            this.valueCount = valueCount;
            return this;
        }
    }
}
