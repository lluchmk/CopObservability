using Microsoft.Data.SqlClient;

namespace MigrationsRunner;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly string _connectionString;

    public Worker(ILogger<Worker> logger, IHostApplicationLifetime applicationLifetime, IConfiguration configuration)
    {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _connectionString = configuration.GetConnectionString("Database")!;
        _logger.LogInformation($"using conn: {_connectionString}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            foreach (var migrationsFile in Directory.GetFiles("scripts"))
            {
                _logger.ApplyingMigrationsScript(migrationsFile);

                var script = File.ReadAllText(migrationsFile);

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(stoppingToken);

                foreach (var scriptPart in script.Split("GO"))
                {
                    var cmd = new SqlCommand(scriptPart, conn);
                    await cmd.ExecuteNonQueryAsync(stoppingToken);
                }
            }

            _logger.Completed();
        }
        catch (Exception ex)
        {
            _logger.ErrorMigration(ex);
            Environment.Exit(1);
        }
            
        _applicationLifetime.StopApplication();
    }
}

public static partial class Logs
{
    [LoggerMessage(
            EventId = 01,
            Level = LogLevel.Information,
            Message = "Applying migrations script: {script}")]
    public static partial void ApplyingMigrationsScript(this ILogger logger, string script);

    [LoggerMessage(
            EventId = 02,
            Level = LogLevel.Information,
            Message = "Applying all the migrations")]
    public static partial void Completed(this ILogger logger);

    [LoggerMessage(
            EventId = 03,
            Level = LogLevel.Error,
            Message = "Error applying migration")]
    public static partial void ErrorMigration(this ILogger logger, Exception ex);
}