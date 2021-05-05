using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace DotNetX.Reflection
{
    public record InterceptAsyncMethod(
        Func<object, MethodInfo, object?[]?, Task>? BeforeAction = null,
        Func<object, MethodInfo, object?[]?, object?, Task>? AfterAction = null,
        Func<object, MethodInfo, object?[]?, Exception, Task>? ErrorAction = null,
        Func<object, MethodInfo, object?[]?, bool>? ShouldInterceptAction = null)
        : IInterceptMethod
    {
        public static readonly InterceptAsyncMethod Default = CreateDefaultOptions();

        private static InterceptAsyncMethod CreateDefaultOptions()
        {
            return new InterceptAsyncMethod();
        }

        public InterceptAsyncMethod With(IInterceptAsyncMethod interceptors)
        {
            if (interceptors is null)
            {
                throw new ArgumentNullException(nameof(interceptors));
            }

            return this
                .ShouldIntercept(interceptors.ShouldIntercept)
                .Before(interceptors.Before)
                .After(interceptors.After)
                .Error(interceptors.Error);
        }

        public InterceptAsyncMethod With(IInterceptSyncMethod interceptors)
        {
            if (interceptors is null)
            {
                throw new ArgumentNullException(nameof(interceptors));
            }

            return this
                .ShouldIntercept(interceptors.ShouldIntercept)
                .Before(interceptors.Before)
                .After(interceptors.After)
                .Error(interceptors.Error);
        }

        public InterceptAsyncMethod Before(Func<Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                BeforeAction = (_, _, _) => action(),
            };
        }

        public InterceptAsyncMethod Before(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                BeforeAction = async (_, _, _) => { action(); await Task.CompletedTask; },
            };
        }

        public InterceptAsyncMethod Before(Func<object, MethodInfo, object?[]?, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                BeforeAction = action,
            };
        }

        public InterceptAsyncMethod Before(Action<object, MethodInfo, object?[]?> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                BeforeAction = async (target, targetMethod, args) => 
                { 
                    action(target, targetMethod, args); 
                    await Task.CompletedTask; 
                },
            };
        }

        public InterceptAsyncMethod After(Func<Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = (_, _, _, _) => action(),
            };
        }

        public InterceptAsyncMethod After(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = async (_, _, _, _) =>
                {
                    action();
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod After(Func<object?, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = (_, _, _, result) => action(result),
            };
        }

        public InterceptAsyncMethod After(Action<object?> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = async (_, _, _, result) =>
                {
                    action(result);
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod After(Func<object, MethodInfo, object?[]?, object?, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = action,
            };
        }

        public InterceptAsyncMethod After(Action<object, MethodInfo, object?[]?, object?> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = async (target, targetMethod, args, result) =>
                {
                    action(target, targetMethod, args, result);
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod Error(Func<Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = (_, _, _, _) => action(),
            };
        }

        public InterceptAsyncMethod Error(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = async (_, _, _, _) =>
                {
                    action();
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod Error(Func<Exception, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = (_, _, _, exception) => action(exception),
            };
        }

        public InterceptAsyncMethod Error(Action<Exception> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = async (_, _, _, exception) =>
                {
                    action(exception);
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod Error(Func<object, MethodInfo, object?[]?, Exception, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = action,
            };
        }

        public InterceptAsyncMethod Error(Action<object, MethodInfo, object?[]?, Exception> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = async (target, targetMethod, args, exception) =>
                {
                    action(target, targetMethod, args, exception);
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod ShouldIntercept(Func<bool> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ShouldInterceptAction = (_, _, _) => action(),
            };
        }

        public InterceptAsyncMethod ShouldIntercept(Func<object, MethodInfo, object?[]?, bool> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ShouldInterceptAction = action,
            };
        }

        public bool TryToIntercept(object target, MethodInfo targetMethod, object?[]? args, out object? result)
        {
            var returnType = targetMethod.ReturnType;

            if (returnType == typeof(Task))
            {
                result = InterceptAsTask(target, targetMethod, args);
                return true;
            }

            if (returnType == typeof(ValueTask))
            {
                result = InterceptAsValueTask(target, targetMethod, args);
                return true;
            }

            if (returnType.TryGetGenericParameters(typeof(Task<>), out var tResult))
            {
                var method = InterceptAsTaskOfMethod.MakeGenericMethod(tResult);
                result = method.Invoke(this, new object?[] { target, targetMethod, args });
                return true;
            }

            if (returnType.TryGetGenericParameters(typeof(ValueTask<>), out tResult))
            {
                var method = InterceptAsValueTaskOfMethod.MakeGenericMethod(tResult);
                result = method.Invoke(this, new object?[] { target, targetMethod, args });
                return true;
            }

            result = null;
            return false;
        }

        private async Task InterceptAsTask(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args);

            try
            {
                if (shouldIntercept && BeforeAction != null)
                {
                    await BeforeAction(target, targetMethod, args);
                }

                Task resultTask = (Task)targetMethod.Invoke(target, args);

                await resultTask;

                if (shouldIntercept && AfterAction != null)
                {
                    await AfterAction(target, targetMethod, args, null);
                }
            }
            catch (Exception exception)
            {
                if (exception is TargetInvocationException ex)
                {
                    exception = ex.InnerException ?? ex;
                }

                if (shouldIntercept && ErrorAction != null)
                {
                    await ErrorAction(target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

        private static readonly MethodInfo InterceptAsTaskOfMethod =
            typeof(InterceptAsyncMethod)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsTaskOf));

        private async Task<T> InterceptAsTaskOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args);

            try
            {
                if (shouldIntercept && BeforeAction != null)
                {
                    await BeforeAction(target, targetMethod, args);
                }

                Task resultTask = (Task)targetMethod.Invoke(target, args);

                await resultTask;

                var result = typeof(Task<T>).InvokeMember(
                    nameof(Task<T>.Result),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                    binder: null,
                    resultTask,
                    args: null);

                if (shouldIntercept && AfterAction != null)
                {
                    await AfterAction(target, targetMethod, args, result);
                }

                return (T)result;
            }
            catch (Exception exception)
            {
                if (exception is TargetInvocationException ex)
                {
                    exception = ex.InnerException ?? ex;
                }

                if (shouldIntercept && ErrorAction != null)
                {
                    await ErrorAction(target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

        private async ValueTask InterceptAsValueTask(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args);

            try
            {
                if (shouldIntercept && BeforeAction != null)
                {
                    await BeforeAction(target, targetMethod, args);
                }

                var resultTask = (ValueTask)targetMethod.Invoke(target, args);

                await resultTask;

                if (shouldIntercept && AfterAction != null)
                {
                    await AfterAction(target, targetMethod, args, null);
                }
            }
            catch (Exception exception)
            {
                if (exception is TargetInvocationException ex)
                {
                    exception = ex.InnerException ?? ex;
                }

                if (shouldIntercept && ErrorAction != null)
                {
                    await ErrorAction(target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

        private static readonly MethodInfo InterceptAsValueTaskOfMethod =
            typeof(InterceptAsyncMethod)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsValueTaskOf));

        private async ValueTask<T> InterceptAsValueTaskOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                ShouldInterceptAction == null || 
                ShouldInterceptAction(target, targetMethod, args);

            try
            {
                if (shouldIntercept && BeforeAction != null)
                {
                    await BeforeAction(target, targetMethod, args);
                }

                ValueTask<T> resultTask = (ValueTask<T>)targetMethod.Invoke(target, args);

                await resultTask;

                var result = typeof(ValueTask<T>).InvokeMember(
                    nameof(ValueTask<T>.Result),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                    binder: null,
                    resultTask,
                    args: null);

                if (shouldIntercept && AfterAction != null)
                {
                    await AfterAction(target, targetMethod, args, result);
                }

                return (T)result;
            }
            catch (Exception exception)
            {
                if (exception is TargetInvocationException ex)
                {
                    exception = ex.InnerException ?? ex;
                }

                if (shouldIntercept && ErrorAction != null)
                {
                    await ErrorAction(target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }
    }
}
