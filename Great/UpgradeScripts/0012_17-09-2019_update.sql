--=========================================================================
-- Date: 17/09/2019
-- Description: fix iscompiled fields on imported fdl and ea
-- Author: Marco Baccarani
--=========================================================================

-- new Event fields
ALTER TABLE [Event] ADD COLUMN IsCancelRequested BOOL NOT NULL DEFAULT 0;
ALTER TABLE [Event] ADD COLUMN Notes NVARCHAR(50)  NULL DEFAULT NULL;

-- fix iscompiled flags
UPDATE FDL SET IsCompiled = 1 WHERE IsReadOnly = 1 AND [Status] = 2 OR [Status] = 3;
UPDATE ExpenseAccount SET IsCompiled = 1 WHERE IsReadOnly = 1 AND [Status] = 2 OR [Status] = 3;

-- New factory field
ALTER TABLE [Factory] ADD COLUMN CountryCode NVARCHAR(2) NULL DEFAULT NULL;

-- Fix weekend type connected to events: delete all and reimport it from exchange
UPDATE  Day Set Type =0, Event = null 
where Day.Timestamp in (
Select Day.Timestamp FROM Day JOIN DayEvent on Day.TimeStamp = Day.TimeStamp
JOIN Event on DayEvent.EventId = Event.Id
where Event.Type =1);

DELETE FROM Event;

-- Add WidheldAmount to ea
ALTER TABLE [ExpenseAccount] ADD COLUMN Deductions REAL NULL DEFAULT NULL;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 12;
--=========================================================================