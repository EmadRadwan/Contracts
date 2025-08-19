import React, { useEffect, useState, useRef } from "react";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Box, Grid, Paper, Typography, Button } from "@mui/material";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import { Grid as KendoGrid, GridColumn, GridCellProps } from "@progress/kendo-react-grid";
import { useAppDispatch } from "../../../../app/store/configureStore";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { Return, ReturnItem, ReturnAdjustment } from "../../../../app/models/order/return";
import useReturn from "../../hook/useReturn";
import useReturnItems from "../../hook/useReturnItems";
import { FormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { requiredValidator } from "../../../../app/common/form/Validators";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import OrderMenu from "../../menu/OrderMenu";
import ReturnsMenu from "../../menu/ReturnsMenu";

interface Props {
    returnId: string;
    orderId?: string;
    selectedReturn?: Return;
    editMode: number;
    cancelEdit: () => void;
    handleNewReturn: () => void;
}

interface ReturnRow {
    type: "item" | "adjustment";
    item?: ReturnItem;
    adjustment?: ReturnAdjustment;
    index: number;
}

export default function OrderReturnItems({ returnId, orderId, selectedReturn, editMode, cancelEdit, handleNewReturn }: Props) {
    const formRef = useRef<Form>(null);
    const loadItemsFormRef = useRef<Form>(null);
    const [selectedMenuItem, setSelectedMenuItem] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();

    const { returnHeader, handleCreate } = useReturn({
        selectedMenuItem,
        editMode,
        selectedReturn,
        setIsLoading,
    });

    const {
        returnItems,
        returnAdjustments,
        returnTypes,
        returnReasons,
        partyOrders,
        returnableItems,
        isFetching,
        handleLoadOrderItems,
        handleRemoveItem,
        handleRemoveAdjustment,
        handleAcceptReturn,
    } = useReturnItems({
        returnId,
        orderId,
        partyId: returnHeader?.returnHeaderTypeId === "VENDOR_RETURN" ? returnHeader.toPartyId : returnHeader.fromPartyId,
        roleTypeId: returnHeader?.returnHeaderTypeId === "VENDOR_RETURN" ? "BILL_FROM_VENDOR" : "PLACING_CUSTOMER",
    });

    const [formInitialValues, setFormInitialValues] = useState({
        orderId: orderId || "",
    });

    const readOnly = returnHeader?.statusId !== "RETURN_REQUESTED" && returnHeader?.statusId !== "SUP_RETURN_REQUESTED";

    // Refactored: Combine return items and adjustments for Kendo Grid
    const gridData: ReturnRow[] = [
        ...(returnItems?.map((item, index) => ({ type: "item" as const, item, index })) || []),
        ...(returnAdjustments?.map((adjustment, index) => ({ type: "adjustment" as const, adjustment, index: returnItems?.length + index })) || []),
    ];

    // Refactored: Calculate return total for display
    const returnTotal = returnItems?.reduce((total, item) => {
        const itemSubTotal = (item.returnQuantity || 0) * (item.returnPrice || 0);
        return total + itemSubTotal;
    }, 0) + (returnAdjustments?.reduce((total, adj) => total + (adj.amount || 0), 0) || 0) || 0;

    // Refactored: Initialize menu items based on return status
    const handleMenuSelect = async (e: MenuSelectEvent) => {
        setSelectedMenuItem(e.item.text);
        if (e.item.text === "New Return") {
            handleNewReturn();
        } else if (e.item.text === "Accept Return") {
            setIsLoading(true);
            await handleAcceptReturn();
            setIsLoading(false);
        }
    };

    // Refactored: Handle form submission for updating return items and adjustments
    const handleSubmit = async (data: any) => {
        if (!data.isValid) return false;
        setIsLoading(true);
        const action = selectedMenuItem || "Update Return";
        await handleCreate(data.values, action);
        setIsLoading(false);
    };

    // Refactored: Handle loading order items
    const handleLoadItemsSubmit = async (data: any) => {
        if (!data.isValid) return false;
        setIsLoading(true);
        await handleLoadOrderItems(data.values.orderId);
        setIsLoading(false);
    };

    // Refactored: Custom cell for Order ID
    const OrderIdCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === "adjustment") return <td colSpan={2}></td>;
        const item = dataItem.item!;
        return (
            <td>
                <a href={`/orderview?orderId=${item.orderId}`} className="buttontext">{item.orderId} - {item.orderItemSeqId || "N/A"}</a>
                <input type="hidden" name={`orderId_o_${dataItem.index}`} value={item.orderId} />
                <input type="hidden" name={`returnId_o_${dataItem.index}`} value={item.returnId} />
                <input type="hidden" name={`returnItemTypeId_o_${dataItem.index}`} value={item.returnItemTypeId} />
                <input type="hidden" name={`returnItemSeqId_o_${dataItem.index}`} value={item.returnItemSeqId} />
                <input type="hidden" name={`_rowSubmit_o_${dataItem.index}`} value="Y" />
            </td>
        );
    };

    // Refactored: Custom cell for Product ID
    const ProductIdCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === "adjustment") return <td></td>;
        const item = dataItem.item!;
        return (
            <td>
                {item.productId ? (
                    <a href={`/catalog/control/EditProductInventoryItems?productId=${item.productId}`} className="buttontext">{item.productId}</a>
                ) : "N/A"}
            </td>
        );
    };

    // Refactored: Custom cell for Description
    const DescriptionCell = (props: GridCellProps) => {
        const { dataItem, field } = props;
        if (dataItem.type === "item") {
            const item = dataItem.item!;
            return (
                <td>
                    {readOnly ? (
                        item.description || "N/A"
                    ) : (
                        <Field
                            name={`returnItems[${dataItem.index}].${field}`}
                            component="input"
                            type="text"
                            size="15"
                            validator={requiredValidator}
                        />
                    )}
                </td>
            );
        } else {
            const adjustment = dataItem.adjustment!;
            return (
                <td colSpan={3} align="right">
                    <span>{adjustment.description || "N/A"}{adjustment.comments ? `: ${adjustment.comments}` : ""}</span>
                </td>
            );
        }
    };

    // Refactored: Custom cell for Quantity
    const QuantityCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === "adjustment") return <td></td>;
        const item = dataItem.item!;
        return (
            <td>
                {readOnly ? (
                    item.returnQuantity?.toString()
                ) : (
                    <Field
                        name={`returnItems[${dataItem.index}].returnQuantity`}
                        component="input"
                        type="text"
                        size="8"
                        validator={requiredValidator}
                    />
                )}
                {item.receivedQuantity && (
                    <>
                        <br />
                        {getTranslatedLabel("Order", "TotalQuantityReceive")}: {item.receivedQuantity}
                        {item.shipmentReceipts?.map((receipt, idx) => (
                            <div key={idx}>
                                {getTranslatedLabel("Order", "Qty")}: {receipt.quantityAccepted}, {receipt.datetimeReceived},
                                <a href={`/facility/control/EditInventoryItem?inventoryItemId=${receipt.inventoryItemId}`} className="buttontext">{receipt.inventoryItemId}</a>
                            </div>
                        ))}
                    </>
                )}
            </td>
        );
    };

    // Refactored: Custom cell for Price
    const PriceCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === "item") {
            const item = dataItem.item!;
            return (
                <td>
                    {readOnly ? (
                        item.returnPrice?.toFixed(2)
                    ) : (
                        <Field
                            name={`returnItems[${dataItem.index}].returnPrice`}
                            component="input"
                            type="text"
                            size="8"
                            validator={requiredValidator}
                        />
                    )}
                </td>
            );
        } else {
            const adjustment = dataItem.adjustment!;
            return (
                <td align="right">
                    {readOnly ? (
                        adjustment.amount?.toFixed(2)
                    ) : (
                        <Field
                            name={`returnAdjustments[${dataItem.index - (returnItems?.length || 0)}].amount`}
                            component="input"
                            type="text"
                            size="8"
                            validator={requiredValidator}
                        />
                    )}
                </td>
            );
        }
    };

    // Refactored: Custom cell for SubTotal
    const SubTotalCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === "adjustment") return <td></td>;
        const item = dataItem.item!;
        return (
            <td>
                {(item.returnQuantity && item.returnPrice) ? (item.returnQuantity * item.returnPrice).toFixed(2) : "N/A"}
            </td>
        );
    };

    // Refactored: Custom cell for Return Reason
    const ReturnReasonCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === "adjustment") return <td></td>;
        const item = dataItem.item!;
        return (
            <td>
                {readOnly ? (
                    item.returnReason?.description || "N/A"
                ) : (
                    <Field
                        name={`returnItems[${dataItem.index}].returnReasonId`}
                        component={FormDropDownList}
                        data={returnReasons}
                        dataItemKey="returnReasonId"
                        textField="description"
                        validator={requiredValidator}
                    />
                )}
            </td>
        );
    };

    // Refactored: Custom cell for Item Status
    const ItemStatusCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === "adjustment") return <td></td>;
        return <td>{dataItem.item!.status?.description || "N/A"}</td>;
    };

    // Refactored: Custom cell for Return Type
    const ReturnTypeCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (dataItem.type === "item") {
            const item = dataItem.item!;
            return (
                <td>
                    {readOnly ? (
                        item.returnType?.description || "N/A"
                    ) : (
                        <Field
                            name={`returnItems[${dataItem.index}].returnTypeId`}
                            component={FormDropDownList}
                            data={returnTypes}
                            dataItemKey="returnTypeId"
                            textField="description"
                            validator={requiredValidator}
                        />
                    )}
                </td>
            );
        } else {
            const adjustment = dataItem.adjustment!;
            return (
                <td>
                    {readOnly ? (
                        adjustment.returnType?.description || "N/A"
                    ) : (
                        <Field
                            name={`returnAdjustments[${dataItem.index - (returnItems?.length || 0)}].returnTypeId`}
                            component={FormDropDownList}
                            data={returnTypes}
                            dataItemKey="returnTypeId"
                            textField="description"
                            validator={requiredValidator}
                        />
                    )}
                </td>
            );
        }
    };

    // Refactored: Custom cell for Return Response
    const ReturnResponseCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (!readOnly || dataItem.type === "adjustment") return <td></td>;
        const item = dataItem.item!;
        return (
            <td>
                {item.returnItemResponse ? (
                    <>
                        {item.returnItemResponse.paymentId && (
                            <div>{getTranslatedLabel("Accounting", "Payment")} <a href={`/accounting/control/paymentOverview?paymentId=${item.returnItemResponse.paymentId}`} className="buttontext">{item.returnItemResponse.paymentId}</a></div>
                        )}
                        {item.returnItemResponse.replacementOrderId && (
                            <div>{getTranslatedLabel("Order", "Order")} <a href={`/orderview?orderId=${item.returnItemResponse.replacementOrderId}`} className="buttontext">{item.returnItemResponse.replacementOrderId}</a></div>
                        )}
                        {item.returnItemResponse.billingAccountId && (
                            <div>{getTranslatedLabel("Accounting", "AccountId")} <a href={`/accounting/control/EditBillingAccount?billingAccountId=${item.returnItemResponse.billingAccountId}`} className="buttontext">{item.returnItemResponse.billingAccountId}</a></div>
                        )}
                    </>
                ) : getTranslatedLabel("Common", "None")}
            </td>
        );
    };

    // Refactored: Custom cell for Remove actions
    const RemoveCell = (props: GridCellProps) => {
        const { dataItem } = props;
        if (readOnly) return <td></td>;
        return (
            <td align="right">
                <Button
                    onClick={() => dataItem.type === "item" ? handleRemoveItem(dataItem.item!.returnItemSeqId) : handleRemoveAdjustment(dataItem.adjustment!.returnAdjustmentId)}
                    color="error"
                    variant="contained"
                >
                    {getTranslatedLabel("Common", "Remove")}
                </Button>
            </td>
        );
    };

    return (
        <>
            <OrderMenu selectedMenuItem="/returns" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container>
                    <Grid container spacing={2}>
                        <Grid item xs={11}>
                            <Box display="flex" justifyContent="space-between" mt={2}>
                                <Typography color="black" sx={{ ml: 3 }} variant="h3">
                                    {getTranslatedLabel("Order", "OrderReturn")} #{returnId}
                                </Typography>
                            </Box>
                        </Grid>
                        <Grid item xs={1}>
                            <Menu onSelect={handleMenuSelect}>
                                <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                                    {(!readOnly && returnItems?.length > 0) && (
                                        <MenuItem text="Accept Return" />
                                    )}
                                    <MenuItem text="New Return" />
                                </MenuItem>
                            </Menu>
                        </Grid>
                    </Grid>
                </Grid>

                <div className="screenlet">
                    <div className="screenlet-title-bar">
                        <Typography variant="h3">{getTranslatedLabel("Order", "PageTitleReturnItems")}</Typography>
                    </div>
                    <div className="screenlet-body">
                        {isFetching ? (
                            <LoadingComponent message="Loading Return Items..." />
                        ) : (
                            <>
                                {/* Order Totals Section */}
                                {returnableItems && (
                                    <TableContainer>
                                        <Table>
                                            <TableBody>
                                                <TableRow>
                                                    <TableCell width="25%">{getTranslatedLabel("Order", "OrderTotal")}</TableCell>
                                                    <TableCell>{returnableItems.orderTotal?.toFixed(2)}</TableCell>
                                                </TableRow>
                                                <TableRow>
                                                    <TableCell>{getTranslatedLabel("Order", "AmountAlreadyCredited")}</TableCell>
                                                    <TableCell>{returnableItems.creditedTotal?.toFixed(2)}</TableCell>
                                                </TableRow>
                                                <TableRow>
                                                    <TableCell>{getTranslatedLabel("Order", "AmountAlreadyRefunded")}</TableCell>
                                                    <TableCell>{returnableItems.refundedTotal?.toFixed(2)}</TableCell>
                                                </TableRow>
                                            </TableBody>
                                        </Table>
                                    </TableContainer>
                                )}

                                {/* Kendo Grid for Return Items and Adjustments */}
                                <Form
                                    ref={formRef}
                                    initialValues={{ returnItems: returnItems || [], returnAdjustments: returnAdjustments || [] }}
                                    onSubmitClick={handleSubmit}
                                    render={() => (
                                        <FormElement>
                                            <input type="hidden" name="_useRowSubmit" value="Y" />
                                            <KendoGrid
                                                data={gridData}
                                                style={{ maxHeight: "400px", overflowY: "auto" }}
                                            >
                                                <GridColumn field="orderId" title={getTranslatedLabel("Order", "OrderItems")} cell={OrderIdCell} />
                                                <GridColumn field="productId" title={getTranslatedLabel("Product", "Product")} cell={ProductIdCell} />
                                                <GridColumn field="description" title={getTranslatedLabel("Common", "Description")} cell={DescriptionCell} />
                                                <GridColumn field="returnQuantity" title={getTranslatedLabel("Order", "Quantity")} cell={QuantityCell} />
                                                <GridColumn field="returnPrice" title={getTranslatedLabel("Order", "Price")} cell={PriceCell} />
                                                <GridColumn field="subTotal" title={getTranslatedLabel("Order", "SubTotal")} cell={SubTotalCell} />
                                                <GridColumn field="returnReasonId" title={getTranslatedLabel("Order", "ReturnReason")} cell={ReturnReasonCell} />
                                                <GridColumn field="statusId" title={getTranslatedLabel("Order", "ItemStatus")} cell={ItemStatusCell} />
                                                <GridColumn field="returnTypeId" title={getTranslatedLabel("Common", "Type")} cell={ReturnTypeCell} />
                                                {readOnly && <GridColumn field="returnResponse" title={getTranslatedLabel("Order", "ReturnResponse")} cell={ReturnResponseCell} />}
                                                <GridColumn title="" cell={RemoveCell} />
                                            </KendoGrid>
                                            <Box sx={{ mt: 2 }}>
                                                <Typography>{getTranslatedLabel("Order", "ReturnTotal")}: {returnTotal.toFixed(2)}</Typography>
                                                {!readOnly && returnItems?.length > 0 && (
                                                    <Box sx={{ textAlign: "right" }}>
                                                        <input type="hidden" name="returnId" value={returnId} />
                                                        <input type="hidden" name="_rowCount" value={gridData.length} />
                                                        <Button type="submit" color="primary" variant="contained">
                                                            {getTranslatedLabel("Common", "Update")}
                                                        </Button>
                                                    </Box>
                                                )}
                                            </Box>
                                        </FormElement>
                                    )}
                                />

                                {/* Load Order Items Form */}
                                {!orderId && !readOnly && (
                                    <Form
                                        ref={loadItemsFormRef}
                                        initialValues={formInitialValues}
                                        onSubmitClick={handleLoadItemsSubmit}
                                        render={() => (
                                            <FormElement>
                                                <Grid container spacing={2} sx={{ mt: 2 }}>
                                                    <Grid item xs={12}>
                                                        <Typography variant="h3">{getTranslatedLabel("Order", "ReturnItems")}</Typography>
                                                    </Grid>
                                                    {partyOrders?.length > 0 ? (
                                                        <Grid item xs={3}>
                                                            <Field
                                                                name="orderId"
                                                                label={getTranslatedLabel("Order", "OrderId")}
                                                                component={FormDropDownList}
                                                                data={partyOrders}
                                                                dataItemKey="orderId"
                                                                textField="orderDate"
                                                                validator={requiredValidator}
                                                            />
                                                        </Grid>
                                                    ) : (
                                                        <>
                                                            <Grid item xs={3}>
                                                                <Typography>{getTranslatedLabel("Order", "NoOrderFoundForParty")}: {returnHeader?.fromPartyId || returnHeader?.toPartyId}</Typography>
                                                            </Grid>
                                                            <Grid item xs={3}>
                                                                <Field
                                                                    name="orderId"
                                                                    label={getTranslatedLabel("Order", "OrderId")}
                                                                    component="input"
                                                                    type="text"
                                                                    size="20"
                                                                    validator={requiredValidator}
                                                                />
                                                            </Grid>
                                                        </>
                                                    )}
                                                    <Grid item xs={2}>
                                                        <Button type="submit" color="primary" variant="contained">
                                                            {getTranslatedLabel("Order", "ReturnLoadItems")}
                                                        </Button>
                                                    </Grid>
                                                </Grid>
                                            </FormElement>
                                        )}
                                    />
                                )}
                            </>
                        )}
                    </div>
                </div>
            </Paper>
        </>
    );
}