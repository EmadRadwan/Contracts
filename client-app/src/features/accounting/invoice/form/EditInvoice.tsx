import React, {useEffect, useRef, useState} from "react";
import {Form, FormElement, Field} from "@progress/kendo-react-form";
import {Button, Grid, Paper, Typography} from "@mui/material";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import {requiredValidator} from "../../../../app/common/form/Validators";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {useNavigate, useParams} from "react-router-dom";
import {
    useAppDispatch,
    useFetchBillingAccountsQuery,
    useFetchCompaniesQuery,
    useFetchCompanyBaseCurrencyQuery,
    useFetchCurrenciesQuery,
    useFetchInvoiceByIdQuery,
    useFetchInvoiceTypesQuery,
    useFetchRoleTypesQuery,
    useUpdateInvoiceMutation
} from "../../../../app/store/configureStore";
import AccountingMenu from "../menu/AccountingMenu";
import {FormComboBoxVirtualCustomer} from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import {FormComboBoxVirtualParty} from "../../../../app/common/form/FormComboBoxVirtualParty";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import FormInput from "../../../../app/common/form/FormInput";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {handleDatesObject} from "../../../../app/util/utils";
import {toast} from "react-toastify";

const EditInvoice = () => {
    const {invoiceId} = useParams<{ invoiceId: string }>();
    const formRef = useRef<Form | null>(null);
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.invoices.form";
    const {data: invoice, isLoading: isLoadingInvoice} = useFetchInvoiceByIdQuery(invoiceId!, {skip: !invoiceId});
    const [adjustedInvoice, setAdjustedInvoice] = useState<any | null>(null);
    const {data: baseCurrency, isLoading: isBaseCurrencyLoading} = useFetchCompanyBaseCurrencyQuery(undefined);

    const {data: invoiceTypes} = useFetchInvoiceTypesQuery(undefined);
    const {data: companies} = useFetchCompaniesQuery(undefined);
    const {data: currencies} = useFetchCurrenciesQuery(undefined);
    const {data: billingAccounts} = useFetchBillingAccountsQuery(undefined);
    const {data: roleTypes} = useFetchRoleTypesQuery(undefined);
    const [updateInvoice, { isLoading: isUpdatingInvoice }] = useUpdateInvoiceMutation();

    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    useEffect(() => {
        console.log("useEffect run", { invoice, companies, baseCurrency });

        if (invoice && companies) {
            // Purpose: Ensures invoiceDate and dueDate are valid Date objects for Kendo FormDatePicker
            // Improvement: Adds validation to handle invalid date strings and prevent getTime errors
            const companyFrom = companies?.find((c) => c.organizationPartyId === invoice.partyIdFrom?.fromPartyId);
            const companyTo = companies?.find((c) => c.organizationPartyId === invoice.partyId?.fromPartyId);

            const parsedInvoice = handleDatesObject(invoice);
            const updatedInvoice = {
                ...parsedInvoice,
                currencyUomId: invoice.currencyUomId || baseCurrency?.currencyUomId || "EGP",
                partyIdFrom: invoice.partyIdFrom, // Keep as object for dropdown
                partyId: invoice.partyId, // Keep as object for dropdown
                partyIdFromName: companyFrom?.organizationPartyName || invoice.partyIdFrom?.fromPartyName || invoice.partyIdFrom?.fromPartyId || "Unknown",
                partyIdName: companyTo?.organizationPartyName || invoice.partyId?.fromPartyName || invoice.partyId?.fromPartyId || "Unknown",
                invoiceDate: parsedInvoice.invoiceDate ? new Date(parsedInvoice.invoiceDate) : null,
                dueDate: parsedInvoice.dueDate ? new Date(parsedInvoice.dueDate) : null,
                paidDate: parsedInvoice.paidDate ? new Date(parsedInvoice.paidDate) : null,
            };

            // Validate Date objects to prevent invalid values
            if (updatedInvoice.invoiceDate && isNaN(updatedInvoice.invoiceDate.getTime())) {
                console.warn("Invalid invoiceDate, setting to null");
                updatedInvoice.invoiceDate = null;
            }
            if (updatedInvoice.dueDate && isNaN(updatedInvoice.dueDate.getTime())) {
                console.warn("Invalid dueDate, setting to null");
                updatedInvoice.dueDate = null;
            }
            if (updatedInvoice.paidDate && isNaN(updatedInvoice.paidDate.getTime())) {
                console.warn("Invalid paidDate, setting to null");
                updatedInvoice.paidDate = null;
            }

            setAdjustedInvoice(updatedInvoice);
        } else if (baseCurrency?.currencyUomId) {
            setAdjustedInvoice({
                currencyUomId: baseCurrency.currencyUomId,
                // REFACTOR: Set default invoiceDate to July 1, 2025
                // Purpose: Provides a default Date object for FormDatePicker when invoice is not available
                // Improvement: Ensures consistent default state for form
                invoiceDate: new Date(2025, 6, 1),
            });
        }
    }, [invoice, baseCurrency, companies]);


    const isSalesInvoice = adjustedInvoice && (
        adjustedInvoice.invoiceTypeId === "SALES_INVOICE" ||
        invoiceTypes?.find((type: any) => type.invoiceTypeId === adjustedInvoice.invoiceTypeId)?.parentTypeId === "SALES_INVOICE"
    );
    const isPurchaseInvoice = adjustedInvoice && (
        adjustedInvoice.invoiceTypeId === "PURCHASE_INVOICE" ||
        invoiceTypes?.find((type: any) => type.invoiceTypeId === adjustedInvoice.invoiceTypeId)?.parentTypeId === "PURCHASE_INVOICE"
    );

    const isGenericInvoice = adjustedInvoice && adjustedInvoice.invoiceTypeId === "PURC_RTN_INVOICE";



    // IMPLEMENTATION: Filter role types based on invoice type
    const customerRoleTypes = roleTypes?.filter((role: any) => role.parentTypeId === "CUSTOMER") || [];
    const vendorRoleTypes = roleTypes?.filter((role: any) => role.parentTypeId === "VENDOR") || [];

    const handleSubmit = async (values: any) => {
        try {
            // Handles dropdown objects while aligning with backend expectations.
            if ((adjustedInvoice?.invoiceTypeId === "PURC_RTN_INVOICE" || isGenericInvoice) &&
                values.partyId?.fromPartyId === values.partyIdFrom?.fromPartyId) {
                throw new Error("partyId and partyIdFrom must be distinct for this invoice type.");
            }
            const formattedValues = {
                ...values,
                partyId: values.partyId?.fromPartyId || values.partyId,
                partyIdFrom: values.partyIdFrom?.fromPartyId || values.partyIdFrom,
            };
            const response = await updateInvoice({invoiceId, ...formattedValues}).unwrap();
            //dispatch(setSelectedInvoice(response));
        } catch (e) {
            console.error("Error updating invoice:", e);
            toast.error(getTranslatedLabel(`${localizationKey}.error`, "Error updating invoice"));
        }
    };


    const handleEditItems = () => {
        navigate(`/invoices/${invoiceId}/items`)
    };

    // IMPLEMENTATION: Check if invoice is editable
    // Purpose: Mimics OFBiz's condition for INVOICE_IN_PROCESS and permissions
    const isEditable = adjustedInvoice?.statusId === "INVOICE_IN_PROCESS";

    if (isLoadingInvoice  || isBaseCurrencyLoading) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, "Loading Invoice...")}/>;
    }

    if (isUpdatingInvoice) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.updating`, "Updating Invoice...")} />;
    }

    if (!adjustedInvoice || !isEditable) {
        return (
            ''
        );
    }

    const invoiceTypeDescription = invoiceTypes?.find((type) => type.invoiceTypeId === adjustedInvoice.invoiceTypeId)?.description || adjustedInvoice.invoiceTypeId;

    console.log('Adjusted Invoice:', adjustedInvoice);
    return (
        <>
            <AccountingMenu selectedMenuItem="/invoices"/>
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Typography variant="h4" sx={{p: 2, color: "green"}}>
                    {getTranslatedLabel(`${localizationKey}.edit`, "Edit Invoice")}
                    {invoiceId && (
                        <span style={{color: "blue", fontWeight: "bold"}}>
                            {" "}{invoiceId}
                        </span>
                    )}
                </Typography>
                <Form
                    onSubmit={handleSubmit}
                    ref={formRef}
                    key={JSON.stringify(adjustedInvoice)}
                    initialValues={
                        adjustedInvoice || {
                            currencyUomId: baseCurrency?.currencyUomId || "EGP",
                            invoiceDate: new Date(2025, 6, 1), // July 1, 2025
                        }
                    } render={(formRenderProps) => (
                    <FormElement>
                        <Grid container spacing={2} direction="row" wrap="wrap">
                            {/* Row 1: invoiceDate and dueDate */}
                            <Grid item xs={6}>
                                <Grid container spacing={1} alignItems="center">
                                    <Grid item xs={8}>
                                        <Field
                                            name="invoiceDate"
                                            id="invoiceDate"
                                            label={getTranslatedLabel(`${localizationKey}.invoiceDate`, "Invoice Date")}
                                            component={FormDatePicker}
                                            format="dd/MM/yyyy"
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>
                            <Grid item xs={6}>
                                <Grid container spacing={1} alignItems="center">
                                    <Grid item xs={8}>
                                        <Field
                                            name="dueDate"
                                            id="dueDate"
                                            label={getTranslatedLabel(`${localizationKey}.dueDate`, "Due Date")}
                                            component={FormDatePicker}
                                            format="dd/MM/yyyy"
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>
                            {/* Row 2: invoiceTypeId and statusId */}
                            <Grid item xs={6}
                                  sx={{minHeight: "40px" /* REFACTOR: Ensure equal row height for alignment */}}>
                                <Grid container spacing={1} alignItems="center">
                                    <Grid item xs={4}>
                                        <Typography
                                            variant="body1"
                                            sx={{
                                                fontWeight: "bold",
                                                lineHeight: "1.5",
                                                margin: 0,
                                                padding: 0,
                                            }}
                                        >
                                            {getTranslatedLabel(`${localizationKey}.invoiceType`, "Invoice Type")}
                                        </Typography>
                                    </Grid>
                                    <Typography
                                        variant="body1"
                                        sx={{
                                            fontWeight: "bold",
                                            color: "blue",
                                            lineHeight: "1.5",
                                            margin: 0,
                                            padding: 0,
                                        }}
                                    >
                                        {invoiceTypeDescription}
                                    </Typography>
                                </Grid>
                            </Grid>
                            <Grid item xs={6}
                                  sx={{minHeight: "40px" /* REFACTOR: Ensure equal row height for alignment */}}>
                                <Grid container spacing={1} alignItems="center">
                                    <Grid item xs={4}>
                                        <Typography
                                            variant="body1"
                                            sx={{
                                                fontWeight: "bold",
                                                lineHeight: "1.5",
                                                margin: 0,
                                                padding: 0,
                                            }}
                                        >
                                            {getTranslatedLabel(`${localizationKey}.status`, "Status")}
                                        </Typography>
                                    </Grid>
                                    <Grid item xs={8}>
                                        <Typography
                                            variant="body1"
                                            sx={{
                                                fontWeight: "bold",
                                                color: "blue",
                                                lineHeight: "1.5",
                                                margin: 0,
                                                padding: 0,
                                            }}
                                        >
                                            {adjustedInvoice.statusDescription || adjustedInvoice.statusId}
                                        </Typography>
                                    </Grid>
                                </Grid>
                            </Grid>

                            {/* Row 3: description */}
                            <Grid item xs={12}>
                                <Grid container spacing={1} alignItems="center">

                                    <Grid item xs={10}>
                                        <Field
                                            name="description"
                                            label={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                            id="description"
                                            component={FormInput}
                                            maxLength={100}
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>
                            {/* Row 4: partyIdFrom and partyId */}
                            <Grid container spacing={1} alignItems="flex-end">
                                <Grid item xs={3}>
                                    {isSalesInvoice ? (
                                        // REFACTOR: Use partyIdFromName for display, ensuring string output.
                                        // Fixes error by avoiding object rendering.
                                        <Typography variant="body1" sx={{fontWeight: "bold", color: "blue"}}>
                                            {adjustedInvoice.partyIdFromName}
                                        </Typography>
                                    ) : (
                                        <Field
                                            name="partyIdFrom"
                                            id="partyIdFrom"
                                            label={getTranslatedLabel(`${localizationKey}.partyIdFrom`, "From Party ID")}
                                            component={FormComboBoxVirtualParty}
                                            data={vendorRoleTypes}
                                            validator={requiredValidator}
                                        />
                                    )}
                                </Grid>
                                <Grid item xs={3}>
                                    {isPurchaseInvoice ? (
                                        // REFACTOR: Use partyIdName for display, ensuring string output.
                                        // Fixes error by avoiding object rendering.
                                        <Typography variant="body1" sx={{fontWeight: "bold", color: "blue"}}>
                                            {adjustedInvoice.partyIdName}
                                        </Typography>
                                    ) : (
                                        <Field
                                            name="partyId"
                                            id="partyIdTo"
                                            label={getTranslatedLabel(`${localizationKey}.partyIdTo`, "To Party ID")}
                                            component={
                                                adjustedInvoice?.invoiceTypeId === "PURC_RTN_INVOICE"
                                                    ? FormComboBoxVirtualParty // REFACTOR: Handle PURC_RTN_INVOICE within sales invoice logic.
                                                    : FormComboBoxVirtualCustomer
                                            }
                                            data={
                                                adjustedInvoice?.invoiceTypeId === "PURC_RTN_INVOICE"
                                                    ? vendorRoleTypes // REFACTOR: Use vendor roles for PURC_RTN_INVOICE.
                                                    : customerRoleTypes
                                            }
                                            validator={requiredValidator}
                                        />
                                    )}
                                </Grid>
                            </Grid>
                            {/* Row 5: partyId */}


                            {/* Row 6: billingAccountId */}

                            {/* Row 7: currencyUomId */}
                            <Grid item xs={12}>
                                <Grid container spacing={1} alignItems="center">
                                    <Grid item xs={10}>
                                        <Field
                                            name="currencyUomId"
                                            id="currencyUomId"
                                            label={getTranslatedLabel(`${localizationKey}.currency`, "Currency")}
                                            component={MemoizedFormDropDownList}
                                            data={currencies || []}
                                            dataItemKey="currencyUomId"
                                            textField="description"
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>
                            {/* Row 8: referenceNumber */}
                            <Grid item xs={12}>
                                <Grid container spacing={1} alignItems="center">
                                    <Grid item xs={10}>
                                        <Field
                                            name="referenceNumber"
                                            id="referenceNumber"
                                            label={getTranslatedLabel(`${localizationKey}.referenceNumber`, "Reference Number")}
                                            component={FormInput}
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>
                            {/* Row 9: Buttons */}
                            {isEditable && (
                                <Grid item xs={12}>
                                    <Grid container spacing={1}>
                                        <Grid item>
                                            <Button
                                                variant="contained"
                                                type="submit"
                                                color="success"
                                                disabled={!formRenderProps.allowSubmit}
                                            >
                                                {getTranslatedLabel(`${localizationKey}.update`, "Update")}
                                            </Button>
                                        </Grid>
                                        <Grid item>
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                onClick={handleEditItems}
                                            >
                                                {getTranslatedLabel(`${localizationKey}.editItems`, "Edit Items")}
                                            </Button>
                                        </Grid>
                                        <Grid item>
                                            <Button
                                                variant="contained"
                                                color="error"
                                                onClick={() => navigate("/invoices")}
                                            >
                                                {getTranslatedLabel(`${localizationKey}.back`, "Back")}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </Grid>
                            )}
                            <Grid item xs={3}>
                                <Field
                                    name="invoiceId"
                                    component="input"
                                    type="hidden"
                                    value={invoiceId}
                                />
                            </Grid>
                        </Grid>
                    </FormElement>
                )}
                />
            </Paper>
        </>
    );
};

export default EditInvoice;