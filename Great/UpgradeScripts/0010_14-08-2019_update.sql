--=========================================================================
-- Date: 13/08/2019
-- Description: fix db wrong flags
-- Author: Marco Baccarani
--=========================================================================

UPDATE FDL SET IsCompiled = 1 WHERE IsReadOnly = 1 AND [Status] = 2 OR [Status] = 3;
UPDATE ExpenseAccount SET IsCompiled = 1 WHERE IsReadOnly = 1 AND [Status] = 2 OR [Status] = 3;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 10;
--=========================================================================