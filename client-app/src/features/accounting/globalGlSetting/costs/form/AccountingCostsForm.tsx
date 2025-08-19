import { Button, Grid, Paper } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import React from "react";
import { MemoizedFormDropDownList } from "../../../../../app/common/form/MemoizedFormDropDownList";
import { requiredValidator } from "../../../../../app/common/form/Validators";
import FormInput from "../../../../../app/common/form/FormInput";
import { useFetchAcctgTransTypesQuery } from "../../../../../app/store/apis";
import { useFetchCurrenciesQuery } from "../../../../../app/store/configureStore";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import AccountingMenu from "../../../invoice/menu/AccountingMenu";
import GlSettingsMenu from "../../menu/GlSettingsMenu";
import { MemoizedFormDropDownList2 } from "../../../../../app/common/form/MemoizedFormDropDownList2";

interface AccountingCostsFormProps {
  editMode: number;
  selectedCostComponent?: any
  cancelEdit: () => void;
}

const AccountingCostsForm = ({
  editMode,
  cancelEdit,
  selectedCostComponent
}: AccountingCostsFormProps) => {
  const { data: acctgTransTypes } = useFetchAcctgTransTypesQuery(undefined);
  const { data: currencies } = useFetchCurrenciesQuery(undefined);
  return (
    <>
      <AccountingMenu selectedMenuItem="/globalGl" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <GlSettingsMenu />
        <Form
          onSubmit={(values) => console.log(values)}
          initialValues={editMode === 1 ? {costGlAccountTypeId: "_NA_", currencyUomId: "EGP"} : selectedCostComponent}
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Field
                      name={"description"}
                      id={"description"}
                      label={"Description"}
                      component={FormInput}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <Field
                      name={"costGlAccountTypeId"}
                      id={"costGlAccountTypeId"}
                      label={"Cost GL Account Type *"}
                      component={MemoizedFormDropDownList2}
                      data={acctgTransTypes ?? []}
                      dataItemKey={"acctgTransTypeId"}
                      textField={"description"}
                      validator={requiredValidator}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <Field
                      name={"fixedCost"}
                      id={"fixedCost"}
                      label={"Fixed Cost *"}
                      component={FormNumericTextBox}
                      min={0}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <Field
                      name={"variableCost"}
                      id={"variableCost"}
                      label={"Variable Cost"}
                      component={FormNumericTextBox}
                      min={0}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <Field
                      name={"perMilliSecond"}
                      id={"perMilliSecond"}
                      label={"Per Millisecond"}
                      component={FormNumericTextBox}
                      min={0}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <Field
                      name={"currencyUomId"}
                      id={"currencyUomId"}
                      label={"Currency *"}
                      component={MemoizedFormDropDownList}
                      data={currencies ? currencies : []}
                      dataItemKey={"currencyUomId"}
                      textField={"description"}
                      validator={requiredValidator}
                    />
                  </Grid>
                </Grid>
    
                <Grid container spacing={2} mt={1}>
                  <Grid item paddingTop={2}>
                    <Button
                      variant="contained"
                      type={"submit"}
                      color="success"
                      disabled={
                        !formRenderProps.valueGetter("uomId") ===
                        !formRenderProps.valueGetter("uomIdTo")
                      }
                    >
                      {editMode === 1 ? "Create" : "Update"}
                    </Button>
                  </Grid>
                  <Grid item paddingTop={2} >
                    <Button
                      variant="contained"
                      type={"submit"}
                      color="error"
                      onClick={cancelEdit}
                    >
                      Cancel
                    </Button>
                  </Grid>
                </Grid>
              </fieldset>
            </FormElement>
          )}
        />
      </Paper>
    </>
  );
};

export default AccountingCostsForm;
