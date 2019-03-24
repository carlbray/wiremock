using WireMock.Server;
using Xunit;

namespace XUnitTestProject1
{
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