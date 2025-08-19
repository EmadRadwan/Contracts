import React, { useEffect, useRef, useState } from 'react'
import { CustomTimePeriod } from '../../../../app/models/accounting/customTimePeriod'
import AccountingMenu from '../../invoice/menu/AccountingMenu'
import { Box, Button, Grid, Paper, Typography } from '@mui/material'
import { Field, Form, FormElement } from '@progress/kendo-react-form'
import { MemoizedFormDropDownList2 } from '../../../../app/common/form/MemoizedFormDropDownList2'
import { useFetchCustomTimePeriodsLovQuery, useFetchTimePeriodTypesLovQuery } from '../../../../app/store/apis/accounting/customTimePeriodsApi'
import { handleDatesArray } from '../../../../app/util/utils'
import { requiredValidator } from '../../../../app/common/form/Validators'
import FormNumericTextBox from '../../../../app/common/form/FormNumericTextBox'
import FormInput from '../../../../app/common/form/FormInput'
import FormDatePicker from '../../../../app/common/form/FormDatePicker'

interface Props {
    selectedTimePeriod?: CustomTimePeriod
    onClose: () => void
    editMode: number
}

const OrgSetupTimePeriodForm = ({selectedTimePeriod, onClose, editMode}: Props) => {

  const formRef = useRef<Form | null>(null);
  const { data: timePeriods } = useFetchCustomTimePeriodsLovQuery(undefined);
  const {data: periodTypes} = useFetchTimePeriodTypesLovQuery(undefined)
  const onSubmitTimePeriod = (values: any) => {}
  return (
    <>
      <AccountingMenu selectedMenuItem={"/orgGL"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
      <Grid container spacing={2}>
      <Grid item xs={5}>
            {
              <Box display="flex" justifyContent="space-between">
                <Typography sx={{ p: 2 }} color={editMode === 1 ? "green" : "black"} variant="h4">
                  {editMode > 1 ? `Time Period ${selectedTimePeriod?.customTimePeriodId}` : "New Time Period"}
                </Typography>
              </Box>
            }
          </Grid>
      </Grid>
      <Form
          onSubmit={(values) => onSubmitTimePeriod(values as CustomTimePeriod)}
          ref={formRef}
          initialValues={selectedTimePeriod ?? {isClosed: "N", periodTypeId: "FISCAL_BIWEEK"}}
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid
                  container
                  spacing={2}
                  flexDirection={"column"}
                >
                  <Grid item xs={3}>
                    <Field
                      name="parentPeriodId"
                      id="parentPeriodId"
                      label="Parent Time Period *"
                      component={MemoizedFormDropDownList2}
                      data={timePeriods ?? []}
                      dataItemKey="customTimePeriodId"
                      textField="description"
                      validator={requiredValidator}
                    />
                  </Grid>
                  <Grid item container spacing={2} flexDirection={"row"}>
                    <Grid item xs={3}>
                      <Field
                        name="periodTypeId"
                        id="periodTypeId"
                        label="Period Type *"
                        component={MemoizedFormDropDownList2}
                        data={periodTypes ?? []}
                        dataItemKey="periodTypeId"
                        textField="periodTypeDescription"
                        disabled={editMode > 1}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field 
                        name="periodNum"
                        id="periodNum"
                        label="Period Num *"
                        component={FormNumericTextBox}
                        disabled={editMode > 1}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field 
                        name="periodName"
                        id="periodName"
                        label="Period Name *"
                        component={FormInput}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field 
                        name="fromDate"
                        id="fromDate"
                        label="From Date *"
                        component={FormDatePicker}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field 
                        name="thruDate"
                        id="thruDate"
                        label="Thru Date *"
                        component={FormDatePicker}
                        validator={requiredValidator}
                        disabled={editMode > 1}
                      />
                    </Grid>
                    <Grid item xs={3}>
                      <Field
                        name="isClosed"
                        id="isClosed"
                        label="Is Closed?"
                        component={MemoizedFormDropDownList2}
                        data={[
                          {value: "N", text: "No"},
                          {value: "Y", text: "Yes"}
                        ]}
                        dataItemKey="value"
                        textField="text"
                        disabled={editMode > 1}
                        validator={requiredValidator}
                      />
                    </Grid>
                  </Grid>
                </Grid>
              </fieldset>
              <div className="k-form-buttons">
                <Button
                  variant="contained"
                  type={"submit"}
                  color="success"
                  disabled={!formRenderProps.allowSubmit}
                >
                  Submit
                </Button>

                <Button
                  variant="contained"
                  type={"button"}
                  color="error"
                  onClick={onClose}
                >
                  Back
                </Button>
              </div>
            </FormElement>
          )}
        />
      </Paper>
    </>
  )
}

export default OrgSetupTimePeriodForm