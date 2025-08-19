// src/features/orders/return/hooks/useMockReturnData.ts
import { useCallback } from 'react';
import { Return, ReturnItem, ReturnAdjustment, PartyOrder, ReturnableItems } from '../../../../app/models/order/return';

// REFACTOR: Create mock data hook for testing
// Purpose: Provide static data when RTK hooks are unavailable
// Why: Prevents undefined errors and enables component testing
export const useMockReturnData = () => {
    const mockReturnHeader: Return = {
        returnId: 'mock_return_1',
        returnHeaderTypeId: 'CUSTOMER_RETURN',
        statusId: 'RETURN_REQUESTED',
        fromPartyId: 'customer_1',
        toPartyId: 'vendor_1',
        destinationFacilityId: 'facility_1',
        needsInventoryReceive: 'N',
    };

    const mockReturnItems: ReturnItem[] = [
        {
            returnId: 'mock_return_1',
            returnItemSeqId: '001',
            orderId: 'order_1',
            orderItemSeqId: '001',
            returnItemTypeId: 'RET_PROD_ITEM',
            productId: 'prod_1',
            description: 'Mock Product 1',
            returnQuantity: 2,
            returnPrice: 10.99,
            returnReasonId: 'reason_1',
            returnTypeId: 'type_1',
            status: { statusId: 'ITEM_COMPLETED', description: 'Completed' },
            returnReason: { returnReasonId: 'reason_1', description: 'Defective' },
            returnType: { returnTypeId: 'type_1', description: 'Refund' },
            receivedQuantity: 1,
            shipmentReceipts: [
                { inventoryItemId: 'inv_1', quantityAccepted: 1, datetimeReceived: '2025-07-23T10:00:00Z' },
            ],
            returnItemResponse: { paymentId: 'pay_1', replacementOrderId: null, billingAccountId: null },
        },
    ];

    const mockReturnAdjustments: ReturnAdjustment[] = [
        {
            returnId: 'mock_return_1',
            returnAdjustmentId: 'adj_1',
            amount: -5.00,
            description: 'Discount Adjustment',
            comments: 'Promotional discount',
            returnTypeId: 'type_2',
            returnType: { returnTypeId: 'type_2', description: 'Discount' },
        },
    ];

    const mockReturnTypes = [
        { returnTypeId: 'type_1', description: 'Refund' },
        { returnTypeId: 'type_2', description: 'Discount' },
    ];

    const mockReturnReasons = [
        { returnReasonId: 'reason_1', description: 'Defective' },
        { returnReasonId: 'reason_2', description: 'Wrong Item' },
    ];

    const mockPartyOrders: PartyOrder[] = [
        { orderId: 'order_1', orderDate: '2025-07-20T12:00:00Z' },
        { orderId: 'order_2', orderDate: '2025-07-21T12:00:00Z' },
    ];

    const mockReturnableItems: ReturnableItems = {
        orderTotal: 100.00,
        creditedTotal: 20.00,
        refundedTotal: 10.00,
    };

    const handleLoadOrderItems = useCallback(async (orderId: string) => {
        console.log(`Mock: Loading items for order ${orderId}`);
        return true;
    }, []);

    const handleRemoveItem = useCallback(async (returnItemSeqId: string) => {
        console.log(`Mock: Removing item ${returnItemSeqId}`);
    }, []);

    const handleRemoveAdjustment = useCallback(async (returnAdjustmentId: string) => {
        console.log(`Mock: Removing adjustment ${returnAdjustmentId}`);
    }, []);

    const handleAcceptReturn = useCallback(async () => {
        console.log('Mock: Accepting return');
    }, []);

    const handleAddItem = useCallback(async (item: ReturnItem) => {
        console.log('Mock: Adding item', item);
    }, []);

    const handleAddAdjustment = useCallback(async (adjustment: ReturnAdjustment) => {
        console.log('Mock: Adding adjustment', adjustment);
    }, []);

    return {
        returnHeader: mockReturnHeader,
        returnItems: mockReturnItems,
        returnAdjustments: mockReturnAdjustments,
        returnTypes: mockReturnTypes,
        returnReasons: mockReturnReasons,
        partyOrders: mockPartyOrders,
        returnableItems: mockReturnableItems,
        isFetching: false,
        handleLoadOrderItems,
        handleRemoveItem,
        handleRemoveAdjustment,
        handleAcceptReturn,
        handleAddItem,
        handleAddAdjustment,
    };
};