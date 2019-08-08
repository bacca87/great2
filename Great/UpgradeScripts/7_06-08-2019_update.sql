--=========================================================================
-- Date: 08/08/2019
-- Todo:
-- Author: Andrea Corradini
--=========================================================================

--ALTER TABLE [Day] RENAME TO Day_ToDelete;

--CREATE TABLE [Day](
--  [Timestamp] INTEGER PRIMARY KEY NOT NULL UNIQUE, 
--  [Type] INTEGER NOT NULL DEFAULT 0 REFERENCES [DayType]([Id]) ON DELETE RESTRICT);
  
--INSERT INTO Day SELECT TimeStamp,Type  FROM Day_ToDelete;

--DROP TABLE Day_ToDelete

CREATE TABLE [DayEvent](
  [Timestamp] INTEGER NOT NULL REFERENCES [Day]([Timestamp]) ON DELETE CASCADE ON UPDATE CASCADE, 
  [EventId] INTEGER NOT NULL REFERENCES [Event]([Id]) ON DELETE CASCADE ON UPDATE CASCADE, 
  PRIMARY KEY([Timestamp], [EventId]));
--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 7;
--=========================================================================