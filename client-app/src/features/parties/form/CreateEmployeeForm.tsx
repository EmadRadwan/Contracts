import React, { useState } from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import FormTextArea from '../../../app/common/form/FormTextArea';
import FormInput from '../../../app/common/form/FormInput';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { FormComboBox } from '../../../app/common/form/FormComboBox';
import { Party } from '../../../app/models/party/party';
import {
    useAppDispatch,
    useFetchCountriesQuery
} from '../../../app/store/configureStore';
import agent from '../../../app/api/agent';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import { setParty } from '../slice/partySlice';
import { setSingleParty } from '../slice/singlePartySlice';
import { Box, Paper, Typography } from '@mui/material';
import { phoneValidator, requiredValidator } from '../../../app/common/form/Validators';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {useFetchEmployeeQuery, useGetEmplPositionTypesQuery} from "../../../app/store/apis";
import {MemoizedFormComboBox2} from "../../../app/common/form/FormComboBox2";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";

interface Props {
    party?: Party;
    editMode: number; // 1 = create, 2 = edit
    cancelEdit: () => void;
}

export default function CreateEmployeeForm({ party, cancelEdit, editMode }: Props) {
    const { data: countries, isSuccess: isCountriesLoaded } = useFetchCountriesQuery(undefined);
    const [buttonFlag, setButtonFlag] = useState(false);

    const { data: employee, isFetching } = useFetchEmployeeQuery(party?.partyId, {
        skip: party?.partyId === undefined || editMode !== 2,
    });

    const {
        data: positionTypes = [],
        isLoading: isPositionTypesLoading,
        isError: positionTypesError,
    } = useGetEmplPositionTypesQuery();

    const [creationSuccess, setCreationSuccess] = useState<{
        partyId: string;
        createdLoanGlAccountId: string | null;
        createdLoanGlAccountName: string | null;
        createdLoanGlAccountArabic: string | null;
        createdAccruedGlAccountId: string | null;
        createdAccruedGlAccountName: string | null;
        createdAccruedGlAccountArabic: string | null;
    } | null>(null);


    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            let response: any;
            if (editMode === 2) {
                response = await agent.Parties.updateEmployee(data);
            } else {
                response = await agent.Parties.createEmployee(data);
            }
            setCreationSuccess({
                partyId: response.partyId || response.PartyId,
                createdLoanGlAccountId: response.createdLoanGlAccountId || null,
                createdLoanGlAccountName: response.createdLoanGlAccountName || null,
                createdLoanGlAccountArabic: response.createdLoanGlAccountArabicName || null,
                createdAccruedGlAccountId: response.createdAccruedGlAccountId || null,
                createdAccruedGlAccountName: response.createdAccruedGlAccountName || null,
                createdAccruedGlAccountArabic: response.createdAccruedGlAccountArabicName || null,
            });

            dispatch(setParty(response));
            dispatch(setSingleParty(response));
        } catch (error) {
            console.error(error);
        } finally {
            setButtonFlag(false);
        }
    }
    

    return (
        <Paper elevation={5} className={`div-container-withBorderCurved`} sx={{ mt: 5 }}>
            {isFetching && <LoadingComponent message={getTranslatedLabel("party.employees.form.loading", "Loading Employee...")} />}
            {creationSuccess && (
                <Box sx={{ p: 3, mb: 3, bgcolor: '#e8f5e9', borderRadius: 2, border: '1px solid #81c784' }}>
                    <Typography variant="h5" color="success.main" gutterBottom>
                        {getTranslatedLabel("party.employees.form.success", "Employee Saved Successfully!")}
                    </Typography>

                    <Typography variant="body1" gutterBottom>
                        {getTranslatedLabel("party.employees.form.employeeId", "Employee ID")}:{" "}
                        <strong>{creationSuccess.partyId}</strong>
                    </Typography>

                    {/* Loan Receivable - new */}
                    {creationSuccess.createdLoanGlAccountId && (
                        <Box sx={{ mt: 3 }}>
                            <Typography variant="h6" color="primary">
                                {getTranslatedLabel("party.employees.form.newLoanAccount", "New Loans Receivable Account Created")}
                            </Typography>
                            <Grid container spacing={2} sx={{ mt: 1 }}>
                                <Grid item xs={12} sm={6}>
                                    <Typography>
                                        <strong>{getTranslatedLabel("party.employees.form.glAccountId", "GL Account ID")}:</strong>{" "}
                                        {creationSuccess.createdLoanGlAccountId}
                                    </Typography>
                                </Grid>
                                <Grid item xs={12} sm={6}>
                                    <Typography>
                                        <strong>{getTranslatedLabel("party.employees.form.accountName", "Account Name")}:</strong>{" "}
                                        {creationSuccess.createdLoanGlAccountName || "—"}
                                    </Typography>
                                </Grid>
                                {creationSuccess.createdLoanGlAccountArabic && (
                                    <Grid item xs={12}>
                                        <Typography>
                                            <strong>{getTranslatedLabel("party.employees.form.accountNameArabic", "Account Name (Arabic)")}:</strong>{" "}
                                            {creationSuccess.createdLoanGlAccountArabic}
                                        </Typography>
                                    </Grid>
                                )}
                            </Grid>
                        </Box>
                    )}

                    {/* Accrued Expenses - new */}
                    {creationSuccess.createdAccruedGlAccountId && (
                        <Box sx={{ mt: 3 }}>
                            <Typography variant="h6" color="primary">
                                {getTranslatedLabel("party.employees.form.newAccruedAccount", "New Accrued Expenses Account Created")}
                            </Typography>
                            <Grid container spacing={2} sx={{ mt: 1 }}>
                                <Grid item xs={12} sm={6}>
                                    <Typography>
                                        <strong>{getTranslatedLabel("party.employees.form.glAccountId", "GL Account ID")}:</strong>{" "}
                                        {creationSuccess.createdAccruedGlAccountId}
                                    </Typography>
                                </Grid>
                                <Grid item xs={12} sm={6}>
                                    <Typography>
                                        <strong>{getTranslatedLabel("party.employees.form.accountName", "Account Name")}:</strong>{" "}
                                        {creationSuccess.createdAccruedGlAccountName || "—"}
                                    </Typography>
                                </Grid>
                                {creationSuccess.createdAccruedGlAccountArabic && (
                                    <Grid item xs={12}>
                                        <Typography>
                                            <strong>{getTranslatedLabel("party.employees.form.accountNameArabic", "Account Name (Arabic)")}:</strong>{" "}
                                            {creationSuccess.createdAccruedGlAccountArabic}
                                        </Typography>
                                    </Grid>
                                )}
                            </Grid>
                        </Box>
                    )}

                    <Box sx={{ mt: 4 }}>
                        <Button variant="contained" color="primary" onClick={cancelEdit} sx={{ mr: 2 }}>
                            {getTranslatedLabel("party.employees.form.done", "Done / Close")}
                        </Button>
                        <Button
                            variant="outlined"
                            onClick={() => setCreationSuccess(null)}
                        >
                            {getTranslatedLabel("party.employees.form.createAnother", "Create Another Employee")}
                        </Button>
                    </Box>
                </Box>
            )}
            {!creationSuccess && (
                <>
                    <Grid container spacing={2}>
                        <Grid item xs={6}>
                            <Box display="flex" justifyContent="space-between" paddingBottom={4}>
                                <Typography sx={{ p: 2 }} variant="h4" color={editMode === 1 ? "green" : "black"}>
                                    {editMode === 1
                                        ? getTranslatedLabel("party.employees.form.createTitle", "Create Employee")
                                        : getTranslatedLabel("party.employees.form.editTitle", "Edit Employee")}
                                </Typography>
                            </Box>
                        </Grid>
                    </Grid>

                    <Form
                        initialValues={editMode === 2 ? employee : undefined}
                        key={JSON.stringify(employee)}
                        onSubmit={(values) => handleSubmitData(values)}
                        render={(formRenderProps) => (
                            <FormElement>
                                <fieldset className={'k-form-fieldset'}>
                                    <Grid container spacing={2}>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'firstName'}
                                                name={'firstName'}
                                                label={getTranslatedLabel("party.employees.form.firstName", "First Name *")}
                                                component={FormInput}
                                                autoComplete={'off'}
                                                validator={requiredValidator}
                                            />
                                        </Grid>

                                        <Grid item xs={3}>
                                            <Field
                                                id="emplPositionTypeId"
                                                name="emplPositionTypeId"
                                                label={getTranslatedLabel("party.employees.form.emplPositionTypeId", "Employee Position")}
                                                component={MemoizedFormComboBox2}
                                                data={positionTypes || []}
                                                dataItemKey="emplPositionTypeId"
                                                textField="description"
                                                validator={requiredValidator}
                                            />
                                        </Grid>

                                        <Grid item xs={2}>
                                            <Field
                                                id="monthlyBaseSalary"
                                                name="monthlyBaseSalary"
                                                label={getTranslatedLabel("party.employees.form.monthlyBaseSalary", "Monthly Base Salary (EGP)")}
                                                component={FormNumericTextBox}
                                                format="n2"
                                                min={0}
                                                validator={requiredValidator}
                                            />
                                        </Grid>

                                        {/* Email, Country, Mobile, Address1, Address2 */}
                                        <Grid item xs={3}>
                                            <Field
                                                id={'infoString'}
                                                name={'infoString'}
                                                label={getTranslatedLabel("party.employees.form.email", "Email Address (Work)")}
                                                component={FormInput}
                                                autoComplete={'off'}
                                            />
                                        </Grid>

                                        <Grid item xs={6}>
                                            {isCountriesLoaded && (
                                                <Field
                                                    id={'geoId'}
                                                    name={'geoId'}
                                                    label={getTranslatedLabel("party.employees.form.countryCode", "Country")}
                                                    component={FormComboBox}
                                                    dataItemKey={'geoId'}
                                                    textField={'geoName'}
                                                    autoComplete={'off'}
                                                    data={countries}
                                                />
                                            )}
                                        </Grid>

                                        <Grid item xs={6}>
                                            <Field
                                                id={'mobileContactNumber'}
                                                name={'mobileContactNumber'}
                                                label={getTranslatedLabel("party.employees.form.mobile", "Mobile Phone *")}
                                                component={FormInput}
                                                autoComplete={'off'}
                                                // validator={phoneValidator}
                                            />
                                        </Grid>

                                        <Grid item xs={6}>
                                            <Field
                                                id={'address1'}
                                                name={'address1'}
                                                label={getTranslatedLabel("party.employees.form.address1", "Address 1")}
                                                component={FormInput}
                                                autoComplete={'off'}
                                            />
                                        </Grid>

                                        <Grid item xs={6}>
                                            <Field
                                                id={'address2'}
                                                name={'address2'}
                                                label={getTranslatedLabel("party.employees.form.address2", "Address 2")}
                                                component={FormTextArea}
                                                rows={3}
                                                autoComplete={'off'}
                                            />
                                        </Grid>

                                        {/* ──────────────────────────────────────────────── */}
                                        {/* Existing Loan Receivable Account */}
                                        {employee?.loanGlAccountId && (
                                            <Grid item xs={12} sx={{ mt: 4 }}>
                                                <Box sx={{ p: 3, bgcolor: "#e3f2fd", borderRadius: 2, border: "1px solid #90caf9" }}>
                                                    <Typography variant="h6" color="primary" gutterBottom>
                                                        {getTranslatedLabel("party.employees.form.linkedLoanAccount", "Linked Loans Receivable Account")}
                                                    </Typography>
                                                    <Grid container spacing={2}>
                                                        <Grid item xs={12} sm={6}>
                                                            <Typography><strong>GL Account ID:</strong> {employee.loanGlAccountId}</Typography>
                                                        </Grid>
                                                        <Grid item xs={12} sm={6}>
                                                            <Typography><strong>Account Name:</strong> {employee.loanGlAccountName || "—"}</Typography>
                                                        </Grid>
                                                        {employee.loanGlAccountNameArabic && (
                                                            <Grid item xs={12}>
                                                                <Typography><strong>اسم الحساب:</strong> {employee.loanGlAccountNameArabic}</Typography>
                                                            </Grid>
                                                        )}
                                                    </Grid>
                                                </Box>
                                            </Grid>
                                        )}

                                        {/* Existing Accrued Expenses Account */}
                                        {employee?.accruedGlAccountId && (
                                            <Grid item xs={12} sx={{ mt: 3 }}>
                                                <Box sx={{ p: 3, bgcolor: "#e8f5e9", borderRadius: 2, border: "1px solid #81c784" }}>
                                                    <Typography variant="h6" color="success.main" gutterBottom>
                                                        {getTranslatedLabel("party.employees.form.linkedAccruedAccount", "Linked Accrued Expenses Account")}
                                                    </Typography>
                                                    <Grid container spacing={2}>
                                                        <Grid item xs={12} sm={6}>
                                                            <Typography><strong>GL Account ID:</strong> {employee.accruedGlAccountId}</Typography>
                                                        </Grid>
                                                        <Grid item xs={12} sm={6}>
                                                            <Typography><strong>Account Name:</strong> {employee.accruedGlAccountName || "—"}</Typography>
                                                        </Grid>
                                                        {employee.accruedGlAccountNameArabic && (
                                                            <Grid item xs={12}>
                                                                <Typography><strong>اسم الحساب:</strong> {employee.accruedGlAccountNameArabic}</Typography>
                                                            </Grid>
                                                        )}
                                                    </Grid>
                                                </Box>
                                            </Grid>
                                        )}

                                        {/* Warnings when missing (edit mode) */}
                                        {editMode === 2 && (
                                            <>
                                                {!employee?.loanGlAccountId && (
                                                    <Grid item xs={12} sx={{ mt: 3 }}>
                                                        <Box sx={{ p: 2, bgcolor: "#fff3e0", borderRadius: 2, border: "1px solid #ff9800" }}>
                                                            <Typography color="warning.dark">
                                                                {getTranslatedLabel(
                                                                    "party.employees.form.noLoanAccount",
                                                                    "No dedicated Loans Receivable account linked yet. Save to create automatically."
                                                                )}
                                                            </Typography>
                                                        </Box>
                                                    </Grid>
                                                )}

                                                {!employee?.accruedGlAccountId && (
                                                    <Grid item xs={12} sx={{ mt: 2 }}>
                                                        <Box sx={{ p: 2, bgcolor: "#fff3e0", borderRadius: 2, border: "1px solid #ff9800" }}>
                                                            <Typography color="warning.dark">
                                                                {getTranslatedLabel(
                                                                    "party.employees.form.noAccruedAccount",
                                                                    "No dedicated Accrued Expenses account linked yet. Save to create automatically."
                                                                )}
                                                            </Typography>
                                                        </Box>
                                                    </Grid>
                                                )}
                                            </>
                                        )}
                                    </Grid>

                                    <div className="k-form-buttons">
                                        <Grid container rowSpacing={2}>
                                            <Grid item xs={1}>
                                                <Button
                                                    variant="contained"
                                                    type="submit"
                                                    color="success"
                                                    disabled={!formRenderProps.allowSubmit || buttonFlag}
                                                >
                                                    {getTranslatedLabel("party.employees.form.submit", "Submit")}
                                                </Button>
                                            </Grid>
                                            <Grid item xs={1}>
                                                <Button onClick={cancelEdit} color="error" variant="contained">
                                                    {getTranslatedLabel("party.employees.form.cancel", "Cancel")}
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </div>

                                    {buttonFlag && (
                                        <LoadingComponent message={getTranslatedLabel("party.employees.form.processing", "Processing Employee...")} />
                                    )}
                                </fieldset>
                            </FormElement>
                        )}
                    />
                </>
            )}
        </Paper>
    );
}