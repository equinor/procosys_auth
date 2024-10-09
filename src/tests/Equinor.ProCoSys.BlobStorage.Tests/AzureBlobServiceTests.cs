using Azure.Core;
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
            var tokenMock = Substitute.For<TokenCredential>();
            var accountName = "pcs";
            var endpoint = $"https://{accountName}.blob.core.windows.net";
            var options = new BlobStorageOptions
            {
                BlobStorageAccountUrl = endpoint,
                BlobStorageAccountName = accountName,
            };
            optionsMock.CurrentValue.Returns(options);
            
            // Act
            var dut = new AzureBlobService(optionsMock, tokenMock);

            // Assert
            Assert.AreEqual(options.BlobStorageAccountUrl, dut.Endpoint);
            Assert.AreEqual($"{accountName}.blob.core.windows.net", dut.HostEndpoint);
            Assert.AreEqual(accountName, dut.AccountName);
        }
    }
}
