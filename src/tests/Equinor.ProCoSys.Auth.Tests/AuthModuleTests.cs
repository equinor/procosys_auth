using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Auth.Tests
{
    [TestClass]
    public class AuthModuleTests
    {
        [TestMethod]
        public void AddPcsAuthIntegration_Should_Return_TheSameService()
        {
            // Arrange
            var serviceCollectionMock = Substitute.For<IServiceCollection>();

            // Act
            // ReSharper disable once InvokeAsExtensionMethod
            var result = AuthModule.AddPcsAuthIntegration(serviceCollectionMock);

            // Assert
            Assert.AreEqual(serviceCollectionMock, result);
        }
    }
}