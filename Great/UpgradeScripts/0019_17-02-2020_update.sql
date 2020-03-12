--=========================================================================
-- Date: 17/02/2020
-- Description: Added expense type category in order to support different localizations and rules
-- Author: Marco Baccarani
--=========================================================================

ALTER TABLE [ExpenseType] ADD [Category] INT NOT NULL DEFAULT 0;

insert into ExpenseType (Category, Description) values(1,'Telephone accessories');
insert into ExpenseType (Category, Description) values(1,'Advance');
insert into ExpenseType (Category, Description) values(1,'Cash advance');
insert into ExpenseType (Category, Description) values(1,'Equipment');
insert into ExpenseType (Category, Description) values(1,'Rental car');
insert into ExpenseType (Category, Description) values(1,'Stationery');
insert into ExpenseType (Category, Description) values(1,'Fuel');
insert into ExpenseType (Category, Description) values(1,'Dinner');
insert into ExpenseType (Category, Description) values(1,'Breakfast');
insert into ExpenseType (Category, Description) values(1,'Transaction fees');
insert into ExpenseType (Category, Description) values(1,'Daily allowance');
insert into ExpenseType (Category, Description) values(1,'Extra baggage');
insert into ExpenseType (Category, Description) values(1,'Exhibitions');
insert into ExpenseType (Category, Description) values(1,'Forfait km - car');
insert into ExpenseType (Category, Description) values(1,'Overnight package');
insert into ExpenseType (Category, Description) values(1,'Hotel');
insert into ExpenseType (Category, Description) values(1,'Internet');
insert into ExpenseType (Category, Description) values(1,'Car wash');
insert into ExpenseType (Category, Description) values(1,'Consumables');
insert into ExpenseType (Category, Description) values(1,'Meter');
insert into ExpenseType (Category, Description) values(1,'Customer gifts');
insert into ExpenseType (Category, Description) values(1,'Parking area');
insert into ExpenseType (Category, Description) values(1,'Toll');
insert into ExpenseType (Category, Description) values(1,'Pocket money');
insert into ExpenseType (Category, Description) values(1,'Lunch');
insert into ExpenseType (Category, Description) values(1,'Perst. for translations');
insert into ExpenseType (Category, Description) values(1,'Car repair / Accessories');
insert into ExpenseType (Category, Description) values(1,'DIFFERENT SERVICES');
insert into ExpenseType (Category, Description) values(1,'External expenses');
insert into ExpenseType (Category, Description) values(1,'Medical expenses');
insert into ExpenseType (Category, Description) values(1,'Expense report ded. 100%');
insert into ExpenseType (Category, Description) values(1,'Representation expenses');
insert into ExpenseType (Category, Description) values(1,'Taxes');
insert into ExpenseType (Category, Description) values(1,'Taxi');
insert into ExpenseType (Category, Description) values(1,'Fixed telephony');
insert into ExpenseType (Category, Description) values(1,'Phone');
insert into ExpenseType (Category, Description) values(1,'Train');
insert into ExpenseType (Category, Description) values(1,'Endorsements, stamps and register');
insert into ExpenseType (Category, Description) values(1,'Vest. and equip.');
insert into ExpenseType (Category, Description) values(1,'Flight');

--=========================================================================
-- MANDATORY: Increment internal db version
PRAGMA user_version = 19;
--=========================================================================