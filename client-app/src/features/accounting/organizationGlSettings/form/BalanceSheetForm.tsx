import { Button, Grid } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import React from "react";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { requiredValidator } from "../../../../app/common/form/Validators";

import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

interface BalanceSheetFormProps {
  onSubmit: (values: any) => void;
}

const BalanceSheetForm = ({ onSubmit }: BalanceSheetFormProps) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.balance-sheet.form";
  return (
    <Form
      onSubmitClick={(values) => onSubmit(values)}
      initialValues={{ glFiscalTypeId: "ACTUAL", thruDate: new Date() }}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2} alignItems={"flex-end"}>
              <Grid container item xs={12} spacing={2}>
                <Grid item xs={6}>
                  <Field
                    name={"thruDate"}
                    id={"thruDate"}
                    label={getTranslatedLabel(`${localizationKey}.thru-date`, "Thru Date")}
                    component={FormDatePicker}
                    validator={requiredValidator}
                  />
                </Grid>
              </Grid>
            </Grid>
            <Grid container item xs={12} spacing={2} mt={2}>
              <Grid item xs={12}>
                <Button variant="contained" type="submit" color="success">
                  {getTranslatedLabel(`${localizationKey}.generate-report`, "Generate Report")}
                </Button>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
      )}
    />
  );
};

export default BalanceSheetForm;
