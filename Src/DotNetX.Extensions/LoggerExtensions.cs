using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetX
{
    public static class LoggerExtensions
    {

        #region [ LogOperation<T> ]

        public static T LogOperation<T>(
            this ILogger logger,
            string caption,
            Func<T> operation,
            Func<T, string?>? showResult = null,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    var result = operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed, showResult?.Invoke(result));

                    return result;
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }
        public static T LogOperation<T>(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            Func<T> operation,
            Func<T, string?>? showResult = null,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    var result = operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed, showResult?.Invoke(result));

                    return result;
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }
        public static T LogOperation<T>(
            this ILogger logger,
            string caption,
            Func<T> operation,
            Func<T, (string, object[])?>? showResult = null,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    var result = operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed, showResult?.Invoke(result));

                    return result;
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }
        public static T LogOperation<T>(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            Func<T> operation,
            Func<T, (string, object[])?>? showResult = null,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    var result = operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed, showResult?.Invoke(result));

                    return result;
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }

        #endregion [ LogOperation<T> ]

        #region [ LogOperation ]

        public static void LogOperation(
            this ILogger logger,
            string caption,
            Action operation,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed);
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }
        public static void LogOperation(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            Action operation,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed);
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }

        #endregion [ LogOperation ]

        #region [ LogOperationAsync<T> ]

        public static async Task<T> LogOperationAsync<T>(
            this ILogger logger,
            string caption,
            Func<Task<T>> operation,
            Func<T, string?>? showResult = null,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    var result = await operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed, showResult?.Invoke(result));

                    return result;
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }
        public static async Task<T> LogOperationAsync<T>(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            Func<Task<T>> operation,
            Func<T, string?>? showResult = null,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    var result = await operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed, showResult?.Invoke(result));

                    return result;
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }
        public static async Task<T> LogOperationAsync<T>(
            this ILogger logger,
            string caption,
            Func<Task<T>> operation,
            Func<T, (string, object[])?>? showResult = null,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    var result = await operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed, showResult?.Invoke(result));

                    return result;
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }
        public static async Task<T> LogOperationAsync<T>(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            Func<Task<T>> operation,
            Func<T, (string, object[])?>? showResult = null,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    var result = await operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed, showResult?.Invoke(result));

                    return result;
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }

        #endregion [ LogOperationAsync<T> ]

        #region [ LogOperationAsync ]

        public static async Task LogOperationAsync(
            this ILogger logger,
            string caption,
            Func<Task> operation,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    await operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed);
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }
        public static async Task LogOperationAsync(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            Func<Task> operation,
            LogOperationOptions? options = null)
        {
            var opts = options ?? LogOperationOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                try
                {
                    await operation();

                    watch.Stop();

                    opts.LogEnd(logger, watch.Elapsed);
                }
                catch (Exception exception)
                {

                    watch.Stop();

                    opts.LogError(logger, watch.Elapsed, exception);

                    throw;
                }
            }
        }

        #endregion [ LogOperationAsync ]

        #region [ LogEnumerable<T> ]

        public static IEnumerable<T> LogEnumerable<T>(
            this ILogger logger,
            string caption,
            IEnumerable<T> source,
            Func<T, string?>? showResult = null,
            LogEnumerableOptions? options = null)
        {
            var opts = options ?? LogEnumerableOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                foreach (var item in source)
                {
                    opts.LogValue(logger, watch.Elapsed, showResult?.Invoke(item));

                    yield return item;
                }

                watch.Stop();

                opts.LogEnd(logger, watch.Elapsed);
            }
        }
        public static IEnumerable<T> LogEnumerable<T>(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            IEnumerable<T> source,
            Func<T, string?>? showResult = null,
            LogEnumerableOptions? options = null)
        {
            var opts = options ?? LogEnumerableOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                foreach (var item in source)
                {
                    opts.LogValue(logger, watch.Elapsed, showResult?.Invoke(item));

                    yield return item;
                }

                watch.Stop();

                opts.LogEnd(logger, watch.Elapsed);
            }
        }
        public static IEnumerable<T> LogEnumerable<T>(
            this ILogger logger,
            string caption,
            IEnumerable<T> source,
            Func<T, (string, object[])?>? showResult = null,
            LogEnumerableOptions? options = null)
        {
            var opts = options ?? LogEnumerableOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                foreach (var item in source)
                {
                    opts.LogValue(logger, watch.Elapsed, showResult?.Invoke(item));

                    yield return item;
                }

                watch.Stop();

                opts.LogEnd(logger, watch.Elapsed);
            }
        }
        public static IEnumerable<T> LogEnumerable<T>(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            IEnumerable<T> source,
            Func<T, (string, object[])?>? showResult = null,
            LogEnumerableOptions? options = null)
        {
            var opts = options ?? LogEnumerableOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                foreach (var item in source)
                {
                    opts.LogValue(logger, watch.Elapsed, showResult?.Invoke(item));

                    yield return item;
                }

                watch.Stop();

                opts.LogEnd(logger, watch.Elapsed);
            }
        }

        #endregion [ LogEnumerable<T> ]

        #region [ LogAsyncEnumerable<T> ]

        public static async IAsyncEnumerable<T> LogAsyncEnumerable<T>(
            this ILogger logger,
            string caption,
            IAsyncEnumerable<T> source,
            Func<T, string?>? showResult = null,
            LogEnumerableOptions? options = null)
        {
            var opts = options ?? LogEnumerableOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                await foreach (var item in source)
                {
                    opts.LogValue(logger, watch.Elapsed, showResult?.Invoke(item));

                    yield return item;
                }

                watch.Stop();

                opts.LogEnd(logger, watch.Elapsed);
            }
        }
        public static async IAsyncEnumerable<T> LogAsyncEnumerable<T>(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            IAsyncEnumerable<T> source,
            Func<T, string?>? showResult = null,
            LogEnumerableOptions? options = null)
        {
            var opts = options ?? LogEnumerableOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                await foreach (var item in source)
                {
                    opts.LogValue(logger, watch.Elapsed, showResult?.Invoke(item));

                    yield return item;
                }

                watch.Stop();

                opts.LogEnd(logger, watch.Elapsed);
            }
        }
        public static async IAsyncEnumerable<T> LogAsyncEnumerable<T>(
            this ILogger logger,
            string caption,
            IAsyncEnumerable<T> source,
            Func<T, (string, object[])?>? showResult = null,
            LogEnumerableOptions? options = null)
        {
            var opts = options ?? LogEnumerableOptions.Default;

            using (logger.BeginScope(caption))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                await foreach (var item in source)
                {
                    opts.LogValue(logger, watch.Elapsed, showResult?.Invoke(item));

                    yield return item;
                }

                watch.Stop();

                opts.LogEnd(logger, watch.Elapsed);
            }
        }
        public static async IAsyncEnumerable<T> LogAsyncEnumerable<T>(
            this ILogger logger,
            string caption,
            object[] captionArgs,
            IAsyncEnumerable<T> source,
            Func<T, (string, object[])?>? showResult = null,
            LogEnumerableOptions? options = null)
        {
            var opts = options ?? LogEnumerableOptions.Default;

            using (logger.BeginScope(caption, captionArgs))
            {
                opts.LogStart(logger);

                var watch = Stopwatch.StartNew();

                await foreach (var item in source)
                {
                    opts.LogValue(logger, watch.Elapsed, showResult?.Invoke(item));

                    yield return item;
                }

                watch.Stop();

                opts.LogEnd(logger, watch.Elapsed);
            }
        }

        #endregion [ LogAsyncEnumerable<T> ]

    }

    public class LogOperationOptions
    {
        public static readonly LogOperationOptions Default = new LogOperationOptions();

        public LogLevel StartLevel { get; init; } = LogLevel.Information;
        public string StartStage { get; init; } = "START";
        public LogLevel? EndLevel { get; init; }
        public string EndStage { get; init; } = "END";
        public LogLevel ErrorLevel { get; init; } = LogLevel.Error;
        public string ErrorStage { get; init; } = "ERROR";

        public void LogStart(ILogger logger) => 
            logger.Log(StartLevel, "[{Stage}]", StartStage);

        public void LogEnd(ILogger logger, TimeSpan elapsed, string? result = null)
        {
            if (result.IsNotNullOrWhiteSpace())
            {
                logger.Log(
                    EndLevel ?? StartLevel,
                    "[{Stage}] = {Result} ({Elapsed}ms)",
                    EndStage,
                    result,
                    elapsed.TotalMilliseconds);
            } 
            else
            {
                logger.Log(
                    EndLevel ?? StartLevel,
                    "[{Stage}] ({Elapsed}ms)",
                    EndStage,
                    elapsed.TotalMilliseconds);
            }
        }

        public void LogEnd(ILogger logger, TimeSpan elapsed, string resultMessage, params object[] resultArgs)
        {
            var args = new[] { (object)EndStage }
                .Concat(resultArgs)
                .Concat(new[] { (object)elapsed.TotalMilliseconds });

            logger.Log(
                EndLevel ?? StartLevel,
                "[{Stage}] = " + resultMessage + " ({Elapsed}ms)",
                args);
        }

        public void LogEnd(ILogger logger, TimeSpan elapsed, (string, object[])? results)
        {
            if (results is null)
            {
                LogEnd(logger, elapsed);
            }
            else
            {
                var (message, args) = results.Value;
                LogEnd(logger, elapsed, message, args);
            }
        }

        public void LogError(ILogger logger, TimeSpan elapsed, Exception exception)
        {
            logger.Log(
                ErrorLevel,
                exception,
                "[{Stage}] ({Elapsed}ms)",
                ErrorStage,
                elapsed.TotalMilliseconds);
        }
    }

    public class LogEnumerableOptions
    {
        public static readonly LogEnumerableOptions Default = new LogEnumerableOptions();

        public LogLevel StartLevel { get; init; } = LogLevel.Information;
        public string StartStage { get; init; } = "START";
        public LogLevel? EndLevel { get; init; }
        public string EndStage { get; init; } = "END";
        public LogLevel? ValueLevel { get; init; }
        public string ValueStage { get; init; } = "VALUE";

        public void LogStart(ILogger logger) => 
            logger.Log(StartLevel, "[{Stage}]", StartStage);

        public void LogEnd(ILogger logger, TimeSpan elapsed) => 
            logger.Log(
                EndLevel ?? StartLevel,
                "[{Stage}] ({Elapsed}ms)", 
                EndStage,
                elapsed.TotalMilliseconds);

        public void LogValue(ILogger logger, TimeSpan elapsed, string? result = null)
        {
            if (result.IsNotNullOrWhiteSpace())
            {
                logger.Log(
                    ValueLevel ?? StartLevel,
                    "[{Stage}] = {Result} ({Elapsed}ms)",
                    ValueStage,
                    result,
                    elapsed.TotalMilliseconds);
            } 
            else
            {
                logger.Log(
                    ValueLevel ?? StartLevel,
                    "[{Stage}] ({Elapsed}ms)",
                    ValueStage,
                    elapsed.TotalMilliseconds);
            }
        }

        public void LogValue(ILogger logger, TimeSpan elapsed, string resultMessage, params object[] resultArgs)
        {
            var args = new[] { (object)ValueStage }
                .Concat(resultArgs)
                .Concat(new[] { (object)elapsed.TotalMilliseconds });

            logger.Log(
                ValueLevel ?? StartLevel,
                "[{Stage}] = " + resultMessage + " ({Elapsed}ms)",
                args);
        }

        public void LogValue(ILogger logger, TimeSpan elapsed, (string, object[])? results)
        {
            if (results is null)
            {
                LogValue(logger, elapsed);
            }
            else
            {
                var (message, args) = results.Value;
                LogValue(logger, elapsed, message, args);
            }
        }
    }
}
