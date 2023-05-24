using MediatR;

namespace Equinor.ProCoSys.Common;

public abstract class DomainEvent: INotification
{
    public DomainEvent(string displayName) => DisplayName = displayName;

    public string DisplayName { get; }
}