import React, {useCallback, useState} from 'react';
import {Container, Grid, Paper, Typography} from "@mui/material";
import Button from "@mui/material/Button";
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {Vehicle} from "../../../app/models/service/vehicle";
import {
    useCreateVehicleMutation,
    useFetchVehicleExteriorColorsQuery,
    useFetchVehicleInteriorColorsQuery,
    useFetchVehicleMakesQuery,
    useFetchVehicleModelsByMakeIdQuery,
    useFetchVehicleTransmissionTypesQuery,
    useFetchVehicleTypesQuery,
    useUpdateVehicleMutation
} from "../../../app/store/apis";
import {FormComboBoxVirtualCustomer} from "../../../app/common/form/FormComboBoxVirtualCustomer";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import FormInput from "../../../app/common/form/FormInput";
import {toast} from "react-toastify";
import VehicleFiles from './VehicleFiles';
import {useAppDispatch} from "../../../app/store/configureStore";
import CreateCustomerModalForm from "../../parties/form/CreateCustomerModalForm";
import {setCustomerId} from "../../orders/slice/sharedOrderUiSlice";
import {requiredValidator} from "../../../app/common/form/Validators";
import ModalContainer from "../../../app/common/modals/ModalContainer";

interface Props {
    onClose: () => void;
    onUpdateVehicleDropDown?: (newVehicle: any, customer: any) => void;
}


export default function VehicleFormModal({onClose, onUpdateVehicleDropDown}: Props) {

    const [selectedVehicleMake, setSelectedVehicleMake] = useState(undefined);
    const [createdVehicle, setCreatedVehicle] = useState<Vehicle | null>(null);
    const [showNewCustomer, setShowNewCustomer] = useState(false);
    const formRef = React.useRef<any>();
    const dispatch = useAppDispatch();


    const onVehicleMakeDropdownChange = React.useCallback(
        (event) => {
            const make = event.value;
            setSelectedVehicleMake(make);
        },
        [setSelectedVehicleMake]
    );


    const {data: vehicleMakes}
        = useFetchVehicleMakesQuery(undefined);

    const {data: vehicleModels}
        = useFetchVehicleModelsByMakeIdQuery(selectedVehicleMake,
        {skip: selectedVehicleMake === undefined});

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
        try {
            const createdVehicle = await addVehicle(data).unwrap();


            setCreatedVehicle(createdVehicle);

            toast.success('Vehicle Created Successfully');
            // Extracting vehicleId and chassisNumber from createdVehicle
            const {vehicleId, chassisNumber, fromPartyId} = createdVehicle;
            if (onUpdateVehicleDropDown) onUpdateVehicleDropDown({vehicleId, chassisNumber}, fromPartyId);
        } catch (error: any) {
            toast.error(error.data.title);
        }
    }

    const updateCustomerDropDown = (newCustomer: any) => {
        // Logic to update the DropDown in the parent with this new customer.
        formRef?.current.onChange('fromPartyId', {value: newCustomer.fromPartyId, valid: true});
        dispatch(setCustomerId(newCustomer.fromPartyId.fromPartyId));

    };


    const memoizedOnClose = useCallback(
        () => {
            setShowNewCustomer(false)
        },
        [],
    );

    return <React.Fragment>
        <Container maxWidth="lg">
            <Paper elevation={3} style={{padding: '20px', borderRadius: '10px'}}>
                <Typography variant="h5" gutterBottom>
                    New Vehicle
                </Typography>
                <Grid container spacing={2}>
                    <Grid item xs={6}>
                        <Form
                            ref={formRef}
                            onSubmit={values => handleSubmitData(values as Vehicle)}
                            render={(formRenderProps) => (
                                <FormElement>
                                    <fieldset className={'k-form-fieldset'}>

                                        <Grid container spacing={2} alignItems="flex-end">
                                            <Grid item xs={8}>
                                                <Field
                                                    id={"fromPartyId"}
                                                    name={"fromPartyId"}
                                                    label={"Customer"}
                                                    component={FormComboBoxVirtualCustomer}
                                                    autoComplete={"off"}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={4}>
                                                <Button onClick={() => {
                                                    setShowNewCustomer(true)
                                                }} variant="contained" size="small" color={"secondary"}>
                                                    New Customer
                                                </Button>
                                            </Grid>
                                        </Grid>


                                        <Grid container spacing={2}>
                                            {/* ... Chassis Number, Plate Number, etc ... */}
                                            <Grid item xs={4}>
                                                <Field
                                                    id={'chassisNumber'}
                                                    name={'chassisNumber'}
                                                    label={'Chassis Number *'}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>

                                            <Grid item xs={4}>
                                                <Field
                                                    id={'plateNumber'}
                                                    name={'plateNumber'}
                                                    label={'Plate Number *'}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                        </Grid>


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
                                                    label={"Transmission Type"}
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
                                                <Grid item xs={3}>
                                                    <Button
                                                        variant="contained"
                                                        type={'submit'}
                                                        disabled={!formRenderProps.allowSubmit}
                                                    >
                                                        Submit
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={2}>
                                                    <Button onClick={onClose} variant="contained">
                                                        Cancel
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </div>

                                        {isVehicleLoading &&
                                            <LoadingComponent message='Processing Vehicle...'/>}
                                    </fieldset>
                                </FormElement>
                            )}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        {createdVehicle &&
                            <VehicleFiles vehicle={createdVehicle}
                                          attachedFiles={[]}/>}
                    </Grid>
                </Grid>
            </Paper>
        </Container>
        {showNewCustomer && (<ModalContainer show={showNewCustomer} onClose={memoizedOnClose} width={500}>
            <CreateCustomerModalForm
                onClose={() => setShowNewCustomer(false)}
                onUpdateCustomerDropDown={updateCustomerDropDown}
            />
        </ModalContainer>)}
    </React.Fragment>
}



