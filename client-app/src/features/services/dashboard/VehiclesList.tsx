import React, {useEffect, useState} from "react";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {Grid, Paper, Typography} from "@mui/material";
import {ExcelExport} from "@progress/kendo-react-excel-export";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {Vehicle} from "../../../app/models/service/vehicle";
import {useFetchVehiclesQuery} from "../../../app/store/configureStore";
import {handleDatesArray} from "../../../app/util/utils";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import VehicleForm from "../form/VehicleForm";
import VehicleMenu from "../menu/VehicleMenu";
import {DataResult, State} from "@progress/kendo-data-query";


export default function VehiclesList() {
    const {getTranslatedLabel} = useTranslationHelper();


    const [editMode, setEditMode] = useState(0);
    const [vehicle, setVehicle] = useState<Vehicle | undefined>(undefined);
    const [show, setShow] = useState(false);
    const [dataState, setDataState] = React.useState<State>({take: 9, skip: 0});
    const [vehicles, setVehicles] = React.useState<DataResult>({data: [], total: 0});

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const {data, error, isFetching}
        = useFetchVehiclesQuery({...dataState});

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data);
                setVehicles({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    function handleSelectVehicle(chassisNumber: string) {


        // select the vehicle from data array based on vehicleId
        const selectedVehicle: Vehicle | undefined = data?.data.find((vehicle: any) => vehicle.chassisNumber === chassisNumber);

        // set component selected vehicle
        setVehicle(selectedVehicle)
        setEditMode(2);

    }

    // convert cancelEdit function to memoizeed function
    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setVehicle(undefined);
    }, [setEditMode, setVehicle]);


    const VehicleDescriptionCell = (props: any) => {
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

                    handleSelectVehicle(props.dataItem.chassisNumber)
                }}
            >
                {props.dataItem.chassisNumber}
            </Button>

            </td>
        )
    }


    // Code for Grid functionality
    const dataToExport = data ? handleDatesArray(data.data) : []

    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            // @ts-ignore
            _export.current!.save();
        }
    };


    if (editMode > 0) {
        return <VehicleForm selectedVehicle={vehicle} cancelEdit={cancelEdit} editMode={editMode}/>

    }


    return <>
        <VehicleMenu/>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <Grid container columnSpacing={1} alignItems="center">
                <Grid item xs={8}>
                    {/* <Grid container alignItems="center">
                        <Grid item xs={4}>
                            <Typography sx={{p: 2}} variant='h4'>Vehicles</Typography>
                        </Grid>


                    </Grid> */}


                    <Grid container>

                        <div className="div-container">
                            <ExcelExport data={dataToExport}
                                         ref={_export}>
                                <KendoGrid className="small-line-height" style={{height: "65vh", width: "94vw", flex: 1}}
                                           data={vehicles ? vehicles : {data: [], total: data!.total}}
                                           resizable={true}
                                           filterable={true}
                                           sortable={true}
                                           pageable={true}
                                           {...dataState}
                                           onDataStateChange={dataStateChange}
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
                                                    Create Vehicle
                                                </Button>
                                            </Grid>
                                        </Grid>


                                    </GridToolbar>
                                    <Column field="vehicleId"
                                            title="Id"
                                            width={0}
                                    />
                                    <Column field="chassisNumber"
                                            title="Chassis Number"
                                            cell={VehicleDescriptionCell} 
                                            locked={true}/>
                                    <Column field="fromPartyName" title="Customer" />
                                    <Column field="makeDescription" title="Make" />
                                    <Column field="modelDescription" title="Model" />
                                    <Column field="serviceDate" title="Last Service" 
                                            format="{0: dd/MM/yyyy}"/>
                                    <Column field="nextServiceDate" title="Next Service" 
                                            format="{0: dd/MM/yyyy}"/>


                                </KendoGrid>
                            </ExcelExport>
                            {isFetching && <LoadingComponent message='Loading Vehicles...'/>}
                        </div>

                    </Grid>
                </Grid>
            </Grid>
        </Paper>
    </>
}

