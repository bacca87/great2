--=========================================================================
-- Date: 05/11/2019
-- Description: Hotfix carrental
-- Author: Andrea Corradini
--=========================================================================

PRAGMA foreign_keys = off;

ALTER TABLE Car RENAME TO Car_Old;

CREATE TABLE Car
( Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,
  LicensePlate TEXT (15) NOT NULL,
  Brand TEXT (10) NOT NULL,
  Model TEXT (10) NOT NULL,
  CarRentalCompany TEXT (50) NOT NULL
);

INSERT INTO Car SELECT * from Car_Old;

ALTER TABLE CarRentalHistory RENAME TO CarRentalHistory_Old;

CREATE TABLE [CarRentalHistory](
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, 
  [Car] INTEGER NOT NULL REFERENCES [Car]([Id]) ON DELETE RESTRICT, 
  [StartKm] INTEGER NOT NULL, 
  [EndKm] INTEGER, 
  [StartLocation] TEXT(50) NOT NULL, 
  [EndLocation] TEXT(50), 
  [StartDate] INTEGER NOT NULL, 
  [EndDate] INTEGER, 
  [StartFuelLevel] INTEGER NOT NULL, 
  [EndFuelLevel] INTEGER, 
  [Notes] TEXT(255));

  INSERT INTO [CarRentalHistory] SELECT * from [CarRentalHistory_Old];
  DROP TABLE CarRentalHistory_Old;
  DROP TABLE Car_Old;

PRAGMA foreign_keys = on;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 15;
--=========================================================================