using System.Data;
using System.Globalization;
using MySqlConnector;

internal static class Program
{
    private static readonly DateOnly FirstBookableDate = new(2026, 12, 1);
    private static readonly DateOnly LastCheckoutDate = new(2027, 1, 1);

    private static async Task<int> Main(string[] args)
    {
        string? database = null;
        if (args.Length > 0)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: dotnet run -- [dev|test|prod]");
                return 1;
            }

            database = args[0].ToLowerInvariant() switch
            {
                "dev" => "holiday_maker_dev",
                "test" => "holiday_maker_test",
                "prod" => "holiday_maker1",
                _ => null
            };
            if (database is null)
            {
                Console.WriteLine("Unknown environment. Use dev, test, or prod.");
                return 1;
            }
        }

        string? connectionString = Environment.GetEnvironmentVariable("HOLIDAY_MAKER_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("Set the HOLIDAY_MAKER_CONNECTION_STRING environment variable first.");
            return 1;
        }

        MySqlConnectionStringBuilder builder;
        try
        {
            builder = new MySqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Invalid connection string. Check the format of HOLIDAY_MAKER_CONNECTION_STRING.");
            return 1;
        }

        if (database is not null)
        {
            builder.Database = database;
        }

        if (string.IsNullOrWhiteSpace(builder.Database))
        {
            Console.WriteLine("Specify a database in the connection string or select dev, test, or prod.");
            return 1;
        }

        try
        {
            Console.WriteLine($"Connecting to database: {builder.Database}");
            await using var connection = new MySqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
            Console.WriteLine("Connected to the Holiday Maker database.");
            await DatabaseInitializer.InitializeAsync(connection);
            await RunMenuAsync(connection);
            return 0;
        }
        catch (MySqlException exception)
        {
            // Show connection/database errors instead of hiding why startup failed.
            Console.WriteLine($"Database error: {exception.Message}");
            return 1;
        }
        catch (InvalidOperationException exception)
        {
            Console.WriteLine($"Setup error: {exception.Message}");
            return 1;
        }
    }

    private static async Task RunMenuAsync(MySqlConnection connection)
    {
        int? selectedCustomerId = null;
        string? selectedCustomerName = null;

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Holiday Maker ===");
            Console.WriteLine($"Database: {connection.Database}");
            Console.WriteLine(selectedCustomerId.HasValue
                ? $"Selected customer: {selectedCustomerName} (ID {selectedCustomerId})"
                : "No customer selected");
            Console.WriteLine("1. Select an existing customer");
            Console.WriteLine("2. Register a customer");
            Console.WriteLine("3. Search available rooms and book");
            Console.WriteLine("4. View selected customer's bookings");
            Console.WriteLine("5. Change guests in a booking");
            Console.WriteLine("6. Cancel a booking");
            Console.WriteLine("0. Exit");

            switch (ReadMenuChoice())
            {
                case 1:
                    (selectedCustomerId, selectedCustomerName) =
                        await SelectCustomerAsync(connection);
                    break;
                case 2:
                    (selectedCustomerId, selectedCustomerName) =
                        await RegisterCustomerAsync(connection);
                    break;
                case 3:
                    if (!selectedCustomerId.HasValue)
                    {
                        Console.WriteLine("Select or register a customer before booking.");
                        break;
                    }

                    try
                    {
                        await SearchAndBookAsync(connection, selectedCustomerId.Value);
                    }
                    catch (InvalidOperationException exception)
                    {
                        Console.WriteLine(exception.Message);
                    }

                    break;
                case 4:
                    if (selectedCustomerId.HasValue)
                    {
                        await ShowBookingsAsync(connection, selectedCustomerId.Value);
                    }
                    else
                    {
                        Console.WriteLine("Select a customer first.");
                    }

                    break;
                case 5:
                    if (selectedCustomerId.HasValue)
                    {
                        await ChangeBookingGuestsAsync(connection, selectedCustomerId.Value);
                    }
                    else
                    {
                        Console.WriteLine("Select a customer first.");
                    }

                    break;
                case 6:
                    if (selectedCustomerId.HasValue)
                    {
                        await CancelBookingAsync(connection, selectedCustomerId.Value);
                    }
                    else
                    {
                        Console.WriteLine("Select a customer first.");
                    }

                    break;
                case 0:
                    return;
            }
        }
    }

    private static int ReadMenuChoice()
    {
        while (true)
        {
            Console.Write("Choose an option: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice is >= 0 and <= 6)
            {
                return choice;
            }

            Console.WriteLine("Enter a number from 0 to 6.");
        }
    }

    private static async Task<(int? Id, string? Name)> SelectCustomerAsync(MySqlConnection connection)
    {
        await using var command = new MySqlCommand(
            "SELECT customer_id, name, email FROM customer ORDER BY name, customer_id;",
            connection);
        await using var reader = await command.ExecuteReaderAsync();
        var customers = new List<(int Id, string Name, string Email)>();

        while (await reader.ReadAsync())
        {
            customers.Add((reader.GetInt32("customer_id"),
                reader.GetString("name"),
                reader.GetString("email")));
        }

        if (customers.Count == 0)
        {
            Console.WriteLine("No customers are registered yet.");
            return (null, null);
        }

        foreach (var customer in customers)
        {
            Console.WriteLine($"{customer.Id}. {customer.Name} ({customer.Email})");
        }

        int id = ReadPositiveInt("Enter the customer ID: ");
        var selected = customers.FirstOrDefault(customer => customer.Id == id);
        if (selected.Id == 0)
        {
            Console.WriteLine("No customer has that ID.");
            return (null, null);
        }

        return (selected.Id, selected.Name);
    }

    private static async Task<(int Id, string Name)> RegisterCustomerAsync(MySqlConnection connection)
    {
        string name = ReadRequiredText("Customer name: ");
        string phone = ReadRequiredText("Phone number: ");

        while (true)
        {
            string email = ReadRequiredText("Email: ");

            await using var command = new MySqlCommand(
                """
                INSERT INTO customer (name, email, phone)
                VALUES (@name, @email, @phone);
                """,
                connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@phone", phone);

            try
            {
                await command.ExecuteNonQueryAsync();
                int customerId = checked((int)command.LastInsertedId);
                Console.WriteLine($"Customer registered with ID {customerId} and selected.");
                return (customerId, name);
            }
            catch (MySqlException exception) when (exception.Number == 1062)
            {
                // Email is unique in the schema; ask for another instead of exiting.
                Console.WriteLine("That email is already registered. Enter a different email.");
            }
        }
    }

    private static async Task SearchAndBookAsync(MySqlConnection connection, int customerId)
    {
        string city = ReadRequiredText("Town/city: ");
        (DateOnly checkIn, DateOnly checkOut) = ReadStayDates();
        int guests = ReadPositiveInt("Number of guests: ");
        bool requirePool = ReadYesNo("Require a pool? (y/n): ");
        bool requireRestaurant = ReadYesNo("Require a restaurant? (y/n): ");
        int? maxBeachDistance = ReadOptionalDistance("Maximum distance to beach in metres (Enter to skip): ");
        int? maxCenterDistance = ReadOptionalDistance("Maximum distance to town centre in metres (Enter to skip): ");

        Console.WriteLine("Sort by: 1. Lowest nightly price  2. Highest accommodation rating");
        int sortChoice = ReadNumberInRange("Choose sort order: ", 1, 2);

        // The sort expression is selected from fixed SQL fragments, never user-provided SQL.
        string orderBy = sortChoice == 1
            ? "r.price_per_night ASC, a.rating DESC"
            : "a.rating DESC, r.price_per_night ASC";

        string sql = $"""
            SELECT r.room_id, a.name, a.city, r.room_number, r.max_guests,
                   r.price_per_night, a.rating
            FROM room AS r
            JOIN accommodation AS a ON a.accommodation_id = r.accommodation_id
            LEFT JOIN facility AS f ON f.accommodation_id = a.accommodation_id
            WHERE a.city = @city
              AND r.max_guests >= @guests
              AND (@requirePool = 0 OR COALESCE(f.has_pool, 0) = 1)
              AND (@requireRestaurant = 0 OR COALESCE(f.has_restaurant, 0) = 1)
              AND (@maxBeachDistance IS NULL OR a.distance_to_beach_m <= @maxBeachDistance)
              AND (@maxCenterDistance IS NULL OR a.distance_to_center_m <= @maxCenterDistance)
              AND NOT EXISTS (
                  SELECT 1
                  FROM booking AS b
                  WHERE b.room_id = r.room_id
                    AND b.check_in < @checkOut
                    AND b.check_out > @checkIn
              )
            ORDER BY {orderBy};
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@city", city);
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@requirePool", requirePool);
        command.Parameters.AddWithValue("@requireRestaurant", requireRestaurant);
        command.Parameters.AddWithValue("@maxBeachDistance", maxBeachDistance is null
            ? DBNull.Value
            : maxBeachDistance.Value);
        command.Parameters.AddWithValue("@maxCenterDistance", maxCenterDistance is null
            ? DBNull.Value
            : maxCenterDistance.Value);
        command.Parameters.AddWithValue("@checkIn", ToDatabaseDate(checkIn));
        command.Parameters.AddWithValue("@checkOut", ToDatabaseDate(checkOut));

        var rooms = new List<AvailableRoom>();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                rooms.Add(new AvailableRoom(
                    reader.GetInt32("room_id"),
                    reader.GetString("name"),
                    reader.GetString("city"),
                    reader.GetString("room_number"),
                    reader.GetInt32("max_guests"),
                    reader.GetDecimal("price_per_night"),
                    reader.GetInt32("rating")));
            }
        }

        if (rooms.Count == 0)
        {
            Console.WriteLine("No rooms match those dates and filters.");
            return;
        }

        Console.WriteLine();
        foreach (AvailableRoom room in rooms)
        {
            Console.WriteLine(
                $"Room ID {room.Id}: {room.Accommodation}, {room.City}, room {room.Number}; " +
                $"capacity {room.Capacity}; {room.PricePerNight:F2} per night; rating {room.Rating}/5");
        }

        int selectedRoomId = ReadPositiveInt("Enter a room ID to book, or 0 to return: ", allowZero: true);
        if (selectedRoomId == 0)
        {
            return;
        }

        AvailableRoom? selectedRoom = rooms.FirstOrDefault(room => room.Id == selectedRoomId);
        if (selectedRoom is null)
        {
            Console.WriteLine("That room was not in the search results.");
            return;
        }

        await CreateBookingAsync(connection, customerId, selectedRoom, checkIn, checkOut, guests);
    }

    private static async Task CreateBookingAsync(
        MySqlConnection connection,
        int customerId,
        AvailableRoom room,
        DateOnly checkIn,
        DateOnly checkOut,
        int guests)
    {
        // Lock the room row so two application instances following this workflow
        // cannot both pass the availability check for that same room at once.
        await using MySqlTransaction transaction =
            await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            await using var roomCommand = new MySqlCommand(
                "SELECT max_guests, price_per_night FROM room WHERE room_id = @roomId FOR UPDATE;",
                connection,
                transaction);
            roomCommand.Parameters.AddWithValue("@roomId", room.Id);

            int capacity;
            decimal pricePerNight;
            await using (var reader = await roomCommand.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("The selected room no longer exists.");
                }

                capacity = reader.GetInt32("max_guests");
                pricePerNight = reader.GetDecimal("price_per_night");
            }

            if (guests > capacity)
            {
                throw new InvalidOperationException(
                    $"The room holds at most {capacity} guests. No booking was created.");
            }

            // The strict inequalities allow checkout and the next check-in on the same day.
            await using var availabilityCommand = new MySqlCommand(
                """
                SELECT COUNT(*)
                FROM booking
                WHERE room_id = @roomId
                  AND check_in < @checkOut
                  AND check_out > @checkIn;
                """,
                connection,
                transaction);
            availabilityCommand.Parameters.AddWithValue("@roomId", room.Id);
            availabilityCommand.Parameters.AddWithValue("@checkIn", ToDatabaseDate(checkIn));
            availabilityCommand.Parameters.AddWithValue("@checkOut", ToDatabaseDate(checkOut));

            long overlapCount = (long)(await availabilityCommand.ExecuteScalarAsync() ?? 0L);
            if (overlapCount > 0)
            {
                throw new InvalidOperationException(
                    "This room has just become unavailable for those dates. Search again.");
            }

            await using var insertCommand = new MySqlCommand(
                """
                INSERT INTO booking (customer_id, room_id, check_in, check_out, guests)
                VALUES (@customerId, @roomId, @checkIn, @checkOut, @guests);
                """,
                connection,
                transaction);
            insertCommand.Parameters.AddWithValue("@customerId", customerId);
            insertCommand.Parameters.AddWithValue("@roomId", room.Id);
            insertCommand.Parameters.AddWithValue("@checkIn", ToDatabaseDate(checkIn));
            insertCommand.Parameters.AddWithValue("@checkOut", ToDatabaseDate(checkOut));
            insertCommand.Parameters.AddWithValue("@guests", guests);
            await insertCommand.ExecuteNonQueryAsync();

            long bookingId = insertCommand.LastInsertedId;
            await transaction.CommitAsync();

            decimal total = (checkOut.DayNumber - checkIn.DayNumber) * pricePerNight;
            Console.WriteLine(
                $"Booking {bookingId} confirmed. Total: {total:F2} " +
                $"({checkOut.DayNumber - checkIn.DayNumber} nights × {pricePerNight:F2}).");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task ShowBookingsAsync(MySqlConnection connection, int customerId)
    {
        await using var command = new MySqlCommand(
            """
            SELECT b.booking_id, a.name, a.city, r.room_number,
                   b.check_in, b.check_out, b.guests,
                   DATEDIFF(b.check_out, b.check_in) * r.price_per_night AS total_price
            FROM booking AS b
            JOIN room AS r ON r.room_id = b.room_id
            JOIN accommodation AS a ON a.accommodation_id = r.accommodation_id
            WHERE b.customer_id = @customerId
            ORDER BY b.check_in, b.booking_id;
            """,
            connection);
        command.Parameters.AddWithValue("@customerId", customerId);

        await using var reader = await command.ExecuteReaderAsync();
        bool foundAny = false;
        while (await reader.ReadAsync())
        {
            foundAny = true;
            Console.WriteLine(
                $"Booking {reader.GetInt32("booking_id")}: {reader.GetString("name")}, " +
                $"{reader.GetString("city")}, room {reader.GetString("room_number")}; " +
                $"{reader.GetDateTime("check_in"):yyyy-MM-dd} to {reader.GetDateTime("check_out"):yyyy-MM-dd}; " +
                $"{reader.GetInt32("guests")} guests; total {reader.GetDecimal("total_price"):F2}");
        }

        if (!foundAny)
        {
            Console.WriteLine("This customer has no bookings.");
        }
    }

    private static async Task ChangeBookingGuestsAsync(MySqlConnection connection, int customerId)
    {
        int bookingId = ReadPositiveInt("Booking number to update: ");
        int newGuestCount = ReadPositiveInt("New number of guests: ");

        await using var capacityCommand = new MySqlCommand(
            """
            SELECT r.max_guests
            FROM booking AS b
            JOIN room AS r ON r.room_id = b.room_id
            WHERE b.booking_id = @bookingId
              AND b.customer_id = @customerId;
            """,
            connection);
        capacityCommand.Parameters.AddWithValue("@bookingId", bookingId);
        capacityCommand.Parameters.AddWithValue("@customerId", customerId);

        object? capacityResult = await capacityCommand.ExecuteScalarAsync();
        if (capacityResult is null)
        {
            Console.WriteLine("No booking with that number belongs to the selected customer.");
            return;
        }

        int capacity = Convert.ToInt32(capacityResult, CultureInfo.InvariantCulture);
        if (newGuestCount > capacity)
        {
            Console.WriteLine($"The room holds at most {capacity} guests. The booking was not changed.");
            return;
        }

        await using var command = new MySqlCommand(
            "UPDATE booking SET guests = @guests WHERE booking_id = @bookingId AND customer_id = @customerId;",
            connection);
        command.Parameters.AddWithValue("@guests", newGuestCount);
        command.Parameters.AddWithValue("@bookingId", bookingId);
        command.Parameters.AddWithValue("@customerId", customerId);
        await command.ExecuteNonQueryAsync();
        Console.WriteLine("Guest count updated.");
    }

    private static async Task CancelBookingAsync(MySqlConnection connection, int customerId)
    {
        int bookingId = ReadPositiveInt("Booking number to cancel: ");
        if (!ReadYesNo($"Cancel booking {bookingId}? (y/n): "))
        {
            Console.WriteLine("Cancellation aborted.");
            return;
        }

        await using var command = new MySqlCommand(
            "DELETE FROM booking WHERE booking_id = @bookingId AND customer_id = @customerId;",
            connection);
        command.Parameters.AddWithValue("@bookingId", bookingId);
        command.Parameters.AddWithValue("@customerId", customerId);

        Console.WriteLine(await command.ExecuteNonQueryAsync() == 1
            ? "Booking cancelled. The room is available for those dates again."
            : "No booking with that number belongs to the selected customer.");
    }

    private static (DateOnly CheckIn, DateOnly CheckOut) ReadStayDates()
    {
        while (true)
        {
            DateOnly checkIn = ReadDate("Check-in date (yyyy-MM-dd): ");
            DateOnly checkOut = ReadDate("Check-out date (yyyy-MM-dd): ");

            if (checkIn < FirstBookableDate || checkIn >= LastCheckoutDate ||
                checkOut > LastCheckoutDate || checkOut <= checkIn)
            {
                Console.WriteLine(
                    "Dates must be within December 2026, check-out may be 2027-01-01, " +
                    "and check-out must be after check-in.");
                continue;
            }

            return (checkIn, checkOut);
        }
    }

    private static DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (DateOnly.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateOnly date))
            {
                return date;
            }

            Console.WriteLine("Enter a valid date in yyyy-MM-dd format.");
        }
    }

    private static int? ReadOptionalDistance(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (int.TryParse(input, out int distance) && distance >= 0)
            {
                return distance;
            }

            Console.WriteLine("Enter zero or a positive distance in metres, or press Enter to skip.");
        }
    }

    private static int ReadPositiveInt(string prompt, bool allowZero = false)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value) &&
                (allowZero ? value >= 0 : value > 0))
            {
                return value;
            }

            Console.WriteLine(allowZero
                ? "Enter zero or a positive whole number."
                : "Enter a positive whole number.");
        }
    }

    private static int ReadNumberInRange(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value) &&
                value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Enter a number from {minimum} to {maximum}.");
        }
    }

    private static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? value = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.WriteLine("This value is required.");
        }
    }

    private static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? answer = Console.ReadLine()?.Trim();
            if (string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(answer, "n", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("Enter y or n.");
        }
    }

    private static DateTime ToDatabaseDate(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue);

    private sealed record AvailableRoom(
        int Id,
        string Accommodation,
        string City,
        string Number,
        int Capacity,
        decimal PricePerNight,
        int Rating);
}
