using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Common
{
    public interface IEventDispatcher
    {
        Task DispatchDomainEventsAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default);
        Task DispatchPostSaveEventsAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default);
        Task DispatchPostCommitEventsAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default);
    }
}
