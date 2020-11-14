using DotNetX;
using DotNetX.Repl.Runtime;
using System;
using System.IO;
using System.Linq;
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
        
        private static readonly Regex HelpCommandRegex = new Regex(@"^\s*(h(elp)?|((h(elp)?\s+|\?\s*)((?<command>\w+(\-\w+)*)(\s+(?<option>\w+(\-\w+)*))?)?))\s*$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        
        private static readonly Regex ListStatesCommandRegex = new Regex(@"^\s*((list-)?states)\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex StoreStateCommandRegex = new Regex(@"^\s*(store-state)\s+(?<name>\w+(\-\w+)*)\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex LoadStateCommandRegex = new Regex(@"^\s*((load-)?state)\s+(?<name>\w+(\-\w+)*)\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex RemoveStateCommandRegex = new Regex(@"^\s*(rm-state)\s+(?<name>\w+(\-\w+)*)\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex ClearStatesCommandRegex = new Regex(@"^\s*(clear-states)\s*$", RegexOptions.IgnoreCase);

        private readonly IReplRuntime runtime;
        private readonly ReplEngineOptions options;

        public ReplEngine(IReplRuntime runtime, ReplEngineOptions? options = null)
        {
            this.runtime = runtime;
            this.options = options ?? new ReplEngineOptions();
        }

        // TODO: add cancellation token
        public async Task StartAsync()
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
                        runtime.PrintOptionHelp(command!, option);
                    }
                    else if (command != null)
                    {
                        runtime.PrintCommandHelp(command);
                    }
                    else 
                    {
                        runtime.PrintHelp();
                        
                        PrintCommonHelp();
                    }

                    continue;
                }

                if (runtime.CanPersistState)
                {
                    if (IsListStatesCommand(line))
                    {
                        ListStates();
                        continue;
                    }
                    else if (IsClearStatesCommand(line))
                    {
                        ClearStates();
                        continue;
                    }
                    else if (IsRemoveStateCommand(line, out var name))
                    {
                        RemoveState(name!);
                        continue;
                    }
                    else if (IsStoreStateCommand(line, out name))
                    {
                        await StoreState(name!);
                        continue;
                    }
                    else if (IsLoadStateCommand(line, out name))
                    {
                        await LoadState(name!);
                        continue;
                    }
                }

                // TODO: Capture exceptions while executing a command

                await runtime.ExecuteAsync(line, default);
            }

        }

        private void ListStates()
        {
            var directory = Environment.CurrentDirectory;

            var pairs = Directory.EnumerateFiles(directory)
                .Select(fileName => IsStateFileName(fileName, out var name) ? (fileName, name) : (fileName, name: null))
                .Where(pair => pair.name != null)
                .ToList();

            if (pairs.Any())
            {
                foreach (var pair in pairs)
                {
                    ConsoleEx.WriteLine(ConsoleColor.White, "  {0}", pair.name ?? "");
                }
            }
            else
            {
                ConsoleEx.WriteLine(ConsoleColor.Cyan, "  (empty)");
            }
        }

        private void ClearStates()
        {
            var directory = Environment.CurrentDirectory;

            var pairs = Directory.EnumerateFiles(directory)
                .Select(fileName => IsStateFileName(fileName, out var name) ? (fileName, name) : (fileName, name: null))
                .Where(pair => pair.name != null)
                .ToList();

            if (pairs.Any())
            {
                ConsoleEx.WriteLine(new ColoredText()
                    .Add("The following states will be removed: ")
                    .Add(ConsoleColor.Yellow, string.Join(", ", pairs.Select(p => p.name)))
                    );

                ConsoleEx.Write(new ColoredText()
                    .Add(ConsoleColor.White, "Are you sure you want to remove all of them? (")
                    .Add(ConsoleColor.Red, "[Y]es")
                    .Add(ConsoleColor.White, "/[N]o) > "));

                var line = Console.ReadLine();

                if (line != "y" && line != "Y")
                {
                    return;
                }

                foreach (var pair in pairs)
                {
                    try
                    {
                        File.Delete(Path.Combine(directory, pair.fileName));
                    }
                    catch
                    {
                        ConsoleEx.WriteLine(ConsoleColor.Red, "Could not delete state {0} on file {1}", pair.name!, pair.fileName);
                    }
                }

                ConsoleEx.WriteLine(ConsoleColor.Cyan, "  Done!");
            }
            else
            {
                ConsoleEx.WriteLine(ConsoleColor.Cyan, "  (empty)");
            }
        }

        private void RemoveState(string name)
        {
            var directory = Environment.CurrentDirectory;

            var fileName = Path.Combine(directory, $"{options.StatePrefix}{name}{options.StateSuffix}");

            if (File.Exists(fileName))
            {
                ConsoleEx.Write(new ColoredText()
                    .Add(ConsoleColor.White, "Are you sure you want to remove state {0}? (", name)
                    .Add(ConsoleColor.Red, "[Y]es")
                    .Add(ConsoleColor.White, "/[N]o) > "));

                var line = Console.ReadLine();

                if (line != "y" && line != "Y")
                {
                    return;
                }

                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                    ConsoleEx.WriteLine(ConsoleColor.Red, "Could not delete state {0} on file {1}", name, fileName);
                    return;
                }

                ConsoleEx.WriteLine(ConsoleColor.Cyan, "  Done!");
            }
            else
            {
                ConsoleEx.WriteLine(ConsoleColor.Cyan, "  (state {0} doesn't exist)", name);
            }
        }

        private async Task LoadState(string name)
        {
            var directory = Environment.CurrentDirectory;

            var fileName = Path.Combine(directory, $"{options.StatePrefix}{name}{options.StateSuffix}");

            if (File.Exists(fileName))
            {
                try
                {
                    var state = await File.ReadAllBytesAsync(fileName);
                    runtime.LoadState(state);
                    ConsoleEx.WriteLine(ConsoleColor.Cyan, "  Done!");
                }
                catch
                {
                    ConsoleEx.WriteLine(ConsoleColor.Red, "Could not load state {0} from file {1}", name, fileName);
                    return;
                }
            }
            else
            {
                ConsoleEx.WriteLine(ConsoleColor.Cyan, "  (state {0} doesn't exist)", name);
            }
        }

        private async Task StoreState(string name)
        {
            var directory = Environment.CurrentDirectory;

            var fileName = Path.Combine(directory, $"{options.StatePrefix}{name}{options.StateSuffix}");

            if (File.Exists(fileName))
            {
                ConsoleEx.Write(new ColoredText()
                    .Add(ConsoleColor.White, "Are you sure you want to override state {0}? (", name)
                    .Add(ConsoleColor.Red, "[Y]es")
                    .Add(ConsoleColor.White, "/[N]o) > "));

                var line = Console.ReadLine();

                if (line != "y" && line != "Y")
                {
                    return;
                }
            }

            try
            {
                var state = runtime.PersistState();
                await File.WriteAllBytesAsync(fileName, state);
                ConsoleEx.WriteLine(ConsoleColor.Cyan, "  Done!");
            }
            catch
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, "Could not store state {0} on file {1}", name, fileName);
                return;
            }
        }

        private bool IsStateFileName(string fileName, out string? stateName)
        {
            var name = Path.GetFileName(fileName);

            var remaining = name.Length - (options.StatePrefix.Length + options.StateSuffix.Length);

            if (remaining > 0 &&
                name.StartsWith(options.StatePrefix) &&
                name.EndsWith(options.StateSuffix))
            {
                stateName = name.Substring(options.StatePrefix.Length, remaining);
                return true;
            }

            stateName = null;
            return false;
        }

        private async Task<string> ReadLine()
        {
            if (Console.CursorLeft > 0)
            {
                Console.WriteLine();
            }
            
            // TODO: Use Ctrl+C/Break to cancel any operation
            ConsoleEx.Write(ConsoleColor.Cyan, await runtime.GetPrompt(default));

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

        private bool IsHelpCommand(string line, out string? command, out string? option)
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

        private bool IsListStatesCommand(string line)
        {
            return ListStatesCommandRegex.IsMatch(line);
        }

        private bool IsClearStatesCommand(string line)
        {
            return ClearStatesCommandRegex.IsMatch(line);
        }

        private bool IsStoreStateCommand(string line, out string? name)
        {
            var match = StoreStateCommandRegex.Match(line);
            name = null;

            if (match.Success)
            {
                var nameGroup = match.Groups["name"];

                if (nameGroup.Success)
                {
                    name = nameGroup.Value;
                }

                return true;
            }

            return false;
        }

        private bool IsLoadStateCommand(string line, out string? name)
        {
            var match = LoadStateCommandRegex.Match(line);
            name = null;

            if (match.Success)
            {
                var nameGroup = match.Groups["name"];

                if (nameGroup.Success)
                {
                    name = nameGroup.Value;
                }

                return true;
            }

            return false;
        }

        private bool IsRemoveStateCommand(string line, out string? name)
        {
            var match = RemoveStateCommandRegex.Match(line);
            name = null;

            if (match.Success)
            {
                var nameGroup = match.Groups["name"];

                if (nameGroup.Success)
                {
                    name = nameGroup.Value;
                }

                return true;
            }

            return false;
        }

        private void PrintCommonHelp()
        {
            ConsoleEx.WriteLine(ConsoleColor.Cyan, "Common Commands:");

            void WriteCommand(string command, string summary, int size = 20)
            {
                ConsoleEx.WriteLine(new ColoredText()
                    .Add(" - ")
                    .Add(ConsoleColor.White, "{0}\t", command.PadRight(size))
                    .Add("    {0}", summary));
            }

            WriteCommand("version|ver", "Prints program version");
            WriteCommand("information|info", "Prints program basic information");
            WriteCommand("help|h|?", "Prints program help");
            WriteCommand("help|h|? <command>", "Prints command help");
            WriteCommand("help|h|? <command> <option>", "Prints command's option help. The option name do not contains dashes '--'");
            Console.WriteLine();

            if (runtime.CanPersistState)
            {
                ConsoleEx.WriteLine(ConsoleColor.Cyan, "State Commands:");

                WriteCommand("list-states|states", "List saved states", 28);
                WriteCommand("store-state <state-name>", "Saves current REPL state into the given state name.", 28);
                WriteCommand("load-state|state <state-name>", "Loads a saved state with given name", 28);
                WriteCommand("rm-state <state-name>", "Removes the saved state with given name", 28);
                WriteCommand("clear-states", "Clear all saved states", 28);
                Console.WriteLine();
            }
        }
    }

    public class ReplEngineOptions
    {
        public string StatePrefix { get; set; } = "state-";
        public string StateSuffix { get; set; } = ".state";
        // IReplParser
        // IReplStateSerializer
        // IReplConsole
        // IReplModelBinder
        // ...
    }
}
