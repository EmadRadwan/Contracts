import React from "react";
import { Grid, Box, Typography } from "@mui/material";
import { Ribbon, RibbonContainer } from "react-ribbons";
import { SalesRequest } from "../../../../../app/models/order/SalesRequest";
import {SalesRequestActionsMenu} from "../menu/SalesRequestActionsMenu";
import {Can} from "../../../../account/Can";

interface SalesRequestHeaderProps {
    salesRequest?: SalesRequest;
    editMode: number;
    defaultAdvancePercent: number;
    defaultMaintenancePercent: number;
    onOpenDefaultsModal: () => void;
    language: string;
    getTranslatedLabel: (key: string, fallback: string) => string;
    onSalesRequestUpdated?: (updated: SalesRequest) => void;
    onSalesRequestDeleted?: () => void;
    disabledActions?: boolean;
}

export const SalesRequestHeader: React.FC<SalesRequestHeaderProps> = React.memo(({
                                                                                     salesRequest,
                                                                                     editMode,
                                                                                     defaultAdvancePercent,
                                                                                     defaultMaintenancePercent,
                                                                                     onOpenDefaultsModal,
                                                                                     language,
                                                                                     getTranslatedLabel,
                                                                                     onSalesRequestUpdated,
                                                                                     onSalesRequestDeleted,
                                                                                     disabledActions = false,
                                                                                 }) => {
    const statusId = salesRequest?.statusId as string | undefined;
    const statusDescription = salesRequest?.statusDescription as string | undefined;

    const ribbonLabel = statusDescription ?? {
        SALES_REQUEST_CREATED: "Created",
        SALES_REQUEST_APPROVED: "Approved",
    }[statusId ?? ""] ?? "Unknown";

    const ribbonBg = {
        SALES_REQUEST_CREATED: "#1976d2",
        SALES_REQUEST_APPROVED: "#4caf50",
    }[statusId ?? ""] ?? "#757575";

    return (
        <Grid container spacing={2} alignItems="center" position="relative">
            <Grid item xs={11}>
                <Box display="flex" justifyContent="space-between" sx={{ p: 2 }}>
                    <Typography
                        variant="h4"
                        color={salesRequest?.salesRequestId ? "text.primary" : "success.main"}
                        sx={{ fontWeight: 500 }}
                    >
                        {salesRequest?.salesRequestId ? (
                            <>
                                <Box component="span" sx={{ opacity: 0.7, mr: 1 }}>
                                    {getTranslatedLabel("salesRequest.form.new2", "Sales Request")}:
                                </Box>
                                <Box component="span" fontWeight="bold">
                                    {salesRequest.salesRequestId}
                                </Box>
                            </>
                        ) : (
                            getTranslatedLabel("salesRequest.form.new", "New Sales Request")
                        )}
                    </Typography>

                    <Typography variant="caption" color="text.secondary">
                        Default Advance: {(defaultAdvancePercent * 100).toFixed(0)}% |
                        Maintenance Deposit: {(defaultMaintenancePercent * 100).toFixed(0)}%
                        {" "}(<a
                        href="#"
                        onClick={(e) => {
                            e.preventDefault();
                            onOpenDefaultsModal();
                        }}
                    >
                        change
                    </a>)
                    </Typography>

                    {(editMode === 2 || editMode === 3) && (
                        // ViewSalesRequest-only users are read-only: never expose approve/reset/delete actions
                        <Can perform="CreateSalesRequest">
                            <SalesRequestActionsMenu
                                salesRequestId={salesRequest?.salesRequestId}
                                currentStatusId={salesRequest?.statusId}
                                isChequesDelivered={salesRequest?.isChequesDelivered}
                                disabled={disabledActions}
                                onSalesRequestUpdated={onSalesRequestUpdated}
                                onSalesRequestDeleted={onSalesRequestDeleted}
                                fromPartyName={salesRequest?.fromPartyName}
                                apartmentName={salesRequest?.apartmentName}
                            />
                        </Can>
                    )}
                </Box>
            </Grid>

            {(editMode === 2 || editMode === 3) && (
                <Grid item xs={1}>
                    <RibbonContainer>
                        <Ribbon
                            side={language === "ar" ? "left" : "right"}
                            type="corner"
                            size="large"
                            backgroundColor={ribbonBg}
                            color="#ffffff"
                            fontFamily="sans-serif"
                        >
                            {ribbonLabel}
                        </Ribbon>
                    </RibbonContainer>
                </Grid>
            )}
        </Grid>
    );
});

SalesRequestHeader.displayName = "SalesRequestHeader";