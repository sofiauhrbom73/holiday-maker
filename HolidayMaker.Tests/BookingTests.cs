using Xunit;

namespace HolidayMaker.Tests;

public class BookingTests : DatabaseTest
{
    private const string SearchForRoom22 = "1\n1\n3\nMiami\n2026-12-10\n2026-12-13\n2\nn\nn\n\n\n1\n";

    [Fact]
    public async Task CreateBookingCalculatesTotalAndPersistsAcrossRestart()
    {
        string output = await App.RunAsync(SearchForRoom22 + "22\n0\n");
        Assert.Contains("Booking 26 confirmed. Total: 2250.00 (3 nights", output);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM booking WHERE customer_id = 1 AND room_id = 22 AND check_in = '2026-12-10' AND check_out = '2026-12-13' AND guests = 2;"));
        output = await App.RunAsync("1\n1\n4\n0\n");
        Assert.Contains("Booking 26: Palm Guesthouse, Miami, room 102; 2026-12-10 to 2026-12-13; 2 guests; total 2250.00", output);
    }

    [Fact]
    public async Task RoomNotInResultsCannotBeBooked()
    {
        string output = await App.RunAsync(SearchForRoom22 + "1\n0\n");
        Assert.Contains("That room was not in the search results.", output);
        Assert.Equal(25, await ScalarAsync("SELECT COUNT(*) FROM booking;"));
    }

    [Fact]
    public async Task BookingCanStartOnPreviousCheckoutDate()
    {
        string output = await App.RunAsync("1\n1\n3\nMiami\n2026-12-16\n2026-12-17\n1\nn\nn\n\n\n1\n1\n0\n");
        Assert.Contains("Booking 26 confirmed. Total: 700.00 (1 nights", output);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM booking WHERE room_id = 1 AND check_in = '2026-12-16' AND check_out = '2026-12-17';"));
    }

    [Fact]
    public async Task AvailabilityIsRecheckedWhenAnotherBookingAppears()
    {
        string output = await App.RunWithInterruptionAsync(SearchForRoom22, "22\n0\n",
            () => ExecuteAsync("INSERT INTO booking (customer_id, room_id, check_in, check_out, guests) VALUES (2, 22, '2026-12-09', '2026-12-17', 1);"));
        Assert.Contains("This room has just become unavailable", output);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM booking WHERE room_id = 22;"));
    }

    [Fact]
    public async Task CapacityIsRecheckedWhenRoomChangesAfterSearch()
    {
        string output = await App.RunWithInterruptionAsync(SearchForRoom22, "22\n0\n",
            () => ExecuteAsync("UPDATE room SET max_guests = 1 WHERE room_id = 22;"));
        Assert.Contains("The room holds at most 1 guests. No booking was created.", output);
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM booking WHERE room_id = 22;"));
    }

    [Fact]
    public async Task InvalidInputIsReportedAndJanuaryFirstCheckoutIsAccepted()
    {
        string input = "bad\n1\n1\n3\nMiami\nnot-a-date\n2026-12-10\n2026-12-10\n" +
                       "2026-11-30\n2026-12-01\n2026-12-31\n2027-01-02\n" +
                       "2026-12-31\n2027-01-01\n0\n1\nn\nn\n\n\n1\n22\n0\n";
        string output = await App.RunAsync(input);
        Assert.Contains("Enter a number from 0 to 6.", output);
        Assert.Contains("Enter a valid date in yyyy-MM-dd format.", output);
        Assert.Contains("Dates must be within December 2026", output);
        Assert.Contains("Enter a positive whole number.", output);
        Assert.Contains("Booking 26 confirmed. Total: 750.00 (1 nights", output);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM booking WHERE room_id = 22 AND check_out = '2027-01-01' AND guests = 1;"));
        Assert.Equal(26, await ScalarAsync("SELECT COUNT(*) FROM booking;"));
    }

    [Fact]
    public async Task GuestCountChangesPersistAndInvalidCountsDoNotModifyBooking()
    {
        string output = await App.RunAsync("1\n2\n5\n2\n1\n4\n5\n2\n3\n4\n0\n");
        Assert.Contains("Guest count updated.", output);
        Assert.Contains("The room holds at most 2 guests. The booking was not changed.", output);
        Assert.Contains("2026-12-01 to 2026-12-05; 1 guests; total 3800.00", output);
        Assert.Equal(1, await ScalarAsync("SELECT guests FROM booking WHERE booking_id = 2;"));
    }

    [Fact]
    public async Task InvalidGuestInputAndOtherCustomersBookingsAreRejected()
    {
        string output = await App.RunAsync("1\n1\n5\n2\n0\n1\n6\n2\ny\n0\n");
        Assert.Contains("Enter a positive whole number.", output);
        Assert.Contains("No booking with that number belongs to the selected customer.", output);
        Assert.Equal(2, await ScalarAsync("SELECT guests FROM booking WHERE booking_id = 2;"));
    }

    [Fact]
    public async Task CancellationRemovesBookingAndMakesRoomAvailable()
    {
        string output = await App.RunAsync("1\n1\n6\n1\ny\n4\n3\nMiami\n2026-12-10\n2026-12-13\n1\nn\nn\n\n\n1\n0\n0\n");
        Assert.Contains("Booking cancelled.", output);
        Assert.DoesNotContain("Booking 1: Seaside Hotel", output);
        Assert.Contains("Room ID 1:", output);
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM booking WHERE booking_id = 1;"));
    }

    [Fact]
    public async Task UnknownOrAbortedCancellationDoesNotChangeBookings()
    {
        string output = await App.RunAsync("1\n1\n6\n1\nn\n6\n999\ny\n0\n");
        Assert.Contains("Cancellation aborted.", output);
        Assert.Contains("No booking with that number belongs to the selected customer.", output);
        Assert.Equal(25, await ScalarAsync("SELECT COUNT(*) FROM booking;"));
    }
}
