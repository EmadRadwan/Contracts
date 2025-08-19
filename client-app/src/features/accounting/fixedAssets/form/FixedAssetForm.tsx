import React, { useState } from "react";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { FixedAsset } from "../../../../app/models/accounting/fixedAsset";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import FormInput from "../../../../app/common/form/FormInput";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { useFetchCurrenciesQuery, useFetchPurchaseCostCurrenciesQuery } from "../../../../app/store/configureStore";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { handleDatesObject } from "../../../../app/util/utils";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import CreateFixedAssetMenu from "../CreateFixedAssetMenu";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import { useFetchFixedAssetTypesQuery } from "../../../../app/store/apis";

interface Props {
  editMode: number;
  cancelEdit: () => void;
  fixedAsset?: FixedAsset | undefined;
}

const FixedAssetForm = ({ editMode, fixedAsset, cancelEdit }: Props) => {
  // console.log(fixedAsset, editMode);
  
  const { data: currencies } = useFetchPurchaseCostCurrenciesQuery(undefined);
  const {data: fixedAssetTypes} = useFetchFixedAssetTypesQuery(undefined)
  // console.log(fixedAssetTypes)
  const [buttonFlag, setButtonFlag] = useState(false);
  return (
    <>
      <AccountingMenu selectedMenuItem="fixedAssets" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container spacing={2}>
          <Grid item xs={5}>
            {
              <Box display="flex" justifyContent="space-between">
                <Typography
                  color={fixedAsset?.fixedAssetName ? "black" : "green"}
                  sx={{ p: 2 }}
                  variant="h4"
                >
                  {" "}
                  {fixedAsset?.fixedAssetName
                    ? fixedAsset.fixedAssetName
                    : "New Fixed Asset"}{" "}
                </Typography>
              </Box>
            }
          </Grid>
          {editMode === 2 && (
            <Grid item xs={7}>
              <CreateFixedAssetMenu />
            </Grid>
          )}
        </Grid>
        <Form
          initialValues={editMode === 2 ? handleDatesObject(fixedAsset) : undefined}
          onSubmit={(values) => console.log(values)}
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid container spacing={2}>
                  <Grid item container xs={4}>
                    <Grid item xs={12}>
                      <Field
                        id={"fixedAssetName"}
                        name={"fixedAssetName"}
                        label={"Fixed Asset Name *"}
                        component={FormInput}
                        autoComplete={"off"}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={12}>
                      <Field
                        id={"fixedAssetTypeId"}
                        name={"fixedAssetTypeId"}
                        label={"Fixed Asset Type *"}
                        component={MemoizedFormDropDownList}
                        dataItemKey={"fixedAssetTypeId"}
                        textField={"cleanName"}
                        data={fixedAssetTypes ? fixedAssetTypes :[]}
                        // onChange={onProductTypeChange}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={12}>
                      <Field
                        id={"fixedAssetTypeDescription"}
                        name={"fixedAssetTypeDescription"}
                        label={"Fixed Asset Type Description"}
                        component={FormTextArea}
                        autoComplete={"off"}
                      />
                    </Grid>
                  </Grid>

                  <Grid item container xs={4}>
                    <Grid item xs={12}>
                      <Field
                        id={"purchaseCost"}
                        name={"purchaseCost"}
                        label={"Purchase Cost *"}
                        component={FormNumericTextBox}
                        autoComplete={"off"}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={12}>
                      <Field
                        id={"purchaseCostUomId"}
                        name={"purchaseCostUomId"}
                        label={"Purchase Cost UOM *"}
                        component={MemoizedFormDropDownList}
                        data={currencies ? currencies : []}
                        dataItemKey={"purchaseCostUomId"}
                        textField={"description"}
                        autoComplete={"off"}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={12}>
                      <Field
                        id={"depreciation"}
                        name={"depreciation"}
                        label={"Depreciation"}
                        component={FormNumericTextBox}
                        autoComplete={"off"}
                        // validator={requiredValidator}
                      />
                    </Grid>
                  </Grid>
                  <Grid item container xs={4}>
                    <Grid item xs={12}>
                        <Field
                            id={"dateAcquired"}
                            name={"dateAcquired"}
                            label={"Date Acquired"}
                            component={FormDatePicker}
                        />
                    </Grid>
                    <Grid item xs={12}>
                        <Field
                            id={"expectedEndOfLife"}
                            name={"expectedEndOfLife"}
                            label={"Expected End of Life"}
                            component={FormDatePicker}
                        />
                    </Grid>
                  </Grid>
                </Grid>

                <div className="k-form-buttons">
                      <Button
                        variant="contained"
                        type={"submit"}
                        color="success"
                        disabled={!formRenderProps.allowSubmit || buttonFlag}
                      >
                        Submit
                      </Button>
                      <Button
                        onClick={cancelEdit}
                        color="error"
                        variant="contained"
                      >
                        Cancel
                      </Button>
                </div>

                {buttonFlag && (
                  <LoadingComponent message="Processing Fixed Asset..." />
                )}
              </fieldset>
            </FormElement>
          )}
        />
      </Paper>
    </>
  );
};

export default FixedAssetForm;
