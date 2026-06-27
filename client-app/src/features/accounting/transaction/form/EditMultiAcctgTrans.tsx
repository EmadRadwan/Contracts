import React, { useState, useCallback, useMemo, useEffect } from "react";
import {Grid, Paper, Typography, Button, Skeleton, Chip, Menu, MenuItem, TextField, Autocomplete, Box} from "@mui/material";
import { Form, FormElement, Field } from "@progress/kendo-react-form";
import { Grid as KendoGrid, GridColumn as Column, GridSortChangeEvent, GridPageChangeEvent, GridRowProps, GridCellProps, GridToolbar } from "@progress/kendo-react-grid";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import { RootState, useAppSelector, useFetchGeneralAcctTransEntriesQuery, useFetchAcctgTransTypesQuery } from "../../../../app/store/configureStore";
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
import {useLocation, useParams} from "react-router-dom";
import useMultiAcctgTrans from "../hook/useMultiAcctgTrans";
import {FormComboBoxVirtualParty} from "../../../../app/common/form/FormComboBoxVirtualParty";
import useDuplicateAcctgTrans from "../hook/useDuplicateAcctgTrans";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {MultiAcctgTransExcel} from "../report/MultiAcctgTransExcel";
import {Can} from "../../../account/Can";
import {useGetCostCentersQuery} from "../../../../app/store/apis/accounting/paymentTypesApi";
import CreateCostCenterModal from "../../payment/form/CreateCostCenterModal";

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
    const {user} = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";
    const companyName = useAppSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
    
    const location = useLocation();
    const { acctgTransId: currentTransId } = useParams<{ acctgTransId: string }>();

    const initialTransFromState = location.state?.selectedAcctgTrans;


    
    const { language } = useAppSelector((state) => state.localization);
    const { data: glAccounts, isLoading: isLoadingGlAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, { skip: !companyId });

    const { data: acctgTransTypes } = useFetchAcctgTransTypesQuery(undefined);

    const { data: transEntriesData, isLoading: isLoadingEntries } = useFetchGeneralAcctTransEntriesQuery(currentTransId, {
        skip: !currentTransId,
    });
    const [transEntries, setTransEntries] = useState<TransEntry[]>([]);
    const [formResetCounter, setFormResetCounter] = useState(0);
    const [sort, setSort] = useState<SortDescriptor[]>([{ field: "id", dir: "asc" }]);
    const [page, setPage] = useState<State>({ skip: 0, take: 10 });
    const { isLoading: isUpdating, handleUpdateMultiAcctgTransWithEntries } = useEditMultiAcctgTrans();
    const { isLoading: isPosting, postTransaction, unpostTransaction } = useMultiAcctgTrans();
    const [justPosted, setJustPosted] = useState(false);
    const [justUnposted, setJustUnposted] = useState(false);
    const { duplicate, isDuplicating } = useDuplicateAcctgTrans();
    const [selectedEntryId, setSelectedEntryId] = useState<string | null>(null);

    const isPosted = justPosted || (transEntriesData?.[0]?.isPosted === "Y" && !justUnposted) || (initialTransFromState?.isPosted === "Y" && !justUnposted);

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const openActions = Boolean(anchorEl);
    const [showCreateCostCenter, setShowCreateCostCenter] = useState(false);

    const { data: costCenters = [] } = useGetCostCentersQuery();

    const handleActionsClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleActionsClose = () => {
        setAnchorEl(null);
    };

    const [headerValues, setHeaderValues] = useState(() => ({
        transactionDate: initialTransFromState?.transactionDate
            ? new Date(initialTransFromState.transactionDate)
            : new Date(),
        headerDescription: initialTransFromState?.description || "",
        party: initialTransFromState?.partyId
            ? {
                fromPartyId: initialTransFromState.partyId,
                fromPartyName: initialTransFromState.partyName || "",
            }
            : null,
        costCenterId: initialTransFromState?.costCenterId || "",
    }));

    // Populate entries when data arrives (using currentTransId)
    useEffect(() => {
        if (transEntriesData) {
            const adjusted = transEntriesData.map((entry: AcctgTransEntry) => ({
                id: `${entry.acctgTransId}-${entry.acctgTransEntrySeqId}`,
                acctgTransEntrySeqId: entry.acctgTransEntrySeqId,
                debitGlAccountId: entry.debitCreditFlag === "D" ? entry.glAccountId : undefined,
                creditGlAccountId: entry.debitCreditFlag === "C" ? entry.glAccountId : undefined,
                amount: entry.amount || 0,
                description: entry.description || "",
                debitCreditFlag: entry.debitCreditFlag as "D" | "C",
            }));
            setTransEntries(adjusted);
        }
    }, [transEntriesData]);

    // Reset justPosted and justUnposted when transaction ID changes
    useEffect(() => {
        setJustPosted(false);
        setJustUnposted(false);
    }, [currentTransId]);

    const accountMap = useMemo(() => {
        const map = new Map<string, any>();
        const flatten = (items: any[]) => {
            items.forEach((item) => {
                map.set(item.glAccountId, item);
                if (item.items && item.items.length > 0) {
                    flatten(item.items);
                }
            });
        };
        if (glAccounts) flatten(glAccounts);
        return map;
    }, [glAccounts]);

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

    const handleSaveTransaction = useCallback(async () => {
        if (!currentTransId) {
            toast.error(getTranslatedLabel("general.error", "No transaction ID provided"));
            return;
        }
        if (transEntries.length === 0) {
            toast.error(getTranslatedLabel("general.error", "No entries to save"));
            return;
        }

        try {
            await handleUpdateMultiAcctgTransWithEntries({
                acctgTransId: currentTransId,
                updateMultiAcctgTransParams: {
                    acctgTransId: currentTransId,
                    transactionDate: headerValues.transactionDate,
                    organizationPartyId: companyId,
                    headerDescription: headerValues.headerDescription,
                    description: transEntries[0]?.description || "",
                    partyId: headerValues.party?.fromPartyId || undefined,
                    costCenterId: headerValues.costCenterId || undefined,
                },
                entries: transEntries.map((entry) => ({
                    acctgTransEntrySeqId: entry.acctgTransEntrySeqId,
                    debitGlAccountId: entry.debitGlAccountId,
                    creditGlAccountId: entry.creditGlAccountId,
                    amount: entry.amount,
                    description: entry.description,
                    debitCreditFlag: entry.debitCreditFlag,
                })),
            });

            toast.success(getTranslatedLabel("general.success", "Transaction updated successfully"));
        } catch (error) {
            toast.error(getTranslatedLabel("general.error", "Failed to update transaction"));
        }
    }, [
        currentTransId,
        transEntries,
        companyId,
        headerValues,
        handleUpdateMultiAcctgTransWithEntries,
        getTranslatedLabel,
        justPosted,
    ]);

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
            // REFACTOR: Fetch glAccountTypeDescription directly from transEntriesData
            // Purpose: Use transEntriesData as the source for both glAccountId and glAccountTypeDescription, as it's the data source for the grid
            // Improvement: Eliminates dependency on glAccounts, ensuring accurate display of glAccountTypeDescription (e.g., "UNINVOICED ITEM RECEIPTS")
            const transEntry = transEntriesData?.find((entry: AcctgTransEntry) => entry.glAccountId === glAccountId);
            const displayText = transEntry
                ? `${transEntry.glAccountId} - ${transEntry.glAccountTypeDescription || 'N/A'}`
                : glAccountId
                    ? `${glAccountId} - N/A`
                    : "-";

            return (
                <td
                    style={{ cursor: "pointer", color: "#1976d2" }}
                    onClick={() => handleEditEntry(dataItem.id)}
                >
                    {displayText}
                </td>
            );
        },
        [transEntriesData, handleEditEntry]
    );


    const RemoveCell = useCallback(
        ({ dataItem }: GridCellProps) => (
            <td>
                <Button
                    variant="text"
                    color="error"
                    onClick={() => handleRemoveEntry(dataItem.id)}
                    disabled={isUpdating || isPosting || isPosted}
                >
                    {getTranslatedLabel("general.remove", "Remove")}
                </Button>
            </td>
        ),
        [handleRemoveEntry, getTranslatedLabel, isUpdating, isPosting, isPosted]
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

    const handlePostTransaction = useCallback(async () => {
        if (!currentTransId) {
            toast.error("No transaction ID available");
            return;
        }

        try {
            const messages = await postTransaction(currentTransId);

            if (Array.isArray(messages) && messages.length === 0) {
                toast.success("Accounting Transaction Posted Successfully");
                setJustPosted(true);
                setJustUnposted(false);
            } else if (Array.isArray(messages)) {
                // ENHANCEMENT: Better display of backend messages
                messages.forEach((msg: string) => {
                    const translatedMsg = getTranslatedLabel(`accounting.messages.${msg}`, msg);
                    if (msg.includes("Error Journal") || msg.toLowerCase().includes("failed") || msg.toLowerCase().includes("error")) {
                        toast.error(translatedMsg);
                    } else {
                        toast.warn(translatedMsg);
                    }
                });
            }
        } catch {
            // handled in hook
        }
    }, [currentTransId, postTransaction]);

    const handleUnpostTransaction = useCallback(async () => {
        if (!currentTransId) return;
        try {
            await unpostTransaction(currentTransId);
            setJustPosted(false);
            setJustUnposted(true);
            handleActionsClose();
            // Refresh entries to get updated isPosted status from backend
            if (typeof transEntriesData !== 'undefined') {
                // RTK Query will automatically refetch if tags are invalidated, 
                // but we can also manually trigger or rely on justPosted/justUnposted for immediate UI feedback.
            }
        } catch (error) {
            // handled in hook
        }
    }, [currentTransId, unpostTransaction]);

    const isLoading = isUpdating || isPosting;

    return (
        <>
            <AccountingMenu selectedMenuItem={"orgGl"} />
            <Paper elevation={5} sx={{ p: 2, borderRadius: 2 }}>
                <Grid container justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
                    <Grid item>
                        <Typography variant="h5">
                            {getTranslatedLabel(`${localizationKey}.editTitle`, "Edit Accounting Transaction")}
                            {currentTransId && (
                                <span style={{ marginLeft: 8, color: "#1976d2", fontWeight: 600 }}>
                                    #{currentTransId}
                                </span>
                            )}
                            {isPosted && (
                                <Chip
                                    label={getTranslatedLabel("general.posted", "Posted")}
                                    color="success"
                                    size="small"
                                    sx={{ ml: 2 }}
                                />
                            )}
                        </Typography>
                    </Grid>
                    {currentTransId && (
                        <Grid item>
                            <Button
                                variant="contained"
                                color="primary"
                                onClick={handleActionsClick}
                                disabled={isLoading}
                            >
                                {getTranslatedLabel("general.actions", "Actions")}
                            </Button>
                            <Menu
                                anchorEl={anchorEl}
                                open={openActions}
                                onClose={handleActionsClose}
                            >
                                {isPosted && (
                                    <Can perform="Unpost_Transaction">
                                        <MenuItem onClick={handleUnpostTransaction} sx={{ color: 'error.main' }}>
                                            {getTranslatedLabel(`${localizationKey}.unpostTransaction`, "Un-post Transaction")}
                                        </MenuItem>
                                    </Can>
                                )}
                                {!isPosted && (
                                    <MenuItem onClick={handlePostTransaction}>
                                        {getTranslatedLabel(`${localizationKey}.postTransaction`, "Post Transaction")}
                                    </MenuItem>
                                )}
                                <MenuItem onClick={() => { duplicate(currentTransId); handleActionsClose(); }}>
                                    {getTranslatedLabel(`${localizationKey}.duplicateAcctgTrans`, "Duplicate")}
                                </MenuItem>
                            </Menu>
                        </Grid>
                    )}
                </Grid>
                <Grid container spacing={2} sx={{ mb: 2 }} alignItems="flex-end">
                    <Grid item xs={3}>
                        <FormDatePicker
                            id="transactionDate"
                            label={getTranslatedLabel(`${localizationKey}.transactionDate`, "Transaction Date *")}
                            value={headerValues.transactionDate}
                            onChange={(e) =>
                                setHeaderValues((prev) => ({
                                    ...prev,
                                    transactionDate: e.value || new Date(),
                                }))
                            }
                            validator={requiredValidator}
                            disabled={isPosted}
                        />
                    </Grid>
                    <Grid item xs={3}>
                        <FormInput
                            id="headerDescription"
                            label={getTranslatedLabel(`${localizationKey}.headerDescription`, "Header Description")}
                            value={headerValues.headerDescription}
                            onChange={(e) =>
                                setHeaderValues((prev) => ({ ...prev, headerDescription: e.value }))
                            }
                            autoComplete="off"
                            disabled={isPosted}
                        />
                    </Grid>

                    <Grid item xs={3}>
                        <FormComboBoxVirtualParty
                            id="partyId"
                            label={getTranslatedLabel(`${localizationKey}.party`, "Employee")}
                            value={headerValues.party}
                            onChange={(e: any) =>
                                setHeaderValues((prev) => ({ ...prev, party: e.value }))
                            }
                            valueField="fromPartyId"
                            textField="fromPartyName"
                            disabled={isPosted}
                        />
                    </Grid>
                    <Grid item xs={3}>
                        <Box sx={{ display: "flex", alignItems: "flex-end", gap: 1 }}>
                            <Autocomplete
                                options={costCenters}
                                getOptionLabel={(option) => option.description || ""}
                                value={costCenters.find(c => c.costCenterId === headerValues.costCenterId) || null}
                                onChange={(_event, newValue) => {
                                    setHeaderValues(prev => ({
                                        ...prev,
                                        costCenterId: newValue?.costCenterId || "",
                                    }));
                                }}
                                isOptionEqualToValue={(option, value) =>
                                    option.costCenterId === value?.costCenterId
                                }
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label={getTranslatedLabel(`${localizationKey}.costCenter`, "Cost Center")}
                                        variant="outlined"
                                        size="small"
                                    />
                                )}
                                sx={{ flex: 1 }}
                                disabled={isPosted}
                            />
                            <Button
                                size="small"
                                variant="outlined"
                                color="secondary"
                                onClick={() => setShowCreateCostCenter(true)}
                                disabled={isPosted}
                                sx={{ minWidth: 36, height: 40, flexShrink: 0 }}
                            >
                                +
                            </Button>
                        </Box>
                    </Grid>
                </Grid>
                <CreateCostCenterModal
                    open={showCreateCostCenter}
                    onClose={() => setShowCreateCostCenter(false)}
                    isOutPayment={false}
                />
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
                                                        disabled={isPosted}
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
                                                        disabled={isPosted}
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
                                                    disabled={isPosted}
                                                />
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="description"
                                                    name="description"
                                                    label={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                                    component={FormInput}
                                                    autoComplete="off"
                                                    disabled={isPosted}
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
                                                        isPosted
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
                                                        transEntries.length === 0 ||           // no entries → disable
                                                        totalDebit !== totalCredit ||          // NOT BALANCED → disable
                                                        isLoading ||                           // ongoing request → disable
                                                        isPosted                               // already posted → disable
                                                    }
                                                >
                                                    {getTranslatedLabel("general.save", "Save Transaction")}
                                                </Button>
                                            </Grid>
                                            {currentTransId && (
                                                <Grid item xs={2}>
                                                    <MultiAcctgTransExcel
                                                        companyName={companyName}
                                                        transactionId={currentTransId}
                                                        headerValues={{
                                                            ...headerValues,
                                                            acctgTransTypeId: initialTransFromState?.acctgTransTypeId || null
                                                        }}
                                                        entries={transEntries}
                                                        acctgTransTypes={acctgTransTypes || []}
                                                        accountMap={accountMap}
                                                        getTranslatedLabel={getTranslatedLabel}
                                                        language={language}
                                                    />
                                                </Grid>
                                            )}
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
                                {(isLoadingEntries || isLoading) && (
                                    <Skeleton
                                        variant="rectangular"
                                        width="100%"
                                        height="60vh"
                                        sx={{ borderRadius: 2, mt: 2 }}
                                    />
                                )}
                                {isLoading && (
                                    <LoadingComponent
                                        message={getTranslatedLabel(`${localizationKey}.processing`, "Processing Transactions...")}
                                    />
                                )}
                            </FormElement>
                        );
                    }}
                />
            </Paper>
        </>
    );
}