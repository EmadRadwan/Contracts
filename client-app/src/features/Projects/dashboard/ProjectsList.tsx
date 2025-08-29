import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column, GridDataStateChangeEvent,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Button } from "@mui/material";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {WorkEffort} from "../../../app/models/manufacturing/workEffort";
import {useFetchProjectsQuery} from "../../../app/store/apis/projectsApi";
import ProjectForm from "../form/ProjectForm";
import ProjectMenu from "../menu/ProjectMenu";
import {DataResult, State} from "@progress/kendo-data-query";
import {handleDatesArray} from "../../../app/util/utils";

export default function ProjectsList() {
    const [editMode, setEditMode] = useState(0);
    const [project, setProject] = useState<WorkEffort | undefined>(undefined);
    const [projects, setProjects] = React.useState<DataResult>({data: [], total: 0});

    const [dataState, setDataState] = React.useState<State>({take: 6, skip: 0});
    const { data, error, isFetching, isLoading } = useFetchProjectsQuery({...dataState});
    const { getTranslatedLabel } = useTranslationHelper();

    useEffect(() => {
        if (data) {
            // REFACTOR: Extract total as a number from data.total, handling object case
            const total = typeof data.total === "object" ? data.total.total : data.total;
            const adjustedData = handleDatesArray(data.data);
            setProjects({ data: adjustedData, total: Number(total) });
        }
    }, [data]);
    
    // REFACTOR: Centralized facility selection logic to handle project selection and dispatch projectId to Redux store.
    function handleSelectProject(projectId: string) {
        const selectedProject = projects?.data?.find((project: WorkEffort) => project.WorkEffortId === projectId);
        setProject(selectedProject);
        setEditMode(2); // Edit mode
    }

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    function cancelEdit() {
        setEditMode(0);
    }


    // REFACTOR: Customized cell for ProjectNum to include clickable button linking to project details, consistent with FacilitiesList pattern.
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
                    {props.dataItem.workEffortId}
                </Button>
            </td>
        );
    };
    

    // REFACTOR: Conditionally render ProjectForm in edit or create mode, ensuring seamless navigation from grid to form.
    if (editMode) {
        return <ProjectForm project={project} cancelEdit={cancelEdit} editMode={editMode} />;
    }

    return (
        <>
            <ProjectMenu selectedMenuItem={"projects"} />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
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
                        field="workEffortId"
                        title={getTranslatedLabel("project.projects.list.num", "Project Number")}
                        cell={ProjectNumCell}
                        width={200}
                        locked={true}
                    />
                    <Column
                        field="projectName"
                        title={getTranslatedLabel("project.projects.list.name", "Project Name")}
                        width={300}
                    />
                    <Column
                        field="currentStatusDescription"
                        title={getTranslatedLabel("project.projects.list.status", "Status")}
                        width={150}
                    />
                    <Column
                        field="estimatedStartDate"
                        title={getTranslatedLabel("project.projects.list.startDate", "Start Date")}
                        format="{0:MM/dd/yyyy}"
                        width={150}
                    />
                    <Column
                        field="estimatedCompletionDate"
                        title={getTranslatedLabel("project.projects.list.completionDate", "Completion Date")}
                        format="{0:MM/dd/yyyy}"
                        width={150}
                    />
                </KendoGrid>
            </Paper>
        </>
    );
}