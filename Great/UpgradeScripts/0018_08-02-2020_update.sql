--=========================================================================
-- Date: 08/02/2020
-- Description: Different distance UOM support for car rentals 
-- Author: Marco Baccarani
--=========================================================================

PRAGMA foreign_keys = off;

CREATE TABLE [UOM](
  [Id] INT PRIMARY KEY NOT NULL, 
  [Name] TEXT(50) NOT NULL, 
  [Symbol] TEXT(3) NOT NULL
);

INSERT INTO [UOM] (Id, Name, Symbol) VALUES(1,	'Kilometers',	'km');
INSERT INTO [UOM] (Id, Name, Symbol) VALUES(2,	'Miles',	'mi');

CREATE TEMPORARY TABLE CarRentalHistory_backup(
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, 
  [Car] INTEGER NOT NULL REFERENCES [Car]([Id]) ON DELETE RESTRICT, 
  [StartDistance] INTEGER NOT NULL, 
  [EndDistance] INTEGER, 
  [UOM] INT NOT NULL DEFAULT 1 REFERENCES [UOM]([Id]) ON DELETE RESTRICT, 
  [StartLocation] TEXT(50) NOT NULL, 
  [EndLocation] TEXT(50), 
  [StartDate] INTEGER NOT NULL, 
  [EndDate] INTEGER, 
  [StartFuelLevel] INTEGER NOT NULL, 
  [EndFuelLevel] INTEGER, 
  [Notes] TEXT(255)
);

INSERT INTO CarRentalHistory_backup SELECT Id, Car, StartKm, EndKm, 1, StartLocation, EndLocation, StartDate, EndDate, StartFuelLevel, EndFuelLevel, Notes FROM CarRentalHistory;

DROP TABLE CarRentalHistory;

CREATE TABLE CarRentalHistory(
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, 
  [Car] INTEGER NOT NULL REFERENCES [Car]([Id]) ON DELETE RESTRICT, 
  [StartDistance] INTEGER NOT NULL, 
  [EndDistance] INTEGER, 
  [UOM] INT NOT NULL DEFAULT 1 REFERENCES [UOM]([Id]) ON DELETE RESTRICT, 
  [StartLocation] TEXT(50) NOT NULL, 
  [EndLocation] TEXT(50), 
  [StartDate] INTEGER NOT NULL, 
  [EndDate] INTEGER, 
  [StartFuelLevel] INTEGER NOT NULL, 
  [EndFuelLevel] INTEGER, 
  [Notes] TEXT(255)
);

INSERT INTO CarRentalHistory SELECT * FROM CarRentalHistory_backup;

DROP TABLE CarRentalHistory_backup;

PRAGMA foreign_keys = on;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 18;
--=========================================================================