using MediatR;

namespace Equinor.ProCoSys.Common;

public class DomainEvent: INotification
{
    public DomainEvent(string displayName) => DisplayName = displayName;

    public string DisplayName { get; }
}