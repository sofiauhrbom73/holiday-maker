using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace HolidayMaker.Tests;

public class SearchTests : DatabaseTest
{
    private static string Search(string start, string end, int guests = 1, string filters = "n\nn\n\n\n", int sort = 1) =>
        $"1\n1\n3\nMiami\n{start}\n{end}\n{guests}\n{filters}{sort}\n0\n0\n";

    [Theory]
    [InlineData("2026-12-10", "2026-12-13", false)]
    [InlineData("2026-12-09", "2026-12-17", false)]
    [InlineData("2026-12-11", "2026-12-12", false)]
    [InlineData("2026-12-16", "2026-12-17", true)]
    public async Task AvailabilityHandlesOverlapsAndCheckoutBoundary(string start, string end, bool available)
    {
        string output = await App.RunAsync(Search(start, end));
        if (available) Assert.Contains("Room ID 1:", output);
        else Assert.DoesNotContain("Room ID 1:", output);
        if (start == "2026-12-10") Assert.Contains("Room ID 2:", output);
    }

    [Fact]
    public async Task TooManyGuestsAndUnknownTownReturnNoRooms()
    {
        string output = await App.RunAsync(Search("2026-12-17", "2026-12-18", 5));
        Assert.Contains("No rooms match those dates and filters.", output);
        output = await App.RunAsync("1\n1\n3\nUnknownTown\n2026-12-17\n2026-12-18\n1\nn\nn\n\n\n1\n0\n");
        Assert.Contains("No rooms match those dates and filters.", output);
    }

    [Fact]
    public async Task CombinedFiltersRestrictResultsAndUnfilteredResultsIncludeOtherHotels()
    {
        string output = await App.RunAsync(Search("2026-12-17", "2026-12-18", filters: "y\ny\n200\n1200\n"));
        string[] lines = RoomLines(output);
        Assert.Equal(3, lines.Length);
        Assert.All(lines, line => Assert.Contains("Seaside Hotel, Miami", line));
        Assert.All(lines, line => Assert.Matches(@"room \d+; capacity \d+; \d+\.\d{2} per night", line));
        output = await App.RunAsync(Search("2026-12-17", "2026-12-18"));
        Assert.Contains("Palm Guesthouse, Miami", output);
    }

    [Theory]
    [InlineData("y\nn\n\n\n", true, false)]
    [InlineData("n\ny\n\n\n", true, true)]
    [InlineData("n\nn\n200\n\n", true, false)]
    [InlineData("n\nn\n\n500\n", false, true)]
    public async Task OptionalFiltersCanBeAppliedIndependently(string filters, bool seaside, bool palm)
    {
        string[] lines = RoomLines(await App.RunAsync(Search("2026-12-17", "2026-12-18", filters: filters)));
        Assert.Equal(seaside, lines.Any(line => line.Contains("Seaside Hotel", StringComparison.Ordinal)));
        Assert.Equal(palm, lines.Any(line => line.Contains("Palm Guesthouse", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task ResultsAreSortedByPriceOrRating(int sort)
    {
        string[] lines = RoomLines(await App.RunAsync(Search("2026-12-17", "2026-12-18", sort: sort)));
        Assert.True(lines.Length > 1);
        if (sort == 1)
        {
            decimal[] prices = lines.Select(line => decimal.Parse(
                Regex.Match(line, @"(\d+\.\d{2}) per night").Groups[1].Value, CultureInfo.InvariantCulture)).ToArray();
            Assert.Equal(prices.OrderBy(value => value), prices);
        }
        else
        {
            int[] ratings = lines.Select(line => int.Parse(Regex.Match(line, @"rating (\d)/5").Groups[1].Value)).ToArray();
            Assert.Equal(ratings.OrderByDescending(value => value), ratings);
        }
    }

    private static string[] RoomLines(string output) =>
        output.Split('\n').Where(line => line.StartsWith("Room ID ", StringComparison.Ordinal)).ToArray();
}
