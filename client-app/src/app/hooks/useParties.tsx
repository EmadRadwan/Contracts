import {useEffect} from "react";

import {useAppDispatch, useAppSelector} from "../store/configureStore";
import {useSelector} from "react-redux";
import {
    fetchContactMechPurposeTypesAsync,
    fetchRoleTypesAsync,
    fetchSuppliersAsync,
    getSelectedPartyIdEntity,
    partySelectors,
    selectPartyById
} from "../../features/parties/slice/partySlice";
import {getSelectedSinglePartyIdEntity, selectSinglePartyById} from "../../features/parties/slice/singlePartySlice";

export default function useParties() {
    const parties = useAppSelector(partySelectors.selectAll);


    const {
        partiesLoaded,
        suppliersLoaded,
        roleTypesLoaded,
        roleTypes,
        metaData,
        contactMechPurposeTypesLoaded
    } = useAppSelector(state => state.party);
    const dispatch = useAppDispatch();

    const selectParty = (partyId: string) => dispatch(selectPartyById(partyId))
    const selectedParty: any = useSelector(getSelectedPartyIdEntity)

    const selectSingleParty = (partyId: string) => dispatch(selectSinglePartyById(partyId))
    const selectedSingleParty: any = useSelector(getSelectedSinglePartyIdEntity)

    useEffect(() => {
        if (!roleTypesLoaded) dispatch(fetchRoleTypesAsync());
    }, [roleTypesLoaded, dispatch]);

    useEffect(() => {
        if (!contactMechPurposeTypesLoaded) dispatch(fetchContactMechPurposeTypesAsync());
    }, [contactMechPurposeTypesLoaded, dispatch]);

    useEffect(() => {
        if (!suppliersLoaded) dispatch(fetchSuppliersAsync());
    }, [suppliersLoaded, dispatch]);


    /*useEffect(() => {
        if (!partiesLoaded) dispatch(fetchPartiesAsync());
    }, [partiesLoaded, dispatch])*/

    const types = roleTypes.map(type => type.description)

    return {
        parties,
        partiesLoaded,
        roleTypesLoaded,
        selectParty,
        selectSingleParty,
        selectedSingleParty,
        selectedParty,
        types,
        metaData
    }
}