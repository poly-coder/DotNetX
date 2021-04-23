using System;
using System.Threading.Tasks;

namespace DotNetX
{
    public static class TaskExtensions
    {
        public static ValueTask<T> AsValueTaskResult<T>(this T value) => new (value);
        
        public static Task<T> AsTaskResult<T>(this T value) => Task.FromResult(value);
        
        public static Task<T> AsTaskResult<T>(this Exception exception) =>
            Task.FromException<T>(exception);

        
        public static async Task<B> SelectMany<A, B>(this Task<A> sourceTask, Func<A, Task<B>> select)
        {
            var source = await sourceTask;

            return await select(source);
        }
        
        public static async Task<B> Select<A, B>(this Task<A> sourceTask, Func<A, B> select)
        {
            var source = await sourceTask;

            return select(source);
        }
    }
}
