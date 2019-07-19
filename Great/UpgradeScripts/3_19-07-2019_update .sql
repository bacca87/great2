--=========================================================================
-- Date: 19/07/2019
-- Todo: Add calendar tables
-- Author: Andrea Corradini
--=========================================================================

CREATE TABLE [EventType](
  [Id] INTEGER PRIMARY KEY NOT NULL UNIQUE, 
  [Name] TEXT(20));

  INSERT INTO [EventType] (Id,Name) VALUES(1,'Vacations');
  INSERT INTO [EventType] (Id,Name) VALUES(2,'Customer Visit');
  INSERT INTO [EventType] (Id,Name) VALUES(3,'Business Trip');
  INSERT INTO [EventType] (Id,Name) VALUES(4,'Education');
  INSERT INTO [EventType] (Id,Name) VALUES(5,'Other');
  INSERT INTO [EventType] (Id,Name) VALUES(6,'Old Vacations');

  CREATE TABLE [EventStatus](
  [Id] INTEGER PRIMARY KEY NOT NULL, 
  [Name] TEXT(20) NOT NULL);

  INSERT INTO [EventStatus] (Id,Name) VALUES(1,'New');
  INSERT INTO [EventStatus] (Id,Name) VALUES(2,'Waiting');
  INSERT INTO [EventStatus] (Id,Name) VALUES(3,'Accepted');
  INSERT INTO [EventStatus] (Id,Name) VALUES(4,'Rejected');
  INSERT INTO [EventStatus] (Id,Name) VALUES(5,'Cancelled');

  CREATE TABLE [Event](
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, 
  [SharepointId] INTEGER NOT NULL, 
  [Type] INTEGER NOT NULL REFERENCES [EventType]([Id]), 
  [Location] TEXT(50), 
  [StartDateTimestamp] INTEGER NOT NULL, 
  [EndDateTimestamp] INTEGER NOT NULL, 
  [Description] TEXT(100), 
  [Status] INTEGER NOT NULL DEFAULT 0 REFERENCES [EventStatus]([Id]), 
  [IsAllDay] BOOL NOT NULL DEFAULT 0, 
  [Approver] TEXT, 
  [ApprovationDate] DATETIME);

   ALTER TABLE [Day] ADD COLUMN [Event] INTEGER null REFERENCES [Event]([Id]);

   UPDATE [DayType] SET Name ='Pending Vacation' WHERE Id=4

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 3;
--=========================================================================