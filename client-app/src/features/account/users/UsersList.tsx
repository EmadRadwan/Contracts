import React, {useMemo, useState} from 'react';
import { useFetchUsersQuery } from '../../../app/store/apis';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GRID_COL_INDEX_ATTRIBUTE, GridPageChangeEvent, GridFilterChangeEvent
} from "@progress/kendo-react-grid";
import { Grid, Paper, Button } from "@mui/material";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { UserListDto } from '../../../app/store/apis';
import UserForm from "./UserForm";
import UsersMenu from "./menu/UsersMenu";
import {CompositeFilterDescriptor, filterBy, orderBy, SortDescriptor, State} from "@progress/kendo-data-query";

const UsersList = () => {
    const { data: users, isLoading, error } = useFetchUsersQuery();
    const { getTranslatedLabel } = useTranslationHelper();
    const [editMode, setEditMode] = React.useState(0);
    const [selectedUser, setSelectedUser] = React.useState<UserListDto | undefined>(undefined);
    const [filter, setFilter] = useState<CompositeFilterDescriptor | undefined>(undefined);
    const initialSort: Array<SortDescriptor> = [
        { field: "userName", dir: "asc" },
    ];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = { skip: 0, take: 25 };
    const [page, setPage] = useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const onFilterChange = (event: GridFilterChangeEvent) => {
        setFilter(event.filter);
    };

    const processedData = useMemo(() => {
        const filtered = filterBy(users ?? [], filter);

        const sorted = orderBy(filtered, sort);

        return sorted.slice(page.skip, page.skip + page.take);
    }, [users, filter, sort, page.skip, page.take]);

// Also update total for correct pager
    const total = filter ? filterBy(users ?? [], filter).length : users?.length ?? 0;




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
                            sortable={true}
                            data={processedData}
                            sort={sort}
                            onSortChange={(e) => setSort(e.sort)}

                            filterable={true}              // ← enable filter row
                            filter={filter}                // ← controlled filter
                            onFilterChange={onFilterChange}

                            skip={page.skip}
                            take={page.take}
                            total={total}               
                            pageable={true}
                            onPageChange={pageChange}
                        >
                            <GridToolbar>
                                <Button
                                    variant="contained"
                                    color="primary"
                                    onClick={() => setEditMode(1)}
                                >
                                    {getTranslatedLabel("users.list.createUser", "Create User")}
                                </Button>
                            </GridToolbar>

                            <Column
                                field="displayName"
                                cell={UserDescriptionCell}
                                title={getTranslatedLabel("users.list.displayName", "Display Name")}
                                width={350}
                                filter="text"
                            />
                            <Column
                                field="email"
                                title={getTranslatedLabel("users.list.email", "Email")}
                                filter="text"
                            />
                            <Column
                                field="organizationPartyId"
                                title={getTranslatedLabel("users.list.organization", "Organization")}
                                width={200}
                                filter="text"
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
