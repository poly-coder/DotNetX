using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace DotNetX
{
    public static class ExceptionExtensions
    {
        private static Lazy<MethodInfo> preserveStackTraceMethod =
            new Lazy<MethodInfo>(() => typeof(Exception).GetMethod(
                "InternalPreserveStackTrace",
                BindingFlags.Instance | BindingFlags.NonPublic));

        public static void PreserveStackTrace(this Exception exn)
        {
            preserveStackTraceMethod.Value.Invoke(exn, null);
        }

        public static void Reraise(this Exception exn)
        {
            exn.PreserveStackTrace();
            throw exn;
        }

        public static void AddObject<T>(this SerializationInfo info, string name, T value)
            where T : class
        {
            if (info == null) return;
            if (value != null)
            {
                info.AddValue($"{name}_Type", "");
            }
            else
            {
                info.AddValue($"{name}_Type", value.GetType().AssemblyQualifiedName);
                info.AddValue(name, value);
            }
        }

        public static T GetObject<T>(this SerializationInfo info, string name)
            where T : class
        {
            if (info == null) return null;
            var typeName = info.GetString($"{name}_Type");
            if (typeName.IsNullOrEmpty())
            {
                return null;
            }
            var type = Type.GetType(typeName);
            return info.GetValue(name, type) as T;
        }

        public static object GetObject(this SerializationInfo info, string name) => info.GetObject<object>(name);

        public static TException FindInner<TException>(this Exception exception, Func<TException, bool> predicate = null)
            where TException : Exception
        {
            switch (exception)
            {
                case TException ex:
                    if (predicate == null || predicate(ex))
                        return ex;
                    return null;

                case AggregateException ex:
                    foreach (var inner in ex.InnerExceptions)
                    {
                        var found = inner.FindInner(predicate);
                        if (found != null)
                            return found;
                    }
                    return null;

                default:
                    if (exception.InnerException != null)
                    {
                        return exception.InnerException.FindInner(predicate);
                    }
                    return null;

            }
        }
    }
}
