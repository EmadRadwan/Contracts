import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { useCallback, useState } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { Grid, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
import { useSelector } from "react-redux";
import { useFetchInvoiceItemsQuery } from "../../../../app/store/configureStore";
import { InvoiceItem } from "../../../../app/models/accounting/invoiceItem";
import { nonDeletedInvoiceItemsSelector } from "../slice/invoiceSelectors";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import EditPayrollInvoiceItem from "./EditPayrollInvoiceItem";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useDeleteInvoiceItemMutation } from "../../../../app/store/apis/invoice/invoiceItemsApi";
import { toast } from "react-toastify";

interface Props {
    invoiceId: string | undefined;
    canEdit: boolean;
    refreshTotal?: () => Promise<void>;
    employeeId: string;
    invoiceDate: string;
}

export default function PayrollInvoiceItemsList({ invoiceId, canEdit, refreshTotal, employeeId, invoiceDate }: Props) {
    const initialSort: Array<SortDescriptor> = [
        { field: "invoiceItemSeqId", dir: "desc" },
    ];
    const localizationKey = "accounting.invoices.display.form";

    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = { skip: 0, take: 10 };
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    
    const [invoiceItem, setInvoiceItem] = useState<InvoiceItem | undefined>(undefined);
    const uiInvoiceItems: any = useSelector(nonDeletedInvoiceItemsSelector);
    const [editMode, setEditMode] = useState(0);
    const [show, setShow] = useState(false);
    const { getTranslatedLabel } = useTranslationHelper();
    const [deleteInvoiceItem, { isLoading: isDeleting }] = useDeleteInvoiceItemMutation();

    const { isLoading } = useFetchInvoiceItemsQuery(invoiceId, { skip: invoiceId === undefined });

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

            toast.success("Item deleted successfully");
            refreshTotal?.();
        } catch (err: any) {
            toast.error("Failed to delete item");
        } finally {
            setDeleteDialogOpen(false);
            setInvoiceItemToDelete(null);
        }
    }, [deleteInvoiceItem, invoiceItemToDelete, refreshTotal]);

    const handleSelectInvoiceItem = useCallback(
        (dataItem: InvoiceItem) => {
            if (!canEdit) return;

            const selectedItem = uiInvoiceItems.find(
                (item: InvoiceItem) =>
                    item.invoiceId === dataItem.invoiceId && item.invoiceItemSeqId === dataItem.invoiceItemSeqId
            );

            setInvoiceItem(selectedItem);
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
                    >
                        Delete
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
            {show && (
                <ModalContainer show={show} onClose={handleCloseModal} width={1000}>
                    <EditPayrollInvoiceItem
                        invoiceItem={invoiceItem}
                        editMode={editMode}
                        onClose={handleCloseModal}
                        invoiceId={invoiceId}
                        refreshTotal={refreshTotal}
                        employeeId={employeeId}
                        invoiceDate={invoiceDate}
                    />
                </ModalContainer>
            )}
            <Grid container direction="column" alignItems="flex-start">
                <KendoGrid
                    style={{ height: "40vh" }}
                    data={orderBy(uiInvoiceItems || [], sort).slice(page.skip, page.take + page.skip)}
                    sortable={true}
                    sort={sort}
                    onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                    skip={page.skip}
                    take={page.take}
                    total={uiInvoiceItems ? uiInvoiceItems.length : 0}
                    pageable={true}
                    onPageChange={pageChange}
                >
                    <GridToolbar>
                        <Button
                            onClick={() => {
                                if (!canEdit) return;
                                setEditMode(1);
                                setShow(true);
                                setInvoiceItem(undefined);
                            }}
                            variant="outlined"
                            disabled={!canEdit}
                            color="secondary"
                        >
                            Manage Payroll Items
                        </Button>
                    </GridToolbar>

                    <Column
                        field="invoiceItemTypeDescription"
                        title="Item Type"
                        width={250}
                        cell={(props) => (
                            <td>
                                <Button 
                                    variant="text" 
                                    size="small" 
                                    onClick={() => handleSelectInvoiceItem(props.dataItem)}
                                    disabled={!canEdit}
                                    sx={{ textTransform: 'none', justifyContent: 'flex-start', width: '100%' }}
                                >
                                    {props.dataItem.invoiceItemTypeDescription}
                                </Button>
                            </td>
                        )}
                    />

                    <Column field="description" title="Description" width={300} />
                    <Column field="quantity" title="Quantity/Hrs" width={120} format="{0:n2}" />
                    <Column field="amount" title="Amount" width={120} format="{0:n2}" />
                    
                    <Column
                        title="Actions"
                        width={110}
                        cell={actionCell}
                    />
                </KendoGrid>

                <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
                    <DialogTitle>Confirm Delete</DialogTitle>
                    <DialogContent>
                        <DialogContentText>
                            Are you sure you want to delete this payroll item?
                        </DialogContentText>
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={() => setDeleteDialogOpen(false)} color="inherit">Cancel</Button>
                        <Button onClick={handleConfirmDelete} color="error" variant="contained">Delete</Button>
                    </DialogActions>
                </Dialog>

                {isLoading && <LoadingComponent message="Loading Payroll Items..." />}
            </Grid>
        </>
    );
}
