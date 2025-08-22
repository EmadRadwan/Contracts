import React, { useState } from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { ExcelExport } from "@progress/kendo-react-excel-export";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Button } from "@mui/material";
import { useAppDispatch, useFetchProjectsQuery } from "../../../app/store/configureStore";
import { setProjectId } from "../slice/projectUiSlice";
import { WorkEffort } from "../../../app/models/workEffort";
import ProjectMenu from "../menu/ProjectMenu";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { State } from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

// REFACTOR: Updated ProjectsList to use OData syntax with dataState and onDataStateChange, mirroring PartiesList for filtering, sorting, and pagination.
// Integrated ExcelExport for grid data export, maintaining consistency with PartiesList.
// Kept existing fields and styling, ensuring seamless integration with useFetchProjectsQuery and /odata/projectRecords endpoint.
export default function ProjectsList() {
    const [editMode, setEditMode] = useState(0);
    const [project, setProject] = useState<WorkEffort | undefined>(undefined);
    const [dataState, setDataState] = React.useState<State>({ take: 10, skip: 0 });
    const { data: projects, isFetching } = useFetchProjectsQuery(dataState);
    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();

    // REFACTOR: Centralized project selection logic to dispatch projectId and navigate to edit form, consistent with PartiesList's handleSelectParty.
    function handleSelectProject(projectId: string) {
        const selectedProject = projects?.data.find((project: WorkEffort) => project.WorkEffortId === projectId);
        dispatch(setProjectId(projectId));
        setProject(selectedProject);
        setEditMode(2); // Edit mode
    }

    function cancelEdit() {
        setEditMode(0);
    }

    // REFACTOR: Added dataStateChange to update OData query parameters dynamically, enabling client-side filtering and sorting.
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    // REFACTOR: Customized ProjectNum cell with clickable button, aligning with PartiesList's PartyDescriptionCell pattern.
    const ProjectNumCell = (props: any) => {
        const field = props.field || "";
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
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
                <Button onClick={() => handleSelectProject(props.dataItem.WorkEffortId)}>
                    {props.dataItem.ProjectNum}
                </Button>
            </td>
        );
    };

    // REFACTOR: Added ExcelExport for exporting project data, consistent with PartiesList's export functionality.
    const dataToExport = projects ? projects.data : [];
    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            _export.current!.save();
        }
    };

    // REFACTOR: Conditionally render ProjectForm for edit/create modes, aligning with PartiesList's form navigation.
    if (editMode) {
        return <ProjectForm project={project} cancelEdit={cancelEdit} editMode={editMode} />;
    }

    return (
        <>
            <ProjectMenu />
            <Paper elevation={5} className={`div-container-withBorderCurved`} style={{ marginTop: 15 }}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={8}>
                        <div className="div-container">
                            <ExcelExport data={dataToExport} ref={_export}>
                                <KendoGrid
                                    style={{ height: "75vh", width: "94vw", flex: 1 }}
                                    resizable={true}
                                    filterable={true}
                                    sortable={true}
                                    pageable={true}
                                    {...dataState}
                                    data={projects ? projects : { data: [], total: 0 }}
                                    onDataStateChange={dataStateChange}
                                >
                                    <GridToolbar>
                                        <Grid container>
                                            <Grid item xs={5}>
                                                <Button
                                                    color={"secondary"}
                                                    onClick={() => setEditMode(1)} // Create mode
                                                    variant="outlined"
                                                >
                                                    {getTranslatedLabel("project.projects.create", "Create Project")}
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </GridToolbar>
                                    <Column
                                        field="ProjectNum"
                                        title={getTranslatedLabel("project.projects.list.num", "Project Number")}
                                        cell={ProjectNumCell}
                                        width={200}
                                        locked={true}
                                    />
                                    <Column
                                        field="ProjectName"
                                        title={getTranslatedLabel("project.projects.list.name", "Project Name")}
                                        width={300}
                                    />
                                    <Column
                                        field="PartyId"
                                        title={getTranslatedLabel("project.projects.list.party", "Party ID")}
                                        width={150}
                                    />
                                    <Column
                                        field="CurrentStatusId"
                                        title={getTranslatedLabel("project.projects.list.status", "Status")}
                                        width={150}
                                    />
                                    <Column
                                        field="EstimatedStartDate"
                                        title={getTranslatedLabel("project.projects.list.startDate", "Start Date")}
                                        format="{0:MM/dd/yyyy}"
                                        width={150}
                                    />
                                    <Column
                                        field="EstimatedCompletionDate"
                                        title={getTranslatedLabel("project.projects.list.completionDate", "Completion Date")}
                                        format="{0:MM/dd/yyyy}"
                                        width={150}
                                    />
                                </KendoGrid>
                            </ExcelExport>
                            {isFetching && (
                                <LoadingComponent message={getTranslatedLabel("project.projects.list.loading", "Loading Projects...")} />
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}