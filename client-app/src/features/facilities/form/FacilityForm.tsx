import React, {useState} from "react";
import {Button, Grid, Paper, Typography} from "@mui/material";
import FormTextArea from "../../../app/common/form/FormTextArea";
import FormInput from "../../../app/common/form/FormInput";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {useAppDispatch} from "../../../app/store/configureStore";
import agent from "../../../app/api/agent";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import {requiredValidator} from "../../../app/common/form/Validators";
import {v4 as uuid} from "uuid";
import {Facility} from "../../../app/models/facility/facility";
import {useFetchFacilityTypesQuery} from "../../../app/store/apis";
import {setFacility} from "../slice/FacilitySlice";
import FacilityMenu from "../menu/FacilityMenu";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

interface Props {
    facility?: Facility;
    editMode: number;
    cancelEdit: () => void;
}

export default function FacilityForm({
                                         facility,
                                         cancelEdit,
                                         editMode,
                                     }: Props) {
    const {data: facilityTypes} = useFetchFacilityTypesQuery(undefined);

    const dispatch = useAppDispatch();

    const [buttonFlag, setButtonFlag] = useState(false);
    const {getTranslatedLabel} = useTranslationHelper()

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            let response: any;
            if (editMode === 2) {
                response = await agent.Facilities.updateFacility(data);
            } else {
                data.facilityId = uuid();
                response = await agent.Facilities.createFacility(data);
            }
            dispatch(setFacility(response));
            cancelEdit();
        } catch (error) {
            console.log(error);
        }
        setButtonFlag(false);
    }

    return (
        <>
            {editMode > 1 && <FacilityMenu/>}
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                {editMode === 1 && (
                    <Typography variant="h4" color={"green"}>
                        {" "}
                        New Facility{" "}
                    </Typography>
                )}
                <Form
                    initialValues={editMode === 2 ? facility : undefined}
                    onSubmit={(values) => handleSubmitData(values)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={"k-form-fieldset"}>
                                <Grid container spacing={2}>
                                    <Grid item xs={3}>
                                        <Field
                                            id={"facilityName"}
                                            name={"facilityName"}
                                            label={getTranslatedLabel("facility.facilities.form.name", "Facility Name")}
                                            component={FormInput}
                                            autoComplete={"off"}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    <Grid item xs={3}>
                                        <Field
                                            id={"facilityTypeId"}
                                            name={"facilityTypeId"}
                                            label={getTranslatedLabel("facility.facilities.form.type", "Facility Type")}
                                            component={MemoizedFormDropDownList}
                                            dataItemKey={"facilityTypeId"}
                                            textField={"description"}
                                            data={facilityTypes ? facilityTypes : []}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    <Grid item xs={12}>
                                        <Field
                                            id={"description"}
                                            name={"description"}
                                            label={getTranslatedLabel("facility.facilities.form.description", "Description")}
                                            autoComplete={"off"}
                                            rows={3}
                                            component={FormTextArea}
                                        />
                                    </Grid>
                                </Grid>
                                <div className="k-form-buttons">
                                    <Grid container spacing={1}>
                                        <Grid item xs={2}>
                                            <Button
                                                type={"submit"}
                                                color="success"
                                                variant="contained"
                                                disabled={!formRenderProps.allowSubmit || buttonFlag}
                                            >
                                                Submit
                                            </Button>
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Button
                                                onClick={cancelEdit}
                                                variant="contained"
                                                color="error"
                                            >
                                                Cancel
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </div>
                                {buttonFlag && (
                                    <LoadingComponent message="Processing Facility..."/>
                                )}
                            </fieldset>
                        </FormElement>
                    )}
                />

                {/* </Grid.Column> */}
            </Paper>
        </>
    );
}
