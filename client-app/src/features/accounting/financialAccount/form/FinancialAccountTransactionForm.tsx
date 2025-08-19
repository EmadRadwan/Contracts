import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { useEffect, useState } from "react";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import {
  useCreateFinAccountTransactionMutation,
  useFetchFinAccountTransStatusesQuery,
  useFetchFinAccountTransTypesQuery,
} from "../../../../app/store/apis/accounting/financialAccountsApi";
import { requiredValidator } from "../../../../app/common/form/Validators";
import {
  useFetchInternalAccountingOrganizationsLovQuery,
  useFetchOrgChartOfAccountsLovQuery,
} from "../../../../app/store/apis";
import { FormDropDownTreeGlAccount } from "../../../../app/common/form/FormDropDownTreeGlAccount";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { useAppSelector } from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import FinancialAccountMenu from "../menu/FinancialAccountMenu";
import { toast } from "react-toastify";
import { FormComboBoxVirtualAllParties } from "../../../../app/common/form/FormComboBoxVirtualAllParties";

interface Props {
  selectedTransaction?: any;
  editMode: number;
  onClose: () => void;
}

const FinancialAccountTransactionForm = ({
  selectedTransaction,
  editMode,
  onClose,
}: Props) => {
  const { selectedFinancialAccount } = useAppSelector(
    (state) => state.accountingSharedUi
  );
  const [formKey, setFormKey] = useState<number>(Math.random());
  const { data: finAccountTransTypes } =
    useFetchFinAccountTransTypesQuery(undefined);
  const { data: companies } =
    useFetchInternalAccountingOrganizationsLovQuery(undefined);
  const { data: glAccounts } = useFetchOrgChartOfAccountsLovQuery(undefined);
  const { data: finAccountTransStatuses } =
    useFetchFinAccountTransStatusesQuery(undefined);
  const [transaction, setTransaction] = useState<any>()
  const [createTrans, {isLoading}] = useCreateFinAccountTransactionMutation()

  useEffect(() => {
    if (selectedTransaction) {
      setTransaction(selectedTransaction)
    } else {
      setTransaction({
        statusId: "FINACT_TRNS_CREATED",
        finAccountTransTypeId: "ADJUSTMENT",
        finAccountId: selectedFinancialAccount?.finAccountId,
        transactionDate: new Date(),
        entryDate: new Date(),
        amount: 0
      })
    }
    setFormKey(Math.random())
  }, [selectedTransaction])


  const onSubmit = async (data: any) => {
    console.log(data)
    const {partyId, postToGlAccountId, ...rest} = data
    const transactionBody = {
      ...rest,
      finAccountId: transaction?.finAccountId,
      performedByPartyId: partyId || "",
      glAccountId: postToGlAccountId
    }
    try {
      await createTrans(transactionBody)
        .unwrap()
        .then(res => {
          console.log(res)
          toast.success("Transaction created successfully")
        })
        .catch(e => {
          console.error(e)
          toast.error("Something went wrong.")
        })
    } catch (e) {
      console.error(e)
    }
  }


  return (
    <>
      <AccountingMenu selectedMenuItem={"/financialAcc"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <FinancialAccountMenu selectedMenuItem="financialAccounts/transactions" />
        <Grid container spacing={2}>
          <Grid item xs={7}>
            <Box display="flex" justifyContent="space-between">
              <Typography
                sx={{ p: 2 }}
                color={editMode > 1 ? "black" : "green"}
                variant="h4"
              >
                {editMode === 1
                  ? "New Financial Account Transacation"
                  : `Transaction: ${selectedTransaction?.finAccountTransId} for Account: ${selectedFinancialAccount?.finAccountName}`}
              </Typography>
            </Box>
          </Grid>
        </Grid>
        <Grid container ml={2}>
          <Form
            onSubmit={(values) => onSubmit(values)}
            key={formKey.toString()}
            initialValues={transaction}
            render={(formRenderProps) => (
              <FormElement>
                <fieldset className={"k-form-fieldset"}>
                  <Grid container spacing={2}>
                    <Grid item container xs={12} spacing={2}>
                      <Grid item xs={4}>
                        <Field
                          name="finAccountTransTypeId"
                          id="finAccountTransTypeId"
                          label="Fin Account Trans Type"
                          component={MemoizedFormDropDownList2}
                          data={finAccountTransTypes ?? []}
                          dataItemKey="finAccountTransTypeId"
                          textField="finAccountTransTypeDescription"
                          validator={requiredValidator}
                        />
                      </Grid>
                      <Grid item xs={4}>
                        <Field
                          name="statusId"
                          id="statusId"
                          label="Fin Account Status"
                          component={MemoizedFormDropDownList2}
                          data={finAccountTransStatuses ?? []}
                          dataItemKey="finAccountTransStatusId"
                          textField="finAccountTransStatusDescription"
                          validator={requiredValidator}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Field
                          name="partyId"
                          id="partyId"
                          label="Party"
                          component={FormComboBoxVirtualAllParties}
                          // data={companies ?? []}
                          // dataItemKey="partyId"
                          // textField="partyName"
                        />
                      </Grid>
                    </Grid>
                    <Grid item container spacing={1} flexDirection={"row"}>
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
                          // validator={requiredValidator}
                        />
                      </Grid>

                      <Grid item xs={2}>
                        <Field
                          name="amount"
                          id="amount"
                          label="Amount"
                          component={FormNumericTextBox}
                          disabled={editMode > 1}
                          min={0}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Field
                          name="transactionDate"
                          id="transactionDate"
                          label="Transaction Date"
                          component={FormDatePicker}
                          validator={requiredValidator}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Field
                          name="entryDate"
                          id="entryDate"
                          label="Entry Date"
                          component={FormDatePicker}
                        />
                      </Grid>
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

export default FinancialAccountTransactionForm;
