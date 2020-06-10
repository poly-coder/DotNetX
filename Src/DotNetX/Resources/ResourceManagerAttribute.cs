using System;

namespace DotNetX.Resources
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ResourceManagerAttribute : Attribute
    {
        public ResourceManagerAttribute(string resources)
        {
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));
        }

        public string Resources { get; }
    }
}
