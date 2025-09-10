import React, { useCallback, useEffect, useState } from "react";
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

interface ProjectCertificate {
    workEffortId: string;
    projectNum: string;
    projectName: string;
    partyId: string;
    partyName: string; // From Party navigation
    description: string;
    estimatedStartDate: string;
    estimatedCompletionDate: string;
    statusDescription: string;
    certificateCategoryDescription: string; // Reflects certificate type (e.g., SUPPLY_PROCUREMENT_CERTIFICATE)
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
    const [editMode, setEditMode] = useState(0);

    console.log("Certificates data:", data);

    const debounce = (func: Function, wait: number) => {
        let timeout: NodeJS.Timeout;
        return (...args: any[]) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => func(...args), wait);
        };
    };

    useEffect(() => {
        if (data) {
            const adjustedData = data.data.map((item: ProjectCertificate) => ({
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
                    (cert: ProjectCertificate) => cert.workEffortId === selectedCertificate.workEffortId
                );
                if (matchingCert && JSON.stringify(matchingCert) !== JSON.stringify(certificate)) {
                    console.log("Syncing certificate with new data:", matchingCert);
                    setCertificate(matchingCert);
                } else if (!matchingCert) {
                    console.warn("Certificate not found for workEffortId:", selectedCertificate.workEffortId);
                }
            }
        }
    }, [data, selectedCertificate?.workEffortId, certificate]);


    

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const handleSelectCertificate = useCallback(
        debounce((workEffortId: string, statusDescription?: string, partyId?: string) => {
            console.log("handleSelectCertificate called with workEffortId:", workEffortId);
            const selectedCert: ProjectCertificate | undefined = certificates.data.find(
                (cert: any) => cert.workEffortId === workEffortId
            );
            if (!selectedCert) {
                console.warn("No certificate found for workEffortId:", workEffortId);
                return;
            }
            setCertificate(selectedCert);
            dispatch(
                setSelectedCertificate({
                    workEffortId: selectedCert.workEffortId || "",
                    projectNum: selectedCert.projectNum || "",
                    projectName: selectedCert.projectName || "",
                    partyIdSupplier: selectedCert.partyId
                        ? { fromPartyId: selectedCert.partyId, partyName: selectedCert.partyName || "" }
                        : undefined,
                    partyIdContractor: undefined,
                    partyName: selectedCert.partyName || "",
                    description: selectedCert.description || "",
                    estimatedStartDate: selectedCert.estimatedStartDate
                        ? new Date(selectedCert.estimatedStartDate).toISOString()
                        : null,
                    estimatedCompletionDate: selectedCert.estimatedCompletionDate
                        ? new Date(selectedCert.estimatedCompletionDate).toISOString()
                        : null,
                    statusDescription: statusDescription || selectedCert.statusDescription || "",
                })
            );
            dispatch(setCurrentCertificateType(selectedCert.certificateCategoryDescription || "SUPPLY_PROCUREMENT_CERTIFICATE"));
            if (statusDescription === "CREATED") {
                dispatch(setCertificateFormEditMode(2));
            } else if (statusDescription === "APPROVED") {
                dispatch(setCertificateFormEditMode(3));
            } else if (statusDescription === "COMPLETED") {
                dispatch(setCertificateFormEditMode(4));
            }
        }, 500), // Debounce for 500ms to wait for refetch
        [dispatch, certificates.data]
    );

    const cancelEdit = useCallback(() => {
        setCertificate(undefined);
        dispatch(setCertificateFormEditMode(0));
        dispatch(resetCertificateUi());
    }, [dispatch]);

    // REFACTOR: Updated handleMenuSelect to support only the five certificate types from the sheet.
    // Purpose: Replaces PROCUREMENTS and CONTRACTING with the five new types (SUPPLY_PROCUREMENT_CERTIFICATE, etc.) to align with the provided data.
    // Context: Simplifies the menu to reflect only the types defined in the sheet, maintaining Redux dispatch for form initialization.
    const handleMenuSelect = useCallback(
        (e: MenuSelectEvent) => {
            dispatch(resetCertificateUi());
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
                            props.dataItem.statusDescription,
                            props.dataItem.partyId
                        )
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
                        {/* REFACTOR: Updated Menu to include only the five certificate types from the sheet.
               Purpose: Removes PROCUREMENTS and CONTRACTING MenuItems, replacing them with the five new types to match the provided data.
               Context: Maintains translation support and menu structure while aligning with the sheet's five extract types. */}
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
                                    field="partyId"
                                    title={getTranslatedLabel("certificate.list.partyId", "Party ID")}
                                />
                                <Column
                                    field="partyName"
                                    title={getTranslatedLabel("certificate.list.partyName", "Party Name")}
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