import {createAsyncThunk, createEntityAdapter, createSlice} from "@reduxjs/toolkit";
import agent from "../../../app/api/agent";
import {Geo} from "../../models/common/geo";
import {RootState} from "../../store/configureStore";


const geoAdapter = createEntityAdapter<Geo>({
    selectId: (Geo) => Geo.geoId,
});


export const fetchGeosAsync = createAsyncThunk<Geo[]>(
    'geo/fetchGeoAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.Geos.ListCountry();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)

export const geoSlice = createSlice({
    name: 'geo',
    initialState: geoAdapter.getInitialState({
        geoLoaded: false,
        status: 'idle'
    }),
    reducers: {},
    extraReducers: (builder => {
        builder.addCase(fetchGeosAsync.pending, (state) => {
            state.status = 'pendingFetchPartyTypes';
        });
        builder.addCase(fetchGeosAsync.fulfilled, (state, action) => {
            geoAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.geoLoaded = true;
        });
        builder.addCase(fetchGeosAsync.rejected, (state, action) => {
            state.status = 'idle';
        });
    })
})

export const geoSelectors = geoAdapter.getSelectors((state: RootState) => state.geo);
