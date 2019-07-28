using HelloWorld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using WireMock.Server;
using Xunit;

namespace XUnitWiremock
{
    public class DiFixture : IDisposable
    {
        public FluentMockServer Server { get; }
        public ServiceProvider ServiceProvider { get; private set; }

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public DiFixture()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            Server = FluentMockServer.Start(8081);

            var serviceCollection = new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
                .AddSingleton(typeof(IHelloWorldApi), new HelloWorldApi(Server));

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public void Dispose()
        {
            Server.Stop();
        }
    }

    [CollectionDefinition("Dependency Injection")]
    public class DiCollection : ICollectionFixture<DiFixture>
    {
    }
}
