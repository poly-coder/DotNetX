using DotNetX.Repl;
using DotNetX.Repl.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
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
                .WithExample("connect http://example.com/ --swagger swagger/v1/swagger.json", b =>
                {
                    b.WithDescription("Connect to the given URL as a base, and uses the given OAS definition to call the API, if any");
                })
                .Build();

            var engine = new ReplEngine(replRuntime);

            return engine;
        }
    }
}
