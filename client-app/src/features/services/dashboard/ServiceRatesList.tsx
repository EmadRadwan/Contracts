import React, {useState} from "react";
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
import {ExcelExport} from "@progress/kendo-react-excel-export";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {useFetchServiceRatesQuery} from "../../../app/store/configureStore";
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {ServiceRate} from "../../../app/models/service/serviceRate";
import VehicleMenu from "../menu/VehicleMenu";
import ServiceRateForm from "../form/ServiceRateForm";
import {CompositeFilterDescriptor, filterBy, orderBy, SortDescriptor, State,} from "@progress/kendo-data-query";


export default function ServiceRatesList() {
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
        {field: "fromDate", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);


    const [editMode, setEditMode] = useState(0);
    const [serviceRate, setServiceRate] = useState<ServiceRate | undefined>(undefined);


    const {data: serviceRatesData, error, isFetching}
        = useFetchServiceRatesQuery(undefined);


    function handleSelectServiceRate(serviceRateId: string) {
        // select the serviceRate from data array based on serviceRateId
        const selectedServiceRate: ServiceRate | undefined = serviceRatesData?.find((serviceRate: any) => serviceRate.serviceRateId === serviceRateId);

        // set component selected serviceRate
        setServiceRate(handleDatesObject(selectedServiceRate))
        setEditMode(2);

    }


    // convert cancelEdit function to memoizeed function
    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setServiceRate(undefined);
    }, [setEditMode, setServiceRate]);


    const ServiceRateDescriptionCell = (props: any) => {
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

                    handleSelectServiceRate(props.dataItem.serviceRateId)
                }}
            >
                {props.dataItem.makeDescription}
            </Button>

            </td>
        )
    }


    // Code for Grid functionality
    const dataToExport = serviceRatesData ? serviceRatesData : []
    //let dataToExport = checked ? data : filterBy(data, {field: 'selected', operator: 'eq', value: true})

    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            // @ts-ignore
            _export.current!.save();
        }
    };


    if (editMode > 0) {
        return <ServiceRateForm selectedServiceRate={serviceRate} cancelEdit={cancelEdit} editMode={editMode}/>

    }


    return <>
        <VehicleMenu/>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <Grid container columnSpacing={1} alignItems="center">
                <Grid item xs={8}>
                    {/* <Grid container alignItems="center">
                        <Grid item xs={4}>
                            <Typography sx={{p: 2}} variant='h4'>Service Rates</Typography>
                        </Grid>
                    </Grid> */}


                    <Grid container>

                        <div className="div-container">
                            <ExcelExport data={dataToExport}
                                         ref={_export}>
                                <KendoGrid className="small-line-height" style={{height: "65vh", width: "94vw", flex: 1}}
                                           data={serviceRatesData ? orderBy(filterBy(handleDatesArray(serviceRatesData), filter), sort).slice(page.skip, page.take + page.skip) : []}
                                           sortable={true}
                                           sort={sort}
                                           onSortChange={(e: GridSortChangeEvent) => {
                                               setSort(e.sort);
                                           }}
                                           skip={page.skip}
                                           take={page.take}
                                           total={serviceRatesData?.length || 0}
                                           pageable={true}
                                           onPageChange={pageChange}
                                           filterable={true}
                                           filter={filter}
                                    //filterOperators={filterOperators}
                                           onFilterChange={(e: GridFilterChangeEvent) => setFilter(e.filter)}
                                           resizable={true}
                                           reorderable={true}
                                >
                                    <GridToolbar>
                                        <Grid container spacing={2} alignItems="flex-end">
                                            {/* <Grid item xs={3}>
                                                <button title="Export Excel" className="k-button k-primary"
                                                        onClick={excelExport}>
                                                    Export to Excel
                                                </button>
                                            </Grid> */}
                                            <Grid item xs={3}>
                                                <Button color={"secondary"} onClick={() => setEditMode(1)}
                                                        variant="outlined">
                                                    Create Service Rate
                                                </Button>
                                            </Grid>
                                        </Grid>


                                    </GridToolbar>
                                    <Column field="serviceRateId"
                                            title="Id"
                                            width={0}
                                    />
                                    <Column field="makeDescription"
                                            title="Make"
                                            cell={ServiceRateDescriptionCell} 
                                            locked={true}/>
                                    <Column field="modelDescription" title="Model" />
                                    <Column field="productStoreName" title="Workshop" />
                                    <Column field="rate" title="Service Rate in Workshop Currency" />
                                    <Column field="fromDate" title="From" 
                                            filter="date" format="{0: dd/MM/yyyy}"/>
                                    <Column field="thruDate" title="To" 
                                            filter="date" format="{0: dd/MM/yyyy}"/>


                                </KendoGrid>
                            </ExcelExport>
                            {isFetching && <LoadingComponent message='Loading Service Rates...'/>}
                        </div>

                    </Grid>
                </Grid>
            </Grid>
        </Paper>
    </>
}

