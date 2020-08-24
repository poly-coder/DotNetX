using System;

namespace DotNetX.Repl.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class ReplServiceAttribute : Attribute
    {
    }
}
