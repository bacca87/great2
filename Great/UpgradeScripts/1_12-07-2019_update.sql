--=========================================================================
-- Date: 12/07/2019
-- Todo:
-- Author: Marco Baccarani
--=========================================================================

ALTER TABLE [FDL] ADD COLUMN [IsCompiled] BOOL NOT NULL DEFAULT 0;
ALTER TABLE [FDL] ADD COLUMN [IsReadOnly] BOOL NOT NULL DEFAULT 0;
ALTER TABLE [ExpenseAccount] ADD COLUMN [IsCompiled] BOOL NOT NULL DEFAULT 0;
ALTER TABLE [ExpenseAccount] ADD COLUMN [IsReadOnly] BOOL NOT NULL DEFAULT 0;

UPDATE [FDL] SET [IsCompiled] = 1 WHERE [Status] IN (1,2,3);
UPDATE [ExpenseAccount] SET [IsCompiled] = 1 WHERE [Status] IN (1,2,3);

INSERT INTO [DayType] ([Id], [Name]) VALUES(3, 'Day of Home Work');

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 1;
--=========================================================================