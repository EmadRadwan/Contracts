import React, {useState} from 'react';
import {Button, Grid, Segment} from 'semantic-ui-react';
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import {useAppDispatch, useAppSelector} from "../../../app/store/configureStore";
import agent from "../../../app/api/agent";
import {PartyContact} from "../../../app/models/party/partyContact";
import useParties from "../../../app/hooks/useParties";
import {addPartyContact, partyContactSelectors, updatePartyContact} from "../slice/partyContactSlice";
import FormInput from "../../../app/common/form/FormInput";
import {requiredValidator} from "../../../app/common/form/Validators";

interface Props {
    partyContact?: PartyContact;
    editMode: number;
    cancelEdit: () => void;
}


export default function PartyContactForm({partyContact, cancelEdit, editMode}: Props) {

    const {selectedPartyContactId} = useAppSelector(state => state.partyContact);
    const {selectedParty} = useParties();
    const selectedPartyContact = useAppSelector(state =>
        partyContactSelectors.selectById(state, selectedPartyContactId!))
    const {contactMechPurposeTypes} = useAppSelector(state => state.party);

    const dispatch = useAppDispatch();

    const [buttonFlag, setButtonFlag] = useState(false);

    async function handleSubmitData(data: any) {
        setButtonFlag(true)
        try {
            let response: any;
            if (editMode === 2) {
                response = await agent.Parties.updatePartyContact(data);
                dispatch(updatePartyContact(response));

            } else {
                if (selectedParty) {
                    data.partyId = selectedParty.partyId
                }
                response = await agent.Parties.createPartyContact(data);
                dispatch(addPartyContact(response));
            }

            cancelEdit();
        } catch (error) {
            console.log(error)
        }
        setButtonFlag(false)
    }


    //console.log('selectedPartyContact', selectedPartyContact)
    //console.log('partyContact', partyContact)


    return (
        <>
            <Grid>
                <Grid.Column width={12}>
                    <Segment clearing>
                        <Form
                            initialValues={editMode === 2 ? partyContact : undefined}
                            onSubmit={values => handleSubmitData(values as PartyContact)}
                            render={(formRenderProps) => (

                                <FormElement>
                                    <fieldset className={'k-form-fieldset'}>
                                        <Grid>
                                            <Grid.Row columns={3}>
                                                <Grid.Column width={4}>
                                                    <Field
                                                        id={"contactMechPurposeTypeId"}
                                                        name={"contactMechPurposeTypeId"}
                                                        label={"Contact Type *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"contactMechPurposeTypeId"}
                                                        textField={"description"}
                                                        data={contactMechPurposeTypes}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid.Column>

                                            </Grid.Row>
                                            <Grid.Row columns={3}>
                                                <Grid.Column width={4}>
                                                    <Field
                                                        id={'contactNumber'}
                                                        name={'contactNumber'}
                                                        label={'Phone Number *'}
                                                        component={FormInput}
                                                        autoComplete={"off"}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid.Column>

                                            </Grid.Row>

                                            <Grid.Row columns={2}>
                                                <Grid.Column width={8}>
                                                    <Field
                                                        id={'fromDate *'}
                                                        name={'fromDate'}
                                                        label={'From Date'}
                                                        component={FormDatePicker}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid.Column>

                                                <Grid.Column>
                                                    <Field
                                                        id={'thruDate'}
                                                        name={'thruDate'}
                                                        label={'To Date'}
                                                        component={FormDatePicker}
                                                    />
                                                </Grid.Column>
                                            </Grid.Row>
                                        </Grid>

                                        <div className="k-form-buttons">
                                            <Button
                                                primary={true}
                                                type={'submit'}
                                                disabled={!formRenderProps.allowSubmit || buttonFlag}
                                            >
                                                Submit
                                            </Button>
                                            <Button onClick={cancelEdit}>
                                                Cancel
                                            </Button>
                                        </div>
                                        {buttonFlag && <LoadingComponent message='Processing Party Contact...'/>}

                                    </fieldset>

                                </FormElement>

                            )}
                        />
                    </Segment>
                </Grid.Column>
            </Grid>

        </>


    );
}


