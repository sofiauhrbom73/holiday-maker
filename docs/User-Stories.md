# Holiday Maker — User Stories

These stories describe the operator's needs and provide a basis for writing
manual test cases. The acceptance criteria state observable results; they do
not prescribe a particular console-menu design.

## US-01 — Register a customer

**As a** travel agency operator,  
**I want** to register a customer with their name and contact details,  
**so that** I can make bookings for them.

### Acceptance criteria

- The operator can enter and save a customer's name and contact details.
- The application confirms a successful registration and makes the customer
  available for selection.
- Missing required information or invalid input is reported without crashing
  the application.

## US-02 — Select an existing customer

**As a** travel agency operator,  
**I want** to find and select an existing customer,  
**so that** I can create or view bookings for that customer.

### Acceptance criteria

- The operator can identify a customer using the application's selection
  method.
- The selected customer's bookings can be displayed.
- An unknown customer is reported clearly; no booking is created for them.

## US-03 — Search for available rooms

**As a** travel agency operator,  
**I want** to search by town, dates and party size,  
**so that** I can offer rooms that fit the customer's stay.

### Acceptance criteria

- Search accepts a town, check-in date, check-out date and number of guests.
- Results contain only rooms in the requested town with capacity equal to or
  greater than the party size.
- Results exclude rooms with any booking overlapping the requested stay.
- A room is available for a new stay starting on the same day another booking
  checks out.
- A search with no matching rooms returns a clear empty-result message.
- Results show accommodation name and town, room identifier, capacity and
  price per night.

## US-04 — Filter and sort search results

**As a** travel agency operator,  
**I want** to combine facility and distance filters and choose a sort order,  
**so that** I can narrow the available rooms to the customer's preferences.

### Acceptance criteria

- The operator can independently request a pool, a restaurant, a maximum
  beach distance and a maximum town-centre distance.
- Active filters are combined: every displayed result satisfies all of them.
- With no optional filters selected, results are not excluded by those
  filters.
- Results can be sorted by nightly price from lowest to highest.
- Results can be sorted by accommodation rating from highest to lowest.

## US-05 — Create a booking

**As a** travel agency operator,  
**I want** to book an available room for a selected customer,  
**so that** the customer's stay is recorded.

### Acceptance criteria

- A booking stores one customer, one room, check-in and check-out dates, and
  the number of guests.
- The application checks room availability again when saving the booking.
- The application rejects a booking whose guest count exceeds the room's
  maximum capacity.
- The application rejects a booking if its dates overlap another booking for
  the same room, including when one stay completely contains the other.
- A check-out date must be later than the check-in date.
- A stay from 10 December to 13 December is counted as three nights.
- The booking is limited to the supported December 2026 period; check-out on
  1 January 2027 is allowed.
- On success, the application displays the booking number and total price,
  calculated as nights multiplied by the room's price per night.

## US-06 — View a customer's bookings

**As a** travel agency operator,  
**I want** to see a customer's bookings,  
**so that** I can answer questions and confirm stay details.

### Acceptance criteria

- Each displayed booking includes its booking number, accommodation, room,
  dates, guest count and total price.
- A customer with no bookings receives a clear message.

## US-07 — Change the guest count

**As a** travel agency operator,  
**I want** to update the number of guests on a booking,  
**so that** the booking reflects the customer's current party size.

### Acceptance criteria

- The guest count can be changed without changing the room or dates.
- The new count must be a positive number and no greater than the room's
  capacity.
- An invalid count is rejected, and the saved booking remains unchanged.
- A valid count is saved and shown when the booking is viewed again.

## US-08 — Cancel a booking

**As a** travel agency operator,  
**I want** to cancel a selected booking,  
**so that** the room becomes available for those dates again.

### Acceptance criteria

- The operator can select a booking and cancel it.
- The cancelled booking no longer appears in the customer's bookings.
- A new availability search includes the room for the cancelled period, if it
  otherwise meets the search criteria.
- An unknown booking number is reported without affecting other bookings.

## US-09 — Keep data between application runs

**As a** travel agency operator,  
**I want** customer, room and booking data to remain in the database,  
**so that** I can continue working after closing and reopening the application.

### Acceptance criteria

- Data saved before the application closes is still available after restart.
- The operator can find previously registered customers and saved bookings
  after restarting.

## US-10 — Handle invalid input safely

**As a** travel agency operator,  
**I want** clear feedback when I enter invalid information,  
**so that** I can correct mistakes without restarting the application.

### Acceptance criteria

- Invalid dates, non-positive guest counts and malformed menu selections are
  reported clearly.
- Invalid input does not crash the application or create a partial booking.

## Suggested manual test coverage

Use the acceptance criteria to create individual test cases. Ensure the test
data includes at least five accommodations across two or more towns, twenty
rooms with at least three capacities, five fictional customers and twenty-five
bookings. Include varied facilities, distances, ratings and prices, plus
available and fully booked periods.