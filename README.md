# holiday-maker

## Description

Plan and create a relational database and use it in a simple C# console application.

Holiday Maker is used by a travel agency operator who helps customers book holiday accommodation over the phone. The operator needs to find available rooms, register customers and manage their bookings.

The focus is on **the database design, your SQL queries and how the application uses the database**. A simple console menu with follow-up prompts is sufficient as the interface.

## Accommodation, rooms and bookings

An *accommodation* is, for example, a hotel or guesthouse. Each accommodation is located in a town and has a number of bookable rooms.

For each accommodation, the following information must be available:

- Name and town.
- Pool and restaurant.
- Distance to the beach and town centre, in metres.
- Rating, expressed as a value from 1 to 5.

Each room has a room number or another identifier within the accommodation, a maximum capacity and a price per night.

The following simplifications apply:

- Each booking is for **one customer and one room**.
- A customer can have multiple bookings.
- All people count equally when checking a room's capacity.
- The room's price per night is the same throughout the booking period.
- All rooms can be booked during December 2026, except when they are already occupied.
- The operator does not need to log in.

You decide how to organise the information into tables.

## The operator must be able to

### 1. Register and select a customer

Register a customer with their name and contact details, and select an existing customer before making a booking.

### 2. Search for available rooms

Enter a town, check-in date, check-out date and number of people. The results must include only rooms with sufficient capacity that are available for the entire period.

The operator must also be able to combine optional filters:

- A pool must be available.
- A restaurant must be available.
- Maximum distance to the beach.
- Maximum distance to the town centre.

The results must be sortable by price per night, lowest first, or by the accommodation's rating, highest first. Display the accommodation's name and town, room, capacity and price.

### 3. Create a booking

Book a selected room for the customer. Save the dates and number of people. Display a confirmation with the booking number and total price: number of nights × price per night.

### 4. Show a customer's bookings

Display the booking number, accommodation, room, dates, number of people and total price.

### 5. Change the number of people in a booking

The new number must fit within the booked room's capacity. Changing the room or dates is not part of the basic assignment.

### 6. Cancel a booking

Remove a selected booking from the database. The room must then be available to book again for that period.

## Date and availability rules

The check-out date must be after the check-in date. A booking from 10 to 13 December is for three nights.

**The room is not occupied on the check-out day.** A customer can therefore check out on 13 December while the next customer checks in on the same day.

The application must check availability again when the booking is saved. A room must not be double-booked in the intended workflow with one operator at a time.

## Test data

The database must contain at least:

- 5 accommodations across at least 2 towns.
- 20 rooms with at least 3 different capacities, for example 1, 2 and 4 people.
- 5 fictional customers.
- 25 bookings with varying dates and durations.

Vary prices, ratings, distances and facilities. It must be possible to demonstrate both searches that find rooms and searches that return no results.

You may create test data using SQL scripts. Not all bookings need to be entered manually through the console application.

## Database and SQL

You must do the following, and may do so together in a small group:

- Start with an ER diagram showing tables, relationships, primary keys and foreign keys.
- Choose appropriate data types and use relevant constraints, such as `NOT NULL`, `UNIQUE` and `CHECK`.
- Avoid unnecessary duplication of stored data (normalise) and be able to explain how you have organised the information.

## Run the console application

The application requires .NET 10, a running MySQL 8.0.16 or later server, and
your own MySQL connection details. After setting the connection string below,
run `dotnet run --project Holiday-Maker.csproj`.

If the database named in the connection string does not exist, the application
creates it. This requires `CREATE` database permission. The application user
also needs permission to create tables, read and write data, and query
`information_schema`. If the account cannot create databases, an administrator
can create the database and grant access before the first run.

On first start, the app creates the tables if the selected database is empty.
It then inserts the sample data if all five project tables are empty.
An initialization marker prevents sample data being inserted again on later
starts. If project tables already contain data, the app keeps it and does not
try to fill missing sample data. If only some required tables exist, setup stops
with an error; inspect the database rather than deleting existing data.
MySQL table creation is not transactional, so an interrupted schema creation
may also need manual inspection.

Alternatively, run `CreateDatabase.sql` in DBeaver to create `holiday_maker`,
refresh the connection, select the database, and run `Schema.sql` followed by
`Data.sql`. Those scripts use the selected database; they do not hard-code its
name, so they also work with the other environments. Run them only on a new,
empty database, and run the entire data script only once. If it fails, stop
execution and run `ROLLBACK` in the same SQL session before investigating.
Neither normal app startup nor these scripts drop the database.

For a single-file setup instead, import `HolidayMaker-sample-dump.sql` into a
MySQL server. The dump creates and selects `holiday_maker`, then creates its
tables and inserts the original sample data. Use it only when that database
does not exist or is empty; it is not intended to update an existing
installation. Choose either the dump, the three SQL scripts, or automatic
first-start setup, not more than one for the same database. The dump was
generated from the scripts, not from an existing customer database.

Each person running the project should use their own MySQL server or an
authorized account on a shared server. Prefer a dedicated MySQL user with
access only to the project database rather than using `root`.

The app reads its connection details from the
`HOLIDAY_MAKER_CONNECTION_STRING` environment variable. Do not put real
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

Replace the placeholders with your own MySQL username and password, and adjust
the server and port if needed. The input is not shown on screen or saved in
shell command history. The variable is available only in the current terminal
session and is inherited by processes started from it. Close the terminal or
run `unset HOLIDAY_MAKER_CONNECTION_STRING` when finished. Environment
variables are not an encrypted secrets vault.

### Select a database

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
running tests that change or delete data. The app inserts sample data into an
empty database, including an empty database intended for production. Selecting
`prod` does not provide production security or prevent destructive operations.

The program
supports registering/selecting customers, searching and filtering available
rooms, creating bookings, viewing bookings, changing guest counts and
cancelling bookings. Room availability is checked again inside a transaction
when a booking is saved.