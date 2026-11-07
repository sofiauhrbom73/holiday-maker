using System.Security.Cryptography;
using System.Text;
using MySqlConnector;

internal static class DatabaseInitializer
{
    private static readonly string[] RequiredTables =
        ["accommodation", "facility", "customer", "room", "booking"];

    public static async Task InitializeAsync(MySqlConnection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.Database))
        {
            throw new InvalidOperationException("Select a database in the connection string before starting.");
        }

        // MySQL named locks serialize first-run setup across app instances.
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(connection.Database)));
        string lockName = "holiday:" + hash[..48];
        await using var acquire = new MySqlCommand("SELECT GET_LOCK(@name, 30);", connection);
        acquire.Parameters.AddWithValue("@name", lockName);
        object? acquired = await acquire.ExecuteScalarAsync();
        if (acquired is null or DBNull || Convert.ToInt32(acquired) != 1)
        {
            throw new InvalidOperationException("Could not acquire the database setup lock.");
        }

        try
        {
            await InitializeLockedAsync(connection);
        }
        finally
        {
            await using var release = new MySqlCommand("SELECT RELEASE_LOCK(@name);", connection);
            release.Parameters.AddWithValue("@name", lockName);
            await release.ExecuteScalarAsync();
        }
    }

    private static async Task InitializeLockedAsync(MySqlConnection connection)
    {
        var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var command = new MySqlCommand(
            "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE();",
            connection))
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                existingTables.Add(reader.GetString(0));
            }
        }

        int present = RequiredTables.Count(existingTables.Contains);
        if (present == 0 && existingTables.Count == 0)
        {
            Console.WriteLine("Creating the Holiday Maker tables...");
            await ExecuteResourceAsync(connection, "HolidayMaker.Schema.sql");
        }
        else if (present != RequiredTables.Length)
        {
            throw new InvalidOperationException(
                "The database is not empty or its Holiday Maker schema is incomplete. " +
                "No data was changed; inspect it in DBeaver before continuing.");
        }

        // Older versions of the schema did not have this marker table.
        await using (var command = new MySqlCommand(
            """
            CREATE TABLE IF NOT EXISTS holiday_maker_setup (
                setup_id TINYINT NOT NULL PRIMARY KEY,
                setup_source VARCHAR(20) NOT NULL,
                CONSTRAINT chk_setup_id CHECK (setup_id = 1)
            ) ENGINE = InnoDB;
            """, connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        await using (var command = new MySqlCommand(
            "SELECT COUNT(*) FROM holiday_maker_setup WHERE setup_id = 1;", connection))
        {
            if (Convert.ToInt64(await command.ExecuteScalarAsync()) != 0)
            {
                return;
            }
        }

        await using var count = new MySqlCommand(
            """
            SELECT (SELECT COUNT(*) FROM accommodation) +
                   (SELECT COUNT(*) FROM facility) +
                   (SELECT COUNT(*) FROM room) +
                   (SELECT COUNT(*) FROM customer) +
                   (SELECT COUNT(*) FROM booking);
            """, connection);
        if (Convert.ToInt64(await count.ExecuteScalarAsync()) != 0)
        {
            // Existing data is authoritative; never insert fixed sample IDs into it.
            await using var markExisting = new MySqlCommand(
                "INSERT INTO holiday_maker_setup (setup_id, setup_source) VALUES (1, 'existing');",
                connection);
            await markExisting.ExecuteNonQueryAsync();
            Console.WriteLine("Existing data found. Sample data was not inserted or changed.");
            return;
        }

        Console.WriteLine("Adding first-run sample data...");
        try
        {
            // Data.sql includes one transaction for the marker and all sample rows.
            await ExecuteResourceAsync(connection, "HolidayMaker.Data.sql");
        }
        catch (MySqlException)
        {
            await using var rollback = new MySqlCommand("ROLLBACK;", connection);
            await rollback.ExecuteNonQueryAsync();
            throw;
        }
        Console.WriteLine("Sample data added.");
    }

    private static async Task ExecuteResourceAsync(MySqlConnection connection, string name)
    {
        await using Stream stream = typeof(DatabaseInitializer).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded setup script {name} was not found.");
        using var reader = new StreamReader(stream);
        string sql = await reader.ReadToEndAsync();
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
