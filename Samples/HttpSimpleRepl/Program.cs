using DotNetX;
using DotNetX.Repl;
using DotNetX.Repl.Annotations;
using DotNetX.Repl.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpSimpleRepl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddTransient<HttpClient>();
            services.AddSingleton<HttpRepl>();

            var serviceProvider = services.BuildServiceProvider();

            var repl = ActivatorUtilities.CreateInstance<HttpRepl>(serviceProvider);

            var runtime = ReplBuilder.FromInstance(serviceProvider, repl).Build();

            var engine = new ReplEngine(runtime);

            await engine.StartAsync();
        }

        //private static ReplEngine DefineBuilderSample(IServiceProvider serviceProvider)
        //{
        //    var replRuntime = new ReplBuilder()
        //        .WithCaption("HTTP Repl")
        //        .WithVersion("0.1")
        //        .WithDescription("Allows operating over an HTTP endpoint")

        //        .WithExample("connect http://example.com/ --swagger swagger/v1/swagger.json", b => b
        //            .WithCaption("Connect to endpoint with swagger spec")
        //            .WithDescription("Connect to the given URL as a base, and uses the given OAS definition to call the API, if any"))

        //        .WithCommand("connect", c => c
        //            .WithName("conn")
        //            .WithName("c")
        //            .WithCaption("Connect to an HTTP endpoint")
        //            .WithExecute(ExecuteConnect)
        //            .WithDescription("Configures the directory structure and base address of the api server based on the arguments and options specified. At least one of [rootAddress], [--base baseAddress] or [--swagger swaggerAddress] must be specified")

        //            .WithPositional("root", p => p
        //                .WithTypeName("rootAddress")
        //                .WithCaption("Root Address")
        //                .WithDescription("Will be used to automatically determine the base address and swagger address")
        //                .WithExample("connect http://example.com/", b => b
        //                    .WithCaption("Connect to endpoint with default base address and no swagger specification file."))
        //            ) // root

        //            .WithOption("base", p => p
        //                .WithName("b")
        //                .WithTypeName("baseAddress")
        //                .WithCaption("Base Address")
        //                .WithDescription("Will be used to automatically determine the base address and swagger address")
        //                .WithExample("connect http://example.com/ --base api", b => b
        //                    .WithCaption("Connect to given endpoint with given base address"))
        //            ) // base

        //            .WithOption("swagger", p => p
        //                .WithName("s")
        //                .WithTypeName("swaggerAddress")
        //                .WithCaption("Swagger Address")
        //                .WithDescription("Will be used to automatically determine the base address and swagger address")
        //                .WithExample("connect http://example.com/ --base api", b => b
        //                    .WithCaption("Connect to given endpoint with given base address"))
        //            ) // base

        //            .WithExample("connect http://example.com/ --swagger swagger/v1/swagger.json", b => b
        //                .WithCaption("Connect to endpoint with swagger spec")
        //                .WithDescription("Connect to the given URL as a base, and uses the given OAS definition to call the API, if any"))
        //        ) // connect

        //        .Build();

        //    var engine = new ReplEngine(replRuntime);

        //    return engine;
        //}

        //private static async Task ExecuteConnect(Dictionary<string, object> args)
        //{
        //    ConsoleEx.WriteLine(ConsoleColor.Green, "Connect executed!!!");
        //    foreach (var pair in args)
        //    {
        //        switch (pair.Value)
        //        {
        //            case List<List<string>> data:
        //                ConsoleEx.WriteLine(ConsoleColor.White, "{0}: ", pair.Key);
        //                foreach (var items in data)
        //                {
        //                    ConsoleEx.WriteLine(ConsoleColor.Gray, "  - {0}", string.Join(", ", items));
        //                }
        //                Console.WriteLine();
        //                break;

        //            case List<string> data:
        //                ConsoleEx.WriteLine(ConsoleColor.White, "{0}: ", pair.Key);
        //                foreach (var item in data)
        //                {
        //                    ConsoleEx.WriteLine(ConsoleColor.Gray, "  - {0}", item);
        //                }
        //                Console.WriteLine();
        //                break;

        //            default:
        //                ConsoleEx.Write(ConsoleColor.White, "{0}: ", pair.Key);
        //                ConsoleEx.WriteLine(ConsoleColor.Gray, "{0}", pair.Value);
        //                break;
        //        }
        //    }
        //}
    }

    [ReplController(
        "HTTP Repl",
        Version = "0.1",
        Description = "Allows operating over an HTTP endpoint")]
    public class HttpRepl : ReplBase<HttpReplState>
    {
        private readonly IServiceProvider serviceProvider;

        public bool IsConnected => State.RootAddress != null;
        public Uri RootAddress => State.RootAddress;
        public Uri BaseAddress => new Uri(State.RootAddress, State.BaseAddress);
        public Uri CurrentAddress => new Uri(BaseAddress, State.CurrentAddress);

        public override Task<string> Prompt => Task.FromResult(IsConnected ? CurrentAddress.AbsoluteUri : "(Disconnected)");

        public HttpRepl(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override bool CanPersistState => true;

        public override byte[] PersistState()
        {
            var json = JsonConvert.SerializeObject(State, Formatting.Indented);
            return Encoding.UTF8.GetBytes(json);
        }

        public override void LoadState(byte[] persistedState)
        {
            var json = Encoding.UTF8.GetString(persistedState);
            State = JsonConvert.DeserializeObject<HttpReplState>(json);
        }

        [ReplCommand(
            "connect",
            Caption = "Connect to an HTTP endpoint",
            Description = "Configures the directory structure and base address of the api server based on the arguments and options specified. At least one of [rootAddress], [--base baseAddress] or [--swagger swaggerAddress] must be specified")]
        [ReplNames("conn", "c")]
        public void ConnectAsync(

            [ReplParam(
                "rootAddress",
                Caption = "Root Address",
                Description = "Will be used to automatically determine the base address")]
            [ReplExample(
                "connect http://example.com/",
                Scope = ReplExampleScope.All,
                Caption = "Connect to endpoint with default base address \"/\"")]
            string root = null,

            [ReplOption(
                "baseAddress",
                Caption = "Base Address",
                Description = "Local address to connect to. Can be changed later with command `cd`")]
            [ReplNames("b")]
            [ReplExample(
                "connect http://example.com/ --base api",
                Scope = ReplExampleScope.Parent,
                Caption = "Connect to given endpoint with given base address.")]
            string @base = null)
        {
            if (@base == null && root == null)
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, "One of rootAddress or --base must be specified");
                return;
            }

            if (root != null && !Uri.IsWellFormedUriString(root, UriKind.Absolute))
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, "Root address must be a valid absolute Url");
                return;
            }

            if (@base != null && !Uri.IsWellFormedUriString(@base, UriKind.RelativeOrAbsolute))
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, "Base address must be a valid absolute or relative Url");
                return;
            }

            var rootAddress = root != null ? new Uri(root, UriKind.Absolute) : null;
            var baseAddress = @base != null ? new Uri(@base, UriKind.RelativeOrAbsolute) : new Uri("", UriKind.Relative);

            if (rootAddress == null && !baseAddress.IsAbsoluteUri)
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, "When root address is missing, base address must be an absolute Url");
                return;
            }

            if (rootAddress != null && baseAddress != null && baseAddress.IsAbsoluteUri)
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, "When root address is present, base address must be a relative Url");
                return;
            }

            if (rootAddress == null)
            {
                rootAddress = new Uri(baseAddress.GetLeftPart(UriPartial.Authority), UriKind.Absolute);
                baseAddress = new Uri(baseAddress.AbsolutePath);
            }

            State.RootAddress = rootAddress;
            State.BaseAddress = baseAddress;
            State.CurrentAddress = new Uri("", UriKind.Relative);
        }

        [ReplCommand(
            "set",
            Caption = "Set default header values",
            Description = "If any value already exists, it is overwritten. If empty value is set, the header is removed from default headers.")]
        public void SetHeader(
            [ReplParam("name", Caption = "Header name")]
            string name,

            [ReplParam("value", Caption = "Header value")]
            string value = null)
        {
            State.DefaultHeaders.RemoveAll(h => h.Key.ToLowerInvariant() == name.ToLowerInvariant());

            if (!value.IsNullOrWhiteSpace())
            {
                State.DefaultHeaders.Add(KeyValuePair.Create(name, value));
            }
        }

        [ReplCommand(
            "add",
            Caption = "Add default header values",
            Description = "If a header with same name already exists, the new value is added to the list.")]
        public void AddHeader(
            [ReplParam("name", Caption = "Header name")]
            string name,

            [ReplParam("value", Caption = "Header value")]
            string value)
        {
            State.DefaultHeaders.Add(KeyValuePair.Create(name, value));
        }

        [ReplCommand("GET", Caption = "Issues a GET request to the given path")]
        [ReplNames("g")]
        public async Task GetAsync(
            [ReplParam("path")]
            string path = "",

            [ReplFlag, ReplNames("b")]
            bool binary = false,

            CancellationToken cancellationToken = default)
        {
            if (!Uri.IsWellFormedUriString(path, UriKind.Relative))
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, $"Given path '{path}' is not a valid URL");
                return;
            }

            await IssueRequest(binary, client => client.GetAsync(path, cancellationToken), cancellationToken);
        }

        [ReplCommand("DELETE", Caption = "Issues a DELETE request to the given path")]
        [ReplNames("d")]
        public async Task DeleteAsync(
            [ReplParam("path")]
            string path = "",

            [ReplFlag, ReplNames("b")]
            bool binary = false,

            CancellationToken cancellationToken = default)
        {
            if (!Uri.IsWellFormedUriString(path, UriKind.Relative))
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, $"Given path '{path}' is not a valid URL");
                return;
            }

            await IssueRequest(binary, client => client.DeleteAsync(path, cancellationToken), cancellationToken);
        }

        [ReplCommand("show", Caption = "Show current state")]
        public Task ShowState()
        {
            if (IsConnected)
            {
                var form = new ConsoleForm
                {
                    Title = "HTTP REPL",
                    Width = 80,
                    FieldPad = 20
                }
                    .Add(new ColoredText(ConsoleColor.White, "Root"), State.RootAddress.ToString())
                    .Add(new ColoredText(ConsoleColor.White, "Base"), State.BaseAddress.ToString())
                    .Add(new ColoredText(ConsoleColor.White, "Current"), State.CurrentAddress.ToString())
                    .Add(new ColoredText(ConsoleColor.DarkYellow, "Download"), State.DownloadResponses.ToString())
                    .Add(new ColoredText(ConsoleColor.DarkYellow, "Binary"), State.ShowBinary.ToString())
                    ;

                foreach (var header in State.DefaultHeaders)
                {
                    form.Add(new ColoredText(ConsoleColor.DarkGreen, $"{header.Key}"), header.Value);
                }

                ConsoleEx.WriteForm(form);
                //form.Write(new
                //{
                //    root = State.RootAddress.ToString(),
                //    @base = State.BaseAddress.ToString(),
                //    current = State.CurrentAddress.ToString(),
                //}, "HTTP REPL");

                //ConsoleEx.WriteLine(new ConsoleForm
                //{
                //});
            }
            else
            {
                Console.WriteLine("Disconnected");
            }
            return Task.CompletedTask;
        }

        private async Task IssueRequest(bool binary, Func<HttpClient, Task<HttpResponseMessage>> useClient, CancellationToken cancellationToken)
        {
            if (!IsConnected)
            {
                ConsoleEx.WriteLine(ConsoleColor.Red, "Use command `connect` first to establish a base address");
                return;
            }

            using var client = serviceProvider.GetRequiredService<HttpClient>();

            client.BaseAddress = CurrentAddress;

            foreach (var pair in State.DefaultHeaders)
            {
                client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
            }

            try
            {
                var response = await useClient(client);

                await PrintResponse(response, binary, cancellationToken);
            }
            catch (Exception ex)
            {
                PrintException("Error issuing the request", ex);
            }
        }

        private async Task PrintResponse(HttpResponseMessage response, bool binary, CancellationToken cancellationToken)
        {
            ConsoleEx.Write(ConsoleColor.White, "HTTP{0} ", response.Version);

            // Status

            if (response.IsSuccessStatusCode)
            {
                ConsoleEx.Write(ConsoleColor.Green, "{0} {1}", (int)response.StatusCode, response.ReasonPhrase);
            }
            else if (response.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
            {
                ConsoleEx.Write(ConsoleColor.Yellow, "{0} {1}", (int)response.StatusCode, response.ReasonPhrase);
            }
            else
            {
                ConsoleEx.Write(ConsoleColor.Red, "{0} {1}", (int)response.StatusCode, response.ReasonPhrase);
            }
            Console.WriteLine();

            // Headers

            void PrintHeaders(string title, HttpHeaders headers)
            {
                if (headers.Any())
                {
                    ConsoleEx.WriteLine(ConsoleColor.Cyan, "#{0}", title);

                    foreach (var pair in headers)
                    {
                        foreach (var value in pair.Value)
                        {
                            ConsoleEx.Write(ConsoleColor.Gray, " - ");
                            ConsoleEx.Write(ConsoleColor.White, pair.Key);
                            ConsoleEx.WriteLine(ConsoleColor.Gray, ": {0}", value);
                        }
                    }
                }
            }

            PrintHeaders("Headers", response.Headers);

            // Content

            if (response.Content != null)
            {
                byte[] bytes;

                try
                {
                    bytes = await response.Content.ReadAsByteArrayAsync();
                }
                catch
                {
                    ConsoleEx.WriteLine(ConsoleColor.Red, "Error reading response content");
                    return;
                }

                if (!binary)
                {
                    try
                    {
                        PrintHeaders("Content Headers", response.Content.Headers);

                        var charSet = response.Content.Headers.ContentType.CharSet;
                        var encoding = charSet.IsNotNullOrWhiteSpace() ? Encoding.GetEncoding(charSet) : Encoding.UTF8;
                        var text = encoding.GetString(bytes);
                        // TODO: if it is JSON, pretty print it
                        Console.WriteLine(text);

                        return;
                    }
                    catch
                    {
                        // could not read as string, show as binary
                    }
                }

                Console.WriteLine(Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks));
            }

            // Trailing Headers

            PrintHeaders("Trailing Headers", response.TrailingHeaders);
        }

        private void PrintException(string title, Exception ex)
        {
            ConsoleEx.WriteLine(ConsoleColor.White, ConsoleColor.Red, title);
            ConsoleEx.WriteLine(ConsoleColor.Cyan, ex.GetType().FullName);
            ConsoleEx.WriteLine(ConsoleColor.White, ex.Message);
            ConsoleEx.WriteLine(ConsoleColor.Gray, ex.ToString());
            Console.WriteLine();
        }
    }

    public class HttpReplState
    {
        [JsonConverter(typeof(UriJsonConverter))]
        public Uri RootAddress { get; set; }
        [JsonConverter(typeof(UriJsonConverter))]
        public Uri BaseAddress { get; set; }
        [JsonConverter(typeof(UriJsonConverter))]
        public Uri CurrentAddress { get; set; }
        public List<KeyValuePair<string, string>> DefaultHeaders { get; set; } = new List<KeyValuePair<string, string>>();

        // Config
        public bool DownloadResponses { get; set; } = false;
        public bool ShowBinary { get; set; } = false;
    }

    public class UriJsonConverter : JsonConverter<Uri>
    {
        public override Uri ReadJson(JsonReader reader, Type objectType, [AllowNull] Uri existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                return new Uri((string)reader.Value, UriKind.RelativeOrAbsolute);
            }
            else
            {
                throw new InvalidOperationException($"Expected a string to read a System.Uri, but found a {reader.TokenType}");
            }
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Uri value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(value.ToString());
            }
        }
    }

}
