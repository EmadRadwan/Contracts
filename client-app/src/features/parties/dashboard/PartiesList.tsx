import React, {useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";

import {ExcelExport} from "@progress/kendo-react-excel-export";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {Grid, Paper, Typography} from "@mui/material";
import CreateCustomerForm from "../form/CreateCustomerForm";
import CreateSupplierForm from "../form/CreateSupplierForm";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {useFetchPartiesQuery} from "../../../app/store/apis";
import {Party, PartyParams} from "../../../app/models/party/party";
import {State} from "@progress/kendo-data-query";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import CreateContractorForm from "../form/CreateContractorForm";


export default function PartiesList() {

    const [editMode, setEditMode] = useState(0);
    const [form, setForm] = useState('');
    const [page, setPage] = useState(1)
    const [show, setShow] = useState(false);
    const params = {pageNumber: page, pageSize: 6, orderBy: "orderIdAsc", roleTypes: []}
    const [partyParam, setPartyParam] = useState<PartyParams>(params);
    const [dataState, setDataState] = React.useState<State>({take: 6, skip: 0});
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };
    const { getTranslatedLabel } = useTranslationHelper();

    const [party, setParty] = useState<Party | undefined>(undefined);

    const {data: parties, error, isFetching, isLoading} = useFetchPartiesQuery({...dataState});
    
    function handleSelectParty(partyId: string, formValue: string) {
        const selectedParty: Party | undefined = parties?.data.find((party: any) => party.partyId === partyId);
        console.log('selectedParty', selectedParty)
        setParty(selectedParty)
        setEditMode(2);
    }


    function cancelEdit() {
        setEditMode(0);
        setForm('')
    }


    const PartyDescriptionCell = (props: any) => {
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
                    const startAt = props.dataItem.description.indexOf('(')
                    const endAt = props.dataItem.description.indexOf(')')
                    //console.log('MainRole', props.dataItem.description.substring(startAt + 2, endAt - 1))
                    const formValue = props.dataItem.description.substring(startAt + 2, endAt - 1)
                    setForm(formValue)
                    handleSelectParty(props.dataItem.partyId, formValue)
                }}
            >
                {props.dataItem.description}
            </Button>

            </td>
        )
    }


    // Code for Grid functionality
    const dataToExport = parties ? parties.data : [];

    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            _export.current!.save();
        }
    };


    if (editMode > 0 && form === 'CUSTOMER') {
        return <CreateCustomerForm party={party} cancelEdit={cancelEdit} editMode={editMode}/>
    }
    if (editMode > 0 && form === 'SUPPLIER') {
        return <CreateSupplierForm party={party} cancelEdit={cancelEdit} editMode={editMode}/>
    }
    if (editMode > 0 && form === 'CONTRACTOR') {
        return <CreateContractorForm party={party} cancelEdit={cancelEdit} editMode={editMode}/>
    }


    return (
        <>
            <Paper elevation={5} className={`div-container-withBorderCurved`} style={{marginTop: 15}}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={8}>

                        <Grid container>
                            <div className="div-container">
                                <ExcelExport data={dataToExport}
                                             ref={_export}>
                                    <KendoGrid
                                        style={{height: "75vh", width: "94vw", flex: 1}}
                                        resizable={true}
                                        filterable={true}
                                        sortable={true}
                                        pageable={true}
                                        {...dataState}
                                        data={parties ? parties : {data: [], total: 77}}
                                        onDataStateChange={dataStateChange}
                                    >
                                        <GridToolbar>
                                            <Grid container>
                                                
                                                <Grid item xs={2}>
                                                    <Button color={"secondary"} onClick={() => {
                                                        setEditMode(1);
                                                        setForm('CUSTOMER')
                                                    }}
                                                            variant="outlined">
                                                        {getTranslatedLabel("party.parties.list.createCustomer", "Create Customer")}
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={2}>
                                                    <Button color={"secondary"} onClick={() => {
                                                        setEditMode(1);
                                                        setForm('SUPPLIER')
                                                    }}
                                                            variant="outlined">
                                                        {getTranslatedLabel("party.parties.list.createSupplier", "Create Supplier")}
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={2}>
                                                    <Button color={"secondary"} onClick={() => {
                                                        setEditMode(1);
                                                        setForm('EMPLOYEE')
                                                    }}
                                                            variant="outlined">
                                                        {getTranslatedLabel("party.parties.list.createEmployee", "Create Employee")}
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={2}>
                                                    <Button color={"secondary"} onClick={() => {
                                                        setEditMode(1);
                                                        setForm('CONTRACTOR')
                                                    }}
                                                            variant="outlined">
                                                        {getTranslatedLabel("party.parties.list.createContractor", "Create Contractor")}
                                                    </Button>
                                                </Grid>

                                            </Grid>


                                        </GridToolbar>
                                        <Column
                                            field="description"
                                            title={getTranslatedLabel("party.parties.list.description", "Party")}
                                            cell={PartyDescriptionCell}
                                            width={300}
                                            locked={true}
                                        />
                                        <Column
                                            field="mobileContactNumber"
                                            title={getTranslatedLabel("party.parties.list.contactNumber", "Contact Number")}
                                        />
                                        <Column
                                            field="address1"
                                            title={getTranslatedLabel("party.parties.list.address", "Address")}
                                        />
                                        <Column
                                            field="infoString"
                                            title={getTranslatedLabel("party.parties.list.email", "Email")}
                                        />
                                        <Column
                                            field="partyTypeDescription"
                                            title={getTranslatedLabel("party.parties.list.partyType", "Party Type")}
                                        />
                                        {/* <Column field="partyId" title="Product ID" width={0} /> */}

                                    </KendoGrid>
                                </ExcelExport>
                                {isFetching && <LoadingComponent message={getTranslatedLabel("party.parties.list.loading", "Loading Parties...")} />}
                            </div>

                        </Grid>
                    </Grid>
                </Grid>
            </Paper>
        </>


    )
}
