import React, { useEffect, useRef, useState } from "react";
import { FinancialAccount } from "../../../../app/models/accounting/financialAccount";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import {
  useFetchCurrenciesQuery,
  useFetchFinAccountStatusesQuery,
  useFetchFinAccountTypesQuery,
  useFetchOrgChartOfAccountsLovQuery,
} from "../../../../app/store/configureStore";
import { FormDropDownTreeGlAccount2 } from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import { FormDropDownTreeGlAccount } from "../../../../app/common/form/FormDropDownTreeGlAccount";
import FormInput from "../../../../app/common/form/FormInput";
import FinancialAccountMenu from "../menu/FinancialAccountMenu";
import { useFetchInternalAccountingOrganizationsLovQuery } from "../../../../app/store/apis";

interface Props {
  editMode: number;
  selectedFinancialAccount?: FinancialAccount;
  onClose: () => void;
}

const FinancialAccountForm = ({
  editMode,
  selectedFinancialAccount,
  onClose,
}: Props) => {
  const formRef = useRef<Form | null>(null);
  const [formKey, setFormKey] = useState(Math.random());
  const [account, setAccount] = useState<FinancialAccount | undefined>(
    selectedFinancialAccount
  );
  const { data: currencies } = useFetchCurrenciesQuery(undefined);
  const { data: finAccountTypes } = useFetchFinAccountTypesQuery(undefined);
  const {data: glAccounts} = useFetchOrgChartOfAccountsLovQuery(undefined);
  const { data: finAccountStatuses } =
    useFetchFinAccountStatusesQuery(undefined);
  const {data: companies} = useFetchInternalAccountingOrganizationsLovQuery(undefined)

  // useEffect(() => {
  //   return () => onClose()
  // }, [])

  useEffect(() => {
    if (selectedFinancialAccount) {
      setAccount({
        ...selectedFinancialAccount,
        fromDate: selectedFinancialAccount?.fromDate ?? new Date()
      })
    } else {
      setAccount({
        ...account,
        finAccountId: "",
        finAccountTypeId: "BANK_ACCOUNT",
        statusId: "FNACT_ACTIVE",
        fromDate: new Date()
      })
    }
    setFormKey(Math.random())
  }, [selectedFinancialAccount])
  return (
    <>
      <AccountingMenu selectedMenuItem={"/financialAcc"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        {editMode > 1 && <FinancialAccountMenu selectedMenuItem="financialAccounts" />}
        <Grid container spacing={2} >
          <Grid item xs={7}>
            <Box display="flex" justifyContent="space-between">
              <Typography
                sx={{ p: 2 }}
                color={editMode > 1 ? "black" : "green"}
                variant="h4"
              >
                {editMode === 1
                  ? "New Financial Account"
                  : `Financial Account: ${selectedFinancialAccount?.finAccountName}`}
              </Typography>
            </Box>
          </Grid>
        </Grid>
        <Grid container ml={2}>
          <Form
            onSubmit={(values) => console.log(values as FinancialAccount)}
            ref={formRef}
            key={formKey.toString()}
            initialValues={account}
            render={(formRenderProps) => (
              <FormElement>
                <fieldset className={"k-form-fieldset"}>
                  <Grid container spacing={2}>
                    <Grid item container xs={12} spacing={2}>
                      <Grid item xs={3}>
                        <Field
                          name="finAccountTypeId"
                          id="finAccountTypeId"
                          label="Fin Account Type"
                          component={MemoizedFormDropDownList2}
                          data={finAccountTypes ?? []}
                          dataItemKey="finAccountTypeId"
                          textField="finAccountTypeDescription"
                          validator={requiredValidator}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Field
                          name="statusId"
                          id="statusId"
                          label="Fin Account Status"
                          component={MemoizedFormDropDownList2}
                          data={finAccountStatuses ?? []}
                          dataItemKey="finAccountStatusId"
                          textField="finAccountStatusDescription"
                          validator={requiredValidator}
                        />
                      </Grid>
                      <Grid item xs={3}>
                      <Field
                        name="currencyUomId"
                        id="currencyUomId"
                        label="Account Currency"
                        component={MemoizedFormDropDownList2}
                        data={currencies ?? []}
                        dataItemKey="currencyUomId"
                        textField="description"
                        validator={requiredValidator}
                      />
                    </Grid>
                    </Grid>
                    <Grid item container xs={12} spacing={2}>
                      <Grid item xs={3}>
                        <Field
                          name="finAccountName"
                          id="finAccountName"
                          label="Account Name"
                          component={FormInput}
                          disabled={editMode > 1}
                        />
                      </Grid>
                      {/* <Grid item xs={3}>
                        <Field
                          name="finAccountCode"
                          id="finAccountCode"
                          label="Account Code"
                          component={FormInput}
                          disabled={editMode > 1}
                        />
                      </Grid>
                      <Grid item xs={2}>
                        <Field
                          name="finAccountPin"
                          id="finAccountPin"
                          label="PIN"
                          component={FormInput}
                          disabled={editMode > 1}
                        />
                      </Grid> */}
                      <Grid item xs={2}>
                        <Field
                          name="isRefundable"
                          id="isRefundable"
                          label="Is Refundable"
                          component={MemoizedFormDropDownList2}
                          data={[
                            {value: "Y", text: "Yes"},
                            {value: "N", text: "No"}
                          ]}
                          dataItemKey="value"
                          textField={"text"}
                        />
                      </Grid>
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
                      <Grid item xs={3}>
                        <Field
                          name="ownerPartyId"
                          id="ownerPartyId"
                          label="Owner"
                          component={MemoizedFormDropDownList2}
                          data={companies ?? []}
                          dataItemKey="partyId"
                          textField="partyName"
                        />
                      </Grid>                    
                    <Grid item xs={4}>
                      <Field
                        id={"postToGlAccountId"}
                        name={"postToGlAccountId"}
                        label={"Post to GL Account"}
                        data={glAccounts ? glAccounts : []}
                        component={FormDropDownTreeGlAccount}
                        dataItemKey={"glAccountId"}
                        textField={"text"}
                        selectField={"selected"}
                        expandField={"expanded"}
                        validator={requiredValidator}
                      />
                    </Grid>
                  </Grid>
                  <Grid item container spacing={2} flexDirection={"row"}>
                  <Grid item xs={2}>
                      <Field
                        name="actualBalance"
                        id="actualBalance"
                        label="Actual Balance"
                        component={FormNumericTextBox}
                        disabled={editMode > 1}
                      />
                    </Grid>
                    <Grid item xs={2}>
                      <Field
                        name="availableBalance"
                        id="availableBalance"
                        label="Available Balance"
                        component={FormNumericTextBox}
                        disabled={editMode > 1}
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field
                        name="fromDate"
                        id="fromDate"
                        label="From Date"
                        component={FormDatePicker}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field
                        name="thruDate"
                        id="thruDate"
                        label="Thru Date"
                        component={FormDatePicker}
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
        </Grid>
      </Paper>
    </>
  );
};

export default FinancialAccountForm;
