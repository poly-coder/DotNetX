using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotNetX.Threading
{
    public record MailboxActorInterface<T>(
        ChannelWriter<T> Mailbox,
        Task ActorTask);

    public record MailboxActorInterface<T, TResult>(
        ChannelWriter<T> Mailbox,
        Task<TResult> ActorTask);

    public static class MailboxActor
    {
        #region [ StartOnChannel ]

        public static MailboxActorInterface<T> StartOnChannel<T>(
            this Channel<T> channel,
            Func<ChannelReader<T>, ChannelWriter<T>, Task> actorLoop)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            var actor = actorLoop(channel.Reader, channel.Writer);

            return new MailboxActorInterface<T>(channel.Writer, actor);
        }

        public static MailboxActorInterface<T, TResult> StartOnChannel<T, TResult>(
            this Channel<T> channel,
            Func<ChannelReader<T>, ChannelWriter<T>, Task<TResult>> actorLoop)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            var actor = actorLoop(channel.Reader, channel.Writer);

            return new MailboxActorInterface<T, TResult>(channel.Writer, actor);
        }

        public static MailboxActorInterface<T> StartOnChannel<T>(
            this Channel<T> channel,
            Func<ChannelReader<T>, Task> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return channel.StartOnChannel((reader, _) => actorLoop(reader));
        }

        public static MailboxActorInterface<T, TResult> StartOnChannel<T, TResult>(
            this Channel<T> channel,
            Func<ChannelReader<T>, Task<TResult>> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return channel.StartOnChannel((reader, _) => actorLoop(reader));
        }

        #endregion

        #region [ StartBounded ]

        public static MailboxActorInterface<T> StartBounded<T>(
            int capacity,
            Func<ChannelReader<T>, ChannelWriter<T>, Task> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return Channel
                .CreateBounded<T>(capacity)
                .StartOnChannel(actorLoop);
        }

        public static MailboxActorInterface<T> StartBounded<T>(
            BoundedChannelOptions options,
            Func<ChannelReader<T>, ChannelWriter<T>, Task> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return Channel
                .CreateBounded<T>(options)
                .StartOnChannel(actorLoop);
        }

        public static MailboxActorInterface<T, TResult> StartBounded<T, TResult>(
            int capacity,
            Func<ChannelReader<T>, ChannelWriter<T>, Task<TResult>> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return Channel
                .CreateBounded<T>(capacity)
                .StartOnChannel(actorLoop);
        }

        public static MailboxActorInterface<T, TResult> StartBounded<T, TResult>(
            BoundedChannelOptions options,
            Func<ChannelReader<T>, ChannelWriter<T>, Task<TResult>> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return Channel
                .CreateBounded<T>(options)
                .StartOnChannel(actorLoop);
        }

        public static MailboxActorInterface<T> StartBounded<T>(
            int capacity,
            Func<ChannelReader<T>, Task> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return StartBounded<T>(capacity, (receiver, _) => actorLoop(receiver));
        }

        public static MailboxActorInterface<T> StartBounded<T>(
            BoundedChannelOptions options,
            Func<ChannelReader<T>, Task> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return StartBounded<T>(options, (receiver, _) => actorLoop(receiver));
        }

        public static MailboxActorInterface<T, TResult> StartBounded<T, TResult>(
            int capacity,
            Func<ChannelReader<T>, Task<TResult>> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return StartBounded<T, TResult>(capacity, (receiver, _) => actorLoop(receiver));
        }

        public static MailboxActorInterface<T, TResult> StartBounded<T, TResult>(
            BoundedChannelOptions options,
            Func<ChannelReader<T>, Task<TResult>> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return StartBounded<T, TResult>(options, (receiver, _) => actorLoop(receiver));
        }

        #endregion

        #region [ StartUnbounded ]

        public static MailboxActorInterface<T> StartUnbounded<T>(
            Func<ChannelReader<T>, ChannelWriter<T>, Task> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return Channel
                .CreateUnbounded<T>()
                .StartOnChannel(actorLoop);
        }

        public static MailboxActorInterface<T> StartUnbounded<T>(
            UnboundedChannelOptions options,
            Func<ChannelReader<T>, ChannelWriter<T>, Task> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return Channel
                .CreateUnbounded<T>(options)
                .StartOnChannel(actorLoop);
        }

        public static MailboxActorInterface<T, TResult> StartUnbounded<T, TResult>(
            Func<ChannelReader<T>, ChannelWriter<T>, Task<TResult>> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return Channel
                .CreateUnbounded<T>()
                .StartOnChannel(actorLoop);
        }

        public static MailboxActorInterface<T, TResult> StartUnbounded<T, TResult>(
            UnboundedChannelOptions options,
            Func<ChannelReader<T>, ChannelWriter<T>, Task<TResult>> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return Channel
                .CreateUnbounded<T>(options)
                .StartOnChannel(actorLoop);
        }

        public static MailboxActorInterface<T> StartUnbounded<T>(
            Func<ChannelReader<T>, Task> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return StartUnbounded<T>((receiver, _) => actorLoop(receiver));
        }

        public static MailboxActorInterface<T> StartUnbounded<T>(
            UnboundedChannelOptions options,
            Func<ChannelReader<T>, Task> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return StartUnbounded<T>(options, (receiver, _) => actorLoop(receiver));
        }

        public static MailboxActorInterface<T, TResult> StartUnbounded<T, TResult>(
            Func<ChannelReader<T>, Task<TResult>> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return StartUnbounded<T, TResult>((receiver, _) => actorLoop(receiver));
        }

        public static MailboxActorInterface<T, TResult> StartUnbounded<T, TResult>(
            UnboundedChannelOptions options,
            Func<ChannelReader<T>, Task<TResult>> actorLoop)
        {
            if (actorLoop == null) throw new ArgumentNullException(nameof(actorLoop));

            return StartUnbounded<T, TResult>(options, (receiver, _) => actorLoop(receiver));
        }

        #endregion

        #region [ PostAsync | TryPost | PostAndReplyAsync ]

        public static ValueTask PostAsync<T>(
            this ChannelWriter<T> writer,
            T message,
            CancellationToken cancellationToken = default) =>
            writer.WriteAsync(message, cancellationToken);

        public static bool TryPost<T>(
            this ChannelWriter<T> writer,
            T message) =>
            writer.TryWrite(message);

        public static async Task<TResult> PostAndReplyAsync<T, TResult>(
            this ChannelWriter<T> writer,
            Func<TaskCompletionSource<TResult>, T> getMessage,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<TResult>();

            var message = getMessage(tcs);

            await writer.WriteAsync(message, cancellationToken);

            return await tcs.Task;
        }

        public static async Task PostAndReplyAsync<T>(
            this ChannelWriter<T> writer,
            Func<TaskCompletionSource, T> getMessage,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource();

            var message = getMessage(tcs);

            await writer.WriteAsync(message, cancellationToken);

            await tcs.Task;
        }

        #endregion
    }
}
