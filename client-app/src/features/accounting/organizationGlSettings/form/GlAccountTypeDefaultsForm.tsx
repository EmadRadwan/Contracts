import React from "react";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Grid } from "@mui/material";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import {
  useFetchGlAccountTypesQuery,
  useFetchOrganizationGlChartOfAccountsQuery,
} from "../../../../app/store/configureStore";
import { State } from "@progress/kendo-data-query";
import Button from "@mui/material/Button";

interface GlAccountTypeDefaultsFormProps {
  selectedAccountingCompanyId: string | undefined;
  onSubmit: (values: any) => void;
}

const GlAccountTypeDefaultsForm = ({
                                     selectedAccountingCompanyId,
                                     onSubmit,
                                   }: GlAccountTypeDefaultsFormProps) => {
  const [dataState, setDataState] = React.useState<State>({ skip: 0 });

  const { data: glAccountTypes } = useFetchGlAccountTypesQuery(undefined);
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

  console.log("glAccountTypes", glAccountTypes);
  console.log("glAccounts", glAccounts);

  return (
      <Form
          onSubmit={(values) => onSubmit(values)}
          render={(formRenderProps) => (
              <FormElement>
                <fieldset className={"k-form-fieldset"}>
                  <Grid container spacing={2} alignItems={"flex-end"}>
                    <Grid item xs={5}>
                      <Field
                          name={"glAccountTypeId"}
                          id={"glAccountTypeId"}
                          label={"Gl Account Type"}
                          component={MemoizedFormDropDownList}
                          data={glAccountTypes ? glAccountTypes : []}
                          dataItemKey={"glAccountTypeId"}
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
                    <Grid item xs={2}>
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
  );
};

export default GlAccountTypeDefaultsForm;
