using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Linq;

namespace DotNetX
{
    public static class HttpExtensions
    {
        public static CultureInfo GetAcceptedLanguage(
            this HttpRequest request,
            CultureInfo? defaultCulture = null)
        {
            var headers = request.GetTypedHeaders();

            var acceptLanguage = headers.AcceptLanguage;

            if ((acceptLanguage?.Count ?? 0) > 0)
            {
                foreach (var item in acceptLanguage!.OrderByDescending(e => e.Quality ?? double.MaxValue))
                {
                    try
                    {
                        var culture = CultureInfo.GetCultureInfo(item.Value.ToString());
                        return culture;
                    }
                    catch (CultureNotFoundException)
                    {
                        continue;
                    }
                }
            }

            return defaultCulture.OrCurrent();
        }

        public static IDisposable WithAcceptedLanguage(
            this HttpRequest request,
            CultureInfo? defaultCulture = null) =>
            request
                .GetAcceptedLanguage(defaultCulture)
                .WithCurrentCultures();
    }
}
