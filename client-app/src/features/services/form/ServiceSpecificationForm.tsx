import React, {useEffect, useState} from 'react';
import {Container, Grid, Paper, Typography} from "@mui/material";
import Button from "@mui/material/Button";
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {ServiceSpecification} from "../../../app/models/service/serviceSpecification";
import {requiredValidator} from "../../../app/common/form/Validators";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import {
    useCreateServiceSpecificationMutation,
    useFetchVehicleMakesQuery,
    useFetchVehicleModelsByMakeIdQuery,
    useUpdateServiceSpecificationMutation
} from "../../../app/store/apis";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {toast} from "react-toastify";
import {FormComboBoxVirtualServiceProduct} from '../../../app/common/form/FormComboBoxVirtualServiceProduct';
import FormNumericTextBox from '../../../app/common/form/FormNumericTextBox';

interface Props {
    selectedServiceSpecification?: ServiceSpecification;
    editMode: number;
    cancelEdit: () => void;
}


export default function ServiceSpecificationForm({selectedServiceSpecification, cancelEdit, editMode}: Props) {

    const [selectedServiceSpecificationMake, setSelectedServiceSpecificationMake] = useState<string | undefined>(undefined);
    console.log('selectedServiceSpecification', selectedServiceSpecification);

    useEffect(() => {
        if (selectedServiceSpecification) {
            setSelectedServiceSpecificationMake(selectedServiceSpecification.makeId);
        }
    }, [selectedServiceSpecification])


    const onServiceSpecificationMakeDropdownChange = React.useCallback(
        (event) => {
            const make = event.value;
            setSelectedServiceSpecificationMake(make);
        },
        [setSelectedServiceSpecificationMake]
    );


    const {data: serviceSpecificationMakes}
        = useFetchVehicleMakesQuery(undefined);

    const {data: serviceSpecificationModels}
        = useFetchVehicleModelsByMakeIdQuery(selectedServiceSpecificationMake || selectedServiceSpecification?.makeId,
        {skip: selectedServiceSpecificationMake === undefined});
    console.log('selectedServiceSpecificationMake', selectedServiceSpecificationMake);

    const [addServiceSpecification, {
        data: addServiceSpecificationResult,
        error: addServiceSpecificationError,
        isLoading: isAddServiceSpecificationLoading
    }] = useCreateServiceSpecificationMutation();
    const [updateServiceSpecification, {
        data: updateServiceSpecificationResult,
        error: updateServiceSpecificationError,
        isLoading: isServiceSpecificationLoading
    }] = useUpdateServiceSpecificationMutation();


    async function handleSubmitData(data: any) {
        if (editMode === 2) {
            try {
                const updatedServiceSpecification = await updateServiceSpecification(data).unwrap()
                toast.success('ServiceSpecification Updated Successfully')
                cancelEdit();
            } catch (error: any) {
                toast.error(error.data.title);
            }

        } else {
            try {
                const createdServiceSpecification = await addServiceSpecification(data).unwrap()
                toast.success('ServiceSpecification Created Successfully')
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
                            {` ${editMode === 1 ? "New" : ""} Service Specification ${editMode > 1 ? `for ${selectedServiceSpecification?.productName} in ${selectedServiceSpecification?.makeDescription} ${selectedServiceSpecification?.modelDescription}` : ""}`}
                        </Typography>
                            <Form
                                initialValues={editMode === 2 ? selectedServiceSpecification : undefined}
                                onSubmit={values => handleSubmitData(values as ServiceSpecification)}
                                render={(formRenderProps) => (

                                    <FormElement>
                                        <fieldset className={'k-form-fieldset'}>

                                            <Grid container spacing={2}>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"productId"}
                                                        name={"productId"}
                                                        label={"Product"}
                                                        component={FormComboBoxVirtualServiceProduct}
                                                        autoComplete={"off"}
                                                        validator={requiredValidator}
                                                        disabled={editMode === 2}
                                                    />
                                                </Grid>
    
                                                <Grid container item xs={12} spacing={2}>
                                                    <Grid item xs={6}>
                                                        <Field
                                                            id={"makeId"}
                                                            name={"makeId"}
                                                            label={"Make *"}
                                                            component={MemoizedFormDropDownList}
                                                            dataItemKey={"makeId"}
                                                            textField={"makeDescription"}
                                                            data={serviceSpecificationMakes ? serviceSpecificationMakes : []}
                                                            validator={requiredValidator}
                                                            onChange={onServiceSpecificationMakeDropdownChange}
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
                                                            data={serviceSpecificationModels ? serviceSpecificationModels : []}
                                                            validator={requiredValidator}
                                                        />
                                                    </Grid>
                                                </Grid>
    
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={'standardTimeInMinutes'}
                                                        format="n0"
                                                        min={1}
                                                        name={'standardTimeInMinutes'}
                                                        label={'Standard Time In Minutes *'}
                                                        component={FormNumericTextBox}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
    
                                                <Grid container item xs={12} spacing={2}>
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

                                            {isServiceSpecificationLoading &&
                                                <LoadingComponent message='Processing Service Specification...'/>}

                                        </fieldset>

                                    </FormElement>

                                )}
                            />
                    </Grid>
                </Grid>
            </Paper>
    );
}

