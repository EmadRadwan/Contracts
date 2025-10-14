// REFACTOR: Import necessary dependencies
// Purpose: Maintains clarity and ensures all required components are included
// Context: Aligns with existing imports, removing unused ones
import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import { Button, Grid, Paper } from "@mui/material";
import { makeStyles } from "@mui/styles";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import { useNavigate } from "react-router-dom";
import {
    certificateUiSelectors,
    resetCertificateUi,
    setCertificateFormEditMode,
    setCurrentCertificateType,
    setSelectedCertificate,
} from "../slice/certificateUiSlice";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useFetchProjectCertificatesQuery } from "../../../app/store/apis/projectsApi";
import ProjectMenu from "../menu/ProjectMenu";
import ProjectCertificateForm from "../form/ProjectCertificateForm";
import { Certificate, CertificateStatus } from "../../../app/models/project/certificate";
import { resetUiCertificateItems } from "../slice/certificateItemsUiSlice";
import { certificateItemsApi } from "../../../app/store/apis/certificateItemsApi";

// REFACTOR: Update styles to remove outer scrollbar
// Purpose: Removes overflow-x: auto from container to prevent double scrollbar
// Context: Lets KendoReact Grid handle scrolling internally
const useStyles = makeStyles({
    gridContainer: {
        width: "100%",
        // REFACTOR: Remove overflow-x to avoid outer scrollbar
        // Purpose: Ensures only the grid's internal scrollbar is used
        // Context: Prevents nesting of scrollable regions
        "& .k-grid": {
            minWidth: "2360px", // Matches total column width to ensure grid content triggers scrollbar
        },
    },
});

interface ProjectCertificate {
    workEffortId?: string;
    certificateNumber?: string;
    projectName?: string;
    projectId?: string;
    partyIdSupplier?: string;
    partyNameSupplier?: string;
    partyIdContractor?: string;
    partyNameContractor?: string;
    description?: string;
    estimatedStartDate?: string;
    estimatedCompletionDate?: string;
    statusDescription?: string;
    statusDescriptionArabic?: string;
    currentStatusId?: CertificateStatus;
    certificateCategory?: string;
    certificateCategoryDescription?: string;
    relatedOrderId?: string;
    facilityId?: string;
    facilityName?: string;
}

export default function ProjectCertificatesList() {
    const classes = useStyles();
    const [certificates, setCertificates] = useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = useState<State>({ take: 6, skip: 0 });
    const { selectedCertificate, certificateFormEditMode } = useAppSelector(certificateUiSelectors.selectCertificateUi);
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const [certificate, setCertificate] = useState<ProjectCertificate | undefined>(undefined);
    const { data, isFetching, refetch } = useFetchProjectCertificatesQuery({ ...dataState });
    const [viewMode, setViewMode] = useState<"list" | "form">("list");

    const debounce = (func: Function, wait: number) => {
        let timeout: NodeJS.Timeout;
        return (...args: any[]) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => func(...args), wait);
        };
    };

    const editModeMap: { [key in CertificateStatus]: number } = useMemo(
        () => ({
            [CertificateStatus.CREATED]: 2,
            [CertificateStatus.APPROVED]: 3,
            [CertificateStatus.COMPLETE]: 4,
        }),
        []
    );

    useEffect(() => {
        return () => {
            dispatch(resetCertificateUi());
        };
    }, [dispatch]);

    useEffect(() => {
        if (data) {
            const adjustedData = data.data.map((item: Certificate) => ({
                ...item,
                estimatedStartDate: item.estimatedStartDate
                    ? new Date(item.estimatedStartDate).toLocaleDateString("en-GB")
                    : "",
                estimatedCompletionDate: item.estimatedCompletionDate
                    ? new Date(item.estimatedCompletionDate).toLocaleDateString("en-GB")
                    : "",
            }));
            setCertificates({ data: adjustedData, total: data.total });
            if (selectedCertificate?.workEffortId) {
                const matchingCert = adjustedData.find(
                    (cert: Certificate) => cert.workEffortId === selectedCertificate.workEffortId
                );
                if (matchingCert && JSON.stringify(matchingCert) !== JSON.stringify(certificate)) {
                    setCertificate(matchingCert);
                }
            }
        }
    }, [data, selectedCertificate?.workEffortId, certificate, dispatch]);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const handleSelectCertificate = useCallback(
        debounce((workEffortId?: string, currentStatusId?: CertificateStatus) => {
            if (!workEffortId) return;
            const selectedCert: Certificate | undefined = certificates.data.find(
                (cert: Certificate) => cert.workEffortId === workEffortId
            );
            if (!selectedCert) return;
            dispatch(resetUiCertificateItems());
            dispatch(certificateItemsApi.util.invalidateTags(["CertificateItems"]));
            dispatch(
                setSelectedCertificate({
                    workEffortId: selectedCert.workEffortId || "",
                    certificateNumber: selectedCert.certificateNumber || "",
                    projectId: selectedCert.projectId || "",
                    projectName: selectedCert.projectName || "",
                    currentStatusId: selectedCert.currentStatusId || CertificateStatus.CREATED,
                    partyIdSupplier: selectedCert.partyIdSupplier
                        ? {
                            fromPartyId:
                                typeof selectedCert.partyIdSupplier === "object"
                                    ? selectedCert.partyIdSupplier.fromPartyId
                                    : selectedCert.partyIdSupplier,
                            partyName: selectedCert.partyNameSupplier || "",
                        }
                        : undefined,
                    partyIdContractor: selectedCert.partyIdContractor
                        ? {
                            fromPartyId:
                                typeof selectedCert.partyIdContractor === "object"
                                    ? selectedCert.partyIdContractor.fromPartyId
                                    : selectedCert.partyIdContractor,
                            partyName: selectedCert.partyNameContractor || "",
                        }
                        : undefined,
                    description: selectedCert.description || "",
                    estimatedStartDate: selectedCert.estimatedStartDate
                        ? (() => {
                            const [day, month, year] = selectedCert.estimatedStartDate.split("/").map(Number);
                            const date = new Date(year, month - 1, day);
                            return isNaN(date.getTime()) ? null : date.toISOString();
                        })()
                        : null,
                    estimatedCompletionDate: selectedCert.estimatedCompletionDate
                        ? (() => {
                            const [day, month, year] = selectedCert.estimatedCompletionDate.split("/").map(Number);
                            const date = new Date(year, month - 1, day);
                            return isNaN(date.getTime()) ? null : date.toISOString();
                        })()
                        : null,
                    statusDescription: selectedCert.statusDescription || "",
                    statusDescriptionArabic: selectedCert.statusDescriptionArabic || "",
                    relatedOrderId: selectedCert.relatedOrderId || "",
                    workEffortTypeId: selectedCert.certificateCategory || "SUPPLY_PROCUREMENT_CERTIFICATE",
                    facilityId: selectedCert.facilityId,
                    facilityName: selectedCert.facilityName || "",
                })
            );
            dispatch(setCurrentCertificateType(selectedCert.certificateCategory || "SUPPLY_PROCUREMENT_CERTIFICATE"));
            dispatch(setCertificateFormEditMode(editModeMap[selectedCert.currentStatusId as CertificateStatus] || 0));
            setViewMode("form");
            dispatch(certificateItemsApi.endpoints.fetchCertificateItems.initiate(workEffortId));
        }, 500),
        [dispatch, certificates.data, editModeMap]
    );

    const cancelEdit = useCallback(() => {
        setCertificate(undefined);
        dispatch(setCertificateFormEditMode(0));
        dispatch(resetCertificateUi());
        setViewMode("list");
    }, [dispatch]);

    const handleMenuSelect = useCallback(
        (e: MenuSelectEvent) => {
            dispatch(resetCertificateUi());
            dispatch(resetUiCertificateItems());
            switch (e.item.data) {
                case "supplyProcurement":
                    dispatch(setCurrentCertificateType("SUPPLY_PROCUREMENT_CERTIFICATE"));
                    dispatch(setCertificateFormEditMode(1));
                    setViewMode("form");
                    break;
                case "workmanshipContracting":
                    dispatch(setCurrentCertificateType("WORKMANSHIP_CONTRACTING_CERTIFICATE"));
                    dispatch(setCertificateFormEditMode(1));
                    setViewMode("form");
                    break;
                case "companySupplySale":
                    dispatch(setCurrentCertificateType("COMPANY_SUPPLY_SALE_CERTIFICATE"));
                    dispatch(setCertificateFormEditMode(1));
                    setViewMode("form");
                    break;
                case "projectCertificates":
                    setViewMode("list");
                    break;
                default:
                    break;
            }
        },
        [dispatch]
    );

    const ProjectNumberCell = (props: any) => {
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
                        handleSelectCertificate(props.dataItem.workEffortId, props.dataItem.currentStatusId)
                    }
                >
                    {props.dataItem.certificateNumber}
                </Button>
            </td>
        );
    };

    if (viewMode === "form" && certificateFormEditMode > 0) {
        return (
            <ProjectCertificateForm
                selectedCertificate={certificate}
                cancelEdit={cancelEdit}
                editMode={certificateFormEditMode}
            />
        );
    }

    const columnWidths = {
        certificateNumber: 150,
        projectName: 200,
        certificateCategoryDescription: 320,
        statusDescription: 120,
        totalAmount: 120,
        partyIdSupplier: 150,
        partyNameSupplier: 200,
        partyIdContractor: 150,
        partyNameContractor: 200,
        description: 250,
        estimatedStartDate: 150,
        estimatedCompletionDate: 150,
        facilityName: 200,
    };

    return (
        <>
            <ProjectMenu />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={4}>
                        <Menu onSelect={handleMenuSelect}>
                            <MenuItem
                                key="newCertificate"
                                text={getTranslatedLabel("projects.certificate.list.new", "New Certificate")}
                            >
                                <MenuItem
                                    key="supplyProcurement"
                                    text={getTranslatedLabel(
                                        "projects.certificate.list.supplyProcurement",
                                        "Supply Procurement"
                                    )}
                                    data="supplyProcurement"
                                />
                                <MenuItem
                                    key="workmanshipContracting"
                                    text={getTranslatedLabel(
                                        "projects.certificate.list.workmanshipContracting",
                                        "Workmanship Contracting"
                                    )}
                                    data="workmanshipContracting"
                                />
                                <MenuItem
                                    key="companySupplySale"
                                    text={getTranslatedLabel(
                                        "projects.certificate.list.companySupplySale",
                                        "Company Supply Sale"
                                    )}
                                    data="companySupplySale"
                                />
                            </MenuItem>
                        </Menu>
                    </Grid>
                    {/* REFACTOR: Move grid inside Grid item for proper layout */}
                    {/* Purpose: Ensures grid container aligns with MUI grid system */}
                    {/* Context: Maintains consistent layout with CertificateItemsList* */}
                    <Grid item xs={12}>
                        <div className={classes.gridContainer}>
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
                                <Column
                                    field="certificateNumber"
                                    title={getTranslatedLabel(
                                        "projects.certificate.list.certificateNumber",
                                        "Project Number"
                                    )}
                                    width={columnWidths.certificateNumber}
                                    locked={false}
                                    cell={ProjectNumberCell}
                                />
                                <Column
                                    field="projectName"
                                    title={getTranslatedLabel("projects.certificate.list.projectName", "Project Name")}
                                    width={columnWidths.projectName}
                                />
                                <Column
                                    field="certificateCategoryDescription"
                                    title={getTranslatedLabel("projects.certificate.list.certificateType", "Type")}
                                    width={columnWidths.certificateCategoryDescription}
                                />
                                <Column
                                    field="statusDescription"
                                    title={getTranslatedLabel("projects.certificate.list.statusDescription", "Type")}
                                    width={columnWidths.statusDescription}
                                />
                                <Column
                                    field="totalAmount"
                                    title={getTranslatedLabel("projects.certificate.list.totalAmount", "Type")}
                                    width={columnWidths.totalAmount}
                                />
                                <Column
                                    field="partyIdSupplier"
                                    title={getTranslatedLabel(
                                        "projects.certificate.list.supplierPartyId",
                                        "Supplier ID"
                                    )}
                                    width={columnWidths.partyIdSupplier}
                                />
                                <Column
                                    field="partyNameSupplier"
                                    title={getTranslatedLabel(
                                        "projects.certificate.list.supplierPartyName",
                                        "Supplier Name"
                                    )}
                                    width={columnWidths.partyNameSupplier}
                                />
                                <Column
                                    field="partyIdContractor"
                                    title={getTranslatedLabel(
                                        "projects.certificate.list.contractorPartyId",
                                        "Contractor ID"
                                    )}
                                    width={columnWidths.partyIdContractor}
                                />
                                <Column
                                    field="partyNameContractor"
                                    title={getTranslatedLabel(
                                        "projects.certificate.list.contractorPartyName",
                                        "Contractor Name"
                                    )}
                                    width={columnWidths.partyNameContractor}
                                />
                                <Column
                                    field="description"
                                    title={getTranslatedLabel(
                                        "projects.certificate.list.description",
                                        "Certificate Description"
                                    )}
                                    width={columnWidths.description}
                                />
                                <Column
                                    field="estimatedStartDate"
                                    title={getTranslatedLabel("projects.certificate.list.startDate", "From Date")}
                                    format="{0: dd/MM/yyyy}"
                                    width={columnWidths.estimatedStartDate}
                                />
                                <Column
                                    field="estimatedCompletionDate"
                                    title={getTranslatedLabel(
                                        "projects.certificate.list.completionDate",
                                        "To Date"
                                    )}
                                    format="{0: dd/MM/yyyy}"
                                    width={columnWidths.estimatedCompletionDate}
                                />
                                <Column
                                    field="facilityName"
                                    title={getTranslatedLabel("projects.certificate.list.facilityName", "Facility Name")}
                                    width={columnWidths.facilityName}
                                />
                            </KendoGrid>
                            {isFetching && (
                                <LoadingComponent
                                    message={getTranslatedLabel(
                                        "projects.certificate.list.loading",
                                        "Loading Certificates..."
                                    )}
                                />
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}