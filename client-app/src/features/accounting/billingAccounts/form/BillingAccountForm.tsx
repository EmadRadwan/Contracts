import React, { useEffect, useRef, useState } from "react";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { requiredValidator } from "../../../../app/common/form/Validators";
import {
  useFetchCurrenciesQuery,
} from "../../../../app/store/configureStore";
import BillingAccountsMenu from "../menu/BillingAccountsMenu";
import { BillingAccount } from "../../../../app/models/accounting/billingAccount";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import { FormComboBoxVirtualCustomer } from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import { useFetchBillingAccountsBalanceQuery } from "../../../../app/store/apis";
import { formatCurrency } from "../../../../app/util/utils";
import {FormComboBoxVirtualProject} from "../../../../app/common/form/FormComboBoxVirtualProject";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";

interface Props {
  editMode: number;
  selectedBillingAccount?: BillingAccount;
  onClose: () => void;
}

const BillingAccountForm = ({
  editMode,
  onClose,
  selectedBillingAccount,
}: Props) => {
  const formRef = useRef<Form | null>(null);
  const [formKey, setFormKey] = useState(Math.random())
  const { data: currencies } = useFetchCurrenciesQuery(undefined);
  const {data: billingAccountBalance} = useFetchBillingAccountsBalanceQuery(selectedBillingAccount?.billingAccountId!, {
    skip: !selectedBillingAccount?.billingAccountId
  })
  const [account, setAccount] = useState<BillingAccount | undefined>(selectedBillingAccount)
  const {getTranslatedLabel} = useTranslationHelper();

  useEffect(() => {
    if (billingAccountBalance) {
      setAccount(
        {
          ...account!,
          availableBalance: billingAccountBalance.billingAccountBalance!
        }
      )
      //setFormKey(Math.random())
    }
  }, [billingAccountBalance])

  console.log('selectedBillingAccount', selectedBillingAccount)
  return (
    <>
      <AccountingMenu selectedMenuItem={"/billingAccounts"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        {/*{editMode > 1 && <BillingAccountsMenu selectedMenuItem="/billingAccounts" />}*/}
        <Grid container spacing={2}>
          <Grid item xs={5}>
            <Box display="flex" justifyContent="space-between">
              <Typography sx={{ p: 2 }} color={editMode > 1 ? "black" : "green"} variant="h4">
                {editMode === 1
                    ? getTranslatedLabel("accounting.billingAccounts.form.new", "حساب أجل جديد")
                    : `${getTranslatedLabel("accounting.billingAccounts.form.title", "حساب الأجل")}: ${selectedBillingAccount?.billingAccountId}`}
              </Typography>
            </Box>
          </Grid>
        </Grid>
        <Form
          onSubmit={(values) => console.log(values as BillingAccount)}
          ref={formRef}
          key={formKey.toString()}
          initialValues={account}
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid container spacing={2}>
                  <Grid item xs={3}>
                    <Field
                      name="accountLimit"
                      id="accountLimit"
                      label={getTranslatedLabel("accounting.billingAccounts.form.accountLimit", "حد الحساب")}
                      component={FormNumericTextBox}
                      validator={requiredValidator}
                    />
                    </Grid>
                    <Grid item xs={3}>
                      <Field
                        name="accountCurrencyUomId"
                        id="accountCurrencyUomId"
                        label={getTranslatedLabel("accounting.billingAccounts.form.currency", "عملة الحساب")}
                        component={MemoizedFormDropDownList2}
                        data={currencies ?? []}
                        dataItemKey="currencyUomId"
                        textField="description"
                        disabled={editMode > 1}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field
                        name="partyId"
                        id="partyId"
                        label={getTranslatedLabel("accounting.billingAccounts.form.party", "العميل / الطرف")}
                        disabled={editMode > 1}
                        component={FormComboBoxVirtualCustomer}
                      />
                    </Grid>
                  </Grid>
                <Grid item xs={2}>
                  <Field
                      id="projectId"
                      name="projectId"
                      component={FormComboBoxVirtualProject}
                      label={getTranslatedLabel("projects.certificate.form.project", "Project")}
                      dataItemKey="projectId"
                      textField="ProjectName"
                      validator={requiredValidator}
                      disabled={editMode > 3}
                  />
                </Grid>
                  <Grid item container spacing={2} flexDirection={"row"}>
                    <Grid item xs={3}>
                      <Field
                        name="fromDate"
                        id="fromDate"
                        label={getTranslatedLabel("accounting.billingAccounts.form.fromDate", "تاريخ البداية")}
                        disabled={editMode > 1}
                        component={FormDatePicker}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field
                        name="thruDate"
                        id="thruDate"
                        label={getTranslatedLabel("accounting.billingAccounts.form.thruDate", "تاريخ النهاية")}
                        component={FormDatePicker}
                      />
                    </Grid>
                    <Grid item xs={12}>
                      <Typography variant="body1" sx={{ fontWeight: "bold", mt: 2 }}>
                        {getTranslatedLabel("accounting.billingAccounts.form.availableBalance", "الرصيد المتاح")}
                      </Typography>
                      <Typography variant="h5" sx={{color: "red", fontWeight: "bold"}}>
                        {formatCurrency(account?.availableBalance || 0)}
                      </Typography>
                    </Grid>
                    <Grid item xs={4}>
                      <Field
                        name="description"
                        id="description"
                        label={getTranslatedLabel("accounting.billingAccounts.form.description", "الوصف")}
                        component={FormTextArea}
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
                  {getTranslatedLabel("general.save", "حفظ")}
                </Button>

                <Button
                  variant="contained"
                  type={"button"}
                  color="error"
                  onClick={onClose}
                >
                  {getTranslatedLabel("general.back", "رجوع")}
                </Button>
              </div>
            </FormElement>
          )}
        />
      </Paper>
    </>
  );
};

export default BillingAccountForm;
