import React, {useCallback, useEffect, useMemo, useState} from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import Button from "@mui/material/Button";
import {Grid, Paper} from "@mui/material";

import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import { useLocation, useNavigate } from "react-router-dom";
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
import { useFetchProjectCertificatesQuery} from "../../../app/store/apis/projectsApi";
import ProjectMenu from "../menu/ProjectMenu";
import ProjectCertificateForm from "../form/ProjectCertificateForm";
import {Certificate, CertificateStatus} from "../../../app/models/project/certificate";
import {resetUiCertificateItems} from "../slice/certificateItemsUiSlice";
import {certificateItemsApi} from "../../../app/store/apis/certificateItemsApi";


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
    statusDescriptionArabic?: string; // Added for localization
    currentStatusId?: CertificateStatus; // Added for consistent status handling
    certificateCategory?: string; // Raw type ID from backend
    certificateCategoryDescription?: string;
    relatedOrderId?: string;
    facilityId?: string;
    facilityName?: string;
}

export default function ProjectCertificatesList() {
    const [certificates, setCertificates] = useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = useState<State>({ take: 6, skip: 0 });
    const { selectedCertificate, certificateFormEditMode } = useAppSelector(certificateUiSelectors.selectCertificateUi);
    const { getTranslatedLabel } = useTranslationHelper();
    const location = useLocation();
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const [certificate, setCertificate] = useState<ProjectCertificate | undefined>(undefined);
    const { data, isFetching, refetch } = useFetchProjectCertificatesQuery({ ...dataState });
    const [viewMode, setViewMode] = useState<"list" | "form">("list");

    console.log("Certificates data:", data);
    
    // console.log('List rendered')

    const debounce = (func: Function, wait: number) => {
        let timeout: NodeJS.Timeout;
        return (...args: any[]) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => func(...args), wait);
        };
    };

    const editModeMap: { [key in CertificateStatus]: number } = useMemo(() => ({
        [CertificateStatus.CREATED]: 2,
        [CertificateStatus.APPROVED]: 3,
        [CertificateStatus.COMPLETE]: 4,
    }), []);

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
                    // // console.log("Syncing certificate with new data:", matchingCert);
                    setCertificate(matchingCert);
                } else if (!matchingCert) {
                    // Purpose: Prevent stale data when certificate is deleted or unavailable
                    // Context: Ensures form resets to initial state
                    // // console.warn("Certificate not found for workEffortId:", selectedCertificate.workEffortId);
                    //dispatch(resetCertificateUi());
                }
            }
        }
    }, [data, selectedCertificate?.workEffortId, certificate, dispatch]);



    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const handleSelectCertificate = useCallback(
        debounce((workEffortId?: string, currentStatusId?: CertificateStatus) => {
            if (!workEffortId) {
                console.warn("No workEffortId provided to handleSelectCertificate");
                return;
            }
            const selectedCert: Certificate | undefined = certificates.data.find(
                (cert: Certificate) => cert.workEffortId === workEffortId
            );
            if (!selectedCert) {
                console.warn("No certificate found for workEffortId:", workEffortId);
                return;
            }
            dispatch(resetUiCertificateItems());
            dispatch(certificateItemsApi.util.invalidateTags(['CertificateItems']));
            
            dispatch(
                setSelectedCertificate({
                    workEffortId: selectedCert.workEffortId || "",
                    certificateNumber: selectedCert.certificateNumber || "",
                    projectId: selectedCert.projectId  || "",
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
                            // Purpose: Correctly map contractor ID from query data
                            // Context: Fixes binding for contractor ComboBox
                            partyName: selectedCert.partyNameContractor || "",
                        }
                        : undefined,
                    description: selectedCert.description || "",
                    estimatedStartDate: selectedCert.estimatedStartDate
                        ? (() => {
                            const [day, month, year] = selectedCert.estimatedStartDate.split('/').map(Number);
                            const date = new Date(year, month - 1, day);
                            return isNaN(date.getTime()) ? null : date.toISOString();
                        })()
                        : null,
                    estimatedCompletionDate: selectedCert.estimatedCompletionDate
                        ? (() => {
                            const [day, month, year] = selectedCert.estimatedCompletionDate.split('/').map(Number);
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

    // Purpose: Replaces PROCUREMENTS and CONTRACTING with the five new types (SUPPLY_PROCUREMENT_CERTIFICATE, etc.) to align with the provided data.
    // Context: Simplifies the menu to reflect only the types defined in the sheet, maintaining Redux dispatch for form initialization.
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
                        handleSelectCertificate(
                            props.dataItem.workEffortId,
                            props.dataItem.currentStatusId
                        )
                        // Purpose: Align with handleSelectCertificate signature
                        // Context: Ensures status-based editMode setting works
                    }
                >
                    {props.dataItem.certificateNumber}
                </Button>
            </td>
        );
    };
    // // console.log('certificateFormEditMode:', certificateFormEditMode)

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
                            <MenuItem key="newCertificate"
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
                    <Grid item xs={12}>
                        <KendoGrid
                            style={{ height: "65vh" }}
                            scrollable="scrollable" // REFACTOR: Add scrollable prop
                            // Purpose: Explicitly enables horizontal and vertical scrolling
                            // Context: Matches CertificateItemsList* configuration
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
                    </Grid>
                   
                </Grid>
            </Paper>
        </>
    );
}