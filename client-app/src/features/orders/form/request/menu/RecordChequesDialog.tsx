import React, { useEffect, useState } from "react";
import {
    Alert,
    Autocomplete,
    Box,
    Button,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    TextField,
    Typography,
} from "@mui/material";
import { Grid as KendoGrid, GridColumn as Column, GridCellProps } from "@progress/kendo-react-grid";
import { toast } from "react-toastify";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import {
    ChequeablePaymentDto,
    useGetChequeablePaymentsQuery,
    useRecordSalesRequestChequesMutation,
} from "../../../../../app/store/apis/salesRequestApi";
import { useFetchPaymentMethodsQuery } from "../../../../../app/store/apis/payment/paymentsApi";

interface PaymentMethodOption {
    paymentMethodId: string;
    paymentMethodTypeId?: string;
    description?: string;
}

const BANK_PAYMENT_METHOD_TYPE_IDS = ["PERSONAL_CHECK", "COMPANY_CHECK", "CERTIFIED_CHECK"];

const generateChequeNumbers = (start: string, count: number): string[] => {
    const match = start.match(/^(.*?)(\d+)$/);
    if (!match) {
        return Array.from({ length: count }, (_, i) => (i === 0 ? start : `${start}${i}`));
    }
    const [, prefix, digits] = match;
    const width = digits.length;
    const startNum = parseInt(digits, 10);
    return Array.from({ length: count }, (_, i) => `${prefix}${String(startNum + i).padStart(width, "0")}`);
};

interface ChequeRow extends ChequeablePaymentDto {
    chequeNumber: string;
}

interface RecordChequesDialogProps {
    open: boolean;
    onClose: () => void;
    salesRequestId: string;
    fromPartyName?: string | null;
    apartmentName?: string | null;
}

export const RecordChequesDialog: React.FC<RecordChequesDialogProps> = ({
    open,
    onClose,
    salesRequestId,
    fromPartyName,
    apartmentName,
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const t = (key: string, fallback: string) => getTranslatedLabel(`salesRequest.cheques.${key}`, fallback);

    const { data: chequeablePayments, isFetching, isError } = useGetChequeablePaymentsQuery(salesRequestId, {
        skip: !open,
    });
    const { data: paymentMethods } = useFetchPaymentMethodsQuery(undefined, { skip: !open });
    const [recordCheques, { isLoading: isSaving }] = useRecordSalesRequestChequesMutation();

    const [rows, setRows] = useState<ChequeRow[]>([]);
    const [bank, setBank] = useState<PaymentMethodOption | null>(null);
    const [startingChequeNumber, setStartingChequeNumber] = useState("");

    const banks = ((paymentMethods ?? []) as PaymentMethodOption[]).filter((pm) =>
        BANK_PAYMENT_METHOD_TYPE_IDS.includes(pm.paymentMethodTypeId ?? "")
    );

    useEffect(() => {
        if (chequeablePayments) {
            setRows(chequeablePayments.map((p) => ({ ...p, chequeNumber: "" })));
        }
    }, [chequeablePayments]);

    useEffect(() => {
        if (!open) {
            setBank(null);
            setStartingChequeNumber("");
        }
    }, [open]);

    const handleChequeNumberChange = (paymentId: string, value: string) => {
        setRows((prev) =>
            prev.map((r) => (r.paymentId === paymentId ? { ...r, chequeNumber: value } : r))
        );
    };

    const handleGenerateChequeNumbers = () => {
        if (!startingChequeNumber.trim() || rows.length === 0) return;
        const generated = generateChequeNumbers(startingChequeNumber.trim(), rows.length);
        setRows((prev) => prev.map((r, i) => ({ ...r, chequeNumber: generated[i] })));
    };

    const chequeNumberCell = (props: GridCellProps) => {
        const row = props.dataItem as ChequeRow;
        return (
            <td>
                <input
                    className="k-input k-input-md k-rounded-md k-input-solid"
                    style={{ width: "100%" }}
                    value={row.chequeNumber}
                    onChange={(e) => handleChequeNumberChange(row.paymentId, e.target.value)}
                    placeholder={t("chequeNumberPlaceholder", "Cheque #")}
                />
            </td>
        );
    };

    const readyRows = rows.filter((r) => r.chequeNumber.trim().length > 0);
    const canSave = !!bank && readyRows.length > 0 && !isSaving;

    const handleSave = async () => {
        if (!bank) {
            toast.error(t("bankRequired", "Please select the bank"));
            return;
        }

        try {
            await recordCheques({
                salesRequestId,
                cheques: readyRows.map((r) => ({
                    paymentId: r.paymentId,
                    paymentMethodId: bank.paymentMethodId,
                    chequeNumber: r.chequeNumber.trim(),
                    chequeDate: r.dueDate ?? "",
                    amount: r.amount,
                })),
            }).unwrap();

            toast.success(t("saved", "Cheques recorded"));
            onClose();
        } catch (err: any) {
            toast.error(err?.data?.title ?? t("saveError", "Failed to record cheques"));
        }
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
            <DialogTitle>{t("title", "Record Received Cheques")}</DialogTitle>
            <DialogContent>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    {fromPartyName} — {apartmentName}
                </Typography>

                <Box sx={{ mb: 2, display: "flex", gap: 2, flexWrap: "wrap" }}>
                    <Autocomplete
                        options={banks}
                        getOptionLabel={(option) => option.description ?? option.paymentMethodId}
                        isOptionEqualToValue={(option, value) => option.paymentMethodId === value.paymentMethodId}
                        value={bank}
                        onChange={(_, value) => setBank(value)}
                        sx={{ minWidth: 260 }}
                        renderInput={(params) => (
                            <TextField {...params} label={t("bank", "Bank *")} size="small" />
                        )}
                    />

                    <TextField
                        label={t("startingChequeNumber", "Starting Cheque Number")}
                        size="small"
                        value={startingChequeNumber}
                        onChange={(e) => setStartingChequeNumber(e.target.value)}
                        sx={{ minWidth: 220 }}
                    />
                    <Button
                        variant="outlined"
                        onClick={handleGenerateChequeNumbers}
                        disabled={!startingChequeNumber.trim() || rows.length === 0}
                    >
                        {t("generate", "Generate")}
                    </Button>
                </Box>

                {isFetching ? (
                    <Box sx={{ display: "flex", justifyContent: "center", p: 3 }}>
                        <CircularProgress size={28} />
                    </Box>
                ) : isError ? (
                    <Alert severity="error">{t("loadError", "Failed to load chequeable installments")}</Alert>
                ) : rows.length === 0 ? (
                    <Alert severity="info">{t("none", "No installments are pending a cheque.")}</Alert>
                ) : (
                    <KendoGrid data={rows} style={{ maxHeight: 400 }}>
                        <Column field="dueDate" title={t("dueDate", "Due Date")} width="120px" />
                        <Column field="comments" title={t("description", "Description")} />
                        <Column field="amount" title={t("amount", "Amount")} format="{0:n2}" width="120px" />
                        <Column
                            field="chequeNumber"
                            title={t("chequeNumber", "Cheque Number")}
                            cell={chequeNumberCell}
                            width="160px"
                        />
                    </KendoGrid>
                )}
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} disabled={isSaving}>
                    {getTranslatedLabel("general.cancel", "Cancel")}
                </Button>
                <Button
                    variant="contained"
                    onClick={handleSave}
                    disabled={!canSave}
                    startIcon={isSaving ? <CircularProgress size={16} /> : null}
                >
                    {t("save", "Save")}
                </Button>
            </DialogActions>
        </Dialog>
    );
};
