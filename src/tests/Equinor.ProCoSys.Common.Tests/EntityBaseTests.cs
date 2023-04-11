using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Common.Tests
{
    [TestClass]
    public class EntityBaseTests
    {
        private readonly byte[] ConvertedRowVersion = {0, 0, 0, 0, 0, 0, 0, 16};
        private TestableEntityBase _dut;
        private Mock<IPreSaveDomainEvent> _preSaveEvent;
        private Mock<IPostSaveDomainEvent> _postSaveEvent;
        private const string RowVersion = "AAAAAAAAABA=";

        [TestInitialize]
        public void SetUp()
        {
            // Arrange
            _dut = new TestableEntityBase();
            _preSaveEvent = new Mock<IPreSaveDomainEvent>();
            _postSaveEvent = new Mock<IPostSaveDomainEvent>();
        }

        [TestMethod]
        public void DomainEventsLists_Are_Empty_After_Construct()
        {
            Assert.IsNotNull(_dut.PreSaveDomainEvents);
            Assert.IsNotNull(_dut.PostSaveDomainEvents);
            Assert.AreEqual(0, _dut.PreSaveDomainEvents.Count);
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void AddPreSaveDomainEvent_Should_AddToPreList()
        {
            // Act
            _dut.AddPreSaveDomainEvent(_preSaveEvent.Object);

            Assert.IsTrue(_dut.PreSaveDomainEvents.Contains(_preSaveEvent.Object));
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void AddPostSaveDomainEvent_Should_AddToPostList()
        {
            // Act
            _dut.AddPostSaveDomainEvent(_postSaveEvent.Object);

            // Assert
            Assert.IsTrue(_dut.PostSaveDomainEvents.Contains(_postSaveEvent.Object));
            Assert.AreEqual(0, _dut.PreSaveDomainEvents.Count);
        }

        [TestMethod]
        public void RemovePreSaveDomainEvent_Should_RemoveFromPreList()
        {
            // Arrange
            _dut.AddPreSaveDomainEvent(_preSaveEvent.Object);
            
            // Act
            _dut.RemovePreSaveDomainEvent(_preSaveEvent.Object);

            // Assert
            Assert.AreEqual(0, _dut.PreSaveDomainEvents.Count);
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void RemovePostSaveDomainEvent_Should_RemoveFromPostList()
        {
            // Arrange
            _dut.AddPostSaveDomainEvent(_postSaveEvent.Object);
            
            // Act
            _dut.RemovePostSaveDomainEvent(_postSaveEvent.Object);

            // Assert
            Assert.AreEqual(0, _dut.PreSaveDomainEvents.Count);
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void ClearPreSaveDomainEvents_Should_ClearPreList()
        {
            // Arrange
            var preSaveEventMock1 = new Mock<IPreSaveDomainEvent>();
            _dut.AddPreSaveDomainEvent(preSaveEventMock1.Object);
            var postSaveEventMock1 = new Mock<IPostSaveDomainEvent>();
            _dut.AddPostSaveDomainEvent(postSaveEventMock1.Object);
            
            var preSaveEventMock2 = new Mock<IPreSaveDomainEvent>();
            _dut.AddPreSaveDomainEvent(preSaveEventMock2.Object);
            var postSaveEventMock2 = new Mock<IPostSaveDomainEvent>();
            _dut.AddPostSaveDomainEvent(postSaveEventMock2.Object);

            // Act
            _dut.ClearPreSaveDomainEvents();

            // Assert
            Assert.AreEqual(0, _dut.PreSaveDomainEvents.Count);
            Assert.AreEqual(2, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void ClearPostSaveDomainEvents_Should_ClearPostList()
        {
            // Arrange
            var preSaveEventMock1 = new Mock<IPreSaveDomainEvent>();
            _dut.AddPreSaveDomainEvent(preSaveEventMock1.Object);
            var postSaveEventMock1 = new Mock<IPostSaveDomainEvent>();
            _dut.AddPostSaveDomainEvent(postSaveEventMock1.Object);

            var preSaveEventMock2 = new Mock<IPreSaveDomainEvent>();
            _dut.AddPreSaveDomainEvent(preSaveEventMock2.Object);
            var postSaveEventMock2 = new Mock<IPostSaveDomainEvent>();
            _dut.AddPostSaveDomainEvent(postSaveEventMock2.Object);

            // Act
            _dut.ClearPostSaveDomainEvents();

            // Assert
            Assert.AreEqual(2, _dut.PreSaveDomainEvents.Count);
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void GetRowVersion_Should_ReturnLastSetRowVersion()
        {
            Assert.IsNotNull(_dut.RowVersion);
            _dut.SetRowVersion(RowVersion);
            Assert.IsTrue(_dut.RowVersion.SequenceEqual(ConvertedRowVersion));
        }
       
        private class TestableEntityBase : EntityBase
        {
            // The base class is abstract, therefor a sub class is needed to test it.
        }
    }
}
