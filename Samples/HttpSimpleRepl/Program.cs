using DotNetX.Repl;
using DotNetX.Repl.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpSimpleRepl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddTransient<HttpClient>();

            var serviceProvider = services.BuildServiceProvider();

            var repl = DefineBuilderSample(serviceProvider);

            await repl.StartAsync(serviceProvider);
        }

        private static ReplEngine DefineBuilderSample(IServiceProvider serviceProvider)
        {
            var replRuntime = new ReplBuilder()
                .WithCaption("HTTP Repl")
                .WithVersion("0.1")
                .WithDescription("Allows operating over an HTTP endpoint")
                
                .WithExample("connect http://example.com/ --swagger swagger/v1/swagger.json", b => b
                    .WithCaption("Connect to endpoint with swagger spec")
                    .WithDescription("Connect to the given URL as a base, and uses the given OAS definition to call the API, if any"))

                .WithCommand("connect", c => c
                    .WithName("conn")
                    .WithName("c")
                    .WithCaption("Connect to an HTTP endpoint")
                    .WithExecute(ExecuteConnect)
                    .WithDescription("Configures the directory structure and base address of the api server based on the arguments and options specified. At least one of [rootAddress], [--base baseAddress] or [--swagger swaggerAddress] must be specified")
                    
                    .WithPositional("root", p => p
                        .WithTypeName("rootAddress")
                        .WithCaption("Root Address")
                        .WithDescription("Will be used to automatically determine the base address and swagger address")
                        .WithExample("connect http://example.com/", b => b
                            .WithCaption("Connect to endpoint with default base address and no swagger specification file."))
                    ) // root

                    .WithOption("base", p => p
                        .WithName("b")
                        .WithTypeName("baseAddress")
                        .WithCaption("Base Address")
                        .WithDescription("Will be used to automatically determine the base address and swagger address")
                        .WithExample("connect http://example.com/ --base api", b => b
                            .WithCaption("Connect to given endpoint with given base address"))
                    ) // base

                    .WithOption("swagger", p => p
                        .WithName("s")
                        .WithTypeName("swaggerAddress")
                        .WithCaption("Swagger Address")
                        .WithDescription("Will be used to automatically determine the base address and swagger address")
                        .WithExample("connect http://example.com/ --base api", b => b
                            .WithCaption("Connect to given endpoint with given base address"))
                    ) // base

                    .WithExample("connect http://example.com/ --swagger swagger/v1/swagger.json", b => b
                        .WithCaption("Connect to endpoint with swagger spec")
                        .WithDescription("Connect to the given URL as a base, and uses the given OAS definition to call the API, if any"))
                ) // connect

                .Build();

            var engine = new ReplEngine(replRuntime);

            return engine;
        }

        private static async Task ExecuteConnect(Dictionary<string, object> arg)
        {
            ConsoleEx.WriteLine(ConsoleColor.Green, "Connect executed!!!");
        }
    }
}
