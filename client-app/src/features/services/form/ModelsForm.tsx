import React, {useCallback, useState} from 'react'
import {Make, Model} from '../../../app/models/service/vehicle'
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import FormInput from '../../../app/common/form/FormInput';
import {requiredValidator} from '../../../app/common/form/Validators';
import {Grid, Paper} from "@mui/material";
import Button from "@mui/material/Button";
import {
    useCreateVehicleModelMutation,
    useFetchVehicleMakesQuery,
    useUpdateVehicleModelMutation
} from '../../../app/store/apis';
import {toast} from "react-toastify";
import {MemoizedFormDropDownList} from '../../../app/common/form/MemoizedFormDropDownList';


interface Props {
    selectedModel: Model | undefined
    editMode: number
    cancelEdit: () => void
}

export default function ModelsForm({selectedModel, editMode, cancelEdit}: Props) {

    const [addVehicleModel, {
        data: addVehicleMakeResult,
        error: addVehicleMakeError,
        isLoading: isAddVehicleMakeLoading
    }] = useCreateVehicleModelMutation()
    const [updateVehicleModel, {
        data: updateVehicleMakeResult,
        error: updateVehicleMakeError,
        isLoading: isUpdateVehicleMakeLoading
    }] = useUpdateVehicleModelMutation()
    const {data: makesData, error, isFetching} = useFetchVehicleMakesQuery(undefined)
    const [make, setMake] = useState<Make>()

    async function handleSubmitData(data: any) {
        if (editMode === 2) {
            try {
                const updatedMake = await updateVehicleModel(data).unwrap()
                toast.success('Vehicle Model Updated Successfully')
                cancelEdit();
            } catch (error: any) {
                toast.error(error.data.title)
            }
        } else {
            try {
                const addedMake = await addVehicleModel(data).unwrap()
                toast.success('Vehicle Model Added Successfully')
                cancelEdit()
            } catch (error: any) {
                toast.error(error.data.title)
            }
        }
    }

    const onMakeChange = useCallback(
        (event) => {
            const make = event.value
            setMake(make)
        }, [setMake]
    )

    return (
        <div className="div-container">
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Form
                    initialValues={editMode === 2 ? selectedModel : undefined}
                    onSubmit={values => handleSubmitData(values as Model)}
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
                                            data={makesData ? makesData : []}
                                            dataItemKey={"makeId"}
                                            textField={"makeDescription"}
                                            validator={requiredValidator}
                                            onChange={onMakeChange}
                                        />
                                    </Grid>
                                    <Grid item container spacing={2} xs={12}>
                                        <Grid item xs={6}>
                                            <Field
                                                id={"modelId"}
                                                name={"modelId"}
                                                label={"Model *"}
                                                component={FormInput}
                                                validator={requiredValidator}
                                                disabled={!make}
                                            />
                                        </Grid>
                                        <Grid item xs={6}>
                                            <Field
                                                id={"modelDescription"}
                                                name={"modelDescription"}
                                                label={"Model Description *"}
                                                component={FormInput}
                                                validator={requiredValidator}
                                                disabled={!make}
                                            />
                                        </Grid>
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
        </div>
    )
}