import { Button, Grid, Paper } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import React, { useState } from "react";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import {
  useAppSelector,
  useFetchCurrenciesQuery,
} from "../../../../app/store/configureStore";
import { useNavigate } from "react-router";
import { useFetchFixedAssetStandardCostTypesQuery } from "../../../../app/store/apis";
import { FixedAsset } from "../../../../app/models/accounting/fixedAsset";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { handleDatesObject } from "../../../../app/util/utils";

interface Props {
  selectedFixedAsset?: FixedAsset;
  selectedCost?: any;
  editMode: number;
  cancelEdit: () => void;
}

const StandardCostsForm = ({ editMode, cancelEdit, selectedCost }: Props) => {
  const navigate = useNavigate();
  console.log(selectedCost);
  const [buttonFlag, setButtonFlag] = useState(false);
  const { data: currencies } = useFetchCurrenciesQuery(undefined);
  const { data: stdCostTypes } =
    useFetchFixedAssetStandardCostTypesQuery(undefined);

  console.log(stdCostTypes);
  return (
    <>
      <AccountingMenu selectedMenuItem={"fixedAssets"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Form
          onSubmit={(values) => console.log(values)}
          initialValues={
            selectedCost
              ? handleDatesObject({
                  ...selectedCost,
                  currencyUomId: selectedCost?.amountUomId,
                })
              : {}
          }
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Field
                      name={"fixedAssetStdCostTypeId"}
                      id={"fixedAssetStdCostTypeId"}
                      label={"Standard Cost Type *"}
                      component={MemoizedFormDropDownList}
                      data={stdCostTypes ? stdCostTypes : []}
                      dataItemKey={"fixedAssetStdCostTypeId"}
                      textField={"description"}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <Field
                      name={"fromDate"}
                      id={"fromDate"}
                      label={"Start date"}
                      component={FormDatePicker}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <Field
                      name={"currencyUomId"}
                      id={"currencyUomId"}
                      label={"Currency"}
                      component={MemoizedFormDropDownList}
                      data={currencies ? currencies : []}
                      dataItemKey={"currencyUomId"}
                      textField={"description"}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <Field
                      name={"thruDate"}
                      id={"thruDate"}
                      label={"End date"}
                      component={FormDatePicker}
                    />
                  </Grid>
                </Grid>
                <div className="k-form-buttons">
                  <Button
                    variant="contained"
                    type={"submit"}
                    color="success"
                    disabled={!formRenderProps.allowSubmit || buttonFlag}
                  >
                    Save
                  </Button>

                  <Button
                    variant="contained"
                    type={"button"}
                    color="error"
                    onClick={cancelEdit}
                  >
                    Back
                  </Button>
                </div>
              </fieldset>
            </FormElement>
          )}
        ></Form>
      </Paper>
    </>
  );
};

export default StandardCostsForm;
