import { State } from '@progress/kendo-data-query';
import React, { useEffect } from 'react'
import { useFetchAllPaymentMethodTypesQuery, useFetchOrganizationGlChartOfAccountsQuery, useFetchPaymentTypesQuery } from '../../../../app/store/configureStore';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { Button, Grid } from '@mui/material';
import { MemoizedFormDropDownList } from '../../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../../app/common/form/Validators';


interface Props {
    selectedAccountingCompanyId: string | undefined;
    onSubmit: (values: any) => void;
}

const PaymentTypeGlAccountsForm = ({selectedAccountingCompanyId, onSubmit}: Props) => {
    const [dataState, setDataState] = React.useState<State>({ skip: 0 });

    const { data: paymentTypes } = useFetchPaymentTypesQuery(undefined);
    const { data: glAccounts } = useFetchOrganizationGlChartOfAccountsQuery(
      { companyId: selectedAccountingCompanyId, dataState },
      { skip: selectedAccountingCompanyId === undefined }
    );

    // Map over the glAccounts to create a new property that concatenates the account name and account ID
    const formattedGlAccounts = glAccounts
        ? glAccounts.data.map((account: any) => ({
            ...account,
            accountDisplay: `${account.accountName} (${account.glAccountId})`,
        }))
        : [];
    
    console.log(paymentTypes)
    return (
      <Form
        onSubmit={(values) => onSubmit(values)}
        render={(formRenderProps) => (
          <FormElement>
            <fieldset className={"k-form-fieldset"}>
              <Grid container spacing={1} alignItems={"flex-end"}>
                <Grid item xs={5}>
                  <Field
                    name={"paymentTypeId"}
                    id={"paymentTypeId"}
                    label={"Payment Type"}
                    component={MemoizedFormDropDownList}
                    data={paymentTypes ?? []}
                    dataItemKey={"paymentTypeId"}
                    textField={"description"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={5}>
                  <Field
                    name={"glAccountId"}
                    id={"glAccountId"}
                    label={"Gl Account"}
                    component={MemoizedFormDropDownList}
                    data={formattedGlAccounts}
                    dataItemKey={"accountCode"}
                    textField={"accountDisplay"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item  xs={2}>
                  <Button
                      variant="contained"
                      type={"submit"}
                      color="success"
                      disabled={
                        !formRenderProps.valueGetter("glAccountId") ||
                        !formRenderProps.valueGetter("paymentTypeId") 
                    }
                  >
                    Save
                  </Button>
                </Grid>
              </Grid>
              
            </fieldset>
          </FormElement>
        )}
      ></Form>
    )
}

export default PaymentTypeGlAccountsForm