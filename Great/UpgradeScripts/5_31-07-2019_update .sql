--=========================================================================
-- Date: 29/07/2019
-- Todo:
-- Author: Andrea Corradini
--=========================================================================

delete from EventStatus Where Id = 3

ALTER TABLE [Event] ADD COLUMN [IsSent] BOOL NOT NULL DEFAULT 0;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 5;
--=========================================================================