using System;
using DotNetX;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.Generic;

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
    
        
        public static void Write(ColoredText text)
        {
            foreach (var item in text.items)
            {
                WriteItem(item);
            }
        }

        public static void WriteLine(ColoredText text)
        {
            Write(text);
            Console.WriteLine();
        }

        private static void WriteItem(ColoredText.CTItem item)
        {
            Write(
                item.ForegroundColor ?? Console.ForegroundColor,
                item.BackgroundColor ?? Console.BackgroundColor,
                item.ToString());
        }


        public static void WriteForm(ConsoleForm form)
        {
            if (form.Title != null)
            {
                ConsoleEx.WriteLine(ConsoleColor.Cyan, form.Title);
            }

            foreach (var field in form.Fields)
            {
                var keyLen = field.Key.ToString().Length;
                ConsoleEx.Write(field.Key);
                if (keyLen < form.FieldPad)
                {
                    Console.Write(new string(' ', form.FieldPad - keyLen));
                }
                Console.Write(": ");
                ConsoleEx.WriteLine(field.Value);
            }
        }
    }

    public class ColoredText
    {
        internal List<CTItem> items = new List<CTItem>();

        public ColoredText()
        {

        }

        // TODO: Create one per parameter combination
        public ColoredText(string message)
        {
            Add(message);
        }

        public ColoredText(ConsoleColor foregroundColor, string message)
        {
            Add(foregroundColor, message);
        }

        public ColoredText Add(string message)
        {
            items.Add(new CTItem { Message = message });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, string message)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                Message = message 
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                BackgroundColor = backgroundColor, 
                Message = message 
            });
            return this;
        }

        public ColoredText Add(string message, object arg0)
        {
            items.Add(new CTItem { 
                Message = message,
                Arg0 = arg0,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, string message, object arg0)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                Message = message,
                Arg0 = arg0,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, object arg0)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                BackgroundColor = backgroundColor, 
                Message = message,
                Arg0 = arg0,
            });
            return this;
        }

        public ColoredText Add(string message, object arg0, object arg1)
        {
            items.Add(new CTItem { 
                Message = message,
                Arg0 = arg0,
                Arg1 = arg1,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, string message, object arg0, object arg1)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                Message = message,
                Arg0 = arg0,
                Arg1 = arg1,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, object arg0, object arg1)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                BackgroundColor = backgroundColor, 
                Message = message,
                Arg0 = arg0,
                Arg1 = arg1,
            });
            return this;
        }

        public ColoredText Add(string message, object arg0, object arg1, object arg2)
        {
            items.Add(new CTItem { 
                Message = message,
                Arg0 = arg0,
                Arg1 = arg1,
                Arg2 = arg2,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, string message, object arg0, object arg1, object arg2)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                Message = message,
                Arg0 = arg0,
                Arg1 = arg1,
                Arg2 = arg2,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, object arg0, object arg1, object arg2)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                BackgroundColor = backgroundColor, 
                Message = message,
                Arg0 = arg0,
                Arg1 = arg1,
                Arg2 = arg2,
            });
            return this;
        }

        public ColoredText Add(string message, params object[] args)
        {
            items.Add(new CTItem { 
                Message = message,
                Args = args,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, string message, params object[] args)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                Message = message,
                Args = args,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, params object[] args)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                BackgroundColor = backgroundColor, 
                Message = message,
                Args = args,
            });
            return this;
        }

        public ColoredText Add(object value)
        {
            items.Add(new CTItem { 
                Value = value,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, object value)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor,
                Value = value,
            });
            return this;
        }

        public ColoredText Add(ConsoleColor foregroundColor, ConsoleColor backgroundColor, object value)
        {
            items.Add(new CTItem { 
                ForegroundColor = foregroundColor, 
                BackgroundColor = backgroundColor,
                Value = value,
            });
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var item in items)
            {
                sb.Append(item.ToString());
            }

            return sb.ToString();
        }

        internal class CTItem
        {
            internal ConsoleColor? ForegroundColor;
            internal ConsoleColor? BackgroundColor;
            internal string? Message;
            internal object? Arg0;
            internal object? Arg1;
            internal object? Arg2;
            internal object[]? Args;
            internal object? Value;

            public override string ToString()
            {
                if (Message != null)
                {
                    if (Args != null)
                    {
                        return string.Format(Message, Args);
                    }
                    else
                    {
                        return string.Format(Message, Arg0, Arg1, Arg2);
                    }
                }
                else
                {
                    return string.Format("{0}", Value);
                }
            }
        }
    }

    public class ConsoleForm
    {
        public string? Title { get; set; }
        public int? Width { get; set; }
        public int FieldPad { get; set; } = 20;

        public List<KeyValuePair<ColoredText, ColoredText>> Fields { get; set; } = new List<KeyValuePair<ColoredText, ColoredText>>();

        public ConsoleForm Add(ColoredText name, ColoredText value)
        {
            Fields.Add(KeyValuePair.Create(name, value));
            return this;
        }

        public ConsoleForm Add(ColoredText name, string value) => Add(name, new ColoredText(value));

        public ConsoleForm Add(string name, ColoredText value) => Add(new ColoredText(name), value);

        public ConsoleForm Add(string name, string value) => Add(new ColoredText(name), new ColoredText(value));
    }
}
