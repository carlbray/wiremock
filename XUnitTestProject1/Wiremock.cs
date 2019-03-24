using System;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Flurl;
using Flurl.Http;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Serilog;
using Serilog.Core;

namespace XUnitTestProject1
{
    [Collection("Wiremock")]
    public class Wiremock
    {
        private readonly WiremockFixture _wiremockFixture;

        private static readonly Logger log = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("/Logs/log.log")
            .CreateLogger();

        public Wiremock(WiremockFixture wiremockFixture)
        {
            _wiremockFixture = wiremockFixture;
        }

        [Fact]
        public async Task TestDemoJsonBody()
        {
            HellowWorld hw = new HellowWorld {
                Msg = "Hello world!"
            };

            log.Information("Checking {@HelloWorld}", hw);

            _wiremockFixture.Server.Given(Request.Create().WithPath("/foo").UsingGet())
                                    .RespondWith(
                                      Response.Create()
                                        .WithSuccess()
                                        .WithHeader(HeaderNames.ContentType, "application/json")
                                        .WithBodyAsJson(hw)
                                        .WithDelay(TimeSpan.FromSeconds(1))
                                    );

            HellowWorld result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorld>();

            Assert.Equal("Hello world!", result.Msg);
        }
    }

    internal class HellowWorld
    {
        public string Msg { get; set; }
    }
}