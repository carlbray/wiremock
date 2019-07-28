using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Flurl;
using Flurl.Http;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using System.Collections.Specialized;
using Xunit.Abstractions;
using Newtonsoft.Json;
using System.Threading;
using WireMock.Logging;
using System;
using Shouldly;


namespace XUnitTestProject1
{
    [Collection("Wiremock")]
    public class EventTests
    {
        private readonly WiremockFixture _wiremockFixture;
        private readonly ITestOutputHelper _output;

        public EventTests(WiremockFixture wiremockFixture, ITestOutputHelper output)
        {
            _wiremockFixture = wiremockFixture;
            _output = output;
        }

        [Fact]
        public async Task TestPassingCheck()
        {
            var fooEvent = SetupResponse(CreateHelloWorldResponse("Hello world!"));

            var result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorldDto>();

            fooEvent.WaitOne(1000).ShouldBeTrue();
            result.Msg.ShouldBe("Hello world!");
        }

        [Fact]
        public async Task TestWhenApiCalledTwice()
        {
            var fooEvent = SetupResponse(CreateHelloWorldResponse("Hello world!"));
            var fooEvent2 = SetupResponse(CreateHelloWorldResponse("Hello world!"));

            var result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorldDto>();

            fooEvent.WaitOne(1000).ShouldBeTrue();
            result.Msg.ShouldBe("Hello world!");

            result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorldDto>();

            fooEvent2.WaitOne(1000).ShouldBeTrue();
            result.Msg.ShouldBe("Hello world!");
        }

        [Fact]
        public async Task TestWhenApiCalledTwice1EventHandler()
        {
            var fooEvent = SetupResponse(CreateHelloWorldResponse("Hello world!"));

            var result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorldDto>();

            fooEvent.WaitOne(1000).ShouldBeTrue();
            result.Msg.ShouldBe("Hello world!");

            var result2 = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .AllowAnyHttpStatus()
                                    .GetAsync()
                                    .ReceiveString();

            fooEvent.WaitOne(1000).ShouldBeFalse();
            result2.ShouldBe("Request not mapped!");
        }

        [Fact]
        public async Task TestFailingCheck()
        {
            var barEvent = SetupResponse(CreateHelloWorldResponse("Hello world!"));

            var result = await "http://localhost:8081"
                                    .AppendPathSegment("bar")
                                    .AllowAnyHttpStatus()
                                    .GetAsync()
                                    .ReceiveString();

            barEvent.WaitOne(1000).ShouldBeFalse();
            result.ShouldBe("Request not mapped!");
        }

        // Check when you expect the call to be made twice. As the mapping will be reused.

        internal class HellowWorldDto
        {
            public string Msg { get; set; }
        }

        private AutoResetEvent SetupResponse(HellowWorldDto hw)
        {
            Guid guid = Guid.NewGuid();
            _wiremockFixture.Server.Given(Request.Create().WithPath("/foo").UsingGet())
                                    .WithGuid(guid)
                                    .RespondWith(
                                      Response.Create()
                                        .WithSuccess()
                                        .WithHeader(HeaderNames.ContentType, "application/json")
                                        .WithBodyAsJson(hw)
                                    );
            var callEvent = new AutoResetEvent(false);
            _wiremockFixture.Server.LogEntriesChanged += CheckForApiCall(callEvent, guid);
            return callEvent;
        }

        private HellowWorldDto CreateHelloWorldResponse(string msg)
        {
            return new HellowWorldDto
            {
                Msg = msg
            };
        }

        private NotifyCollectionChangedEventHandler CheckForApiCall(AutoResetEvent waitEvent, Guid guid)
        {
            return (sender, eventArgs) =>
            {
                foreach(var  item in eventArgs.NewItems)
                {
                    if(item is LogEntry entry && entry.MappingGuid.Equals(guid)) {
                        waitEvent.Set();
                        _wiremockFixture.Server.DeleteMapping(guid);
                        break;
                    }
                }
            };
        }

        private void WriteEntryToLogging(LogEntry entry)
        {
            var json = entry.ResponseMessage.BodyData.BodyAsJson;
            if (null != json)
            {
                _output.WriteLine(JsonConvert.SerializeObject(json, Formatting.Indented));
            }
            else
            {
                _output.WriteLine(entry.ResponseMessage.BodyData.BodyAsString);
            }
        }
    }
}
