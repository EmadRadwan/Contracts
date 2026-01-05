import {chequeValidator, requiredValidator} from "../../../../app/common/form/Validators";
import {Field, Form, FormElement, FormRenderProps,} from "@progress/kendo-react-form";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import {Alert, Box, Button, Grid, Skeleton, Typography} from "@mui/material";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import {Payment} from "../../../../app/models/accounting/payment";
import FormInput from "../../../../app/common/form/FormInput";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import React, {useCallback, useEffect, useMemo, useState} from "react";
import {
    RootState,
    useAppSelector,
    useFetchPaymentAcctTransEntriesQuery,
    useGetCostCentersQuery
} from "../../../../app/store/configureStore";
import {
    useFetchGlAccountOrganizationHierarchyLovQuery,
    useFetchPaymentApplicationsForPaymentQuery,
    useLazyFetchBalancesForVendorAndProjectQuery, useLazyGetPaymentReportPdfQuery
} from "../../../../app/store/apis";
import {FormDropDownTreeGlAccount2} from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import {PaymentExcelTechnical} from "../report/PaymentExcelTechnical";
import {PaymentExcelParty} from "../report/PaymentExcelParty";
import {MemoizedFormComboBox2} from "../../../../app/common/form/FormComboBox2";
import {FormComboBoxVirtualProject} from "../../../../app/common/form/FormComboBoxVirtualProject";
import CreateCostCenterModal from "./CreateCostCenterModal";
import '@react-pdf-viewer/core/lib/styles/index.css';
import '@react-pdf-viewer/default-layout/lib/styles/index.css';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    IconButton,
    CircularProgress
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";

import { Worker, Viewer } from '@react-pdf-viewer/core';
import { defaultLayoutPlugin } from '@react-pdf-viewer/default-layout';
import { version as pdfjsVersion } from 'pdfjs-dist/package.json';

interface EditPaymentFormProps {
    onValidityChange?: (valid: boolean) => void;
    filteredPaymentTypes: any[];
    paymentMethods?: any[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    onUpdate: (data: { values: any; menuItem: string }) => void;
    payment: Payment;
    paymentType: number;
    formEditMode: number;
    currencies: any[];
    handleCancelForm: () => void;
    debugForm?: boolean;
}

const EditPaymentForm: React.FC<EditPaymentFormProps> = ({
                                                             filteredPaymentTypes,
                                                             paymentMethods,
                                                             getTranslatedLabel,
                                                             onUpdate,
                                                             payment,
                                                             paymentType,
                                                             currencies,
                                                             handleCancelForm, debugForm
                                                         }) => {
    const localizationKey = "accounting.payments.form";
    const CASH_PAYMENT_METHOD_ID = "CASH";
    const ADVANCE_TO_VENDOR_CONTRACTOR = "ADVANCE_TO_VENDOR_CONTRACTOR";
    const defaultLayoutPluginInstance = defaultLayoutPlugin();

    const [triggerBalanceFetch, {data: balanceData, isFetching: balanceLoading}] =
        useLazyFetchBalancesForVendorAndProjectQuery();

    const partyIdTo = payment?.partyIdTo ?? "";
    const projectIdFromPayment = payment?.projectId ?? "";
    const [currentProjectId, setCurrentProjectId] = useState<string>("");
    const [showCreateCostCenter, setShowCreateCostCenter] = useState(false);
    const [pdfBlobUrl, setPdfBlobUrl] = useState<string | null>(null);
    const [showPdfViewer, setShowPdfViewer] = useState(false);
    const [previewLoading, setPreviewLoading] = useState(false);

    const handlePreviewPdf = async () => {
        if (!payment?.paymentId) return;

        setPreviewLoading(true);
        try {
            const arrayBuffer = await triggerPdf(payment.paymentId).unwrap();
            const blob = new Blob([arrayBuffer], { type: "application/pdf" });
            const url = URL.createObjectURL(blob);

            setPdfBlobUrl(url);
            setShowPdfViewer(true);
        } catch (error) {
            console.error("Failed to load PDF:", error);
            alert("فشل تحميل بيان الدفعة.");
        } finally {
            setPreviewLoading(false);
        }
    };

    const handleClosePdfViewer = () => {
        setShowPdfViewer(false);
        if (pdfBlobUrl) {
            URL.revokeObjectURL(pdfBlobUrl);
            setPdfBlobUrl(null);
        }
    };

    const [triggerPdf, { isFetching: isPdfFetching }] = useLazyGetPaymentReportPdfQuery();

    useEffect(() => {
        if (payment?.projectId) {
            setCurrentProjectId(payment.projectId);
        }
    }, [payment?.projectId]);

    useEffect(() => {
        if (
            payment?.paymentTypeId === ADVANCE_TO_VENDOR_CONTRACTOR && // only for advance
            partyIdTo &&
            currentProjectId
        ) {
            triggerBalanceFetch(
                {partyId: partyIdTo, projectId: currentProjectId},
                false
            );
        }
    }, [payment?.paymentTypeId, partyIdTo, currentProjectId, triggerBalanceFetch]);


    const nonEditableStatuses = ['PMNT_RECEIVED', 'PMNT_SENT', 'PMNT_CONFIRMED' /*, 'PMNT_CANCELLED' */];
    const isFormDisabled = payment && nonEditableStatuses.includes(payment.statusId);
    const {user} = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";
    const companyName = useAppSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
    const {
        data: glAccounts,
        isLoading: isLoadingGlAccounts
    } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, {
        skip: !companyId,
    });

    const costCenterType = payment?.isDisbursement ? 'out' : 'in';

    const {
        data: paymentCostCenters = [],
        isLoading: loadingCostCenters
    } = useGetCostCentersQuery({type: costCenterType}, {
        skip: !payment,
        refetchOnMountOrArgChange: true,
    });


    const {
        data: paymentApplications = [],
        isLoading: isAppsLoading,
        isFetching: isAppsFetching,
    } = useFetchPaymentApplicationsForPaymentQuery(payment?.paymentId ?? "", {
        skip: !payment?.paymentId,
    });

    const {
        data: acctTransEntryData = [],
        isLoading: isTransLoading,
        isFetching: isTransFetching,
    } = useFetchPaymentAcctTransEntriesQuery(payment?.paymentId ?? "", {
        skip: !payment?.paymentId,
    });

    const excelApplications = useMemo(
        () =>
            paymentApplications.map((app: any) => ({
                invoiceId: app.invoiceId,
                toPaymentId: app.toPaymentId,
                billingAccountId: app.billingAccountId,
                taxAuthGeoId: app.taxAuthGeoId,
                amountApplied: app.amountApplied ?? 0,
            })),
        [paymentApplications]
    );

    const excelTransactions = useMemo(
        () =>
            acctTransEntryData.map((t: any) => ({
                acctgTransId: t.acctgTransId,
                acctgTransEntrySeqId: t.acctgTransEntrySeqId,
                glAccountId: t.glAccountId,
                glAccountName: t.glAccountTypeDescription || t.glAccountName || "",
                debitCreditFlag: t.debitCreditFlag,
                origAmount: t.origAmount ?? 0,
                currency: t.origCurrencyUomId || "",
                transactionDate: t.transactionDate,
            })),
        [acctTransEntryData]
    );

    const isExcelFetching = isAppsFetching || isTransFetching;


    const statusDesc = useMemo(() => ({
        'PMNT_NOT_PAID': 'Not Paid',
        'PMNT_RECEIVED': 'Received',
        'PMNT_SENT': 'Sent',
        'PMNT_CONFIRMED': 'Confirmed',
        'PMNT_CANCELLED': 'Cancelled',
    }[payment?.statusId] || payment?.statusId), [payment?.statusId]);


    const paymentTypeDesc = useMemo(() => {
        if (!payment?.paymentTypeId) return "";
        return (
            filteredPaymentTypes.find(
                (pt: any) => pt.paymentTypeId === payment.paymentTypeId
            )?.description ?? payment.paymentTypeId
        );
    }, [filteredPaymentTypes, payment?.paymentTypeId]);


    // Initialize form with payment values
    const initialValues = useMemo(() => {
        if (!payment) return {};
        const isCash = payment.paymentMethodId === CASH_PAYMENT_METHOD_ID;
        return {
            paymentId: payment.paymentId ?? undefined,
            paymentTypeId: payment.paymentTypeId ?? undefined,
            paymentMethodId: payment.paymentMethodId ?? undefined,
            statusId: payment.statusId ?? undefined,
            fromPartyId: payment.fromPartyId ?? undefined,
            partyIdFromName: payment.partyIdFromName ?? '',
            partyIdTo: payment.partyIdTo ?? undefined,
            partyIdToName: payment.partyIdToName ?? '',
            amount: payment.amount ?? undefined,
            currencyUomId: payment.currencyUomId ?? undefined,
            effectiveDate: payment.effectiveDate ? new Date(payment.effectiveDate) : undefined,
            comments: payment.comments ?? '',
            finAccountTransId: payment.finAccountTransId ?? undefined,
            overrideGlAccountId: payment.overrideGlAccountId ?? undefined,
            isDepositWithDrawPayment: payment.isDepositWithDrawPayment ?? undefined,
            finAcctTransTypeId: payment.finAcctTransTypeId ?? undefined,
            isDisbursement: payment.isDisbursement ?? undefined,
            actualCurrencyUomId: '',
            actualCurrencyAmount: undefined,
            chequeNumber: isCash ? '' : (payment.chequeNumber ?? ''),
            chequeDate: isCash ? null : (payment.chequeDate ? new Date(payment.chequeDate) : null),
            projectId: payment.projectId
                ? {
                    projectId: payment.projectId,
                    projectName: payment.projectName,
                }
                : null,
            costCenterId: payment.costCenterId || "",
            paymentRefNum: payment.paymentRefNum || "",
        };
    }, [payment]);
    
    console.log('initialValues from editPaymentForm', initialValues)

    if (!payment) {
        return (
            <Typography variant="h6" sx={{pl: 2}}>
                {getTranslatedLabel(
                    `${localizationKey}.noPayment`,
                    "No payment selected for editing."
                )}
            </Typography>
        );
    }

    console.log('payment from editPaymentForm', payment)

    // Handle form submission     
    const handleSubmit = (values: any) => {
        if (isFormDisabled) return;
        onUpdate({
            values,
            menuItem: "Update Payment",
        });
    };

    const amountValidator = (value: number, getter: any) => {
        if (!value || value <= 0) return "الرجاء إدخال مبلغ صحيح";

        const paymentTypeId = getter("paymentTypeId");
        if (paymentTypeId !== ADVANCE_TO_VENDOR_CONTRACTOR) return;

        if (!balanceData) return;

        if (balanceData.initialBalance === 0) {
            return "لا يمكن إنشاء دفعة مقدمة: لا يوجد سقف دفع مُعيَّن لهذا المورد على المشروع";
        }

        if (value > balanceData.remainingBalance) {
            return `المبلغ المُدخل (${value.toLocaleString("ar-EG")}) يتجاوز الرصيد المتاح (${balanceData.remainingBalance.toLocaleString("ar-EG")})`;
        }

        return;
    };

    const handleDownloadPdf = async () => {
        if (!payment?.paymentId) return;

        try {
            const arrayBuffer = await triggerPdf(payment.paymentId).unwrap();

            const blob = new Blob([arrayBuffer], { type: "application/pdf" });

            // Fixed: Use global URL instead of window.URL
            const url = URL.createObjectURL(blob);

            const link = document.createElement("a");
            link.href = url;
            link.download = `${payment.paymentId}_بيان_دفعة.pdf`;
            document.body.appendChild(link); // Needed for Firefox
            link.click();
            document.body.removeChild(link);

            // Cleanup
            URL.revokeObjectURL(url);
        } catch (error) {
            console.error("Failed to download PDF:", error);
            alert("فشل تحميل بيان الدفعة. تأكد من الاتصال بالإنترنت وحاول مرة أخرى.");
        }
    };



    return (
        <Grid container>
            <Form
                initialValues={initialValues}
                onSubmit={handleSubmit}
                key={payment.paymentId}
                render={(formRenderProps: FormRenderProps) => {
                    const {
                        valid,
                        validator, onSubmit,
                        errors,
                        touched,
                        visited, values,
                        valueGetter,
                        onChange,
                    } = formRenderProps;

                    const amount = valueGetter("amount") || 0;

                    const isAdvancePayment = payment?.paymentTypeId === ADVANCE_TO_VENDOR_CONTRACTOR;
                    const hasBillingAccountIssue =
                        isAdvancePayment &&
                        balanceData &&
                        (balanceData.initialBalance === 0 || amount > balanceData.remainingBalance);

                    const isSubmitDisabled = !valid || isFormDisabled || balanceLoading || hasBillingAccountIssue;

                    const handleProjectChange = (event: any) => {
                        const selectedProject = event.value;
                        const newProjectId = selectedProject?.projectId || "";

                        setCurrentProjectId(newProjectId);
                        onChange("projectId", {value: selectedProject});
                    };


                    // REFACTOR: Custom handler for payment method change
                    const handlePaymentMethodChange = (event: any) => {
                        const selectedMethodId = event.value;
                        const isCash = selectedMethodId === CASH_PAYMENT_METHOD_ID;

                        // Update the payment method
                        onChange('paymentMethodId', {value: selectedMethodId});

                        // Clear cheque fields if CASH is selected
                        if (isCash) {
                            onChange('chequeNumber', {value: ''});
                            onChange('chequeDate', {value: null});
                        }
                    };

                    // Remove safeValues completely — use valueGetter directly
                    const excelPaymentData = {
                        paymentId: payment.paymentId ?? "NEW",
                        paymentType: paymentTypeDesc,
                        fromParty: paymentType === 1 ? payment.partyIdFromName ?? "" : payment.partyIdToName ?? "",
                        toParty: paymentType === 1 ? payment.partyIdToName ?? "" : payment.partyIdFromName ?? "",

                        amount: valueGetter("amount") ?? payment.amount ?? 0,
                        currency: payment.currencyUomId ?? "",
                        effectiveDate: payment.effectiveDate ?? "",
                        status: statusDesc,

                        paymentMethod: paymentMethods?.find(m => m.paymentMethodId === valueGetter("paymentMethodId"))
                            ?.description ?? payment.paymentMethodId ?? "",

                        chequeNumber: valueGetter("paymentMethodId") === CASH_PAYMENT_METHOD_ID
                            ? ""
                            : (valueGetter("chequeNumber") ?? ""),

                        chequeDate: valueGetter("paymentMethodId") === CASH_PAYMENT_METHOD_ID
                            ? undefined
                            : valueGetter("chequeDate"),

                        costCenter: (() => {
                            const id = valueGetter("costCenterId") || payment.costCenterId;
                            if (!id) return "غير محدد";
                            return paymentCostCenters.find(cc => cc.costCenterId === id)?.description ?? id;
                        })(),

                        paymentRefNum: valueGetter("paymentRefNum") ?? payment.paymentRefNum ?? "",

                        project: (() => {
                            const proj = valueGetter("projectId");
                            if (proj && typeof proj === "object") {
                                return proj.projectName ?? proj.projectId ?? "غير محدد";
                            }
                            return payment.projectName ?? payment.projectId ?? "غير محدد";
                        })(),
                    };

                    return (
                        <FormElement>
                            <fieldset
                                className={`k-form-fieldset ${isFormDisabled ? 'grid-disabled' : 'grid-normal'}`}
                                aria-disabled={isFormDisabled}
                            >
                                <Grid container spacing={1} padding={2}>


                                    {/* Hidden Fields */}
                                    <Field name="statusId" component="input" type="hidden"/>
                                    <Field name="paymentTypeId" component="input" type="hidden"/>
                                    <Field name="partyIdFrom" component="input" type="hidden"/>
                                    <Field name="partyIdTo" component="input" type="hidden"/>
                                    <Field name="currencyUomId" component="input" type="hidden"/>
                                    <Field name="finAccountTransId" component="input" type="hidden"/>
                                    <Field name="isDepositWithDrawPayment" component="input" type="hidden"/>
                                    <Field name="finAcctTransTypeId" component="input" type="hidden"/>
                                    <Field name="isDisbursement" component="input" type="hidden"/>
                                    <Field name="paymentPreferenceId" component="input" type="hidden"/>
                                    <Field name="paymentGatewayResponseId" component="input" type="hidden"/>

                                    {/* Section 2: Party Details */}
                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems="flex-end">
                                            <Grid item xs={3}>
                                                <Typography variant="h6" sx={{pl: 2, pb: 1}}>
                                                    {getTranslatedLabel(
                                                        paymentType === 1 ? `${localizationKey}.from` : `${localizationKey}.to`,
                                                        paymentType === 1 ? "From Party" : "To Party"
                                                    )}
                                                </Typography>
                                                <Typography variant="h6" sx={{pl: 2}}>
                                                    <strong style={{color: "blue"}}>
                                                        {paymentType === 1 ? payment.partyIdFromName : payment.partyIdToName || "N/A"}
                                                    </strong>
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Typography variant="h6" sx={{pl: 2, pb: 1}}>
                                                    {getTranslatedLabel(
                                                        paymentType === 1 ? `${localizationKey}.to` : `${localizationKey}.from`,
                                                        paymentType === 1 ? "To Party" : "From Party"
                                                    )}
                                                </Typography>
                                                <Typography variant="h6" sx={{pl: 2}}>
                                                    <strong style={{color: "blue"}}>
                                                        {paymentType === 1 ? payment.partyIdToName : payment.partyIdFromName || "N/A"}
                                                    </strong>
                                                </Typography>
                                            </Grid>
                                        </Grid>
                                    </Grid>

                                    {/* Section 1: Core Details */}
                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems="flex-end">
                                            <Grid item xs={3}>
                                                <Typography variant="h6" sx={{pl: 2, pb: 1}}>
                                                    {getTranslatedLabel(
                                                        `${localizationKey}.paymentType`,
                                                        "Payment Type"
                                                    )}
                                                </Typography>
                                                <Typography variant="h6" sx={{pl: 2}}>
                                                    <strong style={{color: "blue"}}>{paymentTypeDesc}</strong>
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="paymentMethodId"
                                                    name="paymentMethodId"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.paymentMethod`,
                                                        "Payment Method *"
                                                    )}
                                                    component={MemoizedFormDropDownList}
                                                    dataItemKey="paymentMethodId"
                                                    textField="description"
                                                    data={paymentMethods || []}
                                                    validator={requiredValidator}
                                                    onChange={handlePaymentMethodChange}
                                                />
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Field
                                                    id="chequeNumber"
                                                    name="chequeNumber"
                                                    label={getTranslatedLabel(`${localizationKey}.chequeNumber`, "Cheque Number")}
                                                    component={FormInput}
                                                    autoComplete="off"
                                                    validator={(value, getter) => chequeValidator(value, getter, undefined, formRenderProps)}
                                                />
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="chequeDate"
                                                    name="chequeDate"
                                                    label={getTranslatedLabel(`${localizationKey}.chequeDate`, "Cheque Date")}
                                                    component={FormDatePicker}
                                                    format="yyyy-MM-dd"
                                                    validator={(value, getter) => chequeValidator(value, getter, undefined, formRenderProps)}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>

                                    <Grid item xs={12}></Grid>
                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems="flex-end">
                                            <Grid item xs={2}>
                                                <Field
                                                    id="amount"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.amount`,
                                                        "Amount *"
                                                    )}
                                                    format="n2"
                                                    min={0}
                                                    name="amount"
                                                    component={FormNumericTextBox}
                                                    validator={(value) => requiredValidator(value) || amountValidator(value, valueGetter)}
                                                />
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Typography variant="h6" sx={{pl: 2, pb: 1}}>
                                                    {getTranslatedLabel(
                                                        `${localizationKey}.currency`,
                                                        "Currency"
                                                    )}
                                                </Typography>
                                                <Typography variant="h6" sx={{pl: 2}}>
                                                    <strong style={{color: "blue"}}>{payment.currencyUomId}</strong>
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={3}>
                                                {isLoadingGlAccounts ? (
                                                    <Skeleton variant="rounded" height={56}/>
                                                ) : (
                                                    <Field
                                                        id="overrideGlAccountId"
                                                        name="overrideGlAccountId"
                                                        label={getTranslatedLabel(`${localizationKey}.overrideGlAccountId`, "Override GL Account")}
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
                                                <Field
                                                    id="costCenterId"
                                                    name="costCenterId"
                                                    label={getTranslatedLabel(`${localizationKey}.costCenter`, "Cost Center")}
                                                    component={MemoizedFormComboBox2}
                                                    data={paymentCostCenters || []}
                                                    dataItemKey="costCenterId"
                                                    textField="description"
                                                />
                                            </Grid>
                                            <Grid item xs={1}>
                                                <Button
                                                    size="small"
                                                    variant="outlined"
                                                    color="secondary"
                                                    onClick={() => setShowCreateCostCenter(true)}
                                                    sx={{mt: 1}}
                                                >
                                                    +
                                                </Button>
                                            </Grid>

                                               
                                        </Grid>
                                    </Grid>

                                    {/* Section 4: Metadata */}
                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems="flex-end">
                                            <Grid item xs={2}>
                                                <Field
                                                    id="effectiveDate"
                                                    name="effectiveDate"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.effectiveDate`,
                                                        "Effective Date *"
                                                    )}
                                                    component={FormDatePicker}
                                                    format="yyyy-MM-dd"
                                                    validator={requiredValidator}
                                                />
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Field
                                                    id="paymentRefNum"
                                                    name="paymentRefNum"
                                                    label={getTranslatedLabel(`${localizationKey}.paymentRefNum`, "paymentRefNum")}
                                                    component={FormInput}
                                                    autoComplete="off"
                                                />
                                            </Grid>

                                            <Grid item xs={4}>
                                                <Field
                                                    id="comments"
                                                    name="comments"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.comments`,
                                                        "Comments"
                                                    )}
                                                    component={FormTextArea}
                                                    autoComplete="off"
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="projectId"
                                                    name="projectId"
                                                    component={FormComboBoxVirtualProject}
                                                    label={getTranslatedLabel("projects.certificate.form.project", "Project")}
                                                    dataItemKey="projectId"
                                                    textField="ProjectName"
                                                    // REFACTOR: Required ONLY when it's an advance payment
                                                    validator={(value) =>
                                                        isAdvancePayment ? requiredValidator(value) : undefined
                                                    }
                                                    onChange={handleProjectChange}
                                                />
                                            </Grid>
                                            {isAdvancePayment && partyIdTo && currentProjectId && (
                                                <Grid item xs={12} sx={{mt: 2}}>
                                                    <Box sx={{
                                                        p: 2,
                                                        border: "1px solid #e0e0e0",
                                                        borderRadius: 2,
                                                        bgcolor: "#f9f9f9"
                                                    }}>
                                                        {balanceLoading ? (
                                                            <Skeleton height={80}/>
                                                        ) : balanceData ? (
                                                            balanceData.initialBalance === 0 ? (
                                                                <Alert severity="warning">
                                                                    {balanceData.message || "لا يوجد سقف دفع مُعيَّن لهذا المورد على المشروع"}
                                                                </Alert>
                                                            ) : (
                                                                <Grid container spacing={2}>
                                                                    <Grid item xs={4}>
                                                                        <Typography variant="body2"
                                                                                    color="text.secondary">السقف
                                                                            المتاح</Typography>
                                                                        <Typography variant="h6" color="success.main"
                                                                                    fontWeight="bold">
                                                                            {balanceData.initialBalance.toLocaleString("ar-EG")} ج.م
                                                                        </Typography>
                                                                    </Grid>
                                                                    <Grid item xs={4}>
                                                                        <Typography variant="body2"
                                                                                    color="text.secondary">المستخدم</Typography>
                                                                        <Typography variant="h6" color="warning.main">
                                                                            {balanceData.usedBalance.toLocaleString("ar-EG")} ج.م
                                                                        </Typography>
                                                                    </Grid>
                                                                    <Grid item xs={4}>
                                                                        <Typography variant="body2"
                                                                                    color="text.secondary">المتبقي</Typography>
                                                                        <Typography variant="h6"
                                                                                    color={balanceData.remainingBalance > 0 ? "primary" : "error"}
                                                                                    fontWeight="bold">
                                                                            {balanceData.remainingBalance.toLocaleString("ar-EG")} ج.م
                                                                        </Typography>
                                                                    </Grid>
                                                                    {amount > balanceData.remainingBalance && (
                                                                        <Grid item xs={12}>
                                                                            <Alert severity="error">
                                                                                المبلغ المطلوب
                                                                                ({amount.toLocaleString("ar-EG")} ج.م)
                                                                                يتجاوز الرصيد المتاح
                                                                            </Alert>
                                                                        </Grid>
                                                                    )}
                                                                </Grid>
                                                            )
                                                        ) : (
                                                            <Typography color="text.secondary">جاري تحميل بيانات
                                                                الحساب...</Typography>
                                                        )}
                                                    </Box>
                                                </Grid>
                                            )}
                                        </Grid>
                                    </Grid>

                                </Grid>
                            </fieldset>
                            <div className="k-form-buttons">
                                <Grid container spacing={2}>
                                    <Grid item xs={2}>
                                        <Button
                                            type="submit"
                                            variant="contained"
                                            disabled={isSubmitDisabled}
                                            sx={{mt: 2, mr: 1}}
                                        >
                                            {getTranslatedLabel(
                                                `${localizationKey}.update`,
                                                "Update Payment"
                                            )}
                                        </Button>
                                    </Grid>
                                    <Grid item xs={2}>
                                        <PaymentExcelTechnical
                                            companyName={companyName ?? "N/A"}
                                            payment={excelPaymentData}
                                            applications={excelApplications}
                                            transactions={excelTransactions}
                                            getTranslatedLabel={getTranslatedLabel}
                                            isFetching={isExcelFetching}
                                        />
                                    </Grid>

                                    <Grid item xs={2}>
                                        <PaymentExcelParty
                                            companyName={companyName ?? "N/A"}
                                            payment={excelPaymentData}
                                            getTranslatedLabel={getTranslatedLabel}
                                            isFetching={isExcelFetching}
                                        />
                                    </Grid>

                                    <Grid item xs={2}>
                                        <Button
                                            variant="contained"
                                            color="secondary"
                                            onClick={handlePreviewPdf}
                                            disabled={isPdfFetching || !payment?.paymentId}
                                            sx={{ mt: 2, mr: 1 }}
                                        >
                                            {isPdfFetching ? "جاري التحضير..." : "معاينة بيان الدفعة (PDF)"}
                                        </Button>
                                    </Grid>
                                    
                                    <Grid item xs={1}>
                                        <Button
                                            sx={{mt: 2}}
                                            onClick={handleCancelForm}
                                            color="error"
                                            variant="contained"
                                        >
                                            {getTranslatedLabel("general.cancel", "Cancel")}
                                        </Button>
                                    </Grid>

                                </Grid>
                            </div>
                            {/* Modal */}
                            <CreateCostCenterModal
                                open={showCreateCostCenter}
                                onClose={() => setShowCreateCostCenter(false)}
                                isOutPayment={!!payment?.isDisbursement}
                            />

                            <Dialog
                                open={showPdfViewer}
                                onClose={handleClosePdfViewer}
                                maxWidth="lg"
                                fullWidth
                                fullScreen={window.innerWidth <= 900}
                                PaperProps={{ sx: { height: "90vh", width: "90vw", m: 2 } }}
                            >
                                <DialogTitle sx={{ m: 0, p: 2, pr: 6 }}>
                                    معاينة بيان الدفعة - {payment?.paymentId}
                                    <IconButton
                                        aria-label="close"
                                        onClick={handleClosePdfViewer}
                                        sx={{ position: "absolute", right: 8, top: 8 }}
                                    >
                                        <CloseIcon />
                                    </IconButton>
                                </DialogTitle>

                                <DialogContent dividers sx={{ p: 0, position: "relative" }}>
                                    {previewLoading ? (
                                        <Box sx={{ height: "100%", display: "flex", alignItems: "center", justifyContent: "center" }}>
                                            <CircularProgress />
                                            <Typography sx={{ ml: 2 }}>جاري تحميل البيان...</Typography>
                                        </Box>
                                    ) : pdfBlobUrl ? (
                                        <Worker workerUrl={`https://unpkg.com/pdfjs-dist@${pdfjsVersion}/build/pdf.worker.min.mjs`}>
                                            <Viewer
                                                fileUrl={pdfBlobUrl}
                                                plugins={[defaultLayoutPluginInstance]}
                                            />
                                        </Worker>
                                    ) : (
                                        <Box sx={{ p: 4, textAlign: "center" }}>
                                            <Typography color="error">
                                                فشل تحميل ملف PDF. حاول مرة أخرى.
                                            </Typography>
                                        </Box>
                                    )}
                                </DialogContent>

                                <DialogActions sx={{ p: 2 }}>
                                    <Button onClick={() => window.print()} variant="contained" color="primary">
                                        طباعة
                                    </Button>
                                    <Button onClick={handleClosePdfViewer} variant="outlined">
                                        إغلاق
                                    </Button>
                                </DialogActions>
                            </Dialog>
                            
                        </FormElement>
                    )
                        ;
                }}
            />
        </Grid>
    );
};

export default EditPaymentForm;