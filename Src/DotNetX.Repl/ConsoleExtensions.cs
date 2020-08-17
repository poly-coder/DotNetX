using System;
using DotNetX;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotNetX.Repl
{
    public static class ConsoleEx
    {
        public static void WithColor(ConsoleColor foregroundColor, Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var current = Console.ForegroundColor;

            Console.ForegroundColor = foregroundColor;

            action();

            Console.ForegroundColor = current;
        }

        public static void WithColor(ConsoleColor foregroundColor, ConsoleColor backgroundColor, Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var currentFore = Console.ForegroundColor;
            var currentBack = Console.BackgroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            action();

            Console.ForegroundColor = currentFore;
            Console.BackgroundColor = currentBack;
        }

        public static IDisposable WithColor(ConsoleColor foregroundColor)
        {
            var current = Console.ForegroundColor;

            Console.ForegroundColor = foregroundColor;

            return new Disposable(() => Console.ForegroundColor = current);
        }

        public static IDisposable WithColor(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            var currentFore = Console.ForegroundColor;
            var currentBack = Console.BackgroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            return new Disposable(() =>
            {
                Console.ForegroundColor = currentFore;
                Console.BackgroundColor = currentBack;
            });
        }

        public static void Write(ConsoleColor foregroundColor, string message)
        {
            WithColor(foregroundColor, () => Console.Write(message));
        }

        public static void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.Write(message));
        }

        public static void WriteLine(ConsoleColor foregroundColor, string message)
        {
            WithColor(foregroundColor, () => Console.WriteLine(message));
        }

        public static void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.WriteLine(message));
        }

        public static void Write(ConsoleColor foregroundColor, string message, object arg0)
        {
            WithColor(foregroundColor, () => Console.Write(message, arg0));
        }

        public static void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, object arg0)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.Write(message, arg0));
        }

        public static void WriteLine(ConsoleColor foregroundColor, string message, object arg0)
        {
            WithColor(foregroundColor, () => Console.WriteLine(message, arg0));
        }

        public static void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, object arg0)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.WriteLine(message, arg0));
        }

        public static void Write(ConsoleColor foregroundColor, string message, object arg0, object arg1)
        {
            WithColor(foregroundColor, () => Console.Write(message, arg0, arg1));
        }

        public static void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, object arg0, object arg1)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.Write(message, arg0, arg1));
        }

        public static void WriteLine(ConsoleColor foregroundColor, string message, object arg0, object arg1)
        {
            WithColor(foregroundColor, () => Console.WriteLine(message, arg0, arg1));
        }

        public static void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, object arg0, object arg1)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.WriteLine(message, arg0, arg1));
        }

        public static void Write(ConsoleColor foregroundColor, string message, object arg0, object arg1, object arg2)
        {
            WithColor(foregroundColor, () => Console.Write(message, arg0, arg1, arg2));
        }

        public static void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, object arg0, object arg1, object arg2)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.Write(message, arg0, arg1, arg2));
        }

        public static void WriteLine(ConsoleColor foregroundColor, string message, object arg0, object arg1, object arg2)
        {
            WithColor(foregroundColor, () => Console.WriteLine(message, arg0, arg1, arg2));
        }

        public static void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, object arg0, object arg1, object arg2)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.WriteLine(message, arg0, arg1, arg2));
        }

        public static void Write(ConsoleColor foregroundColor, string message, params object[] args)
        {
            WithColor(foregroundColor, () => Console.Write(message, args));
        }

        public static void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, params object[] args)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.Write(message, args));
        }

        public static void WriteLine(ConsoleColor foregroundColor, string message, params object[] args)
        {
            WithColor(foregroundColor, () => Console.WriteLine(message, args));
        }

        public static void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, params object[] args)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.WriteLine(message, args));
        }

        public static void Write(ConsoleColor foregroundColor, object value)
        {
            WithColor(foregroundColor, () => Console.Write(value));
        }

        public static void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, object value)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.Write(value));
        }

        public static void WriteLine(ConsoleColor foregroundColor, object value)
        {
            WithColor(foregroundColor, () => Console.WriteLine(value));
        }

        public static void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, object value)
        {
            WithColor(foregroundColor, backgroundColor, () => Console.WriteLine(value));
        }
    }
}
