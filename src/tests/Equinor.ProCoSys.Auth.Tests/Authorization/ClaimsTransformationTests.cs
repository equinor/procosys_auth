using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Auth.Tests.Authorization
{
    [TestClass]
    public class ClaimsTransformationTests
    {
        private ClaimsTransformation _dut;
        private readonly Guid Oid = new("{0b627d64-8113-40e1-9394-60282fb6bb9f}");
        private ClaimsPrincipal _principalWithOid;
        private readonly string Plant1 = "Plant1";
        private readonly string Plant2 = "Plant2";
        private readonly string Permission1_Plant1 = "A";
        private readonly string Permission2_Plant1 = "B";
        private readonly string Permission1_Plant2 = "C";
        private readonly string ProjectName1_Plant1 = "Pro1";
        private readonly Guid ProjectGuid1_Plant1 = new("11111111-1111-1111-1111-111111111111");
        private readonly string ProjectName2_Plant1 = "Pro2";
        private readonly Guid ProjectGuid2_Plant1 = new("22222222-2222-2222-2222-222222222222");
        private readonly string ProjectName1_Plant2 = "Pro3";
        private readonly Guid ProjectGuid1_Plant2 = new("33333333-3333-3333-3333-333333333333");
        private readonly string Restriction1_Plant1 = "Res1";
        private readonly string Restriction2_Plant1 = "Res2";
        private readonly string Restriction1_Plant2 = "Res3";

        private ILocalPersonRepository _localPersonRepositoryMock;
        private IPersonCache _personCacheMock;
        private IPlantProvider _plantProviderMock;
        private MainApiAuthenticatorOptions _mainApiAuthenticatorOptions;

        [TestInitialize]
        public void Setup()
        {
            _localPersonRepositoryMock = Substitute.For<ILocalPersonRepository>();
            _personCacheMock = Substitute.For<IPersonCache>();

            var proCoSysPersonNotSuper = new ProCoSysPerson
            {
                Super = false,
                AzureOid = Oid.ToString()
            };
            _localPersonRepositoryMock.GetAsync(Oid).Returns(proCoSysPersonNotSuper);
            _personCacheMock.GetAsync(Oid).Returns(proCoSysPersonNotSuper);

            _plantProviderMock = Substitute.For<IPlantProvider>();
            _plantProviderMock.Plant.Returns(Plant1);

            var permissionCacheMock = Substitute.For<IPermissionCache>();
            permissionCacheMock.GetUserPlantPermissionDataAsync(Oid, Plant1)
                .Returns(new UserPlantPermissionData(Oid, Plant1,
                    [new() { HasAccess = true, Id = Plant1, Title = Plant1 }], [Permission1_Plant1, Permission2_Plant1],
                    [
                        new()
                        {
                            Name = ProjectName1_Plant1,
                            ProCoSysGuid = ProjectGuid1_Plant1
                        },
                        new()
                        {
                            Name = ProjectName2_Plant1,
                            ProCoSysGuid = ProjectGuid2_Plant1
                        }
                    ], [Restriction1_Plant1, Restriction2_Plant1]));

            permissionCacheMock.GetUserPlantPermissionDataAsync(Oid, Plant2)
                .Returns(new UserPlantPermissionData(Oid, Plant2,
                    [new() { HasAccess = true, Id = Plant2, Title = Plant2 }], [Permission1_Plant2],
                    [
                        new()
                        {
                            Name = ProjectName1_Plant2,
                            ProCoSysGuid = ProjectGuid1_Plant2
                        }
                    ], [Restriction1_Plant2]));
            // permissionCacheMock.HasUserAccessToPlantAsync(Plant1, Oid).Returns(true);
            // permissionCacheMock.HasUserAccessToPlantAsync(Plant2, Oid).Returns(true);
            // permissionCacheMock.GetPermissionsForUserAsync(Plant1, Oid)
            //     .Returns(new List<string> { Permission1_Plant1, Permission2_Plant1 });
            // permissionCacheMock.GetProjectsForUserAsync(Plant1, Oid)
            //     .Returns(new List<AccessableProject>
            //     {
            //         new()
            //         {
            //             Name = ProjectName1_Plant1,
            //             ProCoSysGuid = ProjectGuid1_Plant1
            //         },
            //         new()
            //         {
            //             Name = ProjectName2_Plant1,
            //             ProCoSysGuid = ProjectGuid2_Plant1
            //         }
            //     });
            // permissionCacheMock.GetRestrictionRolesForUserAsync(Plant1, Oid)
            //     .Returns(new List<string> { Restriction1_Plant1, Restriction2_Plant1 });
            //
            // permissionCacheMock.GetPermissionsForUserAsync(Plant2, Oid)
            //     .Returns(new List<string> { Permission1_Plant2 });
            // permissionCacheMock.GetProjectsForUserAsync(Plant2, Oid)
            //     .Returns(new List<AccessableProject>
            //     {
            //         new()
            //         {
            //             Name = ProjectName1_Plant2,
            //             ProCoSysGuid = ProjectGuid1_Plant2
            //         }
            //     });
            // permissionCacheMock.GetRestrictionRolesForUserAsync(Plant2, Oid)
            //     .Returns(new List<string> { Restriction1_Plant2 });

            var loggerMock = Substitute.For<ILogger<ClaimsTransformation>>();

            _principalWithOid = new ClaimsPrincipal();
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(ClaimsExtensions.Oid, Oid.ToString()));
            _principalWithOid.AddIdentity(claimsIdentity);

            var authenticatorOptionsMock = Substitute.For<IOptionsMonitor<MainApiAuthenticatorOptions>>();
            _mainApiAuthenticatorOptions = new MainApiAuthenticatorOptions
            {
                MainApiScope = ""
            };
            authenticatorOptionsMock.CurrentValue.Returns(_mainApiAuthenticatorOptions);

            _dut = new ClaimsTransformation(
                _localPersonRepositoryMock,
                _personCacheMock,
                _plantProviderMock,
                permissionCacheMock,
                loggerMock,
                authenticatorOptionsMock);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddRoleClaimsForSuper_WhenPersonNotSuper()
        {
            // Act
            var result = await _dut.TransformAsync(_principalWithOid);

            // Assert
            var roleClaims = GetRoleClaims(result.Claims);
            Assert.IsTrue(roleClaims.All(r => r.Value != ClaimsTransformation.Superuser));
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddRoleClaimsForSuper_WhenPersonIsSuper()
        {
            // Arrange 
            var proCoSysPersonSuper = new ProCoSysPerson
            {
                Super = true
            };
            _localPersonRepositoryMock.GetAsync(Oid).Returns(proCoSysPersonSuper);

            // Act
            var result = await _dut.TransformAsync(_principalWithOid);

            // Assert
            var roleClaims = GetRoleClaims(result.Claims);
            Assert.IsTrue(roleClaims.Any(r => r.Value == ClaimsTransformation.Superuser));
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddRoleClaimsForSuper_WhenPersonIsSuper_AndNoPlantGiven()
        {
            // Arrange 
            _plantProviderMock.Plant.Returns((string)null);
            var proCoSysPersonSuper = new ProCoSysPerson
            {
                Super = true
            };
            _localPersonRepositoryMock.GetAsync(Oid).Returns(proCoSysPersonSuper);

            // Act
            var result = await _dut.TransformAsync(_principalWithOid);

            // Assert
            var roleClaims = GetRoleClaims(result.Claims);
            Assert.AreEqual(1, roleClaims.Count);
            Assert.IsTrue(roleClaims.Any(r => r.Value == ClaimsTransformation.Superuser));
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddRoleClaimsForPermissions()
        {
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertRoleClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_Twice_ShouldNotDuplicateRoleClaimsForPermissions()
        {
            await _dut.TransformAsync(_principalWithOid);
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertRoleClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddUserDataClaimsForProjects()
        {
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertProjectClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddUserDataClaimsForProjects_WhenDisabled()
        {
            // Arrange
            _mainApiAuthenticatorOptions.DisableProjectUserDataClaims = true;

            // Act
            var result = await _dut.TransformAsync(_principalWithOid);

            // Assert
            var projectClaims = GetProjectClaims(result.Claims);
            Assert.AreEqual(0, projectClaims.Count);
        }

        [TestMethod]
        public async Task TransformAsync_Twice_ShouldNotDuplicateUserDataClaimsForProjects()
        {
            await _dut.TransformAsync(_principalWithOid);
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertProjectClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddUserDataClaimsForRestrictionRole()
        {
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertRestrictionRoleForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddUserDataClaimsForRestrictionRole_WhenDisabled()
        {
            // Arrange
            _mainApiAuthenticatorOptions.DisableRestrictionRoleUserDataClaims = true;

            // Act
            var result = await _dut.TransformAsync(_principalWithOid);

            var restrictionRoleClaims = GetRestrictionRoleClaims(result.Claims);
            Assert.AreEqual(0, restrictionRoleClaims.Count);
        }

        [TestMethod]
        public async Task TransformAsync_Twice_ShouldNotDuplicateUserDataClaimsForRestrictionRole()
        {
            await _dut.TransformAsync(_principalWithOid);
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertRestrictionRoleForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddAnyClaims_ForPrincipalWithoutOid()
        {
            var result = await _dut.TransformAsync(new ClaimsPrincipal());

            Assert.AreEqual(0, GetProjectClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRoleClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRestrictionRoleClaims(result.Claims).Count);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddAnyClaims_WhenPersonNotFoundInProCoSys()
        {
            _localPersonRepositoryMock.GetAsync(Oid).Returns((ProCoSysPerson)null);
            _personCacheMock.GetAsync(Oid).Returns((ProCoSysPerson)null);

            var result = await _dut.TransformAsync(_principalWithOid);

            Assert.AreEqual(0, GetProjectClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRoleClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRestrictionRoleClaims(result.Claims).Count);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddRoleClaimsForPermissions_WhenPersonFoundLocalButNotInCache()
        {
            _personCacheMock.GetAsync(Oid).Returns((ProCoSysPerson)null);

            var result = await _dut.TransformAsync(_principalWithOid);

            AssertRoleClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddRoleClaimsForPermissions_WhenPersonNotFoundLocalButInCache()
        {
            _localPersonRepositoryMock.GetAsync(Oid).Returns((ProCoSysPerson)null);

            var result = await _dut.TransformAsync(_principalWithOid);

            AssertRoleClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddAnyClaims_WhenPersonIsNotSuper_AndNoPlantGiven()
        {
            _plantProviderMock.Plant.Returns((string)null);

            var result = await _dut.TransformAsync(_principalWithOid);

            Assert.AreEqual(0, GetProjectClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRoleClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRestrictionRoleClaims(result.Claims).Count);
        }

        [TestMethod]
        public async Task TransformAsync_OnSecondPlant_ShouldClearAllClaimsForFirstPlant()
        {
            var result = await _dut.TransformAsync(_principalWithOid);
            AssertRoleClaimsForPlant1(result.Claims);
            AssertProjectClaimsForPlant1(result.Claims);
            AssertRestrictionRoleForPlant1(result.Claims);

            _plantProviderMock.Plant.Returns(Plant2);
            result = await _dut.TransformAsync(_principalWithOid);

            var claims = GetRoleClaims(result.Claims);
            Assert.AreEqual(1, claims.Count);
            Assert.IsNotNull(claims.SingleOrDefault(r => r.Value == Permission1_Plant2));

            claims = GetProjectClaims(result.Claims);
            Assert.AreEqual(2, claims.Count);
            Assert.IsNotNull(claims.SingleOrDefault(r =>
                r.Value == ClaimsTransformation.GetProjectClaimValue(ProjectName1_Plant2)));

            claims = GetRestrictionRoleClaims(result.Claims);
            Assert.AreEqual(1, claims.Count);
            Assert.IsNotNull(claims.SingleOrDefault(r =>
                r.Value == ClaimsTransformation.GetRestrictionRoleClaimValue(Restriction1_Plant2)));
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNAddExistsClaimsFoPerson_WhenPersonExists()
        {
            var result = await _dut.TransformAsync(_principalWithOid);

            Assert.IsTrue(result.Claims.PersonExistsLocally(Oid.ToString()));
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddExistsClaimsFoPerson_WhenPersonNotExists()
        {
            _localPersonRepositoryMock.GetAsync(Oid).Returns((ProCoSysPerson)null);
            _personCacheMock.GetAsync(Oid).Returns((ProCoSysPerson)null);

            var result = await _dut.TransformAsync(_principalWithOid);

            Assert.IsFalse(result.Claims.PersonExistsLocally(Oid.ToString()));
        }

        private void AssertRoleClaimsForPlant1(IEnumerable<Claim> claims)
        {
            var roleClaims = GetRoleClaims(claims);
            Assert.AreEqual(2, roleClaims.Count);
            Assert.IsTrue(roleClaims.Any(r => r.Value == Permission1_Plant1));
            Assert.IsTrue(roleClaims.Any(r => r.Value == Permission2_Plant1));
        }

        private void AssertProjectClaimsForPlant1(IEnumerable<Claim> claims)
        {
            var projectClaims = GetProjectClaims(claims);
            Assert.AreEqual(4, projectClaims.Count);
            Assert.IsTrue(projectClaims.Any(r =>
                r.Value == ClaimsTransformation.GetProjectClaimValue(ProjectName1_Plant1)));
            Assert.IsTrue(projectClaims.Any(r =>
                r.Value == ClaimsTransformation.GetProjectClaimValue(ProjectGuid1_Plant1)));
            Assert.IsTrue(projectClaims.Any(r =>
                r.Value == ClaimsTransformation.GetProjectClaimValue(ProjectName2_Plant1)));
            Assert.IsTrue(projectClaims.Any(r =>
                r.Value == ClaimsTransformation.GetProjectClaimValue(ProjectGuid2_Plant1)));
        }

        private void AssertRestrictionRoleForPlant1(IEnumerable<Claim> claims)
        {
            var restrictionRoleClaims = GetRestrictionRoleClaims(claims);
            Assert.AreEqual(2, restrictionRoleClaims.Count);
            Assert.IsTrue(restrictionRoleClaims.Any(r =>
                r.Value == ClaimsTransformation.GetRestrictionRoleClaimValue(Restriction1_Plant1)));
            Assert.IsTrue(restrictionRoleClaims.Any(r =>
                r.Value == ClaimsTransformation.GetRestrictionRoleClaimValue(Restriction2_Plant1)));
        }

        private static List<Claim> GetRestrictionRoleClaims(IEnumerable<Claim> claims)
            => claims
                .Where(c => c.Type == ClaimTypes.UserData &&
                            c.Value.StartsWith(ClaimsTransformation.RestrictionRolePrefix))
                .ToList();

        private static List<Claim> GetRoleClaims(IEnumerable<Claim> claims)
            => claims.Where(c => c.Type == ClaimTypes.Role).ToList();

        private static List<Claim> GetProjectClaims(IEnumerable<Claim> claims)
            => claims.Where(
                    c => c.Type == ClaimTypes.UserData && c.Value.StartsWith(ClaimsTransformation.ProjectPrefix))
                .ToList();
    }
}