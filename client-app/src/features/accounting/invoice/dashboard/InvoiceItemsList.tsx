import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useCallback, useState} from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {Grid} from "@mui/material";
import {useSelector} from "react-redux";
import { useFetchInvoiceItemsQuery,} from "../../../../app/store/configureStore";
import {InvoiceItem} from "../../../../app/models/accounting/invoiceItem";
import {nonDeletedInvoiceItemsSelector} from "../slice/invoiceSelectors";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import EditInvoiceItem from "../form/EditInvoiceItem";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {useDeleteInvoiceItemMutation} from "../../../../app/store/apis/invoice/invoiceItemsApi";
import {toast} from "react-toastify";
import {
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
} from "@mui/material";

interface Props {
    invoiceId: string | undefined;
    canEdit: boolean;
    refreshTotal?: () => Promise<void>;   // ← add this (optional for safety)
}

export default function InvoiceItemsList({invoiceId, canEdit, refreshTotal}: Props) {
    const initialSort: Array<SortDescriptor> = [
        {field: "invoiceItemSeqId", dir: "desc"},
    ];    
    const localizationKey = "accounting.invoices.display.form";

    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const [invoiceItem, setInvoiceItem] = useState<InvoiceItem | undefined>(undefined);
    const uiInvoiceItems: any = useSelector(nonDeletedInvoiceItemsSelector);
    const [editMode, setEditMode] = useState(0)
    const [show, setShow] = useState(false)
    const {getTranslatedLabel} = useTranslationHelper()
    const [deleteInvoiceItem, { isLoading: isDeleting }] = useDeleteInvoiceItemMutation();

    const {data: invoiceItemsData, error, isLoading} = useFetchInvoiceItemsQuery(invoiceId,
        {skip: invoiceId === undefined});

    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [invoiceItemToDelete, setInvoiceItemToDelete] = useState<{
        invoiceId: string;
        invoiceItemSeqId: string;
    } | null>(null);

    const handleDeleteClick = useCallback((invoiceId: string, invoiceItemSeqId: string) => {
        setInvoiceItemToDelete({ invoiceId, invoiceItemSeqId });
        setDeleteDialogOpen(true);
    }, []);

    const handleConfirmDelete = useCallback(async () => {
        if (!invoiceItemToDelete) return;

        try {
            await deleteInvoiceItem({
                invoiceId: invoiceItemToDelete.invoiceId,
                invoiceItemSeqId: invoiceItemToDelete.invoiceItemSeqId,
            }).unwrap();

            toast.success(
                getTranslatedLabel(
                    "accounting.invoices.display.form.deleteSuccess",
                    "تم حذف الصنف بنجاح"
                )
            );
        } catch (err: any) {
            // Try to get a nice message from backend
            const errorMsg =
                err?.data?.error ||
                err?.data?.message ||
                "Failed to delete invoice item";
            toast.error(
                err?.data?.error ||
                getTranslatedLabel(
                    "accounting.invoices.display.form.deleteFailed",
                    "فشل حذف الصنف"
                )
            );
        } finally {
            // Always clean up
            setDeleteDialogOpen(false);
            setInvoiceItemToDelete(null);
        }
    }, [deleteInvoiceItem, invoiceItemToDelete]);

    const handleCancelDelete = useCallback(() => {
        setDeleteDialogOpen(false);
        setInvoiceItemToDelete(null);
    }, []);

    const handleSelectInvoiceItem = useCallback(
        (dataItem: InvoiceItem) => {
            // Purpose: Disables edit functionality when canEdit is false
            // Improvement: Aligns with Add Invoice Item button permissions
            if (!canEdit) return;

            console.log('Selected Invoice Item:', dataItem);
            const selectedItem = uiInvoiceItems.find(
                (item: InvoiceItem) =>
                    item.invoiceId === dataItem.invoiceId && item.invoiceItemSeqId === dataItem.invoiceItemSeqId
            );
            console.log('Found Selected Item:', selectedItem);

            const invoiceItemToDisplay: InvoiceItem = {
                ...selectedItem,
                productId: selectedItem?.invoiceItemProduct || selectedItem?.productId,
            };
            setInvoiceItem(invoiceItemToDisplay);
            setEditMode(2);
            setShow(true);
        },
        [uiInvoiceItems, canEdit]
    );

    const handleCloseModal = useCallback(() => {
        setShow(false);
        setInvoiceItem(undefined);
        setEditMode(0);
    }, []);

    const memoizedOnClose = useCallback(
        () => {
            setShow(false)
        },
        [],
    );

    const invoiceItemCell = (props: any) => {
        const { dataItem } = props;
        const hasProduct = !!dataItem.productName || !!dataItem.productId || !!dataItem.invoiceItemProduct;

        const displayText = hasProduct
            ? dataItem.productName || "Unnamed Product"
            : "— Service / Fee —";   // or "No product", "Manual item", ""

        return (
            <td style={{ padding: "4px 8px" }}>
                <Button
                    variant="text"
                    size="small"
                    onClick={() => handleSelectInvoiceItem(dataItem)}
                    disabled={!canEdit}
                    sx={{
                        minWidth: "auto",
                        padding: "2px 6px",
                        textTransform: "none",
                        fontWeight: hasProduct ? 500 : 400,
                        color: hasProduct ? "inherit" : "text.secondary",
                        justifyContent: "flex-start",
                        width: "100%",
                        textAlign: "left",
                    }}
                >
                    {displayText}
                </Button>
            </td>
        );
    };

    const actionCell = (props: any) => {
        const { dataItem } = props;

        return (
            <td style={{ padding: "4px 8px", textAlign: "center" }}>
                {canEdit && (
                    <Button
                        size="small"
                        color="error"
                        variant="text"
                        onClick={() => handleDeleteClick(dataItem.invoiceId, dataItem.invoiceItemSeqId)}
                        disabled={isDeleting}
                        sx={{ minWidth: "auto", p: "2px 6px" }}
                    >
                        {isDeleting
                            ? getTranslatedLabel("general.deleting", "جارٍ الحذف...")
                            : getTranslatedLabel("accounting.invoices.display.form.actions.deleteItem", "حذف الصنف")
                        }
                    </Button>
                )}
            </td>
        );
    };


    if (!invoiceId) {
        return <div>No invoice selected</div>;
    }
    
    return (
        <>
        {show && (<ModalContainer show={show} onClose={memoizedOnClose} width={800}>
            <EditInvoiceItem
                invoiceItem={invoiceItem}
                editMode={editMode}
                onClose={handleCloseModal}
                invoiceId={invoiceId}
                refreshTotal={refreshTotal}
            />
            </ModalContainer>)}
            <Grid container columnSpacing={1} direction={"column"} alignItems="flex-start">

                <KendoGrid className="main-grid" style={{height: "40vh"}}
                           data={orderBy(uiInvoiceItems ? uiInvoiceItems : [], sort).slice(page.skip, page.take + page.skip)}
                           sortable={true}
                           sort={sort}
                           onSortChange={(e: GridSortChangeEvent) => {
                               setSort(e.sort);
                           }}
                           skip={page.skip}
                           resizable={true}
                           take={page.take}
                           total={uiInvoiceItems ? uiInvoiceItems.length : 0}
                           pageable={true}
                           onPageChange={pageChange}
                >
                    <GridToolbar>
                        <Grid item xs={4}>
                            <Button
                                onClick={() => {
                                    // Purpose: Ensures modal only opens when editing is allowed
                                    // Improvement: Consistent with edit button behavior
                                    if (!canEdit) return;
                                    setEditMode(1);
                                    setShow(true);
                                    setInvoiceItem(undefined);
                                }}
                                variant="outlined"
                                disabled={!canEdit}
                                color="secondary"
                            >
                                Add Invoice Item
                            </Button>
                        </Grid>
                    </GridToolbar>

                    <Column
                        field="productName"
                        title={getTranslatedLabel(`${localizationKey}.columns.product`, "Product")}
                        cell={invoiceItemCell}
                        width={250}
                    />

                    <Column
                        field="invoiceItemTypeDescription"
                        title={getTranslatedLabel(`${localizationKey}.columns.item-type-desc`, "Item Type Description")}
                        width={230}
                    />

                    <Column
                        field="amount"
                        title={getTranslatedLabel(`${localizationKey}.columns.amount`, "Amount")}
                        width={100}
                        filter={"numeric"}
                    />

                    <Column
                        field="quantity"
                        title={getTranslatedLabel(`${localizationKey}.columns.quantity`, "Quantity")}
                        width={100}
                        filter={"numeric"}
                    />
                    <Column
                        field="overrideGlAccountId"
                        title={getTranslatedLabel(`${localizationKey}.columns.overrideGlAccountId`, "overrideGlAccountId")}
                        width={100}
                    />

                    <Column
                        field="description"
                        title={getTranslatedLabel(`${localizationKey}.columns.description`, "Description")}
                        width={200}
                    />
                    <Column
                        title=" "
                        width={110}
                        cell={actionCell}
                        filterable={false}
                        sortable={false}
                    />
                </KendoGrid>
                <Dialog
                    open={deleteDialogOpen}
                    onClose={handleCancelDelete}
                    aria-labelledby="delete-invoice-item-title"
                >
                    <DialogTitle id="delete-invoice-item-title">
                        {getTranslatedLabel(
                            "accounting.invoices.display.form.deleteDialogTitle",
                            "تأكيد الحذف"
                        )}
                    </DialogTitle>

                    <DialogContent>
                        <DialogContentText>
                            {getTranslatedLabel(
                                "accounting.invoices.display.form.deleteDialogMessage",
                                "هل أنت متأكد من حذف هذا الصنف من الفاتورة؟ لا يمكن التراجع عن هذا الإجراء."
                            )}
                        </DialogContentText>
                    </DialogContent>

                    <DialogActions>
                        <Button onClick={handleCancelDelete} disabled={isDeleting} color="inherit">
                            {getTranslatedLabel("general.cancel", "إلغاء")}
                        </Button>

                        <Button
                            onClick={handleConfirmDelete}
                            color="error"
                            variant="contained"
                            disabled={isDeleting}
                            autoFocus
                        >
                            {isDeleting
                                ? getTranslatedLabel("general.deleting", "جارٍ الحذف...")
                                : getTranslatedLabel("accounting.invoices.display.form.deleteConfirm", "حذف")
                            }
                        </Button>
                    </DialogActions>
                </Dialog>

                {isLoading && <LoadingComponent
                    message={getTranslatedLabel(`${localizationKey}.loading`, "Loading Invoice Items...")}
                />}
            </Grid>
        </>

    )
}