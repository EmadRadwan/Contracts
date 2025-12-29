import React, { useState } from 'react';
import {
    useFetchAllRolesQuery,
    useFetchUsersByRoleQuery,
    useCreateRoleMutation,
    RoleDto,
    UserListDto,
} from '../../../app/store/apis';
import {
    Grid as KendoGrid,
    GridColumn as Column,
} from "@progress/kendo-react-grid";
import { Grid, Paper, Button, Typography, TextField, Box } from "@mui/material";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import UsersMenu from "./menu/UsersMenu";

const RolesList = () => {
    const { data: roles, isLoading: rolesLoading } = useFetchAllRolesQuery();
    const { getTranslatedLabel } = useTranslationHelper();

    const [selectedRole, setSelectedRole] = useState<RoleDto | null>(null);
    const [showCreateForm, setShowCreateForm] = useState(false);
    const [newRoleName, setNewRoleName] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [createRole] = useCreateRoleMutation();

    const { data: usersInRole, isLoading: usersLoading } = useFetchUsersByRoleQuery(
        selectedRole?.name ?? '',
        { skip: !selectedRole }
    );

    const handleRoleClick = (role: RoleDto) => {
        setSelectedRole(role);
        setShowCreateForm(false);
    };

    const handleCreateRole = async () => {
        if (!newRoleName.trim()) {
            toast.error(getTranslatedLabel("roles.form.nameRequired", "Role name is required"));
            return;
        }

        setIsSubmitting(true);
        try {
            await createRole({ roleName: newRoleName.trim() }).unwrap();
            toast.success(getTranslatedLabel("roles.form.createSuccess", "Role created successfully"));
            setNewRoleName('');
            setShowCreateForm(false);
        } catch (error: any) {
            console.error(error);
            const errorMessage = error?.data || getTranslatedLabel("roles.form.createError", "Failed to create role");
            toast.error(errorMessage);
        }
        setIsSubmitting(false);
    };

    if (rolesLoading) {
        return <LoadingComponent message={getTranslatedLabel("roles.list.loading", "Loading Roles...")} />;
    }

    return (
        <>
            <UsersMenu />
            <Grid container spacing={2} sx={{ p: 2 }}>
                {/* Left Panel - Roles List */}
                <Grid item xs={12} md={4}>
                    <Paper elevation={3} sx={{ p: 2, height: '75vh', display: 'flex', flexDirection: 'column' }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                            <Typography variant="h6">
                                {getTranslatedLabel("roles.list.title", "Roles")}
                            </Typography>
                            <Button
                                variant="contained"
                                color="primary"
                                size="small"
                                onClick={() => {
                                    setShowCreateForm(true);
                                    setSelectedRole(null);
                                }}
                            >
                                {getTranslatedLabel("roles.list.createRole", "Create Role")}
                            </Button>
                        </Box>

                        {/* Create Role Form */}
                        {showCreateForm && (
                            <Paper elevation={1} sx={{ p: 2, mb: 2, bgcolor: 'grey.50' }}>
                                <Typography variant="subtitle1" sx={{ mb: 1 }}>
                                    {getTranslatedLabel("roles.form.createTitle", "Create New Role")}
                                </Typography>
                                <TextField
                                    fullWidth
                                    size="small"
                                    label={getTranslatedLabel("roles.form.roleName", "Role Name")}
                                    value={newRoleName}
                                    onChange={(e) => setNewRoleName(e.target.value)}
                                    sx={{ mb: 1 }}
                                />
                                <Box sx={{ display: 'flex', gap: 1 }}>
                                    <Button
                                        variant="contained"
                                        color="success"
                                        size="small"
                                        onClick={handleCreateRole}
                                        disabled={isSubmitting}
                                    >
                                        {getTranslatedLabel("roles.form.save", "Save")}
                                    </Button>
                                    <Button
                                        variant="outlined"
                                        color="error"
                                        size="small"
                                        onClick={() => {
                                            setShowCreateForm(false);
                                            setNewRoleName('');
                                        }}
                                    >
                                        {getTranslatedLabel("roles.form.cancel", "Cancel")}
                                    </Button>
                                </Box>
                            </Paper>
                        )}

                        {/* Roles List */}
                        <Box sx={{ overflow: 'auto', flex: 1 }}>
                            {roles?.map((role) => (
                                <Paper
                                    key={role.id}
                                    elevation={selectedRole?.id === role.id ? 3 : 1}
                                    sx={{
                                        p: 1.5,
                                        mb: 1,
                                        cursor: 'pointer',
                                        bgcolor: selectedRole?.id === role.id ? 'primary.light' : 'background.paper',
                                        color: selectedRole?.id === role.id ? 'primary.contrastText' : 'text.primary',
                                        '&:hover': {
                                            bgcolor: selectedRole?.id === role.id ? 'primary.light' : 'grey.100',
                                        },
                                    }}
                                    onClick={() => handleRoleClick(role)}
                                >
                                    <Typography variant="body1" fontWeight="medium">
                                        {role.name}
                                    </Typography>
                                </Paper>
                            ))}
                            {(!roles || roles.length === 0) && (
                                <Typography color="text.secondary" sx={{ textAlign: 'center', mt: 2 }}>
                                    {getTranslatedLabel("roles.list.noRoles", "No roles found")}
                                </Typography>
                            )}
                        </Box>
                    </Paper>
                </Grid>

                {/* Right Panel - Users in Role */}
                <Grid item xs={12} md={8}>
                    <Paper elevation={3} sx={{ p: 2, height: '75vh' }}>
                        {selectedRole ? (
                            <>
                                <Typography variant="h6" sx={{ mb: 2 }}>
                                    {getTranslatedLabel("roles.users.title", "Users with role")}: {selectedRole.name}
                                </Typography>
                                {usersLoading ? (
                                    <LoadingComponent message={getTranslatedLabel("roles.users.loading", "Loading users...")} />
                                ) : (
                                    <KendoGrid
                                        style={{ height: 'calc(75vh - 80px)' }}
                                        data={usersInRole || []}
                                        resizable={true}
                                        filterable={true}
                                        sortable={true}
                                        pageable={true}
                                    >
                                        <Column
                                            field="displayName"
                                            title={getTranslatedLabel("roles.users.displayName", "Display Name")}
                                            width={200}
                                        />
                                        <Column
                                            field="userName"
                                            title={getTranslatedLabel("roles.users.userName", "Username")}
                                            width={150}
                                        />
                                        <Column
                                            field="email"
                                            title={getTranslatedLabel("roles.users.email", "Email")}
                                            width={250}
                                        />
                                        <Column
                                            field="organizationPartyId"
                                            title={getTranslatedLabel("roles.users.organization", "Organization")}
                                        />
                                    </KendoGrid>
                                )}
                            </>
                        ) : (
                            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
                                <Typography color="text.secondary">
                                    {getTranslatedLabel("roles.users.selectRole", "Select a role to view its users")}
                                </Typography>
                            </Box>
                        )}
                    </Paper>
                </Grid>
            </Grid>
        </>
    );
};

export default RolesList;
