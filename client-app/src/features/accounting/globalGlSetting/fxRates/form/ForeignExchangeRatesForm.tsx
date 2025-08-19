import React from 'react'
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { MemoizedFormDropDownList } from '../../../../../app/common/form/MemoizedFormDropDownList';
import { useFetchCurrenciesQuery } from '../../../../../app/store/configureStore';
import { Button, Grid } from '@mui/material';
import FormInput from '../../../../../app/common/form/FormInput';
import { DatePicker } from '@progress/kendo-react-dateinputs';
import { requiredValidator } from '../../../../../app/common/form/Validators';

const ForeignExchangeRatesForm = () => {
  const {data: currencies} = useFetchCurrenciesQuery(undefined)
  let reasons = [
    {id: "internal", text: "Internal Conversion"},
    {id: "external", text: "External Conversion"}
  ]
    console.log(currencies)
  return (
    <Form
      onSubmit={(values) => console.log(values)}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Field
                  name={"uomId"}
                  id={"fromCurrency"}
                  label={"From Currency *"}
                  component={MemoizedFormDropDownList}
                  data={currencies ? currencies : []}
                  dataItemKey={"currencyUomId"}
                  textField={"description"}
                  validator={requiredValidator}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                  name={"uomIdTo"}
                  id={"toCurrency"}
                  label={"To Currency *"}
                  component={MemoizedFormDropDownList}
                  data={currencies ? currencies : []}
                  dataItemKey={"currencyUomId"}
                  textField={"description"}
                  validator={requiredValidator}
                />
              </Grid>
            </Grid>
            <Grid container spacing={1} xs={12}>
                <Grid item container xs={12} spacing={1}>
                    <Grid item xs={3}>
                        <Field 
                            name={"purpose"}
                            id={"purpose"}
                            label={"Purpose *"}
                            component={MemoizedFormDropDownList}
                            data={reasons}
                            dataItemKey={"id"}
                            validator={requiredValidator}
                            textField={"text"}
                        />
                    </Grid>
                    <Grid item xs={3}>
                        <Field 
                            name={"rate"}
                            id={"rate"}
                            label={"Rate *"}
                            validator={requiredValidator}
                            component={FormInput}
                        />
                    </Grid>
                </Grid>
                <Grid item container xs={12} spacing={1}>
                    <Grid item xs={3}>
                        <Field 
                            name={"fromDate"}
                            id={"fromDate"}
                            label={"From Date *"}
                            component={DatePicker}
                            validator={requiredValidator}
                        />
                    </Grid>
                    <Grid item xs={3}>
                        <Field 
                            name={"thruDate"}
                            id={"thruDate"}
                            label={"Through Date"}
                            component={DatePicker}
                        />
                    </Grid>
                </Grid>
            </Grid>
            <Grid container>
              <Grid item paddingTop={2} xs={3}>
                <Button
                  variant="contained"
                  type={"submit"}
                  color="success"
                  disabled={
                    !formRenderProps.valueGetter("uomId") === !formRenderProps.valueGetter("uomIdTo")
                  }
                >
                  Update
                </Button>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
      )}
    />
  )
}

export default ForeignExchangeRatesForm