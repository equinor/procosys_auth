using System.Linq;

namespace Equinor.ProCoSys.Common
{
    public interface IReadOnlyContext
    {
        IQueryable<TEntity> QuerySet<TEntity>() where TEntity : class;
    }
}
