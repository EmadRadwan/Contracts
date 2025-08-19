import { Button, Grid } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import React from "react";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { State } from "@progress/kendo-data-query";
import {
  useFetchGlAccountTypesQuery,
  useFetchOrganizationGlChartOfAccountsQuery,
  useFetchProductCategoriesQuery,
} from "../../../../app/store/apis";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { FormDropDownTreeProductCategory } from "../../../../app/common/form/FormDropDownTreeProductCategory";

interface Props {
  selectedAccountingCompanyId: string | undefined;
  onSubmit: (values: any) => void;
}

const ProductCategoryGlAccountsForm = ({
  selectedAccountingCompanyId,
  onSubmit,
}: Props) => {
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
  
  const {data: productCategories} = useFetchProductCategoriesQuery(undefined);
  return (
    <Form
      onSubmit={(values) => onSubmit(values)}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={1} alignItems={"flex-end"}>
              <Grid item xs={3}>
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
              <Grid item xs={4}>
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
              <Grid item xs={3}>
                {productCategories && <Field
                  name={"productCategoryId"}
                  id={"productCategoryId"}
                  label={"Product Category"}
                  component={FormDropDownTreeProductCategory}
                  dataItemKey={"productCategoryId"}
                  textField={"text"}
                  validator={requiredValidator}
                  selectField={"selected"}
                  expandField={"expanded"}
                  data={productCategories ? productCategories : []}
                />}
              </Grid>
              <Grid item xs={2}>
                <Button
                  variant="contained"
                  type={"submit"}
                  color="success"
                  disabled={
                    !formRenderProps.valueGetter("glAccountId") ||
                    !formRenderProps.valueGetter("glAccountTypeId") ||
                    !formRenderProps.valueGetter("productId")
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

export default ProductCategoryGlAccountsForm;
