import React, {useState} from "react";
import Button from "@mui/material/Button";
import Grid from "@mui/material/Grid";
import FormTextArea from "../../../app/common/form/FormTextArea";
import FormInput from "../../../app/common/form/FormInput";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {FormComboBox} from "../../../app/common/form/FormComboBox";
import {Party} from "../../../app/models/party/party";
import {useAppDispatch, useFetchCountriesQuery} from "../../../app/store/configureStore";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {setParty} from "../slice/partySlice";
import {Box, Paper, Typography} from "@mui/material";
import CreateCustomerMenu from "../menu/CreateCustomerMenu";
import {setSingleParty} from "../slice/singlePartySlice";
import {requiredValidator} from "../../../app/common/form/Validators";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {
    useCreateSalesRepMutation,
    useFetchSalesRepQuery,
    useUpdateSalesRepMutation
} from "../../../app/store/apis";

interface Props {
    party?: Party;
    editMode: number;
    cancelEdit: () => void;
}

export default function CreateSalesRepForm({
                                               party,
                                               cancelEdit,
                                               editMode,
                                           }: Props) {
    const {data: countries, isSuccess: isCountriesLoaded} = useFetchCountriesQuery(undefined)
    const [buttonFlag, setButtonFlag] = useState(false);
    const {getTranslatedLabel} = useTranslationHelper();
    const [creationSuccess, setCreationSuccess] = useState<{
        partyId: string;
    } | null>(null);

    const {
        data: salesRep,
        isFetching,
    } = useFetchSalesRepQuery(party?.partyId, {
        skip: party?.partyId === undefined || editMode !== 2,
    });

    const [createSalesRep, { isLoading: isCreating }] = useCreateSalesRepMutation();
    const [updateSalesRep, { isLoading: isUpdating }] = useUpdateSalesRepMutation();

    const isMutating = isCreating || isUpdating || buttonFlag;


    const dispatch = useAppDispatch();

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            let response: any;
            if (editMode === 2) {
                response = await updateSalesRep(data).unwrap();
            } else {
                response = await createSalesRep(data).unwrap();
            }
            setCreationSuccess({
                partyId: response.partyId,
            });
            dispatch(setParty(response));
            dispatch(setSingleParty(response));
        } catch (error) {
            console.log(error);
        }
        setButtonFlag(false);
    }

    return (
        <>
            <Paper
                elevation={5}
                className={`div-container-withBorderCurved`}
                sx={{mt: 5}}
            >
                {isFetching && (
                    <LoadingComponent
                        message={getTranslatedLabel("party.salesReps.form.loading", "Loading Sales Rep...")}/>
                )}

                {creationSuccess && (
                    <Box sx={{p: 3, mb: 3, bgcolor: '#e3f2fd', borderRadius: 2, border: '1px solid #90caf9'}}>
                        <Typography variant="h5" color="primary" gutterBottom>
                            {getTranslatedLabel("party.salesReps.form.success", "Sales Rep Saved Successfully!")}
                        </Typography>

                        <Typography variant="body1" gutterBottom>
                            {getTranslatedLabel("party.salesReps.form.salesRepId", "Sales Rep ID")}:{" "}
                            <strong>{creationSuccess.partyId}</strong>
                        </Typography>

                        <Box sx={{mt: 3}}>
                            <Button
                                variant="contained"
                                color="primary"
                                onClick={cancelEdit}
                                sx={{mr: 2}}
                            >
                                {getTranslatedLabel("party.salesReps.form.done", "Done / Close")}
                            </Button>

                            <Button
                                variant="outlined"
                                onClick={() => {
                                    setCreationSuccess(null);
                                }}
                            >
                                {getTranslatedLabel("party.salesReps.form.createAnother", "Create Another Sales Rep")}
                            </Button>
                        </Box>
                    </Box>
                )}

                {!creationSuccess && (
                    <>
                        <Grid container spacing={2}>
                            <Grid item xs={8}>
                                <Box display="flex" justifyContent="space-between" paddingBottom={4}>
                                    <Typography sx={{p: 2}} variant="h4" color={editMode === 1 ? "green" : "black"}>
                                        {editMode === 1
                                            ? getTranslatedLabel("party.salesReps.form.new", "New Sales Rep")
                                            : salesRep && salesRep?.description}
                                    </Typography>
                                </Box>
                            </Grid>
                            <Grid item xs={4}>
                                <CreateCustomerMenu partyId={salesRep?.partyId} partyName={salesRep?.description}/>
                            </Grid>
                        </Grid>

                        <Form
                            initialValues={editMode === 2 ? salesRep : undefined}
                            key={JSON.stringify(salesRep)}
                            onSubmit={(values) => handleSubmitData(values)}
                            render={(formRenderProps) => (
                                <FormElement>
                                    <fieldset className={"k-form-fieldset"}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"groupName"}
                                                    name={"groupName"}
                                                    label={getTranslatedLabel("party.salesReps.form.groupName", "Sales Rep Name *")}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"infoString"}
                                                    name={"infoString"}
                                                    label={getTranslatedLabel("party.salesReps.form.email", "Email Address")}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                {isCountriesLoaded && <Field
                                                    id={"geoId"}
                                                    name={"geoId"}
                                                    label={getTranslatedLabel("party.salesReps.form.countryCode", "Country Code")}
                                                    component={FormComboBox}
                                                    dataItemKey={"geoId"}
                                                    textField={"geoName"}
                                                    autoComplete={"off"}
                                                    data={countries}
                                                />}
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"mobileContactNumber"}
                                                    name={"mobileContactNumber"}
                                                    label={getTranslatedLabel("party.salesReps.form.mobileContactNumber", "Mobile Contact Number")}
                                                    autoComplete={"off"}
                                                    component={FormInput}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"address1"}
                                                    name={"address1"}
                                                    label={getTranslatedLabel("party.salesReps.form.address1", "Address 1")}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"address2"}
                                                    name={"address2"}
                                                    label={getTranslatedLabel("party.salesReps.form.address2", "Address 2")}
                                                    rows={3}
                                                    component={FormTextArea}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                        </Grid>
                                        
                                        <div className="k-form-buttons">
                                            <Grid container rowSpacing={2}>
                                                <Grid item xs={1}>
                                                    <Button
                                                        variant="contained"
                                                        type={"submit"}
                                                        color="success"
                                                        disabled={!formRenderProps.allowSubmit || isMutating}
                                                    >
                                                        {getTranslatedLabel("party.salesReps.form.submit", "Submit")}
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={1}>
                                                    <Button
                                                        onClick={cancelEdit}
                                                        color="error"
                                                        variant="contained"
                                                    >
                                                        {getTranslatedLabel("party.salesReps.form.cancel", "Cancel")}
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </div>

                                        {isMutating && (
                                            <LoadingComponent
                                                message={getTranslatedLabel("party.salesReps.form.processing", "Processing Sales Rep...")}/>
                                        )}
                                    </fieldset>
                                </FormElement>
                            )}
                        />
                    </>
                )}
            </Paper>
        </>
    );
}
