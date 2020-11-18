using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace DotNetX
{
    public class PassThroughStringLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] =>
            new LocalizedString(name, name, true);

        public LocalizedString this[string name, params object[] arguments] =>
             new LocalizedString(name, name.Format(arguments), true);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            yield break;
        }
    }

}
