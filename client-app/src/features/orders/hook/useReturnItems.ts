import { PartyOrder, ReturnItem, ReturnAdjustment, ReturnType, ReturnReason } from '../../../app/models/order/return';
import {
  useFetchReturnableItemsQuery,
  useGetPartyOrdersQuery,
  useGetReturnableItemsQuery
} from "../../../app/store/apis";

// REFACTOR: Define useReturnItems as an arrow function
// Purpose: Align with preferred arrow function syntax for hooks
// Why: Matches user preference while maintaining functionality
interface UseReturnItemsProps {
  returnId: string;
  orderId?: string;
  partyId?: string;
  roleTypeId?: string;
}

const useReturnItems = ({ returnId, orderId, partyId, roleTypeId }: UseReturnItemsProps) => {
  const isRtkImplemented = true; // Set to false to use mock partyOrders for testing
console.log('orderId from hook', orderId)
  // Purpose: Retrieve orders for the party based on returnId
  const { data: partyOrders, isFetching: isFetchingPartyOrders, error: partyOrdersError } = useGetPartyOrdersQuery(
    { returnId },
    { skip: !returnId } // Skip query if dependencies are missing
  );

  // Purpose: Load returnable items only when handleLoadOrderItems is called
  // Why: Prevents unnecessary queries until orderId is selected
  const { data: returnableItemsResult, isFetching: isFetchingReturnableItems, error: returnableItemsError } = useGetReturnableItemsQuery(
      orderId || '',
      { skip: !orderId }
  );

  // Mock data for other fields
  const mockReturnItems: ReturnItem[] = [
    {
      returnId,
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
      returnId,
      returnAdjustmentId: 'adj_1',
      amount: 5.00,
      description: 'Mock Adjustment',
      returnTypeId: 'type_1',
    },
  ];

  const mockReturnTypes: ReturnType[] = [
    { returnTypeId: 'type_1', description: 'Refund' },
    { returnTypeId: 'type_2', description: 'Replacement' },
  ];

  const mockReturnReasons: ReturnReason[] = [
    { returnReasonId: 'reason_1', description: 'Defective' },
    { returnReasonId: 'reason_2', description: 'Wrong Item' },
  ];

  const mockReturnableItems: any[] = [
    {
      orderId: 'order_1',
      orderItemSeqId: '001',
      productId: 'prod_1',
      description: 'Mock Product 1',
      returnableQuantity: 5,
    },
  ];

  // REFACTOR: Explicitly type mock partyOrders to match PartyOrder[]
  // Purpose: Ensure type safety and alignment with useGetPartyOrdersQuery
  // Why: Prevents TypeScript errors and ensures consistency
  const mockPartyOrders: PartyOrder[] = [
    {
      orderId: 'order_1',
      orderDate: '2025-07-20T12:00:00Z',
      partyId: 'customer_1',
      roleTypeId: 'PLACING_CUSTOMER',
      orderTypeId: 'SALES_ORDER',
      statusId: 'ORDER_COMPLETED',
      productId: 'prod_1',
      quantity: 10,
      unitPrice: 10.99,
      itemDescription: 'Mock Product 1',
      orderItemSeqId: '001',
    },
    {
      orderId: 'order_2',
      orderDate: '2025-07-21T12:00:00Z',
      partyId: 'vendor_1',
      roleTypeId: 'BILL_FROM_VENDOR',
      orderTypeId: 'PURCHASE_ORDER',
      statusId: 'ORDER_COMPLETED',
      productId: 'prod_2',
      quantity: 20,
      unitPrice: 15.99,
      itemDescription: 'Mock Product 2',
      orderItemSeqId: '002',
    },
  ];

  // REFACTOR: Return partyOrders from RTK Query or mock data
  // Purpose: Provide flexibility to toggle between real and mock data
  // Why: Supports testing while transitioning to RTK Query
  return {
    returnItems: mockReturnItems,
    returnAdjustments: mockReturnAdjustments,
    returnTypes: mockReturnTypes,
    returnReasons: mockReturnReasons,
    partyOrders: isRtkImplemented ? (partyOrders || []) : mockPartyOrders,
    returnableItems: isRtkImplemented ? (returnableItemsResult?.ReturnableItems || []) : mockReturnableItems,

    isFetching: isFetchingPartyOrders,
    error: partyOrdersError,
    handleLoadOrderItems: async (selectedOrderId: string) => {
      console.log(`Mock: Loading items for order ${selectedOrderId}`);
      return true;
    },
    handleRemoveItem: async (returnItemSeqId: string) => {
      console.log(`Mock: Removing item ${returnItemSeqId}`);
    },
    handleRemoveAdjustment: async (returnAdjustmentId: string) => {
      console.log(`Mock: Removing adjustment ${returnAdjustmentId}`);
    },
    handleAcceptReturn: async () => {
      console.log('Mock: Accepting return');
    },
    handleAddItem: async (item: ReturnItem) => {
      console.log('Mock: Adding item', item);
    },
    handleAddAdjustment: async (adjustment: ReturnAdjustment) => {
      console.log('Mock: Adding adjustment', adjustment);
    },
  };
};

export default useReturnItems;