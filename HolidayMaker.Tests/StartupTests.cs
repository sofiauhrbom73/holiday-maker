using System.Diagnostics;
using Xunit;

namespace HolidayMaker.Tests;

public class StartupTests
{
    [Theory]
    [InlineData("", "Set the HOLIDAY_MAKER_CONNECTION_STRING environment variable first.")]
    [InlineData("unknown", "Unknown environment. Use dev, test, or prod.")]
    public async Task InvalidStartupConfigurationIsReported(string argument, string expected)
    {
        string directory = AppContext.BaseDirectory;
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (string arg in new[] { "exec", "--runtimeconfig",
                     Path.Combine(directory, "HolidayMaker.Tests.runtimeconfig.json"), "--depsfile",
                     Path.Combine(directory, "HolidayMaker.Tests.deps.json"), Path.Combine(directory, "Holiday-Maker.dll") })
        {
            start.ArgumentList.Add(arg);
        }
        if (argument.Length > 0) start.ArgumentList.Add(argument);
        start.Environment.Remove("HOLIDAY_MAKER_CONNECTION_STRING");
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start the application.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        Task<string> stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
            Assert.Equal(1, process.ExitCode);
            Assert.Contains(expected, await stdout);
            Assert.Equal("", await stderr);
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
}
