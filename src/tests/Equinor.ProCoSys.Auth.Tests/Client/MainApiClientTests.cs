using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Auth.Tests.Client
{
    [TestClass]
    public class MainApiClientTests
    {
        private IMainApiAuthenticator _bearerTokenProviderMock;
        private ILogger<MainApiClient> _loggerMock;

        [TestInitialize]
        public void Setup()
        {
            _bearerTokenProviderMock = Substitute.For<IMainApiAuthenticator>();
            _loggerMock = Substitute.For<ILogger<MainApiClient>>();
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ShouldReturnDeserialized_Object_TestAsync()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "{\"Id\": 123}");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProviderMock, _loggerMock);

            var response = await dut.QueryAndDeserializeAsync<DummyClass>("url");

            Assert.IsNotNull(response);
            Assert.AreEqual(123, response.Id);
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenRequestIsNotSuccessful_TestAsync()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.BadGateway, "");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProviderMock, _loggerMock);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await dut.QueryAndDeserializeAsync<DummyClass>("url"));
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenInvalidResponseIsReceived_TestAsync()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProviderMock, _loggerMock);

            await Assert.ThrowsExceptionAsync<JsonException>(async () => await dut.QueryAndDeserializeAsync<DummyClass>("url"));
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenNoUrl()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProviderMock, _loggerMock);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await dut.QueryAndDeserializeAsync<DummyClass>(null));
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenUrlTooLong()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProviderMock, _loggerMock);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await dut.QueryAndDeserializeAsync<DummyClass>(new string('u', 2001)));
        }

        private class DummyClass
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int Id { get; set; }
        }
    }
}
