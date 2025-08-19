import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useCallback, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {Grid} from "@mui/material";
import {OrderItem} from "../../../../app/models/order/orderItem";

import {
    useAppDispatch,
    useAppSelector,
    useFetchCustomerTaxStatusQuery,
    useFetchJobOrderAdjustmentsQuery,
    useFetchJobOrderItemsQuery,
} from "../../../../app/store/configureStore";
import {useSelector} from "react-redux";
import {OrderAdjustment} from "../../../../app/models/order/orderAdjustment";
import {
    jobOrderAdjustmentsSelector,
    jobOrderItemsSelector,
    selectAdjustedOrderItems,
    setSelectedOrderItem,
    setUiJobOrderAdjustments,
    setUiJobOrderItems,
} from "../../slice/jobOrderUiSlice";
import {JobOrderItemFormMemo} from "../../form/order/JobOrder/JobOrderItemFormMemo";
import JobQuoteMarketingPkgItemsList from "../../../services/dashboard/JobQuoteMarketingPkgItemsList";
import ServiceItemSpecificationRateForm from "../../../services/form/ServiceItemSpecificationRateForm";
import {JobOrderItemAdjustmentsListMemo} from "./JobOrderItemAdjustmentsList";

interface Props {
    orderFormEditMode: number;
    orderId?: string;
}

export default function JobOrderItemsList({
                                              orderFormEditMode,
                                              orderId,
                                          }: Props) {
    // state for the grid
    const initialSort: Array<SortDescriptor> = [
        {field: "orderItemSeqId", dir: "asc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    // state for the order items component
    const [show, setShow] = useState(false);
    const [showPackageProductList, setShowPackageProductList] = useState(false);

    // state for the order item adjustments component
    const [showItemAdjustmentList, setShowItemAdjustmentList] = useState(false);
    const [showNewRateSpecification, setShowNewRateSpecification] =
        useState(false);
    // state for the order items component edit mode
    const [editMode, setEditMode] = useState(0);

    // state for selected order item for editing
    const [orderItem, setOrderItem] = useState<OrderItem | undefined>(undefined);

    const uiJobOrderItems: any = useSelector(jobOrderItemsSelector);
    const uiOrderAdjustments: any = useSelector(jobOrderAdjustmentsSelector);
    const [skipGettingOrderItemProduct, setSkipGettingOrderItemProduct] =
        useState(true);
    const customerId = useAppSelector(
        (state) => state.jobOrderUi.selectedCustomerId,
    );

    // get the order items from the store
    const {data: orderItemsData} = useFetchJobOrderItemsQuery(orderId, {
        skip: orderId === undefined,
    });
    // get the order adjustments from the store
    const {data: orderAdjustmentsData} = useFetchJobOrderAdjustmentsQuery(
        orderId,
        {skip: orderId === undefined},
    );

    const {data: customerTaxStatus} = useFetchCustomerTaxStatusQuery(
        customerId,
        {skip: customerId === undefined},
    );

    const dispatch = useAppDispatch();
    const adjustedJobOrderItems = useSelector(selectAdjustedOrderItems);

    console.log("adjustedJobOrderItems", adjustedJobOrderItems);

    // handle order item selection from grid for editing
    function handleSelectOrderItem(orderItemId: string) {
        // get the order item from the orderItemsAndAdjustments state
        const selectedOrderItem: OrderItem | undefined = uiJobOrderItems!.find(
            (orderItem: any) =>
                orderItem.orderId.concat(orderItem.orderItemSeqId) === orderItemId,
        );

        // set the selected order item in state and start edit mode
        setOrderItem(selectedOrderItem);
        setEditMode(2);
        setShow(true);
    }

    // custom grid cell to show total adjustments for an order item and enable opening the items adjustments component
    const ItemAdjustmentCommandCell = (props: any) => {
        return (
            <td>
                <Button
                    onClick={() => {
                        // get the order item from the orderItemsAndAdjustments state
                        const selectedOrderItem: OrderItem | undefined =
                            uiJobOrderItems!.find(
                                (orderItem: any) =>
                                    orderItem.orderId.concat(orderItem.orderItemSeqId) ===
                                    props.dataItem.orderId.concat(props.dataItem.orderItemSeqId),
                            );

                        // set the selected order item in state and start edit mode
                        dispatch(setSelectedOrderItem(selectedOrderItem));
                        // save it in local state and open the adjustments component
                        setOrderItem(selectedOrderItem);
                        setShowItemAdjustmentList(true);
                    }}
                >
                    {props.dataItem.totalItemAdjustments
                        ? props.dataItem.totalItemAdjustments.toFixed(2)
                        : 0}
                </Button>
            </td>
        );
    };

    const ItemQuantityCommandCell = (props: any) => {
        const {productTypeId, quantity} = props.dataItem;
        const isMarketingPackage = productTypeId === "MARKETING_PKG";

        return (
            <td>
                {isMarketingPackage ? (
                    <Button
                        style={{color: "red"}}
                        onClick={() => {
                            setShowPackageProductList(true);
                        }}
                    >
                        {quantity}
                    </Button>
                ) : (
                    quantity
                )}
            </td>
        );
    };

    // custom grid cell for order item selection
    const orderItemCell = (props: any) => {
        return (
            <td>
                <Button
                    onClick={() =>
                        handleSelectOrderItem(
                            props.dataItem.orderId.concat(props.dataItem.orderItemSeqId),
                        )
                    }
                    disabled={props.dataItem.isPromo === "Y"}
                >
                    {props.dataItem.productName}
                </Button>
            </td>
        );
    };
    // close the order item form
    const memoizedOnClose = useCallback(() => {
        setShow(false);
    }, []);
    // close the order item Adjustment list
    const OnCloseItemAdjustmentList = useCallback(() => {
        setShowItemAdjustmentList(false);
    }, []);

    // custom grid button function to delete an order item
    const DeleteOrderItemCell = (props: any) => {
        const {dataItem} = props;

        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() => props.remove(dataItem)}
                    disabled={props.dataItem.isPromo === "Y"}
                    color="error"
                >
                    Remove
                </Button>
            </td>
        );
    };

    // helper function for the remove function
    const updateItem = (
        item: OrderItem,
        dataItemId: string,
        relatedIds: string[],
    ) => {
        if (
            item.orderItemSeqId === dataItemId ||
            relatedIds.includes(item.orderItemSeqId)
        ) {
            return {...item, isProductDeleted: true};
        }
        return item;
    };

    // helper function for the remove function
    const updateAdjustment = (
        adj: OrderAdjustment,
        relatedOrderAdjustments: OrderAdjustment[],
    ) => {
        if (relatedOrderAdjustments.includes(adj)) {
            return {...adj, isAdjustmentDeleted: true};
        }
        return adj;
    };

    // the actual deletion logic for an order item
    const remove = (dataItem: OrderItem) => {
        // check if the order item has a related promo item(s) and delete them as well
        // start by checking if the other order item is a promo item and by using
        // parentOrderItemSeqId to find the related order items

        // identifying related order items
        const relatedOrderItems =
            uiJobOrderItems?.filter(
                (item: OrderItem) =>
                    item.parentOrderItemSeqId === dataItem?.orderItemSeqId,
            ) || [];

        // get the order item ids for the related order items
        const relatedIds = relatedOrderItems.map(
            (item: OrderItem) => item.orderItemSeqId,
        );

        // Update local order items in a single pass
        const updatedOrderItems = uiJobOrderItems?.map((item: OrderItem) =>
            updateItem(item, dataItem?.orderItemSeqId, relatedIds),
        );

        dispatch(setUiJobOrderItems(updatedOrderItems!));

        // Similarly, update order adjustments in a single pass
        const relatedOrderAdjustments =
            uiOrderAdjustments?.filter((item: OrderAdjustment) =>
                relatedIds.includes(item.orderItemSeqId),
            ) || [];

        // add the order item adjustments related dataItem to relatedOrderAdjustments
        relatedOrderAdjustments.push(
            ...(uiOrderAdjustments?.filter(
                (item: OrderAdjustment) =>
                    item.orderItemSeqId === dataItem?.orderItemSeqId,
            ) || []),
        );

        const updatedOrderAdjustments = uiOrderAdjustments?.map(
            (adj: OrderAdjustment) => updateAdjustment(adj, relatedOrderAdjustments),
        );

        dispatch(setUiJobOrderAdjustments(updatedOrderAdjustments!));
    };

    const CommandCell = (props: GridCellProps) => (
        <DeleteOrderItemCell {...props} remove={remove}/>
    );

    const memoizedOnClose2 = useCallback(() => {
        setShowNewRateSpecification(false);
    }, []);

    const OnCloseMarketingItemsList = useCallback(() => {
        setShowPackageProductList(false);
    }, []);

    return (
        <>
            <JobOrderItemFormMemo
                orderItem={orderItem}
                editMode={editMode}
                onClose={memoizedOnClose}
                orderFormEditMode={orderFormEditMode}
                show={show}
                skip={skipGettingOrderItemProduct}
                width={600}
                setShowNewRateSpecification={setShowNewRateSpecification}
            />

            <JobOrderItemAdjustmentsListMemo
                orderItem={orderItem}
                onClose={OnCloseItemAdjustmentList}
                showItemAdjustmentList={showItemAdjustmentList}
                orderFormEditMode={orderFormEditMode}
                width={600}
            />

            <JobQuoteMarketingPkgItemsList
                onClose={OnCloseMarketingItemsList}
                showList={showPackageProductList}
                width={500}
            />
            <ServiceItemSpecificationRateForm
                onClose={memoizedOnClose2}
                show={showNewRateSpecification}
                width={550}
            />

            <Grid
                container
                columnSpacing={1}
                direction={"column"}
                alignItems="flex-end"
            >
                <Grid item xs={6}>
                    <Button
                        color={"secondary"}
                        onClick={() => {
                            setOrderItem(undefined);
                            setSkipGettingOrderItemProduct(true);
                            setEditMode(1);
                            setShow(true);
                        }}
                        variant="outlined"
                        disabled={customerId === undefined || orderFormEditMode > 2}
                    >
                        Add Product
                    </Button>
                </Grid>
                <Grid
                    className={
                        orderFormEditMode > 2 ? "div-container-disabled" : "div-container"
                    }
                    item
                >
                    <div className="div-container">
                        <KendoGrid
                            className="main-grid"
                            style={{height: "300px"}}
                            data={orderBy(
                                adjustedJobOrderItems ? adjustedJobOrderItems : [],
                                sort,
                            ).slice(page.skip, page.take + page.skip)}
                            sortable={true}
                            sort={sort}
                            onSortChange={(e: GridSortChangeEvent) => {
                                setSort(e.sort);
                            }}
                            skip={page.skip}
                            take={page.take}
                            total={adjustedJobOrderItems ? adjustedJobOrderItems.length : 0}
                            pageable={true}
                            onPageChange={pageChange}
                        >
                            <Column
                                field="productName"
                                title="Product"
                                cell={orderItemCell}
                                width={300}
                            />
                            <Column field="orderItemSeqId" title="orderItemSeqId" width={0}/>
                            <Column field="unitListPrice" title="List Price" width={110}/>
                            <Column field="quantity" title="Quantity" width={120}/>
                            <Column
                                cell={ItemAdjustmentCommandCell}
                                title="Adjustments"
                                width={150}
                            />
                            <Column
                                field="subTotal"
                                title="Sub Total"
                                width={120}
                                format="{0:c}"
                            />
                            <Column cell={CommandCell} width="100px"/>
                        </KendoGrid>
                    </div>
                </Grid>
            </Grid>
        </>
    );
}

export const JobOrderItemsListMemo = React.memo(JobOrderItemsList);
