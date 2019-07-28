using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using HelloWorld;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Shouldly;

namespace XUnitWiremock
{
    [Collection("Dependency Injection")]
    public class DiTests
    {
        private ServiceProvider ServiceProvider { get; }
        private readonly ILogger<DiTests> _logger;
        
        public DiTests(DiFixture di, ITestOutputHelper output)
        {
            ServiceProvider = di.ServiceProvider;

            _logger = ServiceProvider.GetService<ILoggerFactory>()
                .CreateLogger<DiTests>();

            _logger.LogDebug("Starting application");

        }

        [Fact]
        public async Task TestAsync()
        {
            _logger.LogDebug("Run a test");

            var api = ServiceProvider.GetService<IHelloWorldApi>();
            api.SetupGetFooResponse(new HellowWorldDto
            {
                Msg = "Hello world!"
            });

            var result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorldDto>();

            result.Msg.ShouldBe("Hello world!");
        }
    }
}
