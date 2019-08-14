--=========================================================================
-- Date: 13/08/2019
-- Description: fix EA refunds
-- Author: Marco Baccarani
--=========================================================================

UPDATE FDL SET NotifyAsNew = 0 WHERE [Status] > 0;
UPDATE ExpenseAccount SET NotifyAsNew = 0 WHERE [Status] > 0;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 9;
--=========================================================================