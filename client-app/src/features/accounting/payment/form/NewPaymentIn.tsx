import { useCallback } from "react";
import { FormComboBoxVirtualParty } from "../../../../app/common/form/FormComboBoxVirtualParty";
import {chequeValidator, requiredValidator} from "../../../../app/common/form/Validators";
import {
    Field,
    Form,
    FormElement,
    FormRenderProps,
} from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import {Button, Grid, Skeleton, Typography} from "@mui/material";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import { Error } from "@progress/kendo-react-labels";
import FormInput from "../../../../app/common/form/FormInput";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import {MemoizedFormDropDownList2} from "../../../../app/common/form/MemoizedFormDropDownList2";
import {RootState, useAppSelector, useGetCostCentersQuery} from "../../../../app/store/configureStore";
import {useFetchGlAccountOrganizationHierarchyLovQuery} from "../../../../app/store/apis";
import {FormDropDownTreeGlAccount2} from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import {MemoizedFormComboBox2} from "../../../../app/common/form/FormComboBox2";
import {FormComboBoxVirtualProject} from "../../../../app/common/form/FormComboBoxVirtualProject";

interface NewPaymentInProps {
    partyInputRef: React.RefObject<HTMLInputElement>;
    companies?: any[];
    filteredPaymentTypes: any[];
    paymentMethods?: any[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    setShowNewCustomer: (show: boolean) => void;
    onCreate: (data: { values: any; menuItem: string }) => void;
    handleCancelForm: () => void;
}

const NewPaymentIn: React.FC<NewPaymentInProps> = ({
                                                       partyInputRef,
                                                       companies,
                                                       filteredPaymentTypes,
                                                       paymentMethods,
                                                       getTranslatedLabel,
                                                       paymentTypeValidator,
                                                       setShowNewCustomer,
                                                       onCreate,
                                                         handleCancelForm,
                                                   }) => {
    const localizationKey = "accounting.payments.form";
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";
    const companyName = useAppSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
    const { data: glAccounts, isLoading: isLoadingGlAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, {
        skip: !companyId,
    });

    const {
        data: paymentCostCenters = [],
        isLoading: loadingOut
    } = useGetCostCentersQuery({ type: 'in' });


  

    // Kendo onSubmit → values + valid
    const handleSubmit = (values: any) => {
        onCreate({
            values,
            menuItem: "Create Payment",
        });
    };

    const getDefaultOrganizationPartyId = useCallback(() => {
        return companies && companies.length > 0 ? companies[0].organizationPartyId : "";
    }, [companies]);

    return (
        <Form
            initialValues={{
                paymentId: "",
                paymentTypeId: "",
                paymentMethodId: "",
                statusId: "PMNT_NOT_PAID",
                partyIdFrom: "",
                partyIdFromName: "",
                amount: 0,
                paymentRefNum: "",
                currencyUomId: "EGP",
                organizationPartyId: getDefaultOrganizationPartyId(),
                isDepositWithDrawPayment: "Y",
                finAccountTransTypeId: "DEPOSIT",
                isDisbursement: false,
                chequeNumber: "",
                chequeDate: null,
                projectId: "",
                costCenterId: "",
            }}
            onSubmit={handleSubmit}
            render={(formRenderProps: FormRenderProps) => {
                const { valid, onSubmit, onChange } = formRenderProps;

                return (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={2}>
                                {/* Hidden Fields */}
                                <Field name="paymentId" component="input" type="hidden" />
                                <Field name="statusId" component="input" type="hidden" />
                                <Field name="currencyUomId" component="input" type="hidden" />
                                <Field
                                    name="isDepositWithDrawPayment"
                                    component="input"
                                    type="hidden"
                                />
                                <Field
                                    name="finAccountTransTypeId"
                                    component="input"
                                    type="hidden"
                                />
                                <Field name="isDisbursement" component="input" type="hidden" />

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
                                                id="partyIdFrom"
                                                name="partyIdFrom"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.from`,
                                                    "From Party Id *"
                                                )}
                                                component={FormComboBoxVirtualParty}
                                                autoComplete="off"
                                                validator={requiredValidator}
                                                inputRef={partyInputRef}
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
                                                    "New Customer"
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
                                                validator={paymentTypeValidator}
                                                disabled={filteredPaymentTypes.length === 0}
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
                                    </Grid>
                                </Grid>

                                <Grid item xs={12}>
                                    <Grid container spacing={2} alignItems="flex-end">
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
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={2}>
                                            {isLoadingGlAccounts ? (
                                                <Skeleton variant="rounded" height={56} />
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
                                                id="projectId"
                                                name="projectId"
                                                component={FormComboBoxVirtualProject}
                                                label={getTranslatedLabel("projects.certificate.form.project", "Project")}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
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

                                {formRenderProps.errors.form && (
                                    <Grid item xs={12}>
                                        <Error>{formRenderProps.errors.form}</Error>
                                    </Grid>
                                )}
                                <Grid container spacing={2}>
                                    <Grid item xs={2}>
                                        <Button
                                            type="submit"
                                            variant="contained"
                                            disabled={!formRenderProps.valid || !filteredPaymentTypes.length}
                                            sx={{ mt: 2 , ml: 2 }}
                                        >
                                            {getTranslatedLabel(`${localizationKey}.create`, "Create Payment")}
                                        </Button>
                                    </Grid>
                                    <Grid item xs={1}>
                                        <Button
                                            sx={{ mt: 2 }}
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
    );
};

export default NewPaymentIn;