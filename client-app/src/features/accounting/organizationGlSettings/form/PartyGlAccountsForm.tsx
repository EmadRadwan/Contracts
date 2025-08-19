import { State } from '@progress/kendo-data-query';
import React from 'react'
import {  useFetchGlAccountTypesQuery, useFetchOrganizationGlChartOfAccountsQuery, useFetchRolesQuery } from '../../../../app/store/configureStore';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { Button, Grid } from '@mui/material';
import { MemoizedFormDropDownList } from '../../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../../app/common/form/Validators';
import { FormComboBoxVirtualParty } from '../../../../app/common/form/FormComboBoxVirtualParty';

interface Props {
    selectedAccountingCompanyId: string | undefined;
    onSubmit: (values: any) => void;
}

const PartyGlAccountsForm = ({selectedAccountingCompanyId, onSubmit}: Props) => {
    const [dataState, setDataState] = React.useState<State>({ skip: 0 });

    const { data: glAccountTypes } = useFetchGlAccountTypesQuery(undefined);
    const {data: roles} = useFetchRolesQuery(undefined)
    const { data: glAccounts } = useFetchOrganizationGlChartOfAccountsQuery(
      { companyId: selectedAccountingCompanyId, dataState },
      { skip: selectedAccountingCompanyId === undefined }
    );
    return (
      <Form
        onSubmit={(values) => onSubmit(values)}
        render={(formRenderProps) => (
          <FormElement>
            <fieldset className={"k-form-fieldset"}>
              <Grid container spacing={1} alignItems={"flex-end"}>
                <Grid item xs={5}>
                  <Field
                    name={"partyId"}
                    id={"partyId"}
                    label={"Party"}
                    component={FormComboBoxVirtualParty}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={5}>
                  <Field
                    name={"roleTypeId"}
                    id={"roleTypeId"}
                    label={"Role"}
                    component={MemoizedFormDropDownList}
                    data={roles ?? []}
                    dataItemKey={"roleTypeId"}
                    textField={"roleName"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={5}>
                  <Field
                    name={"glAccountTypeId"}
                    id={"glAccountTypeId"}
                    label={"Gl Account Type"}
                    component={MemoizedFormDropDownList}
                    data={glAccountTypes ? glAccountTypes : []}
                    dataItemKey={"glAccountTypeId"}
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
                          !formRenderProps.valueGetter("glAccountTypeId") ||
                          !formRenderProps.valueGetter("partyId") ||
                          !formRenderProps.valueGetter("roleId")
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

export default PartyGlAccountsForm