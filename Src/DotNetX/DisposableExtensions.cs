using System;
using System.Globalization;
using System.Threading;

namespace DotNetX
{
    public static class DisposableExtensions
    {
        public static IDisposable SetAndUndo<T>(this T value, Action doAction, Action<T> undoAction)
        {
            if (doAction is null)
            {
                throw new ArgumentNullException(nameof(doAction));
            }

            if (undoAction is null)
            {
                throw new ArgumentNullException(nameof(undoAction));
            }

            doAction();

            return new Disposable(() => undoAction(value));
        }

        public static IDisposable SetAndUndo<T>(this T value, T newValue, Action<T> setAction)
        {
            if (setAction is null)
            {
                throw new ArgumentNullException(nameof(setAction));
            }

            return value.SetAndUndo(
                doAction: () => setAction(newValue),
                undoAction: setAction);
        }
    }
}
