import React, {useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import {SortDescriptor, State,} from "@progress/kendo-data-query";

import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {Grid, Paper} from "@mui/material";
import {useAppDispatch, useFetchFacilitiesQuery,} from "../../../app/store/configureStore";
import FacilityForm from "../form/FacilityForm";
import Button from "@mui/material/Button";
import {setFacilityId} from "../slice/facilityInventoryUiSlice";
import {Facility} from "../../../app/models/facility/facility";
import FacilityMenu from "../menu/FacilityMenu";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";


export default function FacilitiesList() {

    const [editMode, setEditMode] = useState(0);
    const [facility, setFacility] = useState<Facility | undefined>(undefined);
    const {data: facilities, error, isFetching, isLoading} = useFetchFacilitiesQuery(undefined);
    const dispatch = useAppDispatch();
    const initialDataState: State = {skip: 0, take: 4};
    const {getTranslatedLabel} = useTranslationHelper()


    function handleSelectFacility(facilityId: string) {
        const selectedFacility: Facility | undefined = facilities?.find((facility: any) => facility.facilityId === facilityId);

        dispatch(setFacilityId(facilityId))
        setFacility(selectedFacility);
        setEditMode(2);
    }

    function cancelEdit() {
        setEditMode(0);
    }


    const initialSort: Array<SortDescriptor> = [
        {field: "facilityName", dir: "desc"},
    ];


    const FacilityNameCell = (props: any) => {
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
            >
                <Button
                    onClick={() => handleSelectFacility(props.dataItem.facilityId)}
                >
                    {props.dataItem.facilityName}
                </Button>
            </td>
        )
    }


    // Code for Grid functionality

    const [page, setPage] = React.useState<any>(initialDataState);


    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const [sort, setSort] = React.useState(initialSort);


    if (editMode) {
        return <FacilityForm facility={facility} cancelEdit={cancelEdit} editMode={editMode}/>
    }

    return (
        <>
            <FacilityMenu/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <KendoGrid
                    data={facilities ? facilities : []}
                    sortable={true}
                    sort={sort}
                    onSortChange={(e: GridSortChangeEvent) => {
                        setSort(e.sort);
                    }}
                    skip={page.skip}
                    take={page.take}
                    total={facilities ? facilities.length : 0}
                    pageable={true}
                    onPageChange={pageChange}
                    filterable={true}
                    resizable={true}
                    reorderable={true}
                >
                    <GridToolbar>
                        <Grid container>
                            <Grid item xs={5}>
                                <Button color={"secondary"} onClick={() => {
                                    setEditMode(1);
                                }} variant="outlined"
                                >
                                    {getTranslatedLabel("facility.facilities.create", "Create Facility")}
                                </Button>
                            </Grid>
                        </Grid>
                    </GridToolbar>

                    <Column field="facilityName" title={getTranslatedLabel("facility.facilities.list.name", "Facility Name")} cell={FacilityNameCell} width={300}
                            locked={true}/>
                    <Column field="facilityTypeDescription" title={getTranslatedLabel("facility.facilities.list.type", "Facility Type")} />

                </KendoGrid>
            </Paper>
        </>


    )
}