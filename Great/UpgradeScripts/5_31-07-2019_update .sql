--=========================================================================
-- Date: 29/07/2019
-- Todo:
-- Author: Andrea Corradini
--=========================================================================

Insert into EventStatus VALUES (4,'Cancelled');
Insert into EventStatus VALUES (5,'PendingCancel');
Insert into EventStatus VALUES (6,'PendingUpdate');

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 5;
--=========================================================================