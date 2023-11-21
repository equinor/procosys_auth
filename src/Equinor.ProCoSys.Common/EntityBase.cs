using System;
using System.Collections.Generic;
using MediatR;

namespace Equinor.ProCoSys.Common
{
    /// <summary>
    /// Base class for all entities
    /// </summary>
    public abstract class EntityBase
    {
        private List<INotification> _domainEvents;
        private List<INotification> _postSaveDomainEvents;

        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly() ?? (_domainEvents = new List<INotification>()).AsReadOnly();
        public IReadOnlyCollection<INotification> PostSaveDomainEvents => _postSaveDomainEvents?.AsReadOnly() ?? (_postSaveDomainEvents = new List<INotification>()).AsReadOnly();

        public virtual int Id { get; protected set; }

        public readonly byte[] RowVersion = new byte[8];

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents ??= new List<INotification>();
            _domainEvents.Add(domainEvent);
        }

        public void AddPostSaveDomainEvent(IPostSaveDomainEvent domainEvent)
        {
            _postSaveDomainEvents ??= new List<INotification>();
            _postSaveDomainEvents.Add(domainEvent);
        }

        public virtual void SetRowVersion(string rowVersion)
        {
            if (string.IsNullOrEmpty(rowVersion))
            {
                throw new ArgumentNullException(nameof(rowVersion));
            }
            var newRowVersion = Convert.FromBase64String(rowVersion);
            for (var index = 0; index < newRowVersion.Length; index++)
            {
                RowVersion[index] = newRowVersion[index];
            }
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent) =>
            _domainEvents?.Remove(domainEvent);

        public void ClearDomainEvents() => _domainEvents.Clear();

        public void RemovePostSaveDomainEvent(IPostSaveDomainEvent domainEvent) =>
            _postSaveDomainEvents?.Remove(domainEvent);

        public void ClearPostSaveDomainEvents() => _postSaveDomainEvents.Clear();
    }
}
