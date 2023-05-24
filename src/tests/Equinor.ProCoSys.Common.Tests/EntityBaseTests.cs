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
        private DomainEvent _domainEvent;
        private Mock<IPostSaveDomainEvent> _postSaveEvent;
        private const string RowVersion = "AAAAAAAAABA=";

        [TestInitialize]
        public void SetUp()
        {
            // Arrange
            _dut = new TestableEntityBase();
            _domainEvent = new TestableDomainEvent("Test");
            _postSaveEvent = new Mock<IPostSaveDomainEvent>();
        }

        [TestMethod]
        public void DomainEventsLists_Are_Empty_After_Construct()
        {
            Assert.IsNotNull(_dut.DomainEvents);
            Assert.IsNotNull(_dut.PostSaveDomainEvents);
            Assert.AreEqual(0, _dut.DomainEvents.Count);
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void AddDomainEvent_Should_AddToDomainEvents()
        {
            // Act
            _dut.AddDomainEvent(_domainEvent);

            Assert.IsTrue(_dut.DomainEvents.Contains(_domainEvent));
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void AddPostSaveDomainEvent_Should_AddToPostSaveDomainEvents()
        {
            // Act
            _dut.AddPostSaveDomainEvent(_postSaveEvent.Object);

            // Assert
            Assert.IsTrue(_dut.PostSaveDomainEvents.Contains(_postSaveEvent.Object));
            Assert.AreEqual(0, _dut.DomainEvents.Count);
        }

        [TestMethod]
        public void RemoveDomainEvent_Should_RemoveFromDomainEvents()
        {
            // Arrange
            _dut.AddDomainEvent(_domainEvent);
            
            // Act
            _dut.RemoveDomainEvent(_domainEvent);

            // Assert
            Assert.AreEqual(0, _dut.DomainEvents.Count);
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void RemovePostSaveDomainEvent_Should_RemoveFromPostSaveDomainEvents()
        {
            // Arrange
            _dut.AddPostSaveDomainEvent(_postSaveEvent.Object);
            
            // Act
            _dut.RemovePostSaveDomainEvent(_postSaveEvent.Object);

            // Assert
            Assert.AreEqual(0, _dut.DomainEvents.Count);
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void ClearDomainEvents_Should_ClearDomainEvents()
        {
            // Arrange
            var preSaveEventMock1 = new Mock<DomainEvent>();
            _dut.AddDomainEvent(preSaveEventMock1.Object);
            var postSaveEventMock1 = new Mock<IPostSaveDomainEvent>();
            _dut.AddPostSaveDomainEvent(postSaveEventMock1.Object);
            
            var preSaveEventMock2 = new Mock<DomainEvent>();
            _dut.AddDomainEvent(preSaveEventMock2.Object);
            var postSaveEventMock2 = new Mock<IPostSaveDomainEvent>();
            _dut.AddPostSaveDomainEvent(postSaveEventMock2.Object);

            // Act
            _dut.ClearDomainEvents();

            // Assert
            Assert.AreEqual(0, _dut.DomainEvents.Count);
            Assert.AreEqual(2, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void ClearPostSaveDomainEvents_Should_ClearPostSaveDomainEvents()
        {
            // Arrange
            var preSaveEventMock1 = new Mock<DomainEvent>();
            _dut.AddDomainEvent(preSaveEventMock1.Object);
            var postSaveEventMock1 = new Mock<IPostSaveDomainEvent>();
            _dut.AddPostSaveDomainEvent(postSaveEventMock1.Object);

            var preSaveEventMock2 = new Mock<DomainEvent>();
            _dut.AddDomainEvent(preSaveEventMock2.Object);
            var postSaveEventMock2 = new Mock<IPostSaveDomainEvent>();
            _dut.AddPostSaveDomainEvent(postSaveEventMock2.Object);

            // Act
            _dut.ClearPostSaveDomainEvents();

            // Assert
            Assert.AreEqual(2, _dut.DomainEvents.Count);
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

    internal class TestableDomainEvent : DomainEvent
    {
        public TestableDomainEvent(string displayName) : base(displayName)
        {
        }
    }
}
