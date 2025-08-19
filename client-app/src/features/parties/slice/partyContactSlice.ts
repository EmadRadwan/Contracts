import {
    createAsyncThunk,
    createEntityAdapter,
    createSelector,
    createSlice,
    EntityState,
    PayloadAction
} from "@reduxjs/toolkit";
import agent from "../../../app/api/agent";
import {RootState} from "../../../app/store/configureStore";
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";
import {PartyContact} from "../../../app/models/party/partyContact";

interface PartyContactState extends EntityState<PartyContact> {
    partyContactsLoaded: boolean;
    status: string;
    selectedPartyContactId: string | undefined;
}


const partyContactsAdapter = createEntityAdapter<PartyContact>({
    selectId: (PartyContact) => PartyContact.partyId.concat(PartyContact.contactMechId
        , PartyContact.contactMechPurposeTypeId, PartyContact.fromDate.toString()
    )
});


export const fetchPartyContactsAsync = createAsyncThunk<PartyContact[], string, { state: RootState }>(
    'partyContact/fetchPartyContactsAsync',
    async (partyId, thunkAPI) => {
        try {
            return await agent.Parties.getPartyContacts(partyId);
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)


export const initialState: PartyContactState = partyContactsAdapter.getInitialState({
    partyContactsLoaded: false,
    status: 'idle',
    selectedPartyContactId: undefined
})

export const partyContactSlice = createSlice({
    name: 'partyContact',
    initialState: initialState,
    reducers: {
        addPartyContact: (state, action) => {
            partyContactsAdapter.upsertOne(state, action.payload);
        },
        updatePartyContact: (state, action) => {
            //console.log('state', state.ids[0])
            partyContactsAdapter.upsertOne(state, action.payload);

        },
        removePartyContact: (state, action) => {
            partyContactsAdapter.removeOne(state, action.payload);
            state.partyContactsLoaded = false;
        },
        selectPartyContactId(state, action: PayloadAction<string>) {
            state.selectedPartyContactId = action.payload
        }
    },
    extraReducers: (builder => {
        builder.addCase(fetchPartyContactsAsync.pending, (state) => {
            state.status = 'pendingFetchPartyContacts';
        });
        builder.addCase(fetchPartyContactsAsync.fulfilled, (state, action) => {
            partyContactsAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.partyContactsLoaded = true;
        });
        builder.addCase(fetchPartyContactsAsync.rejected, (state, action) => {
            //console.log(action.payload);
            state.status = 'idle';
        });
    })
})

const partyContactsSelector = (state: RootState) => state.partyContact
export const {
    addPartyContact,
    updatePartyContact,
    removePartyContact,
    selectPartyContactId
} = partyContactSlice.actions;
export const partyContactSelectors = partyContactsAdapter.getSelectors((state: RootState) => state.partyContact);
export const {selectEntities} = partyContactSelectors

const getSelectedPartyContactId = createSelector(
    partyContactsSelector,
    (partyContact) => partyContact.selectedPartyContactId
)

export const getSelectedPartyContactIdEntity = createSelector(
    selectEntities,
    getSelectedPartyContactId,
    (entities, id) => id && handleDatesObject(entities[id])
)


export const getModifiedPartyContacts = createSelector(
    partyContactSelectors.selectAll,
    (entities) => {
        if (entities.length > 0) {
            return handleDatesArray(entities)
        } else {
            return entities
        }
    }
)