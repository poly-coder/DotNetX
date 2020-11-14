using System;

namespace DotNetX.Repl.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class ReplParamAttribute : Attribute
    {
        public string? TypeName { get; set; }
        public string? Caption { get; set; }
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public bool IsRepeated { get; set; }

        public ReplParamAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }

    
}
