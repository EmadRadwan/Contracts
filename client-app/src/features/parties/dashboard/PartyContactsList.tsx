import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import {useAppDispatch, useAppSelector} from "../../../app/store/configureStore";

import {Navigate} from "react-router";
import {Grid, Typography} from "@mui/material";
import Button from "@mui/material/Button";
import {
    fetchPartyContactsAsync,
    getModifiedPartyContacts,
    getSelectedPartyContactIdEntity,
    partyContactSelectors,
    selectPartyContactId
} from "../slice/partyContactSlice";
import useParties from "../../../app/hooks/useParties";
import PartyContactForm from "../form/PartyContactForm";

export default function PartyContactsList() {
    const initialSort: Array<SortDescriptor> = [
        {field: "partyId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };


    const partyContacts = useAppSelector(partyContactSelectors.selectAll);
    const {partyContactsLoaded} = useAppSelector(state => state.partyContact);
    const dispatch = useAppDispatch();
    const [editMode, setEditMode] = useState(0);
    const selectPartyContact = (partyContactId: string) => dispatch(selectPartyContactId(partyContactId))
    const partyContacts2 = useAppSelector(getModifiedPartyContacts)
    const selectedPartyContact = useAppSelector(getSelectedPartyContactIdEntity)
    const {selectedSingleParty} = useParties();


    useEffect(() => {
        if (!partyContactsLoaded) if (selectedSingleParty instanceof Object) {
            dispatch(fetchPartyContactsAsync(selectedSingleParty.partyId));
        }
    }, [partyContactsLoaded, dispatch, selectedSingleParty])


    if (!selectedSingleParty) {
        return <Navigate to="/parties"/>
    }

    function handleSelectPartyContact(partyContactId: string) {
        //console.log('PartyContactId', partyContactId)
        selectPartyContact(partyContactId)
        setEditMode(2);
    }

    function cancelEdit() {
        setEditMode(0);
    }


    const partyContactCell = (props: any) => {

        return (
            <td>
                <Button
                    onClick={() => handleSelectPartyContact(props.dataItem.partyId.concat(props.dataItem.contactMechId
                        , props.dataItem.contactMechPurposeTypeId, props.dataItem.fromDate.toISOString().split('.')[0] + "Z"))}
                >
                    {props.dataItem.contactMechPurposeType}
                </Button>


            </td>
        )
    }

    if (editMode) {
        // @ts-ignore
        return <PartyContactForm partyContact={selectedPartyContact} cancelEdit={cancelEdit} editMode={editMode}/>
    }

    return (

        <Grid container columnSpacing={1}>
            <Grid container alignItems="center">
                <Grid item xs={8}>
                    <Typography sx={{p: 2}} variant='h4'>Contacts for <br/> {selectedSingleParty.description}
                    </Typography>
                </Grid>

                <Grid item xs={3}>
                    <Button color={"secondary"} onClick={() => setEditMode(1)} variant="contained">
                        Create Party Contact
                    </Button>
                </Grid>
            </Grid>

            <Grid container>
                <div className="div-container">
                    <KendoGrid className="main-grid" style={{height: "300px"}}
                               data={orderBy(partyContacts2, sort).slice(page.skip, page.take + page.skip)}
                               sortable={true}
                               sort={sort}
                               onSortChange={(e: GridSortChangeEvent) => {
                                   setSort(e.sort);
                               }}
                               skip={page.skip}
                               take={page.take}
                               total={partyContacts2.length}
                               pageable={true}
                               onPageChange={pageChange}

                    >

                        <Column field="contactMechPurposeType" title="Contact Type" cell={partyContactCell}
                                width={300}/>
                        <Column field="contactNumber" title="Contact Number" width={150}/>
                        <Column field="infoString" title="Email Address" width={150}/>
                        <Column field="fromDate" title="From" width={100} format="{0: dd/MM/yyyy}"/>
                        <Column field="thruDate" title="To" width={100} format="{0: dd/MM/yyyy}"/>

                    </KendoGrid>
                    <Grid item xs={3}>
                        <Button variant="contained">
                            Back
                        </Button>
                    </Grid>
                </div>
            </Grid>


        </Grid>
    )
}