using DotNetX.Repl.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetX.Repl.Builder
{

    public class ReplBuilder : DescribableBuilder<ReplBuilder>
    {
        internal string version = "0.0";
        internal List<ReplCommandBuilder> commands = new List<ReplCommandBuilder>();

        public ReplBuilder WithVersion(string version)
        {
            this.version = version;
            return this;
        }

        public ReplBuilder WithCommand(string name, Action<ReplCommandBuilder> configCommand)
        {
            var commandBuilder = new ReplCommandBuilder().WithName(name);
            configCommand.Invoke(commandBuilder);
            commands.Add(commandBuilder);
            return this;
        }

        public IReplRuntime Build()
        {
            return new ReplRuntime(this);
        }

        private static string AsOptionName(string name)
        {
            var prefix = name.Length <= 0 ? "-" : "--";
            return prefix + name;
        }

        class ReplRuntime : IReplRuntime
        {
            private static readonly Regex OptionsRegex = new Regex(@"(?<long>--\w+)|(?<short>-\w+)|(?<loose>[^'""\s\-][^'""\s]*)|""(?<quoted>(""""|[^""])+)""|'(?<squoted>(''|[^'])+)'");

            public ReplRuntime(ReplBuilder builder)
            {
                this.Version = builder.version;
                this.Caption = builder.caption;
                this.Description = builder.description;
                this.Examples = new ReadOnlyCollection<ReplExampleRuntime>(builder.examples.Select(e => new ReplExampleRuntime(e)).ToArray());
                // TODO: Check command ambiguity
                this.Commands = new ReadOnlyCollection<ReplCommandRuntime>(builder.commands.Select(c => new ReplCommandRuntime(c)).ToArray());
            }

            public string Version { get; }
            public string Caption { get; }
            public string Description { get; }
            public ReadOnlyCollection<ReplExampleRuntime> Examples { get; }
            public ReadOnlyCollection<ReplCommandRuntime> Commands { get; }

            public object CreateEmptyState()
            {
                // TODO: 
                return null;
            }

            public async Task<string> GetPrompt(object state)
            {
                // TODO: 
                return "Hello World!";
            }

            public void PrintHelp(object state)
            {
                PrintInformation(state);
                Console.WriteLine();

                if (Commands.Any())
                {
                    var groups = Commands.GroupBy(c => c.Category).ToArray();

                    foreach (var group in groups)
                    {
                        ConsoleEx.WriteLine(ConsoleColor.Cyan, "{0}Commands:", group.Key.IsNullOrWhiteSpace() ? "" : group.Key + " ");

                        foreach (var command in group)
                        {
                            command.PrintSummary("    ");
                        }

                        Console.WriteLine();
                    }
                }

                if (Examples.Any())
                {
                    ConsoleEx.WriteLine(ConsoleColor.Cyan, "Examples:");
                    foreach (var example in Examples)
                    {
                        example.PrintHelp("    ");
                        Console.WriteLine();
                    }
                }
            }

            public void PrintCommandHelp(object state, string command)
            {
                var commandRuntime = FindCommand(state, command);

                if (commandRuntime != null)
                {
                    commandRuntime.PrintHelp(state);
                }
                else
                {
                    PrintCommandNotAvailable(command);
                }
            }

            public void PrintOptionHelp(object state, string command, string option)
            {
                var commandRuntime = FindCommand(state, command);

                if (commandRuntime != null)
                {
                    var paramRuntime = FindCommandOption(state, commandRuntime, option);

                    if (paramRuntime != null)
                    {
                        paramRuntime.PrintHelp(state);
                    }
                    else
                    {
                        PrintCommandOptionNotAvailable(command, option);
                    }
                }
                else
                {
                    PrintCommandNotAvailable(command);
                }
            }

            public void PrintInformation(object state)
            {
                if (Caption.IsNotNullOrWhiteSpace())
                {
                    ConsoleEx.Write(ConsoleColor.Green, "{0} ", Caption);
                }

                ConsoleEx.WriteLine(ConsoleColor.White, Version);

                if (Description.IsNotNullOrWhiteSpace())
                {
                    ConsoleEx.WriteLine(ConsoleColor.Gray, Description);
                }
            }

            public void PrintVersion(object state)
            {
                Console.WriteLine(Version);
            }

            private static void PrintCommandNotAvailable(string command)
            {
                ConsoleEx.Write(ConsoleColor.Gray, "Command ");
                ConsoleEx.Write(ConsoleColor.Red, command);
                ConsoleEx.Write(ConsoleColor.Gray, " is not available. Use ");
                ConsoleEx.Write(ConsoleColor.White, "help");
                ConsoleEx.Write(ConsoleColor.Gray, " to check the list of available commands.");
            }

            private static void PrintCommandOptionNotAvailable(string command, string option)
            {
                ConsoleEx.Write(ConsoleColor.Gray, "Command ");
                ConsoleEx.Write(ConsoleColor.White, command);
                ConsoleEx.Write(ConsoleColor.Gray, " do not have option ");
                ConsoleEx.Write(ConsoleColor.Red, option);
                ConsoleEx.Write(ConsoleColor.Gray, " available. Use ");
                ConsoleEx.Write(ConsoleColor.White, $"help {command}");
                ConsoleEx.Write(ConsoleColor.Gray, " to check the list of available options.");
            }

            private ReplCommandRuntime FindCommand(object state, string command)
            {
                return Commands.FirstOrDefault(c => c.Names.Contains(command, StringComparer.InvariantCultureIgnoreCase));
            }

            private ReplParameterRuntime FindCommandOption(object state, ReplCommandRuntime command, string option)
            {
                return command.Parameters.FirstOrDefault(c => c.Names.Contains(option, StringComparer.InvariantCultureIgnoreCase));
            }
        }

        class ReplCommandRuntime
        {
            public ReplCommandRuntime(ReplCommandBuilder builder)
            {
                this.Names = new ReadOnlyCollection<string>(builder.Names.ToArray());
                this.Category = builder.Category;
                this.Caption = builder.Caption;
                this.Description = builder.description;
                // TODO: Check parameter ambiguities, only last positional can be repeated, required must be in order: true, true, false, false
                this.Parameters = new ReadOnlyCollection<ReplParameterRuntime>(builder.Parameters.Select(e => new ReplParameterRuntime(e)).ToArray());
                this.Examples = new ReadOnlyCollection<ReplExampleRuntime>(builder.examples.Select(e => new ReplExampleRuntime(e)).ToArray());
            }

            public ReadOnlyCollection<string> Names { get; }
            public string Category { get; }
            public string Caption { get; }
            public string Description { get; }
            public ReadOnlyCollection<ReplParameterRuntime> Parameters { get; }
            public ReadOnlyCollection<ReplExampleRuntime> Examples { get; }

            public string AllNames => string.Join("|", Names);

            public void PrintSummary(string indent)
            {
                ConsoleEx.Write(ConsoleColor.White, "{0}{1,-20}\t", indent, AllNames);
                
                ConsoleEx.WriteLine(ConsoleColor.Gray, Caption ?? "");
            }

            public void PrintHelp(object state)
            {
                // TODO: HelpFlags with ShowExamples, FullHelp, etc.
                ConsoleEx.Write(ConsoleColor.White, "{0,-20}\t", AllNames);
                ConsoleEx.WriteLine(ConsoleColor.Gray, Caption ?? "");
                Console.WriteLine();

                ConsoleEx.Write(ConsoleColor.Cyan, "Usage: ");
                ConsoleEx.Write(ConsoleColor.White, Names[0]);

                foreach (var param in Parameters)
                {
                    Console.Write(" ");

                    Console.Write(param.IsRequired ? "(" : "[");

                    switch (param.ParameterType)
                    {
                        case ReplParameterType.Flag:
                        case ReplParameterType.Option:
                            ConsoleEx.Write(ConsoleColor.White, AsOptionName(param.Names[0]));
                            break;
                    }

                    switch (param.ParameterType)
                    {
                        case ReplParameterType.Option:
                            Console.Write(" {0}", param.TypeName ?? "value");
                            break;
                        case ReplParameterType.Positional:
                            Console.Write("{0}", param.TypeName ?? "value");
                            break;
                    }

                    Console.Write(param.IsRequired ? ")" : "]");
                    Console.Write(param.IsRepeated ? "*" : "");
                }

                Console.WriteLine();
                Console.WriteLine();

                if (Description.IsNotNullOrWhiteSpace())
                {
                    ConsoleEx.WriteLine(ConsoleColor.DarkGray, Description);
                    Console.WriteLine();
                }

                if (Parameters.Any())
                {
                    ConsoleEx.WriteLine(ConsoleColor.Cyan, "Options:");

                    foreach (var param in Parameters)
                    {
                        param.PrintSummary(state, "    ");
                    }
                    
                    Console.WriteLine();
                }

                if (Examples.Any())
                {
                    ConsoleEx.WriteLine(ConsoleColor.Cyan, "Examples:");
                    foreach (var example in Examples)
                    {
                        example.PrintHelp("    ");
                        Console.WriteLine();
                    }
                }
            }
        }

        enum ReplParameterType
        {
            Flag,
            Option,
            Positional,
        }

        class ReplParameterRuntime
        {
            public ReplParameterRuntime(IReplCommandParameterBuilder builder)
            {
                this.Names = new ReadOnlyCollection<string>(builder.Names.ToArray());
                this.Caption = builder.Caption;
                this.Description = builder.Description;
                this.Examples = new ReadOnlyCollection<ReplExampleRuntime>(builder.Examples.Select(e => new ReplExampleRuntime(e)).ToArray());

                switch (builder)
                {
                    case ReplCommandFlagParameterBuilder flag:
                        this.ParameterType = ReplParameterType.Flag;
                        break;
                    case ReplCommandOptionParameterBuilder  option:
                        this.ParameterType = ReplParameterType.Option;
                        this.IsRequired = option.IsRequired;
                        this.TypeName = option.TypeName;
                        this.IsRepeated = option.IsRepeated;
                        this.ValueCount = option.ValueCount;
                        break;
                    case ReplCommandPositionalParameterBuilder param:
                        this.ParameterType = ReplParameterType.Positional;
                        this.IsRequired = param.IsRequired;
                        this.TypeName = param.TypeName;
                        this.IsRepeated = param.IsRepeated;
                        break;
                    default:
                        break;
                }
            }

            public bool IsRequired { get; }
            public ReadOnlyCollection<string> Names { get; }
            public string Caption { get; }
            public string Description { get; }
            public ReadOnlyCollection<ReplExampleRuntime> Examples { get; }
            public ReplParameterType ParameterType { get; }
            public object TypeName { get; }
            public bool IsRepeated { get; }
            public int ValueCount { get; }
            public bool CaptureRest { get; }

            public string AllNames =>
                ParameterType == ReplParameterType.Positional
                ? string.Join("|", Names)
                : string.Join("|", Names.Select(n => AsOptionName(n)));

            public void PrintSummary(object state, string indent)
            {
                ConsoleEx.Write(ConsoleColor.White, "{0}{1,-20}\t", indent, AllNames);
                ConsoleEx.WriteLine(ConsoleColor.Gray, Caption ?? "");
            }

            public void PrintHelp(object state)
            {
                PrintSummary(state, "");
                Console.WriteLine();

                if (Description.IsNotNullOrWhiteSpace())
                {
                    ConsoleEx.WriteLine(ConsoleColor.DarkGray, Description);
                    Console.WriteLine();
                }

                if (Examples.Any())
                {
                    ConsoleEx.WriteLine(ConsoleColor.Cyan, "Examples:");
                    foreach (var example in Examples)
                    {
                        example.PrintHelp("    ");
                        Console.WriteLine();
                    }
                }
            }
        }

        class ReplExampleRuntime
        {
            public ReplExampleRuntime(ReplExampleBuilder builder)
            {
                this.Command = builder.command;
                this.Caption = builder.caption;
                this.Description = builder.description;
            }

            public string Command { get; }
            public string Caption { get; }
            public string Description { get; }

            internal void PrintHelp(string indent)
            {
                if (Caption.IsNotNullOrWhiteSpace())
                {
                    ConsoleEx.WriteLine(ConsoleColor.Gray, "{0}{1}", indent, Caption);
                }

                ConsoleEx.WriteLine(ConsoleColor.White, "{0}{1}", indent, Command);

                if (Description.IsNotNullOrWhiteSpace())
                {
                    ConsoleEx.WriteLine(ConsoleColor.DarkGray, "{0}{1}", indent, Description);
                }
            }
        }
    }
}
