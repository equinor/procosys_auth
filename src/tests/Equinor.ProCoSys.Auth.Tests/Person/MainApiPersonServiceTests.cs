using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Auth.Person;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Auth.Tests.Person
{
    [TestClass]
    public class MainApiPersonServiceTests
    {
        private readonly Guid _azureOid = Guid.NewGuid();
        private IOptionsMonitor<MainApiOptions> _mainApiOptionsMock;
        private IMainApiClient _mainApiClientMock;
        private MainApiPersonService _dut;

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptionsMock = Substitute.For<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptionsMock.CurrentValue
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });
            _mainApiClientMock = Substitute.For<IMainApiClient>();

            _dut = new MainApiPersonService(_mainApiClientMock, _mainApiOptionsMock);
        }

        [TestMethod]
        public async Task TryGetPersonByOidAsync_ShouldReturnPerson()
        {
            // Arrange
            var person = new ProCoSysPerson { FirstName = "Lars", LastName = "Monsen" };
            _mainApiClientMock.TryQueryAndDeserializeAsApplicationAsync<ProCoSysPerson>(Arg.Any<string>())
                .Returns(person);

            // Act
            var result = await _dut.TryGetPersonByOidAsync(_azureOid);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(person.FirstName, result.FirstName);
            Assert.AreEqual(person.LastName, result.LastName);
        }
    }
}
