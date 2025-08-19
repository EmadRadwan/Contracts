import React from 'react'
import {Make} from '../../../app/models/service/vehicle'
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import FormInput from '../../../app/common/form/FormInput';
import {requiredValidator} from '../../../app/common/form/Validators';
import {Grid, Paper} from "@mui/material";
import Button from "@mui/material/Button";
import {useCreateVehicleMakeMutation, useUpdateVehicleMakeMutation} from '../../../app/store/apis';
import {toast} from "react-toastify";


interface Props {
    selectedMake: Make | undefined
    editMode: number
    cancelEdit: () => void
}

export default function MakesForm({selectedMake, editMode, cancelEdit}: Props) {

    const [addVehicleMake, {
        data: addVehicleMakeResult,
        error: addVehicleMakeError,
        isLoading: isAddVehicleMakeLoading
    }] = useCreateVehicleMakeMutation()
    const [updateVehicleMake, {
        data: updateVehicleMakeResult,
        error: updateVehicleMakeError,
        isLoading: isUpdateVehicleMakeLoading
    }] = useUpdateVehicleMakeMutation()

    async function handleSubmitData(data: any) {
        if (editMode === 2) {
            try {
                const updatedMake = await updateVehicleMake(data).unwrap()
                toast.success('Vehicle Make Updated Successfully')
                cancelEdit();
            } catch (error: any) {
                toast.error(error.data.title)
            }
        } else {
            try {
                const addedMake = await addVehicleMake(data).unwrap()
                toast.success('Vehicle Make Added Successfully')
                cancelEdit()
            } catch (error: any) {
                toast.error(error.data.title)
            }
        }
    }

    return (
        <>
            <Paper elevation={5} className={`div-container-withBorderCurved`} sx={{mt: 5}}>
                <Form
                    initialValues={editMode === 2 ? selectedMake : undefined}
                    onSubmit={values => handleSubmitData(values as Make)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Grid container spacing={2}>
                                    <Grid item xs={5}>
                                        <Field
                                            id={"makeId"}
                                            name={"makeId"}
                                            label={"Make *"}
                                            component={FormInput}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    <Grid item xs={5}>
                                        <Field
                                            id={"makeDescription"}
                                            name={"makeDescription"}
                                            label={"Make Description *"}
                                            component={FormInput}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                </Grid>
                                <div className="k-form-buttons">
                                    <Grid container rowSpacing={2}>
                                        <Grid item xs={1}>
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
                            </fieldset>
                        </FormElement>
                    )}
                >

                </Form>
            </Paper>
        </>
    )
}