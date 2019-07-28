using Microsoft.Net.Http.Headers;
using System;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace HelloWorld
{
    public interface IHelloWorldApi
    {
        Guid SetupGetFooResponse(HellowWorldDto hw);
    }

    public class HelloWorldApi : IHelloWorldApi
    {
        private FluentMockServer Server { get; }

        public HelloWorldApi(FluentMockServer server)
        {
            Server = server;
        }

        public Guid SetupGetFooResponse(HellowWorldDto hw)
        {
            Guid guid = Guid.NewGuid();
            Server.Given(BuildGetFooRequest())
                    .WithGuid(guid)
                    .RespondWith(BuildGetFooResponse(hw));
            return guid;
        }

        private IResponseBuilder BuildGetFooResponse(HellowWorldDto hw)
        {
            return Response.Create()
                    .WithSuccess()
                    .WithHeader(HeaderNames.ContentType, "application/json")
                    .WithBodyAsJson(hw);
        }

        private IRequestBuilder BuildGetFooRequest()
        {
            return Request.Create()
                    .WithPath("/foo")
                    .UsingGet();
        }
    }

    public class HellowWorldDto
    {
        public string Msg { get; set; }
    }
}
