import React, {useEffect, useState} from "react";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {Grid, Paper, Typography} from "@mui/material";
import {ExcelExport} from "@progress/kendo-react-excel-export";
import AppPagination from "../../../../app/components/AppPagination";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {handleDatesArray} from "../../../../app/util/utils";
import {Order, OrderParams} from "../../../../app/models/order/order";

import {useAppDispatch, useFetchJobOrdersQuery,} from "../../../../app/store/configureStore";
import {orders_order_listOrder_grid_orderNumber_key} from "../../../../app/common/messages/messages";
import {useLocation} from "react-router-dom";
import JobOrderForm from "../../form/order/JobOrder/JobOrderForm";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import VehicleMenu from "../../../services/menu/VehicleMenu";
import {setCustomerId, setSelectedVehicle, setVehicleId,} from "../../slice/jobOrderUiSlice";

export default function JobOrdersList() {
    const {getTranslatedLabel} = useTranslationHelper();
    const location = useLocation();
    const [shouldShowOrderForm, setShouldShowOrderForm] = useState(false);

    const [editMode, setEditMode] = useState(0);
    const [order, setOrder] = useState<Order | undefined>(undefined);
    const [page, setPage] = useState(1);
    const params = {
        pageNumber: page,
        pageSize: 6,
        orderBy: "orderIdAsc",
        orderTypes: [],
        customerPhone: "",
        customerName: "",
    };
    const [show, setShow] = useState(false);
    const [orderParam, setOrderParam] = useState<OrderParams>(params);
    const dispatch = useAppDispatch();

    const {data, error, isFetching} = useFetchJobOrdersQuery({
        ...orderParam,
        pageNumber: page,
    });

    console.log(data!);

    // When component mounts or location.state.order changes, decide whether to show form
    useEffect(() => {
        if (location.state?.order && !order) {
            setShouldShowOrderForm(true);
        }
    }, [location.state?.order]);

    function setOrderStatus(orderStatus: string) {
        if (orderStatus === "Created") {
            setEditMode(2);
        }

        if (orderStatus === "Approved") {
            setEditMode(3);
        }

        if (orderStatus === "Completed") {
            setEditMode(4);
        }
    }

    function handleSelectOrder(orderId: string, orderStatus: string) {
        // select the order from data array based on orderId
        const selectedJobOrder: Order | undefined = data?.data.find(
            (order: any) => order.orderId === orderId,
        );

        dispatch(setCustomerId(selectedJobOrder?.fromPartyId.fromPartyId));
        dispatch(setVehicleId(selectedJobOrder?.vehicleId.vehicleId));
        dispatch(setSelectedVehicle(selectedJobOrder?.vehicleId));

        // set component selected order
        setOrder(selectedJobOrder);
        setOrderStatus(orderStatus); // set edit mode based on order status
    }

    // memoize the onSubmit function
    const onSubmit = React.useCallback(
        (param: OrderParams) => {
            const newParam = {...orderParam, ...param, pageNumber: page};
            setOrderParam(newParam);
        },
        [orderParam, page],
    );

    const OrderDescriptionCell = (props: any) => {
        const field = props.field || "";
        const value = props.dataItem[field];
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
                <Button
                    onClick={() => {
                        handleSelectOrder(
                            props.dataItem.orderId,
                            props.dataItem.statusDescription,
                        );
                    }}
                >
                    {props.dataItem.orderId}
                </Button>
            </td>
        );
    };

    // Code for Grid functionality
    const dataToExport = data ? handleDatesArray(data.data) : [];

    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            // @ts-ignore
            _export.current!.save();
        }
    };

    // convert cancelEdit function to memoized function
    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setOrder(undefined);
        setShouldShowOrderForm(false);
    }, [setEditMode, setOrder, setShouldShowOrderForm]);

    if (shouldShowOrderForm) {
        return (
            <JobOrderForm
                selectedOrder={location.state?.order}
                cancelEdit={cancelEdit}
                editMode={2}
            />
        );
    }

    if (editMode > 0) {
        return (
            <JobOrderForm
                selectedOrder={order}
                cancelEdit={cancelEdit}
                editMode={editMode}
            />
        );
    }

    return (
        <>
            {isFetching && <LoadingComponent message="Loading Orders..."/>}

            <VehicleMenu/>
            <Grid container columnSpacing={1} alignItems="center">
                <Grid item xs={12}>
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <Grid container columnSpacing={1} alignItems="center">
                            <Grid item xs={8}>
                                {/* <Grid container alignItems="center">
                                    <Grid item xs={4}>
                                        <Typography sx={{p: 2}} variant="h4">
                                            Orders
                                        </Typography>
                                    </Grid>
                                </Grid> */}

                                <Grid container>
                                    <div className="div-container">
                                        <ExcelExport data={dataToExport} ref={_export}>
                                            <KendoGrid
                                                className="small-line-height"
                                                style={{height: "65vh", width: "94vw", flex: 1}}
                                                data={data ? handleDatesArray(data.data) : []}
                                                resizable={true}
                                                pageable={true}
                                                total={data ? data?.total : 0}
                                                reorderable={true}
                                            >
                                                <GridToolbar>
                                                    <Grid container>
                                                        {/* <Grid item xs={3}>
                                                            <button
                                                                title="Export Excel"
                                                                className="k-button k-primary"
                                                                onClick={excelExport}
                                                            >
                                                                Export to Excel
                                                            </button>
                                                        </Grid> */}
                                                        <Grid item xs={2}>
                                                            <Button
                                                                color={"secondary"}
                                                                onClick={() => {
                                                                    setShow(true);
                                                                }}
                                                                variant="outlined"
                                                            >
                                                                Find Orders
                                                            </Button>
                                                        </Grid>
                                                    </Grid>
                                                </GridToolbar>
                                                <Column
                                                    field="orderId"
                                                    title={getTranslatedLabel(
                                                        orders_order_listOrder_grid_orderNumber_key,
                                                        "Order Number",
                                                    )}
                                                    cell={OrderDescriptionCell}
                                                    width={150}
                                                    locked={true}
                                                />
                                                <Column
                                                    field="fromPartyName"
                                                    title="Customer"
                                                    // width={350}
                                                />
                                                <Column field="grandTotal" title="Amount" width={130}/>
                                                <Column
                                                    field="orderDate"
                                                    title="Order Date"
                                                    // width={150}
                                                    format="{0: dd/MM/yyyy}"
                                                />
                                                <Column
                                                    field="statusDescription"
                                                    title="Status"
                                                    // width={100}
                                                />
                                                <Column
                                                    field="orderTypeDescription"
                                                    title="Type"
                                                    // width={100}
                                                />
                                            </KendoGrid>
                                        </ExcelExport>
                                    </div>
                                </Grid>
                                {/* <Grid item xs={9} sx={{mb: 2, padding: 2}}>
                                    {data && (
                                        <AppPagination
                                            metaData={data}
                                            onPageChange={(page: number) => setPage(page)}
                                        />
                                    )}
                                </Grid> */}
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>
            </Grid>
        </>
    );
}
