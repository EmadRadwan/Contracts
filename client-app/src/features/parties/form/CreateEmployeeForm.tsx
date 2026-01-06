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
    useFetchCountriesQuery
} from '../../../app/store/configureStore';
import agent from '../../../app/api/agent';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import { setParty } from '../slice/partySlice';
import { setSingleParty } from '../slice/singlePartySlice';
import { Box, Paper, Typography } from '@mui/material';
import { phoneValidator, requiredValidator } from '../../../app/common/form/Validators';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {useFetchEmployeeQuery} from "../../../app/store/apis";

interface Props {
    party?: Party;
    editMode: number; // 1 = create, 2 = edit
    cancelEdit: () => void;
}

export default function CreateEmployeeForm({ party, cancelEdit, editMode }: Props) {
    const { data: countries, isSuccess: isCountriesLoaded } = useFetchCountriesQuery(undefined);
    const [buttonFlag, setButtonFlag] = useState(false);

    const { data: employee, isFetching } = useFetchEmployeeQuery(party?.partyId, {
        skip: party?.partyId === undefined || editMode !== 2,
    });

    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            let response: any;
            if (editMode === 2) {
                response = await agent.Parties.updateEmployee(data);
            } else {
                response = await agent.Parties.createEmployee(data);
            }
            dispatch(setParty(response));
            dispatch(setSingleParty(response));
            cancelEdit();
        } catch (error) {
            console.error(error);
        } finally {
            setButtonFlag(false);
        }
    }

    return (
        <Paper elevation={5} className={`div-container-withBorderCurved`} sx={{ mt: 5 }}>
            {isFetching && <LoadingComponent message={getTranslatedLabel("party.employees.form.loading", "Loading Employee...")} />}

            <Grid container spacing={2}>
                <Grid item xs={6}>
                    <Box display='flex' justifyContent='space-between' paddingBottom={4}>
                        <Typography
                            sx={{ p: 2 }}
                            variant='h4'
                            color={editMode === 1 ? 'green' : 'black'}
                        >
                            {editMode === 1
                                ? getTranslatedLabel("party.employees.form.createTitle", "Create Employee")
                                : getTranslatedLabel("party.employees.form.editTitle", "Edit Employee")}
                        </Typography>
                    </Box>
                </Grid>
                
            </Grid>

            <Form
                initialValues={editMode === 2 ? employee : undefined}
                key={JSON.stringify(employee)}
                onSubmit={(values) => handleSubmitData(values)}
                render={(formRenderProps) => (
                    <FormElement>
                        <fieldset className={'k-form-fieldset'}>
                            <Grid container spacing={2}>
                                <Grid item xs={6}>
                                    <Field
                                        id={'firstName'}
                                        name={'firstName'}
                                        label={getTranslatedLabel("party.employees.form.firstName", "First Name *")}
                                        component={FormInput}
                                        autoComplete={'off'}
                                        validator={requiredValidator}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        id={'infoString'}
                                        name={'infoString'}
                                        label={getTranslatedLabel("party.employees.form.email", "Email Address (Work)")}
                                        component={FormInput}
                                        autoComplete={'off'}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    {isCountriesLoaded && (
                                        <Field
                                            id={'geoId'}
                                            name={'geoId'}
                                            label={getTranslatedLabel("party.employees.form.countryCode", "Country")}
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
                                        label={getTranslatedLabel("party.employees.form.mobile", "Mobile Phone *")}
                                        component={FormInput}
                                        autoComplete={'off'}
                                        validator={phoneValidator}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        id={'address1'}
                                        name={'address1'}
                                        label={getTranslatedLabel("party.employees.form.address1", "Address 1")}
                                        component={FormInput}
                                        autoComplete={'off'}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        id={'address2'}
                                        name={'address2'}
                                        label={getTranslatedLabel("party.employees.form.address2", "Address 2")}
                                        component={FormTextArea}
                                        rows={3}
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
                                            {getTranslatedLabel("party.employees.form.submit", "Submit")}
                                        </Button>
                                    </Grid>
                                    <Grid item xs={1}>
                                        <Button onClick={cancelEdit} color='error' variant='contained'>
                                            {getTranslatedLabel("party.employees.form.cancel", "Cancel")}
                                        </Button>
                                    </Grid>
                                </Grid>
                            </div>

                            {buttonFlag && <LoadingComponent message={getTranslatedLabel("party.employees.form.processing", "Processing Employee...")} />}
                        </fieldset>
                    </FormElement>
                )}
            />
        </Paper>
    );
}