import React, { useEffect, useState } from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Button } from "@mui/material";
import { DataResult, State } from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import { useFetchProjectCommissionRatesQuery } from "../../../../../app/store/apis/projectsApi";
import { ProjectCommissionRate } from "../../../../../app/models/orders/projectCommissionRate";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import ProjectCommissionRateForm from "../form/ProjectCommissionRateForm";

const SALE_TYPE_LABELS: Record<string, string> = {
    COMM_SALE_DIRECT: "بيع مباشر",
    COMM_SALE_PERSONAL: "بيع شخصي",
    COMM_SALE_INDIRECT: "بيع غير مباشر",
};

export default function ProjectCommissionRatesList() {
    const [editMode, setEditMode] = useState(0);
    const [selectedRate, setSelectedRate] = useState<ProjectCommissionRate | undefined>(undefined);
    const [rates, setRates] = useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = useState<State>({ take: 10, skip: 0 });

    const { data, isFetching } = useFetchProjectCommissionRatesQuery({ ...dataState });
    const { getTranslatedLabel } = useTranslationHelper();

    useEffect(() => {
        if (data) {
            const total = typeof data.total === "object" ? (data.total as any).total : data.total;
            setRates({ data: data.data, total: Number(total) });
        }
    }, [data]);

    function handleSelectRate(rateId: string) {
        const rate = rates.data.find((r: ProjectCommissionRate) => r.projectCommissionRateId === rateId);
        setSelectedRate(rate);
        setEditMode(2);
    }

    function cancelEdit() {
        setEditMode(0);
        setSelectedRate(undefined);
    }

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const RateIdCell = (props: any) => {
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
                <Button onClick={() => handleSelectRate(props.dataItem.projectCommissionRateId)}>
                    {props.dataItem.projectCommissionRateId}
                </Button>
            </td>
        );
    };

    const SaleTypeCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        const label = SALE_TYPE_LABELS[props.dataItem.saleTypeId] ?? props.dataItem.saleTypeId;
        return (
            <td
                className={props.className}
                style={props.style}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                {label}
            </td>
        );
    };

    if (editMode) {
        return (
            <ProjectCommissionRateForm
                rate={selectedRate}
                editMode={editMode}
                cancelEdit={cancelEdit}
            />
        );
    }

    return (
        <>
            <SalesRequestMenu selectedMenuItem="project-commission-rates" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <KendoGrid
                    style={{ height: "75vh", width: "94vw", flex: 1 }}
                    resizable
                    filterable
                    sortable
                    pageable
                    {...dataState}
                    data={rates}
                    onDataStateChange={dataStateChange}
                >
                    <GridToolbar>
                        <Grid container>
                            <Grid item xs={5}>
                                <Button
                                    color="secondary"
                                    onClick={() => setEditMode(1)}
                                    variant="outlined"
                                >
                                    {getTranslatedLabel("commissionRate.list.create", "Create Commission Rate")}
                                </Button>
                            </Grid>
                        </Grid>
                    </GridToolbar>
                    <Column
                        field="projectCommissionRateId"
                        title={getTranslatedLabel("commissionRate.list.id", "Rate ID")}
                        cell={RateIdCell}
                        width={160}
                        locked
                    />
                    <Column
                        field="projectName"
                        title={getTranslatedLabel("commissionRate.list.project", "Project")}
                        width={280}
                    />
                    <Column
                        field="saleTypeId"
                        title={getTranslatedLabel("commissionRate.list.saleType", "Sale Type")}
                        cell={SaleTypeCell}
                        width={180}
                    />
                    <Column
                        field="salesRepPercent"
                        title={getTranslatedLabel("commissionRate.list.salesRepPercent", "Sales Rep %")}
                        width={130}
                        format="{0:n4}"
                    />
                    <Column
                        field="managerPercent"
                        title={getTranslatedLabel("commissionRate.list.managerPercent", "Manager %")}
                        width={130}
                        format="{0:n4}"
                    />
                    <Column
                        field="externalCompanyPercent"
                        title={getTranslatedLabel("commissionRate.list.extCompanyPercent", "Ext. Company %")}
                        width={150}
                        format="{0:n4}"
                    />
                    <Column
                        field="externalSalesRepPercent"
                        title={getTranslatedLabel("commissionRate.list.extSalesRepPercent", "Ext. Sales Rep %")}
                        width={160}
                        format="{0:n4}"
                    />
                    <Column
                        field="externalManagerPercent"
                        title={getTranslatedLabel("commissionRate.list.extManagerPercent", "Ext. Manager %")}
                        width={150}
                        format="{0:n4}"
                    />
                </KendoGrid>
                {isFetching && (
                    <LoadingComponent message={getTranslatedLabel("commissionRate.list.loading", "Loading Commission Rates...")} />
                )}
            </Paper>
        </>
    );
}
