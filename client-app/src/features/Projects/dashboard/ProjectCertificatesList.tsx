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
import { Grid, Paper } from "@mui/material";
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
import { useFetchProjectCertificatesQuery } from "../../../app/store/apis/projectsApi";
import ProjectMenu from "../menu/ProjectMenu";
import ProjectCertificateForm from "../form/ProjectCertificateForm";
import {Certificate, CertificateStatus} from "../../../app/models/project/certificate";

interface ProjectCertificate {
    workEffortId?: string;
    projectNum?: string;
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

    console.log("Certificates data:", data);

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
                    console.log("Syncing certificate with new data:", matchingCert);
                    setCertificate(matchingCert);
                } else if (!matchingCert) {
                    // Purpose: Prevent stale data when certificate is deleted or unavailable
                    // Context: Ensures form resets to initial state
                    console.warn("Certificate not found for workEffortId:", selectedCertificate.workEffortId);
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
            // Purpose: Ensure type safety and consistency with enum
            // Context: Matches ProjectNumberCell call and backend DTO
            if (!workEffortId) {
                console.warn("No workEffortId provided to handleSelectCertificate");
                return;
            }
            console.log("handleSelectCertificate called with workEffortId:", workEffortId);
            const selectedCert: Certificate | undefined = certificates.data.find(
                (cert: Certificate) => cert.workEffortId === workEffortId
                // Purpose: Improve type safety; assumes backend data matches interface
                // Context: Prevents runtime errors on field access
            );
            if (!selectedCert) {
                console.warn("No certificate found for workEffortId:", workEffortId);
                return;
            }
            console.log("Raw selectedCert:", selectedCert); // REFACTOR: Debug log

            dispatch(
                setSelectedCertificate({
                    workEffortId: selectedCert.workEffortId || "",
                    projectNum: selectedCert.projectNum || "",
                    projectId: selectedCert.projectId || selectedCert.projectNum || "",
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
                                    : selectedCert.partyIdContractor, // REFACTOR: Fixed typo from partyIdContractor to contractorPartyId
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
                })
            );
            dispatch(setCurrentCertificateType(selectedCert.certificateCategory || "SUPPLY_PROCUREMENT_CERTIFICATE"));
            dispatch(setCertificateFormEditMode(editModeMap[selectedCert.currentStatusId as CertificateStatus] || 0));
        }, 500),
        [dispatch, certificates.data, editModeMap] 
    );
    
    
    const cancelEdit = useCallback(() => {
        setCertificate(undefined);
        dispatch(setCertificateFormEditMode(0));
        dispatch(resetCertificateUi());
    }, [dispatch]);

    // Purpose: Replaces PROCUREMENTS and CONTRACTING with the five new types (SUPPLY_PROCUREMENT_CERTIFICATE, etc.) to align with the provided data.
    // Context: Simplifies the menu to reflect only the types defined in the sheet, maintaining Redux dispatch for form initialization.
    const handleMenuSelect = useCallback(
        (e: MenuSelectEvent) => {
            //dispatch(resetCertificateUi());
            switch (e.item.data) {
                case "supplyProcurement":
                    dispatch(setCurrentCertificateType("SUPPLY_PROCUREMENT_CERTIFICATE"));
                    dispatch(setCertificateFormEditMode(1));
                    break;
                case "workmanshipContracting":
                    dispatch(setCurrentCertificateType("WORKMANSHIP_CONTRACTING_CERTIFICATE"));
                    dispatch(setCertificateFormEditMode(1));
                    break;
                case "contractorPurchase":
                    dispatch(setCurrentCertificateType("CONTRACTOR_PURCHASE_CERTIFICATE"));
                    dispatch(setCertificateFormEditMode(1));
                    break;
                case "companySupplySale":
                    dispatch(setCurrentCertificateType("COMPANY_SUPPLY_SALE_CERTIFICATE"));
                    dispatch(setCertificateFormEditMode(1));
                    break;
                case "externalSupplySale":
                    dispatch(setCurrentCertificateType("EXTERNAL_SUPPLY_SALE_CERTIFICATE"));
                    dispatch(setCertificateFormEditMode(1));
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

    if (certificateFormEditMode > 0) {
        return (
            <ProjectCertificateForm
                selectedCertificate={certificate}
                cancelEdit={cancelEdit}
                editMode={certificateFormEditMode}
            />
        );
    }

    return (
        <>
            <ProjectMenu />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={4}>
                        <Menu onSelect={handleMenuSelect}>
                            <MenuItem key="newCertificate" text={getTranslatedLabel("certificate.list.new", "New Certificate")}>
                                <MenuItem
                                    key="supplyProcurement"
                                    text={getTranslatedLabel("certificate.list.supplyProcurement", "Supply Procurement")}
                                    data="supplyProcurement"
                                />
                                <MenuItem
                                    key="workmanshipContracting"
                                    text={getTranslatedLabel("certificate.list.workmanshipContracting", "Workmanship Contracting")}
                                    data="workmanshipContracting"
                                />
                                <MenuItem
                                    key="contractorPurchase"
                                    text={getTranslatedLabel("certificate.list.contractorPurchase", "Contractor Purchase")}
                                    data="contractorPurchase"
                                />
                                <MenuItem
                                    key="companySupplySale"
                                    text={getTranslatedLabel("certificate.list.companySupplySale", "Company Supply Sale")}
                                    data="companySupplySale"
                                />
                                <MenuItem
                                    key="externalSupplySale"
                                    text={getTranslatedLabel("certificate.list.externalSupplySale", "External Supply Sale")}
                                    data="externalSupplySale"
                                />
                            </MenuItem>
                        </Menu>
                    </Grid>
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{ height: "65vh" }}
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
                                    title={getTranslatedLabel("certificate.list.projectNum", "Project Number")}
                                    width={150}
                                    locked={false}
                                    cell={ProjectNumberCell}
                                />
                                <Column
                                    field="projectName"
                                    title={getTranslatedLabel("certificate.list.projectName", "Project Name")}
                                />
                                <Column
                                    field="certificateCategoryDescription"
                                    title={getTranslatedLabel("certificate.list.partyId", "Type")}
                                />
                                <Column
                                    field="partyIdSupplier"
                                    title={getTranslatedLabel("certificate.list.supplierPartyId", "Supplier ID")}
                                />
                                <Column
                                    field="partyNameSupplier"
                                    title={getTranslatedLabel("certificate.list.supplierPartyName", "Supplier Name")}
                                />
                                <Column
                                    field="partyIdContractor"
                                    title={getTranslatedLabel("certificate.list.contractorPartyId", "Contractor ID")}
                                />
                                <Column
                                    field="partyNameContractor"
                                    title={getTranslatedLabel("certificate.list.contractorPartyName", "Contractor Name")}
                                />
                                <Column
                                    field="description"
                                    title={getTranslatedLabel("certificate.list.description", "Certificate Description")}
                                />
                                <Column
                                    field="estimatedStartDate"
                                    title={getTranslatedLabel("certificate.list.fromDate", "From Date")}
                                    format="{0: dd/MM/yyyy}"
                                />
                                <Column
                                    field="estimatedCompletionDate"
                                    title={getTranslatedLabel("certificate.list.toDate", "To Date")}
                                    format="{0: dd/MM/yyyy}"
                                />
                            </KendoGrid>
                            {isFetching && (
                                <LoadingComponent
                                    message={getTranslatedLabel("certificate.list.loading", "Loading Certificates...")}
                                />
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}