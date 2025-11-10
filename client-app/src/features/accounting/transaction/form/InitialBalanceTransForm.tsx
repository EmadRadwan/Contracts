import React, { useState, useCallback, useMemo } from "react";
import { Grid, Paper, Typography, Button, Chip } from "@mui/material";
import { Form, FormElement, Field } from "@progress/kendo-react-form";
import { requiredValidator } from "../../../../app/common/form/Validators";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { FormDropDownTreeGlAccount2 } from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import FormInput from "../../../../app/common/form/FormInput";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import useInitialBalanceTrans from "../hook/useInitialBalanceTrans";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { useAppSelector } from "../../../../app/store/configureStore";
import { useFetchGlAccountOrganizationHierarchyLovQuery } from "../../../../app/store/apis";
import {FormDropDownList} from "../../../../app/common/form/FormDropDownList";

interface FormValues {
    glAccountId?: string;
    amount: number | null;
    description?: string;
    debitCreditFlag: "D" | "C";
}

export default function InitialBalanceTransForm() {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.orgGL.accounting.initialBalance";
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";
    const companyName = useAppSelector((state) => state.accountingSharedUi.selectedAccountingCompanyName);

    const { data: glAccounts, isLoading: isLoadingGlAccounts } =
        useFetchGlAccountOrganizationHierarchyLovQuery(companyId, { skip: !companyId });

    const { isLoading, saveInitialBalanceTrans, postTransaction } = useInitialBalanceTrans();

    // REFACTOR: Header fields persist across form resets
    const [headerValues, setHeaderValues] = useState({
        transactionDate: new Date(),
        headerDescription: "الرصيد الافتتاحي",
    });

    // REFACTOR: Track saved transaction
    const [transactionId, setTransactionId] = useState<string | null>(null);
    const [justPosted, setJustPosted] = useState(false);

    // REFACTOR: Form reset key
    const [formResetCounter, setFormResetCounter] = useState(0);

    const initialFormValues: FormValues = useMemo(
        () => ({
            glAccountId: undefined,
            amount: null,
            description: "الرصيد الافتتاحي",
            debitCreditFlag: "D",
        }),
        []
    );

    const handleSubmit = useCallback(
        async (data: any) => {
            if (!data.isValid) return;

            const {
                glAccountId,
                amount,
                description,
                debitCreditFlag,   // <-- this is now the selected data item
            } = data.values;

            try {
                const result = await saveInitialBalanceTrans({
                    CreateInitialBalanceTransParams: {
                        AcctgTransTypeId: "INITIAL_BALANCE",
                        TransactionDate: headerValues.transactionDate,
                        OrganizationPartyId: companyId,
                        HeaderDescription: headerValues.headerDescription,
                        GlFiscalTypeId: "ACTUAL",
                        IsPosted: "N",
                    },
                    Entry: {
                        glAccountId: glAccountId!,
                        amount: amount!,
                        description: description || "",
                        // REFACTOR: Extract the primitive value
                        debitCreditFlag: debitCreditFlag?.value ?? debitCreditFlag,
                    },
                });

                setTransactionId(result.acctgTransId);
                toast.success(getTranslatedLabel("general.success", "Initial balance saved"));
                setFormResetCounter((prev) => prev + 1);
            } catch {
                // error already toasted in hook
            }
        },
        [headerValues, companyId, saveInitialBalanceTrans, getTranslatedLabel]
    );

    // REFACTOR: Post transaction
    const handlePost = useCallback(async () => {
        if (!transactionId) return;

        try {
            const messages = await postTransaction(transactionId);
            if (Array.isArray(messages) && messages.length === 0) {
                toast.success("Initial Balance Posted Successfully");
                setJustPosted(true);
            } else {
                messages.forEach((msg: string) =>
                    msg.includes("Error") ? toast.error(msg) : toast.warn(msg)
                );
            }
        } catch {
            // Handled in hook
        }
    }, [transactionId, postTransaction]);

    // REFACTOR: New transaction reset
    const handleNew = useCallback(() => {
        setTransactionId(null);
        setJustPosted(false);
        setHeaderValues({ transactionDate: new Date(), headerDescription: "" });
        setFormResetCounter((prev) => prev + 1);
    }, []);

    return (
        <>
            <AccountingMenu selectedMenuItem="orgGl" />
            <Paper elevation={5} sx={{ p: 2, borderRadius: 2 }}>
                <Typography variant="h5" sx={{ mb: 2 }}>
                    {getTranslatedLabel(`${localizationKey}.title`, "Initial Balance Entry")}
                    {transactionId && (
                        <span style={{ marginLeft: 8, color: "#1976d2", fontWeight: 600 }}>
              #{transactionId}
            </span>
                    )}
                    {justPosted && (
                        <Chip label="Posted" color="success" size="small" sx={{ ml: 2 }} />
                    )}
                </Typography>

                {/* Header Fields */}
                <Grid container spacing={2} sx={{ mb: 3 }}>
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
                        />
                    </Grid>
                    <Grid item xs={9}>
                        <FormInput
                            id="headerDescription"
                            label={getTranslatedLabel(`${localizationKey}.headerDescription`, "Header Description")}
                            value={headerValues.headerDescription}
                            onChange={(e) =>
                                setHeaderValues((prev) => ({ ...prev, headerDescription: e.value }))
                            }
                        />
                    </Grid>
                </Grid>

                <Form
                    initialValues={initialFormValues}
                    key={formResetCounter}
                    onSubmitClick={handleSubmit}
                    render={(formRenderProps) => (
                        <FormElement>
                            <Grid container spacing={2} alignItems="flex-end">
                                <Grid item xs={5}>
                                    <Field
                                        id="glAccountId"
                                        name="glAccountId"
                                        label={getTranslatedLabel(`${localizationKey}.glAccount`, "GL Account *")}
                                        data={glAccounts || []}
                                        component={FormDropDownTreeGlAccount2}
                                        dataItemKey="glAccountId"
                                        textField="text"
                                        selectField="selected"
                                        expandField="expanded"
                                        validator={requiredValidator}
                                        disabled={isLoadingGlAccounts}
                                    />
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
                                <Grid item xs={2}>
                                    <Field
                                        id="debitCreditFlag"
                                        name="debitCreditFlag"
                                        label={getTranslatedLabel(`${localizationKey}.type`, "Type")}
                                        component={FormDropDownList}
                                        data={[
                                            { text: "Debit", value: "D" },
                                            { text: "Credit", value: "C" },
                                        ]}
                                        textField="text"
                                        dataItemKey="value"
                                    />
                                </Grid>
                                <Grid item xs={2}>
                                    <Field
                                        id="description"
                                        name="description"
                                        label={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                        component={FormInput}
                                    />
                                </Grid>
                                <Grid item xs={1}>
                                    <Button
                                        variant="contained"
                                        color="success"
                                        type="submit"
                                        disabled={!formRenderProps.allowSubmit || isLoading}
                                    >
                                        {getTranslatedLabel("general.save", "Save")}
                                    </Button>
                                </Grid>
                            </Grid>

                            <Grid container spacing={2} sx={{ mt: 2 }}>
                                <Grid item>
                                    <Button
                                        variant="contained"
                                        color="secondary"
                                        onClick={handleNew}
                                        disabled={isLoading}
                                    >
                                        {getTranslatedLabel("general.new", "New")}
                                    </Button>
                                </Grid>
                                {transactionId && (
                                    <Grid item>
                                        <Button
                                            variant="contained"
                                            color="info"
                                            onClick={handlePost}
                                            disabled={isLoading || justPosted}
                                        >
                                            {getTranslatedLabel("general.post", "Post")}
                                        </Button>
                                    </Grid>
                                )}
                            </Grid>

                            {isLoading && (
                                <LoadingComponent message="Saving initial balance..." />
                            )}
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}