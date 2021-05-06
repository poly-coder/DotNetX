using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace DotNetX.Reflection
{
    public record InterceptEnumerableMethod<TState>(
        Func<object, MethodInfo, object?[]?, TState>? BeforeAction = null,
        Action<TState, object, MethodInfo, object?[]?, object?>? NextAction = null,
        Action<TState, object, MethodInfo, object?[]?>? CompleteAction = null,
        Action<TState, object, MethodInfo, object?[]?, Exception>? ErrorAction = null,
        Func<object, MethodInfo, object?[]?, bool>? ShouldInterceptAction = null)
        : IInterceptMethod
    {
        public static readonly InterceptEnumerableMethod<TState> Default = CreateDefaultOptions();

        private static InterceptEnumerableMethod<TState> CreateDefaultOptions()
        {
            return new InterceptEnumerableMethod<TState>();
        }

        public InterceptEnumerableMethod<TState> With(IInterceptEnumerableMethod<TState> interceptors)
        {
            if (interceptors is null)
            {
                throw new ArgumentNullException(nameof(interceptors));
            }

            return this
                .ShouldIntercept(interceptors.ShouldIntercept)
                .Before(interceptors.Before)
                .Next(interceptors.Next)
                .Complete(interceptors.Complete)
                .Error(interceptors.Error);
        }

        public InterceptEnumerableMethod<TState> Before(Func<TState> action)
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

        public InterceptEnumerableMethod<TState> Before(Func<object, MethodInfo, object?[]?, TState> action)
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

        public InterceptEnumerableMethod<TState> Next(Action<TState> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                NextAction = (state, _, _, _, _) => action(state),
            };
        }

        public InterceptEnumerableMethod<TState> Next(Action<TState, object?> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                NextAction = (state, _, _, _, value) => action(state, value),
            };
        }

        public InterceptEnumerableMethod<TState> Next(Action<TState, object, MethodInfo, object?[]?, object?> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                NextAction = action,
            };
        }
        
        public InterceptEnumerableMethod<TState> Complete(Action<TState> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                CompleteAction = (state, _, _, _) => action(state),
            };
        }

        public InterceptEnumerableMethod<TState> Complete(Action<TState, object, MethodInfo, object?[]?> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                CompleteAction = action,
            };
        }

        public InterceptEnumerableMethod<TState> Error(Action<TState> action)
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

        public InterceptEnumerableMethod<TState> Error(Action<TState, Exception> action)
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

        public InterceptEnumerableMethod<TState> Error(Action<TState, object, MethodInfo, object?[]?, Exception> action)
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

        public InterceptEnumerableMethod<TState> ShouldIntercept(Func<bool> action)
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

        public InterceptEnumerableMethod<TState> ShouldIntercept(Func<object, MethodInfo, object?[]?, bool> action)
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

            if (returnType == typeof(IEnumerable))
            {
                result = InterceptAsIEnumerable(target, targetMethod, args);
                return true;
            }

            if (returnType.TryGetGenericParameters(typeof(IEnumerable<>), out var tResult))
            {
                var method = InterceptAsIEnumerableOfMethod.MakeGenericMethod(tResult);
                result = ExceptionExtensions.UnwrapTargetInvocationException(
                    () => method.Invoke(this, new object?[] { target, targetMethod, args }));
                return true;
            }

            if (returnType.TryGetGenericParameters(typeof(IAsyncEnumerable<>), out tResult))
            {
                var method = InterceptAsIAsyncEnumerableOfMethod.MakeGenericMethod(tResult);
                result = ExceptionExtensions.UnwrapTargetInvocationException(
                    () => method.Invoke(this, new object?[] { target, targetMethod, args }));
                return true;
            }

            if (returnType.TryGetGenericParameters(typeof(IObservable<>), out tResult))
            {
                var method = InterceptAsIObservableOfMethod.MakeGenericMethod(tResult);
                result = ExceptionExtensions.UnwrapTargetInvocationException(
                    () => method.Invoke(this, new object?[] { target, targetMethod, args }));
                return true;
            }

            result = null;
            return false;
        }

        private IEnumerable? InterceptAsIEnumerable(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                BeforeAction != null &&
                (ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args));

            var callNextAction = shouldIntercept && NextAction != null;
            var callCompleteAction = shouldIntercept && CompleteAction != null;

            var state = shouldIntercept ? BeforeAction!(target, targetMethod, args) : default;

            IEnumerable Intercept(IEnumerable inner)
            {
                foreach (var item in inner)
                {
                    if (callNextAction)
                    {
                        NextAction!(state!, target, targetMethod, args, item);
                    }

                    yield return item;
                }

                if (callCompleteAction)
                {
                    CompleteAction!(state!, target, targetMethod, args);
                }
            }

            return ExceptionExtensions.UnwrapTargetInvocationException<IEnumerable?>(
                () =>
                {
                    var result = targetMethod.Invoke(target, args) as IEnumerable;

                    if (result != null)
                    {
                        return Intercept(result);
                    }
                    else
                    {
                        if (callCompleteAction)
                        {
                            CompleteAction!(state!, target, targetMethod, args);
                        }

                        return result;
                    }
                },
                exception =>
                {
                    if (shouldIntercept && ErrorAction != null)
                    {
                        ErrorAction(state!, target, targetMethod, args, exception);
                    }

                    return (default, false);
                });
        }

        private static readonly MethodInfo InterceptAsIEnumerableOfMethod =
            typeof(InterceptEnumerableMethod<TState>)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsIEnumerableOf));

        private IEnumerable<T>? InterceptAsIEnumerableOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                BeforeAction != null &&
                (ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args));

            var callNextAction = shouldIntercept && NextAction != null;
            var callCompleteAction = shouldIntercept && CompleteAction != null;

            var state = shouldIntercept ? BeforeAction!(target, targetMethod, args) : default;

            IEnumerable<T> Intercept(IEnumerable<T> inner)
            {
                foreach (var item in inner)
                {
                    if (callNextAction)
                    {
                        NextAction!(state!, target, targetMethod, args, item);
                    }

                    yield return item;
                }

                if (callCompleteAction)
                {
                    CompleteAction!(state!, target, targetMethod, args);
                }
            }


            return ExceptionExtensions.UnwrapTargetInvocationException<IEnumerable<T>?>(
                () =>
                {
                    var result = targetMethod.Invoke(target, args) as IEnumerable<T>;

                    if (result != null)
                    {
                        return Intercept(result);
                    }
                    else
                    {
                        if (callCompleteAction)
                        {
                            CompleteAction!(state!, target, targetMethod, args);
                        }

                        return result;
                    }
                },
                exception =>
                {
                    if (shouldIntercept && ErrorAction != null)
                    {
                        ErrorAction(state!, target, targetMethod, args, exception);
                    }

                    return (default, false);
                });
        }

        private static readonly MethodInfo InterceptAsIAsyncEnumerableOfMethod =
            typeof(InterceptEnumerableMethod<TState>)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsIAsyncEnumerableOf));

        private IAsyncEnumerable<T>? InterceptAsIAsyncEnumerableOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                BeforeAction != null &&
                (ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args));

            var callNextAction = shouldIntercept && NextAction != null;
            var callCompleteAction = shouldIntercept && CompleteAction != null;

            var state = shouldIntercept ? BeforeAction!(target, targetMethod, args) : default;

            async IAsyncEnumerable<T> Intercept(IAsyncEnumerable<T> inner)
            {
                await foreach (var item in inner)
                {
                    if (callNextAction)
                    {
                        NextAction!(state!, target, targetMethod, args, item);
                    }

                    yield return item;
                }

                if (callCompleteAction)
                {
                    CompleteAction!(state!, target, targetMethod, args);
                }
            }

            return ExceptionExtensions.UnwrapTargetInvocationException<IAsyncEnumerable<T>?>(
                () =>
                {
                    var result = targetMethod.Invoke(target, args) as IAsyncEnumerable<T>;

                    if (result != null)
                    {
                        return Intercept(result);
                    }
                    else
                    {
                        if (callCompleteAction)
                        {
                            CompleteAction!(state!, target, targetMethod, args);
                        }

                        return result;
                    }
                },
                exception =>
                {
                    if (shouldIntercept && ErrorAction != null)
                    {
                        ErrorAction(state!, target, targetMethod, args, exception);
                    }

                    return (default, false);
                });
        }

        private static readonly MethodInfo InterceptAsIObservableOfMethod =
            typeof(InterceptEnumerableMethod<TState>)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsIObservableOf));

        private IObservable<T>? InterceptAsIObservableOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                BeforeAction != null &&
                (ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args));

            var callNextAction = shouldIntercept && NextAction != null;
            var callCompleteAction = shouldIntercept && CompleteAction != null;

            var state = shouldIntercept ? BeforeAction!(target, targetMethod, args) : default;

            IObservable<T> Intercept(IObservable<T> inner)
            {
                return new IntercepterObservable<T>(
                    inner,
                    new IntercepterObserver<T>(
                        completed: () =>
                        {
                            if (callCompleteAction)
                            {
                                CompleteAction!(state!, target, targetMethod, args);
                            }
                        },
                        next: value =>
                        {
                            if (callNextAction)
                            {
                                NextAction!(state!, target, targetMethod, args, value);
                            }
                        },
                        error: exception =>
                        {
                            if (shouldIntercept && ErrorAction != null)
                            {
                                ErrorAction(state!, target, targetMethod, args, exception);
                            }
                        }));
            }

            return ExceptionExtensions.UnwrapTargetInvocationException<IObservable<T>?>(
                () =>
                {
                    var result = targetMethod.Invoke(target, args) as IObservable<T>;

                    if (result != null)
                    {
                        return Intercept(result);
                    }
                    else
                    {
                        if (callCompleteAction)
                        {
                            CompleteAction!(state!, target, targetMethod, args);
                        }

                        return result;
                    }
                },
                exception =>
                {
                    if (shouldIntercept && ErrorAction != null)
                    {
                        ErrorAction(state!, target, targetMethod, args, exception);
                    }

                    return (default, false);
                });
        }
    }
}
