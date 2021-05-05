using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace DotNetX.Reflection
{
    public record InterceptAsyncMethod<TState>(
        Func<object, MethodInfo, object?[]?, Task<TState>>? BeforeAction = null,
        Func<TState, object, MethodInfo, object?[]?, object?, Task>? AfterAction = null,
        Func<TState, object, MethodInfo, object?[]?, Exception, Task>? ErrorAction = null,
        Func<object, MethodInfo, object?[]?, bool>? ShouldInterceptAction = null)
        : IInterceptMethod
    {
        public static readonly InterceptAsyncMethod<TState> Default = CreateDefaultOptions();

        private static InterceptAsyncMethod<TState> CreateDefaultOptions()
        {
            return new InterceptAsyncMethod<TState>();
        }

        public InterceptAsyncMethod<TState> With(IInterceptAsyncMethod<TState> interceptors)
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

        public InterceptAsyncMethod<TState> With(IInterceptSyncMethod<TState> interceptors)
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

        public InterceptAsyncMethod<TState> Before(Func<Task<TState>> action)
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

        public InterceptAsyncMethod<TState> Before(Func<TState> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                BeforeAction = async (_, _, _) =>
                {
                    await Task.CompletedTask;
                    return action();
                },
            };
        }

        public InterceptAsyncMethod<TState> Before(Func<object, MethodInfo, object?[]?, Task<TState>> action)
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

        public InterceptAsyncMethod<TState> Before(Func<object, MethodInfo, object?[]?, TState> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                BeforeAction = async (target, targetMethod, args) =>
                {
                    await Task.CompletedTask;
                    return action(target, targetMethod, args);
                },
            };
        }

        public InterceptAsyncMethod<TState> After(Func<TState, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = (state, _, _, _, _) => action(state),
            };
        }

        public InterceptAsyncMethod<TState> After(Action<TState> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = async (state, _, _, _, _) =>
                {
                    await Task.CompletedTask;
                    action(state);
                },
            };
        }

        public InterceptAsyncMethod<TState> After(Func<TState, object?, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = (state, _, _, _, result) => action(state, result),
            };
        }

        public InterceptAsyncMethod<TState> After(Action<TState, object?> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = async (state, target, targetMethod, args, result) =>
                {
                    action(state, result);
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod<TState> After(Func<TState, object, MethodInfo, object?[]?, object?, Task> action)
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

        public InterceptAsyncMethod<TState> After(Action<TState, object, MethodInfo, object?[]?, object?> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                AfterAction = async (state, target, targetMethod, args, result) =>
                {
                    action(state, target, targetMethod, args, result);
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod<TState> Error(Func<TState, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = (state, _, _, _, _) => action(state),
            };
        }

        public InterceptAsyncMethod<TState> Error(Action<TState> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = async (state, target, targetMethod, args, result) =>
                {
                    action(state);
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod<TState> Error(Func<TState, Exception, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = (state, _, _, _, exception) => action(state, exception),
            };
        }

        public InterceptAsyncMethod<TState> Error(Action<TState, Exception> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = async (state, target, targetMethod, args, exception) =>
                {
                    action(state, exception);
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod<TState> Error(Func<TState, object, MethodInfo, object?[]?, Exception, Task> action)
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
        
        public InterceptAsyncMethod<TState> Error(Action<TState, object, MethodInfo, object?[]?, Exception> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                ErrorAction = async (state, target, targetMethod, args, exception) =>
                {
                    action(state, target, targetMethod, args, exception);
                    await Task.CompletedTask;
                },
            };
        }

        public InterceptAsyncMethod<TState> ShouldIntercept(Func<bool> action)
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

        public InterceptAsyncMethod<TState> ShouldIntercept(Func<object, MethodInfo, object?[]?, bool> action)
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
                BeforeAction != null &&
                (ShouldInterceptAction == null || ShouldInterceptAction(target, targetMethod, args));

            var state = shouldIntercept ? await BeforeAction!(target, targetMethod, args) : default;

            try
            {
                Task resultTask = (Task)targetMethod.Invoke(target, args);

                await resultTask;

                if (shouldIntercept && AfterAction != null)
                {
                    await AfterAction(state!, target, targetMethod, args, null);
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
                    await ErrorAction(state!, target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

        private static readonly MethodInfo InterceptAsTaskOfMethod =
            typeof(InterceptAsyncMethod<TState>)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsTaskOf));

        private async Task<T> InterceptAsTaskOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                BeforeAction != null &&
                (ShouldInterceptAction == null || ShouldInterceptAction(target, targetMethod, args));

            var state = shouldIntercept ? await BeforeAction!(target, targetMethod, args) : default;

            try
            {
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
                    await AfterAction(state!, target, targetMethod, args, result);
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
                    await ErrorAction(state!, target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

        private async ValueTask InterceptAsValueTask(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                BeforeAction != null && 
                (ShouldInterceptAction == null || ShouldInterceptAction(target, targetMethod, args));

            var state = shouldIntercept ? await BeforeAction!(target, targetMethod, args) : default;

            try
            {
                var resultTask = (ValueTask)targetMethod.Invoke(target, args);

                await resultTask;

                if (shouldIntercept && AfterAction != null)
                {
                    await AfterAction(state!, target, targetMethod, args, null);
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
                    await ErrorAction(state!, target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

        private static readonly MethodInfo InterceptAsValueTaskOfMethod =
            typeof(InterceptAsyncMethod<TState>)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsValueTaskOf));

        private async ValueTask<T> InterceptAsValueTaskOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                BeforeAction != null &&
                (ShouldInterceptAction == null || ShouldInterceptAction(target, targetMethod, args));

            var state = shouldIntercept ? await BeforeAction!(target, targetMethod, args) : default;

            try
            {
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
                    await AfterAction(state!, target, targetMethod, args, result);
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
                    await ErrorAction(state!, target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }
    }
}
