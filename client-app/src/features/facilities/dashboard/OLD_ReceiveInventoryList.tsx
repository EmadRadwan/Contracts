// noinspection ConditionalExpressionJS

import {
    useAppDispatch,
    useAppSelector,
    useFetchApprovedPurchaseOrdersQuery,
    useFetchFacilitiesQuery,
    useFetchPurchaseOrderItemsQuery,
    useFetchRejectionReasonsQuery,
    useReceiveInventoryProductsMutation,
} from "../../../app/store/configureStore";
import React, {createContext, useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridItemChangeEvent,
    GridRowProps,
} from "@progress/kendo-react-grid";

import {Grid, Paper} from "@mui/material";

import {OrderItem} from "../../../app/models/order/orderItem";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {requiredValidator} from "../../../app/common/form/Validators";
import {
    MemoizedFormDropDownListApprovedPurchaseOrders
} from "../../../app/common/form/MemoizedFormDropDownListApprovedPurchaseOrders";
import {CellRender, RowRender} from "./ReceiveInventoryList/Renderers";
import {RejectionReasonTypeDropDownCell} from "./ReceiveInventoryList/RejectionReasonTypeDropDownCell";
import {NumericTextBox} from "@progress/kendo-react-inputs";
import {Error} from "@progress/kendo-react-labels";
import FacilityMenu from "../menu/FacilityMenu";
import {ReceiveInventoryFacilityDropDown} from "./ReceiveInventoryList/ReceiveInventoryFacilityDropDown";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import {Popup} from "@progress/kendo-react-popup";
import {toast} from "react-toastify";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {resetUiOrderItems} from "../../orders/slice/orderItemsUiSlice";
import {setSelectedApprovedPurchaseOrder} from "../../orders/slice/sharedOrderUiSlice";
import {useFetchShipmentReceiptsQuery} from "../../../app/store/apis";
import {handleDatesArray} from "../../../app/util/utils";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

const EDIT_FIELD = "inEdit";

const QuantityAcceptedCell = (props: any) => {
    const anchor = React.useRef<HTMLTableCellElement | null>(null);
    const quantityReceived = props.dataItem['quantityAccepted'];
    const quantityOrdered = props.dataItem['quantity'];
    const quantityRejected = props.dataItem['quantityRejected']; // Access quantityRejected

    const isValid = quantityReceived >= 0 &&
        quantityReceived + quantityRejected <= quantityOrdered; // Modified validation


    return (
        <ReceiveInventoryContext.Consumer>
            {(context: any) => {
                return props.dataItem.inEdit === props.field ? (
                    <td ref={anchor} style={{border: isValid ? 'none' : '2px solid red'}}>
                        <NumericTextBox
                            required
                            value={
                                props.dataItem[props.field] === null
                                    ? ""
                                    : props.dataItem[props.field]
                            }
                            onChange={(e) => {
                                props.onChange({
                                    dataIndex: 0,
                                    dataItem: props.dataItem,
                                    field: props.field,
                                    syntheticEvent: e.syntheticEvent,
                                    value: e.value,
                                });
                            }}
                            name={props.field}
                            format="n0"
                            min={1}
                        />
                        {!isValid && (
                            <Popup
                                anchor={anchor.current as HTMLElement}
                                show={!isValid}
                                popupClass={"popup-content"}
                            >
                                <Error style={{fontSize: '16px'}}>Quantity Received + Quantity Rejected cannot exceed
                                    Quantity Ordered</Error>
                            </Popup>
                        )}
                    </td>
                ) : (
                    <td ref={anchor}
                        className={props.className}
                        style={{
                            ...props.style,
                            border: isValid ? 'none' : '2px solid red',
                        }}
                        onClick={() => context.enterEdit(props.dataItem, props.field)}
                    >
                        {props.dataItem[props.field]}
                        {!isValid && (
                            <Popup
                                anchor={anchor.current as HTMLElement}
                                show={!isValid}
                                popupClass={"popup-content"}
                            >
                                <Error style={{fontSize: '16px'}}>Quantity Received + Quantity Rejected cannot exceed
                                    Quantity Ordered</Error>
                            </Popup>
                        )}
                    </td>
                );
            }}
        </ReceiveInventoryContext.Consumer>
    );
};

const QuantityRejectedCell = (props: any) => {

    return (
        <ReceiveInventoryContext.Consumer>
            {(context: any) => {
                return props.dataItem.inEdit === props.field ? (
                    <td>
                        <NumericTextBox
                            required
                            value={
                                props.dataItem[props.field] === null
                                    ? ""
                                    : props.dataItem[props.field]
                            }
                            onChange={(e) => {
                                props.onChange({
                                    dataIndex: 0,
                                    dataItem: props.dataItem,
                                    field: props.field,
                                    syntheticEvent: e.syntheticEvent,
                                    value: e.value,
                                });
                            }}
                            name={props.field}
                            format="n0"
                            min={0} // Quantity rejected could be 0
                        />
                    </td>
                ) : (
                    <td
                        className={props.className}
                        onClick={() => context.enterEdit(props.dataItem, props.field)}
                    >
                        {props.dataItem[props.field]}
                    </td>
                );
            }}
        </ReceiveInventoryContext.Consumer>
    );
};


const ReceiveInventoryContext = createContext({
    enterEdit: (dataItem: any, field: any) => null,
    itemChange: (event: any) => null,
});


export default function OLD_ReceiveInventoryList() {
    //console.log('ReceiveInventoryList rendered');
    const dispatch = useAppDispatch();
    const {getTranslatedLabel} = useTranslationHelper()

    const [ReceiveInventoryProduct, {isLoading}] = useReceiveInventoryProductsMutation();

    const [hasValidationErrors, setHasValidationErrors] = useState(false);
    const [isInitialized, setIsInitialized] = useState(0);

    const validateGrid = (): boolean => {
        //console.log('Validating rendered');
        const newOrderItemsData = orderItemsData.map((item: any) => {
            let quantityReceived = item['quantityAccepted'] || 0;
            let quantityRejected = item['quantityRejected'] || 0;

            // Sum up quantities from existing shipment receipts
            shipmentReceipts.forEach((receipt: any) => {
                if (receipt.productId === item.productId) {
                    quantityReceived += receipt.quantityAccepted || 0;
                    quantityRejected += receipt.quantityRejected || 0;
                }
            });

            const quantityOrdered = item['quantity'];
            const rejectionId = item['rejectionId'];

            const isValid = (quantityReceived > 0 &&
                    quantityReceived <= (quantityOrdered - quantityRejected)) &&
                ((quantityRejected > 0 && rejectionId) || (!quantityRejected && (!rejectionId || rejectionId === '')));

            // Return a copy of the item with 'validItem' updated
            //console.log('order item in validation', item);
            return {...item, validItem: isValid};
        });

        // Update the state *once* with the modified array
        setOrderItemsData(newOrderItemsData);

        // Calculate and set hasErrors
        const hasErrors = newOrderItemsData.some(item => !item.validItem);
        setHasValidationErrors(hasErrors);

        return hasErrors; // Still return the overall error state
    };

    const {selectedApprovedPurchaseOrder} = useAppSelector(
        (state) => state.sharedOrderUi,
    );
    const {data: approvedPurchaseOrders} =
        useFetchApprovedPurchaseOrdersQuery(undefined);
    const {data: rejectionReasonsData} =
        useFetchRejectionReasonsQuery(undefined);
    const {data: facilityList} = useFetchFacilitiesQuery(undefined);

    const {data: orderItems} = useFetchPurchaseOrderItemsQuery(
        selectedApprovedPurchaseOrder?.orderId,
        {skip: selectedApprovedPurchaseOrder?.orderId === undefined},
    );

    const {data: shipmentReceiptsData} = useFetchShipmentReceiptsQuery(
        selectedApprovedPurchaseOrder?.orderId,
        {skip: selectedApprovedPurchaseOrder?.orderId === undefined},
    );

    //console.log('rejectionReasonsData', rejectionReasonsData);

    const [orderItemsData, setOrderItemsData] = useState<Array<OrderItem | undefined>>([]);
    const [shipmentReceipts, setShipmentReceipts] = useState<Array<any | undefined>>([]);

    useEffect(() => {
        // Cleanup function to clear the Redux slice when the component is unmounted
        return () => {
            dispatch(setSelectedApprovedPurchaseOrder(undefined));
            dispatch(resetUiOrderItems());
        };
    }, []); // Empty dependency array ensures this effect runs only once, on mount and unmount

    React.useEffect(() => {
        if (orderItems) {
            setOrderItemsData(orderItems);
        }
    }, [orderItems]);

    React.useEffect(() => {
        if (shipmentReceiptsData) {
            setShipmentReceipts(handleDatesArray(shipmentReceiptsData));
        }
    }, [shipmentReceiptsData]);

    useEffect(() => {
        const fetchData = async () => {
            if (isInitialized > 0 && !hasValidationErrors) {
                // create an Order and fill it with data from selectedApprovedPurchaseOrder and orderItemsData
                const order = {
                    orderId: selectedApprovedPurchaseOrder?.orderId,
                    orderItems: orderItemsData.map((item: OrderItem) => {
                        return {
                            orderId: item.orderId,
                            orderItemSeqId: item.orderItemSeqId,
                            productId: item.productId,
                            quantityAccepted: item.quantityAccepted,
                            quantityRejected: item.quantityRejected,
                            rejectionId: item.rejectionId,
                            facilityId: item.facilityId,
                            includeThisItem: item.includeThisItem,
                        };
                    }),
                };
                try {
                    const receivedProductResult = await ReceiveInventoryProduct(order).unwrap(); // Await the call
                    //console.log('receivedProductResult', receivedProductResult);
                    toast.success('Inventory Received Successfully');
                } catch (error) {
                    console.error('Error receiving inventory', error);
                    toast.error('Error receiving inventory');
                }
            } else if (isInitialized > 0 && hasValidationErrors) {
                toast.error('Check Validation Errors');
            }
        };

        fetchData();
    }, [ReceiveInventoryProduct, hasValidationErrors, isInitialized, orderItemsData, selectedApprovedPurchaseOrder]);

    const handleMenuSelect = (e: MenuSelectEvent) => {
        if (e.item.text === 'Receive Inventory') {
            validateGrid();
            // increase the initialized count
            setIsInitialized(isInitialized + 1);
        }
    };

    const itemChange = (event: GridItemChangeEvent) => {
        const field = event.field || "";
        const newData = orderItemsData.map((item: any) =>
            item.orderItemSeqId === event.dataItem.orderItemSeqId
                ? {...item, [field]: event.value}
                : item,
        );
        setOrderItemsData(newData);
        return null;
    };
    const enterEdit = (dataItem: OrderItem, field: string | undefined) => {
        const newData = orderItemsData.map((item: any) => ({
            ...item,
            [EDIT_FIELD]:
                item.orderItemSeqId === dataItem.orderItemSeqId ? field : undefined,
        }));

        setOrderItemsData(newData);
        return null;
    };

    const exitEdit = () => {
        const newData = orderItemsData.map((item: any) => ({
            ...item,
            [EDIT_FIELD]: undefined,
        }));

        setOrderItemsData(newData);
    };

    const customCellRender: any = (
        td: React.ReactElement<HTMLTableCellElement>,
        props: GridCellProps,
    ) => (
        <CellRender
            originalProps={props}
            td={td}
            enterEdit={enterEdit}
            editField={EDIT_FIELD}
        />
    );

    const customRowRender: any = (
        tr: React.ReactElement<HTMLTableRowElement>,
        props: GridRowProps,
    ) => (
        <RowRender
            originalProps={props}
            tr={tr}
            exitEdit={exitEdit}
            editField={EDIT_FIELD}
            isValid={props.dataItem.validItem}
        />
    );


    return (
        <>
            <FacilityMenu />
            {/* sx={{width: '90vw'}} */}
            <Paper elevation={5} className={`div-container-withBorderCurved`} >
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={11}>
                        <Form
                            render={(formRenderProps) => (
                                <FormElement>
                                    <fieldset className={"k-form-fieldset"}>
                                        <Grid container spacing={2} sx={{marginBottom: 2}}>
                                            <Grid item xs={3}>
                                                <Field
                                                    id={"orderId"}
                                                    name={"orderId"}
                                                    label={getTranslatedLabel("facility.receive.list.select", "Select Purchase Order *")}
                                                    component={
                                                        MemoizedFormDropDownListApprovedPurchaseOrders
                                                    }
                                                    dataItemKey={"orderId"}
                                                    textField={"orderDescription"}
                                                    data={
                                                        approvedPurchaseOrders
                                                            ? approvedPurchaseOrders
                                                            : []
                                                    }
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                        </Grid>
                                    </fieldset>
                                </FormElement>
                            )}
                        />
                    </Grid>
                    <Grid item xs={1}>
                        <Menu onSelect={handleMenuSelect}>
                            <MenuItem
                                text={getTranslatedLabel("general.actions", "Actions")}
                                disabled={selectedApprovedPurchaseOrder === undefined}
                            >
                                <MenuItem text={getTranslatedLabel("facility.receive.menu.receive", "Receive Inventory")} />
                            </MenuItem>
                        </Menu>
                    </Grid>
                    <Grid item xs={12}>

                        <ReceiveInventoryContext.Provider
                            value={{enterEdit: enterEdit, itemChange: itemChange}}
                        >
                            <KendoGrid
                                style={{height: shipmentReceipts && shipmentReceipts.length > 0 ? "30vh" : "50vh"}}
                                data={orderItemsData ? orderItemsData : []}
                                dataItemKey={"orderItemSeqId"}
                                rowHeight={47}
                                onItemChange={itemChange}
                                cellRender={customCellRender}
                                rowRender={customRowRender}
                                editField={EDIT_FIELD}
                                resizable={true}
                                reorderable={true}
                            >
                                <Column
                                    field="orderItemSeqId"
                                    title={getTranslatedLabel("facility.receive.list.orderItem", "Order Item")}
                                    width={100}
                                    editable={false}
                                    locked={true}
                                />
                                <Column
                                    field="productName"
                                    title={getTranslatedLabel("facility.receive.list.product", "Product")}
                                    width={200}
                                    editable={false}
                                />
                                <Column
                                    field="quantity"
                                    title={getTranslatedLabel("facility.receive.list.qtyOrdered", "Quantity ordered")}
                                    width={100}
                                    editable={false}
                                />
                                <Column
                                    field="quantityAccepted"
                                    title={getTranslatedLabel("facility.receive.list.qtyAccept", "Quantity acceptd")}
                                    width={100}
                                    cell={QuantityAcceptedCell}
                                />
                                <Column
                                    field="quantityRejected"
                                    title={getTranslatedLabel("facility.receive.list.qtyReject", "Quantity rejected")}
                                    width={100}
                                    editor={"numeric"}
                                    cell={QuantityRejectedCell}
                                />
                                <Column
                                    field="rejectionId"
                                    title={getTranslatedLabel("facility.receive.list.rejectReason", "Rejection reason")}
                                    width={180}
                                    cell={(props) => (
                                        <RejectionReasonTypeDropDownCell
                                            {...props}
                                            rejectionReasonTypesData={rejectionReasonsData}
                                            validateGrid={validateGrid}
                                        />
                                    )}
                                />
                                <Column
                                    field="facilityId"
                                    title={getTranslatedLabel("facility.receive.list.facility", "Receiving facility")}
                                    width={200}
                                    cell={(props) => (
                                        <ReceiveInventoryFacilityDropDown
                                            {...props}
                                            facilityList={facilityList}
                                        />
                                    )}
                                />

                                <Column field="includeThisItem" title={getTranslatedLabel("facility.receive.list.include", "Include this item")} editor="boolean" width={100}/>
                            </KendoGrid>
                        </ReceiveInventoryContext.Provider>
                    </Grid>
                    {shipmentReceipts && shipmentReceipts.length > 0 && (
                        <Grid item xs={12}>
                            <KendoGrid
                                data={shipmentReceipts ? shipmentReceipts : []}
                                total={shipmentReceipts ? shipmentReceipts.length : 0}
                                pageable={true}
                                style={{height: "20vh"}}
                            >

                                <Column field="receiptId" title="Receipt Id" width={300}/>
                                <Column field="productName" title="Product" width={300}/>
                                <Column field="quantityAccepted" title="Accepted" width={100}/>
                                <Column field="quantityRejected" title="Accepted" width={100}/>
                                <Column field="datetimeReceived" title="Date Received" width={150}
                                        format="{0: dd/MM/yyyy HH}"/>


                            </KendoGrid>
                        </Grid>
                    )}
                </Grid>
                {isLoading && (
                    <LoadingComponent message="Receiving Products..."/>
                )}
            </Paper>
        </>
    );
}

