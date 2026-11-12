using System.Data;
using Microsoft.Data.SqlClient;

namespace HeelnetTests.Utils
{
    /// <summary>Directe toegang tot de database van de applicatie, voor testdata en controles.</summary>
    public class MsSqlDatabaseHelper
    {
        private readonly string connectionString;

        public MsSqlDatabaseHelper(string? connectionString = null)
        {
            this.connectionString = connectionString ?? TestConfig.ConnectionString
                ?? throw new InvalidOperationException(
                    "Geen databaseverbinding ingesteld. Vul SA_PASSWORD in de .env in of zet HEELNET_DB_CONNECTION.");
        }

        /// <summary>
        /// Voert een query uit. SELECT geeft een gevulde DataTable terug, andere queries een lege.
        /// Parameters als ("@id", 1), nooit waarden in de SQL-string plakken.
        /// </summary>
        public DataTable ExecuteQuery(string query, params (string Name, object? Value)[] parameters)
        {
            var table = new DataTable();
            using var connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                using var command = new SqlCommand(query, connection);
                foreach (var (name, value) in parameters)
                {
                    command.Parameters.AddWithValue(name, value ?? DBNull.Value);
                }

                if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    using var adapter = new SqlDataAdapter(command);
                    adapter.Fill(table);
                }
                else
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Databasefout bij query: {Query}", query);
                throw;
            }
            return table;
        }

        /// <summary>Eerste kolom van de eerste rij, of null.</summary>
        public object? ExecuteScalar(string query, params (string Name, object? Value)[] parameters)
        {
            var table = ExecuteQuery(query, parameters);
            return table.Rows.Count == 0 || table.Rows[0][0] is DBNull ? null : table.Rows[0][0];
        }
    }
}
