import { Button, Grid, ListItem, ListItemText } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import React from "react";
import { DropDownList } from "@progress/kendo-react-dropdowns";
import { useFetchCustomTimePeriodsLovQuery } from "../../../../app/store/apis/accounting/customTimePeriodsApi";

interface TrialBalanceCustomTimePeriodFormProps {
    onSubmit: (value: any) => void
}

const TrialBalanceCustomTimePeriodForm = ({onSubmit}: TrialBalanceCustomTimePeriodFormProps) => {
  const { data: timePeriods } = useFetchCustomTimePeriodsLovQuery(undefined);

  const itemRender = (
    li: React.ReactElement<HTMLLIElement>,
    itemProps: any
  ) => {
    const period = timePeriods?.find(
      (p) => p.customTimePeriodId === itemProps.dataItem.customTimePeriodId
    );
    return (
      <ListItem
        onClick={() => {
            return itemProps.dataItem.customTimePeriodId
        }}
        {...li.props}
        sx={{ padding: 0, display: "flex", alignItems: "center" }}
      >
        <ListItemText sx={{ marginInlineStart: "4px" }}>
          {`FY ${period?.periodName}: ${new Date(period?.fromDate!).toLocaleDateString("en-GB")} - ${new Date(period?.thruDate!).toLocaleDateString("en-GB")}`}
        </ListItemText>
      </ListItem>
    );
  };
  const valueRender = (element: any, value: any) => {
    if (!value) return element;
    const period = timePeriods?.find(
      (p) => p.customTimePeriodId === value.customTimePeriodId
    );
    const children = [
      `FY ${period?.periodName}: ${new Date(period?.fromDate!).toLocaleDateString("en-GB")} - ${new Date(period?.thruDate!).toLocaleDateString("en-GB")}`,
    ];
    return React.cloneElement(
      element,
      {
        ...element.props,
      },
      children
    );
  };
  return (
      <Form
        onSubmit={(values) => onSubmit({customTimePeriodId: values.customTimePeriodId.customTimePeriodId})}
        render={(formRenderProps) => (
          <FormElement>
            <fieldset className={"k-form-fieldset"}>
              <Grid container spacing={2} alignItems={"flex-end"}>
                <Grid item xs={5}>
                  <Field
                    name={"customTimePeriodId"}
                    id={"customTimePeriodId"}
                    label={"Time Period"}
                    itemRender={itemRender}
                    component={DropDownList}
                    valueRender={valueRender}
                    data={timePeriods ?? []}
                    dataItemKey={"customTimePeriodId"}
                  />
                </Grid>
                <Grid item xs={2}>
                  <Button
                    variant="contained"
                    type={"submit"}
                    color="success"
                    disabled={
                      !formRenderProps.valueGetter("customTimePeriodId")
                    }
                  >
                    Generate Report
                  </Button>
                </Grid>
              </Grid>
            </fieldset>
          </FormElement>
        )}
      />
  );
};

export default TrialBalanceCustomTimePeriodForm;
