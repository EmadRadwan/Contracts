import { useFetchOrdersQuery } from "../../../../app/store/apis";
import React, { useEffect, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import { DataResult, State } from '@progress/kendo-data-query';
import Button from "@mui/material/Button";
import { Grid, Paper } from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { handleDatesArray } from "../../../../app/util/utils";
import { Order } from "../../../../app/models/order/order";
import { useAppDispatch, useAppSelector } from "../../../../app/store/configureStore";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import SalesOrderForm from "../../form/order/SalesOrder/SalesOrderForm";
import PurchaseOrderForm from "../../form/order/PurchaseOrder/PurchaseOrderForm";
import { resetSharedOrderUi, setCurrentOrderType, setCustomerId, setSupplierId, setWhatWasClicked } from "../../slice/sharedOrderUiSlice";
import { setOrderFormEditMode } from "../../slice/ordersUiSlice";
import { ColumnMenuOrderTypeFilter } from "../ColumnMenu";
import { resetUiOrderItems, setUiOrderItems } from "../../slice/orderItemsUiSlice";
import { resetUiOrderAdjustments, setUiOrderAdjustments } from "../../slice/orderAdjustmentsUiSlice";
import AccountingMenu from "../../../accounting/invoice/menu/AccountingMenu";

interface OrdersListProps {
    orderType: 'SALES_ORDER' | 'PURCHASE_ORDER';
}

export default function OrdersList({ orderType }: OrdersListProps) {
    const [orders, setOrders] = React.useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = React.useState<State>({ take: 6, skip: 0 });
    const { whatWasClicked } = useAppSelector((state) => state.sharedOrderUi);
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };
    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();
    const location = useLocation();
    const navigate = useNavigate();
    const [shouldShowOrderForm, setShouldShowOrderForm] = useState(false);
    const [editMode, setEditMode] = useState(0);
    const [order, setOrder] = useState<Order | undefined>(undefined);
    const [localOrderType, setLocalOrderType] = useState(0);
    const [show, setShow] = useState(false);
    const { selectedOrder } = useAppSelector(state => state.accountingSharedUi);

    const { data, error, isFetching } = useFetchOrdersQuery({ ...dataState, orderType });

    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setOrders({ data: adjustedData, total: data.total });
        }
    }, [data]);

    useEffect(() => {
        if (location.state === null) {
            setOrder(undefined);
        }
    }, [location]);

    useEffect(() => {
        if (selectedOrder && orders?.data.length) {
            handleSelectOrder(selectedOrder.orderId);
        }
    }, [selectedOrder, orders]);

    function handleSelectOrder(orderId: string, orderStatus?: string, orderTypeId?: string, fromPartyId?: string) {
        dispatch(setWhatWasClicked('orderId'));
        const selectedOrder: Order | undefined = orders.data?.find((order: any) => order.orderId === orderId);
        const status = orderStatus ?? selectedOrder?.statusId;
        const party = fromPartyId ?? selectedOrder?.fromPartyId.fromPartyId;
        const type = orderTypeId ?? selectedOrder?.orderTypeId;
        setLocalOrderType(type === "SALES_ORDER" ? 1 : 2);
        dispatch(setCurrentOrderType(type));
        if (type === "SALES_ORDER") {
            dispatch(setCustomerId(party));
        } else {
            dispatch(setSupplierId(party));
        }
        setOrder(selectedOrder);
        setOrderStatus(status);
    }

    function setOrderStatus(orderStatus: string) {
        if (orderStatus === "ORDER_CREATED") {
            setEditMode(2);
            dispatch(setOrderFormEditMode(2));
        }
        if (orderStatus === "ORDER_APPROVED") {
            setEditMode(3);
            dispatch(setOrderFormEditMode(3));
        }
        if (orderStatus === "ORDER_COMPLETED") {
            setEditMode(4);
            dispatch(setOrderFormEditMode(4));
        }
    }

    const OrderDescriptionCell = (props: any) => {
        const field = props.field || '';
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: 'blue' }}
                colSpan={props.colSpan}
                role={'gridcell'}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button
                    onClick={() => {
                        handleSelectOrder(
                            props.dataItem.orderId,
                            props.dataItem.statusId,
                            props.dataItem.orderTypeId,
                            props.dataItem.fromPartyId.fromPartyId
                        );
                    }}
                >
                    {props.dataItem.orderId}
                </Button>
            </td>
        );
    };

    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setOrder(undefined);
        setShouldShowOrderForm(false);
    }, [setEditMode, setOrder, setShouldShowOrderForm]);

    if (shouldShowOrderForm) {
        return orderType === 'SALES_ORDER' ? (
            <SalesOrderForm selectedOrder={location.state?.order} cancelEdit={cancelEdit} editMode={2} />
        ) : (
            <PurchaseOrderForm selectedOrder={location.state?.order} cancelEdit={cancelEdit} editMode={2} />
        );
    }

    function handleCreateOrder() {
        dispatch(setWhatWasClicked('orderId'));
        dispatch(resetSharedOrderUi());
        dispatch(setUiOrderItems([]));
        dispatch(resetUiOrderItems());
        dispatch(resetUiOrderAdjustments());
        dispatch(setUiOrderAdjustments([]));
        const newOrderType = orderType === "SALES_ORDER" ? 1 : 2;
        setLocalOrderType(newOrderType);
        dispatch(setCurrentOrderType(orderType));
        dispatch(setOrderFormEditMode(1));
        setEditMode(1);
        navigate(orderType === "SALES_ORDER" ? "/orders/sales" : "/orders/purchase");
    }

    if (editMode > 0 && whatWasClicked === 'orderId') {
        if (localOrderType === 1) {
            return <SalesOrderForm selectedOrder={order} cancelEdit={cancelEdit} editMode={editMode} />;
        } else {
            return <PurchaseOrderForm selectedOrder={order} cancelEdit={cancelEdit} editMode={editMode} />;
        }
    }

    return (
        <>
            <AccountingMenu selectedMenuItem={"/orders"} />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{ height: '65vh' }}
                                resizable={true}
                                filterable={true}
                                sortable={true}
                                pageable={true}
                                {...dataState}
                                data={orders ? orders : { data: [], total: 77 }}
                                onDataStateChange={dataStateChange}
                            >
                                <GridToolbar>
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        onClick={handleCreateOrder}
                                    >
                                        {getTranslatedLabel(
                                            orderType === "SALES_ORDER" ? "order.list.salesOrder" : "order.list.purchaseOrder",
                                            orderType === "SALES_ORDER" ? "Create Sales Order" : "Create Purchase Order"
                                        )}
                                    </Button>
                                </GridToolbar>
                                <Column
                                    field="orderId"
                                    title={getTranslatedLabel("order.list.orderNumber", "Order Number")}
                                    width={150}
                                    locked={!show}
                                    cell={OrderDescriptionCell}
                                />
                                <Column
                                    field="orderTypeDescription"
                                    title={getTranslatedLabel("order.list.type", "Type")}
                                    columnMenu={ColumnMenuOrderTypeFilter}
                                />
                                <Column
                                    field="fromPartyName"
                                    title={getTranslatedLabel("order.list.customer", "Customer")}
                                />
                                <Column
                                    field="grandTotal"
                                    title={getTranslatedLabel("order.list.amount", "Amount")}
                                    width={130}
                                    filter={"numeric"}
                                />
                                <Column
                                    field="currencyUomDescription"
                                    title={getTranslatedLabel("order.list.currency", "Currency")}
                                />
                                <Column
                                    field="orderDate"
                                    title={getTranslatedLabel("order.list.orderDate", "Order Date")}
                                    format="{0: dd/MM/yyyy}"
                                />
                                <Column
                                    field="statusDescription"
                                    title={getTranslatedLabel("order.list.status", "Status")}
                                />
                            </KendoGrid>
                            {isFetching && (
                                <LoadingComponent message={getTranslatedLabel("order.list.loading", 'Loading Orders...')} />
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}