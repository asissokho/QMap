using System.Collections.Generic;

namespace QMap
{
    public interface IDbContext
    {
        IPropertyCollection Execute(IDbConnectionProvider connectionProvider, string requestName, IDictionary<string, object> parameters);
        IEnumerable<T> Query<T>(IDbConnectionProvider connectionProvider, string requestName, IDictionary<string, object> parameters);
    }
}
