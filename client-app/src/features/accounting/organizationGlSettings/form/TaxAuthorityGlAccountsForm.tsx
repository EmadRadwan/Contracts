import { State } from '@progress/kendo-data-query';
import React from 'react'
import { useFetchFinAccountTypesQuery, useFetchOrganizationGlChartOfAccountsQuery } from '../../../../app/store/configureStore';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { Button, Grid } from '@mui/material';
import { MemoizedFormDropDownList } from '../../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../../app/common/form/Validators';
import { useFetchTaxAuthoritiesListQuery } from '../../../../app/store/apis';

interface Props {
    selectedAccountingCompanyId: string | undefined;
    onSubmit: (values: any) => void;
}

const TaxAuthorityGlAccountsForm = ({selectedAccountingCompanyId, onSubmit}: Props) => {
    const [dataState, setDataState] = React.useState<State>({ skip: 0 });

    const { data: taxAuths } = useFetchTaxAuthoritiesListQuery(undefined);
    const { data: glAccounts } = useFetchOrganizationGlChartOfAccountsQuery(
      { companyId: selectedAccountingCompanyId, dataState },
      { skip: selectedAccountingCompanyId === undefined }
    );
    console.log(taxAuths)
    return (
      <Form
        onSubmit={(values) => onSubmit(values)}
        render={(formRenderProps) => (
          <FormElement>
            <fieldset className={"k-form-fieldset"}>
              <Grid container spacing={1} alignItems={"flex-end"}>
                <Grid item xs={5}>
                  <Field
                    name={"taxAuthPartyId"}
                    id={"taxAuthPartyId"}
                    label={"Tax Authority"}
                    component={MemoizedFormDropDownList}
                    data={taxAuths ? taxAuths : []}
                    dataItemKey={"taxAuthPartyId"}
                    textField={"taxAuthPartyName"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={5}>
                  <Field
                    name={"glAccountId"}
                    id={"glAccountId"}
                    label={"Gl Account"}
                    component={MemoizedFormDropDownList}
                    data={glAccounts ? glAccounts!.data : []}
                    dataItemKey={"accountCode"}
                    textField={"accountName"}
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
                          !formRenderProps.valueGetter("finAccountTypeId") 
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

export default TaxAuthorityGlAccountsForm