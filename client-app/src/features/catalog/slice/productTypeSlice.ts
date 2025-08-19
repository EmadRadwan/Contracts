import {createEntityAdapter, createSlice} from "@reduxjs/toolkit";
import {ProductType} from "../../../app/models/product/productType";
import {RootState} from "../../../app/store/configureStore";


const productTypesAdapter = createEntityAdapter<ProductType>({
    selectId: (ProductType) => ProductType.productTypeId,
});


// export const fetchProductTypesAsync = createAsyncThunk<ProductType[]>(
//     'productType/fetchProductTypesAsync',
//     async (_, thunkAPI) => {
//         try {
//             let response = await agent.ProductTypes.list()
//             console.log(response)
//             return await agent.ProductTypes.list();
//         } catch (error: any) {
//             return thunkAPI.rejectWithValue({error: error.data})
//         }
//     }
// )

export const productTypeSlice = createSlice({
    name: 'productType',
    initialState: productTypesAdapter.getInitialState({
        productTypesLoaded: false,
        status: 'idle'
    }),
    reducers: {},
    extraReducers: (builder => {
        // builder.addCase(fetchProductTypesAsync.pending, (state) => {
        //     state.status = 'pendingFetchProductTypes';
        // });
        // builder.addCase(fetchProductTypesAsync.fulfilled, (state, action) => {
        //     productTypesAdapter.setAll(state, action.payload);
        //     state.status = 'idle';
        //     state.productTypesLoaded = true;
        // });
        // builder.addCase(fetchProductTypesAsync.rejected, (state, action) => {
        //     state.status = 'idle';
        // });
    })
})

export const productTypesSelectors = productTypesAdapter.getSelectors((state: RootState) => state.productType);
