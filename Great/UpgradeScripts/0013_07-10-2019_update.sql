--=========================================================================
-- Date: 07/10/2019
-- Description: events bugfix
-- Author: Marco Baccarani
--=========================================================================

DELETE FROM Event;
DELETE FROM Day WHERE strftime('%H:%M:%S', datetime(timestamp, 'unixepoch')) <> '00:00:00';

--=========================================================================
-- Remove CarRentalCompany Table
--=========================================================================
PRAGMA foreign_keys=off;

ALTER TABLE Car RENAME TO Car_Old;

CREATE TABLE Car
( Id INTEGER,
  LicensePlate TEXT (15) NOT NULL,
  Brand TEXT (10) NOT NULL,
  Model TEXT (10) NOT NULL,
  CarRentalCompany TEXT (50) NOT NULL
);

INSERT INTO Car SELECT Car_Old.Id
                       ,Car_Old.LicensePlate
                       ,Car_Old.Brand
                       ,Car_Old.Model
                       ,CarRentalCompany.Name
                        FROM Car_Old  JOIN CarRentalCompany on Car_Old.CarRentalCompany = CarRentalCompany.Id;

DROP TABLE Car_Old;
DROP TABLE CarRentalCompany;

PRAGMA foreign_keys=on;

--========================================================================

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 13;
--=========================================================================