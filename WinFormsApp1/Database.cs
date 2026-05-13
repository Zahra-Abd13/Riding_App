using System.Data;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1;

internal static class Database
{
    private const string DefaultConnectionString =
        "Data Source=ZAHRA;Initial Catalog=RidingAppDB;Integrated Security=True;TrustServerCertificate=True;";

    internal static string ConnectionString =>
        Environment.GetEnvironmentVariable("RIDING_APP_CONNECTION_STRING") ?? DefaultConnectionString;

    internal static async Task<DataTable> QueryAsync(string sql, params SqlParameter[] parameters)
    {
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddRange(parameters);

        DataTable table = new();
        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        table.Load(reader);
        return table;
    }

    internal static async Task<int> ExecuteAsync(string sql, params SqlParameter[] parameters)
    {
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddRange(parameters);

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    internal static SqlParameter Parameter(string name, SqlDbType type, object? value)
    {
        SqlParameter parameter = new(name, type) { Value = value ?? DBNull.Value };
        return parameter;
    }

    internal static SqlParameter Parameter(string name, SqlDbType type, int size, object? value)
    {
        SqlParameter parameter = new(name, type, size) { Value = value ?? DBNull.Value };
        return parameter;
    }
}
