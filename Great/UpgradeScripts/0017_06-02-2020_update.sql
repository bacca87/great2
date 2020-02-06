--=========================================================================
-- Date: 06/02/2020
-- Description: IsForfait deprecated
-- Author: Marco Baccarani
--=========================================================================

PRAGMA foreign_keys = off;

CREATE TEMPORARY TABLE factory_backup(
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 
  [Name] TEXT(50) NOT NULL, 
  [CompanyName] TEXT(100), 
  [Address] TEXT(255), 
  [Latitude] REAL, 
  [Longitude] REAL, 
  [TransferType] INTEGER NOT NULL DEFAULT 0 REFERENCES [TransferType]([Id]) ON DELETE RESTRICT, 
  [NotifyAsNew] BOOL NOT NULL DEFAULT 1, 
  [OverrideAddressOnFDL] BOOL NOT NULL DEFAULT 0, 
  [CountryCode] NVARCHAR(2) DEFAULT NULL
);

INSERT INTO factory_backup SELECT Id, Name, CompanyName, Address, Latitude, Longitude, TransferType, NotifyAsNew, OverrideAddressOnFDL, CountryCode FROM Factory;

DROP TABLE Factory;

CREATE TABLE Factory(
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 
  [Name] TEXT(50) NOT NULL, 
  [CompanyName] TEXT(100), 
  [Address] TEXT(255), 
  [Latitude] REAL, 
  [Longitude] REAL, 
  [TransferType] INTEGER NOT NULL DEFAULT 0 REFERENCES [TransferType]([Id]) ON DELETE RESTRICT, 
  [NotifyAsNew] BOOL NOT NULL DEFAULT 1, 
  [OverrideAddressOnFDL] BOOL NOT NULL DEFAULT 0, 
  [CountryCode] NVARCHAR(2) DEFAULT NULL
);

INSERT INTO Factory SELECT * FROM factory_backup;

DROP TABLE factory_backup;

PRAGMA foreign_keys = on;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 17;
--=========================================================================