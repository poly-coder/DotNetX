using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace DotNetX.Reflection
{
    public record InterceptSyncMethod(
        Action<object, MethodInfo, object?[]?>? BeforeAction = null,
        Action<object, MethodInfo, object?[]?, object?>? AfterAction = null,
        Action<object, MethodInfo, object?[]?, Exception>? ErrorAction = null,
        Func<object, MethodInfo, object?[]?, bool>? ShouldInterceptAction = null) 
        : IInterceptMethod
    {
        public static readonly InterceptSyncMethod Default = CreateDefaultOptions();

        private static InterceptSyncMethod CreateDefaultOptions()
        {
            return new InterceptSyncMethod();
        }

        public InterceptSyncMethod With(IInterceptSyncMethod interceptors)
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

        public InterceptSyncMethod Before(Action action)
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

        public InterceptSyncMethod Before(Action<object, MethodInfo, object?[]?> action)
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

        public InterceptSyncMethod After(Action action)
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

        public InterceptSyncMethod After(Action<object?> action)
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

        public InterceptSyncMethod After(Action<object, MethodInfo, object?[]?, object?> action)
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

        public InterceptSyncMethod Error(Action action)
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

        public InterceptSyncMethod Error(Action<Exception> action)
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

        public InterceptSyncMethod Error(Action<object, MethodInfo, object?[]?, Exception> action)
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

        public InterceptSyncMethod ShouldIntercept(Func<bool> action)
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

        public InterceptSyncMethod ShouldIntercept(Func<object, MethodInfo, object?[]?, bool> action)
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
            bool shouldIntercept =
                ShouldInterceptAction == null || 
                ShouldInterceptAction(target, targetMethod, args);

            result = ExceptionExtensions.UnwrapTargetInvocationException<object?>(
                () =>
                {
                    if (shouldIntercept && BeforeAction != null)
                    {
                        BeforeAction.Invoke(target, targetMethod, args);
                    }

                    var methodResult = targetMethod.Invoke(target, args);

                    if (shouldIntercept && AfterAction != null)
                    {
                        AfterAction.Invoke(target, targetMethod, args, methodResult);
                    }

                    return methodResult;
                },
                exception =>
                {
                    if (shouldIntercept && ErrorAction != null)
                    {
                        ErrorAction.Invoke(target, targetMethod, args, exception);
                    }

                    return (default, false);
                });

            return true;
        }
    }
}
