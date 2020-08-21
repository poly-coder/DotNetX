using System;

namespace DotNetX.Repl.Annotations
{
    public enum ReplExampleScope
    {
        Local = 0,
        Parent = 1,
        All = 2,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class ReplExampleAttribute : Attribute
    {
        public string Caption { get; set; }
        public string Description { get; set; }
        public string Command { get; set; }
        public ReplExampleScope Scope { get; set; } = ReplExampleScope.Local;

        public ReplExampleAttribute(string command)
        {
            Command = command;
        }
    }

    
}
