import { useState } from "react";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Button, Grid, Paper } from "@mui/material";
import BillingAccountsMenu from "../menu/BillingAccountsMenu";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { FormComboBoxVirtualCustomer } from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { useCreateBillingAccountPaymentMutation, useFetchAllPaymentTypesQuery, useFetchInternalAccountingOrganizationsLovQuery } from "../../../../app/store/apis";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { useAppSelector } from "../../../../app/store/configureStore";
import { toast } from "react-toastify";

interface Props {
  onClose: () => void;
}

const BillingAccountPaymentForm = ({ onClose }: Props) => {
  const {selectedBillingAccount} = useAppSelector(state => state.accountingSharedUi)
  const [formKey, setFormKey] = useState(Math.random());
  const { data: companies } =
    useFetchInternalAccountingOrganizationsLovQuery(undefined);
    // const {data: paymentMethodTypes} = useFetchAllPaymentMethodTypesQuery(undefined)
    const {data: paymentTypes} = useFetchAllPaymentTypesQuery(undefined)
    const [createPayment] = useCreateBillingAccountPaymentMutation()

    const onSubmit = async (data: any) => {
      if (!data.isValid) {
        return
      }
      const {values} = data
      const {paymentTypeId, fromPartyId, organizationPartyId, amount} = values
      const billingAccountPayment = {
        billingAccountId: selectedBillingAccount?.billingAccountId,
        paymentTypeId,
        partyIdFrom: fromPartyId.fromPartyId,
        partyIdTo: organizationPartyId,
        amount
      }
      try {
        await createPayment(billingAccountPayment)        
        toast.success("Payment created successfully")
      } catch (error) {
        console.error(error)
        toast.error("Something went wrong")
      }
    }

  return (
    <>
      <AccountingMenu selectedMenuItem="billingAcc" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <BillingAccountsMenu selectedMenuItem="/billingAccounts/payments" />
        <Form
          onSubmitClick={(values) => onSubmit(values)}
          key={formKey.toString()}
          initialValues={{fromPartyId: selectedBillingAccount?.partyId}}
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid container spacing={2}>
                  <Grid item container xs={12} spacing={2}>
                    <Grid item xs={3}>
                      <Field
                        name="fromPartyId"
                        id="fromPartyId"
                        label="Party Id"
                        component={FormComboBoxVirtualCustomer}
                        validator={requiredValidator}
                        disabled
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field
                        name="organizationPartyId"
                        id="organizationPartyId"
                        label="Organization Party"
                        component={MemoizedFormDropDownList2}
                        data={companies ?? []}
                        dataItemKey="partyId"
                        textField="partyName"
                      />
                    </Grid>
                  </Grid>
                  <Grid item xs={3}>
                    <Field
                      name="paymentTypeId"
                      id="paymentTypeId"
                      label="Payment Type"
                      component={MemoizedFormDropDownList2}
                      data={paymentTypes ?? []}
                      dataItemKey="paymentTypeId"
                      textField="description"
                    />
                  </Grid>
                  {/* <Grid item xs={3}>
                    <Field
                      name="paymentMethodTypeId"
                      id="paymentMethodTypeId"
                      label="Payment Method Type"
                      component={MemoizedFormDropDownList2}
                      data={paymentMethodTypes ?? []}
                      dataItemKey="paymentMethodTypeId"
                      textField="description"
                    />
                  </Grid> */}
                  <Grid item xs={3}>
                    <Field
                      name="amount"
                      id="amount"
                      label="Amount"
                      component={FormNumericTextBox}
                    />
                  </Grid>

                </Grid>
              </fieldset>
              <div className="k-form-buttons">
                <Button
                  variant="contained"
                  type={"submit"}
                  color="success"
                  disabled={!formRenderProps.allowSubmit}
                >
                  Save
                </Button>

                <Button
                  variant="contained"
                  type={"button"}
                  color="error"
                  onClick={onClose}
                >
                  Back
                </Button>
              </div>
            </FormElement>
          )}
        />
      </Paper>
    </>
  );
};

export default BillingAccountPaymentForm;
