import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { useCallback, useState, useMemo } from "react";
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

            toast.success(getTranslatedLabel("accounting.invoices.payroll.delete-success", "Item deleted successfully"));
            refreshTotal?.();
        } catch (err: any) {
            toast.error(getTranslatedLabel("accounting.invoices.payroll.delete-failed", "Failed to delete item"));
        } finally {
            setDeleteDialogOpen(false);
            setInvoiceItemToDelete(null);
        }
    }, [deleteInvoiceItem, invoiceItemToDelete, refreshTotal, getTranslatedLabel]);

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
                        {getTranslatedLabel("general.delete", "Delete")}
                    </Button>
                )}
            </td>
        );
    };

    if (!invoiceId) {
        return <div>{getTranslatedLabel("accounting.invoices.payroll.no-invoice-selected", "No invoice selected")}</div>;
    }

    const sortedItems = useMemo(() => {
        const items = [...(uiInvoiceItems || [])];
        const sorted = items.sort((a, b) => {
            if (a.invoiceItemTypeId === "PAYROL_SALARY") return -1;
            if (b.invoiceItemTypeId === "PAYROL_SALARY") return 1;
            return 0;
        });

        // Apply Kendo's sort but always keep PAYROL_SALARY first
        const ordered = orderBy(sorted, sort);
        const salaryIndex = ordered.findIndex(item => item.invoiceItemTypeId === "PAYROL_SALARY");
        if (salaryIndex > 0) {
            const [salaryItem] = ordered.splice(salaryIndex, 1);
            ordered.unshift(salaryItem);
        }
        return ordered;
    }, [uiInvoiceItems, sort]);

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
                        employeeId={employeeId?.fromPartyId}
                        invoiceDate={invoiceDate}
                    />
                </ModalContainer>
            )}
            <Grid container direction="column" alignItems="flex-start">
                <KendoGrid
                    style={{ minHeight: "300px" }}
                    data={sortedItems.slice(page.skip, page.take + page.skip)}
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
                            {getTranslatedLabel("accounting.invoices.payroll.manage-items", "Manage Payroll Items")}
                        </Button>
                    </GridToolbar>

                    <Column
                        field="invoiceItemTypeDescription"
                        title={getTranslatedLabel("accounting.invoices.payroll.item-type", "Item Type")}
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

                    <Column field="description" title={getTranslatedLabel("accounting.invoices.payroll.description", "Description")} width={300} />
                    <Column field="quantity" title={getTranslatedLabel("accounting.invoices.payroll.quantity", "Quantity/Hrs")} width={120} format="{0:n2}" filter={"numeric"} />
                    <Column field="amount" title={getTranslatedLabel("accounting.invoices.payroll.amount", "Amount")} width={120} format="{0:n2}" filter={"numeric"} />
                    
                </KendoGrid>

                {isLoading && <LoadingComponent message={getTranslatedLabel("accounting.invoices.payroll.loading", "Loading Payroll Items...")} />}
            </Grid>
        </>
    );
}
