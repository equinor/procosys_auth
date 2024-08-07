using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Auth.Tests.Client
{
    [TestClass]
    public class BearerTokenApiClientTests
    {
        private readonly ILogger<BearerTokenApiClient> _loggerMock = Substitute.For<ILogger<BearerTokenApiClient>>();

        [TestMethod]
        public async Task QueryAndDeserialize_ShouldReturnDeserialized_Object_TestAsync()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "{\"Id\": 123}");
            var dut = new TestableClient("MyClient", httpClientFactory, _loggerMock);

            var response = await dut.QueryAndDeserializeAsync<DummyClass>("url", CancellationToken.None);

            Assert.IsNotNull(response);
            Assert.AreEqual(123, response.Id);
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenRequestIsNotSuccessful_TestAsync()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.BadGateway, "");
            var dut = new TestableClient("MyClient", httpClientFactory, _loggerMock);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await dut.QueryAndDeserializeAsync<DummyClass>("url", CancellationToken.None));
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenInvalidResponseIsReceived_TestAsync()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "");
            var dut = new TestableClient("MyClient", httpClientFactory, _loggerMock);

            await Assert.ThrowsExceptionAsync<JsonException>(async () => await dut.QueryAndDeserializeAsync<DummyClass>("url", CancellationToken.None));
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenNoUrl()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "");
            var dut = new TestableClient("MyClient", httpClientFactory, _loggerMock);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await dut.QueryAndDeserializeAsync<DummyClass>(null, CancellationToken.None));
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenUrlTooLong()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "");
            var dut = new TestableClient("MyClient", httpClientFactory, _loggerMock);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await dut.QueryAndDeserializeAsync<DummyClass>(new string('u', 2001), CancellationToken.None));
        }

        private class TestableClient(
            string clientName,
            IHttpClientFactory httpClientFactory,
            ILogger<BearerTokenApiClient> logger)
            : BearerTokenApiClient(clientName, httpClientFactory, logger);

        private class DummyClass
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int Id { get; set; }
        }
    }
}
