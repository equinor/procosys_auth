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
        private List<INotification> _preSaveDomainEvents;
        private List<INotification> _postSaveDomainEvents;

        public IReadOnlyCollection<INotification> PreSaveDomainEvents => _preSaveDomainEvents?.AsReadOnly() ?? (_preSaveDomainEvents = new List<INotification>()).AsReadOnly();
        public IReadOnlyCollection<INotification> PostSaveDomainEvents => _postSaveDomainEvents?.AsReadOnly() ?? (_postSaveDomainEvents = new List<INotification>()).AsReadOnly();

        public virtual int Id { get; protected set; }

        public readonly byte[] RowVersion = new byte[8];

        public void AddPreSaveDomainEvent(IPreSaveDomainEvent eventItem)
        {
            _preSaveDomainEvents ??= new List<INotification>();
            _preSaveDomainEvents.Add(eventItem);
        }

        public void AddPostSaveDomainEvent(IPostSaveDomainEvent eventItem)
        {
            _postSaveDomainEvents ??= new List<INotification>();
            _postSaveDomainEvents.Add(eventItem);
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

        public void RemovePreSaveDomainEvent(IPreSaveDomainEvent eventItem) => _preSaveDomainEvents?.Remove(eventItem);

        public void ClearPreSaveDomainEvents() => _preSaveDomainEvents.Clear();

        public void RemovePostSaveDomainEvent(IPostSaveDomainEvent eventItem) => _postSaveDomainEvents?.Remove(eventItem);

        public void ClearPostSaveDomainEvents() => _postSaveDomainEvents.Clear();
    }
}
