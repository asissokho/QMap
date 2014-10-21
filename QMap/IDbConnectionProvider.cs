using System.Data;

namespace QMap
{
    public interface IDbConnectionProvider
    {
        IDbConnection GetConnection(string connectionName);
    }
}
