import React, {useMemo} from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {
    useAppDispatch,
    useAppSelector,
    useFetchOrgChartOfAccountsLovQuery,
} from "../../../../app/store/configureStore";
import {Paper} from "@mui/material";
import {FormDropDownTreeGlAccount} from "../../../../app/common/form/FormDropDownTreeGlAccount";
import OrganizationChartOfAccountsList from "../dashboard/OrganizationChartOfAccountsList";
import OrganizationGlSettingsMenuNavContainer from "../menu/OrganizationGlSettingsMenu";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import {router} from "../../../../app/router/Routes";


export default function ChartOfAccountAssignForm() {


    const {data: glAccounts} = useFetchOrgChartOfAccountsLovQuery(undefined);

    const dispatch = useAppDispatch();
    const companyId = useAppSelector(state => state.accountingSharedUi.selectedAccountingCompanyId);
    if (!companyId) {
        router.navigate("/orgGl");
    }


    async function handleSubmitData(data: any) {
        try {
            let response: any;

        } catch (error) {
            console.log(error)
        }
    }

    console.log('glAccounts for LOV', glAccounts);

    const memoizedOrganizationChartOfAccountsList = useMemo(() => <OrganizationChartOfAccountsList
        companyId={companyId ? companyId : undefined}/>, [companyId]);


    return (
        <>
            <AccountingMenu selectedMenuItem={'/orgGL'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <OrganizationGlSettingsMenuNavContainer/>

                <Form
                    onSubmit={values => handleSubmitData(values)}
                    render={(formRenderProps) => (

                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Grid container spacing={2} alignItems={"end"} className="no-padding-top">
                                    <Grid item xs={5}>
                                        <Field
                                            id={"glAccountId"}
                                            name={"glAccountId"}
                                            label={"GL Account"}
                                            data={glAccounts ? glAccounts : []}
                                            component={FormDropDownTreeGlAccount}
                                            dataItemKey={"glAccountId"}
                                            textField={"text"}
                                            selectField={"selected"}
                                            expandField={"expanded"}
                                            onChange={e => console.log(e)}
                                        />
                                    </Grid>

                                    <Grid item xs={1}>
                                        <Button
                                            variant="contained"
                                            type={'submit'}
                                            color='success'
                                            disabled={!formRenderProps.allowSubmit}
                                        >
                                            Assign
                                        </Button>
                                    </Grid>
                                </Grid>


                                <Grid container spacing={2}>
                                    <Grid item xs={12}>
                                        {memoizedOrganizationChartOfAccountsList}
                                    </Grid>
                                </Grid>


                            </fieldset>

                        </FormElement>

                    )}
                />

            </Paper>

        </>


    );
}


