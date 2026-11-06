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