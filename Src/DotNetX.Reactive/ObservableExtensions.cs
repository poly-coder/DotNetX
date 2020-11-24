using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Reactive
{
    public static class ObservableExtensions
    {
        #region [ AsUpdaters ]

        public static IObservable<Func<T, T>> AsUpdaters<S, T>(
            this IObservable<S> source,
            Func<S, Func<T, T>> makeUpdater) =>
            source.Select(makeUpdater);

        #endregion [ AsUpdaters ]

        #region [ Select ]

        public static IObservable<IObservable<B>> Select<A, B>(
            this IObservable<A> source,
            Func<A, Task<B>> select) =>
            source.Select(a => Observable.FromAsync(() => select(a)));

        public static IObservable<IObservable<Unit>> Select<A>(
            this IObservable<A> source,
            Func<A, Task> select) =>
            source.Select(a => Observable.FromAsync(() => select(a)));

        public static IObservable<IObservable<B>> Select<A, B>(
            this IObservable<A> source,
            Func<A, CancellationToken, Task<B>> select) =>
            source.Select(a => Observable.FromAsync((c) => select(a, c)));

        public static IObservable<IObservable<Unit>> Select<A>(
            this IObservable<A> source,
            Func<A, CancellationToken, Task> select) =>
            source.Select(a => Observable.FromAsync((c) => select(a, c)));

        public static IObservable<IObservable<B>> Select<A, B>(
            this IObservable<A> source,
            Func<A, Task<B>> select,
            IScheduler scheduler) =>
            source.Select(a => Observable.FromAsync(() => select(a), scheduler));

        public static IObservable<IObservable<Unit>> Select<A>(
            this IObservable<A> source,
            Func<A, Task> select,
            IScheduler scheduler) =>
            source.Select(a => Observable.FromAsync(() => select(a), scheduler));

        public static IObservable<IObservable<B>> Select<A, B>(
            this IObservable<A> source,
            Func<A, CancellationToken, Task<B>> select,
            IScheduler scheduler) =>
            source.Select(a => Observable.FromAsync((c) => select(a, c), scheduler));

        public static IObservable<IObservable<Unit>> Select<A>(
            this IObservable<A> source,
            Func<A, CancellationToken, Task> select,
            IScheduler scheduler) =>
            source.Select(a => Observable.FromAsync((c) => select(a, c), scheduler));

        public static IObservable<IObservable<T>> Select<T>(
            this IObservable<Unit> source,
            Func<Task<T>> select) =>
            source.Select(_ => Observable.FromAsync(() => select()));

        public static IObservable<IObservable<Unit>> Select(
            this IObservable<Unit> source,
            Func<Task> select) =>
            source.Select(_ => Observable.FromAsync(() => select()));

        public static IObservable<IObservable<T>> Select<T>(
            this IObservable<Unit> source,
            Func<CancellationToken, Task<T>> select) =>
            source.Select(_ => Observable.FromAsync((c) => select(c)));

        public static IObservable<IObservable<Unit>> Select(
            this IObservable<Unit> source,
            Func<CancellationToken, Task> select) =>
            source.Select(_ => Observable.FromAsync((c) => select(c)));

        public static IObservable<IObservable<T>> Select<T>(
            this IObservable<Unit> source,
            Func<Task<T>> select,
            IScheduler scheduler) =>
            source.Select(_ => Observable.FromAsync(() => select(), scheduler));

        public static IObservable<IObservable<Unit>> Select(
            this IObservable<Unit> source,
            Func<Task> select,
            IScheduler scheduler) =>
            source.Select(_ => Observable.FromAsync(() => select(), scheduler));

        public static IObservable<IObservable<T>> Select<T>(
            this IObservable<Unit> source,
            Func<CancellationToken, Task<T>> select,
            IScheduler scheduler) =>
            source.Select(_ => Observable.FromAsync((c) => select(c), scheduler));

        public static IObservable<IObservable<Unit>> Select(
            this IObservable<Unit> source,
            Func<CancellationToken, Task> select,
            IScheduler scheduler) =>
            source.Select(_ => Observable.FromAsync((c) => select(c), scheduler));

        #endregion [ Select ]
    }

}
