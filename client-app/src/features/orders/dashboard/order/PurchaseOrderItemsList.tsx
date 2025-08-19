import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useCallback, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import {useAppDispatch, useAppSelector, useFetchPurchaseOrderItemsQuery} from "../../../../app/store/configureStore";
import Button from "@mui/material/Button";
import {Grid, Skeleton} from "@mui/material";
import {OrderItem} from "../../../../app/models/order/orderItem";
import {useSelector} from "react-redux";
import {PurchaseOrderItemFormMemo} from "../../form/order/PurchaseOrder/PurchaseOrderItemForm";
import {nonDeletedOrderItemsSelector, orderAdjustmentsSelector, orderSubTotal} from "../../slice/orderSelectors";
import {setSelectedOrderItem, setUiOrderItems} from "../../slice/orderItemsUiSlice";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import OrderAdjustmentsList from "./OrderAdjustmentsList";
import OrderTermsList from "../../form/order/SalesOrder/OrderTermsList";
import {OrderAdjustment} from "../../../../app/models/order/orderAdjustment";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {OrderItemAdjustmentsListMemo} from "./OrderItemAdjustmentsList";


interface Props {
    orderFormEditMode: number
    orderId?: string

}

export default function PurchaseOrderItemsList({orderFormEditMode, orderId}: Props) {

    const initialSort: Array<SortDescriptor> = [
        {field: "partyId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const [show, setShow] = useState(false);
    const [showList, setShowList] = useState(false);
    const [showTermsModal, setShowTermsModal] = useState(false);
    const [showItemAdjustmentList, setShowItemAdjustmentList] = useState(false);

    const [editMode, setEditMode] = useState(0);
    const [orderItem, setOrderItem] = useState<OrderItem | undefined>(undefined);

    const uiOrderItems: any = useSelector(nonDeletedOrderItemsSelector)
    const orderSTotal: any = useSelector(orderSubTotal);
    const uiOrderAdjustments: any = useSelector(orderAdjustmentsSelector)

    const [skipGettingOrderItemProduct, setSkipGettingOrderItemProduct] = useState(true)


    const supplierId = useAppSelector(state => state.sharedOrderUi.selectedSupplierId);

    const {data: orderItemsData, error, isFetching, isLoading} = useFetchPurchaseOrderItemsQuery(orderId,
        {skip: orderId === undefined});

    const dispatch = useAppDispatch();
    const localizationKey = 'order.po.items.list'
    const {getTranslatedLabel} = useTranslationHelper()


    function handleSelectOrderItem(orderItemId: string) {
        const selectedOrderItem: OrderItem | undefined = uiOrderItems!.find((orderItem: any) =>
            orderItem.orderId.concat(orderItem.orderItemSeqId) === orderItemId);

        // change string based productId with the actual product object
        // if this orderItem is a saved one then we'll use the one from orderItemProduct, else we'll use the one from productLov
        let orderItemToDisplay;
        if (selectedOrderItem?.orderItemProduct) {
            orderItemToDisplay = {
                ...selectedOrderItem,
                productId: {
                    productId: selectedOrderItem.orderItemProduct.productId ?? null,
                    productName: selectedOrderItem.orderItemProduct.productName ?? null,
                    lastPrice: selectedOrderItem.orderItemProduct.lastPrice ?? null,
                    quantityUom: selectedOrderItem.orderItemProduct.quantityUom ?? null,
                    uomDescription: selectedOrderItem.orderItemProduct.uomDescription ?? null
                }
            };
        } else {
            orderItemToDisplay = {...selectedOrderItem, productId: selectedOrderItem?.productLov};
        }
        
        console.log('selectedOrderItem', selectedOrderItem);    

        console.log('orderItemToDisplay', orderItemToDisplay);
        // set the selected order item in state and start edit mode
        //setOrderItem(selectedOrderItem)
        setOrderItem(orderItemToDisplay);
        setEditMode(2);
        setShow(true);
    }

    const memoizedOnClose = useCallback(
        () => {
            setShow(false)
            setShowList(false)
            setShowTermsModal(false)
        },
        [show],
    );
    const orderItemCell = (props: any) => {

        return (
            <td>
                <Button
                    onClick={() => handleSelectOrderItem(props.dataItem.orderId.concat(props.dataItem.orderItemSeqId))}
                >
                    {props.dataItem.productName}
                </Button>


            </td>
        )
    }
    // custom grid button function to delete an order item
    const DeleteOrderItemCell = (props: any) => {
        const { dataItem } = props;
        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    disabled={orderFormEditMode > 3}
                    onClick={() => props.remove(dataItem)}
                    color="error"
                >
                    Remove
                </Button>
            </td>
        );
    };
    // update the order item in the grid; called from the order item form
    const updateOrderItems = (orderItem: OrderItem, editMode: number) => {
        let newOrderItems: OrderItem[] | undefined
        try {
            // update orderItems based on editMode
            if (editMode === 1) {
                // add new orderItem
                newOrderItems = uiOrderItems ? [...uiOrderItems!, orderItem] : [orderItem];
            } else if (editMode === 2) {
                // edit existing orderItem
                newOrderItems = uiOrderItems?.map((item: OrderItem) => {
                    if (item!.orderId!.concat(item.orderItemSeqId) === orderItem!.orderId!.concat(orderItem.orderItemSeqId)) {
                        return orderItem;
                    } else {
                        return item;
                    }
                });
            }

            dispatch(setUiOrderItems(newOrderItems));
            // setOrderItems(newOrderItems);
        } catch (e) {
            console.error(e)
        }

    }

    // the actual deletion logic for an order item
    const remove = (dataItem: OrderItem) => {

        const newOrderItems = uiOrderItems?.map((item: OrderItem) => {
            if (item.orderItemSeqId === dataItem?.orderItemSeqId) {
                return {...item, isProductDeleted: true}
            } else {
                return item
            }
        })
        dispatch(setUiOrderItems(newOrderItems!))

    };

    // support for order item deletion cell
    const CommandCell = (props: GridCellProps) => (
        <DeleteOrderItemCell
            {...props}
            remove={remove}
        />
    );

    const ItemDiscountCommandCell = (props: any) => {
        const discountAdjs = uiOrderAdjustments.filter(
            (a: OrderAdjustment) =>
                a.orderAdjustmentTypeId === "DISCOUNT_ADJUSTMENT" &&
                a.correspondingProductId === props.dataItem.productId
        );

        const salesTaxAdjs = uiOrderAdjustments.filter(
            (a: OrderAdjustment) =>
                a.orderAdjustmentTypeId === "VAT_TAX" &&
                a.correspondingProductId === props.dataItem.productId
        );

        const discountValue = discountAdjs.reduce(
            (a: number, b: OrderAdjustment) => a + (b.amount || 0),
            0
        );

        const salesTaxValue = salesTaxAdjs.reduce(
            (a: number, b: OrderAdjustment) => a + (b.amount || 0),
            0
        );

        const discountsNotProcessedYet =
            (discountAdjs.length > 0 || salesTaxAdjs.length > 0) &&
            props.dataItem.discountAndPromotionAdjustments === 0;

        const taxNotProcessedYet =
            props.dataItem.totalItemTaxAdjustments !== salesTaxValue;

        // REFACTOR: Default to 0 if undefined or null
        // Purpose: Ensures finalDiscount and finalTax are numbers to prevent toFixed errors
        const finalDiscount = discountsNotProcessedYet
            ? discountValue
            : (props.dataItem.discountAndPromotionAdjustments ?? 0);

        const finalTax = taxNotProcessedYet
            ? salesTaxValue
            : (props.dataItem.totalItemTaxAdjustments ?? 0);

        const totalAdjustments = finalDiscount + finalTax;

        const showBreakdown = finalDiscount !== 0 && finalTax > 0;

        // REFACTOR: Add debug logging for adjustment values
        // Purpose: Tracks values to diagnose null/undefined issues after addTax and invoice creation
        console.log('ItemDiscountCommandCell:', {
            productId: props.dataItem.productId,
            orderItemSeqId: props.dataItem.orderItemSeqId,
            discountAdjs,
            salesTaxAdjs,
            discountValue,
            salesTaxValue,
            discountsNotProcessedYet,
            taxNotProcessedYet,
            finalDiscount,
            finalTax,
            totalAdjustments,
            dataItem: props.dataItem
        });

        return (
            <td>
                <Button
                    onClick={() => {
                        const selectedOrderItem: OrderItem | undefined = uiOrderItems!.find(
                            (orderItem: any) =>
                                orderItem.orderId.concat(orderItem.orderItemSeqId) ===
                                props.dataItem.orderId.concat(props.dataItem.orderItemSeqId)
                        );
                        dispatch(setSelectedOrderItem(selectedOrderItem));
                        setOrderItem(selectedOrderItem);
                        setShowItemAdjustmentList(true);
                    }}
                    title={
                        showBreakdown
                            ? `Discount: ${finalDiscount.toFixed(2)}\nTax: ${finalTax.toFixed(2)}`
                            : undefined
                    }
                >
                    {totalAdjustments.toFixed(2)}
                    {showBreakdown && (
                        <span
                            style={{
                                fontSize: "1em",
                                color: "#333",
                                marginLeft: 6,
                                fontWeight: "bold",
                                backgroundColor: "#f0f0f0",
                                padding: "2px 6px",
                                borderRadius: "4px",
                            }}
                        >
                            (D: {finalDiscount.toFixed(2)}, T: {finalTax.toFixed(2)})
                        </span>
                    )}
                </Button>
            </td>
        );
    };

    const onCloseItemAdjustmentList = useCallback(
        () => {
            setShowItemAdjustmentList(false)
        },
        [],
    );


    return (
        <>
            {showList && (<ModalContainer show={showList} onClose={memoizedOnClose} width={900}>
                <OrderAdjustmentsList
                    onClose={memoizedOnClose}
                />
            </ModalContainer>)}
            {show && (<ModalContainer show={show} onClose={memoizedOnClose} width={700}>
                <PurchaseOrderItemFormMemo orderItem={orderItem} editMode={editMode} onClose={memoizedOnClose}
                                           orderFormEditMode={orderFormEditMode}/>
            </ModalContainer>)}
            {showTermsModal && (
                <ModalContainer
                    show={showTermsModal}
                    onClose={memoizedOnClose}
                    width={900}
                >
                    <OrderTermsList
                        onClose={memoizedOnClose}
                        orderId={orderId!}
                    />
                </ModalContainer>
            )}

            {showItemAdjustmentList && (
                <ModalContainer show={showItemAdjustmentList} onClose={onCloseItemAdjustmentList} width={800}>
                    <OrderItemAdjustmentsListMemo orderItem={orderItem} onClose={onCloseItemAdjustmentList}
                                                  orderFormEditMode={orderFormEditMode}/>
                </ModalContainer>)}

            <Grid container columnSpacing={1} direction={"column"} alignItems="flex-start" sx={{mt: 1}}>
                <Grid container>
                    {(isFetching|| isLoading) ? 
                    <Grid container spacing={2} direction="column">
                    <Grid item>
                        <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: '70%' }} />
                    </Grid>
                    <Grid item>
                        <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: '70%' }} />
                    </Grid>
                    <Grid item>
                        <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: '70%' }} />
                    </Grid>
                </Grid>
                    :
                     <Grid item xs={12}
                    >
                        <KendoGrid className="main-grid" style={{height: "30vh"}}
                                   data={orderBy(uiOrderItems ? uiOrderItems : [], sort).slice(page.skip, page.take + page.skip)}
                                   sortable={true}
                                   sort={sort}
                                   onSortChange={(e: GridSortChangeEvent) => {
                                       setSort(e.sort);
                                   }}
                                   skip={page.skip}
                                   take={page.take}
                                   total={uiOrderItems ? uiOrderItems.length : 0}
                                   pageable={true}
                                   onPageChange={pageChange}

                        >
                            <GridToolbar>
                                <Grid container justifyContent={"space-between"}>
                                    <Grid item>
                                        <Button color={"secondary"} onClick={() => {
                                            setOrderItem(undefined)
                                            setSkipGettingOrderItemProduct(true);
                                            setEditMode(1);
                                            setShow(true);
                                        }} variant="outlined"
                                                disabled={supplierId === undefined}>
                                            Add Product
                                        </Button>
                                    </Grid>
                                    <Grid item>
                                        <Button color="secondary" onClick={() => {
                                            setShowList(true)
                                        }} variant="outlined" disabled={orderSTotal === 0}>
                                            Order Adjustments
                                        </Button>
                                        <Button sx={{ml: 2}} onClick={() => setShowTermsModal(true)} variant="outlined">
                                            Order Terms
                                        </Button>
                                    </Grid>
                                </Grid>
                            </GridToolbar>
                            <Column field="productName" title="Product" cell={orderItemCell} width={280}/>
                            <Column field="orderId" title="orderId" width={0}/>
                            <Column field="orderItemSeqId" title="orderItemSeqId" width={0}/>
                            <Column field="unitPrice" title="Unit Price" />
                            <Column field="quantity" title="Quantity" />
                            <Column cell={ItemDiscountCommandCell} title={getTranslatedLabel(`${localizationKey}.discount`,"Discounts/Tax")} width={170}/>
                            <Column field="subTotal" title="Sub Total" format="{0:n2}" />
                            <Column cell={CommandCell} />


                        </KendoGrid>
                    </Grid>}
                </Grid>


            </Grid>
        </>

    )
}

export const PurchaseOrderItemsListMemo = React.memo(PurchaseOrderItemsList)

