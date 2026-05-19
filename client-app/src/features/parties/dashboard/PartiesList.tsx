import React, {useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";

import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Box,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControl,
    Grid,
    InputLabel,
    MenuItem,
    Paper,
    Select
} from "@mui/material";
import CreateCustomerForm from "../form/CreateCustomerForm";
import CreateSupplierForm from "../form/CreateSupplierForm";
import CreateSalesRepForm from "../form/CreateSalesRepForm";
import CreateBrokerForm from "../form/CreateBrokerForm";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {useFetchPartiesQuery, useUpdatePartyMainRoleMutation} from "../../../app/store/apis";
import {Party} from "../../../app/models/party/party";
import {State} from "@progress/kendo-data-query";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import CreateContractorForm from "../form/CreateContractorForm";
import CreateEmployeeForm from "../form/CreateEmployeeForm";
import {toast} from "react-toastify";
import ManageRolesModal from "./ManageRolesModal";
import {PartiesExcel} from "../report/PartiesExcel";
import {Can} from "../../account/Can";
import {useDeletePartyMutation} from "../../../app/store/apis";
import {DialogContentText} from "@mui/material";


export default function PartiesList() {

    const [editMode, setEditMode] = useState(0);
    const [form, setForm] = useState('');
    const [page, setPage] = useState(1)
    const [show, setShow] = useState(false);
    const params = {pageNumber: page, pageSize: 6, orderBy: "orderIdAsc", roleTypes: []}
    const [dataState, setDataState] = React.useState<State>({take: 6, skip: 0});
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };
    const {getTranslatedLabel} = useTranslationHelper();

    const [party, setParty] = useState<Party | undefined>(undefined);

    const {data: parties, error, isFetching, isLoading} = useFetchPartiesQuery({...dataState});
    const [roleDialogOpen, setRoleDialogOpen] = useState(false);
    const [manageRolesOpen, setManageRolesOpen] = useState(false);
    const [selectedPartyId, setSelectedPartyId] = useState<string | null>(null);
    const [selectedPartyName, setSelectedPartyName] = useState<string>("");
    const [newRole, setNewRole] = useState<string>("");

    const [updateMainRole, {isLoading: isUpdatingRole}] = useUpdatePartyMainRoleMutation();
    const [deleteParty, {isLoading: isDeleting}] = useDeletePartyMutation();

    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [partyToDelete, setPartyToDelete] = useState<string | null>(null);

    const handleDeleteClick = (partyId: string) => {
        setPartyToDelete(partyId);
        setDeleteDialogOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!partyToDelete) return;

        try {
            await deleteParty(partyToDelete).unwrap();
            toast.success(getTranslatedLabel("party.parties.list.deleteSuccess", "Party deleted successfully"));
        } catch (err: any) {
            let errorMessage = getTranslatedLabel("party.parties.list.deleteFailed", "Failed to delete party");
            if (err?.data) {
                errorMessage = err.data.title;
            } else if (typeof err?.data === "string") {
                errorMessage = err.data;
            }
            toast.error(errorMessage);
        } finally {
            setDeleteDialogOpen(false);
            setPartyToDelete(null);
        }
    };

    const handleCancelDelete = () => {
        setDeleteDialogOpen(false);
        setPartyToDelete(null);
    };

    function handleSelectParty(partyId: string, formValue: string) {
        const selectedParty: Party | undefined = parties?.data.find((party: any) => party.partyId === partyId);
        console.log('selectedParty', selectedParty)
        setParty(selectedParty)
        setEditMode(2);
    }

    const ChangeRoleCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);

        const handleOpenDialog = () => {
            setSelectedPartyId(props.dataItem.partyId);
            setNewRole(props.dataItem.mainRole || "");
            setRoleDialogOpen(true);
        };

        return (
            <td {...navigationAttributes}>
                <Box sx={{ display: 'flex', gap: 1 }}>
                    <Button
                        size="small"
                        variant="outlined"
                        color="primary"
                        onClick={handleOpenDialog}
                    >
                        {getTranslatedLabel("party.parties.list.changeMainRoleTitle", "Change Role")}
                    </Button>
                    <Button
                        size="small"
                        variant="outlined"
                        color="secondary"
                        onClick={() => {
                            setSelectedPartyId(props.dataItem.partyId);
                            setSelectedPartyName(props.dataItem.description);
                            setManageRolesOpen(true);
                        }}
                    >
                        {getTranslatedLabel("party.parties.list.manageRoles", "Manage Roles")}
                    </Button>
                    <Can perform="deleteParty">
                        <Button
                            size="small"
                            variant="outlined"
                            color="error"
                            disabled={isDeleting}
                            onClick={() => handleDeleteClick(props.dataItem.partyId)}
                        >
                            {getTranslatedLabel("common.delete", "Delete")}
                        </Button>
                    </Can>
                </Box>
            </td>
        );
    };

    function cancelEdit() {
        setEditMode(0);
        setParty(undefined);
        setForm('')
    }


    const PartyDescriptionCell = (props: any) => {
        const field = props.field || '';
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: 'blue'}}
                colSpan={props.colSpan}
                role={'gridcell'}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{
                    [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex
                }}
                {...navigationAttributes}
            ><Button
                onClick={() => {
                    console.log('dataItem', props.dataItem)
                    const startAt = props.dataItem.description.indexOf('(')
                    const endAt = props.dataItem.description.indexOf(')')
                    //console.log('MainRole', props.dataItem.description.substring(startAt + 2, endAt - 1))
                    const formValue = props.dataItem.mainRole;
                    setForm(formValue)
                    handleSelectParty(props.dataItem.partyId, formValue)
                }}
            >
                {props.dataItem.description}
            </Button>

            </td>
        )
    }

    console.log('form', form);

    if (editMode > 0 && form === 'CUSTOMER') {
        return <CreateCustomerForm party={party} cancelEdit={cancelEdit} editMode={editMode}/>
    }
    if (editMode > 0 && form === 'SUPPLIER') {
        return <CreateSupplierForm party={party} cancelEdit={cancelEdit} editMode={editMode}/>
    }
    if (editMode > 0 && form === 'CONTRACTOR') {
        return <CreateContractorForm party={party} cancelEdit={cancelEdit} editMode={editMode}/>
    }

    if (editMode > 0 && form === 'EMPLOYEE') {
        return <CreateEmployeeForm party={party} cancelEdit={cancelEdit} editMode={editMode}/>; // ← NEW
    }

    if (editMode > 0 && form === 'SALES_REP') {
        return <CreateSalesRepForm party={party} cancelEdit={cancelEdit} editMode={editMode}/>;
    }

    if (editMode > 0 && form === 'BROKER') {
        return <CreateBrokerForm party={party} cancelEdit={cancelEdit} editMode={editMode}/>;
    }


    return (
        <>
            <Paper elevation={5} className={`div-container-withBorderCurved`} style={{marginTop: 15}}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={8}>

                        <Grid container>
                            <div className="div-container">
                                <KendoGrid
                                    style={{height: "75vh", width: "94vw", flex: 1}}
                                    resizable={true}
                                    filterable={true}
                                    sortable={true}
                                    pageable={true}
                                    {...dataState}
                                    data={parties ? parties : {data: [], total: 77}}
                                    onDataStateChange={dataStateChange}
                                >
                                    <GridToolbar>
                                        <Grid container>

                                            <Grid item xs={2}>
                                                <Button color={"secondary"} onClick={() => {
                                                    setEditMode(1);
                                                    setForm('CUSTOMER')
                                                }}
                                                        variant="outlined">
                                                    {getTranslatedLabel("party.parties.list.createCustomer", "Create Customer")}
                                                </Button>
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Button color={"secondary"} onClick={() => {
                                                    setEditMode(1);
                                                    setForm('SUPPLIER')
                                                }}
                                                        variant="outlined">
                                                    {getTranslatedLabel("party.parties.list.createSupplier", "Create Supplier")}
                                                </Button>
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Button color={"secondary"} onClick={() => {
                                                    setEditMode(1);
                                                    setForm('EMPLOYEE')
                                                }}
                                                        variant="outlined">
                                                    {getTranslatedLabel("party.parties.list.createEmployee", "Create Employee")}
                                                </Button>
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Button color={"secondary"} onClick={() => {
                                                    setEditMode(1);
                                                    setForm('CONTRACTOR')
                                                }}
                                                        variant="outlined">
                                                    {getTranslatedLabel("party.parties.list.createContractor", "Create Contractor")}
                                                </Button>
                                            </Grid>

                                           

                                            <Grid item xs={2}>
                                                <Button color={"secondary"} onClick={() => {
                                                    setEditMode(1);
                                                    setForm('BROKER')
                                                }}
                                                        variant="outlined">
                                                    {getTranslatedLabel("party.parties.list.createBroker", "Create Broker")}
                                                </Button>
                                            </Grid>

                                            <Grid item xs={2}>
                                                <PartiesExcel getTranslatedLabel={getTranslatedLabel} />
                                            </Grid>

                                        </Grid>


                                    </GridToolbar>
                                    <Column
                                        field="description"
                                        title={getTranslatedLabel("party.parties.list.description", "Party")}
                                        cell={PartyDescriptionCell}
                                        width={300}
                                        locked={true}
                                    />
                                    <Column
                                        field="partyId"
                                        title={getTranslatedLabel("party.parties.list.partyId", "Party Id")}
                                    />
                                    <Column
                                        field="mobileContactNumber"
                                        title={getTranslatedLabel("party.parties.list.contactNumber", "Contact Number")}
                                    />
                                    <Column
                                        field="address1"
                                        title={getTranslatedLabel("party.parties.list.address", "Address")}
                                    />
                                    <Column
                                        field="infoString"
                                        title={getTranslatedLabel("party.parties.list.email", "Email")}
                                    />
                                    <Column
                                        field="partyTypeDescription"
                                        title={getTranslatedLabel("party.parties.list.partyType", "Party Type")}
                                    />
                                    <Column
                                        title={getTranslatedLabel("party.parties.list.actions", "Actions")}
                                        cell={ChangeRoleCell}
                                        width={280}
                                        filterable={false}
                                        sortable={false}
                                    />
                                </KendoGrid>
                                {selectedPartyId && (
                                    <ManageRolesModal
                                        partyId={selectedPartyId}
                                        partyName={selectedPartyName}
                                        open={manageRolesOpen}
                                        onClose={() => {
                                            setManageRolesOpen(false);
                                            setSelectedPartyId(null);
                                            setSelectedPartyName("");
                                        }}
                                    />
                                )}
                                <Dialog open={roleDialogOpen} onClose={() => setRoleDialogOpen(false)} maxWidth="xs"
                                        fullWidth>
                                    <DialogTitle>
                                        {getTranslatedLabel("party.parties.list.changeMainRoleTitle", "Change Main Role")}
                                    </DialogTitle>
                                    <DialogContent>
                                        <FormControl fullWidth margin="normal">
                                            <InputLabel>
                                                {getTranslatedLabel("party.parties.list.newRole", "New Role")}
                                            </InputLabel>
                                            <Select
                                                value={newRole}
                                                label={getTranslatedLabel("party.parties.list.newRole", "New Role")}
                                                onChange={(e) => setNewRole(e.target.value as string)}
                                            >
                                                <MenuItem value="CUSTOMER">
                                                    {getTranslatedLabel("party.roles.customer", "Customer")}
                                                </MenuItem>
                                                <MenuItem value="SUPPLIER">
                                                    {getTranslatedLabel("party.roles.supplier", "Supplier")}
                                                </MenuItem>
                                                <MenuItem value="EMPLOYEE">
                                                    {getTranslatedLabel("party.roles.employee", "Employee")}
                                                </MenuItem>
                                                <MenuItem value="CONTRACTOR">
                                                    {getTranslatedLabel("party.roles.contractor", "Contractor")}
                                                </MenuItem>
                                                <MenuItem value="SALES_REP">
                                                    {getTranslatedLabel("party.roles.salesRep", "Sales Rep")}
                                                </MenuItem>
                                                <MenuItem value="BROKER">
                                                    {getTranslatedLabel("party.roles.broker", "Broker")}
                                                </MenuItem>
                                                <MenuItem value="PREVIOUS_EMPLOYEE">
                                                    {getTranslatedLabel("party.roles.previousEmployee", "Previous Employee")}
                                                </MenuItem>
                                            </Select>
                                        </FormControl>
                                    </DialogContent>
                                    <DialogActions>
                                        <Button onClick={() => setRoleDialogOpen(false)}>
                                            {getTranslatedLabel("common.cancel", "Cancel")}
                                        </Button>
                                        <Button
                                            onClick={async () => {
                                                if (!selectedPartyId || !newRole) return;

                                                try {
                                                    await updateMainRole({
                                                        partyId: selectedPartyId,
                                                        mainRole: newRole,
                                                    }).unwrap();

                                                    toast.success(
                                                        getTranslatedLabel("party.parties.list.roleUpdatedSuccess", "Role updated successfully")
                                                    );
                                                    setRoleDialogOpen(false);
                                                    setSelectedPartyId(null);
                                                    setNewRole("");
                                                } catch (err) {
                                                    toast.error(
                                                        getTranslatedLabel("party.parties.list.roleUpdateFailed", "Failed to update role")
                                                    );
                                                }
                                            }}
                                            disabled={isUpdatingRole || !newRole}
                                            variant="contained"
                                            color="primary"
                                        >
                                            {isUpdatingRole
                                                ? getTranslatedLabel("common.saving", "Saving...")
                                                : getTranslatedLabel("common.save", "Save")}
                                        </Button>
                                    </DialogActions>
                                </Dialog>
                                <Dialog
                                    open={deleteDialogOpen}
                                    onClose={handleCancelDelete}
                                    aria-labelledby="delete-party-dialog-title"
                                    aria-describedby="delete-party-dialog-description"
                                >
                                    <DialogTitle id="delete-party-dialog-title">
                                        {getTranslatedLabel(
                                            "party.parties.list.deleteDialogTitle",
                                            "Confirm Deletion"
                                        )}
                                    </DialogTitle>
                                    <DialogContent>
                                        <DialogContentText id="delete-party-dialog-description">
                                            {getTranslatedLabel(
                                                "party.parties.list.deleteDialogMessage",
                                                "Are you sure you want to delete party {0}? This action cannot be undone."
                                            ).replace("{0}", partyToDelete || "")}
                                        </DialogContentText>
                                    </DialogContent>
                                    <DialogActions>
                                        <Button onClick={handleCancelDelete} disabled={isDeleting}>
                                            {getTranslatedLabel("common.cancel", "Cancel")}
                                        </Button>
                                        <Button
                                            onClick={handleConfirmDelete}
                                            color="error"
                                            variant="contained"
                                            disabled={isDeleting}
                                            autoFocus
                                        >
                                            {isDeleting
                                                ? getTranslatedLabel("common.deleting", "Deleting...")
                                                : getTranslatedLabel("common.delete", "Delete")}
                                        </Button>
                                    </DialogActions>
                                </Dialog>
                                {isFetching && <LoadingComponent
                                message={getTranslatedLabel("party.parties.list.loading", "Loading Parties...")}/>}
                            </div>

                        </Grid>
                    </Grid>
                </Grid>
            </Paper>
        </>


    )
}
