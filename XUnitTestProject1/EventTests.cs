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
            var fooEvent = SetupResponse();

            var result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorld>();

            Assert.True(fooEvent.WaitOne(1000));
            Assert.Equal("Hello world!", result.Msg);
        }

        [Fact]
        public async Task TestWhenApiCalledTwice()
        {
            var fooEvent = SetupResponse();
            var fooEvent2 = SetupResponse();

            var result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorld>();

            Assert.True(fooEvent.WaitOne(1000));
            Assert.Equal("Hello world!", result.Msg);

            result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorld>();

            Assert.True(fooEvent2.WaitOne(1000));
            Assert.Equal("Hello world!", result.Msg);
        }

        [Fact]
        public async Task TestWhenApiCalledTwice1EventHandler()
        {
            var fooEvent = SetupResponse();

            var result = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .GetAsync()
                                    .ReceiveJson<HellowWorld>();

            Assert.True(fooEvent.WaitOne(1000));
            Assert.Equal("Hello world!", result.Msg);

            var result2 = await "http://localhost:8081"
                                    .AppendPathSegment("foo")
                                    .AllowAnyHttpStatus()
                                    .GetAsync()
                                    .ReceiveString();

            Assert.False(fooEvent.WaitOne(1000));
            Assert.Equal("Request not mapped!", result2);
        }

        [Fact]
        public async Task TestFailingCheck()
        {
            var barEvent = SetupResponse();

            var result = await "http://localhost:8081"
                                    .AppendPathSegment("bar")
                                    .AllowAnyHttpStatus()
                                    .GetAsync()
                                    .ReceiveString();

            Assert.False(barEvent.WaitOne(1000));
            Assert.Equal("Request not mapped!", result);
        }

        // Check when you expect the call to be made twice. As the mapping will be reused.

        internal class HellowWorld
        {
            public string Msg { get; set; }
        }

        private AutoResetEvent SetupResponse()
        {
            HellowWorld hw = new HellowWorld
            {
                Msg = "Hello world!"
            };

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
