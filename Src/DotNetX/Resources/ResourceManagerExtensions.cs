using DotNetX.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace DotNetX.Resources
{
    public static class ResourceManagerExtensions
    {
        public static Func<CultureInfo, string> LocalizedGetter(
            this ResourceManager resourceManager,
            string name) =>
            culture =>
            resourceManager.GetString(name, culture) ?? name;

        public static Func<CultureInfo, T> LocalizedGetter<T>(
            this ResourceManager resourceManager,
            string name,
            Func<string, T> selector)
        {
            var getter = resourceManager.LocalizedGetter(name);
            return culture => selector(getter(culture));
        }

        public static Func<CultureInfo, IReadOnlyCollection<string>> LocalizedSplitGetter(
            this ResourceManager resourceManager,
            string name,
            string separator = ",")
            => resourceManager.LocalizedGetter(
                name,
                str => str.Split(separator).Select(s => s.Trim()).Where(s => s.IsNotNullOrEmpty()).ToArray());

        public static ResourceManager GetResourceManager(this Type type)
        {
            var resourceName = type.GetConventionValue<Type, ResourceManagerAttribute, string>(
                true,
                t =>
                {
                    var assemblyName = type.Assembly.FullName.Before(",");
                    return $"{assemblyName}.Resources";
                },
                a => a.Resources
            );

            return new ResourceManager(resourceName, type.Assembly);
        }

    }
}
