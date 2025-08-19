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
import {Party, PartyParams} from "../../../app/models/party/party";
import {RoleType} from "../../../app/models/common/roleType";
import {ContactMechPurposeType} from "../../../app/models/party/contactMechPurposeType";
import {MetaData} from "../../../app/models/pagination";

interface PartyState extends EntityState<Party> {
    partiesLoaded: boolean;
    partyLoaded: boolean;
    contactMechPurposeTypesLoaded: boolean;
    suppliersLoaded: boolean;
    roleTypesLoaded: boolean;
    roleTypes: RoleType[];
    contactMechPurposeTypes: ContactMechPurposeType[];
    suppliers: Party[];
    status: string;
    partyParams: PartyParams;
    selectedPartyId: string | undefined;
    metaData: MetaData | null;
}


const partiesAdapter = createEntityAdapter<Party>({
    selectId: (Party) => Party.partyId,
});


/*export const fetchPartiesAsync = createAsyncThunk<Party[], void, { state: RootState }>(
    'party/fetchPartiesAsync',
    async (_, thunkAPI) => {
        const params =  {} //getAxiosParams(thunkAPI.getState().party.partyParams);
        try {
            const response = await agent.Parties.list(params);
            thunkAPI.dispatch(setMetaData(response.metaData));
            return response.items;
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)*/


export const fetchRoleTypesAsync = createAsyncThunk<RoleType[]>(
    'roleType/fetchRoleTypesAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.RoleTypes.list();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)

export const fetchSuppliersAsync = createAsyncThunk<Party[]>(
    'supplier/fetchSuppliersAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.Parties.getSuppliers();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)

export const fetchContactMechPurposeTypesAsync = createAsyncThunk<ContactMechPurposeType[]>(
    'contactMechPurposeType/fetchContactMechPurposeTypesAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.ContactMechPurposeTypes.list();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)

/*
function getAxiosParams(partyParams: PartyParams) {
    const params = new URLSearchParams();
    params.append('pageNumber', partyParams.pageNumber.toString());
    params.append('pageSize', partyParams.pageSize.toString());
    params.append('orderBy', partyParams.orderBy);
    if (partyParams.searchTerm) params.append('searchTerm', partyParams.searchTerm);
    if (partyParams.roleTypes.length > 0) {
        params.append('roleTypes', partyParams.roleTypes.toString());
    }
    return params;
}
*/

function initParams() {
    return {
        pageNumber: 1,
        pageSize: 6,
        orderBy: 'name',
        roleTypes: [],
    }
}

export const initialState: PartyState = partiesAdapter.getInitialState({
    partiesLoaded: false,
    partyLoaded: false,
    roleTypesLoaded: false,
    contactMechPurposeTypesLoaded: false,
    suppliersLoaded: false,
    status: 'idle',
    roleTypes: [],
    contactMechPurposeTypes: [],
    suppliers: [],
    partyParams: initParams(),
    selectedPartyId: undefined,
    metaData: null
})

export const partySlice = createSlice({
    name: 'party',
    initialState: initialState,
    reducers: {
        setPartyParams: (state, action) => {
            state.partiesLoaded = false;
            state.partyParams = {...state.partyParams, ...action.payload, pageNumber: 1};
        },
        setPageNumber: (state, action) => {
            state.partiesLoaded = false;
            state.partyParams = {...state.partyParams, ...action.payload};
        },
        setMetaData: (state, action) => {
            state.metaData = action.payload;
        },
        resetPartyParams: (state) => {
            state.partyParams = initParams();
        },
        setParty: (state, action) => {
            partiesAdapter.upsertOne(state, action.payload);
        },
        removeParty: (state, action) => {
            partiesAdapter.updateOne(state, action.payload[0]);
            state.partiesLoaded = false;
        },
        selectPartyById(state, action: PayloadAction<string>) {
            state.selectedPartyId = action.payload
        }
    },
    extraReducers: (builder => {
        /*builder.addCase(fetchPartiesAsync.pending, (state) => {
            state.status = 'pendingFetchParties';
        });
        builder.addCase(fetchPartiesAsync.fulfilled, (state, action) => {
            partiesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.partiesLoaded = true;
        });
        builder.addCase(fetchPartiesAsync.rejected, (state, action) => {
            state.status = 'idle';
        });*/
        builder.addCase(fetchRoleTypesAsync.pending, (state) => {
            state.status = 'pendingFetchRoleTypes';
        });
        builder.addCase(fetchRoleTypesAsync.fulfilled, (state, action) => {
            state.roleTypes = action.payload;
            state.status = 'idle';
            state.roleTypesLoaded = true;
        });
        builder.addCase(fetchRoleTypesAsync.rejected, (state, action) => {
            state.status = 'idle';
        });
        builder.addCase(fetchContactMechPurposeTypesAsync.pending, (state) => {
            state.status = 'pendingFetchContactMechPurposeTypes';
        });
        builder.addCase(fetchContactMechPurposeTypesAsync.fulfilled, (state, action) => {
            state.contactMechPurposeTypes = action.payload;
            state.status = 'idle';
            state.contactMechPurposeTypesLoaded = true;
        });
        builder.addCase(fetchContactMechPurposeTypesAsync.rejected, (state, action) => {
            state.status = 'idle';
        });
        builder.addCase(fetchSuppliersAsync.pending, (state) => {
            state.status = 'pendingFetchSuppliers';
        });
        builder.addCase(fetchSuppliersAsync.fulfilled, (state, action) => {
            state.suppliers = action.payload;
            state.status = 'idle';
            state.suppliersLoaded = true;
        });
        builder.addCase(fetchSuppliersAsync.rejected, (state, action) => {
            state.status = 'idle';
        });

    })
})

const partySelector = (state: RootState) => state.party
export const {
    selectPartyById,
    resetPartyParams,
    setParty,
    removeParty,
    setPageNumber,
    setPartyParams,
    setMetaData
} = partySlice.actions;
export const partySelectors = partiesAdapter.getSelectors((state: RootState) => state.party);
export const {selectEntities} = partySelectors

const getSelectedPartyId = createSelector(
    partySelector,
    (party) => party.selectedPartyId
)

export const getSelectedPartyIdEntity = createSelector(
    selectEntities,
    getSelectedPartyId,
    (entities, id) => id && entities[id]
)
