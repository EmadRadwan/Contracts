import React, { useState } from "react";
import Button from "@mui/material/Button";
import Grid from "@mui/material/Grid";
import FormTextArea from "../../../app/common/form/FormTextArea";
import FormInput from "../../../app/common/form/FormInput";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { FormComboBox } from "../../../app/common/form/FormComboBox";
import { Party } from "../../../app/models/party/party";
import {
  useAppDispatch,
  useFetchCountriesQuery
} from "../../../app/store/configureStore";
import agent from "../../../app/api/agent";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { setParty } from "../slice/partySlice";
import { setSingleParty } from "../slice/singlePartySlice";
import { Box, Paper, Typography } from "@mui/material";
import CreateCustomerMenu from "../menu/CreateCustomerMenu";
import {
  phoneValidator,
  requiredValidator,
} from "../../../app/common/form/Validators";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {useFetchContractorQuery} from "../../../app/store/apis";

interface Props {
  party?: Party;
  editMode: number;
  cancelEdit: () => void;
}

export default function CreateContractorForm({
                                               party,
                                               cancelEdit,
                                               editMode,
                                             }: Props) {
  const { data: countries, isSuccess: isCountriesLoaded } = useFetchCountriesQuery(undefined);
  const [buttonFlag, setButtonFlag] = useState(false);
  const { getTranslatedLabel } = useTranslationHelper();

  // REFACTOR: Match supplier query signature and include error/loading states
  // Purpose: Consistent RTK Query usage and UX
  // Improvement: Show loading, handle errors, skip when no partyId
  const {
    data: contractor,
    error,
    isFetching,
    isLoading,
  } = useFetchContractorQuery(party?.partyId, {
    skip: party?.partyId === undefined,
  });

  const dispatch = useAppDispatch();

  async function handleSubmitData(data: any) {
    setButtonFlag(true);
    try {
      let response: any;
      if (editMode === 2) {
        response = await agent.Parties.updateContractor(data);
      } else {
        response = await agent.Parties.createContractor(data);
      }
      dispatch(setParty(response));
      dispatch(setSingleParty(response));
      cancelEdit();
    } catch (error) {
      console.log(error);
    }
    setButtonFlag(false);
  }

  // REFACTOR: Add console.log for debugging consistency
  console.log("contractor data:", contractor);

  return (
      <>
        <Paper
            elevation={5}
            className={`div-container-withBorderCurved`}
            sx={{ mt: 5 }}
        >
          {/* REFACTOR: Show loading during fetch */}
          {isFetching && (
              <LoadingComponent message={getTranslatedLabel("party.contractors.form.loading", "Loading Contractor...")} />
          )}

          <Grid container spacing={2}>
            <Grid item xs={8}>
              <Box display="flex" justifyContent="space-between" paddingBottom={4}>
                <Typography
                    sx={{ p: 2 }}
                    variant="h4"
                    color={editMode === 1 ? "green" : "black"}
                >
                  {editMode === 1
                      ? getTranslatedLabel("party.contractors.form.new", "New Contractor")
                      : contractor && contractor?.description}
                </Typography>
              </Box>
            </Grid>
            <Grid item xs={4}>
              {/* REFACTOR: Pass partyId and partyName like supplier */}
              <CreateCustomerMenu partyId={contractor?.partyId} partyName={contractor?.description} />
            </Grid>
          </Grid>

          {/* REFACTOR: Add key prop for form re-render on data change */}
          <Form
              initialValues={editMode === 2 ? contractor : undefined}
              key={JSON.stringify(contractor)}
              onSubmit={(values) => handleSubmitData(values)}
              render={(formRenderProps) => (
                  <FormElement>
                    <fieldset className={"k-form-fieldset"}>
                      <Grid container spacing={2}>
                        <Grid item xs={6}>
                          <Field
                              id={"groupName"}
                              name={"groupName"}
                              label={getTranslatedLabel("party.contractors.form.groupName", "Contractor Name *")}
                              component={FormInput}
                              autoComplete={"off"}
                              validator={requiredValidator}
                          />
                        </Grid>
                        <Grid item xs={6}>
                          <Field
                              id={"infoString"}
                              name={"infoString"}
                              label={getTranslatedLabel("party.contractors.form.email", "Email Address")}
                              component={FormInput}
                              autoComplete={"off"}
                          />
                        </Grid>
                        <Grid item xs={6}>
                          {isCountriesLoaded && (
                              <Field
                                  id={"geoId"}
                                  name={"geoId"}
                                  label={getTranslatedLabel("party.contractors.form.countryCode", "Country Code")}
                                  component={FormComboBox}
                                  dataItemKey={"geoId"}
                                  textField={"geoName"}
                                  autoComplete={"off"}
                                  data={countries}
                              />
                          )}
                        </Grid>
                        <Grid item xs={6}>
                          <Field
                              id={"mobileContactNumber"}
                              name={"mobileContactNumber"}
                              label={getTranslatedLabel("party.contractors.form.mobileContactNumber", "Mobile Contact Number")}
                              autoComplete={"off"}
                              component={FormInput}
                              validator={phoneValidator}
                          />
                        </Grid>
                        <Grid item xs={6}>
                          <Field
                              id={"address1"}
                              name={"address1"}
                              label={getTranslatedLabel("party.contractors.form.address1", "Address 1")}
                              component={FormInput}
                              autoComplete={"off"}
                          />
                        </Grid>
                        <Grid item xs={6}>
                          <Field
                              id={"address2"}
                              name={"address2"}
                              label={getTranslatedLabel("party.contractors.form.address2", "Address 2")}
                              rows={3}
                              component={FormTextArea}
                              autoComplete={"off"}
                          />
                        </Grid>
                      </Grid>

                      <div className="k-form-buttons">
                        <Grid container rowSpacing={2}>
                          <Grid item xs={1}>
                            <Button
                                variant="contained"
                                type={"submit"}
                                color="success"
                                disabled={!formRenderProps.allowSubmit || buttonFlag}
                            >
                              {getTranslatedLabel("party.contractors.form.submit", "Submit")}
                            </Button>
                          </Grid>
                          <Grid item xs={1}>
                            <Button onClick={cancelEdit} color="error" variant="contained">
                              {getTranslatedLabel("party.contractors.form.cancel", "Cancel")}
                            </Button>
                          </Grid>
                        </Grid>
                      </div>

                      {/* REFACTOR: Show processing state */}
                      {buttonFlag && (
                          <LoadingComponent message={getTranslatedLabel("party.contractors.form.processing", "Processing Contractor...")} />
                      )}
                    </fieldset>
                  </FormElement>
              )}
          />
        </Paper>
      </>
  );
}