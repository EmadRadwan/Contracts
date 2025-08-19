import React, {useState} from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import FormTextArea from '../../../app/common/form/FormTextArea';
import FormInput from '../../../app/common/form/FormInput';
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {FormComboBox} from "../../../app/common/form/FormComboBox";
import { useFetchCountriesQuery } from "../../../app/store/configureStore";
import agent from "../../../app/api/agent";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {phoneValidator, requiredValidator} from "../../../app/common/form/Validators";


interface Props {
    onClose: () => void;
    onUpdateCustomerDropDown?: (newCustomer: any) => void;
}

export default function CreateCustomerModalForm({onClose, onUpdateCustomerDropDown}: Props) {
    // const {geoLoaded} = useAppSelector(state => state.geo);
    // const geoCountry = useAppSelector(geoSelectors.selectAll);
    const {data: geoCountry} = useFetchCountriesQuery(undefined)
    const [buttonFlag, setButtonFlag] = useState(false);
    // const dispatch = useAppDispatch();

    console.count("CreateCustomerModalForm Rendered");


    // useEffect(() => {
    //     if (!geoLoaded) dispatch(fetchGeosAsync());
    // }, [geoLoaded, dispatch]);


    async function handleSubmitData(data: any) {
        setButtonFlag(true)
        try {
            let response: any;
            response = await agent.Parties.createCustomer(data);
            if (onUpdateCustomerDropDown) onUpdateCustomerDropDown(response);
            onClose();
        } catch (error) {
            console.log(error)
        }
        setButtonFlag(false)
    }


    return <React.Fragment>
        <Form
            onSubmit={values => handleSubmitData(values)}
            render={(formRenderProps) => (

                <FormElement>
                    <fieldset className={'k-form-fieldset'}>

                        <Field
                            id={'firstName'}
                            name={'firstName'}
                            label={'First Name *'}
                            component={FormInput}
                            autoComplete={"off"}
                            validator={requiredValidator}
                        />


                        <Field
                            id={'mobileContactNumber'}
                            name={'mobileContactNumber'}
                            label={'Mobile Contact Number'}
                            autoComplete={"off"}
                            component={FormInput}
                            validator={phoneValidator}

                        />

                        <Field
                            id={'infoString'}
                            name={'infoString'}
                            label={'Email Address'}
                            component={FormInput}
                            autoComplete={"off"}

                        />

                        <Field
                            id={'address1'}
                            name={'address1'}
                            label={'Address 1'}
                            component={FormInput}
                            autoComplete={"off"}

                        />

                        <Field
                            id={'address2'}
                            name={'address2'}
                            label={'Address 2'}
                            rows={3}
                            component={FormTextArea}
                            autoComplete={"off"}

                        />

                        <Field
                            id={"geoId"}
                            name={"geoId"}
                            label={"Country Code"}
                            component={FormComboBox}
                            dataItemKey={"geoId"}
                            textField={"geoName"}
                            autoComplete={"off"}
                            data={geoCountry ?? []}
                        />
                        <div className="k-form-buttons">
                            <Grid container rowSpacing={2}>
                                <Grid item xs={3}>
                                    <Button
                                        variant="contained"
                                        type={'submit'}
                                        disabled={!formRenderProps.allowSubmit || buttonFlag}
                                    >
                                        Submit
                                    </Button>
                                </Grid>
                                <Grid item xs={1}>
                                    <Button onClick={onClose} variant="contained">
                                        Cancel
                                    </Button>
                                </Grid>

                            </Grid>
                        </div>


                        {buttonFlag && <LoadingComponent message='Processing Customer...'/>}
                    </fieldset>

                </FormElement>

            )}
        />
    </React.Fragment>
}