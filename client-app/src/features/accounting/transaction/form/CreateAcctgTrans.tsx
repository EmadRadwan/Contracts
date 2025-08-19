import React, { useEffect, useState, useRef } from "react";
import Grid from "@mui/material/Grid";
import { Paper, Typography, Button } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormInput from "../../../../app/common/form/FormInput";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { FormComboBoxVirtualParty } from "../../../../app/common/form/FormComboBoxVirtualParty";
import { requiredValidator } from "../../../../app/common/form/Validators";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { RootState, useAppSelector } from "../../../../app/store/configureStore";
import { useFetchAcctgTransTypesQuery } from "../../../../app/store/apis";
import useCreateAcctgTrans from "../hook/useCreateAcctgTrans";
import { router } from "../../../../app/router/Routes";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useSelector } from "react-redux";
import AccountingSummaryMenu from "../../organizationGlSettings/menu/AccountingSummaryMenu";
import FormTextArea from "../../../../app/common/form/FormTextArea";

export default function CreateAcctgTransForm() {
    const formRef = useRef(null);
    const [formKey, setFormKey] = useState(Math.random());
    const { getTranslatedLabel } = useTranslationHelper();
    const [isLoading, setIsLoading] = useState(false);
    const { data: acctgTransTypes } = useFetchAcctgTransTypesQuery(undefined);
    const companyName = useSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
    const companyId = useAppSelector(state => state.accountingSharedUi.selectedAccountingCompanyId);

    useEffect(() => {
        if (!companyId) {
            router.navigate("/orgGl");
        }
    }, [companyId]);

    const { acctgTrans, setAcctgTrans, formEditMode, setFormEditMode, handleCreate } = useCreateAcctgTrans({
        setIsLoading,
    });


    const handleSubmit = (data) => {
        if (!data.isValid) {
            return false;
        }
        setIsLoading(true);
        handleCreate(data.values);
    };

    const notApplicableOption = acctgTransTypes?.find(
        (type) => type.description?.toLowerCase() === 'not applicable'
    );

    console.log("notApplicableOption", notApplicableOption);

    // Set initial form values, defaulting acctgTransTypeId to 'Not Applicable' if available, overridden by acctgTrans if present
    const initialFormValues = {
        acctgTransTypeId: notApplicableOption ? notApplicableOption.acctgTransTypeId : null,
        ...acctgTrans,
    };

    console.log("notApplicableOption", notApplicableOption);
    console.log("initialFormValues", initialFormValues);

    return (
        <>
            <AccountingMenu selectedMenuItem={"orgGl"} />
            <Paper
                elevation={5}
                className="div-container-withBorderCurved"
                style={{ maxWidth: "1200px", margin: "auto", padding: "16px" }}
            >
                <Grid item xs={12}>
                    <Typography sx={{ p: 1 }} variant="h5">
                        Create Accounting Transaction for {companyName}
                    </Typography>
                </Grid>
                <AccountingSummaryMenu selectedMenuItem="accountingTransaction" />

                <Form
                    ref={formRef}
                    initialValues={initialFormValues}
                    key={formKey}
                    onSubmitClick={handleSubmit}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2}>
                                    {/* Row 1: 4 fields */}
                                    <Grid container item spacing={1}>
                                        <Grid item xs={3}>
                                            <Field
                                                id="acctgTransTypeId"
                                                name="acctgTransTypeId"
                                                label="Acctg Trans Type *"
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="acctgTransTypeId"
                                                textField="description"
                                                data={acctgTransTypes || []}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        
                                    </Grid>

                                    {/* Row 2: 4 fields */}
                                    <Grid container item spacing={1}>
                                        
                                        <Grid item xs={3}>
                                            <Field
                                                id="transactionDate"
                                                name="transactionDate"
                                                label="Transaction Date *"
                                                component={FormDatePicker}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                    </Grid>

                                    

                                    {/* Row 7: Description */}
                                    <Grid container item spacing={1}>
                                        <Grid item xs={12}>
                                            <Field
                                                id="description"
                                                name="description"
                                                label="Description"
                                                component={FormTextArea}
                                                multiline={"true"}
                                                rows={3}
                                                autoComplete="off"
                                            />
                                        </Grid>
                                    </Grid>

                                    {/* Submit Button */}
                                    <Grid item xs={12} sx={{ mt: 2 }}>
                                        <Button variant="contained" type="submit" color="success" disabled={!formRenderProps.allowSubmit}>
                                            Create
                                        </Button>
                                    </Grid>
                                </Grid>
                                {isLoading && <LoadingComponent message="Processing Accounting Transaction..." />}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}
