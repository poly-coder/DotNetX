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

            opts.LogStart(logger, caption);

            var watch = Stopwatch.StartNew();

            try
            {
                var result = operation();

                watch.Stop();

                opts.LogEnd(logger, caption, watch.Elapsed, showResult?.Invoke(result));

                return result;
            }
            catch (Exception exception)
            {

                watch.Stop();

                opts.LogError(logger, caption, watch.Elapsed, exception);

                throw;
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

            opts.LogStart(logger, caption);

            var watch = Stopwatch.StartNew();

            try
            {
                operation();

                watch.Stop();

                opts.LogEnd(logger, caption, watch.Elapsed);
            }
            catch (Exception exception)
            {

                watch.Stop();

                opts.LogError(logger, caption, watch.Elapsed, exception);

                throw;
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

            opts.LogStart(logger, caption);

            var watch = Stopwatch.StartNew();

            try
            {
                var result = await operation();

                watch.Stop();

                opts.LogEnd(logger, caption, watch.Elapsed, showResult?.Invoke(result));

                return result;
            }
            catch (Exception exception)
            {

                watch.Stop();

                opts.LogError(logger, caption, watch.Elapsed, exception);

                throw;
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

            opts.LogStart(logger, caption);

            var watch = Stopwatch.StartNew();

            try
            {
                await operation();

                watch.Stop();

                opts.LogEnd(logger, caption, watch.Elapsed);
            }
            catch (Exception exception)
            {

                watch.Stop();

                opts.LogError(logger, caption, watch.Elapsed, exception);

                throw;
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

            opts.LogStart(logger, caption);

            var watch = Stopwatch.StartNew();

            foreach (var item in source)
            {
                opts.LogValue(logger, caption, watch.Elapsed, showResult?.Invoke(item));

                yield return item;
            }

            watch.Stop();

            opts.LogEnd(logger, caption, watch.Elapsed);
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

            opts.LogStart(logger, caption);

            var watch = Stopwatch.StartNew();

            await foreach (var item in source)
            {
                opts.LogValue(logger, caption, watch.Elapsed, showResult?.Invoke(item));

                yield return item;
            }

            watch.Stop();

            opts.LogEnd(logger, caption, watch.Elapsed);
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

        public void LogStart(ILogger logger, string caption) => 
            logger.Log(StartLevel, "[{0}] {1}".Format(StartStage, caption));

        public void LogEnd(ILogger logger, string caption, TimeSpan elapsed, string? result = null)
        {
            if (result.IsNotNullOrWhiteSpace())
            {
                logger.Log(
                    EndLevel ?? StartLevel,
                    "[{0}] {1} = {2} ({3:0.###}ms)".Format(EndStage, caption, result!, elapsed.TotalMilliseconds));
            } 
            else
            {
                logger.Log(
                    EndLevel ?? StartLevel,
                    "[{0}] {1} ({2:0.###}ms)".Format(EndStage, caption, elapsed.TotalMilliseconds));
            }
        }

        public void LogError(ILogger logger, string caption, TimeSpan elapsed, Exception exception)
        {
            logger.Log(ErrorLevel, exception,
                "[{0}] {1} ({2:0.###}ms)".Format(ErrorStage, caption, elapsed.TotalMilliseconds));
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

        public void LogStart(ILogger logger, string caption) => 
            logger.Log(StartLevel, "[{0}] {1}".Format(StartStage, caption));

        public void LogEnd(ILogger logger, string caption, TimeSpan elapsed) => 
            logger.Log(EndLevel ?? StartLevel,
                "[{0}] {1} ({2:0.###}ms)".Format(EndStage, caption, elapsed.TotalMilliseconds));

        public void LogValue(ILogger logger, string caption, TimeSpan elapsed, string? result = null)
        {
            if (result.IsNotNullOrWhiteSpace())
            {
                logger.Log(
                    ValueLevel ?? StartLevel,
                    "[{0}] {1} = {2} ({3:0.###}ms)".Format(ValueStage, caption, result!, elapsed.TotalMilliseconds));
            } 
            else
            {
                logger.Log(
                    ValueLevel ?? StartLevel,
                    "[{0}] {1} ({2:0.###}ms)".Format(ValueStage, caption, elapsed.TotalMilliseconds));
            }
        }
    }
}
