--=========================================================================
-- Date: 18/08/2020
-- Description: Changed timesheet notes max length
-- Author: Andrea Corradini
--=========================================================================

PRAGMA foreign_keys=off;

ALTER TABLE Timesheet RENAME TO _timesheet;

CREATE TABLE [Timesheet]
(
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, 
  [Timestamp] INTEGER NOT NULL REFERENCES [Day]([Timestamp]) ON DELETE CASCADE, 
  [TravelStartTimeAM] INTEGER, 
  [TravelEndTimeAM] INTEGER, 
  [TravelStartTimePM] INTEGER, 
  [TravelEndTimePM] INTEGER, 
  [WorkStartTimeAM] INTEGER, 
  [WorkEndTimeAM] INTEGER, 
  [WorkStartTimePM] INTEGER, 
  [WorkEndTimePM] INTEGER, 
  [FDL] TEXT(10) REFERENCES [FDL]([Id]) ON DELETE CASCADE, 
  [Notes] NVARCHAR(255)
);

 
INSERT INTO TimeSheet SELECT *FROM _timesheet;

DROP TABLE _timesheet;

PRAGMA foreign_keys=on;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 20;
--=========================================================================