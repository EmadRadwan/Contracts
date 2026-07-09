import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column, GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Button } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {WorkEffort} from "../../../app/models/manufacturing/workEffort";
import {useFetchProjectsQuery} from "../../../app/store/apis/projectsApi";
import {useAppDispatch, useAppSelector} from "../../../app/store/configureStore";
import ProjectForm from "../form/ProjectForm";
import ProjectMenu from "../menu/ProjectMenu";
import {DataResult, State} from "@progress/kendo-data-query";
import {handleDatesArray} from "../../../app/util/utils";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {ProjectReportExcel} from "../report/ProjectReportExcel";
import {Can} from "../../account/Can";
import {
    resetCertificateUi,
    setCertificateFormEditMode,
    setCurrentCertificateType,
    setSelectedCertificate,
} from "../slice/certificateUiSlice";
import {resetUiCertificateItems} from "../slice/certificateItemsUiSlice";
import {CertificateStatus} from "../../../app/models/project/certificate";

const certificateTypeButtons = [
    {
        type: "SUPPLY_PROCUREMENT_CERTIFICATE",
        labelKey: "projects.certificate.list.supplyProcurement",
        labelDefault: "Supply Procurement",
    },
    {
        type: "WORKMANSHIP_CONTRACTING_CERTIFICATE",
        labelKey: "projects.certificate.list.workmanshipContracting",
        labelDefault: "Workmanship Contracting",
    },
    {
        type: "COMPANY_SUPPLY_SALE_CERTIFICATE",
        labelKey: "projects.certificate.list.companySupplySale",
        labelDefault: "Company Supply Sale",
    },
];

export default function ProjectsList() {
    const [editMode, setEditMode] = useState(0);
    const [project, setProject] = useState<WorkEffort | undefined>(undefined);
    const [projects, setProjects] = React.useState<DataResult>({data: [], total: 0});

    const [reportDialogOpen, setReportDialogOpen] = useState(false);
    const [selectedProjectForReport, setSelectedProjectForReport] = useState<{id: string, name: string} | null>(null);

    const [dataState, setDataState] = React.useState<State>({take: 6, skip: 0});
    const { data, error, isFetching, isLoading } = useFetchProjectsQuery({...dataState});
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    // Purpose: Mirror the <Can perform="RunProjectReport"> check so the ID column width
    // can shrink when the Project Report button isn't rendered for this user.
    const { user } = useAppSelector((state) => state.account);
    const canRunProjectReport = (user?.roles || []).includes("RunProjectReport");

    useEffect(() => {
        if (data) {
            // REFACTOR: Extract total as a number from data.total, handling object case
            const total = typeof data.total === "object" ? data.total.total : data.total;
            const adjustedData = handleDatesArray(data.data);
            console.log("Fetched Projects Data:", adjustedData);
            setProjects({ data: adjustedData, total: Number(total) });
        }
    }, [data]);
    
    // REFACTOR: Centralized facility selection logic to handle project selection and dispatch projectId to Redux store.
    function handleSelectProject(projectId: string) {
        const selectedProject = projects?.data?.find((project: WorkEffort) => project.workEffortId === projectId);
        setProject(selectedProject);
        setEditMode(2); // Edit mode
    }

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    function cancelEdit() {
        setEditMode(0);
    }

    // Purpose: Start a new project certificate pre-populated with the project the user came from.
    // Context: ProjectCertificateForm reads selectedCertificate from redux; ProjectCertificatesList
    // switches to form view on mount when certificateFormEditMode > 0.
    function handleCreateCertificateForProject(dataItem: WorkEffort, certificateType: string) {
        dispatch(resetCertificateUi());
        dispatch(resetUiCertificateItems());
        dispatch(setCurrentCertificateType(certificateType));
        dispatch(setSelectedCertificate({
            workEffortId: "",
            certificateNumber: "",
            projectId: dataItem.workEffortId || "",
            projectName: (dataItem as any).projectName || dataItem.workEffortName || "",
            currentStatusId: CertificateStatus.CREATED,
            description: "",
            estimatedStartDate: null,
            estimatedCompletionDate: null,
            statusDescription: "",
            relatedOrderId: "",
            // Purpose: Project and facility are linked — pre-populate the facility too.
            facilityId: dataItem.facilityId || "",
            facilityName: dataItem.facilityName || "",
        }));
        dispatch(setCertificateFormEditMode(1));
        navigate("/projectCertificates");
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
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <Button onClick={() => handleSelectProject(props.dataItem.workEffortId)}>
                        {props.dataItem.workEffortId}
                    </Button>
                    <Can perform="RunProjectReport">
                        <Button
                            size="small"
                            variant="outlined"
                            color="success"
                            onClick={() => {
                                setSelectedProjectForReport({
                                    id: props.dataItem.workEffortId,
                                    name: props.dataItem.projectName
                                });
                                setReportDialogOpen(true);
                            }}
                        >
                            {getTranslatedLabel("project.projects.report", "Project Report")}
                        </Button>
                    </Can>
                </div>
            </td>
        );
    };

    // Row-based buttons to start a new project certificate of a given type for this project.
    const CertificateButtonsCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={props.style}
                colSpan={props.colSpan}
                role={"gridcell"}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <div style={{ display: 'flex', alignItems: 'center', gap: '4px', flexWrap: 'wrap' }}>
                    {certificateTypeButtons.map(({ type, labelKey, labelDefault }) => (
                        <Button
                            key={type}
                            size="small"
                            variant="outlined"
                            onClick={() => handleCreateCertificateForProject(props.dataItem, type)}
                        >
                            {getTranslatedLabel(labelKey, labelDefault)}
                        </Button>
                    ))}
                </div>
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
                        width={canRunProjectReport ? 320 : 160}
                        locked={true}
                    />
                    <Column
                        field="projectName"
                        title={getTranslatedLabel("project.projects.list.name", "Project Name")}
                        width={300}
                    />
                    <Column
                        field="certificateActions"
                        title={getTranslatedLabel("project.projects.list.createCertificate", "Create Certificate")}
                        cell={CertificateButtonsCell}
                        width={480}
                        sortable={false}
                        filterable={false}
                    />
                    <Column
                        field="glAccountName"
                        title={getTranslatedLabel("project.projects.list.glAccount", "GL Account")}
                        width={350}
                    />
                    <Column
                        field="operatingExpenseGlAccountName"
                        title={getTranslatedLabel("project.projects.list.operatingExpenseGlAccount", "Operating Expense GL Account")}
                        width={350}
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
                {isFetching && <LoadingComponent message={getTranslatedLabel("project.projects.list.loading", "Loading Projects...")} />}
                {reportDialogOpen && selectedProjectForReport && (
                    <ProjectReportExcel
                        projectId={selectedProjectForReport.id}
                        projectName={selectedProjectForReport.name}
                        open={reportDialogOpen}
                        onClose={() => {
                            setReportDialogOpen(false);
                            setSelectedProjectForReport(null);
                        }}
                    />
                )}
            </Paper>
        </>
    );
}