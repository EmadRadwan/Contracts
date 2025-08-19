import {createAsyncThunk, createEntityAdapter, createSlice} from "@reduxjs/toolkit";
import agent from "../../../app/api/agent";
import {RootState} from "../../../app/store/configureStore";
import {Quantity} from "../../../app/models/common/quantity";


const quantitiesAdapter = createEntityAdapter<Quantity>({
    selectId: (quantity) => quantity.quantityUomId,
});


export const fetchQuantitiesAsync = createAsyncThunk<Quantity[]>(
    'quantity/fetchQuantitiesAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.Uoms.listQuantity();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)

export const quantitySlice = createSlice({
    name: 'quantity',
    initialState: quantitiesAdapter.getInitialState({
        quantitiesLoaded: false,
        status: 'idle'
    }),
    reducers: {},
    extraReducers: (builder => {
        builder.addCase(fetchQuantitiesAsync.pending, (state) => {
            state.status = 'pendingFetchQuantities';
        });
        builder.addCase(fetchQuantitiesAsync.fulfilled, (state, action) => {
            quantitiesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.quantitiesLoaded = true;
        });
        builder.addCase(fetchQuantitiesAsync.rejected, (state, action) => {
            //console.log(action.payload);
            state.status = 'idle';
        });
    })
})

export const quantitiesSelectors = quantitiesAdapter.getSelectors((state: RootState) => state.quantity);
