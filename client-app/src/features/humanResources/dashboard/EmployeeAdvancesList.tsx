import React from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GRID_COL_INDEX_ATTRIBUTE,
    GridDataStateChangeEvent
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Paper, Button, Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions } from "@mui/material";
import { toast } from "react-toastify";
import {DataResult, State} from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { handleDatesArray } from "../../../app/util/utils";
import {EmployeeAdvance} from "../../../app/models/humanResources/employeeAdvance";
import {
    useFetchEmployeeAdvancesQuery,
    useLazyGetEmployeeAdvanceDetailQuery,
    useDeleteEmployeeAdvanceMutation
} from "../../../app/store/apis";
import EmployeeAdvanceForm from "../form/EmployeeAdvanceForm";
import EmployeeAdvanceMenu from "../menu/EmployeeAdvanceMenu";
import { EmployeeAdvancesDateRangeExcel } from "../report/EmployeeAdvancesDateRangeExcel";

function EmployeeAdvancesList() {
    const [viewMode, setViewMode] = React.useState<"list" | "form">("list");
    const [editMode, setEditMode] = React.useState(0); // 0=list, 1=create, 2=edit, 3=closed/paid
    const [selectedAdvance, setSelectedAdvance] = React.useState<EmployeeAdvance | undefined>();
    const [dataState, setDataState] = React.useState<State>({
        sort: [
            {
                field: "advanceDate",
                dir: "desc",
            },
        ],
        skip: 0,
        take: 25,
    });
    const [eAdvances, setEAdvances] = React.useState<DataResult>({
        data: [],
        total: 0,
    });
    const { getTranslatedLabel } = useTranslationHelper();
    const { data, isFetching } = useFetchEmployeeAdvancesQuery({...dataState});
    const [triggerGetDetail] = useLazyGetEmployeeAdvanceDetailQuery();
    const [deleteAdvance] = useDeleteEmployeeAdvanceMutation();
    const [confirmDialogOpen, setConfirmDialogOpen] = React.useState(false);
    const [advanceToDelete, setAdvanceToDelete] = React.useState<EmployeeAdvance | null>(null);
    
    React.useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setEAdvances({ data: adjustedData, total: data!.total});
        }
    }, [data]);
    

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const startEdit = async (adv?: EmployeeAdvance) => {
        if (!adv) {
            // Create new advance
            setEditMode(1);
            setSelectedAdvance(undefined);
            setViewMode("form");
            return;
        }

        // ────────────────────────────────────────────────
        // Quick check using the lightweight list data
        // ────────────────────────────────────────────────
        const isLongTerm = adv.advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE";

        let fullDetail = adv; // default to the list item (enough for regular advances)

        if (isLongTerm) {
            // Only fetch detailed version (with schedules) for long-term advances
            try {
                const result = await triggerGetDetail(adv.advanceId).unwrap();
                fullDetail = result;

                // Optional: toast loading feedback if fetch takes time
                // if (isDetailLoading) toast.info("Loading deduction plan...");
            } catch (err) {
                console.error("Failed to load full advance detail:", err);
                return; // don't open form if critical data is missing
            }
        }

        // Now decide mode based on status
        const isClosed = fullDetail.statusId === "ADVANCE_FULLY_PAID" 
            || fullDetail.statusId === "ADVANCE_CANCELLED"
            || fullDetail.statusId === "ADVANCE_REJECTED";
        setEditMode(isClosed ? 3 : 2);
        setSelectedAdvance(fullDetail);
        setViewMode("form");
    };

    const cancelEdit = () => {
        setEditMode(0);
        setSelectedAdvance(undefined);
        setViewMode("list");
    };

    const hasSentPayment = advanceToDelete?.paymentStatusId === "PMNT_SENT";

    const handleDelete = async (advance: EmployeeAdvance) => {
        setAdvanceToDelete(advance);
        setConfirmDialogOpen(true);
    };

    const confirmDelete = async () => {
        if (advanceToDelete) {
            try {
                await deleteAdvance({ advanceId: advanceToDelete.advanceId, dropPayment: hasSentPayment }).unwrap();
                toast.success(getTranslatedLabel("general.deleteSuccess", "Record deleted successfully"));
            } catch (err: any) {
                const message = err?.data?.title || err?.data?.error || getTranslatedLabel("general.deleteFailed", "Delete failed");
                toast.error(message);
                console.error("Delete failed:", err);
            } finally {
                setConfirmDialogOpen(false);
                setAdvanceToDelete(null);
            }
        }
    };

    const IdCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td {...navigationAttributes} style={{ color: "blue" }} {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}>
                <Button onClick={() => startEdit(props.dataItem)} variant="text">
                    {props.dataItem.advanceId}
                </Button>
            </td>
        );
    };

    const DeleteCell = (props: any) => {
        const isClosed = props.dataItem.statusId === "ADVANCE_CANCELLED"
            || props.dataItem.statusId === "ADVANCE_REJECTED";

        return (
            <td>
                <Button
                    color="error"
                    size="small"
                    onClick={() => handleDelete(props.dataItem as EmployeeAdvance)}
                    disabled={isClosed}
                >
                    {getTranslatedLabel("general.delete", "Delete")}
                </Button>
            </td>
        );
    };

    if (viewMode === "form" && editMode !== 0) {
        return (
            <EmployeeAdvanceForm
                advance={editMode === 1 ? undefined : selectedAdvance}
                editMode={editMode}
                cancelEdit={cancelEdit}
                onAdvanceCreated={(newAdv) => {
                    setSelectedAdvance(newAdv);
                    setEditMode(2);
                }}
                onAdvanceUpdated={(updated) => {
                    setSelectedAdvance(updated);
                }}
            />
        );
    }

    const handleMenuSelect = (key: string) => {
        if (key === "party.employeeAdvance.menu.advances") {
            setViewMode("list");
            setEditMode(0);
            setSelectedAdvance(undefined);
        }
    };

    return (
        <>
            <EmployeeAdvanceMenu
                selectedMenuItem="/employee-advances"
                onMenuSelect={handleMenuSelect}   // your function that sets viewMode="list" etc.
            />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <div className="div-container">
                    <KendoGrid
                        style={{ height: "70vh" }}
                        data={eAdvances ? eAdvances : { data: [], total: 0 }}
                        resizable
                        filterable
                        sortable
                        pageable
                        {...dataState}
                        onDataStateChange={dataStateChange}
                    >
                        <GridToolbar>
                            <Button
                                variant="outlined"
                                color="secondary"
                                onClick={() => startEdit()}
                            >
                                {getTranslatedLabel("party.employeeAdvance.list.create", "Create Advance")}
                            </Button>
                            <EmployeeAdvancesDateRangeExcel
                                getTranslatedLabel={getTranslatedLabel}
                            />
                        </GridToolbar>

                        <Column field="advanceId" title={getTranslatedLabel("party.employeeAdvance.list.id", "ID")} cell={IdCell} width={130} />
                        <Column field="employeeName" title={getTranslatedLabel("party.employeeAdvance.list.employee", "Employee")} width={220} />
                        <Column field="advanceDate" title={getTranslatedLabel("party.employeeAdvance.list.date", "Date")} format="{0:dd/MM/yyyy}" filter="date" width={140} />
                        <Column field="amount" title={getTranslatedLabel("party.employeeAdvance.list.amount", "Amount")} format="{0:n2}" width={130} />
                        <Column field="advanceTypeDescription" title={getTranslatedLabel("party.employeeAdvance.list.advanceTypeDescription", "Advance Type")} format="{0:n2}" width={130} />
                        <Column field="statusDescription" title={getTranslatedLabel("party.employeeAdvance.list.status", "Status")} width={160} />
                        <Column field="description" title={getTranslatedLabel("party.employeeAdvance.list.notes", "Notes")} />
                        <Column cell={DeleteCell} width={100} filterable={false} sortable={false} />
                    </KendoGrid>

                    {isFetching && <LoadingComponent />}
                </div>
            </Paper>

            <Dialog
                open={confirmDialogOpen}
                onClose={() => setConfirmDialogOpen(false)}
                aria-labelledby="alert-dialog-title"
                aria-describedby="alert-dialog-description"
            >
                <DialogTitle id="alert-dialog-title">
                    {getTranslatedLabel("general.confirmDeleteTitle", "Confirm Delete")}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText id="alert-dialog-description">
                        {hasSentPayment
                            ? getTranslatedLabel(
                                "party.employeeAdvance.delete.hasSentPayment",
                                "هذه السلفة مرتبطة بدفعة مُرسلة. سيتم حذف السلفة والدفعة وجميع القيود المحاسبية المرتبطة بها. هل تريد المتابعة؟"
                            )
                            : getTranslatedLabel("general.confirmDelete", "Are you sure you want to delete this record?")}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setConfirmDialogOpen(false)} color="primary">
                        {getTranslatedLabel("general.cancel", "Cancel")}
                    </Button>
                    <Button onClick={confirmDelete} color="error" autoFocus>
                        {getTranslatedLabel("general.delete", "Delete")}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};

export default EmployeeAdvancesList;