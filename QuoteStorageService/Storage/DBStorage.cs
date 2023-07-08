using System.Data;
using Microsoft.Data.SqlClient;
using NLog;
using QuoteStorageService.Configuration;
using QuoteStorageService.Models;

namespace QuoteStorageService.Storage;

// ReSharper disable once InconsistentNaming
public class DBStorage : IStorage
{
    private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly DataBaseConfiguration _config;

    public DBStorage(DataBaseConfiguration config)
    {
        _config = config;
    }

    public async Task<Stream?> GetFile(QuoteProvider quoteProvider, DateTime date, CancellationToken cancellationToken)
    {
        await using var conn = new SqlConnection(_config.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        await using var command = conn.CreateCommand();
        command.Parameters.Add(new SqlParameter("type", (int)quoteProvider));
        command.Parameters.Add(new SqlParameter("date", date));
        command.CommandText = "GetSingleFileFromStore";
        command.CommandType = CommandType.StoredProcedure;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            Logger.Info("File not flound in DB");

        var contentObj = reader["Content"]; //TODO: подумать как вернуть Stream
        var stream = new MemoryStream((byte[])contentObj);
        return stream;
    }

    public async Task SaveFile(QuoteProvider quoteProvider, DateTime date, Stream stream, CancellationToken cancellationToken)
    {
        await using var conn = new SqlConnection(_config.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        await using var command = conn.CreateCommand();
        command.Parameters.Add(new SqlParameter("type", (int)quoteProvider));
        command.Parameters.Add(new SqlParameter("date", date));
        command.Parameters.Add(new SqlParameter("content", stream) { SqlDbType = SqlDbType.VarBinary });
        command.CommandText = "SaveFileToStore";
        command.CommandType = CommandType.StoredProcedure;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<FileDescription>> GetQuoteList(QuoteProvider? quoteProvider, DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken)
    {
        await using var conn = new SqlConnection(_config.ConnectionString);
        await conn.OpenAsync(cancellationToken);
        await using var command = conn.CreateCommand();
        command.Parameters.Add(new SqlParameter("type", (int)quoteProvider!));
        command.Parameters.Add(new SqlParameter("dateFrom", dateFrom));
        command.Parameters.Add(new SqlParameter("dateTo", dateTo));
        command.CommandText = "GetFileListFromStorage";
        command.CommandType = CommandType.StoredProcedure;

        var fileDescriptions = new List<FileDescription>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            fileDescriptions.Add(new()
            {
                QuoteProvider = (QuoteProvider)(int)reader["Type"],
                Date = (DateTime)reader["Date"]
            });
        }

        return fileDescriptions;
    }
}