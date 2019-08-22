--=========================================================================
-- Date: 13/08/2019
-- Description: add send timestamp to fdl and ea
-- Author: Corradini Andrea
--=========================================================================
ALTER TABLE [FDL] ADD COLUMN SendTimeStamp INTEGER  NULL DEFAULT NULL;
ALTER TABLE [ExpenseAccount] ADD COLUMN SendTimeStamp INTEGER  NULL DEFAULT NULL;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 12;
--=========================================================================