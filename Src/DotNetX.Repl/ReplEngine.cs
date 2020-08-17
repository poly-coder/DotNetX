using DotNetX.Repl.Runtime;
using System;
using System.Threading.Tasks;

namespace DotNetX.Repl
{
    // TODO: Abstract the Host console
    public class ReplEngine
    {
        private readonly IReplRuntime runtime;
        private readonly ReplEngineOptions options;

        public ReplEngine(IReplRuntime runtime, ReplEngineOptions options = null)
        {
            this.runtime = runtime;
            this.options = options ?? new ReplEngineOptions();
        }

        public async Task StartAsync(IServiceProvider serviceProvider)
        {
            var state = runtime.CreateEmptyState();

            while (true)
            {
                var line = ReadLine();


            }

        }


        private async Task<string> ReadLine()
        {
            ConsoleEx.Write(ConsoleColor.Cyan, await runtime.GetPrompt());

            ConsoleEx.Write(ConsoleColor.White, " > ");

            var line = Console.ReadLine();

            return line;
        }
    }

    public class ReplEngineOptions
    {

    }
}
