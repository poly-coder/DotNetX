using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace DotNetX.Reflection
{
    public record InterceptSyncMethod(
        Action<object, MethodInfo, object?[]?>? BeforeAction = null,
        Action<object, MethodInfo, object?[]?, object?>? AfterAction = null,
        Action<object, MethodInfo, object?[]?, Exception>? ErrorAction = null) 
        : InterceptMethod()
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

        public override bool TryToIntercept(object target, MethodInfo targetMethod, object?[]? args, out object? result)
        {
            try
            {
                BeforeAction?.Invoke(target, targetMethod, args);

                result = targetMethod.Invoke(target, args);
                
                AfterAction?.Invoke(target, targetMethod, args, result);

                return true;
            }
            catch (Exception exception)
            {
                if (exception is TargetInvocationException ex)
                {
                    exception = ex.InnerException ?? ex;
                }

                ErrorAction?.Invoke(target, targetMethod, args, exception);

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }
    }
}
