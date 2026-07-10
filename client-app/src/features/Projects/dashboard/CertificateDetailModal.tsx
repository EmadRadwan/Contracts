import React, { useMemo } from "react";
import { v4 as uuidv4 } from "uuid";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { Box, Button, Grid, Tooltip, Typography } from "@mui/material";
import { toast } from "react-toastify";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useAppDispatch } from "../../../app/store/configureStore";
import { useFetchCertificateItemsReadOnlyQuery } from "../../../app/store/apis/certificateItemsApi";
import { setProcessedCertificateItems } from "../slice/certificateItemsUiSlice";
import { CertificateItem } from "../../../app/models/project/certificateItem";

export interface CertificateDetailSummary {
    workEffortId?: string;
    certificateNumber?: string;
    projectName?: string;
    partyName?: string;
    description?: string;
    statusDescription?: string;
    certificateCategory?: string;
}

interface CertificateDetailModalProps {
    show: boolean;
    onClose: () => void;
    certificate?: CertificateDetailSummary;
    // Purpose: Copying items only makes sense while creating a brand-new certificate — once a
    // certificate already exists, its item list is the source of truth, so the button is
    // disabled (not hidden, so it's discoverable) outside that context.
    canCopyItems: boolean;
    // Purpose: Called after items are copied into the currently-open certificate's Redux state,
    // so the parent (CertificatesListModal) can also close itself — "both modals close" per the
    // requested UX. This modal only ever closes itself via `onClose`.
    onItemsCopied: () => void;
}

export default function CertificateDetailModal({
                                                     show,
                                                     onClose,
                                                     certificate,
                                                     canCopyItems,
                                                     onItemsCopied,
                                                 }: CertificateDetailModalProps) {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const localizationKey = "projects.certificate.items.list";

    const workEffortId = certificate?.workEffortId;
    const { data: items = [], isFetching } = useFetchCertificateItemsReadOnlyQuery(workEffortId || "", {
        skip: !show || !workEffortId,
    });

    const isWorkmanship = certificate?.certificateCategory === "WORKMANSHIP_CONTRACTING_CERTIFICATE";

    const nonDeletedItems = useMemo(() => items.filter((i) => !i.isDeleted), [items]);

    const displayItems = useMemo(
        () =>
            nonDeletedItems.map((item) => ({
                ...item,
                displayTotal: +((isWorkmanship ? item.net ?? 0 : item.totalAmount ?? 0)).toFixed(2),
            })),
        [nonDeletedItems, isWorkmanship]
    );

    const grandTotal = displayItems.reduce((sum, i) => sum + (i.displayTotal ?? 0), 0);

    const handleCopyItems = () => {
        if (nonDeletedItems.length === 0 || !canCopyItems) return;

        // Purpose: New items must get fresh TEMP- ids (the established convention for
        // not-yet-persisted certificate items — see CertificateItemKendoBulkAddV2.tsx),
        // and workEffortParentId cleared so the backend always treats them as inserts
        // against the currently-open certificate rather than the source certificate.
        const copiedItems: CertificateItem[] = nonDeletedItems.map((item) => ({
            ...item,
            workEffortId: `TEMP-${uuidv4()}`,
            workEffortParentId: "",
            isDeleted: false,
        }));

        dispatch(setProcessedCertificateItems(copiedItems));
        toast.success(
            getTranslatedLabel("projects.certificate.modal.copySuccess", "Items copied to current certificate")
        );
        onClose();
        onItemsCopied();
    };

    const columns = [
        {
            field: "productName",
            title: getTranslatedLabel(`${localizationKey}.product`, "Product"),
            width: 220,
        },
        {
            field: "description",
            title: getTranslatedLabel(`${localizationKey}.description`, "Description"),
            width: 220,
        },
        {
            field: "quantity",
            title: getTranslatedLabel(`${localizationKey}.quantity`, "Quantity"),
            width: 100,
            format: "{0:n3}",
        },
        {
            field: "uomName",
            title: getTranslatedLabel(`${localizationKey}.unitOfMeasure`, "UOM"),
            width: 120,
        },
        ...(isWorkmanship
            ? [
                { field: "materialPrice", title: getTranslatedLabel(`${localizationKey}.materialPrice`, "Material Price"), width: 140, format: "{0:n3}" },
                { field: "laborPrice", title: getTranslatedLabel(`${localizationKey}.laborPrice`, "Labor Price"), width: 140, format: "{0:n3}" },
                { field: "deductions", title: getTranslatedLabel(`${localizationKey}.deductions`, "Deductions"), width: 120, format: "{0:n2}" },
                { field: "insurance", title: getTranslatedLabel(`${localizationKey}.insurance`, "Insurance"), width: 120, format: "{0:n2}" },
                { field: "additionalInsurance", title: getTranslatedLabel(`${localizationKey}.additionalInsurance`, "Additional Insurance"), width: 150, format: "{0:n2}" },
                { field: "achievementPercentage", title: getTranslatedLabel(`${localizationKey}.achievementPercentage`, "Achievement %"), width: 200, format: "{0:n9}" },
            ]
            : [
                { field: "unitPrice", title: getTranslatedLabel(`${localizationKey}.unitPrice`, "Unit Price"), width: 120, format: "{0:n3}" },
                { field: "discount", title: getTranslatedLabel(`${localizationKey}.discount`, "Discount"), width: 120, format: "{0:n2}" },
                { field: "transportationExpenses", title: getTranslatedLabel(`${localizationKey}.transportationExpenses`, "Transportation Expenses"), width: 170, format: "{0:n2}" },
                { field: "gratuities", title: getTranslatedLabel(`${localizationKey}.gratuities`, "Gratuities"), width: 120, format: "{0:n2}" },
            ]),
        {
            field: "displayTotal",
            title: getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount"),
            width: 140,
            format: "{0:n2}",
        },
    ];

    return (
        <ModalContainer show={show} onClose={onClose} width={1250}>
            <Typography variant="h5" gutterBottom>
                {getTranslatedLabel("projects.certificate.form.title", "Project Certificate No")}: {certificate?.certificateNumber}
            </Typography>

            <Grid container spacing={2} sx={{ mb: 2 }}>
                <Grid item xs={3}>
                    <Typography variant="body2" color="text.secondary">
                        {getTranslatedLabel("projects.certificate.projectName", "Project")}
                    </Typography>
                    <Typography>{certificate?.projectName || "-"}</Typography>
                </Grid>
                <Grid item xs={3}>
                    <Typography variant="body2" color="text.secondary">
                        {getTranslatedLabel("projects.certificate.modal.party", "Party")}
                    </Typography>
                    <Typography>{certificate?.partyName || "-"}</Typography>
                </Grid>
                <Grid item xs={3}>
                    <Typography variant="body2" color="text.secondary">
                        {getTranslatedLabel("projects.certificate.status", "Status")}
                    </Typography>
                    <Typography>{certificate?.statusDescription || "-"}</Typography>
                </Grid>
                <Grid item xs={3}>
                    <Typography variant="body2" color="text.secondary">
                        {getTranslatedLabel("projects.certificate.form.description", "Description")}
                    </Typography>
                    <Typography>{certificate?.description || "-"}</Typography>
                </Grid>
            </Grid>

            {isFetching ? (
                <LoadingComponent />
            ) : (
                <KendoGrid data={displayItems} style={{ height: "50vh" }} scrollable="scrollable" resizable sortable={false}>
                    <GridToolbar>
                        <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", width: "100%", gap: 2 }}>
                            <Typography variant="subtitle1" sx={{ fontWeight: "bold", whiteSpace: "nowrap" }}>
                                {getTranslatedLabel("projects.certificate.grandTotal", "Grand Total")}: {grandTotal.toFixed(2)}
                            </Typography>
                            <Box sx={{ display: "flex", gap: 1 }}>
                                <Tooltip
                                    title={
                                        canCopyItems
                                            ? ""
                                            : getTranslatedLabel(
                                                "projects.certificate.modal.copyItemsDisabled",
                                                "Only available while creating a new certificate"
                                            )
                                    }
                                >
                                    <span>
                                        <Button
                                            variant="contained"
                                            color="secondary"
                                            disabled={displayItems.length === 0 || !canCopyItems}
                                            onClick={handleCopyItems}
                                        >
                                            {getTranslatedLabel("projects.certificate.modal.copyItems", "Copy Items to Current Certificate")}
                                        </Button>
                                    </span>
                                </Tooltip>
                                <Button variant="outlined" onClick={onClose}>
                                    {getTranslatedLabel("general.close", "Close")}
                                </Button>
                            </Box>
                        </Box>
                    </GridToolbar>
                    {columns.map((col, i) => (
                        <Column key={i} {...col} />
                    ))}
                </KendoGrid>
            )}
        </ModalContainer>
    );
}
