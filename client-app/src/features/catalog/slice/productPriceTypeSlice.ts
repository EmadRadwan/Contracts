import {createAsyncThunk, createEntityAdapter, createSlice} from "@reduxjs/toolkit";
import agent from "../../../app/api/agent";
import {RootState} from "../../../app/store/configureStore";
import {ProductPriceType} from "../../../app/models/product/productPriceType";


const productPriceTypesAdapter = createEntityAdapter<ProductPriceType>({
    selectId: (ProductPriceType) => ProductPriceType.productPriceTypeId,
});


export const fetchProductPriceTypesAsync = createAsyncThunk<ProductPriceType[]>(
    'productPriceType/fetchProductPriceTypesAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.ProductPriceTypes.list();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)

export const productPriceTypeSlice = createSlice({
    name: 'productPriceType',
    initialState: productPriceTypesAdapter.getInitialState({
        productPriceTypesLoaded: false,
        status: 'idle'
    }),
    reducers: {},
    extraReducers: (builder => {
        builder.addCase(fetchProductPriceTypesAsync.pending, (state) => {
            state.status = 'pendingFetchProductTypes';
        });
        builder.addCase(fetchProductPriceTypesAsync.fulfilled, (state, action) => {
            productPriceTypesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.productPriceTypesLoaded = true;
        });
        builder.addCase(fetchProductPriceTypesAsync.rejected, (state, action) => {
            //console.log(action.payload);
            state.status = 'idle';
        });
    })
})

export const productPriceTypesSelectors = productPriceTypesAdapter.getSelectors((state: RootState) => state.productPriceType);
