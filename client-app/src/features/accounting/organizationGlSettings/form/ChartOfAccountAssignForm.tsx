import React, {useMemo} from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {toast} from 'react-toastify';
import {
    useAppDispatch,
    useAppSelector,
    useFetchOrgChartOfAccountsLovQuery,
    useAssignGlAccountToOrganizationMutation,
} from "../../../../app/store/configureStore";
import {Paper} from "@mui/material";
import {FormDropDownTreeGlAccount} from "../../../../app/common/form/FormDropDownTreeGlAccount";
import OrganizationChartOfAccountsList from "../dashboard/OrganizationChartOfAccountsList";
import OrganizationGlSettingsMenuNavContainer from "../menu/OrganizationGlSettingsMenu";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import {router} from "../../../../app/router/Routes";
import { FormDropDownTreeGlAccount2 } from '../../../../app/common/form/FormDropDownTreeGlAccount2';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';

const messages: Record<string, Record<string, string>> = {
    en: {
        // Success messages
        GL_ACCOUNT_ASSIGNED: "GL Account assigned successfully.",
        // Error messages
        ALREADY_EXISTS: "Record already exists.",
        PARENT_GL_ACCOUNT_NOT_ASSIGNED: "Parent GL Account is not assigned to the specified Company.",
        USER_NOT_FOUND: "Unauthorized: User not found.",
        GL_ACCOUNT_NOT_FOUND: "The specified GL Account could not be found.",
        UNEXPECTED_ERROR: "An unexpected error occurred. Please try again.",
        DEFAULT: "An unexpected error occurred. Please try again.",
    },
    ar: {
        // Success messages
        GL_ACCOUNT_ASSIGNED: "تم تعيين حساب دفتر الأستاذ بنجاح.",
        // Error messages
        ALREADY_EXISTS: "السجل موجود بالفعل.",
        PARENT_GL_ACCOUNT_NOT_ASSIGNED: "لم يتم تعيين حساب دفتر الأستاذ الرئيسي للشركة المحددة.",
        USER_NOT_FOUND: "غير مصرح: المستخدم غير موجود.",
        GL_ACCOUNT_NOT_FOUND: "حساب دفتر الأستاذ المحدد غير موجود.",
        UNEXPECTED_ERROR: "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
        DEFAULT: "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
    },
};


export default function ChartOfAccountAssignForm() {

    const {data: glAccounts, refetch} = useFetchOrgChartOfAccountsLovQuery(undefined);
    const [assignGlAccountToOrganization] = useAssignGlAccountToOrganizationMutation();

    const dispatch = useAppDispatch();
    const {getTranslatedLabel} = useTranslationHelper()
    const companyId = useAppSelector(state => state.accountingSharedUi.selectedAccountingCompanyId);
    const language = useAppSelector(state => state.localization.language || "en");

    if (!companyId) {
        router.navigate("/orgGl");
    }

    const getMessage = (code: string) => {
        return messages[language]?.[code] || messages["en"]?.[code] || code;
    };

    const handleApiError = (error: any, defaultMessage: string) => {
        const errorCode = error?.data?.errorCode || "DEFAULT";
        const errorMessage = error?.data?.title || defaultMessage;
        const localizedMessage = messages[language]?.[errorCode] || errorMessage || defaultMessage;
        toast.error(localizedMessage);
        console.error(error);
    };

    async function handleSubmitData(data: any) {
        try {
            if (!companyId || !data.glAccountId) return;
            await assignGlAccountToOrganization({
                glAccountId: data.glAccountId,
                companyId: companyId,
            }).unwrap();
            toast.success(getMessage("GL_ACCOUNT_ASSIGNED"));
        } catch (error) {
            handleApiError(error, getMessage("DEFAULT"));
        } finally {
            refetch();
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
                                            component={FormDropDownTreeGlAccount2}
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
                                            {getTranslatedLabel("general.assign", "Assign")}
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


