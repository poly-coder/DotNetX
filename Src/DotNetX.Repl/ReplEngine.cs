using DotNetX;
using DotNetX.Repl.Runtime;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetX.Repl
{
    // TODO: Abstract the Host console
    public class ReplEngine
    {
        private static readonly Regex ExitCommandRegex = new Regex(@"^\s*(exit|x)\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex VersionCommandRegex = new Regex(@"^\s*(ver(sion)?)\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex InformationCommandRegex = new Regex(@"^\s*(info(rmation)?)\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex HelpCommandRegex = new Regex(@"^\s*(h(elp)?|((h(elp)?\s+|\?\s*)((?<command>\w+)(\s+(?<option>\w+))?)?))\s*$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        private readonly IReplRuntime runtime;
        private readonly ReplEngineOptions options;
        private object state;

        public ReplEngine(IReplRuntime runtime, ReplEngineOptions options = null)
        {
            this.runtime = runtime;
            this.options = options ?? new ReplEngineOptions();
        }

        public async Task StartAsync(IServiceProvider serviceProvider)
        {
            while (true)
            {
                var line = await ReadLine();

                if (line.IsNullOrWhiteSpace())
                {
                    continue;
                }

                if (IsVersionCommand(line))
                {
                    runtime.PrintVersion();
                    continue;
                }

                if (IsInformationCommand(line))
                {
                    runtime.PrintInformation();
                    continue;
                }

                if (IsExitCommand(line))
                {
                    break;
                }

                // TODO: Implement state storage

                if (IsHelpCommand(line, out var command, out var option))
                {
                    if (option != null)
                    {
                        runtime.PrintOptionHelp(command, option);
                    }
                    else if (command != null)
                    {
                        runtime.PrintCommandHelp(command);
                    }
                    else 
                    {
                        runtime.PrintHelp();
                    }

                    continue;
                }

                // TODO: Capture exceptions while executing a command

                await runtime.ExecuteAsync(line);
            }

        }

        private async Task<string> ReadLine()
        {
            if (Console.CursorLeft > 0)
            {
                Console.WriteLine();
            }

            ConsoleEx.Write(ConsoleColor.Cyan, await runtime.Prompt);

            ConsoleEx.Write(ConsoleColor.White, " > ");

            var line = Console.ReadLine();

            return line;
        }

        private bool IsExitCommand(string line)
        {
            return ExitCommandRegex.IsMatch(line);
        }

        private bool IsVersionCommand(string line)
        {
            return VersionCommandRegex.IsMatch(line);
        }

        private bool IsInformationCommand(string line)
        {
            return InformationCommandRegex.IsMatch(line);
        }

        private bool IsHelpCommand(string line, out string command, out string option)
        {
            var match = HelpCommandRegex.Match(line);
            command = null;
            option = null;

            if (match.Success)
            {
                var commandGroup = match.Groups["command"];

                if (commandGroup.Success)
                {
                    command = commandGroup.Value;
                    
                    var optionGroup = match.Groups["option"];
                    
                    if (optionGroup.Success)
                    {
                        option = optionGroup.Value;
                    }
                }

                return true;
            }

            return false;
        }
    }

    public class ReplEngineOptions
    {
        // IReplParser
        // IReplConsole
        // IReplValueBinder
        // ...
    }
}
