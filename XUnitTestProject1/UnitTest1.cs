using System;
using System.Threading.Tasks;
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
        public async Task TestDemo()
        {
            _wiremockFixture.Server.Given(Request.Create().WithPath("/foo").UsingGet())
                                    .RespondWith(
                                      Response.Create()
                                        .WithStatusCode(200)
                                        .WithHeader("Content-Type", "application/json")
                                        .WithBody(@"{ ""msg"": ""Hello world!"" }")
                                        .WithDelay(TimeSpan.FromSeconds(1))
                                    );

            dynamic result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetJsonAsync();

            Assert.Equal("Hello world!", result.msg);
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
}