import React, { useState } from 'react'
import AccountingMenu from '../../../invoice/menu/AccountingMenu'
import { Button, Grid, Paper } from '@mui/material'
import GlSettingsMenu from '../../menu/GlSettingsMenu'
import { State } from '@progress/kendo-data-query'
import { useFetchCostComponentCalcsQuery } from '../../../../../app/store/apis'
import { useTableKeyboardNavigation } from '@progress/kendo-react-data-tools'
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent, GridToolbar,
} from "@progress/kendo-react-grid";
import LoadingComponent from '../../../../../app/layout/LoadingComponent'
import AccountingCostsForm from '../form/AccountingCostsForm'

const AccountingCosts = () => {
    const [editMode, setEditMode] = useState(0);
    const [show, setShow] = useState(false);
    const [selectedCostComponent, setSelectedCostComponent] = useState(undefined)
    const [dataState, setDataState] = React.useState<State>({take: 9, skip: 0});
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };
    const {data, error, isFetching} = useFetchCostComponentCalcsQuery({...dataState});

    function handleSelectCostComponentCalc(costComponentCalcId: string) {
        const selectedCostComponentCalc: any | undefined = data?.data?.find((routing: any) => routing.costComponentCalcId === costComponentCalcId);
        setSelectedCostComponent(selectedCostComponentCalc)
        setEditMode(2);
    }

    function cancelEdit() {
        setSelectedCostComponent(undefined)
        setEditMode(0);
    }


    const CostComponentCalcIdCell = (props: any) => {
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
                <Button onClick={() => handleSelectCostComponentCalc(props.dataItem.costComponentCalcId)}>
                    {props.dataItem.costComponentCalcId}
                </Button>

            </td>
        )
    };
    if (editMode > 0) {
        return <AccountingCostsForm editMode={editMode} cancelEdit={cancelEdit} selectedCostComponent={selectedCostComponent} />
    }
  return (
    <>
        <AccountingMenu selectedMenuItem={'/accountingCosts'}/>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <GlSettingsMenu />
            <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid style={{height: '65vh', flex: 1}}
                                       data={data ? {data: data.data, total: data.total} : {data: [], total: 0}}
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
                                            <Button color={"secondary"} onClick={() => setEditMode(1)}
                                                    variant="outlined">
                                                Create Cost Component Calculation
                                            </Button>
                                        </Grid>

                                    </Grid>
                                </GridToolbar>

                                <Column field="costComponentCalcId" title="Routing Id" cell={CostComponentCalcIdCell} width={200}
                                        locked={true}/>
                                <Column field="description" title="Description" width={350}/>
                                <Column field="fixedCost" title="Fixed Cost" width={150}/>
                                <Column field="variableCost" title="Variable Cost" width={150}/>
                                <Column field="perMilliSecond" title="Per Milli Second" width={150}/>
                                <Column field="currencyUomId" title="Currency" width={250}/>

                            </KendoGrid>
                            {isFetching && <LoadingComponent message='Loading Cost Components...'/>}
                        </div>

                    </Grid>

                </Grid>
        </Paper>
    </>
  )
}

export default AccountingCosts