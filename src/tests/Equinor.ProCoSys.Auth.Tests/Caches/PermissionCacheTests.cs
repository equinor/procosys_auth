using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Tests;
using Equinor.ProCoSys.Common.Time;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Auth.Tests.Caches
{
    [TestClass]
    public class PermissionCacheTests
    {
        private PermissionCache _dut;
        private readonly Guid _currentUserOid = new("{3BFB54C7-91E2-422E-833F-951AD07FE37F}");
        private IPermissionApiService _permissionApiServiceMock;
        private ICurrentUserProvider _currentUserProviderMock;
        private readonly string Plant1IdWithAccess = "P1";
        private readonly string Plant2IdWithAccess = "P2";
        private readonly string PlantIdWithoutAccess = "P3";
        private readonly string Plant1TitleWithAccess = "P1 Title";
        private readonly string Plant2TitleWithAccess = "P2 Title";
        private readonly string PlantTitleWithoutAccess = "P3 Title";
        private readonly string Permission1 = "A";
        private readonly string Permission2 = "B";
        private readonly string Project1WithAccess = "P1";
        private readonly string Project2WithAccess = "P2";
        private readonly string ProjectWithoutAccess = "P3";
        private readonly string Restriction1 = "R1";
        private readonly string Restriction2 = "R2";

        [TestInitialize]
        public void Setup()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

            _permissionApiServiceMock = Substitute.For<IPermissionApiService>();
            _permissionApiServiceMock.GetAllPlantsForUserAsync(_currentUserOid, CancellationToken.None)
                .Returns(
                [
                    new()
                    {
                        Id = Plant1IdWithAccess,
                        Title = Plant1TitleWithAccess,
                        HasAccess = true
                    },

                    new()
                    {
                        Id = Plant2IdWithAccess,
                        Title = Plant2TitleWithAccess,
                        HasAccess = true
                    },

                    new()
                    {
                        Id = PlantIdWithoutAccess,
                        Title = PlantTitleWithoutAccess
                    }
                ]);
            _permissionApiServiceMock.GetAllOpenProjectsForCurrentUserAsync(Plant1IdWithAccess, CancellationToken.None)
                .Returns([
                    new() { Name = Project1WithAccess, HasAccess = true },
                    new() { Name = Project2WithAccess, HasAccess = true },
                    new() { Name = ProjectWithoutAccess }
                ]);
            _permissionApiServiceMock.GetPermissionsForCurrentUserAsync(Plant1IdWithAccess, CancellationToken.None)
                .Returns([Permission1, Permission2]);
            _permissionApiServiceMock.GetRestrictionRolesForCurrentUserAsync(Plant1IdWithAccess, CancellationToken.None)
                .Returns([Restriction1, Restriction2]);

            var optionsMock = Substitute.For<IOptionsMonitor<CacheOptions>>();
            optionsMock.CurrentValue.Returns(new CacheOptions());

            _currentUserProviderMock = Substitute.For<ICurrentUserProvider>();
            _currentUserProviderMock.GetCurrentUserOid().Returns(_currentUserOid);

            OptionsWrapper<MemoryDistributedCacheOptions> _options = new(new MemoryDistributedCacheOptions());
            _dut = new PermissionCache(
                new DistributedCacheManager(new MemoryDistributedCache(_options)),
                _currentUserProviderMock,
                _permissionApiServiceMock,
                optionsMock);
        }

        [TestMethod]
        public async Task GetPlantIdsWithAccessForUserAsync_ShouldReturnPlantIdsFromPlantApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetPlantIdsWithAccessForUserAsync(_currentUserOid, CancellationToken.None);

            // Assert
            AssertPlants(result);
            await _permissionApiServiceMock.Received(1).GetAllPlantsForUserAsync(_currentUserOid, CancellationToken.None);
        }

        [TestMethod]
        public async Task GetPlantIdsWithAccessForUserAsync_ShouldReturnPlantsFromCacheSecondTime()
        {
            await _dut.GetPlantIdsWithAccessForUserAsync(_currentUserOid, CancellationToken.None);

            // Act
            var result = await _dut.GetPlantIdsWithAccessForUserAsync(_currentUserOid, CancellationToken.None);

            // Assert
            AssertPlants(result);
            // since GetPlantIdsWithAccessForUserAsyncAsync has been called twice, but GetAllPlantsAsync has been called once, the second Get uses cache
            await _permissionApiServiceMock.Received(1).GetAllPlantsForUserAsync(_currentUserOid, CancellationToken.None);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlantAsync_ShouldReturnTrue_WhenKnownPlant()
        {
            // Act
            var result = await _dut.HasCurrentUserAccessToPlantAsync(Plant2IdWithAccess, CancellationToken.None);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlantAsync_ShouldReturnFalse_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.HasCurrentUserAccessToPlantAsync("XYZ", CancellationToken.None);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlantAsync_ShouldReturnPlantIdsFromPlantApiServiceFirstTime()
        {
            // Act
            await _dut.HasCurrentUserAccessToPlantAsync(Plant2IdWithAccess, CancellationToken.None);

            // Assert
            await _permissionApiServiceMock.Received(1).GetAllPlantsForUserAsync(_currentUserOid, CancellationToken.None);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlantAsync_ShouldReturnPlantsFromCacheSecondTime()
        {
            await _dut.HasCurrentUserAccessToPlantAsync("XYZ", CancellationToken.None);
            // Act
            await _dut.HasCurrentUserAccessToPlantAsync(Plant2IdWithAccess, CancellationToken.None);

            // Assert
            await _permissionApiServiceMock.Received(1).GetAllPlantsForUserAsync(_currentUserOid, CancellationToken.None);
        }

        [TestMethod]
        public async Task HasUserAccessToPlantAsync_ShouldReturnTrue_WhenKnownPlant()
        {
            // Act
            var result = await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HasUserAccessToPlantAsync_ShouldReturnFalse_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.HasUserAccessToPlantAsync("XYZ", _currentUserOid, CancellationToken.None);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasUserAccessToPlantAsync_ShouldReturnPlantIdsFromPlantApiServiceFirstTime()
        {
            // Act
            await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            await _permissionApiServiceMock.Received(1).GetAllPlantsForUserAsync(_currentUserOid, CancellationToken.None);
        }

        [TestMethod]
        public async Task HasUserAccessToPlantAsync_ShouldReturnPlantsFromCache()
        {
            await _dut.HasUserAccessToPlantAsync("ABC", _currentUserOid, CancellationToken.None);
            // Act
            await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            await _permissionApiServiceMock.Received(1).GetAllPlantsForUserAsync(_currentUserOid, CancellationToken.None);
        }

        [TestMethod]
        public async Task IsAValidPlantForCurrentUserAsync_ShouldReturnTrue_WhenKnownPlantWithAccess()
        {
            // Act
            var result = await _dut.IsAValidPlantForCurrentUserAsync(Plant2IdWithAccess, CancellationToken.None);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsAValidPlantForCurrentUserAsync_ShouldReturnTrue_WhenKnownPlantWithoutAccess()
        {
            // Act
            var result = await _dut.IsAValidPlantForCurrentUserAsync(PlantIdWithoutAccess, CancellationToken.None);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsAValidPlantForCurrentUserAsync_ShouldReturnFalse_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.IsAValidPlantForCurrentUserAsync("XYZ", CancellationToken.None);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetPlantTitleForCurrentUserAsync_ShouldReturnPlant_WhenKnownPlantWithAccess()
        {
            // Act
            var result = await _dut.GetPlantTitleForCurrentUserAsync(Plant2IdWithAccess, CancellationToken.None);

            // Assert
            Assert.AreEqual(Plant2TitleWithAccess, result);
        }

        [TestMethod]
        public async Task GetPlantTitleForCurrentUserAsync_ShouldReturnPlant_WhenKnownPlantWithoutAccess()
        {
            // Act
            var result = await _dut.GetPlantTitleForCurrentUserAsync(PlantIdWithoutAccess, CancellationToken.None);

            // Assert
            Assert.AreEqual(PlantTitleWithoutAccess, result);
        }

        [TestMethod]
        public async Task GetPlantTitleForCurrentUserAsync_ShouldReturnNull_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.GetPlantTitleForCurrentUserAsync("XYZ", CancellationToken.None);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Clear_ShouldForceGettingPlantsFromApiServiceAgain()
        {
            // Arrange
            var result = await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid, CancellationToken.None);
            Assert.IsTrue(result);
            await _permissionApiServiceMock.Received(1).GetAllPlantsForUserAsync(_currentUserOid, CancellationToken.None);

            // Act
            await _dut.ClearAllAsync(Plant2IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            result = await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid, CancellationToken.None);
            Assert.IsTrue(result);
            await _permissionApiServiceMock.Received(2).GetAllPlantsForUserAsync(_currentUserOid, CancellationToken.None);
        }

        [TestMethod]
        public async Task GetPermissionsForUserAsync_ShouldReturnPermissionsFromPermissionApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetPermissionsForUserAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            AssertPermissions(result);
            await _permissionApiServiceMock.Received(1).GetPermissionsForCurrentUserAsync(Plant1IdWithAccess, CancellationToken.None);
        }

        [TestMethod]
        public async Task GetPermissionsForUserAsync_ShouldReturnPermissionsFromCacheSecondTime()
        {
            await _dut.GetPermissionsForUserAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);
            // Act
            var result = await _dut.GetPermissionsForUserAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            AssertPermissions(result);
            // since GetPermissionsForUserAsync has been called twice, but GetPermissionsAsync has been called once, the second Get uses cache
            await _permissionApiServiceMock.Received(1).GetPermissionsForCurrentUserAsync(Plant1IdWithAccess, CancellationToken.None);
        }

        [TestMethod]
        public async Task GetProjectNamesForUserAsync_ShouldReturnProjectsFromPermissionApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetProjectNamesForUserAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            AssertProjects(result);
            await _permissionApiServiceMock.GetAllOpenProjectsForCurrentUserAsync(Plant1IdWithAccess, CancellationToken.None);
        }

        [TestMethod]
        public async Task GetProjectNamesForUserAsync_ShouldReturnProjectsFromCacheSecondTime()
        {
            await _dut.GetProjectNamesForUserAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);
            // Act
            var result = await _dut.GetProjectNamesForUserAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            AssertProjects(result);
            // since GetProjectNamesForUserAsync has been called twice, but GetProjectsAsync has been called once, the second Get uses cache
            await _permissionApiServiceMock.Received(1).GetAllOpenProjectsForCurrentUserAsync(Plant1IdWithAccess, CancellationToken.None);
        }

        [TestMethod]
        public async Task GetRestrictionRolesForUserAsync_ShouldReturnPermissionsFromPermissionApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetRestrictionRolesForUserAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            AssertRestrictions(result);
            await _permissionApiServiceMock.Received(1).GetRestrictionRolesForCurrentUserAsync(Plant1IdWithAccess, CancellationToken.None);
        }

        [TestMethod]
        public async Task GetRestrictionRolesForUserAsync_ShouldReturnPermissionsFromCacheSecondTime()
        {
            await _dut.GetRestrictionRolesForUserAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);
            // Act
            var result = await _dut.GetRestrictionRolesForUserAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            AssertRestrictions(result);
            // since GetRestrictionRolesForUserAsync has been called twice, but GetRestrictionRolesAsync has been called once, the second Get uses cache
            await _permissionApiServiceMock.Received(1).GetRestrictionRolesForCurrentUserAsync(Plant1IdWithAccess, CancellationToken.None);
        }

        [TestMethod]
        public async Task GetPermissionsForUserAsync_ShouldThrowExceptionWhenOidIsEmpty()
            => await Assert.ThrowsExceptionAsync<Exception>(() => _dut.GetPermissionsForUserAsync(Plant1IdWithAccess, Guid.Empty, CancellationToken.None));

        [TestMethod]
        public async Task GetProjectNamesForUserAsync_ShouldThrowExceptionWhenOidIsEmpty()
            => await Assert.ThrowsExceptionAsync<Exception>(() => _dut.GetProjectNamesForUserAsync(Plant1IdWithAccess, Guid.Empty, CancellationToken.None));

        [TestMethod]
        public async Task GetRestrictionRolesForUserAsync_ShouldThrowExceptionWhenOidIsEmpty()
            => await Assert.ThrowsExceptionAsync<Exception>(() => _dut.GetRestrictionRolesForUserAsync(Plant1IdWithAccess, Guid.Empty, CancellationToken.None));

        [TestMethod]
        public async Task ClearAll_ShouldClearAllPermissionCaches()
        {
            // Arrange
            var cacheManagerMock = Substitute.For<ICacheManager>();
            var dut = new PermissionCache(
                cacheManagerMock,
                _currentUserProviderMock,
                _permissionApiServiceMock,
                Substitute.For<IOptionsMonitor<CacheOptions>>());
            
            // Act
            await dut.ClearAllAsync(Plant1IdWithAccess, _currentUserOid, CancellationToken.None);

            // Assert
            await cacheManagerMock.Received(4).RemoveAsync(Arg.Any<string>(), CancellationToken.None);
        }

        private void AssertPlants(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Plant1IdWithAccess, result.First());
            Assert.AreEqual(Plant2IdWithAccess, result.Last());
        }

        private void AssertPermissions(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Permission1, result.First());
            Assert.AreEqual(Permission2, result.Last());
        }

        private void AssertProjects(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Project1WithAccess, result.First());
            Assert.AreEqual(Project2WithAccess, result.Last());
        }

        private void AssertRestrictions(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Restriction1, result.First());
            Assert.AreEqual(Restriction2, result.Last());
        }
    }
}
