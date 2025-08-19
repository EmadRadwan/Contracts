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
import {useFetchServiceSpecificationQuery} from "../../../app/store/configureStore";
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {ServiceSpecification} from "../../../app/models/service/serviceSpecification";
import VehicleMenu from "../menu/VehicleMenu";
import ServiceSpecificationForm from "../form/ServiceSpecificationForm";
import {CompositeFilterDescriptor, filterBy, orderBy, SortDescriptor, State} from "@progress/kendo-data-query";


export default function ServiceSpecificationsList() {
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
    const [serviceSpecification, setServiceSpecification] = useState<ServiceSpecification | undefined>(undefined);


    const {data, error, isFetching}
        = useFetchServiceSpecificationQuery(undefined);


    function handleSelectServiceSpecification(serviceSpecificationId: string) {


        // select the serviceSpecification from data array based on serviceSpecificationId
        const selectedServiceSpecification: ServiceSpecification | undefined = data?.find((serviceSpecification: any) => serviceSpecification.serviceSpecificationId === serviceSpecificationId);

        // set component selected serviceSpecification
        setServiceSpecification(handleDatesObject(selectedServiceSpecification))
        setEditMode(2);

    }


    // convert cancelEdit function to memoizeed function
    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setServiceSpecification(undefined);
    }, [setEditMode, setServiceSpecification]);


    const ServiceSpecificationDescriptionCell = (props: any) => {
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

                    handleSelectServiceSpecification(props.dataItem.serviceSpecificationId)
                }}
            >
                {props.dataItem.productName}
            </Button>

            </td>
        )
    }


    // Code for Grid functionality
    const dataToExport = data ? handleDatesArray(data) : []

    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            // @ts-ignore
            _export.current!.save();
        }
    };


    if (editMode > 0) {
        return <ServiceSpecificationForm selectedServiceSpecification={serviceSpecification} cancelEdit={cancelEdit}
                                         editMode={editMode}/>

    }


    return <>
        <VehicleMenu/>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <Grid container columnSpacing={1} alignItems="center">
                <Grid item xs={8}>
                    {/* <Grid container alignItems="center">
                        <Grid item xs={6}>
                            <Typography sx={{p: 2}} variant='h4'>Service Specifications</Typography>
                        </Grid>
                    </Grid> */}


                    <Grid container>

                        <div className="div-container">
                            <ExcelExport data={dataToExport}
                                         ref={_export}>
                                <KendoGrid className="small-line-height" style={{height: "65vh", width: "94vw", flex: 1}}
                                           data={data ? orderBy(filterBy(handleDatesArray(data), filter), sort).slice(page.skip, page.take + page.skip) : []}
                                           sortable={true}
                                           sort={sort}
                                           onSortChange={(e: GridSortChangeEvent) => {
                                               setSort(e.sort);
                                           }}
                                           skip={page.skip}
                                           take={page.take}
                                           total={data?.length || 0}
                                           pageable={true}
                                           onPageChange={pageChange}
                                           filterable={true}
                                           filter={filter}
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
                                            <Grid item xs={4}>
                                                <Button color={"secondary"} onClick={() => setEditMode(1)}
                                                        variant="outlined">
                                                    Create Service Specification
                                                </Button>
                                            </Grid>
                                        </Grid>


                                    </GridToolbar>
                                    <Column field="serviceSpecificationId"
                                            title="Id"
                                            width={0}
                                    />
                                    <Column field="productName"
                                            title="Service"
                                            cell={ServiceSpecificationDescriptionCell} 
                                            locked={true}/>
                                    <Column field="makeDescription" title="Make" />
                                    <Column field="modelDescription" title="Model" />
                                    <Column field="standardTimeInMinutes" title="Standard Time in Minutes" />
                                    <Column field="fromDate" title="From" 
                                            filter="date" format="{0: dd/MM/yyyy}"/>
                                    <Column field="thruDate" title="To" 
                                            filter="date" format="{0: dd/MM/yyyy}"/>


                                </KendoGrid>
                            </ExcelExport>
                            {isFetching && <LoadingComponent message='Loading Service Specifications...'/>}
                        </div>

                    </Grid>
                </Grid>
            </Grid>
        </Paper>
    </>
}

