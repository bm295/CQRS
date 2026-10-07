using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace ACCORD;

internal static class DatabaseMigrator
{
    public static async Task ApplyAsync(IConfiguration configuration, ILogger logger, CancellationToken cancellationToken)
    {
        if (!configuration.GetValue("Database:AutoMigrate", true))
        {
            logger.LogWarning("Automatic database migrations are disabled.");
            return;
        }

        var connectionString = configuration.GetConnectionString("SupabaseDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Auto migration requires ConnectionStrings:SupabaseDatabase. Configure it using dotnet user-secrets; see README.");

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            async Task Execute(string sql)
            {
                await using var command = new NpgsqlCommand(sql, connection, transaction) { CommandTimeout = 60 };
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            // Transaction-scoped lock serializes startup migrations across app instances.
            await Execute("SET LOCAL lock_timeout = '30s'; SELECT pg_advisory_xact_lock(1094927183, 1);");
            await Execute("""
                CREATE SCHEMA IF NOT EXISTS accord_internal;
                REVOKE ALL ON SCHEMA accord_internal FROM PUBLIC, anon, authenticated;
                CREATE TABLE IF NOT EXISTS accord_internal.schema_migrations (
                    name text PRIMARY KEY,
                    checksum text NOT NULL,
                    applied_at timestamptz NOT NULL DEFAULT now()
                );
                """);

            var assembly = typeof(DatabaseMigrator).Assembly;
            var scripts = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith("ACCORD.Migrations.", StringComparison.Ordinal) && name.EndsWith(".sql", StringComparison.Ordinal))
                .Order(StringComparer.Ordinal).ToArray();
            if (scripts.Length == 0) throw new InvalidOperationException("No embedded database migrations were found.");
            foreach (var name in scripts)
            {
                await using var stream = assembly.GetManifestResourceStream(name)!;
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var sql = await reader.ReadToEndAsync(cancellationToken);
                var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
                await using var query = new NpgsqlCommand("SELECT checksum FROM accord_internal.schema_migrations WHERE name = @name", connection, transaction);
                query.Parameters.AddWithValue("name", name);
                var previousChecksum = await query.ExecuteScalarAsync(cancellationToken) as string;
                if (previousChecksum is not null)
                {
                    if (previousChecksum != checksum)
                        throw new InvalidOperationException($"Applied migration {name} was modified. Restore it and add a new migration instead.");
                    continue;
                }

                await Execute(sql);
                await using var insert = new NpgsqlCommand("INSERT INTO accord_internal.schema_migrations(name, checksum) VALUES (@name, @checksum)", connection, transaction);
                insert.Parameters.AddWithValue("name", name);
                insert.Parameters.AddWithValue("checksum", checksum);
                await insert.ExecuteNonQueryAsync(cancellationToken);
                logger.LogInformation("Applied database migration {Migration}", name);
            }
            await Execute("NOTIFY pgrst, 'reload schema';");
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Database migrations are up to date.");
        }
        catch (Exception exception) when (exception is NpgsqlException or ArgumentException)
        {
            var rootCause = exception.GetBaseException();
            logger.LogError("Migration connection failure: {Type}; cause: {Cause}; SQL state: {SqlState}",
                exception.GetType().Name, rootCause.GetType().Name, (exception as PostgresException)?.SqlState ?? "none");
            // Do not include connection strings or database diagnostics in startup logs.
            throw new InvalidOperationException("Database migration failed. Check the PostgreSQL connection string, network access and DDL permissions. Application startup was stopped.");
        }
    }
}
