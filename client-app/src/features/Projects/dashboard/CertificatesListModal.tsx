import React, { useEffect, useMemo, useState } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { DropDownList, DropDownListChangeEvent } from "@progress/kendo-react-dropdowns";
import { Box, Button, Typography } from "@mui/material";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useGetCertificatesByPartyQuery } from "../../../app/store/apis/projectsApi";
import CertificateDetailModal, { CertificateDetailSummary } from "./CertificateDetailModal";

interface ProjectFilterOption {
    projectId: string;
    projectName: string;
}

interface CertificatesListModalProps {
    show: boolean;
    onClose: () => void;
    contractorId?: string;
    supplierId?: string;
    certificateType: string;
    // Purpose: "Copy items to current certificate" only makes sense while creating a brand-new
    // certificate (editMode === 1 in ProjectCertificateForm) — an existing certificate's item
    // list is already the source of truth once it's been created, so copying into it there would
    // be surprising/destructive rather than a convenience.
    canCopyItems?: boolean;
}

export default function CertificatesListModal({
                                                  show,
                                                  onClose,
                                                  contractorId,
                                                  supplierId,
                                                  certificateType,
                                                  canCopyItems = false,
                                              }: CertificatesListModalProps) {
    const { getTranslatedLabel } = useTranslationHelper();
    const subtotalLabel = getTranslatedLabel("projects.certificate.modal.subtotal", "Subtotal →");

    const {
        data: certificates = [],
        isLoading,
        error,
    } = useGetCertificatesByPartyQuery(
        { contractorId, supplierId, certificateType },
        { skip: !show || (!contractorId && !supplierId) }
    );

    const isWorkmanship = certificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";

    const allProjectsOption: ProjectFilterOption = useMemo(() => ({
        projectId: "",
        projectName: getTranslatedLabel("projects.certificate.modal.allProjects", "All Projects"),
    }), [getTranslatedLabel]);

    const [selectedProject, setSelectedProject] = useState<ProjectFilterOption | null>(null);

    // Purpose: The same modal instance is reused across different contractors/suppliers
    // (controlled via the `show` prop rather than mount/unmount), so the project filter
    // must reset back to "All Projects" whenever the underlying party/type changes —
    // otherwise a stale projectId could filter out every row in the new certificate set.
    useEffect(() => {
        setSelectedProject(null);
    }, [contractorId, supplierId, certificateType]);

    const projectOptions = useMemo(() => {
        const uniqueProjects = new Map<string, ProjectFilterOption>();
        certificates.forEach((c: any) => {
            if (c.projectId && !uniqueProjects.has(c.projectId)) {
                uniqueProjects.set(c.projectId, { projectId: c.projectId, projectName: c.projectName || c.projectId });
            }
        });
        return [allProjectsOption, ...Array.from(uniqueProjects.values()).sort((a, b) => a.projectName.localeCompare(b.projectName))];
    }, [certificates, allProjectsOption]);

    const handleProjectFilterChange = (e: DropDownListChangeEvent) => {
        const value: ProjectFilterOption = e.target.value;
        setSelectedProject(value.projectId ? value : null);
    };

    const filteredCertificates = useMemo(
        () => (selectedProject ? certificates.filter((c: any) => c.projectId === selectedProject.projectId) : certificates),
        [certificates, selectedProject]
    );

    const [detailCertificate, setDetailCertificate] = useState<CertificateDetailSummary | null>(null);

    const openCertificateDetail = (dataItem: any) => {
        setDetailCertificate({
            workEffortId: dataItem.certificateWorkEffortId,
            certificateNumber: dataItem.certificateNumber,
            projectName: dataItem.projectName,
            partyName: dataItem.partyNameSupplier || dataItem.partyNameContractor,
            description: dataItem.__type === "header" ? dataItem.certificateDescription : dataItem.description,
            statusDescription: dataItem.statusDescription,
            certificateCategory: dataItem.certificateCategory || certificateType,
        });
    };

    // Build data with correct order: Header → Items → Subtotal
    const displayData = useMemo(() => {
        if (!isWorkmanship) return filteredCertificates;

        const result: any[] = [];

        // Group by certificateNumber
        const groups = new Map<string, any[]>();
        filteredCertificates.forEach((item: any) => {
            const key = item.certificateNumber;
            if (!groups.has(key)) groups.set(key, []);
            groups.get(key)!.push(item);
        });

        // Sort certificate numbers
        const sortedKeys = Array.from(groups.keys()).sort();

        sortedKeys.forEach((certNum) => {
            const items = groups.get(certNum)!;

            // 1. Header row
            result.push({
                __type: "header",
                certificateNumber: certNum,
                projectName: items[0].projectName,
                // `description` here is a synthetic display label for this grid's own (unused-for-
                // workmanship) description column — NOT the certificate's real description, so it's
                // kept separate from `certificateDescription` below to avoid leaking into the detail modal.
                description: `${getTranslatedLabel("projects.certificate.modal.certificateLabel", "Certificate")} ${certNum}`,
                certificateDescription: items[0].description,
                certificateWorkEffortId: items[0].certificateWorkEffortId,
                partyNameSupplier: items[0].partyNameSupplier,
                partyNameContractor: items[0].partyNameContractor,
                statusDescription: items[0].statusDescription,
                certificateCategory: items[0].certificateCategory,
            });

            // 2. All items
            items.forEach((item) =>
                result.push({
                    ...item,
                    __type: "item",
                })
            );

            // 3. Subtotal row (after items!)
            const subtotal = items.reduce((sum: number, i: any) => sum + (i.total || 0), 0);
            result.push({
                __type: "subtotal",
                certificateNumber: subtotalLabel,
                total: subtotal,
            });
        });

        return result;
    }, [filteredCertificates, isWorkmanship, getTranslatedLabel, subtotalLabel]);

    const grandTotal = filteredCertificates.reduce((sum: number, c: any) => sum + (c.total || 0), 0);

    const columns = [
        {
            field: "certificateNumber",
            title: getTranslatedLabel("projects.certificate.certificateNumber", "Certificate Number"),
            width: 180,
            cell: (props: any) => {
                const type = props.dataItem.__type;
                const isHeader = type === "header";
                const isSubtotal = type === "subtotal";
                const isItem = type === "item";
                // Purpose: Only the row that represents a whole certificate (the workmanship
                // group header, or a plain row for other categories) opens the detail modal —
                // individual item rows and the subtotal row aren't clickable.
                const isClickable = !isSubtotal && !isItem && !!props.dataItem.certificateWorkEffortId;

                return (
                    <td
                        style={{
                            fontWeight: isHeader || isSubtotal ? "bold" : "normal",
                            backgroundColor: isHeader ? "#e3f2fd" : isSubtotal ? "#f5f5f5" : "inherit",
                            paddingLeft: isHeader ? "12px" : isItem ? "36px" : "12px",
                            color: isSubtotal ? "#1565c0" : isHeader ? "#1976d2" : "inherit",
                        }}
                    >
                        {isSubtotal ? (
                            subtotalLabel
                        ) : isClickable ? (
                            <Button
                                onClick={() => openCertificateDetail(props.dataItem)}
                                sx={{ p: 0, minWidth: 0, textTransform: "none", fontWeight: "inherit", fontSize: "inherit" }}
                            >
                                {props.dataItem.certificateNumber}
                            </Button>
                        ) : (
                            props.dataItem.certificateNumber
                        )}
                    </td>
                );
            },
        },
        {
            field: "projectName",
            title: getTranslatedLabel("projects.certificate.projectName", "Project"),
            width: 200,
            cell: (props: any) => (
                <td style={{ fontWeight: props.dataItem.__type === "header" ? "bold" : "normal" }}>
                    {props.dataItem.projectName || ""}
                </td>
            ),
        },
        ...(!isWorkmanship ? [{
            field: "description",
            title: getTranslatedLabel("projects.certificate.description", "Description"),
            width: 320,
            cell: (props: any) => (
                <td style={{ fontStyle: props.dataItem.__type === "header" ? "italic" : "normal" }}>
                    {props.dataItem.description || "-"}
                </td>
            ),
        }] : []),
        ...(isWorkmanship
            ? [
                {
                    field: "productName",
                    title: getTranslatedLabel("projects.certificate.productName", "Product / Service"),
                    width: 280,
                    cell: (props: any) => {
                        if (props.dataItem.__type !== "item") return <td />;
                        return (
                            <td style={{ fontWeight: 500 }}>
                                {props.dataItem.productName || "—"}
                            </td>
                        );
                    },
                },

                {
                    field: "totalPrice",
                    title: getTranslatedLabel("projects.certificate.totalPrice", "Total Price (100%)"),
                    width: 160,
                    format: "{0:n2}",
                    cell: (props: any) => {
                        if (props.dataItem.__type !== "item") return <td />;
                        return (
                            <td style={{ textAlign: "right", color: "#424242" }}>
                                {(props.dataItem.totalPrice ?? 0).toFixed(2)}
                            </td>
                        );
                    },
                },

                {
                    field: "deserved",
                    title: getTranslatedLabel("projects.certificate.deserved", "Deserved Amount"),
                    width: 160,
                    format: "{0:n2}",
                    cell: (props: any) => {
                        if (props.dataItem.__type !== "item") return <td />;
                        return (
                            <td style={{ textAlign: "right", color: "#2e7d32", fontWeight: 600 }}>
                                {(props.dataItem.deserved ?? 0).toFixed(2)}
                            </td>
                        );
                    },
                },

                {
                    field: "achievementPercent",
                    title: getTranslatedLabel("projects.certificate.achievement", "Achievement %"),
                    width: 140,
                    cell: (props: any) => {
                        if (props.dataItem.__type !== "item") return <td>-</td>;
                        return (
                            <td style={{ textAlign: "right", color: "#2e7d32", fontWeight: 600 }}>
                                {props.dataItem.achievementPercent !== undefined
                                    ? (props.dataItem.achievementPercent / 100).toLocaleString(undefined, {
                                        style: "percent",
                                        minimumFractionDigits: 2,
                                    })
                                    : "-"}
                            </td>
                        );
                    },
                },
            ]
            : []),
        {
            field: "total",
            title: getTranslatedLabel("projects.certificate.total", "Total"),
            width: 260,
            format: "{0:n2}",
            footerCell: () => (
                <td style={{ textAlign: "right", fontWeight: "bold", color: "#d32f2f", fontSize: "1.2em", whiteSpace: "nowrap" }}>
                    {getTranslatedLabel("projects.certificate.grandTotal", "Grand Total")}: {grandTotal.toFixed(2)}
                </td>
            ),
            cell: (props: any) => {
                const type = props.dataItem.__type;

                // In workmanship mode: hide total on header rows
                if (isWorkmanship && type === "header") return <td />;

                // For all other cases (non-workmanship OR item/subtotal in workmanship)
                // → always show the total if it exists
                const value = props.dataItem.total ?? 0;

                return (
                    <td style={{
                        textAlign: "right",
                        fontWeight: type === "subtotal" ? "bold" : "normal",
                        color: type === "subtotal" ? "#1565c0" : "inherit",
                    }}>
                        {value.toFixed(2)}
                    </td>
                );
            },
        },
        {
            field: "statusDescription",
            title: getTranslatedLabel("projects.certificate.status", "Status"),
            width: 140,
            cell: (props: any) => {
                const type = props.dataItem.__type;

                // Only hide status for header rows in workmanship mode
                if (isWorkmanship && type === "header") return <td />;

                return (
                    <td>
                        {props.dataItem.statusDescription || ""}
                    </td>
                );
            },
        },
    ];

    if (isLoading) return <LoadingComponent />;
    if (error) return (
        <Typography color="error">
            {getTranslatedLabel("projects.certificate.modal.loadError", "Failed to load certificates")}
        </Typography>
    );

    return (
        <ModalContainer show={show} onClose={onClose} width={1350}>
            <Typography variant="h5" gutterBottom>
                {isWorkmanship
                    ? getTranslatedLabel("projects.certificate.modal.titleWorkmanship", "Certificates List - Workmanship Contracting")
                    : getTranslatedLabel("projects.certificate.modal.titleSupply", "Certificates List - Supply")}
            </Typography>

            <KendoGrid
                data={displayData}
                sortable={false} // Disabled to preserve perfect order
                style={{ height: "70vh" }}
            >
                <GridToolbar>
                    <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", width: "100%", gap: 2 }}>
                        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                            <Typography variant="body2">
                                {getTranslatedLabel("projects.certificate.form.project", "Project")}:
                            </Typography>
                            <DropDownList
                                style={{ width: 220 }}
                                data={projectOptions}
                                textField="projectName"
                                dataItemKey="projectId"
                                value={selectedProject ?? allProjectsOption}
                                onChange={handleProjectFilterChange}
                            />
                        </Box>
                        <Typography variant="subtitle1">
                            {isWorkmanship
                                ? `${filteredCertificates.length} ${getTranslatedLabel("projects.certificate.modal.itemsLabel", "items")} • ${new Set(filteredCertificates.map((c: any) => c.certificateNumber)).size} ${getTranslatedLabel("projects.certificate.modal.certificatesLabel", "certificates")}`
                                : `${filteredCertificates.length} ${getTranslatedLabel("projects.certificate.modal.certificatesLabel", "certificates")}`}
                        </Typography>
                        <Button variant="contained" onClick={onClose}>
                            {getTranslatedLabel("general.close", "Close")}
                        </Button>
                    </Box>
                </GridToolbar>

                {columns.map((col: any, i) => (
                    <Column key={i} {...col} />
                ))}
            </KendoGrid>

            {detailCertificate && (
                <CertificateDetailModal
                    show={!!detailCertificate}
                    onClose={() => setDetailCertificate(null)}
                    certificate={detailCertificate}
                    canCopyItems={canCopyItems}
                    onItemsCopied={() => {
                        // Purpose: "Copy items" closes both modals — this one (the certificates
                        // list) and the detail modal (which closes itself before calling this).
                        setDetailCertificate(null);
                        onClose();
                    }}
                />
            )}
        </ModalContainer>
    );
}