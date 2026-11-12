## Manual testing procedure

These steps assume the original sample data is unchanged. Use a separate test database if possible, because some tests change or delete bookings. Record your actual result for each test.

1. **Available and occupied rooms**  
   Select a customer and search Miami for **1 guest, December 10–13, 2026**. Check that room ID **1** is not shown because it is booked, while room ID **2** is shown as available.  
   **Result:** ___

2. **Check-in on the previous customer’s checkout day**  
   Room ID 1 has a booking ending on December 16. Search Miami for **1 guest, December 16–17**. Check that room ID **1** is shown as available.  
   **Result:** ___

3. **Overlapping bookings**  
   Search Miami for **1 guest** for each of these periods: **December 9–17** and **December 11–12**. Check that room ID **1** is absent from both results. The first period surrounds an existing booking; the second falls within one.  
   **Result:** ___

4. **Party larger than room capacity**  
   Search Miami for **5 guests**. Check that no rooms are shown: the sample rooms have a maximum capacity of four.  
   **Result:** ___

5. **Combined filters and sorting**  
   Search Miami for **1 guest, December 17–18**. Require both a pool and a restaurant, set the maximum beach distance to **200 metres** and the maximum town-centre distance to **1,200 metres**. Check that only rooms at Seaside Hotel are shown.

   Then search again without the optional filters. Choose **lowest nightly price** and check the prices are in ascending order. Repeat with **highest accommodation rating** and check the ratings are in descending order.  
   **Result:** ___

6. **Changing the number of guests**  
   Select customer **Alice** and find her booking for room ID **2**, which holds up to two guests. Change the guest count from two to one and check that it saves. Then try changing it to three and check that the change is refused.  
   **Result:** ___

7. **Cancelling a booking**  
   Select customer **Sofia** and cancel her booking for room ID **1, December 10–13**. Search again for the same dates and check that the room is now available.  
   **Result:** ___

8. **Data persists after restart**  
   Book an available room and write down the booking number and details. Exit and restart the application, select the same customer, and view their bookings. Check that the booking and its details are still there.  
   **Result:** ___

Perform the cancellation test **after tests 1–3**, since those tests rely on the original booking still existing.

## Automated tests

The xUnit project exercises the console application and its MySQL queries. It
includes scenarios for all ten user stories and the manual cases above. Each database test
creates a uniquely named temporary database, initializes it with [Schema.sql](../database/Schema.sql)
and [Data.sql](../database/Data.sql) through normal application startup, and drops only that database
afterward. The tests do not use or reset your existing application databases.

Use a **dedicated test MySQL server/account** with permission to create and drop
databases, create tables, and query `information_schema`. Do not use a
production account. In PowerShell, set the connection string without saving it
in a file or shell history:

```powershell
$secret = Read-Host "MySQL test connection string" -AsSecureString
$credential = [System.Management.Automation.PSCredential]::new("test", $secret)
$env:HOLIDAY_MAKER_TEST_CONNECTION_STRING = $credential.GetNetworkCredential().Password
dotnet test .\HolidayMaker.Tests\HolidayMaker.Tests.csproj
Remove-Item Env:\HOLIDAY_MAKER_TEST_CONNECTION_STRING
```

In Git Bash, save the test connection string once from the project folder:

```bash
read -rs -p "MySQL test connection string: " test_connection
echo
printf 'HOLIDAY_MAKER_TEST_CONNECTION_STRING=%s\n' "$test_connection" > .env.test.local
unset test_connection
```

Then run the tests whenever needed:

```bash
bash run-tests.sh
```

The script loads `.env.test.local` without executing it. An already-exported
`HOLIDAY_MAKER_TEST_CONNECTION_STRING` takes precedence; if neither is set, the
script prompts without displaying the input. The local file is ignored by Git
but stores the password as **plain text**, so keep it private and use only a
dedicated test account. Do not commit or share it.

Use the same connection-string format described in [HolidayMakerRun.md](./HolidayMakerRun.md); the database
field is ignored because the tests create their own databases. If a test process
is forcibly terminated, a database named `holiday_maker_test_<random id>` may
remain and must be inspected and removed manually. Tests fail explicitly if the
test connection is not configured. Startup configuration tests can be run
without MySQL:

```powershell
dotnet test .\HolidayMaker.Tests\HolidayMaker.Tests.csproj --filter "FullyQualifiedName~StartupTests"
```