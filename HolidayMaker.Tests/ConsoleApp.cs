using System.Diagnostics;
using System.Text;
using MySqlConnector;
using Xunit;

namespace HolidayMaker.Tests;

internal sealed class ConsoleApp
{
    private readonly string connectionString;

    public ConsoleApp(string connectionString) => this.connectionString = connectionString;

    public async Task<string> RunAsync(string input)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        string directory = AppContext.BaseDirectory;
        start.ArgumentList.Add("exec");
        start.ArgumentList.Add("--runtimeconfig");
        start.ArgumentList.Add(Path.Combine(directory, "HolidayMaker.Tests.runtimeconfig.json"));
        start.ArgumentList.Add("--depsfile");
        start.ArgumentList.Add(Path.Combine(directory, "HolidayMaker.Tests.deps.json"));
        start.ArgumentList.Add(Path.Combine(directory, "Holiday-Maker.dll"));
        start.Environment["HOLIDAY_MAKER_CONNECTION_STRING"] = connectionString;
        start.Environment["DOTNET_SYSTEM_GLOBALIZATION_INVARIANT"] = "1";

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start the application.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var output = new StringBuilder();
        Task<string> stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await process.StandardInput.WriteAsync(input.AsMemory(), timeout.Token);
            process.StandardInput.Close();
            await ReadRemainingOutputAsync(process.StandardOutput, output, timeout.Token);
            await process.WaitForExitAsync(timeout.Token);
            string errors = await stderr;
            Assert.True(process.ExitCode == 0, $"Application exited with code {process.ExitCode}.\n{output}\n{errors}");
            return output.ToString();
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
    }

    public async Task<string> RunWithInterruptionAsync(string beforePrompt, string afterPrompt, Func<Task> action)
    {
        // Feed the first part while reading until the room-selection prompt.
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        string directory = AppContext.BaseDirectory;
        foreach (string arg in new[] { "exec", "--runtimeconfig",
                     Path.Combine(directory, "HolidayMaker.Tests.runtimeconfig.json"), "--depsfile",
                     Path.Combine(directory, "HolidayMaker.Tests.deps.json"), Path.Combine(directory, "Holiday-Maker.dll") })
        {
            start.ArgumentList.Add(arg);
        }
        start.Environment["HOLIDAY_MAKER_CONNECTION_STRING"] = connectionString;
        start.Environment["DOTNET_SYSTEM_GLOBALIZATION_INVARIANT"] = "1";

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start the application.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        Task<string> stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        var output = new StringBuilder();
        try
        {
            await process.StandardInput.WriteAsync(beforePrompt.AsMemory(), timeout.Token);
            await process.StandardInput.FlushAsync(timeout.Token);
            const string prompt = "Enter a room ID to book, or 0 to return: ";
            var buffer = new char[1];
            while (!output.ToString().Contains(prompt, StringComparison.Ordinal))
            {
                if (await process.StandardOutput.ReadAsync(buffer, timeout.Token) == 0)
                {
                    throw new InvalidOperationException($"Application exited before room selection. Output: {output}");
                }
                output.Append(buffer[0]);
            }
            await action();
            await process.StandardInput.WriteAsync(afterPrompt.AsMemory(), timeout.Token);
            process.StandardInput.Close();
            await ReadRemainingOutputAsync(process.StandardOutput, output, timeout.Token);
            await process.WaitForExitAsync(timeout.Token);
            string errors = await stderr;
            Assert.True(process.ExitCode == 0, $"Application exited with code {process.ExitCode}.\n{output}\n{errors}");
            return output.ToString();
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
    }

    private static async Task ReadRemainingOutputAsync(StreamReader reader, StringBuilder output, CancellationToken token)
    {
        var buffer = new char[4096];
        int count;
        while ((count = await reader.ReadAsync(buffer, token)) > 0)
        {
            output.Append(buffer, 0, count);
            if (output.Length > 256_000)
            {
                throw new InvalidOperationException("Application output exceeded the test limit. Check the scripted input.");
            }
        }
    }
}

public abstract class DatabaseTest : IAsyncLifetime
{
    private string? databaseName;
    protected string ConnectionString { get; private set; } = "";
    private protected ConsoleApp App { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        string value = Environment.GetEnvironmentVariable("HOLIDAY_MAKER_TEST_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Set HOLIDAY_MAKER_TEST_CONNECTION_STRING to a MySQL test server account with CREATE and DROP DATABASE privileges.");
        var builder = new MySqlConnectionStringBuilder(value) { Database = "" };
        await using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        string name = "holiday_maker_test_" + Guid.NewGuid().ToString("N");
        await using var command = new MySqlCommand($"CREATE DATABASE `{name}`;", connection);
        await command.ExecuteNonQueryAsync();
        databaseName = name;
        builder.Database = name;
        ConnectionString = builder.ConnectionString;
        App = new ConsoleApp(ConnectionString);
        try
        {
            await App.RunAsync("0\n");
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        if (databaseName is null) return;
        var builder = new MySqlConnectionStringBuilder(ConnectionString) { Database = "" };
        await using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand($"DROP DATABASE `{databaseName}`;", connection);
        await command.ExecuteNonQueryAsync();
        databaseName = null;
    }

    protected async Task ExecuteAsync(string sql)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    protected async Task<long> ScalarAsync(string sql)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }
}
