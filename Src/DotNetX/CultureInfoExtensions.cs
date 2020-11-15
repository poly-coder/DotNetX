using System;
using System.Globalization;
using System.Threading;

namespace DotNetX
{
    public static class CultureInfoExtensions
    {
        public static CultureInfo OrCurrent(this CultureInfo? array) =>
            array ?? CultureInfo.CurrentCulture;

        public static CultureInfo OrCurrentUI(this CultureInfo? array) =>
            array ?? CultureInfo.CurrentUICulture;

        public static IDisposable WithCurrentCultures(this CultureInfo culture)
        {
            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var thread = Thread.CurrentThread;

            return (c: thread.CurrentCulture, uic: thread.CurrentUICulture)
                .SetAndUndo((c: culture, uic: culture),
                    prev =>
                    {
                        thread.CurrentUICulture = prev.c;
                        thread.CurrentUICulture = prev.uic;
                    });
        }

        public static IDisposable WithCurrentCulture(this CultureInfo culture)
        {
            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var thread = Thread.CurrentThread;

            return thread.CurrentUICulture.SetAndUndo(
                culture, prev => thread.CurrentCulture = prev);
        }

        public static IDisposable WithCurrentUICulture(this CultureInfo culture)
        {
            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var thread = Thread.CurrentThread;

            return thread.CurrentUICulture.SetAndUndo(
                culture, prev => thread.CurrentUICulture = prev);
        }
    }
}
