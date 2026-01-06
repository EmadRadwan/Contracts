import {useCallback, useEffect, useState} from "react";
import {FormComboBoxVirtualParty} from "../../../../app/common/form/FormComboBoxVirtualParty";
import {chequeValidator, requiredValidator} from "../../../../app/common/form/Validators";
import {Field, Form, FormElement, FormRenderProps} from "@progress/kendo-react-form";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import {Alert, Box, Button, Grid, Skeleton, Typography} from "@mui/material";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import FormInput from "../../../../app/common/form/FormInput";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import {MemoizedFormDropDownList2} from "../../../../app/common/form/MemoizedFormDropDownList2";
import {RootState, useAppSelector, useGetCostCentersQuery} from "../../../../app/store/configureStore";
import {
    useFetchGlAccountOrganizationHierarchyLovQuery,
    useLazyFetchBalancesForVendorAndProjectQuery
} from "../../../../app/store/apis";
import {FormDropDownTreeGlAccount2} from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import {FormComboBoxVirtualProject} from "../../../../app/common/form/FormComboBoxVirtualProject";
import {MemoizedFormComboBox2} from "../../../../app/common/form/FormComboBox2";
import CreateCostCenterModal from "./CreateCostCenterModal";

interface NewPaymentOutProps {
    partyInputRef: React.RefObject<HTMLInputElement>;
    companies?: any[];
    filteredPaymentTypes: any[];
    paymentMethods?: any[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    setShowNewCustomer: (show: boolean) => void;
    onCreate: (data: { values: any; menuItem: string }) => void;
    handleCancelForm: () => void;
}

const ADVANCE_TO_VENDOR_CONTRACTOR = "ADVANCE_TO_VENDOR_CONTRACTOR";

const NewPaymentOut: React.FC<NewPaymentOutProps> = ({
                                                         partyInputRef,
                                                         companies,
                                                         filteredPaymentTypes,
                                                         paymentMethods,
                                                         getTranslatedLabel,
                                                         setShowNewCustomer,
                                                         onCreate,
                                                         handleCancelForm,
                                                     }) => {
    const localizationKey = "accounting.payments.form";
    const {user} = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";
    const companyName = useAppSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
    const {
        data: glAccounts,
        isLoading: isLoadingGlAccounts
    } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, {
        skip: !companyId,
    });
    const [selectedParty, setSelectedParty] = useState<any>(null);
    const [selectedProject, setSelectedProject] = useState<any>(null);

    const [currentPaymentTypeId, setCurrentPaymentTypeId] = useState<string>("");
    const [triggerBalanceFetch, {data: balanceData, isFetching: balanceLoading}] =
        useLazyFetchBalancesForVendorAndProjectQuery();


    const {
        data: paymentCostCenters = [],
        isLoading: loadingOut
    } = useGetCostCentersQuery({ type: 'out' }, { refetchOnMountOrArgChange: true });

    // REFACTOR: Added state to control the Create Cost Center modal (mirrors EditPaymentForm behavior)
    const [showCreateCostCenter, setShowCreateCostCenter] = useState(false);

    const stableTrigger = useCallback(
        (partyId: string, projectId: string) => {
            if (partyId && projectId) {
                triggerBalanceFetch({partyId, projectId}, false);
            }
        },
        [triggerBalanceFetch]
    );

    useEffect(() => {
        const isAdvance = currentPaymentTypeId === ADVANCE_TO_VENDOR_CONTRACTOR;
        
        const hasParty = !!selectedParty;
        const hasProject = !!selectedProject;

        if (isAdvance && hasParty && hasProject) {
            const partyId = typeof selectedParty === "string" ? selectedParty : selectedParty.fromPartyId;
            const projectId = typeof selectedProject === "string" ? selectedProject : selectedProject.projectId;

            stableTrigger(partyId, projectId);
        }
    }, [currentPaymentTypeId, selectedParty, selectedProject, stableTrigger]);


    console.log("useEffect currentPaymentTypeId", currentPaymentTypeId);
    // Handle form submission
    const handleSubmit = (values: any) => {
        onCreate({
            values,
            menuItem: "Create Payment",
        });
    };

    const getDefaultOrganizationPartyId = useCallback(() => {
        return companies && companies.length > 0 ? companies[0].organizationPartyId : "";
    }, [companies]);

    const amountValidator = (value: number, getter: any) => {
        if (!value || value <= 0) return "الرجاء إدخال مبلغ صحيح";

        const paymentTypeId = getter("paymentTypeId");
        if (paymentTypeId !== ADVANCE_TO_VENDOR_CONTRACTOR) return; // No balance check needed

        if (!balanceData) return "جاري التحقق من الرصيد...";

        if (balanceData.initialBalance === 0) {
            return "لا يمكن إنشاء دفعة مقدمة: لا يوجد سقف دفع مُعيَّن لهذا المورد على المشروع";
        }

        if (value > balanceData.remainingBalance) {
            return `المبلغ المُدخل (${value.toLocaleString("ar-EG")}) يتجاوز الرصيد المتاح (${balanceData.remainingBalance.toLocaleString("ar-EG")})`;
        }

        return undefined;
    };


    return (
        <>
        <Form
            initialValues={{
                paymentId: "",
                paymentTypeId: "",
                paymentMethodId: "",
                statusId: "PMNT_NOT_PAID",
                partyIdTo: "",
                partyIdToName: "",
                amount: 0,
                paymentRefNum: "",
                currencyUomId: "EGP",
                organizationPartyId: getDefaultOrganizationPartyId(),
                isDepositWithDrawPayment: "Y",
                finAccountTransTypeId: "WITHDRAWAL",
                isDisbursement: true,
                chequeNumber: "",
                chequeDate: null,
                projectId: "",
                costCenterId: "",
            }}
            onSubmit={handleSubmit}
            render={(formRenderProps: FormRenderProps) => {
                const {valid, onSubmit, onChange, valueGetter} = formRenderProps;
                const currentPaymentTypeId = formRenderProps.valueGetter("paymentTypeId");
                const amount = valueGetter("amount") || 0;
                const isAdvancePayment = currentPaymentTypeId === ADVANCE_TO_VENDOR_CONTRACTOR;
                const hasPartyAndProject = !!selectedParty && !!selectedProject;

                const hasBillingAccountIssue =
                    isAdvancePayment &&
                    balanceData &&
                    (balanceData.initialBalance === 0 || amount > balanceData.remainingBalance);

                const isSubmitDisabled =
                    !valid ||
                    balanceLoading ||
                    hasBillingAccountIssue ||
                    !filteredPaymentTypes.length;


                return (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={2}>
                                {/* Hidden Fields */}
                                <Field name="paymentId" component="input" type="hidden"/>
                                <Field name="statusId" component="input" type="hidden"/>
                                <Field name="currencyUomId" component="input" type="hidden"/>
                                <Field
                                    name="isDeposit_WithDrawPayment"
                                    component="input"
                                    type="hidden"
                                />
                                <Field
                                    name="finAccountTransTypeId"
                                    component="input"
                                    type="hidden"
                                />
                                <Field name="isDisbursement" component="input" type="hidden"/>

                                <Grid item xs={12}>
                                    <Grid container spacing={2} alignItems="flex-end">
                                        <Grid item xs={3}>
                                            <Field
                                                id="organizationPartyId"
                                                name="organizationPartyId"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.orgPartyId`,
                                                    "Organization Party Id *"
                                                )}
                                                component={MemoizedFormDropDownList2}
                                                dataItemKey="organizationPartyId"
                                                textField="organizationPartyName"
                                                data={companies || []}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="partyIdTo"
                                                name="partyIdTo"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.to`,
                                                    "To Party Id *"
                                                )}
                                                component={FormComboBoxVirtualParty}
                                                autoComplete="off"
                                                validator={requiredValidator}
                                                inputRef={partyInputRef}
                                                onChange={(e: any) => {
                                                    const value = e.value;
                                                    setSelectedParty(value);
                                                    formRenderProps.onChange("partyIdTo", {value});
                                                }}
                                            />
                                        </Grid>

                                        <Grid item xs={1}>
                                            <Button
                                                color="secondary"
                                                onClick={() => setShowNewCustomer(true)}
                                                variant="outlined"
                                            >
                                                {getTranslatedLabel(
                                                    `${localizationKey}.new-party`,
                                                    "New Contractor"
                                                )}
                                            </Button>
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="costCenterId"
                                                name="costCenterId"
                                                label={getTranslatedLabel(`${localizationKey}.costCenter`, "Cost Center)")}
                                                component={MemoizedFormComboBox2}
                                                data={paymentCostCenters || []}
                                                dataItemKey="costCenterId"      // tells FormComboBox which field is the key
                                                textField="description"      // tells FormComboBox which field to display
                                            />
                                        </Grid>

                                        <Grid item xs={1}>
                                            {/* REFACTOR: Added "+" button that opens the CreateCostCenterModal */}
                                            <Button
                                                size="small"
                                                variant="outlined"
                                                color="secondary"
                                                onClick={() => setShowCreateCostCenter(true)}
                                                sx={{ mt: 3 }} // Aligns with the ComboBox height
                                            >
                                                +
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </Grid>

                                <Grid item xs={12}>
                                    <Grid container spacing={2} alignItems="flex-end">
                                        <Grid item xs={3}>
                                            <Field
                                                id="paymentTypeId"
                                                name="paymentTypeId"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.paymentType`,
                                                    "Payment Type *"
                                                )}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="paymentTypeId"
                                                textField="description"
                                                data={filteredPaymentTypes}
                                                validator={requiredValidator}
                                                disabled={filteredPaymentTypes.length === 0}
                                                onChange={(e: any) => {
                                                    const value = e.value;
                                                    setCurrentPaymentTypeId(value);
                                                    formRenderProps.onChange("paymentTypeId", {value});
                                                }}
                                            />
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
                                        <Grid item xs={2}>
                                            <Field
                                                id="chequeDate"
                                                name="chequeDate"
                                                label={getTranslatedLabel(`${localizationKey}.chequeDate`, "Cheque Date")}
                                                component={FormDatePicker}
                                                format="yyyy-MM-dd"
                                                validator={(value, getter) => chequeValidator(value, getter, undefined, formRenderProps)}
                                            />
                                        </Grid>
                                        <Grid item xs={1}>
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Field
                                                id="amount"
                                                format="n2"
                                                min={0}
                                                name="amount"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.amount`,
                                                    "Amount *"
                                                )}
                                                component={FormNumericTextBox}
                                                validator={(value) => requiredValidator(value) || amountValidator(value, valueGetter)}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
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
                                        <Grid item xs={2}>
                                            <Field
                                                id="projectId"
                                                name="projectId"
                                                component={FormComboBoxVirtualProject}
                                                label={getTranslatedLabel("projects.certificate.form.project", "Project")}
                                                validator={(value) =>
                                                    isAdvancePayment ? requiredValidator(value) : undefined
                                                }
                                                onChange={(e: any) => {
                                                    const value = e.value;
                                                    setSelectedProject(value);
                                                    formRenderProps.onChange("projectId", {value});
                                                }}
                                            />
                                        </Grid>
                                        <Grid item xs={2}>
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
                                    </Grid>
                                </Grid>

                                <Grid container spacing={2} alignItems="flex-end">
                                    <Grid item xs={2}>
                                        <Field
                                            id="paymentRefNum"
                                            name="paymentRefNum"
                                            label={getTranslatedLabel(`${localizationKey}.paymentRefNum`, "paymentRefNum")}
                                            component={FormInput}
                                            autoComplete="off"
                                        />
                                    </Grid>
                                </Grid>
                                
                                {isAdvancePayment && hasPartyAndProject && (
                                    <Grid item xs={12}>
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
                                                    <Alert severity="warning" sx={{mb: 0}}>
                                                        {balanceData.message || "لا يوجد سقف دفع مُعيَّن لهذا المورد على المشروع"}
                                                    </Alert>
                                                ) : (
                                                    <Grid container spacing={2}>
                                                        <Grid item xs={4}>
                                                            <Typography variant="body2" color="text.secondary">السقف
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
                                                                    ({amount.toLocaleString("ar-EG")} ج.م) يتجاوز الرصيد
                                                                    المتاح
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

                                <Grid container spacing={2}>
                                    <Grid item xs={2}>
                                        <Button
                                            type="submit"
                                            variant="contained"
                                            disabled={isSubmitDisabled}
                                            sx={{mt: 2, ml: 2}}
                                        >
                                            {getTranslatedLabel(`${localizationKey}.create`, "Create Payment")}
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
                            </Grid>
                        </fieldset>
                    </FormElement>
                );
            }}
        />
            <CreateCostCenterModal
                open={showCreateCostCenter}
                onClose={() => setShowCreateCostCenter(false)}
                isOutPayment={true}
            />
        </>
    );
};

export default NewPaymentOut;