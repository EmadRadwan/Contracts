import {createAsyncThunk, createEntityAdapter, createSlice} from "@reduxjs/toolkit";
import agent from "../../../app/api/agent";
import {RootState} from "../../../app/store/configureStore";
import {PartyType} from "../../../app/models/party/partyType";


const partyTypesAdapter = createEntityAdapter<PartyType>({
    selectId: (PartyType) => PartyType.partyTypeId,
});


export const fetchPartyTypesAsync = createAsyncThunk<PartyType[]>(
    'partyType/fetchPartyTypesAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.PartyTypes.list();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)

export const partyTypeSlice = createSlice({
    name: 'partyType',
    initialState: partyTypesAdapter.getInitialState({
        partyTypesLoaded: false,
        status: 'idle'
    }),
    reducers: {},
    extraReducers: (builder => {
        builder.addCase(fetchPartyTypesAsync.pending, (state) => {
            state.status = 'pendingFetchPartyTypes';
        });
        builder.addCase(fetchPartyTypesAsync.fulfilled, (state, action) => {
            partyTypesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.partyTypesLoaded = true;
        });
        builder.addCase(fetchPartyTypesAsync.rejected, (state, action) => {
            state.status = 'idle';
        });
    })
})

export const partyTypesSelectors = partyTypesAdapter.getSelectors((state: RootState) => state.partyType);
