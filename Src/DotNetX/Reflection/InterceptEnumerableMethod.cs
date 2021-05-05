using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace DotNetX.Reflection
{
    public record InterceptEnumerableMethod(
        Action<object, MethodInfo, object?[]?>? BeforeAction = null,
        Action<object, MethodInfo, object?[]?, object?>? NextAction = null,
        Action<object, MethodInfo, object?[]?>? CompleteAction = null,
        Action<object, MethodInfo, object?[]?, Exception>? ErrorAction = null,
        Func<object, MethodInfo, object?[]?, bool>? ShouldInterceptAction = null)
        : IInterceptMethod
    {
        public static readonly InterceptEnumerableMethod Default = CreateDefaultOptions();

        private static InterceptEnumerableMethod CreateDefaultOptions()
        {
            return new InterceptEnumerableMethod();
        }

        public InterceptEnumerableMethod With(IInterceptEnumerableMethod interceptors)
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

        public InterceptEnumerableMethod Before(Action action)
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

        public InterceptEnumerableMethod Before(Action<object, MethodInfo, object?[]?> action)
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

        public InterceptEnumerableMethod Next(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                NextAction = (_, _, _, _) => action(),
            };
        }

        public InterceptEnumerableMethod Next(Action<object?> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                NextAction = (_, _, _, value) => action(value),
            };
        }

        public InterceptEnumerableMethod Next(Action<object, MethodInfo, object?[]?, object?> action)
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
        
        public InterceptEnumerableMethod Complete(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return this with
            {
                CompleteAction = (_, _, _) => action(),
            };
        }

        public InterceptEnumerableMethod Complete(Action<object, MethodInfo, object?[]?> action)
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

        public InterceptEnumerableMethod Error(Action action)
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

        public InterceptEnumerableMethod Error(Action<Exception> action)
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

        public InterceptEnumerableMethod Error(Action<object, MethodInfo, object?[]?, Exception> action)
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

        public InterceptEnumerableMethod ShouldIntercept(Func<bool> action)
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

        public InterceptEnumerableMethod ShouldIntercept(Func<object, MethodInfo, object?[]?, bool> action)
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
                result = method.Invoke(this, new object?[] { target, targetMethod, args });
                return true;
            }

            if (returnType.TryGetGenericParameters(typeof(IAsyncEnumerable<>), out tResult))
            {
                var method = InterceptAsIAsyncEnumerableOfMethod.MakeGenericMethod(tResult);
                result = method.Invoke(this, new object?[] { target, targetMethod, args });
                return true;
            }

            result = null;
            return false;
        }

        private IEnumerable? InterceptAsIEnumerable(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args);

            var callNextAction = shouldIntercept && NextAction != null;
            var callCompleteAction = shouldIntercept && CompleteAction != null;

            IEnumerable Intercept(IEnumerable inner)
            {
                var enumerator = inner.GetEnumerator();

                while (true)
                {
                    bool moved = false;
                    try
                    {
                        moved = enumerator.MoveNext();
                    }
                    catch (Exception exception)
                    {
                        if (shouldIntercept && ErrorAction != null)
                        {
                            ErrorAction(target, targetMethod, args, exception);
                        }

                        throw;
                    }

                    if (!moved)
                    {
                        break;
                    }

                    var item = enumerator.Current;

                    if (callNextAction)
                    {
                        NextAction!(target, targetMethod, args, item);
                    }

                    yield return item;
                }

                if (callCompleteAction)
                {
                    CompleteAction!(target, targetMethod, args);
                }
            }

            try
            {
                if (shouldIntercept && BeforeAction != null)
                {
                    BeforeAction(target, targetMethod, args);
                }

                var result = targetMethod.Invoke(target, args) as IEnumerable;

                if (result != null)
                {
                    return Intercept(result);
                }
                else
                {
                    if (callCompleteAction)
                    {
                        CompleteAction!(target, targetMethod, args);
                    }

                    return result;
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
                    ErrorAction(target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

        private static readonly MethodInfo InterceptAsIEnumerableOfMethod =
            typeof(InterceptEnumerableMethod)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsIEnumerableOf));

        private IEnumerable<T>? InterceptAsIEnumerableOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args);

            var callNextAction = shouldIntercept && NextAction != null;
            var callCompleteAction = shouldIntercept && CompleteAction != null;

            IEnumerable<T> Intercept(IEnumerable<T> inner)
            {
                foreach (var item in inner)
                {
                    if (callNextAction)
                    {
                        NextAction!(target, targetMethod, args, item);
                    }

                    yield return item;
                }

                if (callCompleteAction)
                {
                    CompleteAction!(target, targetMethod, args);
                }
            }

            try
            {
                if (shouldIntercept && BeforeAction != null)
                {
                    BeforeAction(target, targetMethod, args);
                }

                var result = targetMethod.Invoke(target, args) as IEnumerable<T>;

                if (result != null)
                {
                    return Intercept(result);
                }
                else
                {
                    if (callCompleteAction)
                    {
                        CompleteAction!(target, targetMethod, args);
                    }

                    return result;
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
                    ErrorAction(target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

        private static readonly MethodInfo InterceptAsIAsyncEnumerableOfMethod =
            typeof(InterceptEnumerableMethod)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsIAsyncEnumerableOf));

        private IAsyncEnumerable<T>? InterceptAsIAsyncEnumerableOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args);

            var callNextAction = shouldIntercept && NextAction != null;
            var callCompleteAction = shouldIntercept && CompleteAction != null;

            async IAsyncEnumerable<T> Intercept(IAsyncEnumerable<T> inner)
            {
                await foreach (var item in inner)
                {
                    if (callNextAction)
                    {
                        NextAction!(target, targetMethod, args, item);
                    }

                    yield return item;
                }

                if (callCompleteAction)
                {
                    CompleteAction!(target, targetMethod, args);
                }
            }

            try
            {
                if (shouldIntercept && BeforeAction != null)
                {
                    BeforeAction(target, targetMethod, args);
                }

                var result = targetMethod.Invoke(target, args) as IAsyncEnumerable<T>;

                if (result != null)
                {
                    return Intercept(result);
                }
                else
                {
                    if (callCompleteAction)
                    {
                        CompleteAction!(target, targetMethod, args);
                    }

                    return result;
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
                    ErrorAction(target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

        private static readonly MethodInfo InterceptAsIObservableOfMethod =
            typeof(InterceptEnumerableMethod)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(InterceptAsIObservableOf));

        private IObservable<T>? InterceptAsIObservableOf<T>(object target, MethodInfo targetMethod, object?[]? args)
        {
            bool shouldIntercept =
                ShouldInterceptAction == null ||
                ShouldInterceptAction(target, targetMethod, args);

            var callNextAction = shouldIntercept && NextAction != null;
            var callCompleteAction = shouldIntercept && CompleteAction != null;

            IObservable<T> Intercept(IObservable<T> inner)
            {
                return new IntercepterObservable<T>(
                    inner,
                    new IntercepterObserver<T>(
                        completed: () =>
                        {
                            if (callCompleteAction)
                            {
                                CompleteAction!(target, targetMethod, args);
                            }
                        },
                        next: value =>
                        {
                            if (callNextAction)
                            {
                                NextAction!(target, targetMethod, args, value);
                            }
                        },
                        error: exception =>
                        {
                            if (shouldIntercept && ErrorAction != null)
                            {
                                ErrorAction(target, targetMethod, args, exception);
                            }
                        }));
            }

            try
            {
                if (shouldIntercept && BeforeAction != null)
                {
                    BeforeAction(target, targetMethod, args);
                }

                var result = targetMethod.Invoke(target, args) as IObservable<T>;

                if (result != null)
                {
                    return Intercept(result);
                }
                else
                {
                    if (callCompleteAction)
                    {
                        CompleteAction!(target, targetMethod, args);
                    }

                    return result;
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
                    ErrorAction(target, targetMethod, args, exception);
                }

                ExceptionDispatchInfo.Throw(exception);

                throw;
            }
        }

    }
    class IntercepterObservable<T> : IObservable<T>
    {
        private readonly IObservable<T> inner;
        private readonly IObserver<T> observer;

        public IntercepterObservable(IObservable<T> inner, IObserver<T> observer)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
            this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
        }

        public IDisposable Subscribe(IObserver<T> obs)
        {
            if (obs is null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            var innerSubscription = inner
                .Subscribe(new IntercepterObserver<T>(
                    completed: () =>
                    {
                        observer.OnCompleted();
                        obs.OnCompleted();
                    },
                    next: value =>
                    {
                        observer.OnNext(value);
                        obs.OnNext(value);
                    }, 
                    error: exception =>
                    {
                        observer.OnError(exception);
                        obs.OnError(exception);
                    }));

            return innerSubscription;
        }
    }

    class IntercepterObserver<T> : IObserver<T>
    {
        private readonly Action completed;
        private readonly Action<Exception> error;
        private readonly Action<T> next;

        public IntercepterObserver(
            Action completed,
            Action<Exception> error,
            Action<T> next)
        {
            this.completed = completed;
            this.error = error;
            this.next = next;
        }

        public void OnCompleted()
        {
            completed();
        }

        public void OnError(Exception exception)
        {
            error(exception);
        }

        public void OnNext(T value)
        {
            next(value);
        }
    }
}
