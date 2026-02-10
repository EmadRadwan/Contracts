import React, {useState, useCallback, useMemo, useEffect} from "react";
import {Grid, Paper, Typography, Button, Skeleton, TextField, Chip, Autocomplete} from "@mui/material";
import {Form, FormElement, Field} from "@progress/kendo-react-form";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridSortChangeEvent,
    GridPageChangeEvent,
    GridRowProps,
    GridCellProps,
} from "@progress/kendo-react-grid";
import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import {RootState, useAppSelector, useFetchAcctgTransTypesQuery} from "../../../../app/store/configureStore";
import {useFetchGlAccountOrganizationHierarchyLovQuery} from "../../../../app/store/apis";
import {requiredValidator} from "../../../../app/common/form/Validators";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import {FormDropDownTreeGlAccount2} from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import FormInput from "../../../../app/common/form/FormInput";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {toast} from "react-toastify";
import useMultiAcctgTrans from "../hook/useMultiAcctgTrans";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {FormComboBoxVirtualParty} from "../../../../app/common/form/FormComboBoxVirtualParty";
import useDuplicateAcctgTrans from "../hook/useDuplicateAcctgTrans";

interface TransEntry {
    id: string;
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

export default function MultiAcctgTransEntryForm() {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.orgGL.accounting.trans.multi";
    const {user} = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";
    const companyName = useAppSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
    const {
        data: glAccounts,
        isLoading: isLoadingGlAccounts
    } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, {
        skip: !companyId,
    });
    console.log("glAccounts", glAccounts);
    const [transEntries, setTransEntries] = useState<TransEntry[]>([]);
    const [formResetCounter, setFormResetCounter] = useState(0);
    const [sort, setSort] = useState<SortDescriptor[]>([{field: "id", dir: "asc"}]);
    const [page, setPage] = useState<State>({skip: 0, take: 10});
    const {isLoading, saveMultiAcctgTransWithEntries, postTransaction} = useMultiAcctgTrans();
    const [justPosted, setJustPosted] = useState(false);
    const { data: acctgTransTypes, isLoading: isLoadingTransTypes } = useFetchAcctgTransTypesQuery(undefined);
    const { duplicate, isDuplicating } = useDuplicateAcctgTrans();
    // REFACTOR: Manage header-level fields outside the form to persist across resets
    const [headerValues, setHeaderValues] = useState({
        transactionDate: new Date(),
        headerDescription: "",
        party: null as { fromPartyId: string; fromPartyName: string } | null,
        acctgTransTypeId: null as string | null, // ← NEW
    });

    console.log("transEntries", transEntries);


    // REFACTOR: Add state for transactionId to display after save
    const [transactionId, setTransactionId] = useState<string | null>(null);
    const [selectedEntryId, setSelectedEntryId] = useState<string | null>(null);
    let formRenderProps: any;


    // REFACTOR: Define initialFormValues for entry-level fields only
    const initialFormValues: FormValues = useMemo(
        () => ({
            debitGlAccountId: undefined,
            creditGlAccountId: undefined,
            amount: null,
            description: "",
        }),
        []
    );

    // REFACTOR: Handle adding entries, resetting only entry-level fields
    const handleAddOrUpdateEntry = useCallback(
        async (data: any) => {
            if (!data.isValid) return;
            const {debitGlAccountId, creditGlAccountId, amount, description} = data.values;
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
                        ? [{
                            id: `D-${Date.now()}`,
                            debitGlAccountId,
                            amount,
                            description,
                            debitCreditFlag: "D" as const
                        }]
                        : []),
                    ...(creditGlAccountId
                        ? [{
                            id: `C-${Date.now() + 1}`,
                            creditGlAccountId,
                            amount,
                            description,
                            debitCreditFlag: "C" as const
                        }]
                        : []),
                ];
                setTransEntries((prev) => [...prev, ...newEntries]);
            }
            setFormResetCounter((prev) => prev + 1); // Reset entry-level fields
        },
        [selectedEntryId]
    );

    useEffect(() => {
        setJustPosted(false);
    }, [transactionId]);

    // REFACTOR: Handle editing an entry by populating the form with its data
    // Purpose: Allow users to click a GL Account cell to edit the corresponding entry
    // Improvement: Enhances UX by enabling direct editing from the grid
    const handleEditEntry = useCallback(
        (entryId: string) => {
            const entry = transEntries.find((e) => e.id === entryId);
            if (entry) {
                setSelectedEntryId(entryId);
                // Update form values with selected entry data
                const form = document.getElementById("multiAcctgTransEntryForm") as HTMLFormElement;
                if (form && formRenderProps) {
                    formRenderProps.onChange("debitGlAccountId", {value: entry.debitGlAccountId});
                    formRenderProps.onChange("creditGlAccountId", {value: entry.creditGlAccountId});
                    formRenderProps.onChange("amount", {value: entry.amount});
                    formRenderProps.onChange("description", {value: entry.description});
                }
            }
        },
        [transEntries]
    );
    // REFACTOR: Handle removing entries from the local grid
    const handleRemoveEntry = useCallback((entryId: string) => {
        setTransEntries((prev) => prev.filter((entry) => entry.id !== entryId));
    }, []);

    // REFACTOR: Handle saving transaction, using headerValues and storing transactionId
    const handleSaveTransaction = useCallback(
        async () => {
            if (transEntries.length === 0) {
                toast.error(getTranslatedLabel("general.error", "No entries to save"));
                return;
            }
            try {
                // REFACTOR: Capture transactionId from API response
                const result = await saveMultiAcctgTransWithEntries({
                    CreateMultiAcctgTransParams: {
                        AcctgTransTypeId: headerValues.acctgTransTypeId || "_NA_",
                        TransactionDate: headerValues.transactionDate,
                        OrganizationPartyId: companyId,
                        HeaderDescription: headerValues.headerDescription,
                        Description: transEntries[0]?.description || "",
                        IsPosted: "N",
                        GlFiscalTypeId: "ACTUAL",
                        partyId: headerValues.party?.fromPartyId || undefined,
                    },
                    Entries: transEntries.map((entry) => ({
                        debitGlAccountId: entry.debitGlAccountId,
                        creditGlAccountId: entry.creditGlAccountId,
                        amount: entry.amount,
                        description: entry.description,
                        debitCreditFlag: entry.debitCreditFlag,
                    })),
                });
                // REFACTOR: Set transactionId and keep form data intact
                setTransactionId(result.acctgTransId);
                toast.success(getTranslatedLabel("general.success", "Transaction saved successfully"));
            } catch (error) {
                toast.error(getTranslatedLabel("general.error", "Failed to save transaction"));
            }
        },
        [transEntries, companyId, saveMultiAcctgTransWithEntries, getTranslatedLabel, headerValues]
    );

    // REFACTOR: Add handler for New Transaction button to clear all data
    const handleNewTransaction = useCallback(() => {
        setTransEntries([]);
        setFormResetCounter((prev) => prev + 1);
        setHeaderValues({
            transactionDate: new Date(),
            headerDescription: "",
            partyId: "" ,
            acctgTransTypeId: null
        });
        setTransactionId(null);
        setJustPosted(false);
    }, []);

    // REFACTOR: Handle page change for grid pagination
    const pageChange = useCallback((event: GridPageChangeEvent) => {
        setPage(event.page);
    }, []);

    // REFACTOR: Handle sort change for grid
    const handleSortChange = useCallback((event: GridSortChangeEvent) => {
        setSort(event.sort);
    }, []);

    // REFACTOR: Render row with conditional styling
    const rowRender = useCallback(
        (trElement: React.ReactElement<HTMLTableRowElement>, props: GridRowProps) => {
            const isDebit = props.dataItem.debitCreditFlag === "D";
            const style = {backgroundColor: isDebit ? "rgba(55, 180, 0, 0.32)" : "#ffffff"};
            return React.cloneElement(trElement, {style}, trElement.props.children);
        },
        []
    );

    // Add this near the top of your component
    const accountMap = useMemo(() => {
        const map = new Map<string, any>();

        const traverse = (nodes: any[]) => {
            for (const node of nodes) {
                map.set(node.glAccountId, node);
                if (node.items?.length) {
                    traverse(node.items);
                }
            }
        };

        if (glAccounts) {
            traverse(glAccounts);
        }

        return map;
    }, [glAccounts]);


// Then in your GlAccountCell:
    const GlAccountCell = ({ dataItem }: GridCellProps) => {
        const glAccountId = dataItem.debitGlAccountId || dataItem.creditGlAccountId;

        const glAccount = accountMap.get(glAccountId);

        const displayText = glAccount?.text
            || glAccount?.accountName
            || glAccountId
            || "-";

        return (
            <td
                style={{ cursor: "pointer", color: "#1976d2" }}
                onClick={() => handleEditEntry(dataItem.id)}
            >
                {displayText}
            </td>
        );
    };

    // REFACTOR: Custom cell for GL Account ID
   /* const GlAccountCell = useCallback(
        ({dataItem}: GridCellProps) => {
            const glAccountId = dataItem.debitGlAccountId || dataItem.creditGlAccountId;
            const glAccount = glAccounts?.find((acc) => acc.glAccountId === glAccountId);
            console.log('glAccount?.text', glAccount?.text);
            return (
                <td style={{cursor: "pointer", color: "#1976d2"}} onClick={() => handleEditEntry(dataItem.id)}>
                    {glAccount?.text || glAccountId || "-"}
                </td>
            );
        },
        [glAccounts, handleEditEntry]
    );*/
    // REFACTOR: Custom cell for remove action
    const RemoveCell = useCallback(
        ({dataItem}: GridCellProps) => (
            <td>
                <Button variant="text" color="error" onClick={() => handleRemoveEntry(dataItem.id)}
                        disabled={isLoading}>
                    {getTranslatedLabel("general.remove", "Remove")}
                </Button>
            </td>
        ),
        [handleRemoveEntry, getTranslatedLabel, isLoading]
    );

    // REFACTOR: Calculate totals for footer
    const totalDebit = useMemo(
        () => transEntries.filter((e) => e.debitCreditFlag === "D").reduce((sum, e) => sum + (e.amount || 0), 0),
        [transEntries]
    );
    const totalCredit = useMemo(
        () => transEntries.filter((e) => e.debitCreditFlag === "C").reduce((sum, e) => sum + (e.amount || 0), 0),
        [transEntries]
    );

    const handlePostTransaction = useCallback(async () => {
        if (!transactionId) return;

        try {
            const messages = await postTransaction(transactionId);

            // Success: empty array
            if (Array.isArray(messages) && messages.length === 0) {
                toast.success("Accounting Transaction Posted Successfully");
                setJustPosted(true);
            }
            // Warnings / Errors
            else if (Array.isArray(messages)) {
                messages.forEach((msg: string) => {
                    if (msg.includes("Error Journal")) toast.warn(msg);
                    else toast.error(msg);
                });
            }
        } catch {
            
        }
    }, [transactionId, postTransaction]);
    
    return (
        <>
            <AccountingMenu selectedMenuItem={"orgGl"}/>
            <Paper elevation={5} sx={{p: 2, borderRadius: 2}}>
                <Typography variant="h5" sx={{mb: 2}}>
                    {getTranslatedLabel(`${localizationKey}.title`, "Create Accounting Transaction")}
                    {transactionId && (
                        <span style={{marginLeft: 8, color: "#1976d2", fontWeight: 600}}>
                            #{transactionId}
                        </span>
                    )}
                    {justPosted && (
                        <Chip
                            label={getTranslatedLabel("general.posted", "Posted")}
                            color="success"
                            size="small"
                            sx={{ ml: 2 }}
                        />
                    )}
                </Typography>
                {/* REFACTOR: Move header-level fields outside the Form to prevent resetting, add transactionId */}
                <Grid container spacing={2} sx={{mb: 2}} alignItems={"flex-end"}>
                    <Grid item xs={3}>
                        <FormDatePicker
                            id="transactionDate"
                            label={getTranslatedLabel(`${localizationKey}.transactionDate`, "Transaction Date *")}
                            value={headerValues.transactionDate}
                            onChange={(e) => setHeaderValues((prev) => ({
                                ...prev,
                                transactionDate: e.value || new Date()
                            }))}
                            validator={requiredValidator}
                        />
                    </Grid>
                    <Grid item xs={2}>
                        {isLoadingTransTypes ? (
                            <Skeleton variant="rounded" height={56} sx={{ borderRadius: "4px" }} />
                        ) : (
                            <Autocomplete
                                options={acctgTransTypes || []}
                                getOptionLabel={(option) => option.description || ""}
                                value={acctgTransTypes?.find(t => t.acctgTransTypeId === headerValues.acctgTransTypeId) || null}
                                onChange={(event, newValue) => {
                                    setHeaderValues(prev => ({
                                        ...prev,
                                        acctgTransTypeId: newValue?.acctgTransTypeId || null
                                    }));
                                }}
                                isOptionEqualToValue={(option, value) =>
                                    option.acctgTransTypeId === value?.acctgTransTypeId
                                }
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label={getTranslatedLabel(`${localizationKey}.acctgTransType`, "Acctg Trans Type *")}
                                        required
                                        variant="outlined"
                                        size="small"
                                    />
                                )}
                                loading={isLoadingTransTypes}
                                fullWidth
                                disableClearable={false}
                            />
                        )}
                    </Grid>
                    <Grid item xs={3}>
                        <FormInput
                            id="headerDescription"
                            label={getTranslatedLabel(`${localizationKey}.headerDescription`, "Header Description")}
                            value={headerValues.headerDescription}
                            onChange={(e) => setHeaderValues((prev) => ({...prev, headerDescription: e.value}))}
                            autoComplete="off"
                        />
                    </Grid>
                    <Grid item xs={3}>
                        <FormComboBoxVirtualParty
                            id="partyId"
                            label={getTranslatedLabel(`${localizationKey}.party`, "Employee")}
                            value={headerValues.party}  
                            onChange={(e: any) => setHeaderValues((prev) => ({
                                ...prev,
                                party: e.value 
                            }))}
                            valueField="fromPartyId"
                            textField="fromPartyName"
                        />
                    </Grid>

                </Grid>
                <Form
                    initialValues={initialFormValues}
                    key={formResetCounter}
                    onSubmitClick={handleAddOrUpdateEntry}
                    render={(props) => {
                        formRenderProps = props; // Store form render props for updating form values
                        return (
                            <FormElement id="multiAcctgTransEntryForm">
                                <Grid container spacing={2} direction="column">
                                    {/* Middle Row: GL Accounts, Amount, Description, Add/Update Button */}
                                    <Grid item xs={12}>
                                        <Grid container spacing={2} alignItems="flex-end">
                                            <Grid item xs={3}>
                                                {isLoadingGlAccounts ? (
                                                    <Skeleton variant="rounded" height={40}
                                                              sx={{width: "100%", borderRadius: "4px"}}/>
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
                                                    />
                                                )}
                                            </Grid>
                                            <Grid item xs={3}>
                                                {isLoadingGlAccounts ? (
                                                    <Skeleton variant="rounded" height={40}
                                                              sx={{width: "100%", borderRadius: "4px"}}/>
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
                                                />
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="description"
                                                    name="description"
                                                    label={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                                    component={FormInput}
                                                    autoComplete="off"
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
                                                        isLoading
                                                    }
                                                >
                                                    {selectedEntryId
                                                        ? getTranslatedLabel("general.update", "Update Entry")
                                                        : getTranslatedLabel("general.add", "Add Entry")}
                                                </Button>
                                            </Grid>
                                            {transactionId && (
                                                <Grid item xs={2}>
                                                    <Button
                                                        variant="outlined"
                                                        color="secondary"
                                                        onClick={() => duplicate(transactionId)}
                                                        disabled={isDuplicating}
                                                    >
                                                        {isDuplicating ? "Duplicating..." : getTranslatedLabel("general.duplicate", "Duplicate")}
                                                    </Button>
                                                </Grid>
                                            )}
                                        </Grid>
                                    </Grid>
                                    {/* Bottom: Save Transaction and New Transaction Buttons, Kendo Grid */}
                                    <Grid item xs={12}>
                                        <Grid container spacing={2} alignItems="center">
                                            <Grid item xs={2}>
                                                {/* REFACTOR: Disable Save Transaction button unless debit and credit totals are balanced */}
                                                <Button
                                                    variant="contained"
                                                    color="primary"
                                                    onClick={handleSaveTransaction}
                                                    disabled={transEntries.length === 0 || totalDebit !== totalCredit || isLoading}
                                                >
                                                    {getTranslatedLabel("general.save", "Save Transaction")}
                                                </Button>
                                            </Grid>
                                            {/* REFACTOR: Add New Transaction button */}
                                            <Grid item xs={2}>
                                                <Button
                                                    variant="contained"
                                                    color="secondary"
                                                    onClick={handleNewTransaction}
                                                    disabled={isLoading}
                                                >
                                                    {getTranslatedLabel("general.newTransaction", "New Transaction")}
                                                </Button>
                                            </Grid>

                                            {transactionId && (
                                                <Grid item xs={3} container justifyContent="flex-end">
                                                    <Button
                                                        variant="contained"
                                                        color="info"
                                                        onClick={handlePostTransaction}
                                                        disabled={isLoading || justPosted}
                                                    >
                                                        {getTranslatedLabel("general.postTransaction", "Post Transaction")}
                                                    </Button>
                                                </Grid>
                                            )}
                                            
                                            <Grid item xs={12}>
                                                <KendoGrid
                                                    style={{height: "40vh"}}
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
                                                    <Column width={320}
                                                            title={""}
                                                            footerCell={() => (
                                                                <td style={{
                                                                    textAlign: "left",
                                                                    fontWeight: "bold",
                                                                    color: "#1565C0"
                                                                }}>
                                                                    {getTranslatedLabel(`${localizationKey}.totalDebit`, "Total Debit")}:{" "}
                                                                    {totalDebit.toFixed(2)} |{" "}
                                                                    {getTranslatedLabel(`${localizationKey}.totalCredit`, "Total Credit")}:{" "}
                                                                    {totalCredit.toFixed(2)}
                                                                </td>
                                                            )}
                                                    />
                                                    <Column
                                                        field="description"
                                                        title={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                                        width={400}
                                                    />
                                                    <Column
                                                        field="debitCreditFlag"
                                                        title={getTranslatedLabel(`${localizationKey}.debitCredit`, "Debit/Credit")}
                                                        width={130}
                                                    />
                                                    <Column title={""} width={100} cell={RemoveCell}/>

                                                </KendoGrid>
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                </Grid>
                                {isLoading && (
                                    <LoadingComponent
                                        message={getTranslatedLabel("accounting.orgGL.accounting.summary.loading", "Loading Accounting Transactions...")}
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