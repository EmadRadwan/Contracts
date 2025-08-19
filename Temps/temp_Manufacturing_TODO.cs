// Reviewing Maanufacturing code:

// 1 - Production Run list to be tag validated by create producion run and similar code
// 2 - Check the use of FacilityId in create production run
// 3 - Check the role of QuantityToProduce in WorkEffort specially in tasks
// 4 - Inventory issuance depends on the facilityId from the UI
// 5 - Inventory Issuance can be enhanced by allowing the selection from certain LotId, etc
// 6 - Check if inventory Issuance sends a proper failure notification if inventory is not available
// 7 - Error in opening Materials Modal if button is clicked after Start Task
// 8 - The Facility DropDown needs to show only facilities that has row materials for the manufactured product
// 9 - Create new Production - from the menu - run while in existing one isn't working.
// 10 - seed data needs to put finished products in the finished products facility
// 11 - WorkEffortId to be added in InventoryItemDetails for the issued items
// 12 - consider aading summary row in inventory reports where needed
// 13 - Consider adding quantity UOM in the inventory reports

// seed data like time to complete a task to be more realistic
// FOH calculation to be sub divided into more realistic categories