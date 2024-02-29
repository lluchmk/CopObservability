using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;

namespace DocumentsGenerator;

public class DocumentsRepository
{
    private readonly string _connectionString;

    public DocumentsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Database")
            ?? throw new ArgumentNullException("The Database connection string has to be set up.");
    }

    public async Task UpdateDocumentStatus(Guid documentId, DocumentStatus status, string? downloadUrl = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE Documents 
            SET Status = @status, DownloadUrl = @downloadUrl
            WHERE Id = @documentId", connection);

        command.Parameters.AddWithValue("@status", status);
        command.Parameters.AddWithValue("@documentId", documentId);
        command.Parameters.AddWithValue("@downloadUrl", downloadUrl ?? SqlString.Null);

        await command.ExecuteNonQueryAsync();
    }
}
