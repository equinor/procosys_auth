using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.BlobStorage.Tests
{
    [TestClass]
    public class AzureBlobServiceTests
    {
        [TestMethod]
        public void Constructor_Should_Accept_ConnectionString()
        {
            // Arrange
            var optionsMock = new Mock<IOptionsMonitor<BlobStorageOptions>>();
            var accountName = "pcs";
            var accountKey = "pw";
            var endpoint = "core.windows.net";
            var options = new BlobStorageOptions
            {
                ConnectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix={endpoint}"
            };
            optionsMock.SetupGet(o => o.CurrentValue).Returns(options);
            
            // Act
            var dut = new AzureBlobService(optionsMock.Object);

            // Assert
            Assert.AreEqual(options.ConnectionString, dut.ConnectionString);
            Assert.AreEqual(accountName, dut.AccountName);
            Assert.AreEqual(accountKey, dut.AccountKey);
            Assert.AreEqual("blob." + endpoint, dut.Endpoint);
        }
    }
}
