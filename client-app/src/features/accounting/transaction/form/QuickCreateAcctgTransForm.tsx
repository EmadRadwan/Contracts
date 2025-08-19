import React, {useEffect, useState} from "react";
import Grid from "@mui/material/Grid";
import {Paper, Skeleton, Typography} from "@mui/material";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import Button from "@mui/material/Button";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";

import {RootState, useAppSelector} from "../../../../app/store/configureStore";

import AccountingMenu from "../../invoice/menu/AccountingMenu";
import {requiredValidator} from "../../../../app/common/form/Validators";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import {router} from "../../../../app/router/Routes";
import {useFetchAcctgTransTypesQuery, useFetchGlAccountOrganizationHierarchyLovQuery} from "../../../../app/store/apis";
import useCreateAcctgTrans from "../hook/useCreateAcctgTrans";
import FormInput from "../../../../app/common/form/FormInput";
import {FormComboBoxVirtualParty} from "../../../../app/common/form/FormComboBoxVirtualParty";
import {
    FormMultiColumnComboBoxVirtualSalesProduct
} from "../../../../app/common/form/FormMultiColumnComboBoxVirtualSalesProduct";
import AccountingSummaryMenu from "../../organizationGlSettings/menu/AccountingSummaryMenu";
import {useSelector} from "react-redux";
import {FormDropDownTreeGlAccount2} from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";


export default function QuickCreateAcctgTransForm() {
    const formRef = React.useRef<any>();
    const [formKey, setFormKey] = React.useState(Math.random());
    const {getTranslatedLabel} = useTranslationHelper()

    const [isLoading, setIsLoading] = useState(false);
    const {data: acctgTransTypes} = useFetchAcctgTransTypesQuery(undefined);
    const companyName = useSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);

    const companyId = useAppSelector(state => state.accountingSharedUi.selectedAccountingCompanyId);
    useEffect(() => {
        if (!companyId) {
            router.navigate("/orgGl");
        }
    }, [companyId]);


    const {acctgTrans, setAcctgTrans, formEditMode, setFormEditMode, handleCreateQuick} = useCreateAcctgTrans({
        setIsLoading,
    });

    const { data: glAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, { skip: companyId === undefined });


    useEffect(() => {
        if (formEditMode && formEditMode < 2) {
            setAcctgTrans(undefined)
        }
    }, [formEditMode, setAcctgTrans]);

    // Find the 'Not Applicable' option from acctgTransTypes to set as default for acctgTransTypeId
    const notApplicableOption = acctgTransTypes?.find(
        (type) => type.description?.toLowerCase() === 'not applicable'
    );
    
    console.log("notApplicableOption", notApplicableOption);

    // Set initial form values, defaulting acctgTransTypeId to 'Not Applicable' if available, overridden by acctgTrans if present
    const initialFormValues = {
        acctgTransTypeId: notApplicableOption ? notApplicableOption.acctgTransTypeId : null,
        ...acctgTrans,
    };

    // menu select event handler
    async function handleMenuSelect(e: MenuSelectEvent) {
        if (e.item.text === 'New Accounting Transaction') {
            handleNewAccgTrans()
        }
    }


    const handleSubmit = (data: any) => {
        if (!data.isValid) {
            return false
        }
        setIsLoading(true);
        handleCreateQuick(data.values);
    };


    const handleNewAccgTrans = () => {
        setAcctgTrans(undefined);
        setFormEditMode(1);
        setFormKey(Math.random());
    };

    return (
        <>
            <AccountingMenu selectedMenuItem={'orgGl'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid item xs={12}>
                    <Typography sx={{p: 2}} variant='h5'>Create Accounting Transaction for {companyName}</Typography>
                </Grid>
                {/* <SetupAccountingMenu selectedMenuItem="orgAccountingSummary" /> */}
                <AccountingSummaryMenu selectedMenuItem="accountingTransaction" />
                <Form
                    ref={formRef}
                    initialValues={initialFormValues}
                    key={formKey}
                    onSubmitClick={(values) => handleSubmit(values)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={"k-form-fieldset"}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12}>
                                        <Grid container spacing={2} flexDirection={"column"}>
                                            <Grid container sx={{marginLeft: 2}} alignItems={"flex-end"}>
                                                <Grid item xs={2}>
                                                    <Field
                                                        id={"acctgTransTypeId"}
                                                        name={"acctgTransTypeId"}
                                                        label={"Acctg Trans Type *"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"acctgTransTypeId"}
                                                        textField={"description"}
                                                        data={acctgTransTypes ? acctgTransTypes : []}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>

                                                <Grid item xs={9}>
                                                </Grid>
                                                <Grid item xs={1}>
                                                    <Menu onSelect={handleMenuSelect}>
                                                        <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                                                            <MenuItem text="New Accounting Transaction"/>
                                                        </MenuItem>
                                                    </Menu>
                                                </Grid>
                                            </Grid>
                                            <Grid container sx={{ ml: 2, mt: 3  }}>
                                                {glAccounts ? (
                                                    <>
                                                        <Grid item xs={5}>
                                                            <Field
                                                                id={"debitGlAccountId"}
                                                                name={"debitGlAccountId"}
                                                                label={"Debit GL Account Id"}
                                                                data={glAccounts}
                                                                component={FormDropDownTreeGlAccount2}
                                                                dataItemKey={"glAccountId"}
                                                                textField={"text"}
                                                                selectField={"selected"}
                                                                expandField={"expanded"}
                                                                validator={requiredValidator}
                                                            />
                                                        </Grid>
                                                        <Grid item xs={1}></Grid>
                                                        <Grid item xs={5}>
                                                            <Field
                                                                id={"creditGlAccountId"}
                                                                name={"creditGlAccountId"}
                                                                label={"Credit GL Account Id"}
                                                                data={glAccounts}
                                                                component={FormDropDownTreeGlAccount2}
                                                                dataItemKey={"glAccountId"}
                                                                textField={"text"}
                                                                selectField={"selected"}
                                                                expandField={"expanded"}
                                                                validator={requiredValidator}
                                                            />
                                                        </Grid>
                                                    </>
                                                ) : (
                                                    <>
                                                        <Grid item xs={5}>
                                                            <Skeleton
                                                                variant="rounded"
                                                                animation="wave"
                                                                height={40}
                                                                sx={{ width: "100%", borderRadius: "4px" }}
                                                            />
                                                        </Grid>
                                                        <Grid item xs={1}></Grid>
                                                        <Grid item xs={5}>
                                                            <Skeleton
                                                                variant="rounded"
                                                                animation="wave"
                                                                height={40}
                                                                sx={{ width: "100%", borderRadius: "4px" }}
                                                            />
                                                        </Grid>
                                                    </>
                                                )}
                                            </Grid>

                                        </Grid>


                                    </Grid>


                                    <Grid item xs={12}>
                                        <Grid container spacing={2} alignItems={"flex-end"}>
                                            <Grid item xs={2}>
                                                <Field
                                                    id={"amount"}
                                                    format="n2"
                                                    min={0}
                                                    name={"amount"}
                                                    label={"Amount *"}
                                                    component={FormNumericTextBox}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Field
                                                    id={'transactionDate'}
                                                    name={'transactionDate'}
                                                    label={'Transaction Date *'}
                                                    component={FormDatePicker}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={2}>
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id={"description"}
                                                    name={"description"}
                                                    label={"Description"}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>

                                        </Grid>
                                    </Grid>
                                    <Grid item xs={12}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={2}>
                                                <Field
                                                    id={"fromPartyId"}
                                                    name={"fromPartyId"}
                                                    label={"Party Id"}
                                                    component={FormComboBoxVirtualParty}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Field
                                                    id={"productId"}
                                                    name={"productId"}
                                                    label={"Product Id"}
                                                    component={FormMultiColumnComboBoxVirtualSalesProduct}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                            <Grid item xs={2}>
                                            </Grid>
                                            <Grid item xs={1}>
                                                <Field
                                                    id={'paymentId'}
                                                    name={'paymentId'}
                                                    label={'Payment Id'}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                            <Grid item xs={1}>
                                            </Grid>
                                            <Grid item xs={1}>
                                                <Field
                                                    id={'invoiceId'}
                                                    name={'invoiceId'}
                                                    label={'Invoice Id'}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                            <Grid item xs={1}>
                                            </Grid>
                                            <Grid item xs={1}>
                                                <Field
                                                    id={'fixedAssetId'}
                                                    name={'fixedAssetId'}
                                                    label={'Fixed Asset'}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                        </Grid>

                                    </Grid>

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
                                </Grid>

                                {isLoading && (
                                    <LoadingComponent message="Processing Accounting Transaction..."/>
                                )}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}
