-- Holiday Maker test data for MySQL 8.0.16 or later.
-- Select your intended Holiday Maker database in DBeaver and run Schema.sql first.
-- The application also runs this script on first start in its selected database.
-- Run this script only once, on empty accommodation, facility, customer,
-- room and booking tables. It uses fixed IDs so the relationships are clear.
-- It never deletes existing data. If an error occurs, stop the script and
-- run ROLLBACK in the same SQL session before investigating.

-- Supports databases created with an older version of Schema.sql.
CREATE TABLE IF NOT EXISTS holiday_maker_setup (
    setup_id TINYINT NOT NULL PRIMARY KEY,
    setup_source VARCHAR(20) NOT NULL,
    CONSTRAINT chk_setup_id CHECK (setup_id = 1)
) ENGINE = InnoDB;

START TRANSACTION;

-- Insert the marker in the same transaction as the data. A second run fails
-- before inserting any sample rows, even if some data was later deleted.
INSERT INTO holiday_maker_setup (setup_id, setup_source) VALUES (1, 'sample');

-- Six accommodations in five towns, with varied ratings and distances.
INSERT INTO accommodation
    (accommodation_id, name, city, distance_to_beach_m, distance_to_center_m, rating)
VALUES
    (1, 'Seaside Hotel', 'Miami', 200, 1200, 5),
    (2, 'City Guesthouse', 'Malmo', 2500, 150, 3),
    (3, 'Harbour View', 'Gothenburg', 800, 600, 4),
    (4, 'Garden Lodge', 'Ystad', 4000, 1800, 2),
    (5, 'Beach Retreat', 'Nice', 100, 900, 4),
    (6, 'Palm Guesthouse', 'Miami', 600, 500, 3);

-- Each accommodation has one row describing its facilities.
INSERT INTO facility (accommodation_id, has_pool, has_restaurant)
VALUES
    (1, TRUE, TRUE),
    (2, FALSE, TRUE),
    (3, TRUE, FALSE),
    (4, FALSE, FALSE),
    (5, TRUE, TRUE),
    (6, FALSE, TRUE);

-- Four rooms per accommodation, with capacities of 1, 2, 3 and 4.
-- Room numbers can be reused at different accommodations.
INSERT INTO room (room_id, accommodation_id, room_number, max_guests, price_per_night)
VALUES
    (1,  1, '101', 1,  700.00),
    (2,  1, '102', 2,  950.00),
    (3,  1, '103', 3, 1200.00),
    (4,  1, '104', 4, 1500.00),
    (5,  2, '101', 1,  450.00),
    (6,  2, '102', 2,  650.00),
    (7,  2, '103', 3,  850.00),
    (8,  2, '104', 4, 1050.00),
    (9,  3, '101', 1,  600.00),
    (10, 3, '102', 2,  800.00),
    (11, 3, '103', 3, 1050.00),
    (12, 3, '104', 4, 1300.00),
    (13, 4, '101', 1,  400.00),
    (14, 4, '102', 2,  550.00),
    (15, 4, '103', 3,  750.00),
    (16, 4, '104', 4,  900.00),
    (17, 5, '101', 1,  650.00),
    (18, 5, '102', 2,  875.00),
    (19, 5, '103', 3, 1100.00),
    (20, 5, '104', 4, 1400.00),
    (21, 6, '101', 1,  500.00),
    (22, 6, '102', 2,  750.00),
    (23, 6, '103', 3,  950.00),
    (24, 6, '104', 4, 1100.00);

-- Customer records for testing.
INSERT INTO customer (customer_id, name, email, phone)
VALUES
    (1, 'Sofia',  'sofia@example.com',  '070-000-0001'),
    (2, 'Alice',  'alice@example.com',  '070-000-0002'),
    (3, 'Mohammed',  'mohammed@example.com',  '070-000-0003'),
    (4, 'Yassin', 'yassin@example.com', '070-000-0004'),
    (5, 'Abdimajid',  'abdimajid@example.com',  '070-000-0005');

-- All stays are within December 2026 and respect room capacity.
-- Room 1 has back-to-back bookings: checkout and check-in on 13 December.
INSERT INTO booking (customer_id, room_id, check_in, check_out, guests)
VALUES
    (1,  1, '2026-12-10', '2026-12-13', 1),
    (2,  2, '2026-12-01', '2026-12-05', 2),
    (3,  3, '2026-12-06', '2026-12-09', 3),
    (4,  4, '2026-12-15', '2026-12-20', 4),
    (5,  5, '2026-12-02', '2026-12-04', 1),
    (1,  6, '2026-12-08', '2026-12-12', 2),
    (2,  7, '2026-12-14', '2026-12-17', 2),
    (3,  8, '2026-12-21', '2026-12-28', 4),
    (4,  9, '2026-12-03', '2026-12-05', 1),
    (5, 10, '2026-12-09', '2026-12-14', 2),
    (1, 11, '2026-12-18', '2026-12-22', 3),
    (2, 12, '2026-12-25', '2026-12-29', 4),
    (3, 13, '2026-12-01', '2026-12-03', 1),
    (4, 14, '2026-12-05', '2026-12-08', 2),
    (5, 15, '2026-12-12', '2026-12-18', 3),
    (1, 16, '2026-12-24', '2026-12-27', 4),
    (2, 17, '2026-12-04', '2026-12-07', 1),
    (3, 18, '2026-12-11', '2026-12-16', 2),
    (4, 19, '2026-12-19', '2026-12-23', 3),
    (5, 20, '2026-12-28', '2027-01-01', 4),
    (2,  1, '2026-12-13', '2026-12-16', 1),
    (3,  2, '2026-12-05', '2026-12-08', 1),
    (4,  3, '2026-12-20', '2026-12-24', 2),
    (5,  4, '2026-12-01', '2026-12-04', 3),
    (1,  5, '2026-12-26', '2026-12-30', 1);

COMMIT;