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
import {useFetchVehicleModelsQuery} from "../../../app/store/apis";
import {CompositeFilterDescriptor, filterBy, orderBy, SortDescriptor, State,} from "@progress/kendo-data-query";
import {handleDatesArray} from "../../../app/util/utils";
import {ExcelExport} from "@progress/kendo-react-excel-export";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {Model} from "../../../app/models/service/vehicle";
import ModelsForm from "../form/ModelsForm";


export default function ModelsList() {
    const {getTranslatedLabel} = useTranslationHelper();
    const initialDataState: State = {skip: 0, take: 5};
    const initialFilter: CompositeFilterDescriptor = {
        logic: "and",
        filters: [{field: "modelDescription", operator: "neq", value: ""}],
    };
    const [filter, setFilter] = React.useState<CompositeFilterDescriptor>(initialFilter);
    const [page, setPage] = React.useState<any>(initialDataState);

    const initialSort: Array<SortDescriptor> = [
        {field: "modelId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const [editMode, setEditMode] = React.useState(0)

    const [model, setModel] = useState<Model | undefined>(undefined);

    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setModel(undefined);
    }, [setEditMode, setModel]);

    const {data: modelsData, error: modelsError, isFetching: modelsIsFetching} = useFetchVehicleModelsQuery(undefined)

    function handleSelectModel(modelId: string) {
        const selectedModel: Model | undefined = modelsData?.find((model: any) => model.modelId === modelId)

        setModel(selectedModel)
        setEditMode(2)
    }

    // Code for Grid functionality
    const dataToExport = modelsData ? modelsData : []

    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            // @ts-ignore
            _export.current!.save();
        }
    };

    const ModelDescriptionCell = (props: any) => {
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
                    handleSelectModel(props.dataItem.modelId)
                }}
            >
                {props.dataItem.modelDescription}
            </Button>

            </td>
        )
    }

    if (editMode > 0) return <ModelsForm selectedModel={model} cancelEdit={cancelEdit} editMode={editMode}/>

    return <>

        <VehicleMenu/>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <Grid container columnSpacing={1} alignItems="center">
                <Grid item xs={8}>
                    {/* <Grid container alignItems="center">
                        <Grid item xs={4}>
                            <Typography sx={{p: 2}} variant='h4'>Models List</Typography>
                        </Grid>
                    </Grid> */}

                    <Grid container>

                        <div className="div-container">
                            <ExcelExport data={dataToExport}
                                         ref={_export}>
                                <KendoGrid className="small-line-height" style={{height: "65vh", width: "94vw", flex: 1}}
                                           data={modelsData ? orderBy(filterBy(handleDatesArray(modelsData), filter), sort).slice(page.skip, page.take + page.skip) : []}
                                           sortable={true}
                                           sort={sort}
                                           onSortChange={(e: GridSortChangeEvent) => {
                                               setSort(e.sort)
                                           }}
                                           skip={page.skip}
                                           take={page.take}
                                           total={modelsData?.length || 0}
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
                                                    Add New Model
                                                </Button>
                                            </Grid>
                                        </Grid>


                                    </GridToolbar>
                                    <Column field="modelId"
                                            title="Id"
                                            width={0}
                                    />
                                    <Column field="modelDescription"
                                            title="Model"
                                            cell={ModelDescriptionCell}
                                    />
                                    <Column field="makeId"
                                            title="Make"
                                    />
                                </KendoGrid>
                                {modelsIsFetching && <LoadingComponent message='Loading Models...'/>}
                            </ExcelExport>
                        </div>

                    </Grid>
                </Grid>
            </Grid>
        </Paper>
    </>
}