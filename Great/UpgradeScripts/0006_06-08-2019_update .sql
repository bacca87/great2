﻿--=========================================================================
-- Date: 06/08/2019
-- Todo:
-- Author: Andrea Corradini
--=========================================================================

ALTER TABLE [Event] ADD COLUMN [SendDateTime] DateTime  NULL DEFAULT NULL;

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 6;
--=========================================================================