import React from 'react'
import { useAppSelector, useFetchOrganizationGlChartOfAccountsQuery, useFetchVarianceReasonsQuery } from '../../../../app/store/configureStore';
import { State } from '@progress/kendo-data-query';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { Button, Grid } from '@mui/material';
import { MemoizedFormDropDownList } from '../../../../app/common/form/MemoizedFormDropDownList';

interface GlVarianceReasonFormProps {
  selectedAccountingCompanyId: string | undefined;
  onSubmit: (values: any) => void;
}

const GlVarianceReasonForm = ({onSubmit, selectedAccountingCompanyId}: GlVarianceReasonFormProps) => {
  
  const [dataState, setDataState] = React.useState<State>({ skip: 0 });

  const {data: varianceReasons} = useFetchVarianceReasonsQuery(undefined)

  const {data: glAccounts} = useFetchOrganizationGlChartOfAccountsQuery(
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

  
  return (
    <Form
      onSubmit={(values) => onSubmit(values)}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2}  alignItems={"flex-end"}>
              <Grid item xs={5}>
                <Field
                  name={"varianceReasonId"}
                  id={"varianceReasonId"}
                  label={"Variance Reason"}
                  component={MemoizedFormDropDownList}
                  data={varianceReasons ? varianceReasons : []}
                  dataItemKey={"varianceReasonId"}
                  textField={"description"}
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
                />
              </Grid>
              <Grid item  xs={2}>
                <Button
                    variant="contained"
                    type={"submit"}
                    color="success"
                    disabled={
                        !formRenderProps.valueGetter("glAccountId") &&
                        !formRenderProps.valueGetter("glAccountId")
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

export default GlVarianceReasonForm