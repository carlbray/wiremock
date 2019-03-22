using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Flurl;
using Flurl.Http;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace XUnitTestProject1
{
    [Collection("Wiremock")]
    public class UnitTest1
    {
        private readonly WiremockFixture _wiremockFixture;

        public UnitTest1(WiremockFixture wiremockFixture)
        {
            _wiremockFixture = wiremockFixture;
        }

        [Fact]
        public async Task TestDemoJsonBody()
        {
            HellowWorld hw = new HellowWorld {
                Msg = "Hello world!"
            };

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

        public class WiremockFixture
        {
            public FluentMockServer Server { get; }

            public WiremockFixture()
            {
                Server = FluentMockServer.Start(8081);
            }
        }

        [CollectionDefinition("Wiremock")]
        public class WiremockCollection : ICollectionFixture<WiremockFixture>
        {
        }
    }

    internal class HellowWorld
    {
        public string Msg { get; set; }
    }
}