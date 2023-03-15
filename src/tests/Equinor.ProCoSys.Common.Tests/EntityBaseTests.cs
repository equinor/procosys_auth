using System.Linq;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Common.Tests
{
    [TestClass]
    public class EntityBaseTests
    {
        private readonly byte[] ConvertedRowVersion = {0, 0, 0, 0, 0, 0, 0, 16};
        private TestableEntityBase _dut;
        private Mock<INotification> _domainEventMock;
        private const string RowVersion = "AAAAAAAAABA=";

        [TestInitialize]
        public void SetUp()
        {
            // Arrange
            _dut = new TestableEntityBase();
            _domainEventMock = new Mock<INotification>();
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
            _dut.AddPreSaveDomainEvent(_domainEventMock.Object);

            Assert.IsTrue(_dut.PreSaveDomainEvents.Contains(_domainEventMock.Object));
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void AddPostSaveDomainEvent_Should_AddToPostList()
        {
            // Act
            _dut.AddPostSaveDomainEvent(_domainEventMock.Object);

            // Assert
            Assert.IsTrue(_dut.PostSaveDomainEvents.Contains(_domainEventMock.Object));
            Assert.AreEqual(0, _dut.PreSaveDomainEvents.Count);
        }

        [TestMethod]
        public void RemovePreSaveDomainEvent_Should_RemoveFromPreList()
        {
            // Arrange
            _dut.AddPreSaveDomainEvent(_domainEventMock.Object);
            
            // Act
            _dut.RemovePreSaveDomainEvent(_domainEventMock.Object);

            // Assert
            Assert.AreEqual(0, _dut.PreSaveDomainEvents.Count);
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void RemovePostSaveDomainEvent_Should_RemoveFromPostList()
        {
            // Arrange
            _dut.AddPostSaveDomainEvent(_domainEventMock.Object);
            
            // Act
            _dut.RemovePostSaveDomainEvent(_domainEventMock.Object);

            // Assert
            Assert.AreEqual(0, _dut.PreSaveDomainEvents.Count);
            Assert.AreEqual(0, _dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void ClearPreSaveDomainEvents_Should_ClearPreList()
        {
            // Arrange
            var domainEventMock1 = new Mock<INotification>();
            _dut.AddPreSaveDomainEvent(domainEventMock1.Object);
            _dut.AddPostSaveDomainEvent(domainEventMock1.Object);
            
            var domainEventMock2 = new Mock<INotification>();
            _dut.AddPreSaveDomainEvent(domainEventMock2.Object);
            _dut.AddPostSaveDomainEvent(domainEventMock2.Object);

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
            var domainEventMock1 = new Mock<INotification>();
            _dut.AddPreSaveDomainEvent(domainEventMock1.Object);
            _dut.AddPostSaveDomainEvent(domainEventMock1.Object);

            var domainEventMock2 = new Mock<INotification>();
            _dut.AddPreSaveDomainEvent(domainEventMock2.Object);
            _dut.AddPostSaveDomainEvent(domainEventMock2.Object);

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
