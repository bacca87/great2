--=========================================================================
-- Date: 13/08/2019
-- Description: trim empty spaces and new lines in last error fields
-- Author: Marco Baccarani
--=========================================================================

UPDATE FDL SET LastError = TRIM(TRIM(TRIM(LastError, CHAR(13)), CHAR(10))) WHERE LastError IS NOT NULL;
UPDATE ExpenseAccount SET LastError = TRIM(TRIM(TRIM(LastError, CHAR(13)), CHAR(10))) WHERE LastError IS NOT NULL;

ALTER TABLE [FDL] ADD COLUMN LastSAPSendTimestamp INTEGER  NULL DEFAULT NULL;
ALTER TABLE [ExpenseAccount] ADD COLUMN LastSAPSendTimestamp INTEGER  NULL DEFAULT NULL;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 11;
--=========================================================================