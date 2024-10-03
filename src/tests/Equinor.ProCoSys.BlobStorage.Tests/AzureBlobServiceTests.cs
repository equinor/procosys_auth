using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.BlobStorage.Tests
{
    [TestClass]
    public class AzureBlobServiceTests
    {
        [TestMethod]
        public void Constructor_Should_Accept_BlobStorageAccountUrl()
        {
            // Arrange
            var optionsMock = Substitute.For<IOptionsMonitor<BlobStorageOptions>>();
            var accountName = "pcs";
            var accountKey = "pw";
            var endpoint = $"https://{accountName}.blob.core.windows.net";
            var options = new BlobStorageOptions
            {
                BlobStorageAccountUrl = endpoint,
                BlobStorageAccountName = accountName,
                BlobStorageAccountKey = accountKey,
            };
            optionsMock.CurrentValue.Returns(options);
            
            // Act
            var dut = new AzureBlobService(optionsMock);

            // Assert
            Assert.AreEqual(options.BlobStorageAccountUrl, dut.Endpoint);
            Assert.AreEqual($"{accountName}.blob.core.windows.net", dut.HostEndpoint);
            Assert.AreEqual(accountName, dut.AccountName);
            Assert.AreEqual(accountKey, dut.AccountKey);
        }
    }
}
