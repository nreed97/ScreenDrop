using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using ScreenDrop.Models;

namespace ScreenDrop.Services;

public class DatabaseService
{
    private readonly string _connectionString;
    private readonly string _dbPath;

    public DatabaseService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ScreenDrop");

        Directory.CreateDirectory(appDataPath);

        _dbPath = Path.Combine(appDataPath, "uploads.db");
        _connectionString = $"Data Source={_dbPath}";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS uploads (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FileName TEXT NOT NULL,
                Url TEXT NOT NULL,
                S3Key TEXT NOT NULL,
                Folder TEXT NOT NULL,
                FileSize INTEGER NOT NULL,
                ContentType TEXT NOT NULL,
                UploadDate TEXT NOT NULL,
                UploadType TEXT NOT NULL,
                ThumbnailData BLOB
            )";

        connection.Execute(createTableSql);

        // Create index on UploadDate for faster queries
        connection.Execute("CREATE INDEX IF NOT EXISTS idx_upload_date ON uploads(UploadDate DESC)");
    }

    public async Task<int> AddUploadRecordAsync(UploadRecord record)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO uploads (FileName, Url, S3Key, Folder, FileSize, ContentType, UploadDate, UploadType, ThumbnailData)
            VALUES (@FileName, @Url, @S3Key, @Folder, @FileSize, @ContentType, @UploadDate, @UploadType, @ThumbnailData);
            SELECT last_insert_rowid();";

        var id = await connection.ExecuteScalarAsync<int>(sql, record);
        return id;
    }

    public async Task<List<UploadRecord>> GetRecentUploadsAsync(int daysBack = 30)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string sql;
        IEnumerable<UploadRecord> records;

        if (daysBack == 0)
        {
            // Keep all history forever
            sql = "SELECT * FROM uploads ORDER BY UploadDate DESC";
            records = await connection.QueryAsync<UploadRecord>(sql);
        }
        else
        {
            // Filter by date
            var cutoffDate = DateTime.Now.AddDays(-daysBack).ToString("o");
            sql = @"
                SELECT * FROM uploads 
                WHERE UploadDate >= @cutoffDate 
                ORDER BY UploadDate DESC";
            records = await connection.QueryAsync<UploadRecord>(sql, new { cutoffDate });
        }

        return records.ToList();
    }

    public async Task<List<UploadRecord>> SearchUploadsAsync(string searchTerm)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT * FROM uploads 
            WHERE FileName LIKE @searchTerm 
               OR Url LIKE @searchTerm
               OR UploadType LIKE @searchTerm
            ORDER BY UploadDate DESC";

        var searchPattern = $"%{searchTerm}%";
        var records = await connection.QueryAsync<UploadRecord>(sql, new { searchTerm = searchPattern });
        return records.ToList();
    }

    public async Task<bool> DeleteUploadRecordAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "DELETE FROM uploads WHERE Id = @id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { id });
        return rowsAffected > 0;
    }

    public async Task<int> DeleteOldUploadsAsync(int daysBack = 30)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cutoffDate = DateTime.Now.AddDays(-daysBack).ToString("o");

        var sql = "DELETE FROM uploads WHERE UploadDate < @cutoffDate";
        var rowsAffected = await connection.ExecuteAsync(sql, new { cutoffDate });
        return rowsAffected;
    }

    public async Task<int> ClearAllHistoryAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "DELETE FROM uploads";
        var rowsAffected = await connection.ExecuteAsync(sql);
        return rowsAffected;
    }

    public async Task<UploadRecord?> GetUploadByIdAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT * FROM uploads WHERE Id = @id";
        var record = await connection.QueryFirstOrDefaultAsync<UploadRecord>(sql, new { id });
        return record;
    }
}
