using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Middlewares
{
    // Task Middlewares

    public delegate Task TaskMiddlewareFunc<TContext>(
        TContext context, 
        CancellationToken cancellationToken);

    public delegate Task SimpleTaskMiddlewareFunc<TContext>(TContext context);

    public delegate Task TaskMiddleware<TContext>(
        TContext context, 
        TaskMiddlewareFunc<TContext> next,
        CancellationToken cancellationToken);

    public static class TaskMiddleware
    {

        // Combine a Func with a Middleware

        public static TaskMiddlewareFunc<TContext> Combine<TContext>(
            this TaskMiddlewareFunc<TContext> func,
            TaskMiddleware<TContext> middleware) =>
            (context, ct) => middleware(context, func, ct);

        public static TaskMiddlewareFunc<TContext> Combine<TContext>(
            this TaskMiddleware<TContext> middleware,
            TaskMiddlewareFunc<TContext> func) =>
            (context, ct) => middleware(context, func, ct);

        public static TaskMiddlewareFunc<TContext> Combine<TContext>(
            this SimpleTaskMiddlewareFunc<TContext> func,
            TaskMiddleware<TContext> middleware) =>
            (context, ct) => middleware(context, (c, _) => func(c), ct);

        public static TaskMiddlewareFunc<TContext> Combine<TContext>(
            this TaskMiddleware<TContext> middleware,
            SimpleTaskMiddlewareFunc<TContext> func) =>
            (context, ct) => middleware(context, (c, _) => func(c), ct);


        // Compose multiple middlewares into one

        public static TaskMiddleware<TContext> Compose<TContext>(
            this IEnumerable<TaskMiddleware<TContext>> middlewares)
        {
            return async (context, next, ct) =>
            {
                var enumerator = middlewares.GetEnumerator();

                async Task LocalNext(TContext ctx, CancellationToken ct)
                {
                    if (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        await current(ctx, LocalNext, ct);
                    }
                    else
                    {
                        await next(ctx, ct);
                    }
                }

                await LocalNext(context, ct);
            };
        }

        public static TaskMiddleware<TContext> Compose<TContext>(
            this TaskMiddleware<TContext> middleware,
            params TaskMiddleware<TContext>[] middlewares) =>
            middleware.Singleton().Concat(middlewares).Compose();


        // Switch

        public static TaskMiddleware<TContext> Switch<TContext>(
            this Func<TContext, TaskMiddleware<TContext>> switchFunc) =>
            (context, next, ct) => switchFunc(context)(context, next, ct);

        public static TaskMiddleware<TContext> Switch<TContext>(
            this Func<TContext, Task<TaskMiddleware<TContext>>> switchFunc) =>
            async (context, next, ct) =>
            {
                var middleware = await switchFunc(context);
                await middleware(context, next, ct);
            };

        public static TaskMiddleware<TContext> Switch<TContext>(
            this Func<TContext, CancellationToken, Task<TaskMiddleware<TContext>>> switchFunc) =>
            async (context, next, ct) =>
            {
                var middleware = await switchFunc(context, ct);
                await middleware(context, next, ct);
            };

        // Choose

        public static TaskMiddleware<TContext> Choose<TContext>(
            this IEnumerable<TaskMiddleware<TContext>> choices,
            Func<TContext, bool> wasChosen,
            TaskMiddleware<TContext> defaultAction) =>
            async (context, next, ct) =>
            {
                foreach (var middleware in choices)
                {
                    await middleware(context, next, ct);

                    if (wasChosen(context))
                    {
                        return;
                    }
                }

                await defaultAction(context, next, ct);
            };

        public static TaskMiddleware<TContext> Choose<TContext>(
            this IEnumerable<TaskMiddleware<TContext>> choices,
            Func<TContext, Task<bool>> wasChosen,
            TaskMiddleware<TContext> defaultAction) =>
            async (context, next, ct) =>
            {
                foreach (var middleware in choices)
                {
                    await middleware(context, next, ct);

                    if (await wasChosen(context))
                    {
                        return;
                    }
                }

                await defaultAction(context, next, ct);
            };
    }
}
