-- Create the default Holiday Maker database on MySQL 8.0.16 or later.
-- An existing database is left unchanged; this script does not create tables.
-- Select holiday_maker in DBeaver before running Schema.sql and Data.sql.
-- The application can also create its selected database on first start.
CREATE DATABASE IF NOT EXISTS holiday_maker
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;
