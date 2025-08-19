import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {Grid, Paper} from "@mui/material";
import Button from "@mui/material/Button";
import {useLocation} from 'react-router-dom';
import {DataResult, State} from "@progress/kendo-data-query";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {handleDatesArray} from "../../../app/util/utils";
import {RootState, useAppDispatch, useFetchProductionRunsQuery} from "../../../app/store/configureStore";
import {
    clearJobRunUnderProcessing,
    setJobRunUnderProcessing,
    setProductionRunStatusDescription
} from "../slice/manufacturingSharedUiSlice";
import CreateProductionRunEditForm from "../form/CreateProductionRunEditForm";
import CreateProductionRunDisplayForm from "../form/CreateProductionRunDisplayForm";
import {useSelector} from "react-redux";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";


function ProductionRunsList() {
    const location = useLocation();
    const dispatch = useAppDispatch();
    const [editMode, setEditMode] = useState(0);
    //const [productionRun, setProductionRun] = useState<DataResult>({data: [], total: 0});
    const initialDataState = {take: 6, skip: 0};
    const [dataState, setDataState] = React.useState<State>(initialDataState);
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    };
    const {data, isFetching} = useFetchProductionRunsQuery({...dataState});
    const [productionRuns, setProductionRuns] = useState<DataResult>({data: [], total: 0});
    const productionRunStatusDescription = useSelector((state: RootState) => state.manufacturingSharedUi.productionRunStatusDescription);
    const {getTranslatedLabel} = useTranslationHelper()

    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setProductionRuns({data: adjustedData, total: data.totalCount});
        }
    }, [data]);

    useEffect(() => {
        if (location.state?.editMode !== undefined) {
            setEditMode(location.state.editMode);
        }
    }, [location.state]);

    function handleSelectProductionRun(workEffortId: string) {
        const selectedProductionRun: any | undefined = data?.data?.find((productionRun: any) => productionRun.workEffortId === workEffortId);
        const workEffort = {
            workEffortId: selectedProductionRun.workEffortId,
            workEffortName: selectedProductionRun.workEffortName,
            description: selectedProductionRun.description,
            estimatedStartDate: selectedProductionRun.estimatedStartDate,
            actualStartDate: selectedProductionRun.actualStartDate,
            estimatedCompletionDate: selectedProductionRun.estimatedCompletionDate,
            actualCompletionDate: selectedProductionRun.actualCompletionDate,
            facilityId: selectedProductionRun.facilityId,
            facilityName: selectedProductionRun.facilityName,
            quantityToProduce: selectedProductionRun.quantityToProduce,
            currentStatusId: selectedProductionRun.currentStatusId,
            productId: selectedProductionRun.productId,
            productName: selectedProductionRun.productName,
            currentStatusDescription: selectedProductionRun.currentStatusDescription,
            quantityProduced: selectedProductionRun.quantityProduced,
            quantityRejected: selectedProductionRun.quantityRejected,
        };
        dispatch(setProductionRunStatusDescription(selectedProductionRun.currentStatusDescription));
        dispatch(setJobRunUnderProcessing(workEffort));

        setEditMode(2);
    }

    console.log('productionRunStatusDescription from list', productionRunStatusDescription);
    console.log('editMode', editMode);

    function cancelEdit() {
        setEditMode(0);
    }
    
    function initiateNewRecord() {
        console.log('initiateNewRecord');
        dispatch(clearJobRunUnderProcessing(undefined));
        dispatch(setProductionRunStatusDescription(undefined));

        setEditMode(1);
    }

    const ProductionRunIdCell = (props: any) => {
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
                onClick={() => handleSelectProductionRun(props.dataItem.workEffortId)}
            >
                {props.dataItem.workEffortId}
            </Button>

            </td>
        )
    };


        if ( editMode > 0 && (productionRunStatusDescription === undefined ) ) {
        return <CreateProductionRunEditForm editMode={1}
                                     cancelEdit={cancelEdit}
        />
    }

    if ( editMode > 0 && (productionRunStatusDescription === 'Created' || productionRunStatusDescription === 'Scheduled') ) {
        return <CreateProductionRunEditForm editMode={2}
                                     cancelEdit={cancelEdit}
        />
    }


    if (editMode > 0 && productionRunStatusDescription !== undefined && productionRunStatusDescription !== 'Created' && productionRunStatusDescription !== 'Scheduled') {
        return <CreateProductionRunDisplayForm/>
    }
    

    return (
        <>
            <ManufacturingMenu selectedMenuItem={'jobShop'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>

                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid style={{height: '65vh', flex: 1}}
                                       data={productionRuns ? productionRuns : {data: [], total: 0}}
                                       resizable={true}
                                       filterable={true}
                                       sortable={true}
                                       pageable={true}
                                       {...dataState}
                                       onDataStateChange={dataStateChange}
                            >
                                <GridToolbar>
                                    <Grid container>
                                        <Grid item xs={4}>
                                            <Button color={"secondary"} onClick={() => initiateNewRecord()}
                                                    variant="outlined">
                                                {getTranslatedLabel("manufacturing.jobshop.list.create", "Create Production Run")}
                                            </Button>
                                        </Grid>

                                    </Grid>
                                </GridToolbar>


                                <Column field="workEffortId" title={getTranslatedLabel("manufacturing.jobshop.list.runId", "Production Run Id")} cell={ProductionRunIdCell}
                                        width={100}
                                        locked={true}/>
                                <Column field="estimatedStartDate" title={getTranslatedLabel("manufacturing.jobshop.list.startDate", "Start Date")} width={150}
                                        format="{0: dd/MM/yyyy}"/>
                                <Column field="productName" title={getTranslatedLabel("manufacturing.jobshop.list.product", "Product")} width={150}/>
                                <Column field="workEffortName" title={getTranslatedLabel("manufacturing.jobshop.list.runName", "Production Run Name")} width={450}/>
                                <Column field="uomAndQuantity" title={getTranslatedLabel("manufacturing.jobshop.list.quantity", "Quantity To Produce")} width={200}/>
                                <Column field="currentStatusDescription" title={getTranslatedLabel("manufacturing.jobshop.list.status", "Current Status")} width={250}/>
                                <Column field="description" title={getTranslatedLabel("manufacturing.jobshop.list.description", "Description")} width={250}/>

                            </KendoGrid>
                            {isFetching && <LoadingComponent message={getTranslatedLabel("manufacturing.jobshop.list.loading", "Loading Production Runs...")}/>}
                        </div>

                    </Grid>

                </Grid>
            </Paper>
        </>
    )
}

export default ProductionRunsList;
