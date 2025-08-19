import React, {useState} from "react"
import VehicleMenu from "../menu/VehicleMenu"
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridFilterChangeEvent,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {Grid, Paper, Typography} from "@mui/material";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {useFetchVehicleMakesQuery} from "../../../app/store/apis";
import {CompositeFilterDescriptor, filterBy, orderBy, SortDescriptor, State,} from "@progress/kendo-data-query";
import {handleDatesArray} from "../../../app/util/utils";
import {ExcelExport} from "@progress/kendo-react-excel-export";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {Make} from "../../../app/models/service/vehicle";
import MakesForm from "../form/MakesForm";


export default function MakesList() {
    const {getTranslatedLabel} = useTranslationHelper();
    const initialDataState: State = {skip: 0, take: 4};
    const initialFilter: CompositeFilterDescriptor = {
        logic: "and",
        filters: [{field: "makeDescription", operator: "neq", value: ""}],
    };
    const [filter, setFilter] = React.useState<CompositeFilterDescriptor>(initialFilter);
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const initialSort: Array<SortDescriptor> = [
        {field: "makeId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const [editMode, setEditMode] = React.useState(0)
    const [make, setMake] = useState<Make | undefined>(undefined);

    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setMake(undefined);
    }, [setEditMode, setMake]);

    const {data: makesData, error, isFetching} = useFetchVehicleMakesQuery(undefined)

    function handleSelectMake(makeId: string) {
        const selectedMake: Make | undefined = makesData?.find((make: any) => make.makeId === makeId)

        setMake(selectedMake)
        setEditMode(2)
    }

    // Code for Grid functionality
    const dataToExport = makesData ? makesData : []

    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            // @ts-ignore
            _export.current!.save();
        }
    };

    const MakeDescriptionCell = (props: any) => {
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
                    handleSelectMake(props.dataItem.makeId)
                }}
            >
                {props.dataItem.makeDescription}
            </Button>

            </td>
        )
    }

    if (editMode > 0) return <MakesForm selectedMake={make} editMode={editMode} cancelEdit={cancelEdit}/>

    return (
        <>
            <VehicleMenu/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={8}>
                        {/* <Grid container alignItems="center">
                            <Grid item xs={4}>
                                <Typography sx={{p: 2}} variant='h4'>Makes List</Typography>
                            </Grid>
                        </Grid> */}

                        <Grid container>

                            <div className="div-container">
                                <ExcelExport data={dataToExport}
                                             ref={_export}>
                                    <KendoGrid className="small-line-height" style={{height: "65vh", width: "94vw", flex: 1}}
                                               data={makesData ? orderBy(filterBy(handleDatesArray(makesData), filter), sort).slice(page.skip, page.take + page.skip) : []}
                                               sortable={true}
                                               sort={sort}
                                               onSortChange={(e: GridSortChangeEvent) => {
                                                   setSort(e.sort)
                                               }}
                                               skip={page.skip}
                                               take={page.take}
                                               total={makesData?.length || 0}
                                               pageable={true}
                                               onPageChange={(event: GridPageChangeEvent) => {
                                                   setPage(event.page)
                                               }}
                                               filterable={true}
                                               filter={filter}
                                               onFilterChange={(e: GridFilterChangeEvent) => setFilter(e.filter)}
                                               resizable={true}
                                               reorderable={true}
                                    >
                                        <GridToolbar>
                                            <Grid container spacing={2} alignItems="flex-end">
                                                {/* <Grid item xs={5}>
                                                    <button title="Export Excel" className="k-button k-primary"
                                                            onClick={excelExport}>
                                                        Export to Excel
                                                    </button>
                                                </Grid> */}
                                                <Grid item xs={3}>
                                                    <Button color={"secondary"} onClick={() => setEditMode(1)}
                                                            variant="outlined">
                                                        Add New Make
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </GridToolbar>
                                        <Column field="makeId"
                                                title="Id"
                                                width={0}
                                        />
                                        <Column field="makeDescription"
                                                title="Make"
                                                cell={MakeDescriptionCell}
                                        />
                                    </KendoGrid>
                                </ExcelExport>
                                {isFetching && <LoadingComponent message='Loading Makes...'/>}
                            </div>

                        </Grid>
                    </Grid>
                </Grid>
            </Paper>
        </>
    )
}