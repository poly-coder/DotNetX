using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetX.Middlewares
{
    // Void Sync Middlewares

    public delegate void VoidSyncMiddlewareFunc<TContext>(TContext context);

    public delegate void VoidSyncMiddleware<TContext>(TContext context, VoidSyncMiddlewareFunc<TContext> next);

    public static class VoidSyncMiddleware
    {

        // Combine a Func with a Middleware

        public static VoidSyncMiddlewareFunc<TContext> Combine<TContext>(
            this VoidSyncMiddlewareFunc<TContext> func,
            VoidSyncMiddleware<TContext> middleware) =>
            context => middleware(context, func);

        public static VoidSyncMiddlewareFunc<TContext> Combine<TContext>(
            this VoidSyncMiddleware<TContext> middleware,
            VoidSyncMiddlewareFunc<TContext> func) =>
            context => middleware(context, func);


        // Compose multiple middlewares into one

        public static VoidSyncMiddleware<TContext> Compose<TContext>(
            this IEnumerable<VoidSyncMiddleware<TContext>> middlewares)
        {
            return (context, next) =>
            {
                var enumerator = middlewares.GetEnumerator();

                void LocalNext(TContext ctx)
                {
                    if (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        current(ctx, LocalNext);
                    }
                    else
                    {
                        next(ctx);
                    }
                }

                LocalNext(context);
            };
        }

        public static VoidSyncMiddleware<TContext> Compose<TContext>(
            this VoidSyncMiddleware<TContext> middleware,
            params VoidSyncMiddleware<TContext>[] middlewares) =>
            middleware.Singleton().Concat(middlewares).Compose();


        // Switch

        public static VoidSyncMiddleware<TContext> Switch<TContext>(
            this Func<TContext, VoidSyncMiddleware<TContext>> switchFunc) =>
            (context, next) => switchFunc(context)(context, next);


        // Choose

        public static VoidSyncMiddleware<TContext> Choose<TContext>(
            this IEnumerable<VoidSyncMiddleware<TContext>> choices,
            Func<TContext, bool> wasChosen,
            VoidSyncMiddleware<TContext> defaultAction) =>
            (context, next) =>
            {
                foreach (var middleware in choices)
                {
                    middleware(context, next);

                    if (wasChosen(context))
                    {
                        return;
                    }
                }

                defaultAction(context, next);
            };
    }
}
