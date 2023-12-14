using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Auth.Permission;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Auth.Tests.Permission
{
    [TestClass]
    public class MainApiPermissionServiceTests
    {
        private readonly Guid _azureOid = Guid.NewGuid();
        private readonly string _plant = "PCS$TESTPLANT";
        private IOptionsMonitor<MainApiOptions> _mainApiOptionsMock;
        private IMainApiClient _mainApiClientMock;
        private MainApiPermissionService _dut;

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptionsMock = Substitute.For<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptionsMock.CurrentValue
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });
            _mainApiClientMock = Substitute.For<IMainApiClient>();

            _dut = new MainApiPermissionService(_mainApiClientMock, _mainApiOptionsMock);
        }

        [TestMethod]
        public async Task GetAllPlants_ShouldReturnCorrectNumberOfPlants()
        {
            // Arrange
            _mainApiClientMock.QueryAndDeserializeAsApplicationAsync<List<AccessablePlant>>(Arg.Any<string>())
                .Returns(new List<AccessablePlant>
                {
                    new() { Id = "PCS$ASGARD", Title = "Åsgard" },
                    new() { Id = "PCS$ASGARD_A", Title = "ÅsgardA" },
                    new() { Id = "PCS$ASGARD_B", Title = "ÅsgardB" },
                });

            // Act
            var result = await _dut.GetAllPlantsForUserAsync(_azureOid);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetAllPlants_ShouldSetsCorrectProperties()
        {
            // Arrange
            var proCoSysPlant = new AccessablePlant { Id = "PCS$ASGARD_B", Title = "ÅsgardB" };
            _mainApiClientMock.QueryAndDeserializeAsApplicationAsync<List<AccessablePlant>>(Arg.Any<string>())
                .Returns(new List<AccessablePlant> { proCoSysPlant });
            // Act
            var result = await _dut.GetAllPlantsForUserAsync(_azureOid);

            // Assert
            var plant = result.Single();
            Assert.AreEqual(proCoSysPlant.Id, plant.Id);
            Assert.AreEqual(proCoSysPlant.Title, plant.Title);
        }

        [TestMethod]
        public async Task GetPermissions_ShouldReturnThreePermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClientMock.QueryAndDeserializeAsync<List<string>>(Arg.Any<string>())
                .Returns(new List<string> { "A", "B", "C" });

            // Act
            var result = await _dut.GetPermissionsForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetPermissions_ShouldReturnNoPermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClientMock.QueryAndDeserializeAsync<List<string>>(Arg.Any<string>())
                .Returns(new List<string>());

            // Act
            var result = await _dut.GetPermissionsForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPermissions_ShouldReturnNoPermissions_OnInvalidPlant()
        {
            // Act
            var result = await _dut.GetPermissionsForCurrentUserAsync("INVALIDPLANT");

            // Assert
            Assert.AreEqual(0, result.Count);
        }
 
        [TestMethod]
        public async Task GetAllOpenProjectsAsync_ShouldReturnTwoProjects_OnValidPlant()
        {
            // Arrange
            _mainApiClientMock.QueryAndDeserializeAsync<List<AccessableProject>>(Arg.Any<string>())
                .Returns(new List<AccessableProject> { new(), new() });

            // Act
            var result = await _dut.GetAllOpenProjectsForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetAllOpenProjectsAsync_ShouldReturnNoProjects_OnValidPlant()
        {
            // Arrange
            _mainApiClientMock.QueryAndDeserializeAsync<List<AccessableProject>>(Arg.Any<string>())
                .Returns(new List<AccessableProject>());

            // Act
            var result = await _dut.GetAllOpenProjectsForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllOpenProjectsAsync_ShouldReturnNoProjects_OnInvalidPlant()
        {
            // Act
            var result = await _dut.GetAllOpenProjectsForCurrentUserAsync("INVALIDPLANT");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetRestrictionRolesAsync_ShouldReturnThreePermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClientMock.QueryAndDeserializeAsync<List<string>>(Arg.Any<string>())
                .Returns(new List<string> { "A", "B", "C" });

            // Act
            var result = await _dut.GetRestrictionRolesForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetRestrictionRolesAsync_ShouldReturnNoPermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClientMock.QueryAndDeserializeAsync<List<string>>(Arg.Any<string>())
                .Returns(new List<string>());

            // Act
            var result = await _dut.GetRestrictionRolesForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetRestrictionRolesAsync_ShouldReturnNoPermissions_OnInValidPlant()
        {
            // Act
            var result = await _dut.GetRestrictionRolesForCurrentUserAsync("INVALIDPLANT");

            // Assert
            Assert.AreEqual(0, result.Count);
        }
    }
}
