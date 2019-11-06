--=========================================================================
-- Date: 05/11/2019
-- Description: Virtual FDL and EA
-- Author: Marco Baccarani
--=========================================================================

ALTER TABLE FDL ADD COLUMN IsVirtual BOOL DEFAULT 0;
ALTER TABLE ExpenseAccount ADD COLUMN IsVirtual BOOL DEFAULT 0;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 16;
--=========================================================================