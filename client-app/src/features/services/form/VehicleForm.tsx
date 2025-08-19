import React, {useState} from 'react';
import {Container, Grid, Paper, Typography} from "@mui/material";
import Button from "@mui/material/Button";
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {Vehicle} from "../../../app/models/service/vehicle";
import {
    useFetchVehicleContentsQuery,
    useFetchVehicleExteriorColorsQuery,
    useFetchVehicleInteriorColorsQuery,
    useFetchVehicleMakesQuery,
    useFetchVehicleModelsByMakeIdQuery,
    useFetchVehicleTransmissionTypesQuery,
    useFetchVehicleTypesQuery
} from "../../../app/store/configureStore";
import {FormComboBoxVirtualCustomer} from "../../../app/common/form/FormComboBoxVirtualCustomer";
import {requiredValidator} from "../../../app/common/form/Validators";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import {useCreateVehicleMutation, useUpdateVehicleMutation} from "../../../app/store/apis";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import FormInput from "../../../app/common/form/FormInput";
import {toast} from "react-toastify";
import VehicleFiles from './VehicleFiles';

interface Props {
    selectedVehicle?: Vehicle;
    editMode: number;
    cancelEdit: () => void;
}


export default function VehicleForm({selectedVehicle, cancelEdit, editMode}: Props) {

    console.log('editMode', editMode);
    const [selectedVehicleMake, setSelectedVehicleMake] = useState(undefined);


    const onVehicleMakeDropdownChange = React.useCallback(
        (event) => {
            const make = event.value;
            setSelectedVehicleMake(make);
        },
        [setSelectedVehicleMake]
    );

    const {data: vehicleContents}
        = useFetchVehicleContentsQuery(selectedVehicle?.vehicleId,
        {skip: selectedVehicle?.vehicleId === undefined});
    console.log('selectedVehicleMake', selectedVehicleMake);
    console.log('selectedVehicle', selectedVehicle);

    const {data: vehicleMakes}
        = useFetchVehicleMakesQuery(undefined);

    const {data: vehicleModels}
        = useFetchVehicleModelsByMakeIdQuery(selectedVehicleMake || selectedVehicle?.makeId,
        {skip: selectedVehicleMake === undefined && selectedVehicle?.makeId === undefined});

    const {data: vehicleInteriorColors} = useFetchVehicleInteriorColorsQuery(undefined);


    const {data: vehicleExteriorColors} = useFetchVehicleExteriorColorsQuery(undefined);

    const {data: vehicleTypes} = useFetchVehicleTypesQuery(undefined);

    const {data: vehicleTransmissionTypes} = useFetchVehicleTransmissionTypesQuery(undefined);
    const [addVehicle, {
        data: addVehicleResult,
        error: addVehicleError,
        isLoading: isAddVehicleLoading
    }] = useCreateVehicleMutation();
    const [updateVehicle, {
        data: updateVehicleResult,
        error: updateVehicleError,
        isLoading: isVehicleLoading
    }] = useUpdateVehicleMutation();


    async function handleSubmitData(data: any) {
        if (editMode === 2) {
            try {
                const updatedVehicle = await updateVehicle(data).unwrap()
                toast.success('Vehicle Updated Successfully')
                //cancelEdit();
            } catch (error: any) {
                toast.error(error.data.title);
            }

        } else {
            try {
                const createdVehicle = await addVehicle(data).unwrap()
                toast.success('Vehicle Created Successfully')
                console.log('createdVehicle', createdVehicle);

            } catch (error: any) {
                toast.error(error.data.title);
            }
        }
    }


    return (
        // <Container maxWidth="lg">
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Typography variant="h5" gutterBottom>
                    Vehicle Information
                </Typography>
                <Grid container spacing={2}>
                    <Grid item xs={12}>
                        {/* <div className="div-container-withBorderCurved"> */}
                            <Form
                                initialValues={editMode === 2 ? selectedVehicle : undefined}
                                onSubmit={values => handleSubmitData(values as Vehicle)}
                                render={(formRenderProps) => (
                                    <FormElement>
                                        <fieldset className={'k-form-fieldset'}>
                                            <Grid container spacing={2} pb={2}>
                                                <Grid item xs={12}>
                                                    <Typography variant="subtitle1">
                                                        Customer Details
                                                    </Typography>
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"fromPartyId"}
                                                        name={"fromPartyId"}
                                                        label={"Customer"}
                                                        component={FormComboBoxVirtualCustomer}
                                                        autoComplete={"off"}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
                                            </Grid>

                                            <Typography variant="subtitle1" gutterBottom>
                                                Vehicle Identity
                                            </Typography>
                                            <Grid container spacing={2}>
                                                {/* ... Chassis Number, Plate Number, etc ... */}
                                                <Grid item xs={4}>
                                                    <Field
                                                        id={'chassisNumber'}
                                                        name={'chassisNumber'}
                                                        label={'Chassis Number *'}
                                                        component={FormInput}
                                                        autoComplete={"off"}
                                                        disabled={editMode === 2}
                                                    />
                                                </Grid>

                                                <Grid item xs={3}>
                                                    <Field
                                                        id={'plateNumber'}
                                                        name={'plateNumber'}
                                                        label={'Plate Number *'}
                                                        component={FormInput}
                                                        autoComplete={"off"}
                                                    />
                                                </Grid>
                                            </Grid>

                                            <Typography variant="subtitle1" gutterBottom>
                                                Vehicle Details
                                            </Typography>
                                            <Grid container spacing={2}>
                                                {/* ... Make, Model, Body Type, etc ... */}
                                                <Grid item xs={4}>
                                                    <Field
                                                        id={"makeId"}
                                                        name={"makeId"}
                                                        label={"Make *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"makeId"}
                                                        textField={"makeDescription"}
                                                        data={vehicleMakes ? vehicleMakes : []}
                                                        validator={requiredValidator}
                                                        onChange={onVehicleMakeDropdownChange}
                                                    />
                                                </Grid>

                                                <Grid item xs={4}>
                                                    <Field
                                                        id={"modelId"}
                                                        name={"modelId"}
                                                        label={"Model *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"modelId"}
                                                        textField={"modelDescription"}
                                                        data={vehicleModels ? vehicleModels : []}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>

                                                <Grid item xs={4}>
                                                    <Field
                                                        id={"vehicleTypeId"}
                                                        name={"vehicleTypeId"}
                                                        label={"Body Type *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"vehicleTypeId"}
                                                        textField={"vehicleTypeDescription"}
                                                        data={vehicleTypes ? vehicleTypes : []}
                                                        validator={requiredValidator}
                                                        //disabled={editMode === 2}
                                                    />
                                                </Grid>

                                                <Grid item xs={4}>
                                                    <Field
                                                        id={"transmissionTypeId"}
                                                        name={"transmissionTypeId"}
                                                        label={"Transmission Type *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"transmissionTypeId"}
                                                        textField={"transmissionTypeDescription"}
                                                        data={vehicleTransmissionTypes ? vehicleTransmissionTypes : []}
                                                        validator={requiredValidator}
                                                        //disabled={editMode === 2}
                                                    />
                                                </Grid>

                                                <Grid item xs={4}>
                                                    <Field
                                                        id={"exteriorColorId"}
                                                        name={"exteriorColorId"}
                                                        label={"Exterior Color *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"exteriorColorId"}
                                                        textField={"exteriorColorDescription"}
                                                        data={vehicleExteriorColors ? vehicleExteriorColors : []}
                                                        validator={requiredValidator}
                                                        //disabled={editMode === 2}
                                                    />
                                                </Grid>

                                                <Grid item xs={4}>
                                                    <Field
                                                        id={"interiorColorId"}
                                                        name={"interiorColorId"}
                                                        label={"Interior Color *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"interiorColorId"}
                                                        textField={"interiorColorDescription"}
                                                        data={vehicleInteriorColors ? vehicleInteriorColors : []}
                                                        validator={requiredValidator}
                                                        //disabled={editMode === 2}
                                                    />
                                                </Grid>
                                            </Grid>

                                            <div className="k-form-buttons">
                                                <Grid container rowSpacing={2}>
                                                    {/* ... Submit and Cancel Buttons ... */}
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
                                                    <Grid item xs={2}>
                                                        <Button onClick={cancelEdit} color="error" variant="contained">
                                                            Cancel
                                                        </Button>
                                                    </Grid>
                                                </Grid>
                                            </div>

                                            {isVehicleLoading && <LoadingComponent message='Processing Vehicle...'/>}
                                        </fieldset>
                                    </FormElement>
                                )}
                            />
                        {/* </div> */}
                    </Grid>
                    <Grid item xs={6}>
                        {editMode === 2 &&
                            <VehicleFiles vehicle={selectedVehicle} attachedFiles={vehicleContents || []}/>}
                    </Grid>
                </Grid>
            </Paper>
        // {/* </Container> */}
    );
}




