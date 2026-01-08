import React, {useState} from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import FormTextArea from '../../../app/common/form/FormTextArea';
import FormInput from '../../../app/common/form/FormInput';
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {FormComboBox} from "../../../app/common/form/FormComboBox";
import {useFetchCountriesQuery} from "../../../app/store/configureStore";
import agent from "../../../app/api/agent";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {phoneValidator, requiredValidator} from "../../../app/common/form/Validators";

interface Props {
    onClose: () => void;
    onPartyCreated?: (newParty: any) => void;
    initialRole?: 'CONTRACTOR' | 'CUSTOMER' | 'EMPLOYEE' | 'SUPPLIER'; // Optional: preselect role
}

const partyRoleOptions = [
    {mainRole: 'CUSTOMER', description: 'عميل'},
    {mainRole: 'CONTRACTOR', description: 'مقاول'},
    {mainRole: 'EMPLOYEE', description: 'موظف'},
    {mainRole: 'SUPPLIER', description: 'مورد'},
];

export default function CreatePartyModalForm({
                                                 onClose,
                                                 onPartyCreated,
                                                 initialRole
                                             }: Props) {
    const {data: geoCountry} = useFetchCountriesQuery(undefined);
    const [buttonFlag, setButtonFlag] = useState(false);

    // REFACTOR: Extract submit logic into async function for clarity and reuse
    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            const response = await agent.Parties.createParty({
                ...data
            });

            if (onPartyCreated) onPartyCreated(response);
            onClose();
        } catch (error) {
            console.error('Error creating party:', error);
        } finally {
            setButtonFlag(false);
        }
    }

    return (
        <Form
            onSubmit={handleSubmitData}
            render={(formRenderProps) => (
                <FormElement>
                    <fieldset className={'k-form-fieldset'}>

                        {/* REFACTOR: Added Party Role dropdown with Arabic labels */}
                        <Field
                            id={'mainRole'}
                            name={'mainRole'}
                            label={'دور الطرف *'}
                            component={FormComboBox}
                            data={partyRoleOptions}
                            dataItemKey={'mainRole'}
                            textField={'description'}
                            validator={requiredValidator}
                            filterable={false}
                        />

                        <Field
                            id={'firstName'}
                            name={'firstName'}
                            label={'الاسم الأول *'}
                            component={FormInput}
                            autoComplete={"off"}
                            validator={requiredValidator}
                        />

                        <Field
                            id={'mobileContactNumber'}
                            name={'mobileContactNumber'}
                            label={'رقم الجوال'}
                            component={FormInput}
                            autoComplete={"off"}
                        />

                        <Field
                            id={'infoString'}
                            name={'infoString'}
                            label={'البريد الإلكتروني'}
                            component={FormInput}
                            autoComplete={"off"}
                        />

                        <Field
                            id={'address1'}
                            name={'address1'}
                            label={'العنوان 1'}
                            component={FormInput}
                            autoComplete={"off"}
                        />

                        <Field
                            id={'address2'}
                            name={'address2'}
                            label={'العنوان 2'}
                            component={FormTextArea}
                            autoComplete={"off"}
                        />

                        <Field
                            id={"geoId"}
                            name={"geoId"}
                            label={"الدولة"}
                            component={FormComboBox}
                            dataItemKey={"geoId"}
                            textField={"geoName"}
                            data={geoCountry ?? []}
                            autoComplete={"off"}
                        />

                        <div className="k-form-buttons">
                            <Grid container spacing={2} justifyContent="flex-start">
                                <Grid item>
                                    <Button
                                        variant="contained"
                                        type="submit"
                                        disabled={!formRenderProps.allowSubmit || buttonFlag}
                                        color="primary"
                                    >
                                        إنشاء
                                    </Button>
                                </Grid>
                                <Grid item>
                                    <Button onClick={onClose} variant="contained" color="inherit">
                                        إلغاء
                                    </Button>
                                </Grid>
                            </Grid>
                        </div>

                        {buttonFlag && <LoadingComponent message="جاري إنشاء الطرف..."/>}
                    </fieldset>
                </FormElement>
            )}
        />
    );
}