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
import {Party} from "../../../app/models/party/party";


interface SinglePartyState extends EntityState<Party> {
    singlePartyLoaded: boolean;
    selectedSinglePartyId: string | undefined;
    status: string;
}


const singlePartyAdapter = createEntityAdapter<Party>({
    selectId: (Party) => Party.partyId,
});


export const fetchCustomerAsync = createAsyncThunk<Party, string>(
    'party/fetchCustomerAsync',
    async (partyId, thunkAPI) => {
        try {
            return await agent.Parties.getCustomer(partyId);
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)

export const fetchSupplierAsync = createAsyncThunk<Party, string>(
    'party/fetchSupplierAsync',
    async (partyId, thunkAPI) => {
        try {
            return await agent.Parties.getSupplier(partyId);
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)


export const initialState: SinglePartyState = singlePartyAdapter.getInitialState({
    singlePartyLoaded: false,
    selectedSinglePartyId: undefined,
    status: 'idle',
})

export const singlePartySlice = createSlice({
    name: 'singleParty',
    initialState: initialState,
    reducers: {
        setSingleParty: (state, action) => {
            singlePartyAdapter.upsertOne(state, action.payload);
        },
        selectSinglePartyById(state, action: PayloadAction<string>) {
            state.selectedSinglePartyId = action.payload
        }
    },
    extraReducers: (builder => {
        builder.addCase(fetchCustomerAsync.pending, (state) => {
            state.status = 'pendingFetchCustomer';
        });
        builder.addCase(fetchCustomerAsync.fulfilled, (state, action) => {
            singlePartyAdapter.upsertOne(state, action.payload);
            state.status = 'idle';
            state.singlePartyLoaded = true;
        });
        builder.addCase(fetchCustomerAsync.rejected, (state, action) => {
            state.status = 'idle';
        });
        builder.addCase(fetchSupplierAsync.pending, (state) => {
            state.status = 'pendingFetchSupplier';
        });
        builder.addCase(fetchSupplierAsync.fulfilled, (state, action) => {
            singlePartyAdapter.upsertOne(state, action.payload);
            state.status = 'idle';
            state.singlePartyLoaded = true;
        });
        builder.addCase(fetchSupplierAsync.rejected, (state, action) => {
            state.status = 'idle';
        });
    })
})

const singlePartySelector = (state: RootState) => state.singleParty
export const {
    selectSinglePartyById,
    setSingleParty,
} = singlePartySlice.actions;

export const singlePartySelectors = singlePartyAdapter.getSelectors((state: RootState) => state.singleParty);
export const {selectEntities} = singlePartySelectors

const getSelectedSinglePartyId = createSelector(
    singlePartySelector,
    (singleParty) => singleParty.selectedSinglePartyId
)

export const getSelectedSinglePartyIdEntity = createSelector(
    selectEntities,
    getSelectedSinglePartyId,
    (entities, id) => id && entities[id]
)
