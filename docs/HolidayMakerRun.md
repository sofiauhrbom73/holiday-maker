# Run the console application

The application requires .NET 10, a running MySQL 8.0.16 or later server, and your own MySQL connection details. After setting the connection string below,
run `dotnet run --project Holiday-Maker.csproj`.

If the database named in the connection string does not exist, the application creates it. This requires `CREATE` database permission. The application user also needs permission to create tables, read and write data, and query
`information_schema`. If the account cannot create databases, an administrator can create the database and grant access before the first run.

On first start, the app creates the tables if the selected database is empty.

It then inserts the sample data if all five project tables are empty.
An initialization marker prevents sample data being inserted again on later
starts. If project tables already contain data, the app keeps it and does not try to fill missing sample data. If only some required tables exist, setup stops with an error; inspect the database rather than deleting existing data.

MySQL table creation is not transactional, so an interrupted schema creation may also need manual inspection.

Alternatively, run [CreateDatabase.sql](../database/CreateDatabase.sql) in DBeaver to create `holiday_maker`, refresh the connection, select the database, and run [Schema.sql](../database/Schema.sql) followed by [Data.sql](../database/Data.sql).

Those scripts use the selected database; they do not hard-code its
name, so they also work with the other environments. Run them only on a new, empty database, and run the entire data script only once. If it fails, stop execution and run `ROLLBACK` in the same SQL session before investigating. Neither normal app startup nor these scripts drop the database.

For a single-file setup instead, import [HolidayMaker-sample-dump.sql](../database/HolidayMaker-sample-dump.sql) into a MySQL server. The dump creates and selects `holiday_maker`, then creates its tables and inserts the original sample data. Use it only when that database does not exist or is empty; it is not intended to update an existing installation. Choose either the dump, the three SQL scripts, or automatic first-start setup, not more than one for the same database. The dump was generated from the scripts, not from an existing customer database.

Each person running the project should use their own MySQL server or an
authorized account on a shared server. Prefer a dedicated MySQL user with
access only to the project database rather than using `root`.

The app reads its connection details from the `HOLIDAY_MAKER_CONNECTION_STRING` environment variable. Do not put real
passwords or connection strings in source code, documentation, or files
committed to Git.

**Git Bash in VS Code:**

```bash
read -rs -p "MySQL connection string: " HOLIDAY_MAKER_CONNECTION_STRING
echo
export HOLIDAY_MAKER_CONNECTION_STRING
```

At the prompt, enter a connection string in this format:

```text
Server=localhost;Port=3306;Database=holiday_maker;User ID=YOUR_MYSQL_USER;Password=YOUR_MYSQL_PASSWORD;
```

Replace the placeholders with your own MySQL username and password, and adjust the server and port if needed. The input is not shown on screen or saved in shell command history. The variable is available only in the current terminal session and is inherited by processes started from it. Close the terminal or
run `unset HOLIDAY_MAKER_CONNECTION_STRING` when finished. Environment
variables are not an encrypted secrets vault.

## Select a database

After setting the environment variable, choose an environment:

```bash
dotnet run --project Holiday-Maker.csproj -- dev
dotnet run --project Holiday-Maker.csproj -- test
dotnet run --project Holiday-Maker.csproj -- prod
```

Run one command at a time. `dev` selects `holiday_maker_dev`, `test` selects
`holiday_maker_test`, and `prod` selects `holiday_maker1`. The selected name
overrides the database in the connection string; the server and credentials
stay the same. A missing selected database is created on first start if the
MySQL user has permission.

The app prints the selected database before connecting and shows it in the
menu. Exit with option `0` before starting it with another environment.
Without an environment argument, the app continues to use the database in
the connection string:

```bash
dotnet run --project Holiday-Maker.csproj
```

These databases have independent data. Check the selected database before
running tests that change or delete data. The app inserts sample data into an empty database, including an empty database intended for production. Selecting
`prod` does not provide production security or prevent destructive operations.

The program supports registering/selecting customers, searching and filtering available rooms, creating bookings, viewing bookings, changing guest counts and cancelling bookings. Room availability is checked again inside a transaction when a booking is saved.