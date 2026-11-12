-- Holiday Maker schema for MySQL 8.0.16 or later.
-- Select an existing, empty database in DBeaver before running this script.
-- CHECK constraints are enforced starting with MySQL 8.0.16.

CREATE TABLE accommodation (
    accommodation_id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    city VARCHAR(100) NOT NULL,
    distance_to_beach_m INT NOT NULL,
    distance_to_center_m INT NOT NULL,
    rating INT NOT NULL,
    PRIMARY KEY (accommodation_id),
    CONSTRAINT chk_accommodation_beach CHECK (distance_to_beach_m >= 0),
    CONSTRAINT chk_accommodation_center CHECK (distance_to_center_m >= 0),
    CONSTRAINT chk_accommodation_rating CHECK (rating BETWEEN 1 AND 5)
) ENGINE = InnoDB;

CREATE TABLE facility (
    accommodation_id INT NOT NULL,
    has_pool BOOLEAN NOT NULL DEFAULT FALSE,
    has_restaurant BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (accommodation_id),
    CONSTRAINT fk_facility_accommodation FOREIGN KEY (accommodation_id)
        REFERENCES accommodation (accommodation_id) ON DELETE RESTRICT,
    CONSTRAINT chk_facility_pool CHECK (has_pool IN (0, 1)),
    CONSTRAINT chk_facility_restaurant CHECK (has_restaurant IN (0, 1))
) ENGINE = InnoDB;

CREATE TABLE customer (
    customer_id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL,
    phone VARCHAR(30) NOT NULL,
    PRIMARY KEY (customer_id),
    CONSTRAINT uq_customer_email UNIQUE (email)
) ENGINE = InnoDB;

CREATE TABLE room (
    room_id INT NOT NULL AUTO_INCREMENT,
    accommodation_id INT NOT NULL,
    room_number VARCHAR(20) NOT NULL,
    max_guests INT NOT NULL,
    price_per_night DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (room_id),
    CONSTRAINT uq_room_accommodation_number UNIQUE (accommodation_id, room_number),
    CONSTRAINT fk_room_accommodation FOREIGN KEY (accommodation_id)
        REFERENCES accommodation (accommodation_id) ON DELETE RESTRICT,
    CONSTRAINT chk_room_capacity CHECK (max_guests > 0),
    CONSTRAINT chk_room_price CHECK (price_per_night > 0)
) ENGINE = InnoDB;

CREATE TABLE booking (
    booking_id INT NOT NULL AUTO_INCREMENT,
    customer_id INT NOT NULL,
    room_id INT NOT NULL,
    check_in DATE NOT NULL,
    check_out DATE NOT NULL,
    guests INT NOT NULL,
    PRIMARY KEY (booking_id),
    CONSTRAINT fk_booking_customer FOREIGN KEY (customer_id)
        REFERENCES customer (customer_id) ON DELETE RESTRICT,
    CONSTRAINT fk_booking_room FOREIGN KEY (room_id)
        REFERENCES room (room_id) ON DELETE RESTRICT,
    CONSTRAINT chk_booking_dates CHECK (check_out > check_in),
    CONSTRAINT chk_booking_december CHECK (
        check_in >= '2026-12-01' AND check_out <= '2027-01-01'
    ),
    CONSTRAINT chk_booking_guests CHECK (guests > 0),
    INDEX idx_booking_room_dates (room_id, check_in, check_out)
) ENGINE = InnoDB;

-- A single row records that first-run initialization has completed.
CREATE TABLE holiday_maker_setup (
    setup_id TINYINT NOT NULL PRIMARY KEY,
    setup_source VARCHAR(20) NOT NULL,
    CONSTRAINT chk_setup_id CHECK (setup_id = 1)
) ENGINE = InnoDB;

-- The application must check room capacity and overlapping bookings when
-- creating or changing a booking. These cross-table/row rules are not
-- enforced by the CHECK constraints above.
-- Insert one facility row for each accommodation. A missing row means the
-- facilities are unknown, not that the accommodation has no facilities.
