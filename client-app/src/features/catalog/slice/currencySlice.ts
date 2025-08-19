import {createAsyncThunk, createEntityAdapter, createSlice} from "@reduxjs/toolkit";
import agent from "../../../app/api/agent";
import {RootState} from "../../../app/store/configureStore";
import {Currency} from "../../../app/models/common/currency";


const currenciesAdapter = createEntityAdapter<Currency>({
    selectId: (Currency) => Currency.currencyUomId,
});


export const fetchCurrenciesAsync = createAsyncThunk<Currency[]>(
    'currency/fetchCurrenciesAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.Uoms.listCurrency();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)

export const currencySlice = createSlice({
    name: 'currency',
    initialState: currenciesAdapter.getInitialState({
        currenciesLoaded: false,
        status: 'idle'
    }),
    reducers: {},
    extraReducers: (builder => {
        builder.addCase(fetchCurrenciesAsync.pending, (state) => {
            state.status = 'pendingFetchCurrencies';
        });
        builder.addCase(fetchCurrenciesAsync.fulfilled, (state, action) => {
            currenciesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.currenciesLoaded = true;
        });
        builder.addCase(fetchCurrenciesAsync.rejected, (state, action) => {
            //console.log(action.payload);
            state.status = 'idle';
        });
    })
})

export const currenciesSelectors = currenciesAdapter.getSelectors((state: RootState) => state.currency);
