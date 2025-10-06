import React, { useState, useCallback, useMemo, useEffect } from "react";
import { Grid, Paper, Typography, Button, Skeleton } from "@mui/material";
import { Form, FormElement, Field } from "@progress/kendo-react-form";
import { Grid as KendoGrid, GridColumn as Column, GridSortChangeEvent, GridPageChangeEvent, GridRowProps, GridCellProps, GridToolbar } from "@progress/kendo-react-grid";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import { RootState, useAppSelector, useFetchGeneralAcctTransEntriesQuery } from "../../../../app/store/configureStore";
import { useFetchGlAccountOrganizationHierarchyLovQuery } from "../../../../app/store/apis";
import { requiredValidator } from "../../../../app/common/form/Validators";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { FormDropDownTreeGlAccount2 } from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import FormInput from "../../../../app/common/form/FormInput";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import useEditMultiAcctgTrans from "../hook/useEditMultiAcctgTrans";
import { router } from "../../../../app/router/Routes";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { AcctgTransEntry } from "../../../../app/models/accounting/acctgTransEntry";
import { useLocation } from "react-router-dom";

interface TransEntry {
    id: string;
    acctgTransEntrySeqId?: string;
    debitGlAccountId?: string;
    creditGlAccountId?: string;
    amount: number;
    description?: string;
    debitCreditFlag: "D" | "C";
}

interface FormValues {
    debitGlAccountId?: string;
    creditGlAccountId?: string;
    amount: number | null;
    description?: string;
}

export default function EditMultiAcctgTrans() {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.orgGL.accounting.trans.multi";
    const companyId = useAppSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyId);
    const companyName = useAppSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
    const location = useLocation();
    const selectedAcctgTrans = location.state?.selectedAcctgTrans;
    const { data: glAccounts, isLoading: isLoadingGlAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, { skip: !companyId });
    const { data: transEntriesData, isLoading: isLoadingEntries } = useFetchGeneralAcctTransEntriesQuery(selectedAcctgTrans?.acctgTransId, { skip: !selectedAcctgTrans?.acctgTransId });
    const [transEntries, setTransEntries] = useState<TransEntry[]>([]);
    const [formResetCounter, setFormResetCounter] = useState(0);
    const [sort, setSort] = useState<SortDescriptor[]>([{ field: "id", dir: "asc" }]);
    const [page, setPage] = useState<State>({ skip: 0, take: 10 });
    const { isLoading, handleUpdateMultiAcctgTransWithEntries } = useEditMultiAcctgTrans();
    const [selectedEntryId, setSelectedEntryId] = useState<string | null>(null);

    const [headerValues, setHeaderValues] = useState({
        transactionDate: selectedAcctgTrans?.transactionDate ? new Date(selectedAcctgTrans.transactionDate) : new Date(),
        headerDescription: selectedAcctgTrans?.description || "",
    });
    
    useEffect(() => {
        if (!companyId) {
            router.navigate("/orgGl");
        }
    }, [companyId]);

    useEffect(() => {
        if (selectedAcctgTrans && transEntriesData) {
            const adjustedEntries = transEntriesData.map((entry: AcctgTransEntry) => ({
                id: `${entry.acctgTransId}-${entry.acctgTransEntrySeqId}`,
                acctgTransEntrySeqId: entry.acctgTransEntrySeqId,
                debitGlAccountId: entry.debitCreditFlag === "D" ? entry.glAccountId : undefined,
                creditGlAccountId: entry.debitCreditFlag === "C" ? entry.glAccountId : undefined,
                amount: entry.amount || 0,
                description: entry.description || "",
                debitCreditFlag: entry.debitCreditFlag as "D" | "C",
            }));
            setTransEntries(adjustedEntries);
        }
    }, [selectedAcctgTrans, transEntriesData]);

    const initialFormValues: FormValues = useMemo(
        () => ({
            debitGlAccountId: undefined,
            creditGlAccountId: undefined,
            amount: null,
            description: "",
        }),
        []
    );

    const handleAddOrUpdateEntry = useCallback(
        async (data: any) => {
            if (!data.isValid) return;
            const { debitGlAccountId, creditGlAccountId, amount, description } = data.values;

            if (selectedEntryId) {
                // Update existing entry
                setTransEntries((prev) =>
                    prev.map((entry) =>
                        entry.id === selectedEntryId
                            ? {
                                ...entry,
                                debitGlAccountId: debitGlAccountId || entry.debitGlAccountId,
                                creditGlAccountId: creditGlAccountId || entry.creditGlAccountId,
                                amount: amount || entry.amount,
                                description: description || entry.description,
                            }
                            : entry
                    )
                );
                setSelectedEntryId(null); // Clear edit mode
            } else {
                // Add new entries
                const newEntries: TransEntry[] = [
                    ...(debitGlAccountId
                        ? [{ id: `D-${Date.now()}`, debitGlAccountId, amount, description, debitCreditFlag: "D" as const, acctgTransEntrySeqId: undefined }]
                        : []),
                    ...(creditGlAccountId
                        ? [{ id: `C-${Date.now() + 1}`, creditGlAccountId, amount, description, debitCreditFlag: "C" as const, acctgTransEntrySeqId: undefined }]
                        : []),
                ];
                setTransEntries((prev) => [...prev, ...newEntries]);
            }
            setFormResetCounter((prev) => prev + 1);
        },
        [selectedEntryId]
    );

    const handleEditEntry = useCallback(
        (entryId: string) => {
            const entry = transEntries.find((e) => e.id === entryId);
            if (entry) {
                setSelectedEntryId(entryId);
                // Update form values with selected entry data
                const form = document.getElementById("editMultiAcctgTransForm") as HTMLFormElement;
                if (form) {
                    formRenderProps?.onChange("debitGlAccountId", { value: entry.debitGlAccountId });
                    formRenderProps?.onChange("creditGlAccountId", { value: entry.creditGlAccountId });
                    formRenderProps?.onChange("amount", { value: entry.amount });
                    formRenderProps?.onChange("description", { value: entry.description });
                }
            }
        },
        [transEntries]
    );

    const handleRemoveEntry = useCallback((entryId: string) => {
        setTransEntries((prev) => prev.filter((entry) => entry.id !== entryId));
    }, []);

    const handleSaveTransaction = useCallback(
        async () => {
            if (!selectedAcctgTrans?.acctgTransId) {
                toast.error(getTranslatedLabel("general.error", "No transaction ID provided"));
                return;
            }
            if (transEntries.length === 0) {
                toast.error(getTranslatedLabel("general.error", "No entries to save"));
                return;
            }
            try {
                await handleUpdateMultiAcctgTransWithEntries({
                    acctgTransId: selectedAcctgTrans.acctgTransId,
                    UpdateMultiAcctgTransParams: {
                        AcctgTransId: selectedAcctgTrans.acctgTransId, // REFACTOR: Include AcctgTransId in UpdateMultiAcctgTransParams
                        // Purpose: Match backend validation requirement for AcctgTransId
                        // Improvement: Ensures payload structure aligns with backend expectations
                        AcctgTransTypeId: "_NA_",
                        TransactionDate: headerValues.transactionDate,
                        OrganizationPartyId: companyId,
                        HeaderDescription: headerValues.headerDescription,
                        Description: transEntries[0]?.description || "",
                        IsPosted: selectedAcctgTrans?.isPosted || "N",
                        GlFiscalTypeId: "ACTUAL",
                    },
                    Entries: transEntries.map((entry) => ({
                        acctgTransEntrySeqId: entry.acctgTransEntrySeqId,
                        debitGlAccountId: entry.debitGlAccountId,
                        creditGlAccountId: entry.creditGlAccountId,
                        amount: entry.amount,
                        description: entry.description,
                        debitCreditFlag: entry.debitCreditFlag,
                    })),
                });
                toast.success(getTranslatedLabel("general.success", "Transaction updated successfully"));
                router.navigate("/orgGl");
            } catch (error) {
                toast.error(getTranslatedLabel("general.error", "Failed to update transaction"));
            }
        },
        [transEntries, companyId, handleUpdateMultiAcctgTransWithEntries, getTranslatedLabel, headerValues, selectedAcctgTrans]
    );

    const pageChange = useCallback((event: GridPageChangeEvent) => {
        setPage(event.page);
    }, []);

    const handleSortChange = useCallback((event: GridSortChangeEvent) => {
        setSort(event.sort);
    }, []);

    const rowRender = useCallback(
        (trElement: React.ReactElement<HTMLTableRowElement>, props: GridRowProps) => {
            const isDebit = props.dataItem.debitCreditFlag === "D";
            const style = { backgroundColor: isDebit ? "rgba(55, 180, 0, 0.32)" : "#ffffff" };
            return React.cloneElement(trElement, { style }, trElement.props.children);
        },
        []
    );

    const GlAccountCell = useCallback(
        ({ dataItem }: GridCellProps) => {
            const glAccountId = dataItem.debitGlAccountId || dataItem.creditGlAccountId;
            const glAccount = glAccounts?.find((acc) => acc.glAccountId === glAccountId);
            return (
                <td
                    style={{ cursor: "pointer", color: "#1976d2" }}
                    onClick={() => handleEditEntry(dataItem.id)}
                >
                    {glAccount?.text || glAccountId || "-"}
                </td>
            );
        },
        [glAccounts, handleEditEntry]
    );

    const UpdateCell = useCallback(
        ({ dataItem }: GridCellProps) => (
            <td>
                <Button
                    variant="text"
                    color="primary"
                    onClick={() => handleEditEntry(dataItem.id)}
                    disabled={isLoading || selectedAcctgTrans?.isPosted === "Y"}
                >
                    {getTranslatedLabel("general.update", "Update")}
                </Button>
            </td>
        ),
        [handleEditEntry, getTranslatedLabel, isLoading, selectedAcctgTrans]
    );

    const RemoveCell = useCallback(
        ({ dataItem }: GridCellProps) => (
            <td>
                <Button
                    variant="text"
                    color="error"
                    onClick={() => handleRemoveEntry(dataItem.id)}
                    disabled={isLoading || selectedAcctgTrans?.isPosted === "Y"}
                >
                    {getTranslatedLabel("general.remove", "Remove")}
                </Button>
            </td>
        ),
        [handleRemoveEntry, getTranslatedLabel, isLoading, selectedAcctgTrans]
    );

    const totalDebit = useMemo(
        () => transEntries.filter((e) => e.debitCreditFlag === "D").reduce((sum, e) => sum + (e.amount || 0), 0),
        [transEntries]
    );
    const totalCredit = useMemo(
        () => transEntries.filter((e) => e.debitCreditFlag === "C").reduce((sum, e) => sum + (e.amount || 0), 0),
        [transEntries]
    );

    let formRenderProps: any; // Store form render props for updating form values


    return (
        <>
            <AccountingMenu selectedMenuItem={"orgGl"} />
            <Paper elevation={5} sx={{ p: 2, borderRadius: 2 }}>
                <Typography variant="h5" sx={{ mb: 2 }}>
                    {getTranslatedLabel(`${localizationKey}.editTitle`, "Edit Accounting Transaction for")} {companyName}
                </Typography>
                <Grid container spacing={2} sx={{ mb: 2 }}>
                    <Grid item xs={3}>
                        <FormDatePicker
                            id="transactionDate"
                            label={getTranslatedLabel(`${localizationKey}.transactionDate`, "Transaction Date *")}
                            value={headerValues.transactionDate}
                            onChange={(e) => setHeaderValues((prev) => ({ ...prev, transactionDate: e.value || new Date() }))}
                            validator={requiredValidator}
                            disabled={selectedAcctgTrans?.isPosted === "Y"}
                        />
                    </Grid>
                    <Grid item xs={3}>
                        <FormInput
                            id="headerDescription"
                            label={getTranslatedLabel(`${localizationKey}.headerDescription`, "Header Description")}
                            value={headerValues.headerDescription}
                            onChange={(e) => setHeaderValues((prev) => ({ ...prev, headerDescription: e.value }))}
                            autoComplete="off"
                            disabled={selectedAcctgTrans?.isPosted === "Y"}
                        />
                    </Grid>
                </Grid>
                <Form
                    initialValues={initialFormValues}
                    key={formResetCounter}
                    onSubmitClick={handleAddOrUpdateEntry}
                    render={(props) => {
                        formRenderProps = props; // Store form render props
                        return (
                            <FormElement id="editMultiAcctgTransForm">
                                <Grid container spacing={2} direction="column">
                                    <Grid item xs={12}>
                                        <Grid container spacing={2} alignItems="flex-end">
                                            <Grid item xs={3}>
                                                {isLoadingGlAccounts ? (
                                                    <Skeleton variant="rounded" height={40} sx={{ width: "100%", borderRadius: "4px" }} />
                                                ) : (
                                                    <Field
                                                        id="debitGlAccountId"
                                                        name="debitGlAccountId"
                                                        label={getTranslatedLabel(`${localizationKey}.debitGlAccount`, "Debit GL Account")}
                                                        data={glAccounts || []}
                                                        component={FormDropDownTreeGlAccount2}
                                                        dataItemKey="glAccountId"
                                                        textField="text"
                                                        selectField="selected"
                                                        expandField="expanded"
                                                        disabled={selectedAcctgTrans?.isPosted === "Y"}
                                                    />
                                                )}
                                            </Grid>
                                            <Grid item xs={3}>
                                                {isLoadingGlAccounts ? (
                                                    <Skeleton variant="rounded" height={40} sx={{ width: "100%", borderRadius: "4px" }} />
                                                ) : (
                                                    <Field
                                                        id="creditGlAccountId"
                                                        name="creditGlAccountId"
                                                        label={getTranslatedLabel(`${localizationKey}.creditGlAccount`, "Credit GL Account")}
                                                        data={glAccounts || []}
                                                        component={FormDropDownTreeGlAccount2}
                                                        dataItemKey="glAccountId"
                                                        textField="text"
                                                        selectField="selected"
                                                        expandField="expanded"
                                                        disabled={selectedAcctgTrans?.isPosted === "Y"}
                                                    />
                                                )}
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Field
                                                    id="amount"
                                                    name="amount"
                                                    label={getTranslatedLabel(`${localizationKey}.amount`, "Amount *")}
                                                    format="n2"
                                                    min={0}
                                                    component={FormNumericTextBox}
                                                    validator={requiredValidator}
                                                    disabled={selectedAcctgTrans?.isPosted === "Y"}
                                                />
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="description"
                                                    name="description"
                                                    label={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                                    component={FormInput}
                                                    autoComplete="off"
                                                    disabled={selectedAcctgTrans?.isPosted === "Y"}
                                                />
                                            </Grid>
                                            <Grid item xs={1}>
                                                <Button
                                                    variant="contained"
                                                    color="success"
                                                    type="submit"
                                                    disabled={
                                                        !formRenderProps.allowSubmit ||
                                                        (!formRenderProps.valueGetter("debitGlAccountId") && !formRenderProps.valueGetter("creditGlAccountId")) ||
                                                        isLoading ||
                                                        selectedAcctgTrans?.isPosted === "Y"
                                                    }
                                                >
                                                    {selectedEntryId ? getTranslatedLabel("general.update", "Update Entry") : getTranslatedLabel("general.add", "Add Entry")}
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={12}>
                                        <Grid container spacing={2} alignItems="center">
                                            <Grid item xs={2}>
                                                <Button
                                                    variant="contained"
                                                    color="primary"
                                                    onClick={handleSaveTransaction}
                                                    disabled={
                                                        !formRenderProps.allowSubmit ||
                                                        (!formRenderProps.valueGetter("debitGlAccountId") && !formRenderProps.valueGetter("creditGlAccountId")) ||
                                                        isLoading
                                                    }
                                                >
                                                    {getTranslatedLabel("general.save", "Save Transaction")}
                                                </Button>
                                            </Grid>
                                            <Grid item xs={12}>
                                                <KendoGrid
                                                    style={{ height: "40vh" }}
                                                    data={orderBy(transEntries, sort).slice(page.skip, page.take + page.skip)}
                                                    sortable
                                                    sort={sort}
                                                    onSortChange={handleSortChange}
                                                    skip={page.skip}
                                                    take={page.take}
                                                    total={transEntries.length}
                                                    pageable
                                                    onPageChange={pageChange}
                                                    rowRender={rowRender}
                                                    resizable={true}
                                                >
                                                    <Column
                                                        field="glAccountId"
                                                        title={getTranslatedLabel(`${localizationKey}.glAccount`, "GL Account")}
                                                        width={300}
                                                        cell={GlAccountCell}
                                                    />
                                                    <Column
                                                        field="amount"
                                                        title={getTranslatedLabel(`${localizationKey}.amount`, "Amount")}
                                                        width={100}
                                                        format="{0:n2}"
                                                    />
                                                    <Column
                                                        field="description"
                                                        title={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                                        width={400}
                                                    />
                                                    <Column
                                                        field="debitCreditFlag"
                                                        title={getTranslatedLabel(`${localizationKey}.debitCredit`, "Debit/Credit")}
                                                        width={150}
                                                    />
                                                    <Column title={""} width={100} cell={RemoveCell} />
                                                    <Column
                                                        title={""}
                                                        footerCell={() => (
                                                            <td style={{ textAlign: "left", fontWeight: "bold", color: "#1565C0" }}>
                                                                {getTranslatedLabel(`${localizationKey}.totalDebit`, "Total Debit")}: {totalDebit.toFixed(2)} |{" "}
                                                                {getTranslatedLabel(`${localizationKey}.totalCredit`, "Total Credit")}: {totalCredit.toFixed(2)}
                                                            </td>
                                                        )}
                                                    />
                                                </KendoGrid>
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                </Grid>
                                {isLoadingEntries && <Skeleton variant="rectangular" width="100%" height="60vh" sx={{ borderRadius: 2 }} />}
                            </FormElement>
                        );
                    }}
                />
            </Paper>
        </>
    );
}