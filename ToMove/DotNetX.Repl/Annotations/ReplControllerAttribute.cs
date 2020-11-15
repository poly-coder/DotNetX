using System;

namespace DotNetX.Repl.Annotations
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ReplControllerAttribute : Attribute
    {
        public string Caption { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }

        public ReplControllerAttribute(string caption)
        {
            Caption = caption;
        }
    }
}
