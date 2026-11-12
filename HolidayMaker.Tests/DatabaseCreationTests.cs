using MySqlConnector;
using Xunit;

namespace HolidayMaker.Tests;

public class DatabaseCreationTests : DatabaseTest
{
    [Fact]
    public async Task MissingDatabaseIsCreatedAndSeededWithoutResettingOnRestart()
    {
        string name = "holiday_maker_startup_" + Guid.NewGuid().ToString("N");
        var serverBuilder = new MySqlConnectionStringBuilder(ConnectionString) { Database = "" };
        await using var server = new MySqlConnection(serverBuilder.ConnectionString);
        await server.OpenAsync();
        await using var check = new MySqlCommand(
            "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @name;", server);
        check.Parameters.AddWithValue("@name", name);
        Assert.Equal(0L, Convert.ToInt64(await check.ExecuteScalarAsync()));

        var builder = new MySqlConnectionStringBuilder(ConnectionString) { Database = name };
        try
        {
            var app = new ConsoleApp(builder.ConnectionString);
            string firstRun = await app.RunAsync("0\n");
            Assert.Contains($"Database {name} is ready.", firstRun);
            Assert.Contains("Sample data added.", firstRun);

            await using var connection = new MySqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
            await using var count = new MySqlCommand("SELECT COUNT(*) FROM booking;", connection);
            Assert.Equal(25L, Convert.ToInt64(await count.ExecuteScalarAsync()));

            await using var delete = new MySqlCommand("DELETE FROM booking WHERE booking_id = 1;", connection);
            await delete.ExecuteNonQueryAsync();
            string restart = await app.RunAsync("0\n");
            Assert.DoesNotContain("Sample data added.", restart);
            Assert.Equal(24L, Convert.ToInt64(await count.ExecuteScalarAsync()));
        }
        finally
        {
            await using var drop = new MySqlCommand($"DROP DATABASE IF EXISTS `{name}`;", server);
            await drop.ExecuteNonQueryAsync();
        }
    }
}
