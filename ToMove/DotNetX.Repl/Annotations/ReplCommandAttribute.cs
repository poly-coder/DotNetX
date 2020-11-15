using System;

namespace DotNetX.Repl.Annotations
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ReplCommandAttribute : Attribute
    {
        public string? Caption { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; }

        public ReplCommandAttribute(string name)
        {
            Name = name;
        }
    }
}
