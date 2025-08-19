import { State } from '@progress/kendo-data-query';
import React from 'react'
import { useFetchOrganizationGlChartOfAccountsQuery } from '../../../../app/store/configureStore';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { Button, Grid } from '@mui/material';
import { MemoizedFormDropDownList } from '../../../../app/common/form/MemoizedFormDropDownList';
import { MemoizedFormDropDownList2 } from '../../../../app/common/form/MemoizedFormDropDownList2';
import { requiredValidator } from '../../../../app/common/form/Validators';
import { useFetchFixedAssetTypesQuery, useFetchOrganizationGlAccountsByClassQuery } from '../../../../app/store/apis';

interface Props {
    selectedAccountingCompanyId: string | undefined;
    onSubmit: (values: any) => void;
}

const FixedAssetTypeGlMappingsForm = ({selectedAccountingCompanyId, onSubmit}: Props) => {

    const { data: fixedAssetTypes } = useFetchFixedAssetTypesQuery(undefined);

    const {data: assetGlAccounts} = useFetchOrganizationGlAccountsByClassQuery({
        companyId: selectedAccountingCompanyId!, accountClass: "LONGTERM_ASSET"
    }, {
        skip: !selectedAccountingCompanyId
    })
    const {data: accumDepAccounts} = useFetchOrganizationGlAccountsByClassQuery({
        companyId: selectedAccountingCompanyId!, accountClass: "ACCUM_DEPRECIATION"
    }, {
        skip: !selectedAccountingCompanyId
    })
    const {data: depreciationAccounts} = useFetchOrganizationGlAccountsByClassQuery({
        companyId: selectedAccountingCompanyId!, accountClass: "DEPRECIATION"
    }, {
        skip: !selectedAccountingCompanyId
    })
    const {data: profitAccounts} = useFetchOrganizationGlAccountsByClassQuery({
        companyId: selectedAccountingCompanyId!, accountClass: "CASH_INCOME"
    }, {
        skip: !selectedAccountingCompanyId
    })
    const {data: lossAccounts} = useFetchOrganizationGlAccountsByClassQuery({
        companyId: selectedAccountingCompanyId!, accountClass: "SGA_EXPENSE"
    }, {
        skip: !selectedAccountingCompanyId
    })

    console.log(assetGlAccounts)
    return (
      <Form
        onSubmit={(values) => onSubmit(values)}
        render={(formRenderProps) => (
          <FormElement>
            <fieldset className={"k-form-fieldset"}>
              <Grid container spacing={1} alignItems={"flex-end"}>
                <Grid item xs={6}>
                  <Field
                    name={"assetGlAccountId"}
                    id={"assetGlAccountId"}
                    label={"Asset Gl Account"}
                    component={MemoizedFormDropDownList2}
                    data={assetGlAccounts ? assetGlAccounts : []}
                    dataItemKey={"glAccountId"}
                    textField={"accountName"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={6}>
                  <Field
                    name={"accumulatedDepreciationGlAccountId"}
                    id={"accumulatedDepreciationGlAccountId"}
                    label={"Accumulated Depreciation Gl Account"}
                    component={MemoizedFormDropDownList2}
                    data={accumDepAccounts ? accumDepAccounts : []}
                    dataItemKey={"glAccountId"}
                    textField={"accountName"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={6}>
                  <Field
                    name={"depreciationGlAccountId"}
                    id={"depreciationGlAccountId"}
                    label={"Depreciation Gl Account"}
                    component={MemoizedFormDropDownList2}
                    data={depreciationAccounts ? depreciationAccounts : []}
                    dataItemKey={"glAccountId"}
                    textField={"accountName"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={6}>
                  <Field
                    name={"profitGlAccountId"}
                    id={"profitGlAccountId"}
                    label={"Profit Gl Account"}
                    component={MemoizedFormDropDownList2}
                    data={profitAccounts ? profitAccounts : []}
                    dataItemKey={"glAccountId"}
                    textField={"accountName"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={6}>
                  <Field
                    name={"lossGlAccountId"}
                    id={"lossGlAccountId"}
                    label={"Loss Gl Account"}
                    component={MemoizedFormDropDownList2}
                    data={lossAccounts ? lossAccounts : []}
                    dataItemKey={"glAccountId"}
                    textField={"accountName"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid container spacing={1} item xs={12}>
                    <Grid item xs={6}>
                      <Field
                        name={"fixedAssetTypeId"}
                        id={"fixedAssetTypeId"}
                        label={"Fixed Asset Type"}
                        component={MemoizedFormDropDownList}
                        data={fixedAssetTypes ? fixedAssetTypes : []}
                        dataItemKey={"fixedAssetTypeId"}
                        textField={"cleanName"}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={6}>
                      <Field
                        name={"fixedAssetId"}
                        id={"fixedAssetId"}
                        label={"Fixed Asset"}
                        component={MemoizedFormDropDownList}
                        data={[]}
                        dataItemKey={"fixedAssetId"}
                        textField={"cleanName"}
                        validator={requiredValidator}
                      />
                    </Grid>
                </Grid>
                <Grid item  xs={2}>
                  <Button
                      variant="contained"
                      type={"submit"}
                      color="success"
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

export default FixedAssetTypeGlMappingsForm