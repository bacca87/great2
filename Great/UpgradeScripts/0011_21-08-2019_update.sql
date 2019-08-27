--=========================================================================
-- Date: 13/08/2019
-- Description: trim empty spaces and new lines in last error fields
-- Author: Marco Baccarani
--=========================================================================

UPDATE FDL SET LastError = TRIM(TRIM(TRIM(LastError, CHAR(13)), CHAR(10))) WHERE LastError IS NOT NULL;
UPDATE ExpenseAccount SET LastError = TRIM(TRIM(TRIM(LastError, CHAR(13)), CHAR(10))) WHERE LastError IS NOT NULL;

ALTER TABLE [FDL] ADD COLUMN LastSAPSendTimestamp INTEGER  NULL DEFAULT NULL;
ALTER TABLE [ExpenseAccount] ADD COLUMN LastSAPSendTimestamp INTEGER  NULL DEFAULT NULL;

-- fix day types
Update Day SET Type = 1 where TimeStamp in 
( SELECT Day.Timestamp FROM Day inner join DayEvent on Day.Timestamp = DayEvent.Timestamp 
inner Join Event on Event.Id = DayEvent.EventId 
where Event.Status = 2 and Day.Type= 4);

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 11;
--=========================================================================