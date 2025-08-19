export interface ReturnableItem {
  orderId?: string;
  itemTypeKey?: string;
  itemType?: string;
  returnableQuantity?: number;
  returnablePrice?: number;
  orderItemSeqId?: string | null;
  productId?: string;
  itemDescription?: string;
  unitPrice?: number | null;
  statusId?: string;
  orderAdjustmentId?: string | null;
  returnAdjustmentTypeId?: string | null;
  description?: string | null;
  amount?: number | null;
  included?: boolean
  returnItemStatusId?: string
  returnTypeId?: string
  returnReasonId?: string
}
