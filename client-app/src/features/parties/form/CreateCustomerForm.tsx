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
import {
  useCreateCustomerMutation,
  useCreateEmployeeMutation,
  useUpdateCustomerMutation,
  useUpdateEmployeeMutation
} from "../../../app/store/apis";

interface Props {
  party?: Party;
  editMode: number;
  cancelEdit: () => void;
}

export default function CreateCustomerForm({ party, cancelEdit, editMode }: Props) {
  const { data: countries, isSuccess: isCountriesLoaded } = useFetchCountriesQuery(undefined);
  const [buttonFlag, setButtonFlag] = useState(false);
  const [creationSuccess, setCreationSuccess] = useState<{
    partyId: string;
    glAccountId: string | null;
    glAccountName: string | null;
    glAccountArabic: string | null;
  } | null>(null);


  const { data: customer, error, isFetching, isLoading } = useFetchCustomerQuery(party?.partyId, {
    skip: party?.partyId === undefined,
  });

  const [createCustomer, { isLoading: isCreating }] = useCreateCustomerMutation();
  const [updateCustomer, { isLoading: isUpdating }] = useUpdateCustomerMutation();

  const isMutating = isCreating || isUpdating || buttonFlag;



  const { getTranslatedLabel } = useTranslationHelper();

  const dispatch = useAppDispatch();

  async function handleSubmitData(data: any) {
    setButtonFlag(true);
    setCreationSuccess(null); // reset previous success

    try {
      let response: any;
      if (editMode === 2) {
        response = await updateCustomer(data).unwrap();

        setCreationSuccess({
          partyId: response.partyId,
          glAccountId: response.createdGlAccountId || null,
          glAccountName: response.createdGlAccountName || null,
          glAccountArabic: response.createdGlAccountArabicName || null,
        });
        dispatch(setParty(response));
        dispatch(setSingleParty(response));
        // For edit mode you might want different success UI
      } else {
        response = await createCustomer(data).unwrap();

        dispatch(setParty(response));
        dispatch(setSingleParty(response));

        // NEW: store success info if GL account was created
        setCreationSuccess({
          partyId: response.partyId,
          glAccountId: response.createdGlAccountId || null,
          glAccountName: response.createdGlAccountName || null,
          glAccountArabic: response.createdGlAccountArabicName || null,
        });
      }

      // Optional: do NOT call cancelEdit() immediately → let user see success
      // cancelEdit();  ← comment out or move to a "Done" button
    } catch (error) {
      console.log(error);
      // Optional: show error toast/notification here
    } finally {
      setButtonFlag(false);
    }
  }


  return (
      <Paper elevation={5} className={`div-container-withBorderCurved`} sx={{ mt: 5 }}>
        {isFetching && <LoadingComponent message={getTranslatedLabel("party.customers.form.loading", "Loading Customer...")} />}

        {/* NEW: Success message when GL account created */}
        {creationSuccess && (
            <Box sx={{ p: 3, mb: 3, bgcolor: '#e8f5e9', borderRadius: 2, border: '1px solid #81c784' }}>
              <Typography variant="h5" color="success.main" gutterBottom>
                {getTranslatedLabel("party.customers.form.success", "Customer Created Successfully!")}
              </Typography>
              <Typography variant="body1" gutterBottom>
                {getTranslatedLabel("party.customers.form.customerId", "Customer ID")}: <strong>{creationSuccess.partyId}</strong>
              </Typography>

              {creationSuccess.glAccountId && (
                  <>
                    <Typography variant="h6" sx={{ mt: 2 }} color="primary">
                      {getTranslatedLabel("party.customers.form.newArAccount", "New Accounts Receivable Sub-Account Created")}
                    </Typography>
                    <Grid container spacing={2} sx={{ mt: 1 }}>
                      <Grid item xs={12} sm={6}>
                        <Typography>
                          <strong>{getTranslatedLabel("party.customers.form.glAccountId", "GL Account ID")}:</strong>{' '}
                          {creationSuccess.glAccountId}
                        </Typography>
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <Typography>
                          <strong>{getTranslatedLabel("party.customers.form.accountName", "Account Name")}:</strong>{' '}
                          {creationSuccess.glAccountName}
                        </Typography>
                      </Grid>
                      {creationSuccess.glAccountArabic && (
                          <Grid item xs={12}>
                            <Typography>
                              <strong>{getTranslatedLabel("party.customers.form.accountNameArabic", "Account Name (Arabic)")}:</strong>{' '}
                              {creationSuccess.glAccountArabic}
                            </Typography>
                          </Grid>
                      )}
                    </Grid>
                  </>
              )}

              <Box sx={{ mt: 3 }}>
                <Button
                    variant="contained"
                    color="primary"
                    onClick={cancelEdit}
                    sx={{ mr: 2 }}
                >
                  {getTranslatedLabel("party.customers.form.done", "Done / Close")}
                </Button>
                <Button
                    variant="outlined"
                    onClick={() => {
                      setCreationSuccess(null); // reset to allow creating another
                      // Optional: reset form values if needed
                    }}
                >
                  {getTranslatedLabel("party.customers.form.createAnother", "Create Another Customer")}
                </Button>
              </Box>
            </Box>
        )}

        {/* Original form – hide when success? or keep visible */}
        {!creationSuccess && (
            <>
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <Box display='flex' justifyContent='space-between' paddingBottom={4}>
                    <Typography sx={{ p: 2 }} variant='h4' color={editMode === 1 ? 'green' : 'black'}>
                      {/* title */}
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={6}>
                  <CreateCustomerMenu partyId={party?.partyId} partyName={customer?.description} />
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
                                  //validator={phoneValidator}
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
                          {customer?.linkedGlAccounts?.length > 0 ? (
                              <Grid item xs={12} sx={{mt: 4}}>
                                <Box
                                    sx={{
                                      p: 3,
                                      bgcolor: "#e8f5e9",
                                      borderRadius: 2,
                                      border: "1px solid #81c784",
                                    }}
                                >
                                  <Typography variant="h6" color="success.main" gutterBottom>
                                    {getTranslatedLabel("party.customers.form.linkedAccounts", "Linked GL Accounts")}
                                  </Typography>

                                  {customer.linkedGlAccounts.map((acc, index) => (
                                      <Box
                                          key={acc.glAccountId}
                                          sx={{
                                            mt: index > 0 ? 3 : 1,
                                            p: 2,
                                            bgcolor: "white",
                                            borderRadius: 1,
                                            border: "1px solid #e0e0e0",
                                          }}
                                      >
                                        <Grid container spacing={2}>
                                          <Grid item xs={12} sm={4}>
                                            <Typography>
                                              <strong>{getTranslatedLabel("party.customers.form.glAccountId", "GL Account ID")}:</strong>{" "}
                                              {acc.glAccountId}
                                            </Typography>
                                          </Grid>
                                          <Grid item xs={12} sm={4}>
                                            <Typography>
                                              <strong>{getTranslatedLabel("party.customers.form.role", "Role")}:</strong>{" "}
                                              {acc.roleDescription || acc.roleTypeId}
                                            </Typography>
                                          </Grid>
                                          <Grid item xs={12} sm={4}>
                                            <Typography>
                                              <strong>{getTranslatedLabel("party.customers.form.accountType", "Type")}:</strong>{" "}
                                              {acc.glAccountTypeId}
                                            </Typography>
                                          </Grid>

                                          <Grid item xs={12} sm={6}>
                                            <Typography>
                                              <strong>{getTranslatedLabel("party.customers.form.accountName", "Name")}:</strong>{" "}
                                              {acc.accountName || "—"}
                                            </Typography>
                                          </Grid>
                                          <Grid item xs={12} sm={6}>
                                            <Typography>
                                              <strong>{getTranslatedLabel("party.customers.form.accountNameArabic", "Arabic Name")}:</strong>{" "}
                                              {acc.accountNameArabic || "—"}
                                            </Typography>
                                          </Grid>

                                          {acc.accountDescription && (
                                              <Grid item xs={12}>
                                                <Typography variant="body2"
                                                            color="text.secondary">
                                                  {acc.accountDescription}
                                                </Typography>
                                              </Grid>
                                          )}
                                        </Grid>
                                      </Box>
                                  ))}
                                </Box>
                              </Grid>
                          ) : editMode === 2 ? (
                              // No accounts message (same as before)
                              <Grid item xs={12} sx={{mt: 3}}>
                                <Box sx={{
                                  p: 2,
                                  bgcolor: "#fff3e0",
                                  borderRadius: 2,
                                  border: "1px solid #ff9800"
                                }}>
                                  <Typography variant="body1" color="warning.dark">
                                    {getTranslatedLabel(
                                        "party.customers.form.noLinkedAccounts",
                                        "No GL accounts are linked yet."
                                    )}
                                  </Typography>
                                </Box>
                              </Grid>
                          ) : null}
                          <div className='k-form-buttons'>
                            <Grid container rowSpacing={2}>
                              <Grid item xs={1}>
                                <Button
                                    variant='contained'
                                    type={'submit'}
                                    color='success'
                                    disabled={!formRenderProps.allowSubmit || isMutating}
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

                          {isMutating && <LoadingComponent message={getTranslatedLabel("party.customers.form.processing", "Processing Customer...")} />}

                        </fieldset>
                      </FormElement>
                  )}
              />
            </>
        )}
      </Paper>
  );
}