import React, { useState } from 'react'
import { useAppSelector, useFetchAllPaymentMethodTypesQuery } from '../../../../app/store/configureStore';
import { router } from '../../../../app/router/Routes';
import AccountingMenu from '../../invoice/menu/AccountingMenu';
import { Button, Grid, Paper, Typography } from '@mui/material';
import FinancialAccountMenu from '../menu/FinancialAccountMenu';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { MemoizedFormDropDownList2 } from '../../../../app/common/form/MemoizedFormDropDownList2';
import FormDatePicker from '../../../../app/common/form/FormDatePicker';
import { useFetchFinAccountWidthrawDepositsQuery } from '../../../../app/store/apis/accounting/financialAccountsApi';

const FinancialAccountDepositWithdrawal = () => {
  const [formValues, setFormValues] = useState<any>({});

  const {data: depositsWidthrawlsList, isSuccess, refetch} = useFetchFinAccountWidthrawDepositsQuery({...formValues})
  const {data: paymentMethodTypes} = useFetchAllPaymentMethodTypesQuery(undefined)

  const { selectedFinancialAccount } = useAppSelector(
        (state) => state.accountingSharedUi
      );
      if (!selectedFinancialAccount) {
        router.navigate("/financialAccounts");
        return;
      }

      const onSearch = (data: any) => {
        refetch()
      }
  return (
    <>
      <AccountingMenu selectedMenuItem="financialAcc" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <FinancialAccountMenu selectedMenuItem="financialAccounts/depositWithdraw" />
        <Grid container ml={2} mt={2}>
          <Grid item xs={12}>
            <Typography variant="h5" fontWeight="bold">
              Deposits/Withdrawals for Account:{" "}
              {selectedFinancialAccount?.finAccountName}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Form
              onSubmitClick={(values) => onSearch(values)}
              render={() => (
                <FormElement>
                  <fieldset className={"k-form-fieldset"}>
                    <Grid container spacing={2}>
                      {/* <Grid container item xs={12} spacing={2}> */}
                        <Grid item xs={3}>
                          <Field
                            name="paymentMethodTypeId"
                            id="paymentMethodTypeId"
                            label="Payment Method Type"
                            component={MemoizedFormDropDownList2}
                            data={paymentMethodTypes ?? []}
                            dataItemKey={"paymentMethodTypeId"}
                            textField="description"
                          />
                        </Grid>
                        <Grid item xs={3}>
                          <Field
                            name="statusId"
                            id="statusId"
                            label="Status"
                            component={MemoizedFormDropDownList2}
                            data={[]}
                            dataItemKey={"finAccountTransStatusId"}
                            textField="finAccountTransStatusDescription"
                          />
                        </Grid>
                      {/* </Grid> */}
                      {/* <Grid container item xs={12} spacing={2} mr={2}> */}
                        <Grid item xs={2}>
                          <Field
                            name="fromDate"
                            id="fromDate"
                            label="From date"
                            component={FormDatePicker}
                          />
                        </Grid>
                        <Grid item xs={2}>
                          <Field
                            name="thruDate"
                            id="thruDate"
                            label="Thru date"
                            component={FormDatePicker}
                          />
                        </Grid>
                      {/* </Grid> */}
                    </Grid>
                  </fieldset>
                  <div className="k-form-buttons">
                    <Button variant="contained" type={"submit"} color="success">
                      Find
                    </Button>
                  </div>
                </FormElement>
              )}
            />
          </Grid>
        </Grid>
      </Paper>
    </>
  )
}

export default FinancialAccountDepositWithdrawal