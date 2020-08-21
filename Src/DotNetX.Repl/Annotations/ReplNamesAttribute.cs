using System;

namespace DotNetX.Repl.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class ReplNamesAttribute : Attribute
    {
        public string[] Names { get; private set; }

        public ReplNamesAttribute(params string[] names)
        {
            Names = names;
        }
    }

    
}
