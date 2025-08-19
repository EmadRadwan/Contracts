import { FormElement, Field, Form } from "@progress/kendo-react-form";
import React from "react";
import { Grid, Button, ListItem, ListItemText } from "@mui/material";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { useFetchGlAccountOrganizationGlAccountsQuery } from "../../../../app/store/apis";
import { useAppSelector } from "../../../../app/store/configureStore";
import { useFetchCustomTimePeriodsLovQuery } from "../../../../app/store/apis/accounting/customTimePeriodsApi";
import { DropDownList } from "@progress/kendo-react-dropdowns";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

interface GlAccountTrialBalanceFormProps {
  onSubmit: (values: any) => void;
}

const GlAccountTrialBalanceForm = ({
  onSubmit,
}: GlAccountTrialBalanceFormProps) => {
  const {getTranslatedLabel} = useTranslationHelper()
  const localizationKey = "accounting.orgGL.reports.gl-trial-balance"
  const companyId = useAppSelector(
    (state) => state.accountingSharedUi.selectedAccountingCompanyId
  );
  const { data: glAccounts } = useFetchGlAccountOrganizationGlAccountsQuery(
    companyId,
    {
      skip: !companyId,
    }
  );
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
          return itemProps.dataItem.customTimePeriodId;
        }}
        {...li.props}
        sx={{ padding: 0, display: "flex", alignItems: "center" }}
      >
        <ListItemText sx={{ marginInlineStart: "4px" }}>
          {`FY ${period?.periodName}: ${new Date(
            period?.fromDate!
          ).toLocaleDateString("en-GB")} - ${new Date(
            period?.thruDate!
          ).toLocaleDateString("en-GB")}`}
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
      `FY ${period?.periodName}: ${new Date(
        period?.fromDate!
      ).toLocaleDateString("en-GB")} - ${new Date(
        period?.thruDate!
      ).toLocaleDateString("en-GB")}`,
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
      initialValues={{isPosted: "ALL"}}
      onSubmit={(values) => onSubmit(values)}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2} alignItems={"flex-end"}>
              <Grid container item xs={12} spacing={2}>
                <Grid item xs={4}>
                  <Field
                    name={"glAccountId"}
                    id={"glAccountId"}
                    label={getTranslatedLabel(`${localizationKey}.glAccount`, "GL Account")}
                    component={MemoizedFormDropDownList}
                    validator={requiredValidator}
                    data={glAccounts ?? []}
                    textField="accountName"
                    dataItemKey="glAccountId"
                  />
              </Grid>
                <Grid item xs={4}>
                  <Field
                    name={"timePeriodId"}
                    id={"timePeriodId"}
                    label={getTranslatedLabel(`${localizationKey}.timePeriod`, "Time Period")}
                    itemRender={itemRender}
                    component={MemoizedFormDropDownList2}
                    valueRender={valueRender}
                    data={timePeriods ?? []}
                    dataItemKey={"customTimePeriodId"}
                  />
                </Grid>
                <Grid item xs={3}>
                  <Field
                    name={"isPosted"}
                    id={"isPosted"}
                    label={getTranslatedLabel(`${localizationKey}.isPosted`, "Is Posted")}
                    component={MemoizedFormDropDownList}
                    data={[
                      { text: "Yes", isPosted: "Y" },
                      { text: "No", isPosted: "N" },
                      { text: "All", isPosted: "ALL" },
                    ]}
                    textField="text"
                    dataItemKey="isPosted"
                  />
                </Grid>
              </Grid>
              </Grid>
            <Grid container item xs={12} spacing={2} mt={2}>
              <Grid item xs={12}>
                <Button variant="contained" type="submit" color="success">
                  {getTranslatedLabel("general.generate", "Generate Report")}
                </Button>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
      )}
    />
  );
};

export default GlAccountTrialBalanceForm;
