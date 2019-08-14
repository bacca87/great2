--=========================================================================
-- Date: 13/08/2019
-- Description: fix db wrong flags
-- Author: Marco Baccarani
--=========================================================================

UPDATE FDL SET NotifyAsNew = 0 WHERE [Status] > 0;
UPDATE ExpenseAccount SET NotifyAsNew = 0 WHERE [Status] > 0;
UPDATE ExpenseAccount SET [IsRefunded] = 0 WHERE [Status] <> 2;
UPDATE ExpenseAccount SET [IsRefunded] = 1 WHERE [Status] = 2 AND IsReadOnly = 1;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 9;
--=========================================================================