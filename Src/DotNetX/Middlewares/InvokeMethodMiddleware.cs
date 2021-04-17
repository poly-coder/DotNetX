using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Middlewares
{
    using InvokeMethodSyncMiddlewareFunc = VoidSyncMiddlewareFunc<InvokeMethodContext>;
    using InvokeMethodSyncMiddleware = VoidSyncMiddleware<InvokeMethodContext>;
    using InvokeMethodAsyncMiddlewareFunc = TaskMiddlewareFunc<InvokeMethodContext>;
    using InvokeMethodAsyncMiddleware = TaskMiddleware<InvokeMethodContext>;

    public record InvokeMethodContext(
        MethodInfo Method,
        object[] Parameters,
        object? Instance)
    {
        private object? _result;
        private Exception? _exception;

        public object? Result => _result;
        public Exception? Exception => _exception;
        
        public bool HasResult { get; private set; }
        public bool HasException { get; private set; }

        public void SetResult(object? result)
        {
            _result = result;
            HasResult = true;
            _exception = null;
            HasException = false;
        }

        public void SetException(Exception exception)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
            HasException = true;
            _result = null;
            HasResult = false;
        }

        public TResult Return<TResult>()
        {
            if (HasException)
            {
                throw new TargetInvocationException(Exception);
            }

            if (HasResult)
            {
                return (TResult)Result!;
            }

            throw new InvokeMethodWithoutResultException();
        }

        public void Return()
        {
            if (HasException)
            {
                throw new TargetInvocationException(Exception);
            }

            if (HasResult)
            {
                return;
            }

            throw new InvokeMethodWithoutResultException();
        }
    }

    public static class InvokeMethodMiddleware
    {
        public static void CallSyncFunc(InvokeMethodContext context)
        {
            try
            {
                var result = context.Method.Invoke(
                    obj: context.Instance, 
                    parameters: context.Parameters);

                context.SetResult(result);
            }
            catch (TargetInvocationException exception)
            {
                context.SetException(exception.InnerException ?? exception);
            }
            catch (Exception exception)
            {
                context.SetException(exception);
            }
        }

        private static readonly ConcurrentDictionary<Type, PropertyInfo> TaskResultPropertyCache = new ConcurrentDictionary<Type, PropertyInfo>();
        public static async Task CallAsyncFunc(InvokeMethodContext context, CancellationToken cancellationToken)
        {
            try
            {
                var task = (Task)context.Method.Invoke(
                    obj: context.Instance, 
                    parameters: context.Parameters)!;

                await task;

                var taskType = task.GetType();
                if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var property = TaskResultPropertyCache
                        .GetOrAdd(taskType, t => taskType.GetProperty(
                            name: nameof(Task<string>.Result),
                            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                            binder: default,
                            returnType: default,
                            types: Array.Empty<Type>(),
                            modifiers: default)!);

                    object? result = property.GetValue(task);
                    context.SetResult(result);
                }
                else
                {
                    context.SetResult(null);
                }
            }
            catch (TargetInvocationException exception)
            {
                context.SetException(exception.InnerException ?? exception);
            }
            catch (Exception exception)
            {
                context.SetException(exception);
            }
        }

        public static void CallInvoke(
            this InvokeMethodSyncMiddlewareFunc func,
            object instance,
            MethodInfo method,
            params object[] parameters)
        {
            var context = new InvokeMethodContext(method, parameters, instance);

            func(context);

            context.Return();
        }

        public static void CallInvoke(
            this InvokeMethodSyncMiddlewareFunc func,
            MethodInfo method,
            params object[] parameters)
        {
            var context = new InvokeMethodContext(method, parameters, Instance: null);

            func(context);

            context.Return();
        }

        public static TResult CallInvoke<TResult>(
            this InvokeMethodSyncMiddlewareFunc func,
            object instance,
            MethodInfo method,
            params object[] parameters)
        {
            var context = new InvokeMethodContext(method, parameters, instance);

            func(context);

            return context.Return<TResult>();
        }

        public static TResult CallInvoke<TResult>(
            this InvokeMethodSyncMiddlewareFunc func,
            MethodInfo method,
            params object[] parameters)
        {
            var context = new InvokeMethodContext(method, parameters, Instance: null);

            func(context);

            return context.Return<TResult>();
        }

        public static async Task CallInvokeAsync(
            this InvokeMethodAsyncMiddlewareFunc func,
            object instance,
            MethodInfo method,
            CancellationToken cancellationToken,
            params object[] parameters)
        {
            var context = new InvokeMethodContext(method, parameters, instance);

            await func(context, cancellationToken);

            context.Return();
        }

        public static async Task CallInvokeAsync(
            this InvokeMethodAsyncMiddlewareFunc func,
            MethodInfo method,
            CancellationToken cancellationToken,
            params object[] parameters)
        {
            var context = new InvokeMethodContext(method, parameters, Instance: null);

            await func(context, cancellationToken);

            context.Return();
        }

        public static async Task<TResult> CallInvokeAsync<TResult>(
            this InvokeMethodAsyncMiddlewareFunc func,
            object instance,
            MethodInfo method,
            CancellationToken cancellationToken,
            params object[] parameters)
        {
            var context = new InvokeMethodContext(method, parameters, instance);

            await func(context, cancellationToken);

            return context.Return<TResult>();
        }

    }


    [Serializable]
    public class InvokeMethodWithoutResultException : Exception
    {
        public InvokeMethodWithoutResultException() { }
        public InvokeMethodWithoutResultException(string message) : base(message) { }
        public InvokeMethodWithoutResultException(string message, Exception inner) : base(message, inner) { }
        protected InvokeMethodWithoutResultException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
