import React, { useMemo } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { Box, Button, Typography } from "@mui/material";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useGetCertificatesByPartyQuery } from "../../../app/store/apis/projectsApi";

interface CertificatesListModalProps {
    show: boolean;
    onClose: () => void;
    contractorId?: string;
    supplierId?: string;
    certificateType: string;
}

export default function CertificatesListModal({
                                                  show,
                                                  onClose,
                                                  contractorId,
                                                  supplierId,
                                                  certificateType,
                                              }: CertificatesListModalProps) {
    const { getTranslatedLabel } = useTranslationHelper();

    const {
        data: certificates = [],
        isLoading,
        error,
    } = useGetCertificatesByPartyQuery(
        { contractorId, supplierId, certificateType },
        { skip: !show || (!contractorId && !supplierId) }
    );

    const isWorkmanship = certificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";

    // Build data with correct order: Header → Items → Subtotal
    const displayData = useMemo(() => {
        if (!isWorkmanship) return certificates;

        const result: any[] = [];

        // Group by certificateNumber
        const groups = new Map<string, any[]>();
        certificates.forEach((item: any) => {
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
                description: `Certificate ${certNum}`,
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
                certificateNumber: "Subtotal →",
                total: subtotal,
            });
        });

        return result;
    }, [certificates, isWorkmanship]);

    const grandTotal = certificates.reduce((sum: number, c: any) => sum + (c.total || 0), 0);

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

                return (
                    <td
                        style={{
                            fontWeight: isHeader || isSubtotal ? "bold" : "normal",
                            backgroundColor: isHeader ? "#e3f2fd" : isSubtotal ? "#f5f5f5" : "inherit",
                            paddingLeft: isHeader ? "12px" : isItem ? "36px" : "12px",
                            color: isSubtotal ? "#1565c0" : isHeader ? "#1976d2" : "inherit",
                        }}
                    >
                        {isSubtotal ? "Subtotal →" : props.dataItem.certificateNumber}
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
        {
            field: "description",
            title: getTranslatedLabel("projects.certificate.description", "Description"),
            width: 320,
            cell: (props: any) => (
                <td style={{ fontStyle: props.dataItem.__type === "header" ? "italic" : "normal" }}>
                    {props.dataItem.description || "-"}
                </td>
            ),
        },
        ...(isWorkmanship
            ? [
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
            width: 180,
            format: "{0:n2}",
            footerCell: () => (
                <td style={{ textAlign: "right", fontWeight: "bold", color: "#d32f2f", fontSize: "1.2em" }}>
                    {getTranslatedLabel("projects.certificate.grandTotal", "Grand Total")}: {grandTotal.toFixed(2)}
                </td>
            ),
            cell: (props: any) => {
                const type = props.dataItem.__type;
                if (type === "header") return <td />;
                return (
                    <td
                        style={{
                            textAlign: "right",
                            fontWeight: type === "subtotal" ? "bold" : "normal",
                            color: type === "subtotal" ? "#1565c0" : "inherit",
                        }}
                    >
                        {type === "subtotal" || type === "item"
                            ? (props.dataItem.total ?? 0).toFixed(2)
                            : ""}
                    </td>
                );
            },
        },
        {
            field: "statusDescription",
            title: getTranslatedLabel("projects.certificate.status", "Status"),
            width: 140,
            cell: (props: any) => (
                <td>{props.dataItem.__type === "item" ? props.dataItem.statusDescription : ""}</td>
            ),
        },
    ];

    if (isLoading) return <LoadingComponent />;
    if (error) return <Typography color="error">Failed to load certificates</Typography>;

    return (
        <ModalContainer show={show} onClose={onClose} width={1350}>
            <Typography variant="h5" gutterBottom>
                Certificates List - {isWorkmanship ? "Workmanship Contracting" : "Supply"}
            </Typography>

            <KendoGrid
                data={displayData}
                sortable={false} // Disabled to preserve perfect order
                style={{ height: "70vh" }}
            >
                <GridToolbar>
                    <Box sx={{ display: "flex", justifyContent: "space-between", width: "100%" }}>
                        <Typography variant="subtitle1">
                            {isWorkmanship
                                ? `${certificates.length} items • ${new Set(certificates.map((c: any) => c.certificateNumber)).size} certificates`
                                : `${certificates.length} certificates`}
                        </Typography>
                        <Button variant="contained" onClick={onClose}>
                            Close
                        </Button>
                    </Box>
                </GridToolbar>

                {columns.map((col: any, i) => (
                    <Column key={i} {...col} />
                ))}
            </KendoGrid>
        </ModalContainer>
    );
}