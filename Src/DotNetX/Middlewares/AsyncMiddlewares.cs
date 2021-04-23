using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Middlewares
{
    // Async Middlewares

    public delegate Task<TResult> AsyncMiddlewareFunc<TContext, TResult>(
        TContext context, 
        CancellationToken cancellationToken);
    
    public delegate Task<TResult> SimpleAsyncMiddlewareFunc<TContext, TResult>(TContext context);
    
    public delegate Task<TResult> AsyncMiddleware<TContext, TResult>(
        TContext context, 
        AsyncMiddlewareFunc<TContext, TResult> next, 
        CancellationToken cancellationToken);

    public static class AsyncMiddleware
    {
        // Constants

        public static AsyncMiddlewareFunc<TContext, TResult> ConstantFunc<TContext, TResult>(TResult result) => 
            (_, _) => Task.FromResult(result);

        public static AsyncMiddleware<TContext, TResult> Constant<TContext, TResult>(TResult result) =>
            (_, _, _) => Task.FromResult(result);


        // Combine a Func with a Middleware

        public static AsyncMiddlewareFunc<TContext, TResult> Combine<TContext, TResult>(
            this AsyncMiddlewareFunc<TContext, TResult> func,
            AsyncMiddleware<TContext, TResult> middleware) =>
            (context, ct) => middleware(context, func, ct);

        public static AsyncMiddlewareFunc<TContext, TResult> Combine<TContext, TResult>(
            this SimpleAsyncMiddlewareFunc<TContext, TResult> func,
            AsyncMiddleware<TContext, TResult> middleware) =>
            (context, ct) => middleware(context, (c, _) => func(c), ct);

        public static AsyncMiddlewareFunc<TContext, TResult> Combine<TContext, TResult>(
            this AsyncMiddleware<TContext, TResult> middleware,
            AsyncMiddlewareFunc<TContext, TResult> func) =>
            (context, ct) => middleware(context, func, ct);

        public static AsyncMiddlewareFunc<TContext, TResult> Combine<TContext, TResult>(
            this AsyncMiddleware<TContext, TResult> middleware,
            SimpleAsyncMiddlewareFunc<TContext, TResult> func) =>
            (context, ct) => middleware(context, (c, _) => func(c), ct);


        // Compose multiple middlewares into one

        public static AsyncMiddleware<TContext, TResult> Compose<TContext, TResult>(
            this IEnumerable<AsyncMiddleware<TContext, TResult>> middlewares)
        {
            return async (context, next, ct) =>
            {
                var enumerator = middlewares.GetEnumerator();

                async Task<TResult> LocalNext(TContext ctx, CancellationToken ct)
                {
                    if (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        return await current(ctx, LocalNext, ct);
                    }
                    else
                    {
                        return await next (ctx, ct);
                    }
                }

                return await LocalNext(context, ct);
            };
        }

        public static AsyncMiddleware<TContext, TResult> Compose<TContext, TResult>(
            this AsyncMiddleware<TContext, TResult> middleware,
            params AsyncMiddleware<TContext, TResult>[] middlewares) =>
            middleware.Singleton().Concat(middlewares).Compose();


        // Switch

        public static AsyncMiddleware<TContext, TResult> Switch<TContext, TResult>(
            this Func<TContext, AsyncMiddleware<TContext, TResult>> switchFunc) =>
            (context, next, ct) => switchFunc(context)(context, next, ct);

        public static AsyncMiddleware<TContext, TResult> Switch<TContext, TResult>(
            this Func<TContext, Task<AsyncMiddleware<TContext, TResult>>> switchFunc) =>
            async (context, next, ct) =>
            {
                var middleware = await switchFunc(context);
                return await middleware(context, next, ct);
            };

        public static AsyncMiddleware<TContext, TResult> Switch<TContext, TResult>(
            this Func<TContext, CancellationToken, Task<AsyncMiddleware<TContext, TResult>>> switchFunc) =>
            async (context, next, ct) =>
            {
                var middleware = await switchFunc(context, ct);
                return await middleware(context, next, ct);
            };


        // Choose

        public static AsyncMiddleware<TContext, TResult> Choose<TContext, TResult>(
            this IEnumerable<AsyncMiddleware<TContext, TResult>> choices,
            Func<TResult, TContext, bool> wasChosen,
            AsyncMiddleware<TContext, TResult> defaultAction) =>
            async (context, next, ct) =>
            {
                foreach (var middleware in choices)
                {
                    var result = await middleware(context, next, ct);

                    if (wasChosen(result, context))
                    {
                        return result;
                    }
                }

                return await defaultAction (context, next, ct);
            };

        public static AsyncMiddleware<TContext, TResult> Choose<TContext, TResult>(
            this IEnumerable<AsyncMiddleware<TContext, TResult>> choices,
            Func<TResult, TContext, Task<bool>> wasChosen,
            AsyncMiddleware<TContext, TResult> defaultAction) =>
            async (context, next, ct) =>
            {
                foreach (var middleware in choices)
                {
                    var result = await middleware(context, next, ct);

                    if (await wasChosen(result, context))
                    {
                        return result;
                    }
                }

                return await defaultAction (context, next, ct);
            };
    }
}
