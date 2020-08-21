using System;

namespace DotNetX.Repl.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class ReplOptionAttribute: Attribute
    {
        public string TypeName { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public bool IsRepeated { get; set; }
        public int ValueCount { get; set; } = 1;

        public ReplOptionAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }

    
}
