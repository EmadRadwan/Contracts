import  { useState } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    Grid,
    Typography,
    Box,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    IconButton,
    Tooltip,
} from "@mui/material";
import { ComboBox, ComboBoxFilterChangeEvent, ComboBoxPageChangeEvent } from "@progress/kendo-react-dropdowns";
import DeleteIcon from "@mui/icons-material/Delete";
import LockIcon from "@mui/icons-material/Lock";
import AddIcon from "@mui/icons-material/Add";
import { Grid as KendoGrid, GridColumn as Column } from "@progress/kendo-react-grid";
import {
    useFetchPartyRolesQuery,
    useAddPartyRoleMutation,
    useDeletePartyRoleMutation, useFetchRolesTypesQuery
} from "../../../app/store/apis";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import LoadingComponent from "../../../app/layout/LoadingComponent";

interface Props {
    partyId: string;
    partyName: string;
    open: boolean;
    onClose: () => void;
}

const NON_DELETABLE_ROLES = [
    "EMPLOYEE",
    "CUSTOMER",
    "BILL_TO_CUSTOMER",
    "CONTACT",
    "END_USER_CUSTOMER",
    "PLACING_CUSTOMER",
    "SHIP_TO_CUSTOMER",
    "CONTRACTOR",
    "SUPPLIER",
    "ACCOUNT",
    "BILL_FROM_VENDOR",
    "SHIP_FROM_VENDOR",
    "SUPPLIER_AGENT"
];

export default function ManageRolesModal({ partyId, partyName, open, onClose }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const [searchTerm, setSearchTerm] = useState<string>("");
    const { data: roleTypes, isLoading: isLoadingRoleTypes } = useFetchRolesTypesQuery({ searchTerm });
    const { data: partyRoles, isLoading: isLoadingPartyRoles, isFetching: isFetchingPartyRoles } = useFetchPartyRolesQuery(partyId);
    const [addPartyRole, { isLoading: isAdding }] = useAddPartyRoleMutation();
    const [deletePartyRole, { isLoading: isDeleting }] = useDeletePartyRoleMutation();

    const [selectedRoleType, setSelectedRoleType] = useState<{ roleTypeId: string, roleName: string } | null>(null);

    const handleAddRole = async () => {
        if (!selectedRoleType) return;
        try {
            await addPartyRole({ partyId, roleTypeId: selectedRoleType.roleTypeId }).unwrap();
            toast.success(getTranslatedLabel("party.parties.manageRoles.addedSuccess", "Role added successfully"));
            setSelectedRoleType(null);
        } catch (err: any) {
            toast.error(err?.data || getTranslatedLabel("party.parties.manageRoles.addedError", "Failed to add role"));
        }
    };

    const handleDeleteRole = async (roleTypeId: string) => {
        try {
            await deletePartyRole({ partyId, roleTypeId }).unwrap();
            toast.success(getTranslatedLabel("party.parties.manageRoles.deletedSuccess", "Role removed successfully"));
        } catch (err: any) {
            toast.error(err?.data || getTranslatedLabel("party.parties.manageRoles.deletedError", "Failed to remove role"));
        }
    };

    const DeleteCell = (props: any) => {
        const isNonDeletable = NON_DELETABLE_ROLES.includes(props.dataItem.roleTypeId);

        if (isNonDeletable) {
            return (
                <td>
                    <Tooltip title={getTranslatedLabel("party.parties.manageRoles.nonDeletable", "Core role - non-deletable")}>
                        <span>
                            <IconButton color="default" disabled>
                                <LockIcon fontSize="small" />
                            </IconButton>
                        </span>
                    </Tooltip>
                </td>
            );
        }

        return (
            <td>
                <IconButton
                    color="error"
                    onClick={() => handleDeleteRole(props.dataItem.roleTypeId)}
                    disabled={isDeleting}
                >
                    <DeleteIcon />
                </IconButton>
            </td>
        );
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
            <DialogTitle>
                {getTranslatedLabel("party.parties.manageRoles.manageTitle", "Manage Roles for")} {partyName}
            </DialogTitle>
            <DialogContent dividers>
                <Grid container spacing={3}>
                    <Grid item xs={12} md={5}>
                        <Box sx={{ p: 2, border: '1px solid #ddd', borderRadius: 1 }}>
                            <Typography variant="h6" gutterBottom>
                                {getTranslatedLabel("party.parties.manageRoles.addRole", "Add New Role")}
                            </Typography>
                            <FormControl fullWidth margin="normal">
                                
                                <ComboBox 
                                    data={roleTypes || []}
                                    value={selectedRoleType}
                                    popupSettings={{ appendTo: document.querySelector(".MuiModal-root") as HTMLElement }}
                                    textField="roleName"
                                    placeholder={''}
                                    label=''
                                    dataItemKey="roleTypeId"
                                    onChange={(e) => setSelectedRoleType(e.value)}
                                    filterable
                                    onFilterChange={(e: ComboBoxFilterChangeEvent) => {
                                        const filterValue = e.filter.value || "";
                                        setSearchTerm(filterValue);
                                    }}
                                />
                            </FormControl>
                            <Button
                                variant="contained"
                                color="primary"
                                startIcon={<AddIcon />}
                                fullWidth
                                onClick={handleAddRole}
                                disabled={!selectedRoleType || isAdding}
                                sx={{ mt: 2 }}
                            >
                                {isAdding ? getTranslatedLabel("common.adding", "Adding...") : getTranslatedLabel("common.add", "Add Role")}
                            </Button>
                        </Box>
                    </Grid>
                    <Grid item xs={12} md={7}>
                        <Typography variant="h6" gutterBottom>
                            {getTranslatedLabel("party.parties.manageRoles.assignedRoles", "Assigned Roles")}
                        </Typography>
                        {isLoadingPartyRoles ? (
                            <LoadingComponent message="Loading roles..." />
                        ) : (
                            <KendoGrid
                                data={partyRoles || []}
                                style={{ height: "300px" }}
                            >
                                <Column field="roleTypeId" title={getTranslatedLabel("party.parties.manageRoles.roleId", "Role ID")} width="150px" />
                                <Column field="roleName" title={getTranslatedLabel("party.parties.manageRoles.roleName", "Role Name")} />
                                <Column cell={DeleteCell} width="80px" filterable={false} sortable={false} />
                            </KendoGrid>
                        )}
                    </Grid>
                </Grid>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} variant="outlined">
                    {getTranslatedLabel("common.close", "Close")}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
