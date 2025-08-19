import React, { useState } from 'react'
import FacilityMenu from '../menu/FacilityMenu';
import { Button, Grid, Paper } from '@mui/material';
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
    GridDataStateChangeEvent,
} from "@progress/kendo-react-grid";
import {SortDescriptor, State,} from "@progress/kendo-data-query";
import { useTableKeyboardNavigation } from '@progress/kendo-react-data-tools';
import { useFetchFacilityLocationsQuery } from '../../../app/store/configureStore';

const FacilityLocationsList = () => {
    const initialDataState: State = {skip: 0, take: 4};
    const [editMode, setEditMode] = useState<number>(0)
    const [dataState, setDataState] = useState<State>(initialDataState)

    function cancelEdit() {
        setEditMode(0);
    }

    const handleDataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState)
    }

    const {data: facilityLocations, isFetching} = useFetchFacilityLocationsQuery({...dataState})

    const handleSelectFacilityLocation = (data: string) => {}
    
    const FacilityNameCell = (props: any) => {const field = props.field || '';
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
                    onClick={() => handleSelectFacilityLocation(props.dataItem.facilityId)}
                >
                    {props.dataItem.facilityName}
                </Button>
            </td>
        )
    }

  return (
    <>
    <FacilityMenu/>
    <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <KendoGrid
            data={facilityLocations?.data ?? []}
            sortable={true}
            total={facilityLocations?.total ?? 0}
            pageable={true}
            filterable={true}
            resizable={true}
            reorderable={true}
            {...dataState}
            onDataStateChange={handleDataStateChange}
        >
            <GridToolbar>
                <Grid container>
                    <Grid item xs={5}>
                        <Button color={"secondary"} onClick={() => {
                            setEditMode(1);
                        }} variant="outlined"
                        >
                            Create Facility Location
                        </Button>
                    </Grid>
                </Grid>
            </GridToolbar>

            <Column field="facilityName" title="Facility Name" cell={FacilityNameCell} width={280}
                    locked={true}/>
            <Column field="locationTypeEnumDescription" title="Location Type" width={200}/>
            <Column field="areaId" title="Area Id" />
            <Column field="aisleId" title="Aisle Id" />
            <Column field="sectionId" title="Section Id" />
            <Column field="levelId" title="Level Id" />
            <Column field="positionId" title="Position Id" />

        </KendoGrid>
    </Paper>
</>
  )
}

export default FacilityLocationsList