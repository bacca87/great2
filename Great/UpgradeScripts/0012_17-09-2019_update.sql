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
ALTER TABLE [Factory] ADD COLUMN Country NVARCHAR(2) NULL DEFAULT NULL;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 12;
--=========================================================================