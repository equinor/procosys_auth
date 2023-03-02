using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Auth.Tests
{
    [TestClass]
    public class AuthModuleTests
    {
        [TestMethod]
        public void AddPcsAuthIntegration_Should_Return_TheSameService()
        {
            // Arrange
            var serviceCollectionMock = new Mock<IServiceCollection>();

            // Act
            // ReSharper disable once InvokeAsExtensionMethod
            var result = AuthModule.AddPcsAuthIntegration(serviceCollectionMock.Object);

            // Assert
            Assert.AreEqual(serviceCollectionMock.Object, result);
        }
    }
}