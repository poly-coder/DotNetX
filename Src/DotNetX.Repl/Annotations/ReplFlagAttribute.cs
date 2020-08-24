using System;

namespace DotNetX.Repl.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class ReplFlagAttribute : Attribute
    {
        public string Caption { get; set; }
        public string Description { get; set; }

        public ReplFlagAttribute()
        {
        }
    }
}
