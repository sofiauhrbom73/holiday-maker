
/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `holiday_maker` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `holiday_maker`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `accommodation` (
  `accommodation_id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `city` varchar(100) NOT NULL,
  `distance_to_beach_m` int NOT NULL,
  `distance_to_center_m` int NOT NULL,
  `rating` int NOT NULL,
  PRIMARY KEY (`accommodation_id`),
  CONSTRAINT `chk_accommodation_beach` CHECK ((`distance_to_beach_m` >= 0)),
  CONSTRAINT `chk_accommodation_center` CHECK ((`distance_to_center_m` >= 0)),
  CONSTRAINT `chk_accommodation_rating` CHECK ((`rating` between 1 and 5))
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `accommodation` VALUES (1,'Seaside Hotel','Miami',200,1200,5),(2,'City Guesthouse','Malmo',2500,150,3),(3,'Harbour View','Gothenburg',800,600,4),(4,'Garden Lodge','Ystad',4000,1800,2),(5,'Beach Retreat','Nice',100,900,4),(6,'Palm Guesthouse','Miami',600,500,3);
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `booking` (
  `booking_id` int NOT NULL AUTO_INCREMENT,
  `customer_id` int NOT NULL,
  `room_id` int NOT NULL,
  `check_in` date NOT NULL,
  `check_out` date NOT NULL,
  `guests` int NOT NULL,
  PRIMARY KEY (`booking_id`),
  KEY `fk_booking_customer` (`customer_id`),
  KEY `idx_booking_room_dates` (`room_id`,`check_in`,`check_out`),
  CONSTRAINT `fk_booking_customer` FOREIGN KEY (`customer_id`) REFERENCES `customer` (`customer_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_booking_room` FOREIGN KEY (`room_id`) REFERENCES `room` (`room_id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_booking_dates` CHECK ((`check_out` > `check_in`)),
  CONSTRAINT `chk_booking_december` CHECK (((`check_in` >= _latin1'2026-12-01') and (`check_out` <= _latin1'2027-01-01'))),
  CONSTRAINT `chk_booking_guests` CHECK ((`guests` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `booking` VALUES (1,1,1,'2026-12-10','2026-12-13',1),(2,2,2,'2026-12-01','2026-12-05',2),(3,3,3,'2026-12-06','2026-12-09',3),(4,4,4,'2026-12-15','2026-12-20',4),(5,5,5,'2026-12-02','2026-12-04',1),(6,1,6,'2026-12-08','2026-12-12',2),(7,2,7,'2026-12-14','2026-12-17',2),(8,3,8,'2026-12-21','2026-12-28',4),(9,4,9,'2026-12-03','2026-12-05',1),(10,5,10,'2026-12-09','2026-12-14',2),(11,1,11,'2026-12-18','2026-12-22',3),(12,2,12,'2026-12-25','2026-12-29',4),(13,3,13,'2026-12-01','2026-12-03',1),(14,4,14,'2026-12-05','2026-12-08',2),(15,5,15,'2026-12-12','2026-12-18',3),(16,1,16,'2026-12-24','2026-12-27',4),(17,2,17,'2026-12-04','2026-12-07',1),(18,3,18,'2026-12-11','2026-12-16',2),(19,4,19,'2026-12-19','2026-12-23',3),(20,5,20,'2026-12-28','2027-01-01',4),(21,2,1,'2026-12-13','2026-12-16',1),(22,3,2,'2026-12-05','2026-12-08',1),(23,4,3,'2026-12-20','2026-12-24',2),(24,5,4,'2026-12-01','2026-12-04',3),(25,1,5,'2026-12-26','2026-12-30',1);
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customer` (
  `customer_id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `email` varchar(150) NOT NULL,
  `phone` varchar(30) NOT NULL,
  PRIMARY KEY (`customer_id`),
  UNIQUE KEY `uq_customer_email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `customer` VALUES (1,'Sofia','sofia@example.com','070-000-0001'),(2,'Alice','alice@example.com','070-000-0002'),(3,'Mohammed','mohammed@example.com','070-000-0003'),(4,'Yassin','yassin@example.com','070-000-0004'),(5,'Abdimajid','abdimajid@example.com','070-000-0005');
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `facility` (
  `accommodation_id` int NOT NULL,
  `has_pool` tinyint(1) NOT NULL DEFAULT '0',
  `has_restaurant` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`accommodation_id`),
  CONSTRAINT `fk_facility_accommodation` FOREIGN KEY (`accommodation_id`) REFERENCES `accommodation` (`accommodation_id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_facility_pool` CHECK ((`has_pool` in (0,1))),
  CONSTRAINT `chk_facility_restaurant` CHECK ((`has_restaurant` in (0,1)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `facility` VALUES (1,1,1),(2,0,1),(3,1,0),(4,0,0),(5,1,1),(6,0,1);
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `holiday_maker_setup` (
  `setup_id` tinyint NOT NULL,
  `setup_source` varchar(20) NOT NULL,
  PRIMARY KEY (`setup_id`),
  CONSTRAINT `chk_setup_id` CHECK ((`setup_id` = 1))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `holiday_maker_setup` VALUES (1,'sample');
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `room` (
  `room_id` int NOT NULL AUTO_INCREMENT,
  `accommodation_id` int NOT NULL,
  `room_number` varchar(20) NOT NULL,
  `max_guests` int NOT NULL,
  `price_per_night` decimal(10,2) NOT NULL,
  PRIMARY KEY (`room_id`),
  UNIQUE KEY `uq_room_accommodation_number` (`accommodation_id`,`room_number`),
  CONSTRAINT `fk_room_accommodation` FOREIGN KEY (`accommodation_id`) REFERENCES `accommodation` (`accommodation_id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_room_capacity` CHECK ((`max_guests` > 0)),
  CONSTRAINT `chk_room_price` CHECK ((`price_per_night` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `room` VALUES (1,1,'101',1,700.00),(2,1,'102',2,950.00),(3,1,'103',3,1200.00),(4,1,'104',4,1500.00),(5,2,'101',1,450.00),(6,2,'102',2,650.00),(7,2,'103',3,850.00),(8,2,'104',4,1050.00),(9,3,'101',1,600.00),(10,3,'102',2,800.00),(11,3,'103',3,1050.00),(12,3,'104',4,1300.00),(13,4,'101',1,400.00),(14,4,'102',2,550.00),(15,4,'103',3,750.00),(16,4,'104',4,900.00),(17,5,'101',1,650.00),(18,5,'102',2,875.00),(19,5,'103',3,1100.00),(20,5,'104',4,1400.00),(21,6,'101',1,500.00),(22,6,'102',2,750.00),(23,6,'103',3,950.00),(24,6,'104',4,1100.00);
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

