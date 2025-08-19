import React from 'react'
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { Button, Grid } from '@mui/material';
import { MemoizedFormDropDownList } from '../../../../app/common/form/MemoizedFormDropDownList';
import { useFetchOrganizationGlChartOfAccountsQuery } from '../../../../app/store/apis';
import { useAppSelector, useFetchInvoiceItemTypesQuery } from '../../../../app/store/configureStore';
import { State } from '@progress/kendo-data-query';
interface SalesInvoiceAccountFormProps {
    selectedAccountingCompanyId: string | undefined
}
const SalesInvoiceAccountForm = ({selectedAccountingCompanyId}: SalesInvoiceAccountFormProps) => {
    let accountDataState = {
        "filter": {
            "logic":
                "and",
            "filters":
                [
                    {
                        "field": "glAccountClassId",
                        "operator": "eq",
                        "value": "REVENUE"
                    }
                ]
        },
        "skip": 0,
    };
    let typesDataState = {
      "filter": {
          "logic": "or",
          "filters": [
              {
                  "logic": "or",
                  "filters": [
                      {
                          "field": "parentTypeId",
                          "operator": "contains",
                          "value": "SINV"
                      },
                      {
                          "field": "parentTypeId",
                          "operator": "contains",
                          "value": "SINVOICE"
                      }
                  ]
              },
              {
                  "logic": "and",
                  "filters": [
                      {
                          "field": "description",
                          "operator": "contains",
                          "value": "(Sales)"
                      },
                      
                  ]
              }
          ]
      },
      "skip": 0,
      "take": 30
    };
    const { data: salesInvoiceTypes } = useFetchInvoiceItemTypesQuery({...typesDataState});
    const [dataState, setDataState] = React.useState<State>(accountDataState);
    const {data: glAccounts} = useFetchOrganizationGlChartOfAccountsQuery(
        { companyId: selectedAccountingCompanyId, dataState },
      );

    // Map over the glAccounts to create a new property that concatenates the account name and account ID
    const formattedGlAccounts = glAccounts
        ? glAccounts.data.map((account: any) => ({
            ...account,
            accountDisplay: `${account.accountName} (${account.glAccountId})`,
        }))
        : [];

    return (
    <>
      <Form
      onSubmit={(values) => console.log(values)}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2}   alignItems={"flex-end"}>
              <Grid item xs={5}>
                <Field
                  name={"invoiceItemTypeId"}
                  id={"invoiceItemTypeId"}
                  label={"Invoice Item Type"}
                  component={MemoizedFormDropDownList}
                  data={salesInvoiceTypes ? salesInvoiceTypes.data : []}
                  dataItemKey={"invoiceItemTypeId"}
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
                            !formRenderProps.valueGetter("glAccountId") ||
                            !formRenderProps.valueGetter("invoiceItemTypeId")
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
    </>
  );
}

export default SalesInvoiceAccountForm