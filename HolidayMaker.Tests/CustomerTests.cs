using System.Text.RegularExpressions;
using Xunit;

namespace HolidayMaker.Tests;

public class CustomerTests : DatabaseTest
{
    [Fact]
    public async Task RegistrationValidatesRequiredFieldsAndCustomerCanBeSelectedAfterRestart()
    {
        string output = await App.RunAsync(
            "2\n\nTest Customer\n\n12345\n\nalice@example.com\ntest-customer@example.com\n4\n0\n");
        Assert.Contains("This value is required.", output);
        Assert.Contains("That email is already registered.", output);
        Match registration = Regex.Match(output, @"Customer registered with ID (\d+) and selected\.");
        Assert.True(registration.Success, "Registration confirmation was not displayed.");
        Assert.Contains("This customer has no bookings.", output);

        string customerId = registration.Groups[1].Value;
        string restart = await App.RunAsync($"1\n{customerId}\n4\n0\n");
        Assert.Contains($"Selected customer: Test Customer (ID {customerId})", restart);
        Assert.Contains("This customer has no bookings.", restart);
    }

    [Fact]
    public async Task UnknownCustomerCannotMakeOrViewBookings()
    {
        string output = await App.RunAsync("1\n999\n3\n4\n0\n");
        Assert.Contains("No customer has that ID.", output);
        Assert.Contains("Select or register a customer before booking.", output);
        Assert.Contains("Select a customer first.", output);
        Assert.Equal(25, await ScalarAsync("SELECT COUNT(*) FROM booking;"));
    }

    [Fact]
    public async Task ExistingCustomerCanViewOnlyTheirBookingDetails()
    {
        string output = await App.RunAsync("1\n1\n4\n0\n");
        Assert.Contains("Booking 1: Seaside Hotel, Miami, room 101; 2026-12-10 to 2026-12-13; 1 guests; total 2100.00", output);
        Assert.DoesNotContain("Booking 2:", output);
    }
}
