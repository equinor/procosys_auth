using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.BlobStorage.Tests
{
    [TestClass]
    public class AzureBlobServiceTests
    {
        [TestMethod]
        public void Constructor_Should_Accept_ConnectionString()
        {
            // Arrange
            var optionsMock = Substitute.For<IOptionsMonitor<BlobStorageOptions>>();
            var accountName = "pcs";
            var accountKey = "pw";
            var endpoint = "core.windows.net";
            var options = new BlobStorageOptions
            {
                ConnectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix={endpoint}"
            };
            optionsMock.CurrentValue.Returns(options);
            
            // Act
            var dut = new AzureBlobService(optionsMock);

            // Assert
            Assert.AreEqual(options.ConnectionString, dut.ConnectionString);
            Assert.AreEqual(accountName, dut.AccountName);
            Assert.AreEqual(accountKey, dut.AccountKey);
            Assert.AreEqual("blob." + endpoint, dut.Endpoint);
        }
    }
}
