# Manual testing

These steps assume the original sample data is unchanged. Use a separate test database if possible, because some tests change or delete bookings. Record your actual result for each test before submission; blank results do not indicate that a test passed.

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