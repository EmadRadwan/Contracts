import React, {useCallback, useEffect, useMemo, useState} from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent, GridToolbar,
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
import {toast} from "react-toastify";
import ExcelJS from "exceljs";
import {CertificateItem} from "../../../app/models/project/certificateItem";
import {generateSupplyExcel, generateWorkmanshipExcel} from "../report/excelUtils";
import {saveAs} from "file-saver";


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
    const dispatch = useAppDispatch();
    const [certificate, setCertificate] = useState<ProjectCertificate | undefined>(undefined);
    const { data, isFetching } = useFetchProjectCertificatesQuery({ ...dataState });
    const [viewMode, setViewMode] = useState<"list" | "form">("list");
    const [isGeneratingAll, setIsGeneratingAll] = useState(false);

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
        excelCommand: 150,
    };


    const handleGenerateAllCertificates = async () => {
        if (!data?.data?.length) {
            toast.warn(
                getTranslatedLabel(
                    "certificate.list.empty",
                    "No certificates available to generate"
                )
            );
            return;
        }

        setIsGeneratingAll(true);
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = "System";

        let logoImageId: number | null = null;
        try {
            const response = await fetch("/goldenlandlogo.jpg");
            if (!response.ok) throw new Error("Failed to fetch logo");
            const blob = await response.blob();
            const arrayBuffer = await blob.arrayBuffer();
            logoImageId = workbook.addImage({
                buffer: arrayBuffer,
                extension: "jpeg",
            });
        } catch (error) {
            console.warn("Logo fetch failed:", error);
        }

        const errors: string[] = [];

        // Process each certificate sequentially
        for (const certificate of data.data) {
            if (!certificate.workEffortId || !certificate.certificateNumber) {
                console.warn("Skipping certificate due to invalid data:", {
                    workEffortId: certificate.workEffortId,
                    certificateNumber: certificate.certificateNumber,
                });
                errors.push(`Certificate ${certificate.certificateNumber || "unknown"}: Invalid data`);
                continue;
            }

            try {
                // REFACTOR: Use certificateItemsApi.endpoints.fetchCertificateItems.initiate
                // Purpose: Programmatically fetch certificate items using RTK Query's initiate method
                // Improvement: Correctly triggers the query and handles response, fixing 'fetch is not a function' error
                const response = await dispatch(
                    certificateItemsApi.endpoints.fetchCertificateItems.initiate(certificate.workEffortId)
                ).unwrap();

                const itemsArray: CertificateItem[] = Array.isArray(response) ? response : [];
                if (!itemsArray.length) {
                    console.warn(`No items found for workEffortId: ${certificate.workEffortId}`);
                    errors.push(`Certificate ${certificate.certificateNumber}: No items available`);
                    continue;
                }

                // Calculate subtotal
                const subtotal = itemsArray.reduce((sum: number, item: CertificateItem) => {
                    const total =
                        certificate.certificateCategory === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                            ? item.net || 0
                            : item.totalAmount || 0;
                    return sum + total;
                }, 0);

                // Format items to match Excel generation props
                const formattedItems: CertificateItem[] = itemsArray.map((item) => ({
                    ...item,
                    productName: item.productName || "N/A",
                    code: item.code || "N/A",
                    description: item.description || "",
                    quantity: item.quantity || 0,
                    uomName: item.uomName || "N/A",
                    unitPrice: item.unitPrice || 0,
                    displayTotal: item.totalAmount || 0,
                    discount: item.discount || 0,
                    formattedProcurementDate: item.procurementDate
                        ? new Date(item.procurementDate).toLocaleDateString("en-US")
                        : "N/A",
                    transportationExpenses: item.transportationExpenses || 0,
                    gratuities: item.gratuities || 0,
                    materialPrice: item.materialPrice || 0,
                    laborPrice: item.laborPrice || 0,
                    deductions: item.deductions || 0,
                    deductionDescription: item.deductionDescription || "",
                    deserved: item.deserved || 0,
                    insurance: item.insurance || 0,
                    additionalInsurance: item.additionalInsurance || 0,
                    net: item.net || 0,
                    achievementPercentage: item.achievementPercentage || "0%",
                    isLastInGroup: item.isLastInGroup || false,
                    productSubtotal: item.productSubtotal || 0,
                    mainItemDescription: item.mainItemDescription || "",
                    discountNote: item.discountNote || "",
                }));

                // Generate worksheet(s) for the certificate
                const generateFn =
                    certificate.certificateCategory === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                        ? generateWorkmanshipExcel
                        : generateSupplyExcel;

                const buffer = await generateFn(certificate, formattedItems, subtotal, getTranslatedLabel);
                if (buffer) {
                    // Load the generated workbook to extract worksheets
                    const tempWorkbook = new ExcelJS.Workbook();
                    await tempWorkbook.xlsx.load(buffer);
                    tempWorkbook.eachSheet((worksheet, sheetId) => {
                        const newWorksheet = workbook.addWorksheet(
                            `${certificate.certificateNumber}_${worksheet.name}`
                        );
                        worksheet.eachRow((row, rowNumber) => {
                            const newRow = newWorksheet.getRow(rowNumber);
                            row.eachCell((cell, colNumber) => {
                                const newCell = newRow.getCell(colNumber);
                                newCell.value = cell.value;
                                newCell.style = cell.style;
                                newCell.numFmt = cell.numFmt;
                            });
                            newRow.commit();
                        });
                        // Copy image if present
                        if (logoImageId !== null && worksheet.getImages().length > 0) {
                            newWorksheet.addImage(logoImageId, worksheet.getImages()[0].range);
                        }
                        newWorksheet.pageSetup = worksheet.pageSetup;
                        newWorksheet.views = worksheet.views;
                        newWorksheet.columns = worksheet.columns;
                    });
                } else {
                    errors.push(`Certificate ${certificate.certificateNumber}: Failed to generate worksheet`);
                }
            } catch (err: any) {
                console.error(`Error processing certificate ${certificate.certificateNumber}:`, err);
                errors.push(`Certificate ${certificate.certificateNumber}: ${err.message || "Unknown error"}`);
            }
        }

        if (workbook.worksheets.length === 0) {
            toast.error(
                getTranslatedLabel(
                    "certificate.excel.all.error",
                    "Failed to generate Excel report for all certificates"
                )
            );
            setIsGeneratingAll(false);
            return;
        }

        try {
            const buffer = await workbook.xlsx.writeBuffer();
            const blob = new Blob([buffer], {
                type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            });
            saveAs(blob, `AllCertificates_${new Date().toISOString().slice(0, 10)}.xlsx`);
            toast.success(
                getTranslatedLabel(
                    "certificate.excel.all.success",
                    "Excel report for all certificates generated successfully"
                )
            );

            if (errors.length > 0) {
                toast.warn(
                    getTranslatedLabel(
                        "certificate.excel.all.partial",
                        "Excel generated with issues: " + errors.join("; ")
                    )
                );
            }
        } catch (err) {
            console.error("Error generating final Excel file:", err);
            toast.error(
                getTranslatedLabel(
                    "certificate.excel.all.error",
                    "Failed to generate Excel report for all certificates"
                )
            );
        } finally {
            setIsGeneratingAll(false);
        }
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
                            <GridToolbar>
                                <Button
                                    color="primary"
                                    variant="contained"
                                    disabled={isGeneratingAll || !data?.data?.length}
                                    onClick={handleGenerateAllCertificates}
                                >
                                    {isGeneratingAll
                                        ? getTranslatedLabel("certificate.excel.all.loading", "Generating All...")
                                        : getTranslatedLabel("certificate.excel.all", "Generate All Certificates")}
                                </Button>
                            </GridToolbar>
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