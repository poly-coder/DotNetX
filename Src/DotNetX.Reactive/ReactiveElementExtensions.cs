using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Reactive
{

    public static class ReactiveElementExtensions
    {
        public static void DeferDispose(this IReactiveElement self, Action dispose) =>
            self.DeferDispose(new Disposable(dispose));

        public static TReactive CreateReactive<T, TReactive>(
            this IReactiveElement self,
            IObservable<T> valueStream,
            Func<IObservable<T>, TReactive> factory)
            where TReactive : class, IUpdatableElement, IDisposable
        {
            if (self is null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (valueStream is null)
            {
                throw new ArgumentNullException(nameof(valueStream));
            }

            var reactive = factory(valueStream);

            self.Effect(reactive.ValueChanged, self.StateChanged.Call);

            return self.DeferDispose(reactive);
        }

        private static Computed<T> ComputedFactory<T>(IObservable<T> stream) =>
            new Computed<T>(stream);

        public static Computed<T> RawComputed<T>(
            this IReactiveElement self,
            IObservable<T> valueStream) =>
            self.CreateReactive(valueStream, ComputedFactory);

        #region [ Effect ]

        public static IDisposable Effect<T>(this IReactiveElement self, IObservable<T> stream, Action<T> reaction)
        {
            var context = SynchronizationContext.Current;
            if (context != null)
            {
                stream = stream.ObserveOn(context);
            }
            return self.DeferDispose(stream.Subscribe(reaction));
        }

        public static IDisposable Effect(this IReactiveElement self, IObservable<Unit> stream, Action reaction) =>
            self.Effect(stream, _ => reaction());

        public static IDisposable Effect(this IReactiveElement self, IObservable<Unit> stream) =>
            self.Effect(stream, _ => { });

        public static IDisposable Effect<T>(this IReactiveElement self, Computed<T> computed, Action<T> reaction) =>
            self.Effect(computed.Stream, reaction);

        public static IDisposable Effect<T>(this IReactiveElement self, Property<T> property, Action<T> reaction) =>
            self.Effect(property.Stream, reaction);

        public static IDisposable Effect<T>(this IReactiveElement self, Command<T> command, Action<T> reaction) =>
            self.Effect(command.Stream, reaction);

        public static IDisposable Effect(this IReactiveElement self, Command command, Action reaction) =>
            self.Effect(command.Stream, reaction);

        public static IDisposable Effect(this IReactiveElement self, Command command) =>
            self.Effect(command.Stream);

        #endregion [ Effect ]

        #region [ Property ]

        public static Property<T> Property<T>(
            this IReactiveElement self,
            T? initialValue,
            Func<T?, T?, T?>? coerceValue = null,
            IEqualityComparer<T>? comparer = null)
        {
            if (self is null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            var property = self.DeferDispose(
                new Property<T>(initialValue, coerceValue, comparer));

            return property;
        }

        public static Property<T> Property<T>(
            this IReactiveElement self,
            T? initialValue,
            Func<T?, T?>? coerceValue,
            IEqualityComparer<T>? comparer = null)
        {
            Func<T?, T?, T?>? fullCoerceValue = null;

            if (coerceValue != null)
            {
                fullCoerceValue = (newValue, _current) => coerceValue(newValue);
            }

            return self.Property(
                initialValue,
                fullCoerceValue,
                comparer);
        }

        #endregion [ Property ]

        #region [ Command ]

        public static Command<T> Command<T>(this IReactiveElement self)
        {
            if (self is null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            var command = self.DeferDispose(new Command<T>());

            //// Logging
            //self.Effect(
            //    command,
            //    value => Console.WriteLine($"OnCommand: {self.GetType().Name}({value})"));

            return command;
        }

        public static Command Command(this IReactiveElement self)
        {
            if (self is null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            var command = self.DeferDispose(new Command());

            //// Logging
            //self.Effect(
            //    command,
            //    () => Console.WriteLine($"OnCommand: {self.GetType().Name}()"));

            return command;
        }

        #endregion [ Command ]

        #region [ Reactive ]

        public static TReactive Reactive<TValue, TReactive>(
            this IReactiveElement self,
            Func<IObservable<TValue>, TReactive> factory,
            IObservable<TValue> source,
            IEqualityComparer<TValue>? comparer = null)
            where TReactive : class, IUpdatableElement, IDisposable
        {
            if (comparer != null)
            {
                source = source.DistinctUntilChanged(comparer ?? EqualityComparer<TValue>.Default);
            }

            return self.CreateReactive(source, factory);
        }

        public static TReactive Reactive<TValue, TState, TReactive>(
            this IReactiveElement self,
            Func<IObservable<TValue>, TReactive> factory,
            IObservable<TState> source,
            Func<TState, TValue> select,
            IEqualityComparer<TValue>? comparer = null)
            where TReactive : class, IUpdatableElement, IDisposable =>
            self.Reactive(factory, source.Select(select), comparer);

        public static TReactive Reactive<TValue, TEvent, TReactive>(
            this IReactiveElement self,
            Func<IObservable<TValue>, TReactive> factory,
            TValue initialValue,
            IObservable<TEvent> events,
            Func<TValue, TEvent, TValue> update,
            IEqualityComparer<TValue>? comparer = null)
            where TReactive : class, IUpdatableElement, IDisposable =>
            self.Reactive(
                factory,
                events
                    .Scan(initialValue, update)
                    .StartWith(initialValue), comparer);

        public static TReactive Reactive<TValue, TReactive>(
            this IReactiveElement self,
            Func<IObservable<TValue>, TReactive> factory,
            TValue initialValue,
            IObservable<Func<TValue, TValue>> updates,
            IEqualityComparer<TValue>? comparer = null)
            where TReactive : class, IUpdatableElement, IDisposable =>
            self.Reactive(
                factory,
                updates
                    .Scan(initialValue, (acc, update) => update(acc))
                    .StartWith(initialValue), comparer);

        public static TReactive Reactive<TValue, TState, TEvent, TReactive>(
            this IReactiveElement self,
            Func<IObservable<TValue>, TReactive> factory,
            TState initialState,
            IObservable<TEvent> events,
            Func<TState, TEvent, TState> update,
            Func<TState, TValue> select,
            IEqualityComparer<TValue>? comparer = null)
            where TReactive : class, IUpdatableElement, IDisposable =>
            self.Reactive(
                factory,
                events
                    .Scan(initialState, update)
                    .StartWith(initialState)
                    .Select(select), comparer);

        public static TReactive Reactive<TValue, TState, TReactive>(
            this IReactiveElement self,
            Func<IObservable<TValue>, TReactive> factory,
            TState initialState,
            IObservable<Func<TState, TState>> updates,
            Func<TState, TValue> select,
            IEqualityComparer<TValue>? comparer = null)
            where TReactive : class, IUpdatableElement, IDisposable =>
            self.Reactive(
                factory,
                updates
                    .Scan(initialState, (acc, update) => update(acc))
                    .StartWith(initialState)
                    .Select(select), comparer);

        private static IObservable<Func<LoadingResult<TValue, TError>, LoadingResult<TValue, TError>>> CreateUpdates<TValue, TCommand, TError>(
            TCommand? initialCommand,
            IObservable<TCommand?> reloadCommand,
            Func<TCommand?, IObservable<TValue>> loadProperty,
            Func<Exception, TError> onError)
        {
            return reloadCommand
                .StartWith(initialCommand)
                .Select(command =>
                    loadProperty(command)
                        .LastAsync()
                        .Materialize()
                        .Select<Notification<TValue>, Func<LoadingResult<TValue, TError>, LoadingResult<TValue, TError>>>(notification =>
                        {
                            switch (notification.Kind)
                            {
                                case NotificationKind.OnNext:
                                    {
                                        LoadingResult<TValue, TError> Update(LoadingResult<TValue, TError> previous) =>
                                            previous.SetValue(notification.Value);
                                        return Update;

                                    }
                                case NotificationKind.OnError:
                                    {
                                        LoadingResult<TValue, TError> Update(LoadingResult<TValue, TError> previous) =>
                                            previous.SetError(onError(notification.Exception!));
                                        return Update;
                                    }
                                default:
                                    {
                                        LoadingResult<TValue, TError> Update(LoadingResult<TValue, TError> previous) =>
                                            previous;
                                        return Update;
                                    }
                            }
                        })
                        .StartWith(previous => previous))
                .Switch();
        }

        public static TReactive Reactive<TValue, TCommand, TError, TReactive>(
            this IReactiveElement self,
            Func<IObservable<LoadingResult<TValue, TError>>, TReactive> factory,
            TCommand? initialCommand,
            IObservable<TCommand?> reloadCommand,
            Func<TCommand?, IObservable<TValue>> loadProperty,
            Func<Exception, TError> onError)
            where TReactive : class, IUpdatableElement, IDisposable
        {
            var updates = CreateUpdates(initialCommand, reloadCommand, loadProperty, onError)
                .Replay(1);

            self.DeferDispose(updates.Connect());

            return self.Reactive(
                factory,
                LoadingResult.Create<TValue, TError>().StartLoading(),
                updates);
        }

        public static TReactive Reactive<TValue, TError, TReactive>(
            this IReactiveElement self,
            Func<IObservable<LoadingResult<TValue, TError>>, TReactive> factory,
            IObservable<Unit> reloadCommand,
            Func<IObservable<TValue>> loadProperty,
            Func<Exception, TError> onError)
            where TReactive : class, IUpdatableElement, IDisposable =>
            self.Reactive(
                factory,
                Unit.Default,
                reloadCommand,
                (_) => loadProperty(),
                onError);

        public static TReactive Reactive<TValue, TCommand, TError, TReactive>(
            this IReactiveElement self,
            Func<IObservable<LoadingResult<TValue, TError>>, TReactive> factory,
            TCommand? initialCommand,
            IObservable<TCommand?> reloadCommand,
            Func<TCommand?, Task<TValue>> loadProperty,
            Func<Exception, TError> onError)
            where TReactive : class, IUpdatableElement, IDisposable =>
            self.Reactive(
                factory,
                initialCommand,
                reloadCommand,
                c => Observable.FromAsync(() => loadProperty(c)),
                onError);

        public static TReactive Reactive<TValue, TCommand, TError, TReactive>(
            this IReactiveElement self,
            Func<IObservable<LoadingResult<TValue, TError>>, TReactive> factory,
            IObservable<TCommand?> reloadCommand,
            Func<TCommand?, Task<TValue>> loadProperty,
            Func<Exception, TError> onError)
            where TReactive : class, IUpdatableElement, IDisposable =>
            self.Reactive(factory, default, reloadCommand, loadProperty, onError);

        public static TReactive Reactive<TValue, TError, TReactive>(
            this IReactiveElement self,
            Func<IObservable<LoadingResult<TValue, TError>>, TReactive> factory,
            IObservable<Unit> reloadCommand,
            Func<Task<TValue>> loadProperty,
            Func<Exception, TError> onError)
            where TReactive : class, IUpdatableElement, IDisposable =>
            self.Reactive(factory, Unit.Default, reloadCommand, (_) => loadProperty(), onError);

        #endregion [ Reactive ]

        #region [ Computed ]

        public static Computed<TValue> Computed<TValue>(
            this IReactiveElement self,
            IObservable<TValue> source,
            IEqualityComparer<TValue>? comparer = null) =>
            self.Reactive(ComputedFactory, source, comparer);

        public static Computed<TValue> Computed<TValue, TState>(
            this IReactiveElement self,
            IObservable<TState> source,
            Func<TState, TValue> select,
            IEqualityComparer<TValue>? comparer = null) =>
            self.Reactive(ComputedFactory, source, select, comparer);

        public static Computed<T> Computed<T, TEvent>(
            this IReactiveElement self,
            T initialValue,
            IObservable<TEvent> events,
            Func<T, TEvent, T> update,
            IEqualityComparer<T>? comparer = null) =>
            self.Reactive(ComputedFactory, initialValue, events, update, comparer);

        public static Computed<TValue> Computed<TValue>(
            this IReactiveElement self,
            TValue initialValue,
            IObservable<Func<TValue, TValue>> updates,
            IEqualityComparer<TValue>? comparer = null) =>
            self.Reactive(ComputedFactory, initialValue, updates, comparer);

        public static Computed<TValue> Computed<TValue, TState, TEvent>(
            this IReactiveElement self,
            TState initialState,
            IObservable<TEvent> events,
            Func<TState, TEvent, TState> update,
            Func<TState, TValue> select,
            IEqualityComparer<TValue>? comparer = null) =>
            self.Reactive(ComputedFactory, initialState, events, update, select, comparer);

        public static Computed<TValue> Computed<TValue, TState>(
            this IReactiveElement self,
            TState initialState,
            IObservable<Func<TState, TState>> updates,
            Func<TState, TValue> select,
            IEqualityComparer<TValue>? comparer = null) =>
            self.Reactive(ComputedFactory, initialState, updates, select, comparer);

        public static Computed<LoadingResult<TValue, TError>> Computed<TValue, TCommand, TError>(
            this IReactiveElement self,
            TCommand? initialCommand,
            IObservable<TCommand?> reloadCommand,
            Func<TCommand?, IObservable<TValue>> loadProperty,
            Func<Exception, TError> onError) =>
            self.Reactive(ComputedFactory, initialCommand, reloadCommand, loadProperty, onError);

        public static Computed<LoadingResult<TValue, TError>> Computed<TValue, TError>(
            this IReactiveElement self,
            IObservable<Unit> reloadCommand,
            Func<IObservable<TValue>> loadProperty,
            Func<Exception, TError> onError) =>
            self.Reactive(ComputedFactory, reloadCommand, loadProperty, onError);

        public static Computed<LoadingResult<TValue, TError>> Computed<TValue, TCommand, TError>(
            this IReactiveElement self,
            TCommand? initialCommand,
            IObservable<TCommand?> reloadCommand,
            Func<TCommand?, Task<TValue>> loadProperty,
            Func<Exception, TError> onError) =>
            self.Reactive(ComputedFactory, initialCommand, reloadCommand, loadProperty, onError);

        public static Computed<LoadingResult<TValue, TError>> Computed<TValue, TCommand, TError>(
            this IReactiveElement self,
            IObservable<TCommand?> reloadCommand,
            Func<TCommand?, Task<TValue>> loadProperty,
            Func<Exception, TError> onError) =>
            self.Reactive(ComputedFactory, reloadCommand, loadProperty, onError);

        public static Computed<LoadingResult<TValue, TError>> Computed<TValue, TError>(
            this IReactiveElement self,
            IObservable<Unit> reloadCommand,
            Func<Task<TValue>> loadProperty,
            Func<Exception, TError> onError) =>
            self.Reactive(ComputedFactory, reloadCommand, loadProperty, onError);

        #endregion [ Computed ]

        #region [ WhereHasValue ]

        public static IObservable<T> WhereHasValue<T>(this IObservable<T?> source)
            where T : struct =>
            source
                .SelectMany(v => v.HasValue
                    ? v.Value.Singleton().ToObservable()
                    : Observable.Empty<T>());

        public static IObservable<T> WhereHasValue<T>(this IObservable<T> source)
            where T : class =>
            source
                .SelectMany(v => v != null
                    ? v.Singleton().ToObservable()
                    : Observable.Empty<T>());

        #endregion [ WhereHasValue ]

        #region [ TrackModel ]

        public static void TrackModel(this IReactiveElement parentModel, IReactiveElement subModel)
        {
            if (parentModel is null)
            {
                throw new ArgumentNullException(nameof(parentModel));
            }

            if (subModel is null)
            {
                throw new ArgumentNullException(nameof(subModel));
            }

            // Propagate ViewModel state changes to Component StateChanges
            parentModel.Effect(subModel.StateChanged, parentModel.StateChanged.Call);
        }

        public static TViewModel TrackModel<TViewModel>(this IReactiveElement parentModel, IServiceProvider serviceProvider)
            where TViewModel : class, IReactiveElement
        {
            var model = parentModel.DeferDispose(serviceProvider.CreateInstance<TViewModel>());

            parentModel.TrackModel(model);

            return model;
        }

        #endregion [ TrackModel ]

        #region [ Execute ]

        public static async Task Execute<TResponse>(
            this IReactiveElement self,
            Property<bool> isSubmitting,
            Property<Exception?> submitError,
            Func<Task<TResponse>> action,
            Func<TResponse, Task>? onSuccess = null,
            Func<Exception, Task>? onError = null)
        {
            if (self is null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (isSubmitting is null)
            {
                throw new ArgumentNullException(nameof(isSubmitting));
            }

            if (submitError is null)
            {
                throw new ArgumentNullException(nameof(submitError));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            isSubmitting.Set(true);
            submitError.Set(null);
            try
            {
                var response = await action();

                isSubmitting.Set(false);

                if (onSuccess != null)
                {
                    await onSuccess(response);
                }
            }
            catch (Exception exception)
            {
                submitError.Set(exception);

                if (onError != null)
                {
                    await onError(exception);
                }
            }
            finally
            {
                isSubmitting.Set(false);
            }
        }

        public static Task Execute<TResponse>(
            this IReactiveElement self,
            Property<bool> isSubmitting,
            Property<Exception?> submitError,
            Func<Task<TResponse>> action,
            Action<TResponse>? onSuccessSync,
            Action<Exception>? onErrorSync = null)
        {
            Func<TResponse, Task>? onSuccess = null;
            Func<Exception, Task>? onError = null;

            if (onSuccessSync != null)
            {
                onSuccess = response =>
                {
                    onSuccessSync(response);
                    return Task.CompletedTask;
                };
            }

            if (onErrorSync != null)
            {
                onError = (exception) =>
                {
                    onErrorSync(exception);
                    return Task.CompletedTask;
                };
            }

            return self.Execute(isSubmitting, submitError, action, onSuccess, onError);
        }

        public static Task Execute(
            this IReactiveElement self,
            Property<bool> isSubmitting,
            Property<Exception?> submitError,
            Func<Task> action,
            Func<Task>? onSuccess = null,
            Func<Exception, Task>? onError = null)
        {
            async Task<bool> BoolAction()
            {
                await action();
                return true;
            }

            Func<bool, Task>? onBoolSuccess = null;
            if (onSuccess != null)
            {
                onBoolSuccess = _ => onSuccess();
            }

            return self.Execute(isSubmitting, submitError, BoolAction, onBoolSuccess, onError);
        }

        public static Task Execute(
            this IReactiveElement self,
            Property<bool> isSubmitting,
            Property<Exception?> submitError,
            Func<Task> action,
            Action? onSuccessSync,
            Action<Exception>? onErrorSync = null)
        {
            Func<Task>? onSuccess = null;
            Func<Exception, Task>? onError = null;

            if (onSuccessSync != null)
            {
                onSuccess = () =>
                {
                    onSuccessSync();
                    return Task.CompletedTask;
                };
            }

            if (onErrorSync != null)
            {
                onError = (exception) =>
                {
                    onErrorSync(exception);
                    return Task.CompletedTask;
                };
            }

            return self.Execute(isSubmitting, submitError, action, onSuccess, onError);
        }
        
        #endregion [ Execute ]
    }

}
