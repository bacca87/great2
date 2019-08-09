--=========================================================================
-- Date: 08/08/2019
-- Todo:
-- Author: Andrea Corradini
--=========================================================================


 CREATE TEMPORARY TABLE [DayTemp](
   [Timestamp] INTEGER PRIMARY KEY NOT NULL UNIQUE, 
   [Type] INTEGER NOT NULL DEFAULT 0) ;
  
INSERT INTO [DayTemp] SELECT TimeStamp,Type  FROM Day;

CREATE TEMPORARY TABLE [TempTimesheet](
Id INTEGER NOT NULL,
  [Timestamp]				INTEGER NOT NULL , 
  [TravelStartTimeAM]			INTEGER, 
  [TravelEndTimeAM]				INTEGER, 
  [TravelStartTimePM]			INTEGER, 
  [TravelEndTimePM]			INTEGER, 
  [WorkStartTimeAM]				INTEGER, 
  [WorkEndTimeAM]			INTEGER, 
  [WorkStartTimePM]		 INTEGER, 
  [WorkEndTimePM]			INTEGER, 
  [FDL] TEXT(10) , 
  [Notes] NVARCHAR(50));
  

INSERT INTO [TempTimesheet] SELECT Id,[Timestamp],[TravelStartTimeAM],[TravelEndTimeAM],[TravelStartTimePM]
,[TravelEndTimePM],[WorkStartTimeAM],[WorkEndTimeAM],[WorkStartTimePM],[WorkEndTimePM],[FDL],[Notes] FROM TimeSheet;


DROP TABLE Day;

 CREATE TABLE [Day](
   [Timestamp] INTEGER PRIMARY KEY NOT NULL UNIQUE, 
   [Type] INTEGER NOT NULL DEFAULT 0 REFERENCES [DayType]([Id]) ON DELETE RESTRICT);

   

CREATE TABLE [DayEvent](
  [Timestamp] INTEGER NOT NULL REFERENCES [Day]([Timestamp]) ON DELETE CASCADE ON UPDATE CASCADE, 
  [EventId] INTEGER NOT NULL REFERENCES [Event]([Id]) ON DELETE CASCADE ON UPDATE CASCADE, 
  PRIMARY KEY([Timestamp], [EventId]));

  INSERT INTO Day SELECT Timestamp,Type FROM [DayTemp];
  
  INSERT INTO TimeSheet select Id, [Timestamp],[TravelStartTimeAM],[TravelEndTimeAM],[TravelStartTimePM]
,[TravelEndTimePM],[WorkStartTimeAM],[WorkEndTimeAM],[WorkStartTimePM],[WorkEndTimePM],[FDL],[Notes] FROM [TempTimesheet];

drop table TempTimesheet;
drop table DayTemp;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 7;
--=======================================================================