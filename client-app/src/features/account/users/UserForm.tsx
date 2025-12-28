import React, { useState, useEffect } from "react";
import { Button, Grid, Paper, Typography, TextField, Box, IconButton } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import CloseIcon from "@mui/icons-material/Close";
import FormInput from "../../../app/common/form/FormInput";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { MultiSelect, MultiSelectChangeEvent, DropDownList, DropDownListChangeEvent } from "@progress/kendo-react-dropdowns";
import { requiredValidator, emailValidator } from "../../../app/common/form/Validators";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import {
    useFetchAllRolesQuery,
    useFetchUserRolesQuery,
    useCreateUserMutation,
    useUpdateUserMutation,
    useCreateRoleMutation,
    useFetchInternalAccountingOrganizationsLovQuery,
    UserListDto,
    RoleDto,
} from "../../../app/store/apis";
import UsersMenu from "./menu/UsersMenu";

interface Props {
    user?: UserListDto;
    editMode: number; // 1 = create, 2 = edit
    cancelEdit: () => void;
}

export default function UserForm({ user, cancelEdit, editMode }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const [buttonFlag, setButtonFlag] = useState(false);
    const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
    const [selectedOrganizationId, setSelectedOrganizationId] = useState<string>("");

    const { data: allRoles, isLoading: rolesLoading } = useFetchAllRolesQuery();
    const { data: organizations, isLoading: organizationsLoading } = useFetchInternalAccountingOrganizationsLovQuery({});
    const { data: userRoles, isLoading: userRolesLoading } = useFetchUserRolesQuery(
        user?.id ?? "",
        { skip: !user?.id }
    );

    const [createUser] = useCreateUserMutation();
    const [updateUser] = useUpdateUserMutation();
    const [createRole] = useCreateRoleMutation();

    const [showCreateRole, setShowCreateRole] = useState(false);
    const [newRoleName, setNewRoleName] = useState('');
    const [isCreatingRole, setIsCreatingRole] = useState(false);

    useEffect(() => {
        if (userRoles) {
            setSelectedRoles(userRoles);
        }
    }, [userRoles]);

    useEffect(() => {
        if (user?.organizationPartyId) {
            setSelectedOrganizationId(user.organizationPartyId);
        }
    }, [user?.organizationPartyId]);

    const handleRolesChange = (event: MultiSelectChangeEvent) => {
        setSelectedRoles(event.value);
    };

    const handleOrganizationChange = (event: DropDownListChangeEvent) => {
        setSelectedOrganizationId(event.value?.partyId || "");
    };

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            // Create new user with roles
            await createUser({
                email: data.email,
                userName: data.userName,
                displayName: data.displayName,
                organizationPartyId: selectedOrganizationId,
                roles: selectedRoles,
            }).unwrap();
            toast.success(
                getTranslatedLabel("users.form.createSuccess", "User created successfully")
            );
            cancelEdit();
        } catch (error: any) {
            console.error(error);
            const errorMessage = error?.data?.errors
                ? Object.values(error.data.errors).flat().join(" ")
                : error?.data || getTranslatedLabel("users.form.createError", "Failed to create user");
            toast.error(errorMessage);
        }
        setButtonFlag(false);
    }

    async function handleUpdateUser(data: any) {
        if (!user) return;
        setButtonFlag(true);
        try {
            await updateUser({
                userId: user.id,
                userName: data.userName || user.userName,
                displayName: data.displayName,
                email: data.email,
                organizationPartyId: selectedOrganizationId,
                roles: selectedRoles,
            }).unwrap();

            toast.success(
                getTranslatedLabel("users.form.updateSuccess", "User updated successfully")
            );
            cancelEdit();
        } catch (error: any) {
            console.error(error);
            const errorMessage = error?.data?.errors
                ? Object.values(error.data.errors).flat().join(" ")
                : error?.data || getTranslatedLabel("users.form.updateError", "Failed to update user");
            toast.error(errorMessage);
        }
        setButtonFlag(false);
    }

    async function handleCreateRole() {
        if (!newRoleName.trim()) {
            toast.error(getTranslatedLabel("roles.form.nameRequired", "Role name is required"));
            return;
        }

        setIsCreatingRole(true);
        try {
            await createRole({ roleName: newRoleName.trim() }).unwrap();
            toast.success(getTranslatedLabel("roles.form.createSuccess", "Role created successfully"));
            setNewRoleName('');
            setShowCreateRole(false);
        } catch (error: any) {
            console.error(error);
            const errorMessage = error?.data || getTranslatedLabel("roles.form.createError", "Failed to create role");
            toast.error(errorMessage);
        }
        setIsCreatingRole(false);
    }

    if (rolesLoading || organizationsLoading || (editMode === 2 && userRolesLoading)) {
        return <LoadingComponent message={getTranslatedLabel("users.form.loading", "Loading...")} />;
    }

    const roleNames = allRoles?.map((r: RoleDto) => r.name) || [];

    return (
        <>
        <UsersMenu />
        <Paper elevation={5} className="div-container-withBorderCurved">
            <Typography variant="h5" sx={{ mb: 2 }}>
                {editMode === 1
                    ? getTranslatedLabel("users.form.createTitle", "Create New User")
                    : getTranslatedLabel("users.form.editTitle", "Edit User")}
            </Typography>
            <Form
                initialValues={
                    editMode === 2 && user
                        ? {
                              userName: user.userName,
                              displayName: user.displayName,
                              email: user.email,
                              organizationPartyId: organizations?.find(o => o.partyId === user.organizationPartyId),
                          }
                        : undefined
                }
                onSubmit={(values) => editMode === 1 ? handleSubmitData(values) : handleUpdateUser(values)}
                render={(formRenderProps) => (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={2}>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="userName"
                                        name="userName"
                                        label={getTranslatedLabel("users.form.userName", "Username")}
                                        component={FormInput}
                                        autoComplete="off"
                                        validator={requiredValidator}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="displayName"
                                        name="displayName"
                                        label={getTranslatedLabel("users.form.displayName", "Display Name")}
                                        component={FormInput}
                                        autoComplete="off"
                                        validator={requiredValidator}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="email"
                                        name="email"
                                        label={getTranslatedLabel("users.form.email", "Email")}
                                        component={FormInput}
                                        autoComplete="off"
                                        validator={emailValidator}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <label className="k-label">
                                        {getTranslatedLabel("users.form.company", "Company")}
                                    </label>
                                    <DropDownList
                                        data={organizations || []}
                                        dataItemKey="partyId"
                                        textField="partyName"
                                        value={organizations?.find(o => o.partyId === selectedOrganizationId) || null}
                                        onChange={handleOrganizationChange}
                                        style={{ width: "100%" }}
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                                        <label className="k-label" style={{ margin: 0 }}>
                                            {getTranslatedLabel("users.form.roles", "Roles")}
                                        </label>
                                        <IconButton
                                            size="small"
                                            color="primary"
                                            onClick={() => setShowCreateRole(!showCreateRole)}
                                            title={getTranslatedLabel("users.form.createRole", "Create Role")}
                                        >
                                            <AddIcon fontSize="small" />
                                        </IconButton>
                                    </Box>
                                    {showCreateRole && (
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1, p: 1, bgcolor: 'grey.100', borderRadius: 1 }}>
                                            <TextField
                                                size="small"
                                                label={getTranslatedLabel("roles.form.roleName", "Role Name")}
                                                value={newRoleName}
                                                onChange={(e) => setNewRoleName(e.target.value)}
                                                sx={{ flex: 1 }}
                                            />
                                            <Button
                                                size="small"
                                                variant="contained"
                                                color="success"
                                                onClick={handleCreateRole}
                                                disabled={isCreatingRole}
                                            >
                                                {getTranslatedLabel("roles.form.save", "Save")}
                                            </Button>
                                            <IconButton
                                                size="small"
                                                onClick={() => {
                                                    setShowCreateRole(false);
                                                    setNewRoleName('');
                                                }}
                                            >
                                                <CloseIcon fontSize="small" />
                                            </IconButton>
                                        </Box>
                                    )}
                                    <MultiSelect
                                        data={roleNames}
                                        value={selectedRoles}
                                        onChange={handleRolesChange}
                                        placeholder={getTranslatedLabel("users.form.selectRoles", "Select roles...")}
                                        style={{ width: "100%" }}
                                        filterable={true}
                                    />
                                </Grid>
                            </Grid>
                            <div className="k-form-buttons" style={{ marginTop: 16 }}>
                                <Grid container spacing={1}>
                                    <Grid item>
                                        <Button
                                            type={editMode === 1 ? "submit" : "button"}
                                            color="success"
                                            variant="contained"
                                            disabled={buttonFlag || (editMode === 1 && !formRenderProps.allowSubmit)}
                                            onClick={editMode === 2 ? () => handleUpdateUser({
                                                userName: formRenderProps.valueGetter("userName"),
                                                displayName: formRenderProps.valueGetter("displayName"),
                                                email: formRenderProps.valueGetter("email"),
                                            }) : undefined}
                                        >
                                            {getTranslatedLabel("users.form.submit", "Submit")}
                                        </Button>
                                    </Grid>
                                    <Grid item>
                                        <Button
                                            onClick={cancelEdit}
                                            variant="contained"
                                            color="error"
                                        >
                                            {getTranslatedLabel("users.form.cancel", "Cancel")}
                                        </Button>
                                    </Grid>
                                </Grid>
                            </div>
                            {buttonFlag && (
                                <LoadingComponent message={getTranslatedLabel("users.form.processing", "Processing...")} />
                            )}
                        </fieldset>
                    </FormElement>
                )}
            />
        </Paper>
        </>
    );
}
