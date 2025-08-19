import { useEffect, useState } from "react";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Button, Grid, Paper, Typography } from "@mui/material";
import FinancialAccountMenu from "../menu/FinancialAccountMenu";
import {
  useFetchFinAccountTransAndTotalsQuery,
  useFetchFinAccountTransStatusesQuery,
  useFetchFinAccountTransTypesQuery,
  useLazyFetchFinAccountTransAndTotalsQuery,
} from "../../../../app/store/apis/accounting/financialAccountsApi";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { useAppSelector } from "../../../../app/store/configureStore";
import { router } from "../../../../app/router/Routes";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GRID_COL_INDEX_ATTRIBUTE,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { formatCurrency, handleDatesArray } from "../../../../app/util/utils";
import FinancialAccountTransactionForm from "../form/FinancialAccountTransactionForm";

const FinancialAccountTransactions = () => {
  const { selectedFinancialAccount } = useAppSelector(
    (state) => state.accountingSharedUi
  );
  const [formKey, setFormKey] = useState<number>(Math.random())
  if (!selectedFinancialAccount) {
    router.navigate("/financialAccounts");
    return;
  }
  const [editMode, setEditMode] = useState<number>(0)
  const [formValues, setFormValues] = useState<any>({
    finAccountId: selectedFinancialAccount?.finAccountId,
  });
  const [getTransactions, {
    data: finAccountTxns,
    isSuccess
  }] = useLazyFetchFinAccountTransAndTotalsQuery({
    ...formValues,
  });
  const { data: transTypes } = useFetchFinAccountTransTypesQuery(undefined);
  const { data: transactionStatuses } =
    useFetchFinAccountTransStatusesQuery(undefined);
  const [txnsList, setTxnsList] = useState([]);

  useEffect(() => {
    if (finAccountTxns) {
      setTxnsList(handleDatesArray(finAccountTxns.finAccountTransList));
    }
  }, [finAccountTxns]);

  const onSearch = (data: any) => {
    const { values } = data;
    const {
      thruEntryDate,
      thruTransactionDate,
      fromEntryDate,
      fromTransactionDate,
      ...rest
    } = values;
    getTransactions({
      finAccountId: selectedFinancialAccount?.finAccountId,
      thruEntryDate: thruEntryDate ? new Date(thruEntryDate).toISOString() : null,
      fromEntryDate: fromEntryDate ? new Date(fromEntryDate).toISOString() : null,
      thruTransactionDate: thruTransactionDate
        ? new Date(thruTransactionDate).toISOString()
        : null,
      fromTransactionDate: fromTransactionDate
        ? new Date(fromTransactionDate).toISOString()
        : null,
      ...rest,
    });
  };

  const onResetForm = () => {
    setFormKey(Math.random())
    getTransactions(undefined)
  }

  if (editMode > 0) {
    return <FinancialAccountTransactionForm editMode={editMode} onClose={() => setEditMode(0)} />
  }

  return (
    <>
      <AccountingMenu selectedMenuItem="financialAcc" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <FinancialAccountMenu selectedMenuItem="financialAccounts/transactions" />
        <Grid container ml={2} mt={2}>
          <Grid item xs={12}>
            <Typography variant="h5" fontWeight="bold">
              Find transactions for Account:{" "}
              {selectedFinancialAccount?.finAccountName}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <Form
              onSubmitClick={(values) => onSearch(values)}
              key={formKey}
              render={() => (
                <FormElement>
                  <fieldset className={"k-form-fieldset"}>
                    <Grid container spacing={2}>
                      <Grid container item xs={12} spacing={2}>
                        <Grid item xs={3}>
                          <Field
                            name="finAccountTransTypeId"
                            id="finAccountTransTypeId"
                            label="Transaction Type"
                            component={MemoizedFormDropDownList2}
                            data={transTypes ?? []}
                            dataItemKey={"finAccountTransTypeId"}
                            textField="finAccountTransTypeDescription"
                          />
                        </Grid>
                        <Grid item xs={3}>
                          <Field
                            name="statusId"
                            id="statusId"
                            label="Status"
                            component={MemoizedFormDropDownList2}
                            data={transactionStatuses ?? []}
                            dataItemKey={"finAccountTransStatusId"}
                            textField="finAccountTransStatusDescription"
                          />
                        </Grid>
                      </Grid>
                      <Grid container item xs={12} spacing={2} mr={2}>
                        <Grid item xs={3}>
                          <Field
                            name="fromTransactionDate"
                            id="fromTransactionDate"
                            label="From transaction date"
                            component={FormDatePicker}
                          />
                        </Grid>
                        <Grid item xs={3}>
                          <Field
                            name="thruTransactionDate"
                            id="thruTransactionDate"
                            label="Thru transaction date"
                            component={FormDatePicker}
                          />
                        </Grid>
                        <Grid item xs={3}>
                          <Field
                            name="fromEntryDate"
                            id="fromEntryDate"
                            label="From entry date"
                            component={FormDatePicker}
                          />
                        </Grid>
                        <Grid item xs={3}>
                          <Field
                            name="thruEntryDate"
                            id="thruEntryDate"
                            label="Thru entry date"
                            component={FormDatePicker}
                          />
                        </Grid>
                      </Grid>
                    </Grid>
                  </fieldset>
                  <div className="k-form-buttons">
                    <Button variant="contained" type={"submit"} color="success">
                      Find
                    </Button>
                    <Button variant="contained" type={"button"} onClick={onResetForm} color="info">
                      Reset
                    </Button>
                  </div>
                </FormElement>
              )}
            />
          </Grid>
          {isSuccess && (
            <>
              <Grid container item xs={12} spacing={2} mt={2}>
                <Grid item xs={3}>
                  <Typography variant="body1" fontWeight={"bold"}>
                    Grand Total
                  </Typography>
                  <Typography variant="body1">
                    {formatCurrency(finAccountTxns?.grandTotal || 0)} (
                    {finAccountTxns?.searchedNumberOfRecords || 0}{" "}
                    Transaction(s))
                  </Typography>
                </Grid>
                <Grid item xs={3}>
                  <Typography variant="body1" fontWeight={"bold"}>
                    Created Grand Total
                  </Typography>
                  <Typography variant="body1">
                    {formatCurrency(finAccountTxns?.createdGrandTotal || 0)} (
                    {finAccountTxns?.totalCreatedTransactions || 0}{" "}
                    Transaction(s))
                  </Typography>
                </Grid>
                <Grid item xs={3}>
                  <Typography variant="body1" fontWeight={"bold"}>
                    Approved Grand Total
                  </Typography>
                  <Typography variant="body1">
                    {formatCurrency(finAccountTxns?.approvedGrandTotal || 0)} (
                    {finAccountTxns?.totalApprovedTransactions || 0}{" "}
                    Transaction(s))
                  </Typography>
                </Grid>
                <Grid item xs={3}>
                  <Typography variant="body1" fontWeight={"bold"}>
                    Created/Approved Grand Total
                  </Typography>
                  <Typography variant="body1">
                    {formatCurrency(
                      finAccountTxns?.createdApprovedGrandTotal || 0
                    )}{" "}
                    ({finAccountTxns?.totalCreatedApprovedTransactions || 0}{" "}
                    Transaction(s))
                  </Typography>
                </Grid>
              </Grid>
              <Grid item xs={12} mt={2}>
                <KendoGrid
                  style={{ height: "35vh", width: "95%" }}
                  data={txnsList}
                >
                  <GridToolbar>
                    <Grid container>
                      <Grid item>
                        <Button variant="outlined" color="secondary" onClick={() => setEditMode(1)}>
                          Create Transaction
                        </Button>
                      </Grid>
                    </Grid>
                  </GridToolbar>
                  <Column field="finAccountTransId" title="Transaction Id" />
                  <Column
                    field="finAccountTransTypeDescription"
                    title="Transaction Type"
                  />
                  <Column
                    field="transactionDate"
                    title="Transaction Date"
                    format="{0: dd/MM/yyyy}"
                  />
                  <Column
                    field="entryDate"
                    title="Entry Date"
                    format="{0: dd/MM/yyyy}"
                  />
                  <Column field="amount" title="Amount" format="{0: n2}" />
                  <Column field="paymentId" title="Payment Id" />
                  <Column field="statusDescription" title="Status" />
                  {/* <Column field="paymentTypeDescription" title="Payment Type" format="{0: n2}" />
                <Column field="paymentMethodTypeDescription" title="Payment Method Type" format="{0: n2}" /> */}
                </KendoGrid>
              </Grid>
            </>
          )}
        </Grid>
      </Paper>
    </>
  );
};

export default FinancialAccountTransactions;
