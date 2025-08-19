// src/features/orders/return/components/ReturnGrid.tsx
import React from 'react';
import { Grid as KendoGrid, GridColumn, GridCellProps, GRID_COL_INDEX_ATTRIBUTE } from '@progress/kendo-react-grid';
import { useTableKeyboardNavigation } from '@progress/kendo-react-data-tools';
import { Button, Box } from '@mui/material';
import { GetTranslatedLabel } from '../../../../app/hooks/useTranslationHelper';
import { ReturnRow } from '../OrderReturnItems';

// REFACTOR: Extract grid and cell rendering into a separate component
// Purpose: Isolate grid logic for better maintainability
// Why: Reduces complexity in main component and groups cell logic
interface ReturnGridProps {
    gridData: ReturnRow[];
    readOnly: boolean;
    handleSelectRow: (row: ReturnRow) => void;
    handleRemoveRow: (row: ReturnRow) => void;
    getTranslatedLabel: GetTranslatedLabel;
}

export const ReturnGrid: React.FC<ReturnGridProps> = ({
                                                          gridData,
                                                          readOnly,
                                                          handleSelectRow,
                                                          handleRemoveRow,
                                                          getTranslatedLabel,
                                                      }) => {
    const OrderIdCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === 'adjustment') return <td colSpan={2}></td>;
        const item = dataItem.item!;
        return (
            <td>
                <a href={`/orderview?orderId=${item.orderId}`} className="buttontext">
                    {item.orderId} - {item.orderItemSeqId || 'N/A'}
                </a>
                <input type="hidden" name={`orderId_o_${dataItem.index}`} value={item.orderId} />
                <input type="hidden" name={`returnId_o_${dataItem.index}`} value={item.returnId} />
                <input type="hidden" name={`returnItemTypeId_o_${dataItem.index}`} value={item.returnItemTypeId} />
                <input type="hidden" name={`returnItemSeqId_o_${dataItem.index}`} value={item.returnItemSeqId} />
                <input type="hidden" name={`_rowSubmit_o_${dataItem.index}`} value="Y" />
            </td>
        );
    };

    const ProductIdCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === 'adjustment') return <td></td>;
        const item = dataItem.item!;
        return (
            <td>
                {item.productId ? (
                    <a href={`/catalog/control/EditProductInventoryItems?productId=${item.productId}`} className="buttontext">
                        {item.productId}
                    </a>
                ) : (
                    'N/A'
                )}
            </td>
        );
    };

    const DescriptionCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === 'item') {
            const item = dataItem.item!;
            return <td>{item.description || 'N/A'}</td>;
        } else {
            const adjustment = dataItem.adjustment!;
            return (
                <td colSpan={3} align="right">
          <span>
            {adjustment.description || 'N/A'}
              {adjustment.comments ? `: ${adjustment.comments}` : ''}
          </span>
                </td>
            );
        }
    };

    const QuantityCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === 'adjustment') return <td></td>;
        const item = dataItem.item!;
        return (
            <td>
                {item.returnQuantity?.toString()}
                {item.receivedQuantity && (
                    <>
                        <br />
                        {getTranslatedLabel('Order', 'TotalQuantityReceive')}: {item.receivedQuantity}
                        {item.shipmentReceipts?.map((receipt, idx) => (
                            <div key={idx}>
                                {getTranslatedLabel('Order', 'Qty')}: {receipt.quantityAccepted}, {receipt.datetimeReceived},
                                <a
                                    href={`/facility/control/EditInventoryItem?inventoryItemId=${receipt.inventoryItemId}`}
                                    className="buttontext"
                                >
                                    {receipt.inventoryItemId}
                                </a>
                            </div>
                        ))}
                    </>
                )}
            </td>
        );
    };

    const PriceCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === 'item') {
            const item = dataItem.item!;
            return <td>{item.returnPrice?.toFixed(2)}</td>;
        } else {
            const adjustment = dataItem.adjustment!;
            return <td align="right">{adjustment.amount?.toFixed(2)}</td>;
        }
    };

    const SubTotalCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === 'adjustment') return <td></td>;
        const item = dataItem.item!;
        return (
            <td>
                {(item.returnQuantity && item.returnPrice) ? (item.returnQuantity * item.returnPrice).toFixed(2) : 'N/A'}
            </td>
        );
    };

    const ReturnReasonCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === 'adjustment') return <td></td>;
        const item = dataItem.item!;
        return <td>{item.returnReason?.description || 'N/A'}</td>;
    };

    const ItemStatusCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === 'adjustment') return <td></td>;
        return <td>{dataItem.item!.status?.description || 'N/A'}</td>;
    };

    const ReturnTypeCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === 'item') {
            const item = dataItem.item!;
            return <td>{item.returnType?.description || 'N/A'}</td>;
        } else {
            const adjustment = dataItem.adjustment!;
            return <td>{adjustment.returnType?.description || 'N/A'}</td>;
        }
    };

    const ReturnResponseCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (!readOnly || dataItem.type === 'adjustment') return <td></td>;
        const item = dataItem.item!;
        return (
            <td>
                {item.returnItemResponse ? (
                    <>
                        {item.returnItemResponse.paymentId && (
                            <div>
                                {getTranslatedLabel('Accounting', 'Payment')}{' '}
                                <a
                                    href={`/accounting/control/paymentOverview?paymentId=${item.returnItemResponse.paymentId}`}
                                    className="buttontext"
                                >
                                    {item.returnItemResponse.paymentId}
                                </a>
                            </div>
                        )}
                        {item.returnItemResponse.replacementOrderId && (
                            <div>
                                {getTranslatedLabel('Order', 'Order')}{' '}
                                <a
                                    href={`/orderview?orderId=${item.returnItemResponse.replacementOrderId}`}
                                    className="buttontext"
                                >
                                    {item.returnItemResponse.replacementOrderId}
                                </a>
                            </div>
                        )}
                        {item.returnItemResponse.billingAccountId && (
                            <div>
                                {getTranslatedLabel('Accounting', 'AccountId')}{' '}
                                <a
                                    href={`/accounting/control/EditBillingAccount?billingAccountId=${item.returnItemResponse.billingAccountId}`}
                                    className="buttontext"
                                >
                                    {item.returnItemResponse.billingAccountId}
                                </a>
                            </div>
                        )}
                    </>
                ) : (
                    getTranslatedLabel('Common', 'None')
                )}
            </td>
        );
    };

    const EditCell = (props: GridCellProps) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        if (readOnly) return <td />;
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: 'blue' }}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button onClick={() => handleSelectRow(props.dataItem)}>
                    {getTranslatedLabel('Common', 'Edit')}
                </Button>
            </td>
        );
    };

    const RemoveCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (readOnly) return <td></td>;
        return (
            <td align="right">
                <Button onClick={() => handleRemoveRow(dataItem)} color="error" variant="contained">
                    {getTranslatedLabel('Common', 'Remove')}
                </Button>
            </td>
        );
    };

    return (
        <KendoGrid data={gridData} style={{ maxHeight: '400px', overflowY: 'auto' }} resizable={true} reorderable={true}>
            <GridColumn field="orderId" title={getTranslatedLabel('Order', 'OrderItems')} cell={OrderIdCell} />
            <GridColumn field="productId" title={getTranslatedLabel('Product', 'Product')} cell={ProductIdCell} />
            <GridColumn field="description" title={getTranslatedLabel('Common', 'Description')} cell={DescriptionCell} />
            <GridColumn field="returnQuantity" title={getTranslatedLabel('Order', 'Quantity')} cell={QuantityCell} />
            <GridColumn field="returnPrice" title={getTranslatedLabel('Order', 'Price')} cell={PriceCell} />
            <GridColumn field="subTotal" title={getTranslatedLabel('Order', 'SubTotal')} cell={SubTotalCell} />
            <GridColumn field="returnReasonId" title={getTranslatedLabel('Order', 'ReturnReason')} cell={ReturnReasonCell} />
            <GridColumn field="statusId" title={getTranslatedLabel('Order', 'ItemStatus')} cell={ItemStatusCell} />
            <GridColumn field="returnTypeId" title={getTranslatedLabel('Common', 'Type')} cell={ReturnTypeCell} />
            {readOnly && <GridColumn field="returnResponse" title={getTranslatedLabel('Order', 'ReturnResponse')} cell={ReturnResponseCell} />}
            <GridColumn title="" cell={EditCell} />
            <GridColumn title="" cell={RemoveCell} />
        </KendoGrid>
    );
};