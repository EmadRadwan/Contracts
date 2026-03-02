import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import { Button, Grid, Paper } from "@mui/material";
import MultiPaymentCertificateForm from "../form/MultiPaymentCertificateForm";
import { useAppDispatch } from "../../../app/store/configureStore";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { MultiPaymentCertificate } from "../../../app/models/project/MultiPaymentCertificate";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {
    useDeleteMultiPaymentCertificateMutation,
    useFetchMultiPaymentCertificatesQuery
} from "../../../app/store/apis/multiPaymentCertificateApi";
import AccountingMenu from "../../accounting/invoice/menu/AccountingMenu";
import {handleDatesArray} from "../../../app/util/utils";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import WorkEffortTransactionsList from "../../accounting/transaction/dashboard/WorkEffortTransactionsList";
import {Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle} from "@mui/material";
import {toast} from "react-toastify";

export default function MultiPaymentCertificatesList() {
    const [certificates, setCertificates] = useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = React.useState<State>({
        sort: [
            {
                field: "lastUpdatedStamp",
                dir: "desc",
            },
        ],
        skip: 0,
        take: 25,
    });
    const [formEditMode, setFormEditMode] = useState<number>(0);
    const [viewMode, setViewMode] = useState<"list" | "form">("list");
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "projects.multiPaymentCertificate.HeaderList";
    const [selectedWorkEffortIdForTransactions, setSelectedWorkEffortIdForTransactions] = useState<string | null>(null);
    const [paymentCertificate, setPaymentCertificate] = useState<MultiPaymentCertificate | null>(null);
    const { data, isFetching } = useFetchMultiPaymentCertificatesQuery({ ...dataState });

    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [certificateToDelete, setCertificateToDelete] = useState<string | null>(null);
    const [deleteCertificate, { isLoading: isDeleting }] = useDeleteMultiPaymentCertificateMutation();

    const handleDeleteClick = (workEffortId: string) => {
        setCertificateToDelete(workEffortId);
        setDeleteDialogOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!certificateToDelete) return;

        try {
            await deleteCertificate(certificateToDelete).unwrap();
            toast.success(
                getTranslatedLabel("projects.multiPaymentCertificate.list.deleteSuccess", "Certificate deleted successfully")
            );
        } catch (err: any) {
            let errorMessage = getTranslatedLabel("projects.multiPaymentCertificate.list.deleteFailed", "Failed to delete certificate");
            if (err?.data?.message) {
                errorMessage = err.data.message;
            }
            toast.error(errorMessage);
            console.error('Delete certificate failed:', err);
        } finally {
            setDeleteDialogOpen(false);
            setCertificateToDelete(null);
        }
    };

    const handleCancelDelete = () => {
        setDeleteDialogOpen(false);
        setCertificateToDelete(null);
    };

    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setCertificates({ data: adjustedData, total: data.total });
        }
    }, [data]);

    useEffect(() => {
        if (viewMode === "list") {
            setPaymentCertificate(null);
            setFormEditMode(0); // REFACTOR: Ensure editMode is reset when returning to list view
        }
    }, [viewMode]);
    
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const handleSelectCertificate = useCallback(
        (workEffortId?: string) => {
            if (!workEffortId) return;
            const selectedCert = certificates.data.find(
                (cert: MultiPaymentCertificate) => cert.workEffortId === workEffortId
            );
            if (!selectedCert) return;
            setPaymentCertificate(selectedCert);
            setFormEditMode(2);
            setViewMode("form");
        },
        [certificates.data]
    );

    const handleCreateNew = useCallback(() => {
        setPaymentCertificate(null);
        setFormEditMode(1);
        setViewMode("form");
    }, []);

    const cancelEdit = useCallback(() => {
        setPaymentCertificate(null);
        setFormEditMode(0);
        setViewMode("list");
    }, []);

    const CertificateNumberCell = (props: any) => {
        const field = props.field || "";
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: "blue" }}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button
                    onClick={() =>
                        handleSelectCertificate(props.dataItem.workEffortId)
                    }
                >
                    {value}
                </Button>
            </td>
        );
    };

    if (viewMode === "form" && formEditMode > 0) {
        return (
            <MultiPaymentCertificateForm
                selectedCertificate={paymentCertificate}
                cancelEdit={cancelEdit}
                editMode={formEditMode}
                setEditMode={setFormEditMode}
                setParentCertificate={setPaymentCertificate}
            />
        );
    }

    const columnWidths = {
        workEffortId: 150,
        date: 150,
        description: 350,
        accountName: 250,
        statusDescription: 120,
        notes: 100,
        partyName: 200,
    };

    const handleShowTransactions = (workEffortId: string) => {
        setSelectedWorkEffortIdForTransactions(workEffortId);
    };

    const ActionsCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);

        return (
            <td
                className={props.className}
                style={{ ...props.style, textAlign: "center" }}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <div style={{ display: "flex", gap: 8, justifyContent: "center" }}>
                    <Button
                        size="small"
                        color="info"
                        variant="outlined"
                        onClick={() => handleShowTransactions(props.dataItem.workEffortId)}
                    >
                        {getTranslatedLabel("projects.multiPaymentCertificate.transactions", "Transactions")}
                    </Button>
                    <Button
                        size="small"
                        color="error"
                        variant="outlined"
                        onClick={() => handleDeleteClick(props.dataItem.workEffortId)}
                        disabled={isDeleting}
                    >
                        {getTranslatedLabel("projects.multiPaymentCertificate.list.deleteButton", "Delete")}
                    </Button>
                </div>
            </td>
        );
    };

    return (
        <>
            <AccountingMenu selectedMenuItem={"/multiPaymentCertificates"} />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <KendoGrid
                            style={{ height: "65vh" }}
                            scrollable="scrollable"
                            resizable={true}
                            filterable={true}
                            sortable={true}
                            pageable={true}
                            {...dataState}
                            data={certificates ? certificates : { data: [], total: 0 }}
                            onDataStateChange={dataStateChange}
                        >
                            <GridToolbar>
                                <Button
                                    variant="contained"
                                    color="primary"
                                    onClick={handleCreateNew}
                                    style={{ margin: "5px" }}
                                >
                                    {getTranslatedLabel(`${localizationKey}.createNew`, "Create New Certificate")}
                                </Button>
                            </GridToolbar>
                            <Column
                                field="workEffortId"
                                title={getTranslatedLabel(`${localizationKey}.workEffortId`, "Certificate ID")}
                                width={columnWidths.workEffortId}
                                cell={CertificateNumberCell}
                            />
                            <Column
                                field="date"
                                title={getTranslatedLabel(`${localizationKey}.date`, "Date")}
                                format="{0: dd/MM/yyyy}"
                                width={columnWidths.date}
                            />
                            <Column
                                field="description"
                                title={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                width={columnWidths.description}
                            />
                            <Column
                                field="accountName"
                                title={getTranslatedLabel(`${localizationKey}.accountName`, "accountName")}
                                width={columnWidths.accountName}
                            />
                            <Column
                                field="partyName"
                                title={getTranslatedLabel(`${localizationKey}.paymentTo`, "Payment To")}
                                width={columnWidths.partyName}
                            />
                            <Column
                                field="notes"
                                title={getTranslatedLabel(`${localizationKey}.referenceNum`, "Reference Number")}
                                width={columnWidths.notes}
                            />
                            <Column
                                field="statusDescription"
                                title={getTranslatedLabel(`${localizationKey}.statusDescription`, "statusDescription")}
                                width={columnWidths.statusDescription}
                            />

                            <Column
                                title={getTranslatedLabel(`${localizationKey}.actions`, "Actions")}
                                width={220}
                                cell={ActionsCell}
                                locked={true}
                            />
                           
                        </KendoGrid>
                        <Dialog
                            open={deleteDialogOpen}
                            onClose={handleCancelDelete}
                            aria-labelledby="delete-certificate-dialog-title"
                            aria-describedby="delete-certificate-dialog-description"
                        >
                            <DialogTitle id="delete-certificate-dialog-title">
                                {getTranslatedLabel(
                                    "projects.multiPaymentCertificate.list.deleteDialogTitle",
                                    "Confirm Deletion"
                                )}
                            </DialogTitle>
                            <DialogContent>
                                <DialogContentText id="delete-certificate-dialog-description">
                                    {getTranslatedLabel(
                                        "projects.multiPaymentCertificate.list.deleteDialogMessage",
                                        "Are you sure you want to delete certificate {0}? This action cannot be undone."
                                    ).replace("{0}", certificateToDelete || "")}
                                </DialogContentText>
                            </DialogContent>
                            <DialogActions>
                                <Button onClick={handleCancelDelete} disabled={isDeleting}>
                                    {getTranslatedLabel("global.cancel", "Cancel")}
                                </Button>
                                <Button
                                    onClick={handleConfirmDelete}
                                    color="error"
                                    variant="contained"
                                    disabled={isDeleting}
                                    autoFocus
                                >
                                    {isDeleting
                                        ? getTranslatedLabel("global.deleting", "Deleting...")
                                        : getTranslatedLabel("projects.multiPaymentCertificate.list.deleteConfirm", "Delete")}
                                </Button>
                            </DialogActions>
                        </Dialog>
                        {isFetching && (
                            <LoadingComponent
                                message={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.loading",
                                    "Loading Certificates..."
                                )}
                            />
                        )}
                        {selectedWorkEffortIdForTransactions && (
                            <ModalContainer
                                show={!!selectedWorkEffortIdForTransactions}
                                onClose={() => setSelectedWorkEffortIdForTransactions(null)}
                                width={950}           // same as your payment transactions modal
                                title={getTranslatedLabel(
                                    "projects.multiPaymentCertificate.transactionsModalTitle",
                                    "Accounting Transactions for Certificate"
                                )}
                            >
                                <WorkEffortTransactionsList
                                    onClose={() => setSelectedWorkEffortIdForTransactions(null)}
                                    workEffortId={selectedWorkEffortIdForTransactions}
                                />
                            </ModalContainer>
                        )}
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}