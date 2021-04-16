using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetX.Middlewares
{
    // Sync Middlewares

    public delegate TResult SyncMiddlewareFunc<TContext, TResult>(TContext context);
    
    public delegate TResult SyncMiddleware<TContext, TResult>(TContext context, SyncMiddlewareFunc<TContext, TResult> next);

    public static class SyncMiddleware
    {
        // Constants

        public static SyncMiddlewareFunc<TContext, TResult> ConstantFunc<TContext, TResult>(TResult result) => 
            _ => result;

        public static SyncMiddleware<TContext, TResult> Constant<TContext, TResult>(TResult result) =>
            (_, _) => result;


        // Combine a Func with a Middleware

        public static SyncMiddlewareFunc<TContext, TResult> Combine<TContext, TResult>(
            this SyncMiddlewareFunc<TContext, TResult> func,
            SyncMiddleware<TContext, TResult> middleware) =>
            context => middleware(context, func);

        public static SyncMiddlewareFunc<TContext, TResult> Combine<TContext, TResult>(
            this SyncMiddleware<TContext, TResult> middleware,
            SyncMiddlewareFunc<TContext, TResult> func) =>
            context => middleware(context, func);


        // Compose multiple middlewares into one

        public static SyncMiddleware<TContext, TResult> Compose<TContext, TResult>(
            this IEnumerable<SyncMiddleware<TContext, TResult>> middlewares)
        {
            return (context, next) =>
            {
                var enumerator = middlewares.GetEnumerator();

                TResult LocalNext(TContext ctx)
                {
                    if (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        return current(ctx, LocalNext);
                    }
                    else
                    {
                        return next(ctx);
                    }
                }

                return LocalNext(context);
            };
        }

        public static SyncMiddleware<TContext, TResult> Compose<TContext, TResult>(
            this SyncMiddleware<TContext, TResult> middleware,
            params SyncMiddleware<TContext, TResult>[] middlewares) =>
            middleware.Singleton().Concat(middlewares).Compose();


        // Switch

        public static SyncMiddleware<TContext, TResult> Switch<TContext, TResult>(
            this Func<TContext, SyncMiddleware<TContext, TResult>> switchFunc) =>
            (context, next) => switchFunc(context)(context, next);


        // Choose

        public static SyncMiddleware<TContext, TResult> Choose<TContext, TResult>(
            this IEnumerable<SyncMiddleware<TContext, TResult>> choices,
            Func<TResult, TContext, bool> wasChosen,
            SyncMiddleware<TContext, TResult> defaultAction) =>
            (context, next) =>
            {
                foreach (var middleware in choices)
                {
                    var result = middleware(context, next);

                    if (wasChosen(result, context))
                    {
                        return result;
                    }
                }

                return defaultAction(context, next);
            };
    }
}
