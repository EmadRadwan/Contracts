import React, {useEffect, useState} from 'react';
import {Container, Grid, Paper, Typography} from "@mui/material";
import Button from "@mui/material/Button";
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {ServiceRate} from "../../../app/models/service/serviceRate";
import {requiredValidator} from "../../../app/common/form/Validators";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import {
    useCreateServiceRateMutation,
    useFetchVehicleMakesQuery,
    useFetchVehicleModelsQuery,
    useUpdateServiceRateMutation
} from "../../../app/store/apis";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {toast} from "react-toastify";
import FormNumericTextBox from '../../../app/common/form/FormNumericTextBox';

interface Props {
    selectedServiceRate?: ServiceRate;
    editMode: number;
    cancelEdit: () => void;
}


export default function ServiceRateForm({selectedServiceRate, cancelEdit, editMode}: Props) {

    const [filteredVehicleModels, setFilteredVehicleModels] = useState<any[] | undefined>(undefined);
    console.log('selectedServiceRate', selectedServiceRate);

    const {data: serviceRateMakes}
        = useFetchVehicleMakesQuery(undefined);

    const {data: serviceRateModels}
        = useFetchVehicleModelsQuery(undefined);

    const onServiceRateMakeDropdownChange = React.useCallback(
        (event) => {
            const make = event.value;
            if (serviceRateModels) {
                const filteredModels = serviceRateModels.filter((model) => model.makeId === make);
                setFilteredVehicleModels(filteredModels);
            }
        },
        [serviceRateModels]
    );

    useEffect(() => {
        if (editMode === 2) {
            if (selectedServiceRate) {
                if (serviceRateModels) {
                    const filteredModels = serviceRateModels.filter((model) => model.makeId === selectedServiceRate.makeId);
                    setFilteredVehicleModels(filteredModels);
                }
            }
        }
    }, [editMode, selectedServiceRate, serviceRateModels]);


    const [addServiceRate, {
        data: addServiceRateResult,
        error: addServiceRateError,
        isLoading: isAddServiceRateLoading
    }] = useCreateServiceRateMutation();
    const [updateServiceRate, {
        data: updateServiceRateResult,
        error: updateServiceRateError,
        isLoading: isServiceRateLoading
    }] = useUpdateServiceRateMutation();


    async function handleSubmitData(data: any) {
        if (editMode === 2) {
            try {
                const updatedServiceRate = await updateServiceRate(data).unwrap()
                toast.success('ServiceRate Updated Successfully')
                cancelEdit();
            } catch (error: any) {
                toast.error(error.data.title);
            }

        } else {
            try {
                const createdServiceRate = await addServiceRate(data).unwrap()
                toast.success('ServiceRate Created Successfully')
                cancelEdit();

            } catch (error: any) {
                toast.error(error.data.title);
            }
        }
    }


    return (
            <Paper elevation={3} className={`div-container-withBorderCurved`} sx={{mt: 5}}>
                <Grid container spacing={2}>
                    <Grid item xs={12} alignContent={"center"}>
                        <Typography variant="h5" gutterBottom color={editMode === 1 ? "green" : "black"}>
                            {` ${editMode === 1 ? "New" : ""} Service Rate ${editMode > 1 ? `for ${selectedServiceRate?.makeDescription} ${selectedServiceRate?.modelDescription}` : ""}`}
                        </Typography>

                            <Form
                                initialValues={editMode === 2 ? selectedServiceRate : undefined}
                                onSubmit={values => handleSubmitData(values as ServiceRate)}
                                render={(formRenderProps) => (

                                    <FormElement>
                                        <fieldset className={'k-form-fieldset'}>

                                            <Grid container spacing={2}>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"makeId"}
                                                        name={"makeId"}
                                                        label={"Make *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"makeId"}
                                                        textField={"makeDescription"}
                                                        data={serviceRateMakes ? serviceRateMakes : []}
                                                        validator={requiredValidator}
                                                        onChange={onServiceRateMakeDropdownChange}
                                                    />
                                                </Grid>
    
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"modelId"}
                                                        name={"modelId"}
                                                        label={"Model *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"modelId"}
                                                        textField={"modelDescription"}
                                                        data={filteredVehicleModels ? filteredVehicleModels : []}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
    
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={'rate'}
                                                        format="n0"
                                                        min={1}
                                                        name={'rate'}
                                                        label={'Service Rate - per hour - in Workshop Currency *'}
                                                        component={FormNumericTextBox}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
    
                                                <Grid item container xs={12} spacing={2}>
                                                    <Grid item xs={6}>
                                                        <Field
                                                            id={'fromDate'}
                                                            name={'fromDate'}
                                                            label={'From Date *'}
                                                            component={FormDatePicker}
                                                            validator={requiredValidator}
                                                            disabled={editMode === 2}
                                                        />
                                                    </Grid>
        
                                                    <Grid item xs={6}>
                                                        <Field
                                                            id={'thruDate'}
                                                            name={'thruDate'}
                                                            label={'To Date'}
                                                            component={FormDatePicker}
                                                        />
                                                    </Grid>
                                                </Grid>
                                            </Grid>


                                            <div className="k-form-buttons">
                                                <Grid container rowSpacing={2} style={{padding: '10px'}}>
                                                    <Grid item xs={2}>
                                                        <Button
                                                            variant="contained"
                                                            type={'submit'}
                                                            color='success'
                                                            disabled={!formRenderProps.allowSubmit}
                                                        >
                                                            Submit
                                                        </Button>
                                                    </Grid>
                                                    <Grid item xs={1}>
                                                        <Button onClick={cancelEdit} color='error' variant="contained">
                                                            Cancel
                                                        </Button>
                                                    </Grid>

                                                </Grid>
                                            </div>

                                            {isServiceRateLoading &&
                                                <LoadingComponent message='Processing Service Rate...'/>}

                                        </fieldset>

                                    </FormElement>

                                )}
                            />
                    </Grid>
                </Grid>
            </Paper>
    );
}



