# Automated tests

The xUnit project exercises the console application and its MySQL queries. It
includes scenarios for all ten user stories and the cases in
[Manual-Testing.md](./Manual-Testing.md). Each database test
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