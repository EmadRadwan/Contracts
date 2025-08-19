import {
    useAppDispatch,
    useAppSelector,
    useFetchApprovedSalesOrdersQuery,
} from "../../../app/store/configureStore";
import React, { useCallback, useState } from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
} from "@progress/kendo-react-grid";
import { Button, Grid, Paper } from "@mui/material";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { requiredValidator } from "../../../app/common/form/Validators";
import FacilityMenu from "../menu/FacilityMenu";
import { resetUiOrderItems } from "../../orders/slice/orderItemsUiSlice";
import { setSelectedApprovedSalesOrder } from "../../orders/slice/sharedOrderUiSlice";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { toast } from "react-toastify";
import { MemoizedFormDropDownListApprovedSalesOrders } from "../../../app/common/form/MemoizedFormDropDownListApprovedSalesOrders";
import {
    useFetchPackingDataQuery,
    usePackOrderMutation,
} from "../../../app/store/apis";
import PackItemForm from "../form/PackItemForm";
import {FormComboBoxVirtualCustomer} from "../../../app/common/form/FormComboBoxVirtualCustomer";

const PackingList = () => {
    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();
    const [show, setShow] = useState(false);
    const [selectedItem, setSelectedItem] = useState(null);
    const [packOrder, { isLoading: isMutationLoading }] = usePackOrderMutation();
    const { selectedApprovedSalesOrder } = useAppSelector(
        (state) => state.sharedOrderUi
    );
    const [selectedPartyId, setSelectedPartyId] = useState(null);

    const { data: approvedSalesOrders, isLoading: isSalesOrdersLoading } =
        useFetchApprovedSalesOrdersQuery(
            { partyId: selectedPartyId },
            { skip: !selectedPartyId } // Skip query if no partyId is selected
        );

    const handleCustomerChange = useCallback((event) => {
        setSelectedPartyId(event.value?.fromPartyId || null);
        dispatch(setSelectedApprovedSalesOrder(undefined)); // Clear selected order
        setOrderItems({ items: [], invoiceIds: [] }); // Reset order items
    }, [dispatch]);
    
    const {
        data: fetchedOrderItems,
        isLoading: isPackingDataLoading,
        refetch,
    } = useFetchPackingDataQuery(
        {
            facilityId: "a5826c99-ca43-4114-9496-0acf1ed71049",
            orderId: selectedApprovedSalesOrder?.orderId,
            shipGroupSeqId: selectedApprovedSalesOrder?.shipGroupSeqId || "01",
        },
        { skip: !selectedApprovedSalesOrder?.orderId }
    );

    // Local state to manage orderItems
    const [orderItems, setOrderItems] = useState(fetchedOrderItems || { items: [], invoiceIds: [] });

    // Check if any items have quantityToShip > 0
    const hasItemsToShip = orderItems?.items?.some(item => item.quantityToShip > 0) || false;

    // Sync local state with fetched data, overriding quantityToShip
    React.useEffect(() => {
        if (fetchedOrderItems) {
            setOrderItems({
                ...fetchedOrderItems,
                items: fetchedOrderItems.items.map((item) => ({
                    ...item,
                    quantityToShip: Math.max(0, (item.quantity || 0) - (item.shippedQuantity || 0)),
                    includeThisItem: item.includeThisItem ?? false,
                })),
            });
        }
    }, [fetchedOrderItems]);

    // Cleanup on unmount
    React.useEffect(() => {
        return () => {
            dispatch(setSelectedApprovedSalesOrder(undefined));
            dispatch(resetUiOrderItems());
        };
    }, [dispatch]);

    const PackCell = (props) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        // Hide Edit button if quantityToShip is 0
        if (props.dataItem.quantityToShip <= 0) {
            return <td />;
        }
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: "blue" }}
                colSpan={props.colSpan}
                role={"gridcell"}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button onClick={() => handleSelectItemToPack(props.dataItem)}>
                    {getTranslatedLabel("facility.receive.list.receive", "Edit")}
                </Button>
            </td>
        );
    };

    const IncludeCell = (props) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{
                    ...props.style,
                    color: props.dataItem.includeThisItem ? "green" : "red",
                }}
                colSpan={props.colSpan}
                role={"gridcell"}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                {props.dataItem.includeThisItem ? "Yes" : "No"}
            </td>
        );
    };

    const handleSelectItemToPack = (orderItem) => {
        const selected = orderItems?.items?.find(
            (ii) => ii.orderItemSeqId === orderItem.orderItemSeqId
        );
        if (selected) {
            setSelectedItem(selected);
            setShow(true);
        } else {
            toast.error("Item not found.");
        }
    };

    const memoizedOnClose = useCallback(() => {
        setShow(false);
        setSelectedItem(null);
    }, []);

    const handleSubmit = (updatedItem) => {
        // Update local orderItems state
        setOrderItems((prev) => ({
            ...prev,
            items: prev.items.map((item) =>
                item.orderItemSeqId === updatedItem.orderItemSeqId
                    ? {
                        ...item,
                        quantityToShip: updatedItem.quantityToShip,
                        includeThisItem: updatedItem.includeThisItem,
                    }
                    : item
            ),
        }));
        toast.success("Item updated successfully.");
        memoizedOnClose();
    };

    const handleMenuSelect = async (e: MenuSelectEvent) => {
        if (e.item.text === "Issue Products") {
            await handleIssueProducts();
        }
    };

    const handleIssueProducts = async () => {
        const itemsToPack = orderItems?.items?.filter((o) => o.includeThisItem) || [];
        if (itemsToPack.length === 0) {
            toast.error("No items selected for packing.");
            return;
        }

        const payload = {
            OrderId: selectedApprovedSalesOrder?.orderId,
            ShipGroupSeqId: selectedApprovedSalesOrder?.shipGroupSeqId || "01",
            FacilityId: "a5826c99-ca43-4114-9496-0acf1ed71049",
            PicklistBinId: undefined,
            ItemsToPack: itemsToPack.map((item) => ({
                OrderId: item.orderId,
                OrderItemSeqId: item.orderItemSeqId,
                ShipGroupSeqId: item.shipGroupSeqId,
                ProductId: item.productId,
                InventoryItemId: item.inventoryItemId,
                Quantity: item.quantityToShip,
                Weight: item.weight || 0,
                PackageSeqId: item.packageSeqId || 1,
            })),
        };

        try {
            await packOrder(payload).unwrap();
            toast.success("Products packed successfully.");
            await refetch(); // Refetch after issuing products
        } catch (error) {
            console.error(error);
            toast.error("Failed to pack products.");
        }
    };

    return (
        <>
            {show && selectedItem && (
                <ModalContainer show={show} onClose={memoizedOnClose} width={950}>
                    <PackItemForm
                        orderItem={selectedItem}
                        onClose={memoizedOnClose}
                        handleSubmit={handleSubmit}
                    />
                </ModalContainer>
            )}
            <FacilityMenu />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={10}>
                        <Form
                            render={() => (
                                <FormElement>
                                    <fieldset className="k-form-fieldset">
                                        <Grid
                                            container
                                            spacing={2}
                                            sx={{ marginBottom: 2 }}
                                        >
                                            <Grid item xs={3}>
                                                <Field
                                                    id="fromPartyId"
                                                    name="fromPartyId"
                                                    label={getTranslatedLabel(
                                                        "facility.pack.list.customer",
                                                        "Customer *"
                                                    )}
                                                    component={FormComboBoxVirtualCustomer}
                                                    autoComplete="off"
                                                    onChange={handleCustomerChange}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="orderId"
                                                    name="orderId"
                                                    label={getTranslatedLabel(
                                                        "facility.pack.list.select",
                                                        "Select Sales Order *"
                                                    )}
                                                    component={
                                                        MemoizedFormDropDownListApprovedSalesOrders
                                                    }
                                                    dataItemKey="orderId"
                                                    textField="orderDescription"
                                                    data={approvedSalesOrders || []}
                                                    validator={requiredValidator}
                                                    disabled={isSalesOrdersLoading}
                                                />
                                            </Grid>
                                        </Grid>
                                    </fieldset>
                                </FormElement>
                            )}
                        />
                    </Grid>
                    <Grid item xs={1}>
                        <Button
                            variant="contained"
                            onClick={handleIssueProducts}
                            disabled={!orderItems?.items?.length || !hasItemsToShip}
                        >
                            {getTranslatedLabel("facility.pack.menu.issuceInventory", "Issue Products")}
                        </Button>
                    </Grid>
                    <Grid item xs={12}>
                     {/*   {orderItems?.invoiceIds?.length > 0 && (
                            <div>
                                <p>Invoices:</p>
                                <ul>
                                    {orderItems.invoiceIds.map((invoiceId) => (
                                        <li key={invoiceId}>
                                            Nbr{" "}
                                            <a
                                                href={`/accounting/invoiceOverview?invoiceId=${invoiceId}`}
                                                target="_blank"
                                            >
                                                {invoiceId}
                                            </a>{" "}
                                            (
                                            <a
                                                href={`/accounting/invoice.pdf?invoiceId=${invoiceId}`}
                                                target="_blank"
                                            >
                                                PDF
                                            </a>
                                            )
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        )}*/}
                        {orderItems?.items?.length === 0 &&
                            !isPackingDataLoading && (
                                <p>No items available for packing.</p>
                            )}
                        <KendoGrid
                            data={orderItems?.items || []}
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
                                field="productId"
                                title={getTranslatedLabel(
                                    "facility.receive.list.product",
                                    "Product Code"
                                )}
                                width={100}
                                editable={false}
                            />
                            <Column
                                field="productName"
                                title={getTranslatedLabel(
                                    "facility.receive.list.product",
                                    "Product"
                                )}
                                width={400}
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
                                field="shippedQuantity"
                                title={getTranslatedLabel(
                                    "facility.receive.list.qtyShipped",
                                    "Shipped Quantity"
                                )}
                                width={100}
                            />
                            <Column
                                field="quantityToShip"
                                title={getTranslatedLabel(
                                    "facility.receive.list.qtyToShip",
                                    "Quantity to Ship"
                                )}
                                width={100}
                                editor="numeric"
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
                            <Column width={100} cell={PackCell} />
                        </KendoGrid>
                    </Grid>
                </Grid>
                {(isSalesOrdersLoading || isPackingDataLoading) && (
                    <LoadingComponent message="Loading Order..." />
                )}
                {isMutationLoading && (
                    <LoadingComponent message="Issuing Inventory..." />
                )}
            </Paper>
        </>
    );
};

export default PackingList;