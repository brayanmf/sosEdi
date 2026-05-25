using System.Data;
using Microsoft.Data.SqlClient;

namespace SOS.Data
{
    public interface IConexion
    {
        Task<IDbConnection> CreateConnectionAsync();
    }

    public class SqlConexion : IConexion
    {
        private readonly string _connectionString;

        public SqlConexion(string? connectionString) => _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var sqlConnection = new SqlConnection(_connectionString);
            await sqlConnection.OpenAsync();
            return sqlConnection;
        }
    }
}
