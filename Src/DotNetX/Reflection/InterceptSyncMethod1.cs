using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace DotNetX.Reflection
{
    public record InterceptSyncMethod<TState>(
        Func<object, MethodInfo, object?[]?, TState>? BeforeAction = null,
        Action<TState, object, MethodInfo, object?[]?, object?>? AfterAction = null,
        Action<TState, object, MethodInfo, object?[]?, Exception>? ErrorAction = null,
        Func<object, MethodInfo, object?[]?, bool>? ShouldInterceptAction = null) 
        : IInterceptMethod
    {
        public static readonly InterceptSyncMethod<TState> Default = CreateDefaultOptions();

        private static InterceptSyncMethod<TState> CreateDefaultOptions()
        {
            return new InterceptSyncMethod<TState>();
        }

        public InterceptSyncMethod<TState> With(IInterceptSyncMethod<TState> interceptors)
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

        public InterceptSyncMethod<TState> Before(Func<TState> action)
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

        public InterceptSyncMethod<TState> Before(Func<object, MethodInfo, object?[]?, TState> action)
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

        public InterceptSyncMethod<TState> After(Action<TState> action)
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

        public InterceptSyncMethod<TState> After(Action<TState, object?> action)
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

        public InterceptSyncMethod<TState> After(Action<TState, object, MethodInfo, object?[]?, object?> action)
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

        public InterceptSyncMethod<TState> Error(Action<TState> action)
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

        public InterceptSyncMethod<TState> Error(Action<TState, Exception> action)
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

        public InterceptSyncMethod<TState> Error(Action<TState, object, MethodInfo, object?[]?, Exception> action)
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

        public InterceptSyncMethod<TState> ShouldIntercept(Func<bool> action)
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

        public InterceptSyncMethod<TState> ShouldIntercept(Func<object, MethodInfo, object?[]?, bool> action)
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
                BeforeAction != null &&
                (ShouldInterceptAction == null || ShouldInterceptAction(target, targetMethod, args));

            var state = shouldIntercept ? BeforeAction!(target, targetMethod, args) : default;

            try
            {
                result = targetMethod.Invoke(target, args);

                if (shouldIntercept && AfterAction != null)
                {
                    AfterAction.Invoke(state!, target, targetMethod, args, result);
                }

                return true;
            }
            catch (Exception exception)
            {
                if (exception is TargetInvocationException ex)
                {
                    exception = ex.InnerException ?? ex;
                }

                if (shouldIntercept && ErrorAction != null)
                {
                    ErrorAction.Invoke(state!, target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);
                
                throw;
            }
        }
    }
}
