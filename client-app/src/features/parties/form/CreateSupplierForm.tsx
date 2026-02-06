import React, {useState} from "react";
import Button from "@mui/material/Button";
import Grid from "@mui/material/Grid";
import FormTextArea from "../../../app/common/form/FormTextArea";
import FormInput from "../../../app/common/form/FormInput";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {FormComboBox} from "../../../app/common/form/FormComboBox";
import {Party} from "../../../app/models/party/party";
import {useAppDispatch, useFetchCountriesQuery, useFetchSupplierQuery,} from "../../../app/store/configureStore";
import agent from "../../../app/api/agent";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {setParty} from "../slice/partySlice";
import {Box, Paper, Typography} from "@mui/material";
import CreateCustomerMenu from "../menu/CreateCustomerMenu";
import {setSingleParty} from "../slice/singlePartySlice";
import {requiredValidator,} from "../../../app/common/form/Validators";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {
    useCreateEmployeeMutation,
    useCreateSupplierMutation,
    useUpdateEmployeeMutation,
    useUpdateSupplierMutation
} from "../../../app/store/apis";

interface Props {
    party?: Party;
    editMode: number;
    cancelEdit: () => void;
}

export default function CreateSupplierForm({
                                               party,
                                               cancelEdit,
                                               editMode,
                                           }: Props) {
    const {data: countries, isSuccess: isCountriesLoaded} = useFetchCountriesQuery(undefined)
    const [buttonFlag, setButtonFlag] = useState(false);
    const {getTranslatedLabel} = useTranslationHelper();
    const [creationSuccess, setCreationSuccess] = useState<{
        partyId: string;
        apGlAccountId: string | null;
        apGlAccountName: string | null;
        apGlAccountArabic: string | null;
    } | null>(null);

    const {
        data: supplier,
        error,
        isFetching,
        isLoading,
    } = useFetchSupplierQuery(party?.partyId, {
        skip: party?.partyId === undefined,
    });

    const [createSupplier, { isLoading: isCreating }] = useCreateSupplierMutation();
    const [updateSupplier, { isLoading: isUpdating }] = useUpdateSupplierMutation();

    const isMutating = isCreating || isUpdating || buttonFlag;


    const dispatch = useAppDispatch();

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            let response: any;
            if (editMode === 2) {
                response = await updateSupplier(data).unwrap();
            } else {
                response = await createSupplier(data).unwrap();
            }
            // after await agent.Parties.updateSupplier / createSupplier
            setCreationSuccess({
                partyId: response.partyId,
                apGlAccountId: response.createdApGlAccountId || null,
                apGlAccountName: response.createdApGlAccountName || null,
                apGlAccountArabic: response.createdApGlAccountArabicName || null,
            });
            dispatch(setParty(response));
            dispatch(setSingleParty(response));
        } catch (error) {
            console.log(error);
        }
        setButtonFlag(false);
    }

    console.log("supplier data:", supplier);

    return (
        <>
            <Paper
                elevation={5}
                className={`div-container-withBorderCurved`}
                sx={{mt: 5}}
            >
                {isFetching && (
                    <LoadingComponent
                        message={getTranslatedLabel("party.suppliers.form.loading", "Loading Supplier...")}/>
                )}

                {creationSuccess && (
                    <Box sx={{p: 3, mb: 3, bgcolor: '#e3f2fd', borderRadius: 2, border: '1px solid #90caf9'}}>
                        <Typography variant="h5" color="primary" gutterBottom>
                            {getTranslatedLabel("party.suppliers.form.success", "Supplier Saved Successfully!")}
                        </Typography>

                        <Typography variant="body1" gutterBottom>
                            {getTranslatedLabel("party.suppliers.form.supplierId", "Supplier ID")}:{" "}
                            <strong>{creationSuccess.partyId}</strong>
                        </Typography>

                        {creationSuccess.apGlAccountId && (
                            <>
                                <Typography variant="h6" sx={{mt: 2}} color="primary">
                                    {getTranslatedLabel("party.suppliers.form.newApAccount", "New Accounts Payable Sub-Account Created")}
                                </Typography>

                                <Grid container spacing={2} sx={{mt: 1}}>
                                    <Grid item xs={12} sm={6}>
                                        <Typography>
                                            <strong>{getTranslatedLabel("party.suppliers.form.glAccountId", "GL Account ID")}:</strong>{" "}
                                            {creationSuccess.apGlAccountId}
                                        </Typography>
                                    </Grid>
                                    <Grid item xs={12} sm={6}>
                                        <Typography>
                                            <strong>{getTranslatedLabel("party.suppliers.form.accountName", "Account Name")}:</strong>{" "}
                                            {creationSuccess.apGlAccountName || "—"}
                                        </Typography>
                                    </Grid>
                                    {creationSuccess.apGlAccountArabic && (
                                        <Grid item xs={12}>
                                            <Typography>
                                                <strong>
                                                    {getTranslatedLabel("party.suppliers.form.accountNameArabic", "Account Name (Arabic)")}:
                                                </strong>{" "}
                                                {creationSuccess.apGlAccountArabic}
                                            </Typography>
                                        </Grid>
                                    )}
                                </Grid>
                            </>
                        )}

                        <Box sx={{mt: 3}}>
                            <Button
                                variant="contained"
                                color="primary"
                                onClick={cancelEdit}
                                sx={{mr: 2}}
                            >
                                {getTranslatedLabel("party.suppliers.form.done", "Done / Close")}
                            </Button>

                            <Button
                                variant="outlined"
                                onClick={() => {
                                    setCreationSuccess(null);
                                    // Optional: reset form if you want to allow creating another immediately
                                }}
                            >
                                {getTranslatedLabel("party.suppliers.form.createAnother", "Create Another Supplier")}
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
                                            ? getTranslatedLabel("party.suppliers.form.new", "New Supplier")
                                            : supplier && supplier?.description}
                                    </Typography>
                                </Box>
                            </Grid>
                            <Grid item xs={4}>
                                <CreateCustomerMenu partyId={supplier?.partyId} partyName={supplier?.description}/>
                            </Grid>
                        </Grid>

                        <Form
                            initialValues={editMode === 2 ? supplier : undefined}
                            key={JSON.stringify(supplier)}
                            onSubmit={(values) => handleSubmitData(values)}
                            render={(formRenderProps) => (
                                <FormElement>
                                    <fieldset className={"k-form-fieldset"}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"groupName"}
                                                    name={"groupName"}
                                                    label={getTranslatedLabel("party.suppliers.form.groupName", "Supplier Name *")}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"infoString"}
                                                    name={"infoString"}
                                                    label={getTranslatedLabel("party.suppliers.form.email", "Email Address")}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                {isCountriesLoaded && <Field
                                                    id={"geoId"}
                                                    name={"geoId"}
                                                    label={getTranslatedLabel("party.suppliers.form.countryCode", "Country Code")}
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
                                                    label={getTranslatedLabel("party.suppliers.form.mobileContactNumber", "Mobile Contact Number")}
                                                    autoComplete={"off"}
                                                    component={FormInput}
                                                    //validator={phoneValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"address1"}
                                                    name={"address1"}
                                                    label={getTranslatedLabel("party.suppliers.form.address1", "Address 1")}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"address2"}
                                                    name={"address2"}
                                                    label={getTranslatedLabel("party.suppliers.form.address2", "Address 2")}
                                                    rows={3}
                                                    component={FormTextArea}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                        </Grid>
                                        {supplier?.linkedGlAccounts?.length > 0 ? (
                                            <Grid item xs={12} sx={{mt: 4}}>
                                                <Box
                                                    sx={{
                                                        p: 3,
                                                        bgcolor: "#e8f5e9",
                                                        borderRadius: 2,
                                                        border: "1px solid #81c784",
                                                    }}
                                                >
                                                    <Typography variant="h6" color="success.main" gutterBottom>
                                                        {getTranslatedLabel("party.suppliers.form.linkedAccounts", "Linked GL Accounts")}
                                                    </Typography>

                                                    {supplier.linkedGlAccounts.map((acc, index) => (
                                                        <Box
                                                            key={acc.glAccountId}
                                                            sx={{
                                                                mt: index > 0 ? 3 : 1,
                                                                p: 2,
                                                                bgcolor: "white",
                                                                borderRadius: 1,
                                                                border: "1px solid #e0e0e0",
                                                            }}
                                                        >
                                                            <Grid container spacing={2}>
                                                                <Grid item xs={12} sm={4}>
                                                                    <Typography>
                                                                        <strong>{getTranslatedLabel("party.suppliers.form.glAccountId", "GL Account ID")}:</strong>{" "}
                                                                        {acc.glAccountId}
                                                                    </Typography>
                                                                </Grid>
                                                                <Grid item xs={12} sm={4}>
                                                                    <Typography>
                                                                        <strong>{getTranslatedLabel("party.suppliers.form.role", "Role")}:</strong>{" "}
                                                                        {acc.roleDescription || acc.roleTypeId}
                                                                    </Typography>
                                                                </Grid>
                                                                <Grid item xs={12} sm={4}>
                                                                    <Typography>
                                                                        <strong>{getTranslatedLabel("party.suppliers.form.accountType", "Type")}:</strong>{" "}
                                                                        {acc.glAccountTypeId}
                                                                    </Typography>
                                                                </Grid>

                                                                <Grid item xs={12} sm={6}>
                                                                    <Typography>
                                                                        <strong>{getTranslatedLabel("party.suppliers.form.accountName", "Name")}:</strong>{" "}
                                                                        {acc.accountName || "—"}
                                                                    </Typography>
                                                                </Grid>
                                                                <Grid item xs={12} sm={6}>
                                                                    <Typography>
                                                                        <strong>{getTranslatedLabel("party.suppliers.form.accountNameArabic", "Arabic Name")}:</strong>{" "}
                                                                        {acc.accountNameArabic || "—"}
                                                                    </Typography>
                                                                </Grid>

                                                                {acc.accountDescription && (
                                                                    <Grid item xs={12}>
                                                                        <Typography variant="body2"
                                                                                    color="text.secondary">
                                                                            {acc.accountDescription}
                                                                        </Typography>
                                                                    </Grid>
                                                                )}
                                                            </Grid>
                                                        </Box>
                                                    ))}
                                                </Box>
                                            </Grid>
                                        ) : editMode === 2 ? (
                                            // No accounts message (same as before)
                                            <Grid item xs={12} sx={{mt: 3}}>
                                                <Box sx={{
                                                    p: 2,
                                                    bgcolor: "#fff3e0",
                                                    borderRadius: 2,
                                                    border: "1px solid #ff9800"
                                                }}>
                                                    <Typography variant="body1" color="warning.dark">
                                                        {getTranslatedLabel(
                                                            "party.suppliers.form.noLinkedAccounts",
                                                            "No GL accounts are linked yet."
                                                        )}
                                                    </Typography>
                                                </Box>
                                            </Grid>
                                        ) : null}
                                        <div className="k-form-buttons">
                                            <Grid container rowSpacing={2}>
                                                <Grid item xs={1}>
                                                    <Button
                                                        variant="contained"
                                                        type={"submit"}
                                                        color="success"
                                                        disabled={!formRenderProps.allowSubmit || isMutating}
                                                    >
                                                        {getTranslatedLabel("party.suppliers.form.submit", "Submit")}
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={1}>
                                                    <Button
                                                        onClick={cancelEdit}
                                                        color="error"
                                                        variant="contained"
                                                    >
                                                        {getTranslatedLabel("party.suppliers.form.cancel", "Cancel")}
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </div>

                                        {isMutating && (
                                            <LoadingComponent
                                                message={getTranslatedLabel("party.suppliers.form.processing", "Processing Supplier...")}/>
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
