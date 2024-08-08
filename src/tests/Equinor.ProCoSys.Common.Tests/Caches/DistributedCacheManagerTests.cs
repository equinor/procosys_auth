using Equinor.ProCoSys.Common.Caches;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
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
            _dut = new DistributedCacheManager(new MemoryDistributedCache(_options), Substitute.For<ILogger<DistributedCacheManager>>());
        }

        [TestMethod]
        public async Task GetOrCreateAsync_ShouldReturnCachedValue_OnFirstCall()
        {
            // Act
            var result = await _dut.GetOrCreateAsync("KeyA", token => CreateTestableObject("V1", "V2", token), CacheDuration.Minutes, 2, CancellationToken.None);

            // Assert
            AssertObject("V1", "V2", result);
        }

        [TestMethod]
        public async Task GetOrCreateAsync_ShouldReturnFirstCachedValue_OnSecondCall()
        {
            // Arrange
            await _dut.GetOrCreateAsync("KeyA", token => CreateTestableObject("V1", "V2", token), CacheDuration.Minutes, 2, CancellationToken.None);

            // Act
            var result = await _dut.GetOrCreateAsync("KeyA", token => CreateTestableObject("X", "Y", token), CacheDuration.Minutes, 2, CancellationToken.None);

            // Assert
            AssertObject("V1", "V2", result);
        }

        [TestMethod]
        public async Task GetOrCreateAsync_ShouldThrowException_WhenActionThrowException()
            // Act and Assert
            => await Assert.ThrowsExceptionAsync<Exception>(() 
                => _dut.GetOrCreateAsync("KeyA", token => ThrowTestableException("V1", "V2", token), CacheDuration.Minutes, 2, CancellationToken.None));

        [TestMethod]
        public async Task GetOrCreateAsync_ShouldReturnCachedValue_OnSecondCallAfterException()
        {
            // Arrange
            await Assert.ThrowsExceptionAsync<Exception>(()
                => _dut.GetOrCreateAsync("KeyA", token => ThrowTestableException("V1", "V2", token), CacheDuration.Minutes, 2, CancellationToken.None));

            // Act
            var result = await _dut.GetOrCreateAsync("KeyA", token => CreateTestableObject("X", "Y", token), CacheDuration.Minutes, 2, CancellationToken.None);

            // Assert
            AssertObject("X", "Y", result);
        }

        [TestMethod]
        public async Task GetOrCreateAsync_ShouldNotReturnCachedValue_WhenActionThrowException()
        {
            // Act
            await Assert.ThrowsExceptionAsync<Exception>(()
                => _dut.GetOrCreateAsync("KeyA", token => ThrowTestableException("V1", "V2", token), CacheDuration.Minutes, 2, CancellationToken.None));

            // Assert
            var result = await _dut.GetAsync<TestableClass>("KeyA", CancellationToken.None);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAsync_ShouldGetCachedValue()
        {
            // Arrange
            await _dut.GetOrCreateAsync("KeyA", token => CreateTestableObject("V1", "V2", token), CacheDuration.Minutes, 2, CancellationToken.None);

            // Act
            var result = await _dut.GetAsync<TestableClass>("KeyA", CancellationToken.None);

            // Assert
            AssertObject("V1", "V2", result);
        }

        [TestMethod]
        public async Task GetAsync_ShouldThrowException_WhenGettingIncompatibleObject()
        {
            // Arrange
            await _dut.GetOrCreateAsync("KeyA", token => CreateTestableObject("V1", "V2", token), CacheDuration.Minutes, 2, CancellationToken.None);

            // Act
            var result = await Assert.ThrowsExceptionAsync<Exception>(() => _dut.GetAsync<string>("KeyA", CancellationToken.None));
            Assert.AreEqual($"Failed to deserialize cached value for key KeyA as {typeof(string)}", result.Message);
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
            await _dut.GetOrCreateAsync("KeyA", token => CreateTestableObject("V1", "V2", token), CacheDuration.Minutes, 2, CancellationToken.None);
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

        private  async Task<TestableClass> CreateTestableObject(string v1, string v2, CancellationToken cancellationToken)
        {
            return await Task.Run(() => new TestableClass { Val1 = v1, Val2 = v2 }, cancellationToken);
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private Task<TestableClass> ThrowTestableException(string v1, string v2, CancellationToken cancellationToken)
        {
            throw new Exception("Test");
        }

        private class TestableClass
        {
            public string Val1 { get; init; }
            public string Val2 { get; init; }
        }
    }
}
