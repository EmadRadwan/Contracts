import React from 'react';
import { useFetchUsersQuery } from '../../../app/store/apis';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GRID_COL_INDEX_ATTRIBUTE
} from "@progress/kendo-react-grid";
import { Grid, Paper, Button } from "@mui/material";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { UserListDto } from '../../../app/store/apis';
import UserForm from "./UserForm";
import UsersMenu from "./menu/UsersMenu";

const UsersList = () => {
    const { data: users, isLoading, error } = useFetchUsersQuery();
    const { getTranslatedLabel } = useTranslationHelper();
    const [editMode, setEditMode] = React.useState(0);
    const [selectedUser, setSelectedUser] = React.useState<UserListDto | undefined>(undefined);

    if (isLoading) {
        return <LoadingComponent message={getTranslatedLabel("users.list.loading", 'Loading Users...')} />;
    }

    const handleSelectUser = (user: UserListDto) => {
        setSelectedUser(user);
        setEditMode(2);
    };

    const cancelEdit = () => {
        setEditMode(0);
        setSelectedUser(undefined);
    };

    if (editMode > 0) {
        return <UserForm user={selectedUser} editMode={editMode} cancelEdit={cancelEdit} />;
    }

    const UserDescriptionCell = (props: any) => {
        const field = props.field || '';
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: 'blue' }}
                colSpan={props.colSpan}
                role={'gridcell'}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button
                    onClick={() => {
                        handleSelectUser(
                            props.dataItem
                        );
                    }}
                >
                    {props.dataItem.displayName}
                </Button>
            </td>
        );
    };

    return (
        <>
        <UsersMenu />
        <Paper elevation={5} className="div-container-withBorderCurved">
            <Grid container columnSpacing={1} alignItems="center">
                <Grid item xs={12}>
                    <div className="div-container">
                        <KendoGrid
                            style={{ height: '65vh' }}
                            resizable={true}
                            filterable={true}
                            sortable={true}
                            pageable={true}
                            data={users}
                        >
                            <GridToolbar>
                                <Button
                                    variant="contained"
                                    color="primary"
                                    onClick={() => {
                                        setEditMode(1);
                                    }}
                                >
                                    {getTranslatedLabel("users.list.createUser", "Create User")}
                                </Button>
                            </GridToolbar>
                            <Column
                                cell={UserDescriptionCell}
                                title={getTranslatedLabel("users.list.displayName", "Display Name")}
                                width={350}
                            />
                            <Column
                                field="email"
                                title={getTranslatedLabel("users.list.email", "Email")}
                            />
                            <Column
                                field="organizationPartyId"
                                title={getTranslatedLabel("users.list.organization", "Organization")}
                                width={200}
                            />
                        </KendoGrid>
                    </div>
                </Grid>
            </Grid>
        </Paper>
        </>
    );
};

export default UsersList;
