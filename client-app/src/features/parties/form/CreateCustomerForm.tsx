import React, { useState } from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import FormTextArea from '../../../app/common/form/FormTextArea';
import FormInput from '../../../app/common/form/FormInput';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { FormComboBox } from '../../../app/common/form/FormComboBox';
import { Party } from '../../../app/models/party/party';
import {
  useAppDispatch,
  useFetchCountriesQuery,
  useFetchCustomerQuery,
} from '../../../app/store/configureStore';
import agent from '../../../app/api/agent';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import { setParty } from '../slice/partySlice';
import { Box, Paper, Typography } from '@mui/material';
import CreateCustomerMenu from '../menu/CreateCustomerMenu';
import { setSingleParty } from '../slice/singlePartySlice';
import { phoneValidator, requiredValidator } from '../../../app/common/form/Validators';
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";

interface Props {
  party?: Party;
  editMode: number;
  cancelEdit: () => void;
}

export default function CreateCustomerForm({ party, cancelEdit, editMode }: Props) {
  const { data: countries, isSuccess: isCountriesLoaded } = useFetchCountriesQuery(undefined);
  const [buttonFlag, setButtonFlag] = useState(false);
  const { data: customer, error, isFetching, isLoading } = useFetchCustomerQuery(party?.partyId, {
    skip: party?.partyId === undefined,
  });
  const { getTranslatedLabel } = useTranslationHelper();

  const dispatch = useAppDispatch();

  async function handleSubmitData(data: any) {
    setButtonFlag(true);
    try {
      let response: any;
      if (editMode === 2) {
        response = await agent.Parties.updateCustomer(data);
      } else {
        response = await agent.Parties.createCustomer(data);
      }
      dispatch(setParty(response));
      dispatch(setSingleParty(response));
      cancelEdit();
    } catch (error) {
      console.log(error);
    }
    setButtonFlag(false);
  }

  return (
    <Paper elevation={5} className={`div-container-withBorderCurved`} sx={{ mt: 5 }}>
      {isFetching && <LoadingComponent message={getTranslatedLabel("party.customers.form.loading", "Loading Customer...")} />}

      <Grid container spacing={2}>
        <Grid item xs={6}>
          <Box display='flex' justifyContent='space-between' paddingBottom={4}>
            <Typography
              sx={{ p: 2 }}
              variant='h4'
              color={editMode === 1 ? 'green' : 'black'}
            >
              {isFetching && <LoadingComponent message={getTranslatedLabel("party.customers.form.loading", "Loading Customer...")} />}

            </Typography>
          </Box>
        </Grid>
        <Grid item xs={6}>
          <CreateCustomerMenu partyId={party?.partyId} />
        </Grid>
      </Grid>

      <Form
        initialValues={editMode === 2 ? customer : undefined}
        key={JSON.stringify(customer)}
        onSubmit={(values) => handleSubmitData(values)}
        render={(formRenderProps) => (
          <FormElement>
            <fieldset className={'k-form-fieldset'}>
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <Field
                    id={'firstName'}
                    name={'firstName'}
                    label={getTranslatedLabel("party.customers.form.firstName", "First Name *")}
                    component={FormInput}
                    autoComplete={'off'}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={6}>
                  <Field
                    id={'infoString'}
                    name={'infoString'}
                    label={getTranslatedLabel("party.customers.form.email", "Email Address")}
                    component={FormInput}
                    autoComplete={'off'}
                  />
                </Grid>
                <Grid item xs={6}>
                  {isCountriesLoaded && (
                    <Field
                      id={'geoId'}
                      name={'geoId'}
                      label={getTranslatedLabel("party.customers.form.countryCode", "Country Code")}
                      component={FormComboBox}
                      dataItemKey={'geoId'}
                      textField={'geoName'}
                      autoComplete={'off'}
                      data={countries}
                    />
                  )}
                </Grid>
                <Grid item xs={6}>
                  <Field
                    id={'mobileContactNumber'}
                    name={'mobileContactNumber'}
                    label={getTranslatedLabel("party.customers.form.mobileContactNumber", "Mobile Contact Number *")}
                    autoComplete={'off'}
                    component={FormInput}
                    validator={phoneValidator}
                  />
                </Grid>
                <Grid item xs={6}>
                  <Field
                    id={'address1'}
                    name={'address1'}
                    label={getTranslatedLabel("party.customers.form.address1", "Address 1")}
                    component={FormInput}
                    autoComplete={'off'}
                  />
                </Grid>
                <Grid item xs={6}>
                  <Field
                    id={'address2'}
                    name={'address2'}
                    label={getTranslatedLabel("party.customers.form.address2", "Address 2")}
                    rows={3}
                    component={FormTextArea}
                    autoComplete={'off'}
                  />
                </Grid>
              </Grid>
              <div className='k-form-buttons'>
                <Grid container rowSpacing={2}>
                  <Grid item xs={1}>
                    <Button
                      variant='contained'
                      type={'submit'}
                      color='success'
                      disabled={!formRenderProps.allowSubmit || buttonFlag}
                    >
                      {getTranslatedLabel("party.customers.form.submit", "Submit")}
                    </Button>
                  </Grid>
                  <Grid item xs={1}>
                    <Button onClick={cancelEdit} color='error' variant='contained'>
                      {getTranslatedLabel("party.customers.form.cancel", "Cancel")}
                    </Button>
                  </Grid>
                </Grid>
              </div>

              {buttonFlag && <LoadingComponent message={getTranslatedLabel("party.customers.form.processing", "Processing Customer...")} />}

            </fieldset>
          </FormElement>
        )}
      />
    </Paper>
  );
}