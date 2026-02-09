// app/features/employees/advances/EmployeeAdvancesList.tsx
import React from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GRID_COL_INDEX_ATTRIBUTE,
    GridDataStateChangeEvent
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Button } from "@mui/material";
import {DataResult, State} from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { handleDatesArray } from "../../../app/util/utils";
import {EmployeeAdvance} from "../../../app/models/humanResources/employeeAdvance";
import {useFetchEmployeeAdvancesQuery} from "../../../app/store/apis";
import EmployeeAdvanceMenu from "../menu/EmployeeAdvanceMenu";

function EmployeeAdvancesList() {
    const [viewMode, setViewMode] = React.useState<"list" | "form">("list");
    const [editMode, setEditMode] = React.useState(0); // 0=list, 1=create, 2=edit, 3=closed/paid
    const [selectedAdvance, setSelectedAdvance] = React.useState<EmployeeAdvance | undefined>();
    const [dataState, setDataState] = React.useState<State>({ take: 9, skip: 0 });
    const [eAdvances, setEAdvances] = React.useState<DataResult>({
        data: [],
        total: 0,
    });
    const { getTranslatedLabel } = useTranslationHelper();
    const { data, isFetching } = useFetchEmployeeAdvancesQuery({...dataState});

    React.useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setEAdvances({ data: adjustedData, total: data!.total});
        }
    }, [data]);
    

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const startEdit = (adv?: EmployeeAdvance) => {
        if (!adv) {
            setEditMode(1); // create
            setSelectedAdvance(undefined);
        } else {
            const isClosed = adv.StatusId === "ADVANCE_PAID" || adv.StatusId === "ADVANCE_CANCELLED";
            setEditMode(isClosed ? 3 : 2);
            setSelectedAdvance(adv);
        }
        setViewMode("form");
    };

    const cancelEdit = () => {
        setEditMode(0);
        setSelectedAdvance(undefined);
        setViewMode("list");
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

    /*if (viewMode === "form" && editMode !== 0) {
        return (
            <EmployeeAdvanceForm
                advance={editMode === 1 ? undefined : selectedAdvance}
                editMode={editMode}
                cancelEdit={cancelEdit}
                onAdvanceCreated={(newAdv) => {
                    setSelectedAdvance(newAdv);
                    setEditMode(2);
                }}
            />
        );
    }*/

    const handleMenuSelect = (key: string) => {
        if (key === "employeeAdvance.menu.advances") {
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
                                {getTranslatedLabel("employeeAdvance.list.create", "Create Advance")}
                            </Button>
                        </GridToolbar>

                        <Column field="advanceId" title="ID" cell={IdCell} width={130} />
                        <Column field="employeeName" title={getTranslatedLabel("employeeAdvance.list.employee", "Employee")} width={220} />
                        <Column field="advanceDate" title="Date" format="{0:dd/MM/yyyy}" filter="date" width={140} />
                        <Column field="amount" title="Amount" format="{0:n2}" width={130} />
                        <Column field="installmentCount" title="Installments" width={120} />
                        <Column field="installmentAmount" title="Monthly" format="{0:n2}" width={130} />
                        <Column field="statusDescription" title="Status" width={160} />
                        <Column field="description" title="Notes" />
                    </KendoGrid>

                    {isFetching && <LoadingComponent />}
                </div>
            </Paper>
        </>
    );
}

export default EmployeeAdvancesList;