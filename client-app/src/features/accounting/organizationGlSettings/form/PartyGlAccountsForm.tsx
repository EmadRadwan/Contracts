import { State } from '@progress/kendo-data-query';
import React from 'react';
import {
    CreatePartyGlAccountRequest, useCreatePartyGlAccountMutation,
    useFetchGlAccountTypesQuery,
    useFetchRolesQuery,
} from '../../../../app/store/configureStore';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { Button, Grid } from '@mui/material';
import { requiredValidator } from '../../../../app/common/form/Validators';
import { FormComboBoxVirtualParty } from '../../../../app/common/form/FormComboBoxVirtualParty';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import {MemoizedFormComboBox2} from "../../../../app/common/form/FormComboBox2";
import {FormDropDownTreeGlAccount2} from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import {useFetchGlAccountOrganizationHierarchyLovQuery} from "../../../../app/store/apis";
import {toast} from "react-toastify";  // ← Add this import

interface Props {
    selectedAccountingCompanyId: string | undefined;
}

const PartyGlAccountsForm = ({ selectedAccountingCompanyId }: Props) => {
    const { getTranslatedLabel } = useTranslationHelper();  // ← Hook to get translations

    const [dataState, setDataState] = React.useState<State>({ skip: 0 });
    const ALLOWED_GL_ACCOUNT_TYPES = [
        "ACCOUNTS_PAYABLE",
        "ACCOUNTS_RECEIVABLE",
        "OWNERS_EQUITY",
    ];
    const { data: glAccountTypes } = useFetchGlAccountTypesQuery(undefined);
    const filteredGlAccountTypes = React.useMemo(() =>
            (glAccountTypes ?? []).filter(type =>
                ALLOWED_GL_ACCOUNT_TYPES.includes(type.glAccountTypeId)
            ),
        [glAccountTypes]
    );
    const { data: roles } = useFetchRolesQuery(undefined);
   
    const {
        data: glAccounts,
        isLoading: isLoadingGlAccounts
    } = useFetchGlAccountOrganizationHierarchyLovQuery(selectedAccountingCompanyId, {
        skip: !selectedAccountingCompanyId,
    });

    const [createPartyGlAccount, { isLoading: isSubmitting }] = useCreatePartyGlAccountMutation();

    const onSubmitHandler = async (values: any) => {
        if (!selectedAccountingCompanyId) return;

        const payload: CreatePartyGlAccountRequest = {
            companyId: selectedAccountingCompanyId,
            partyId: values.partyId.fromPartyId,
            roleTypeId: values.roleTypeId,
            glAccountId: values.glAccountId,
            glAccountTypeId: values.glAccountTypeId,
        };

        try {
            const result = await createPartyGlAccount(payload).unwrap();

            toast.success(
                getTranslatedLabel(
                    "accounting.partyGlAccounts.toast.saved",
                    "Party GL account assigned successfully"
                )
            );

            // Optional: reset form
            // formRenderProps.onFormReset?.();
        } catch (err: any) {
            toast.error(
                err?.data?.title === 'PARTY_GL_ACCOUNT_ALREADY_EXISTS' ? getTranslatedLabel(
                    "accounting.partyGlAccounts.toast.duplicate",
                    "Failed to save party GL account"
                ) :
                getTranslatedLabel(
                    "accounting.partyGlAccounts.toast.saveFailed",
                    "Failed to save party GL account"
                )
            );
        }
    };



    return (
        <Form
            onSubmit={(values) => onSubmitHandler(values)}
            render={(formRenderProps) => (
                <FormElement>
                    <fieldset className={"k-form-fieldset"}>
                        <Grid container spacing={1} alignItems={"flex-end"}>
                            <Grid item xs={5}>
                                <Field
                                    name={"partyId"}
                                    id={"partyId"}
                                    label={getTranslatedLabel(
                                        "accounting.partyGlAccountsForm.party",
                                        "Party"
                                    )}
                                    component={FormComboBoxVirtualParty}
                                    validator={requiredValidator}
                                />
                            </Grid>

                            <Grid item xs={5}>
                                <Field
                                    name={"roleTypeId"}
                                    id={"roleTypeId"}
                                    label={getTranslatedLabel(
                                        "accounting.partyGlAccountsForm.role",
                                        "Role"
                                    )}
                                    component={MemoizedFormComboBox2}
                                    data={roles ?? []}
                                    dataItemKey={"roleTypeId"}
                                    textField={"roleName"}
                                    validator={requiredValidator}
                                />
                            </Grid>

                            <Grid item xs={5}>
                                <Field
                                    name={"glAccountTypeId"}
                                    id={"glAccountTypeId"}
                                    label={getTranslatedLabel(
                                        "accounting.partyGlAccountsForm.glAccountType",
                                        "GL Account Type"
                                    )}
                                    component={MemoizedFormComboBox2}
                                    data={filteredGlAccountTypes}
                                    dataItemKey={"glAccountTypeId"}
                                    textField={"description"}
                                    validator={requiredValidator}
                                />
                            </Grid>

                            <Grid item xs={5}>
                                <Field
                                    name={"glAccountId"}
                                    id={"glAccountId"}
                                    label={getTranslatedLabel(
                                        "accounting.partyGlAccountsForm.glAccount",
                                        "GL Account"
                                    )}
                                    data={glAccounts || []}
                                    component={FormDropDownTreeGlAccount2}
                                    dataItemKey="glAccountId"
                                    textField="text"
                                    selectField="selected"
                                    expandField="expanded"
                                    validator={requiredValidator}
                                />
                            </Grid>

                            <Grid item xs={2}>
                                <Button
                                    variant="contained"
                                    type={"submit"}
                                    color="success"
                                    disabled={
                                        !formRenderProps.valueGetter("glAccountId") ||
                                        !formRenderProps.valueGetter("glAccountTypeId") ||
                                        !formRenderProps.valueGetter("partyId") ||
                                        !formRenderProps.valueGetter("roleTypeId")  // Fixed typo: was "roleId"
                                    }
                                >
                                    {getTranslatedLabel(
                                        "accounting.partyGlAccountsForm.saveButton",
                                        "Save"
                                    )}
                                </Button>
                            </Grid>
                        </Grid>
                    </fieldset>
                </FormElement>
            )}
        />
    );
};

export default PartyGlAccountsForm;