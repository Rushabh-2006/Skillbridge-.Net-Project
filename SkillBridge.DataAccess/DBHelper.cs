using System.Data;
using MySqlConnector;

namespace SkillBridge.DataAccess;

public class DBHelper
{
    private readonly string _connectionString;

    public DBHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, params MySqlParameter[] parameters)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        if (parameters.Length > 0)
        {
            cmd.Parameters.AddRange(parameters);
        }
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<object?> ExecuteScalarAsync(string sql, params MySqlParameter[] parameters)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        if (parameters.Length > 0)
        {
            cmd.Parameters.AddRange(parameters);
        }
        return await cmd.ExecuteScalarAsync();
    }

    public async Task<DataTable> ExecuteQueryAsync(string sql, params MySqlParameter[] parameters)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        if (parameters.Length > 0)
        {
            cmd.Parameters.AddRange(parameters);
        }
        using var reader = await cmd.ExecuteReaderAsync();
        var dt = new DataTable();
        dt.Load(reader);
        return dt;
    }
}
