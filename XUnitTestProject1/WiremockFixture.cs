﻿using Microsoft.Net.Http.Headers;
using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
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

            //Server.AddCatchAllMapping();

            Server.Given(Request.Create().WithPath("/*").UsingAnyMethod())
                        .AtPriority(1000)
                        .RespondWith(
                            Response.Create()
                            .WithStatusCode(HttpStatusCode.NotImplemented)
                            .WithHeader(HeaderNames.ContentType, "application/text")
                            .WithBody("Request not mapped!")
                        );

        }
    }

    [CollectionDefinition("Wiremock")]
    public class WiremockCollection : ICollectionFixture<WiremockFixture>
    {
    }
}