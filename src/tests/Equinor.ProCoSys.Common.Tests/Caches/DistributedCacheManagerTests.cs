using Equinor.ProCoSys.Common.Caches;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Common.Tests.Caches
{
    [TestClass]
    public class DistributedCacheManagerTests
    {
        private DistributedCacheManager _dut;

        [TestInitialize]
        public void Setup()
        {
            OptionsWrapper<MemoryDistributedCacheOptions> _options = new(new MemoryDistributedCacheOptions());
            _dut = new DistributedCacheManager(new MemoryDistributedCache(_options));
        }

        [TestMethod]
        public async Task GetOrCreateAsync_ShouldReturnCachedValue_OnFirstCall()
        {
            // Act
            var result = await _dut.GetOrCreateAsync("KeyA", () => CreateTestableObject("V1", "V2"), CacheDuration.Minutes, 2, CancellationToken.None);

            // Assert
            AssertObject("V1", "V2", result);
        }

        [TestMethod]
        public async Task GetOrCreateAsync_ShouldReturnFirstCachedValue_OnSecondCall()
        {
            // Arrange
            await _dut.GetOrCreateAsync("KeyA", () => CreateTestableObject("V1", "V2"), CacheDuration.Minutes, 2, CancellationToken.None);

            // Act
            var result = await _dut.GetOrCreateAsync("KeyA", () => CreateTestableObject("KeyA", "Y"), CacheDuration.Minutes, 2, CancellationToken.None);

            // Assert
            AssertObject("V1", "V2", result);
        }

        [TestMethod]
        public async Task GetAsync_ShouldReuseCachedValue()
        {
            // Arrange
            await _dut.GetOrCreateAsync("KeyA", () => CreateTestableObject("V1", "V2"), CacheDuration.Minutes, 2, CancellationToken.None);

            // Act
            var result = await _dut.GetAsync<TestableClass>("KeyA", CancellationToken.None);

            // Assert
            AssertObject("V1", "V2", result);
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnNull_WhenUnknownKey()
        {
            // Act
            var result = await _dut.GetAsync<TestableClass>("KeyA", CancellationToken.None);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RemoveAsync_ShouldRemoveKnownKey()
        {
            // Arrange
            await _dut.GetOrCreateAsync("KeyA", () => CreateTestableObject("V1", "V2"), CacheDuration.Minutes, 2, CancellationToken.None);
            var result = await _dut.GetAsync<TestableClass>("KeyA", CancellationToken.None);
            Assert.IsNotNull(result);

            // Act
            await _dut.RemoveAsync("KeyA", CancellationToken.None);

            // Assert
            result = await _dut.GetAsync<TestableClass>("KeyA", CancellationToken.None);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RemoveAsync_ShouldDoNothing_WhenRemoveUnknownKey()
            => await _dut.RemoveAsync("KeyA", CancellationToken.None);

        private static void AssertObject(string v1, string v2, TestableClass result)
        {
            Assert.AreEqual(v1, result.Val1);
            Assert.AreEqual(v2, result.Val2);
        }

        private  async Task<TestableClass> CreateTestableObject(string v1, string v2)
        {
            return await Task.Run(() => new TestableClass { Val1 = v1, Val2 = v2 });
        }

        private class TestableClass
        {
            public string Val1 { get; init; }
            public string Val2 { get; init; }
        }
    }
}
