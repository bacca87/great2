--=========================================================================
-- Date: 17/09/2019
-- Description: fix iscompiled fields on imported fdl and ea
-- Author: Marco Baccarani
--=========================================================================

UPDATE FDL SET IsCompiled = 1 WHERE IsReadOnly = 1 AND [Status] = 2 OR [Status] = 3;
UPDATE ExpenseAccount SET IsCompiled = 1 WHERE IsReadOnly = 1 AND [Status] = 2 OR [Status] = 3;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 12;
--=========================================================================