import {
    useAppDispatch,
    useAppSelector,
    useFetchApprovedPurchaseOrdersQuery, useFetchFacilitiesQuery,
} from "../../../app/store/configureStore";
import React, {useCallback, useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";

import {Button, Grid, Paper} from "@mui/material";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import {OrderItem} from "../../../app/models/order/orderItem";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {requiredValidator} from "../../../app/common/form/Validators";
import {
    MemoizedFormDropDownListApprovedPurchaseOrders
} from "../../../app/common/form/MemoizedFormDropDownListApprovedPurchaseOrders";
import FacilityMenu from "../menu/FacilityMenu";
import {resetUiOrderItems} from "../../orders/slice/orderItemsUiSlice";
import {setSelectedApprovedPurchaseOrder} from "../../orders/slice/sharedOrderUiSlice";
import {useFetchPurchaseOrderItemsForReceiveQuery, useReceiveInventoryProductsMutation} from "../../../app/store/apis";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import ReceiveInventoryItemForm from "../form/ReceiveInventoryItemForm";
import {MemoizedFormDropDownList2} from "../../../app/common/form/MemoizedFormDropDownList2";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {toast} from "react-toastify";
import {ReceiveInventoryItems} from "../../../app/models/order/ReceiveInventoryItems";
import {handleDatesArray} from "../../../app/util/utils";
import {FormComboBoxVirtualSupplier} from "../../../app/common/form/FormComboBoxVirtualSupplier";

const ReceiveInventoryList = () => {
    const dispatch = useAppDispatch();
    const {getTranslatedLabel} = useTranslationHelper();
    const [show, setShow] = useState(false);
    const [selectedItem, setSelectedItem] = useState<any>(null);
    const [facilityId, setFacilityId] = useState<string | null>(null); // State for selected facilityId
    const [receiveInventoryProduct, {isLoading: isMutationLoading}] = useReceiveInventoryProductsMutation();
    const [selectedPartyId, setSelectedPartyId] = useState<string | null>(null);

    const ReceiveInventoryCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: "blue"}}
                colSpan={props.colSpan}
                role={"gridcell"}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{
                    [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
                }}
                {...navigationAttributes}
            >
                <Button onClick={() => handleSelectItemToReceive(props.dataItem)}>
                    {getTranslatedLabel("facility.receive.list.receive", "Edit")}
                </Button>
            </td>
        );
    };

    const handleSelectItemToReceive = (orderItem: any) => {
        const selected = orderItemsData?.find(
            (ii: any) => ii.orderItemSeqId === orderItem.orderItemSeqId
        );
        console.log("selected", selected)
        if (selected) {
            setSelectedItem(selected);
            setShow(true);
        }
    };

    const {selectedApprovedPurchaseOrder} = useAppSelector(
        (state) => state.sharedOrderUi
    );

    const {data: approvedPurchaseOrders, isLoading: isPurchaseOrdersLoading} =
        useFetchApprovedPurchaseOrdersQuery(
            {partyId: selectedPartyId},
            {skip: !selectedPartyId}
        );

    const {data: facilityList} = useFetchFacilitiesQuery(undefined);


    const {data: orderItems, isLoading} = useFetchPurchaseOrderItemsForReceiveQuery(
        {
            purchaseOrderId: selectedApprovedPurchaseOrder?.orderId,
            facilityId: facilityId, // Use the updated facilityId state
        },
        {
            skip: !selectedApprovedPurchaseOrder?.orderId || !facilityId, // Skip if either purchaseOrderId or facilityId is unavailable
        }
    );

    console.log('orderItems - ReceiveInventoryList.tsx', orderItems)


    const [orderItemsData, setOrderItemsData] = useState<OrderItem[] | undefined>([]);

    console.log('orderItemsData - ReceiveInventoryList.tsx', orderItemsData)
    useEffect(() => {
        // Cleanup function to clear the Redux slice when the component is unmounted
        return () => {
            dispatch(setSelectedApprovedPurchaseOrder(undefined));
            dispatch(resetUiOrderItems());
        };
    }, [dispatch]); // Empty dependency array ensures this effect runs only once, on mount and unmount

    React.useEffect(() => {
        if (orderItems) {
            const modifiedOrderItems = orderItems.purchaseOrderItems.map((item: OrderItem) => {
                return {
                    ...item,
                    includeThisItem: true,
                    quantityAccepted: item.quantity
                }
            })
            setOrderItemsData(modifiedOrderItems);
        }
    }, [orderItems]);

    const memoizedOnClose = useCallback(() => {
        setShow(false);
    }, []);

    console.log('facilityId', facilityId)

    const handleSubmit = (data: any) => {
        console.log(data);

        setOrderItemsData((prev) => {
            return prev?.map((o: any) => {
                if (o.orderId.concat(o.orderItemSeqId) === data.orderId.concat(data.orderItemSeqId)) {
                    return {
                        ...o,
                        ...data
                    }
                } else {
                    return o
                }
            })
        })
    };

    const handleFacilityChange = (e: string) => {
        setFacilityId(e.value || null); // Update state with selected facilityId
    };

    const IncludeCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: props.dataItem.includeThisItem ? 'green' : "red"}}
                colSpan={props.colSpan}
                role={'gridcell'}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{
                    [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex
                }}
                {...navigationAttributes}
            >
                {props.dataItem.includeThisItem ? "Yes" : "No"}
            </td>
        )
    }

    async function handleMenuSelect(e: MenuSelectEvent) {
        if (e.item.text === 'Receive Products') {
            handleReceiveProducts()
        }
    }

    const handleReceiveProducts = async () => {
        const itemsToReceive = [...orderItemsData!.filter((o: OrderItem) => {
            if (o.includeThisItem) return o
        })]

        // Transform data to match backend expectations
        const payload: ReceiveInventoryItems = {
            orderItems: itemsToReceive.map((item) => ({
                orderId: item.orderId,
                orderItemSeqId: item.orderItemSeqId,
                productId: item.productId,
                facilityId: facilityId,
                quantity: item.quantity,
                unitPrice: item.unitPrice,
                quantityAccepted: item.defaultQuantityToReceive,
                quantityRejected: item.quantityRejected || 0,
                rejectionReasonId: item.rejectionReasonId,
                color: item.productFeatureId
            })),
        };

        if (payload.orderItems?.length > 0) {
            try {
                const res = await receiveInventoryProduct(payload).unwrap();
                console.log(res);
                toast.success("Products received successfully.");
            } catch (e) {
                console.error(e);
                toast.error("Something went wrong while receiving products.");
            }
        } else {
            toast.error("No items were included to be received, select at least one.");
        }
    }

// Check if receivedItems exist and are not empty
    const receivedItemsData = orderItems?.receivedItems ? handleDatesArray(orderItems?.receivedItems) : [];
    const hasReceivedItems = receivedItemsData && receivedItemsData.length > 0;

    const handleSupplierChange = useCallback((event) => {
        setSelectedPartyId(event.value?.fromPartyId || null);
        dispatch(setSelectedApprovedPurchaseOrder(undefined)); // Clear selected purchase order
        setOrderItemsData([]); // Reset order items
        setFacilityId(null); // Reset facility to ensure valid selection
    }, [dispatch]);

    return (
        <>
            {show && (
                <ModalContainer show={show} onClose={memoizedOnClose} width={950}>
                    <ReceiveInventoryItemForm
                        orderItem={selectedItem}
                        onClose={memoizedOnClose}
                        handleSubmit={handleSubmit}
                    />
                </ModalContainer>
            )}
            <FacilityMenu/>
            {/* sx={{width: '90vw'}} */}
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={11}>
                        <Form
                            render={() => (
                                <FormElement>
                                    <fieldset className={"k-form-fieldset"}>
                                        <Grid container spacing={2} sx={{marginBottom: 2}} alignItems={"flex-end"}>
                                            <Grid item xs={2}>
                                                <Field
                                                    id="fromPartyId"
                                                    name="fromPartyId"
                                                    label={getTranslatedLabel("facility.receive.list.supplier", "Customer *")}
                                                    component={FormComboBoxVirtualSupplier}
                                                    autoComplete="off"
                                                    onChange={handleSupplierChange}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Field
                                                    id={"orderId"}
                                                    name={"orderId"}
                                                    label={getTranslatedLabel(
                                                        "facility.receive.list.select",
                                                        "Select Purchase Order *"
                                                    )}
                                                    component={
                                                        MemoizedFormDropDownListApprovedPurchaseOrders
                                                    }
                                                    dataItemKey={"orderId"}
                                                    textField={"orderDescription"}
                                                    data={
                                                        approvedPurchaseOrders ? approvedPurchaseOrders : []
                                                    }
                                                    validator={requiredValidator}
                                                    disabled={isPurchaseOrdersLoading}
                                                />
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Field
                                                    id={"facilityId"}
                                                    name={"facilityId"}
                                                    label={getTranslatedLabel(
                                                        "facility.receive.list.facility",
                                                        "Facility"
                                                    )}
                                                    component={MemoizedFormDropDownList2}
                                                    data={facilityList ?? []}
                                                    dataItemKey={'facilityId'}
                                                    textField={'facilityName'}
                                                    autoComplete={"off"}
                                                    onChange={handleFacilityChange}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                          <Grid item xs={2}>
                                          </Grid>
                                          <Grid item xs={2}>
                                            <Button
                                                variant="contained"
                                                onClick={handleReceiveProducts}
                                                disabled={!orderItemsData || orderItemsData.length === 0}
                                            >
                                              {getTranslatedLabel("facility.receive.list.ReceiveProduct", "Receive Products")}
                                            </Button>
                                          </Grid>
                                          
                                        </Grid>
                                    </fieldset>
                                </FormElement>
                            )}
                        />
                    </Grid>


                    {/* If we have receivedItems, show a grid above the main grid */}
                    {hasReceivedItems && (
                        <Grid item xs={10} style={{marginBottom: "20px"}}>
                            <h3>Previously Received Items</h3>
                            <KendoGrid data={receivedItemsData ?? []} resizable={true} reorderable={true}>
                                <Column field="shipmentId" title="Shipment ID" width={100}/>
                                <Column field="receiptId" title="Receipt" width={100}/>
                                <Column field="datetimeReceived" title="Date" width={150} format="{0: dd/MM/yyyy}"/>
                                <Column field="orderId" title="PO" width={100}/>
                                <Column field="orderItemSeqId" title="Line" width={100}/>
                                <Column field="productName" title="Product ID" width={150}/>
                                <Column field="lotId" title="Lot ID" width={100}/>
                                <Column field="unitPrice" title="Per Unit Price" width={120}/>
                                <Column field="quantityRejected" title="Rejected" width={100}/>
                                <Column field="quantityAccepted" title="Accepted" width={100}/>
                            </KendoGrid>
                        </Grid>)}

                        
                        

                    <Grid item xs={12}>
                        <KendoGrid
                            data={orderItemsData ?? []}
                            resizable={true}
                            reorderable={true}
                        >
                            <Column
                                field="orderItemSeqId"
                                title={getTranslatedLabel(
                                    "facility.receive.list.orderItem",
                                    "Order Item"
                                )}
                                width={100}
                                editable={false}
                                locked={true}
                            />
                            <Column
                                field="productName"
                                title={getTranslatedLabel(
                                    "facility.receive.list.product",
                                    "Product"
                                )}
                                width={200}
                                editable={false}
                            />
                            <Column
                                field="quantity"
                                title={getTranslatedLabel(
                                    "facility.receive.list.qtyOrdered",
                                    "Quantity ordered"
                                )}
                                width={100}
                                editable={false}
                            />
                            <Column
                                field="defaultQuantityToReceive"
                                title={getTranslatedLabel(
                                    "facility.receive.list.qtyAccept",
                                    "Quantity to accept"
                                )}
                                width={100}
                            />
                            <Column
                                field="unitPrice"
                                title={getTranslatedLabel(
                                    "facility.receive.list.price",
                                    "Unit Price"
                                )}
                                width={100}
                            />
                            <Column
                                field="quantityRejected"
                                title={getTranslatedLabel(
                                    "facility.receive.list.qtyReject",
                                    "Quantity to reject"
                                )}
                                width={100}
                                editor={"numeric"}
                            />
                            <Column
                                field="rejectionDescription"
                                title={getTranslatedLabel(
                                    "facility.receive.list.rejectReason",
                                    "Rejection reason"
                                )}
                                width={180}
                            />
                            <Column
                                field="includeThisItem"
                                title={getTranslatedLabel(
                                    "facility.receive.list.include",
                                    "Include Item"
                                )}
                                width={100}
                                cell={IncludeCell}
                            />

                            <Column width={100} cell={ReceiveInventoryCell}/>
                        </KendoGrid>
                    </Grid>
                </Grid>
                {isLoading && (
                    <LoadingComponent message="Loading Order..."/>
                )}
                {isMutationLoading && (
                    <LoadingComponent message="Processing Order..."/>
                )}
            </Paper>
        </>
    );
};

export default ReceiveInventoryList;
