import CRMMenu from '../menu/CRMMenu'
import React, { useState } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GRID_COL_INDEX_ATTRIBUTE,
    GridDataStateChangeEvent
} from "@progress/kendo-react-grid";

import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Typography } from "@mui/material";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { State } from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import CreateContractorForm from "../form/CreateContractorForm";

const CRMDashboard = () => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [editMode, setEditMode] = useState(0);
    const [dataState, setDataState] = React.useState<State>({ take: 6, skip: 0 });
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const localizationKey = 'crm.leads.list'

    const PartyDescriptionCell = (props: any) => {
        const field = props.field || '';
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: 'blue' }}
                colSpan={props.colSpan}
                role={'gridcell'}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{
                    [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex
                }}
                {...navigationAttributes}
            ><Button
                onClick={() => {
                    const startAt = props.dataItem.description.indexOf('(')
                    const endAt = props.dataItem.description.indexOf(')')
                    setEditMode(2);

                }}
            >
                    {props.dataItem.description}
                </Button>

            </td>
        )
    }
    return (
        <>
            <CRMMenu selectedMenuItem='sales-opportunities' />
            <Paper elevation={5} className={`div-container-withBorderCurved`} style={{ marginTop: 15 }}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={8}>

                        <Grid container>
                            <div className="div-container">
                                <KendoGrid
                                    style={{ height: "75vh", width: "94vw", flex: 1 }}
                                    resizable={true}
                                    filterable={true}
                                    sortable={true}
                                    pageable={true}
                                    {...dataState}
                                    data={{ data: [], total: 0 }}
                                    onDataStateChange={dataStateChange}
                                >
                                    <GridToolbar>
                                        <Grid container>
                                            <Button color={"secondary"} onClick={() => {
                                                setEditMode(1);
                                            }}
                                                variant="outlined">
                                                {getTranslatedLabel(`${localizationKey}.createLead`, "Create New Lead")}
                                            </Button>
                                        </Grid>


                                    </GridToolbar>
                                    <Column
                                        field="description"
                                        title={getTranslatedLabel(`${localizationKey}.description`, "Party")}
                                        cell={PartyDescriptionCell}
                                        width={300}
                                        locked={true}
                                    />
                                    <Column
                                        field="mobileLeadNumber"
                                        title={getTranslatedLabel(`${localizationKey}.leadNumber`, "Lead Number")}
                                    />
                                    <Column
                                        field="address1"
                                        title={getTranslatedLabel(`${localizationKey}.address`, "Address")}
                                    />
                                    <Column
                                        field="infoString"
                                        title={getTranslatedLabel(`${localizationKey}.email`, "Email")}
                                    />
                                    <Column
                                        field="partyTypeDescription"
                                        title={getTranslatedLabel(`${localizationKey}.partyType`, "Party Type")}
                                    />

                                </KendoGrid>
                                {false && <LoadingComponent message={getTranslatedLabel("crm.leads.list.loading", "Loading Leads...")} />}
                            </div>

                        </Grid>
                    </Grid>
                </Grid>
            </Paper>
        </>
    )
}

export default CRMDashboard