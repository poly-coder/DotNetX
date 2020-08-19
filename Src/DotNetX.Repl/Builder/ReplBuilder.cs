﻿using DotNetX.Repl.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
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
            private static readonly Regex CommandRegex = new Regex(@"^(?<command>\w+)(\s+|$)");
            private static readonly Regex OptionsRegex = new Regex(@"^((""(?<dqvalue>(""""|[^""])*)"")|('(?<sqvalue>(''|[^'])*)')|(?<value>[^""'\s\-][^""'\s]*)|\-\-(?<loption>\w+)|\-(?<soption>\w+))(\s+|$)");

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

            public Task<string> Prompt
            {
                get
                {
                    return Task.FromResult("Hello World!");

                }
                // TODO: 
            }

            public void PrintHelp()
            {
                PrintInformation();
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

            public void PrintCommandHelp(string command)
            {
                var commandRuntime = FindCommand(command);

                if (commandRuntime != null)
                {
                    commandRuntime.PrintHelp();
                }
                else
                {
                    PrintCommandNotAvailable(command);
                }
            }

            public void PrintOptionHelp(string command, string option)
            {
                var commandRuntime = FindCommand(command);

                if (commandRuntime != null)
                {
                    var paramRuntime = FindCommandOption(commandRuntime, option);

                    if (paramRuntime != null)
                    {
                        paramRuntime.PrintHelp();
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

            public void PrintInformation()
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

            public void PrintVersion()
            {
                Console.WriteLine(Version);
            }

            public async Task ExecuteAsync(string line)
            {
                var commandMatch = CommandRegex.Match(line);

                if (!commandMatch.Success)
                {
                    PrintCommandNotAvailable(line);
                    return;
                }

                line = line.Substring(commandMatch.Value.Length);

                var command = commandMatch.Groups["command"].Value;
                var commandRuntime = FindCommand(command);

                if (commandRuntime == null)
                {
                    PrintCommandNotAvailable(command);
                    return;
                }

                if (commandRuntime.Execute == null)
                {
                    ConsoleEx.WriteLine(ConsoleColor.Red, "Command \"{0}\" is not implemented", commandRuntime.Names.First());
                    return;
                }

                //var position = commandMatch.Length;
                var positionalIndex = 0;
                var parameterValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                var optionParam = default(ReplParameterRuntime);
                var optionValues = default(List<string>);

                while (line.Length > 0)
                {
                    var optionMatch = OptionsRegex.Match(line);

                    if (!optionMatch.Success)
                    {
                        ConsoleEx.WriteLine(ConsoleColor.Red, "Could not parse the command: \"{0}\"", line);
                        return;
                    }

                    line = line.Substring(optionMatch.Value.Length);

                    var value =
                        optionMatch.Groups["dqvalue"].Success
                        ? optionMatch.Groups["dqvalue"].Value.Replace("\"\"", "\"")
                        : optionMatch.Groups["sqvalue"].Success
                        ? optionMatch.Groups["sqvalue"].Value.Replace("\'\'", "\'")
                        : optionMatch.Groups["value"].Success
                        ? optionMatch.Groups["value"].Value
                        : null;

                    var longOption =
                        optionMatch.Groups["loption"].Success
                        ? optionMatch.Groups["loption"].Value
                        : null;

                    var shortOptions =
                        optionMatch.Groups["soption"].Success
                        ? optionMatch.Groups["soption"].Value
                        : null;

                    // TODO: Start running the state machine

                    /**
                     * Default -(Value)-> Default
                     *      Look for a positional param to fill
                     * Default -(Flag)-> Default : 
                     *      Look for a flag to enable
                     * Default -(Option)-> Option : 
                     *      Look for the option and initialize a list to start adding values
                     * Option -(Value)-> Option : 
                     *      Add the value to the options list. If the option have more pending values keep current Option state
                     * Option -(Value)-> Default : 
                     *      Add the value to the options list. If the option have no more pending values go to Default state
                     * Option -(*)-> Error
                     */

                    if (optionParam == null)
                    {
                        // Default State
                        if (value != null)
                        {
                            // It is a positional parameter value

                            if (positionalIndex >= commandRuntime.PositionalParameters.Count)
                            {
                                ConsoleEx.WriteLine(ConsoleColor.Red, "A unexpected value was found \"{0}\" but no positional option is available to get it. Type help {1} to check the expected parameters.", value, command);
                                return;
                            }

                            var param = commandRuntime.PositionalParameters[positionalIndex];

                            if (!parameterValues.TryGetValue(param.Names.First(), out var paramValue))
                            {
                                if (param.IsRepeated)
                                {
                                    paramValue = new List<string>();
                                }
                                else
                                {
                                    paramValue = value;
                                }
                            }

                            if (param.IsRepeated)
                            {
                                ((List<string>)paramValue).Add(value);
                            }

                            foreach (var name in param.Names)
                            {
                                parameterValues[name] = paramValue;
                            }

                            if (!param.IsRepeated)
                            {
                                positionalIndex++;
                            }
                        }
                        else if (shortOptions != null && shortOptions.Length > 1)
                        {
                            // It is a bunch of flags to turn on
                            foreach (var character in shortOptions)
                            {
                                var name = character.ToString();

                                var param = commandRuntime.Parameters.FirstOrDefault(p => p.Names.Contains(name, StringComparer.InvariantCultureIgnoreCase));

                                if (param == null)
                                {
                                    ConsoleEx.WriteLine(ConsoleColor.Red, "Unknown flag \"{0}\" found in option \"-{1}\". Type help {2} to see available options.", name, shortOptions, command);
                                    return;
                                }

                                if (param.ParameterType != ReplParameterType.Flag)
                                {
                                    ConsoleEx.WriteLine(ConsoleColor.Red, "Expected flag \"{0}\" but found an option or positional parameter in option \"-{1}\". Type help {2} to see available options.", name, shortOptions, command);
                                    return;
                                }

                                if (parameterValues.ContainsKey(name))
                                {
                                    ConsoleEx.WriteLine(ConsoleColor.Red, "Flag \"{0}\" is repeated in option \"-{1}\". Type help {2} to see available options.", name, shortOptions, command);
                                    return;
                                }

                                foreach (var pName in param.Names)
                                {
                                    parameterValues[pName] = true;
                                }
                            }
                        }
                        else
                        {
                            var name = shortOptions ?? longOption;

                            var param = commandRuntime.Parameters.FirstOrDefault(p => p.Names.Contains(name, StringComparer.InvariantCultureIgnoreCase));

                            if (param == null)
                            {
                                ConsoleEx.WriteLine(ConsoleColor.Red, "Unknown option \"{0}\" found", name);
                                return;
                            }

                            if (param.ParameterType != ReplParameterType.Option)
                            {
                                ConsoleEx.WriteLine(ConsoleColor.Red, "Expected option \"{0}\" but found a flag or positional parameter", name);
                                return;
                            }

                            if (!param.IsRepeated && parameterValues.ContainsKey(param.Names.First()))
                            {
                                ConsoleEx.WriteLine(ConsoleColor.Red, "Option \"{0}\" is repeated but it is expected to have a single value", name);
                                return;
                            }

                            optionValues = new List<string>(); // Go to Option State
                            optionParam = param;
                        }
                    }
                    else
                    {
                        // Option State
                        if (value == null)
                        {
                            ConsoleEx.WriteLine(ConsoleColor.Red, "While reading option \"{0}\" it followed a flag or another option, but a value was expected. Type help {1} to see available options.", optionParam.Names.First(), command);
                            return;
                        }

                        optionValues.Add(value);

                        if (optionValues.Count >= optionParam.ValueCount)
                        {
                            object paramValue;

                            if (optionParam.IsRepeated && optionParam.ValueCount > 1)
                            {
                                if (!parameterValues.TryGetValue(optionParam.Names.First(), out paramValue))
                                {
                                    paramValue = new List<List<string>>();
                                }
                                ((List<List<string>>)paramValue).Add(optionValues);
                            }
                            else if (optionParam.IsRepeated)
                            {
                                if (!parameterValues.TryGetValue(optionParam.Names.First(), out paramValue))
                                {
                                    paramValue = new List<string>();
                                }
                                ((List<string>)paramValue).Add(optionValues[0]);
                            }
                            else if (optionParam.ValueCount > 1)
                            {
                                paramValue = optionValues;
                            }
                            else
                            {
                                paramValue = optionValues[0];
                            }

                            foreach (var pName in optionParam.Names)
                            {
                                parameterValues[pName] = paramValue;
                            }

                            optionParam = null;
                            optionValues = null;
                        }
                    }
                }

                // Check for missing options or positional parameters

                var missingOptions = commandRuntime.Parameters.Where(p => p.IsRequired && !parameterValues.ContainsKey(p.Names.First())).ToArray();

                if (missingOptions.Length > 0)
                {
                    ConsoleEx.WriteLine(ConsoleColor.Red, "The following options are required: \"{0}\"", String.Join(", ", missingOptions.Select(p => p.Names.First())));
                    return;
                }

                await commandRuntime.Execute(parameterValues);
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

            private ReplCommandRuntime FindCommand(string command)
            {
                return Commands.FirstOrDefault(c => c.Names.Contains(command, StringComparer.InvariantCultureIgnoreCase));
            }

            private ReplParameterRuntime FindCommandOption(ReplCommandRuntime command, string option)
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
                this.PositionalParameters = new ReadOnlyCollection<ReplParameterRuntime>(this.Parameters.Where(p => p.ParameterType == ReplParameterType.Positional).ToArray());
                this.Execute = builder.Execute;
            }

            public ReadOnlyCollection<string> Names { get; }
            public string Category { get; }
            public string Caption { get; }
            public string Description { get; }
            public ReadOnlyCollection<ReplParameterRuntime> Parameters { get; }
            public ReadOnlyCollection<ReplExampleRuntime> Examples { get; }
            public ReadOnlyCollection<ReplParameterRuntime> PositionalParameters { get; }
            public Func<Dictionary<string, object>, Task> Execute { get; }

            public string AllNames => string.Join("|", Names);

            public void PrintSummary(string indent)
            {
                ConsoleEx.Write(ConsoleColor.White, "{0}{1,-20}\t", indent, AllNames);
                
                ConsoleEx.WriteLine(ConsoleColor.Gray, Caption ?? "");
            }

            public void PrintHelp()
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
                        param.PrintSummary("    ");
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

            public void PrintSummary(string indent)
            {
                ConsoleEx.Write(ConsoleColor.White, "{0}{1,-20}\t", indent, AllNames);
                ConsoleEx.WriteLine(ConsoleColor.Gray, Caption ?? "");
            }

            public void PrintHelp()
            {
                PrintSummary("");
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
