import {
    createAsyncThunk,
    createEntityAdapter,
    createSelector,
    createSlice,
    EntityState,
    PayloadAction
} from "@reduxjs/toolkit";
import agent from "../../../app/api/agent";
import {ProductPrice} from "../../../app/models/product/productPrice";
import {RootState} from "../../../app/store/configureStore";
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";

interface ProductPriceState extends EntityState<ProductPrice> {
    productPricesLoaded: boolean;
    status: string;
    selectedProductPriceId: string | undefined;
}


const productPricesAdapter = createEntityAdapter<ProductPrice>({
    selectId: (ProductPrice) => ProductPrice.productId.concat(ProductPrice.productPriceTypeId
        , ProductPrice.currencyUomId, ProductPrice.fromDate.toString()
    )
});


export const fetchProductPricesAsync = createAsyncThunk<ProductPrice[], string, { state: RootState }>(
    'productPrice/fetchProductPricesAsync',
    async (productId, thunkAPI) => {
        try {
            return await agent.Products.getProductPrices(productId);
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)


export const initialState: ProductPriceState = productPricesAdapter.getInitialState({
    productPricesLoaded: false,
    status: 'idle',
    selectedProductPriceId: undefined
})

export const productPriceSlice = createSlice({
    name: 'productPrice',
    initialState: initialState,
    reducers: {
        addProductPrice: (state, action) => {
            productPricesAdapter.upsertOne(state, action.payload);
        },
        updateProductPrice: (state, action) => {
            //console.log('state', state.ids[0])
            productPricesAdapter.upsertOne(state, action.payload);

        },
        removeProductPrice: (state, action) => {
            productPricesAdapter.removeOne(state, action.payload);
            state.productPricesLoaded = false;
        },
        selectProductPriceId(state, action: PayloadAction<string>) {
            state.selectedProductPriceId = action.payload
        },
        resetProductPriceSlice: (state, action) => {
            productPricesAdapter.removeAll(state)
            state.productPricesLoaded = false;
        },

    },
    extraReducers: (builder => {
        builder.addCase(fetchProductPricesAsync.pending, (state) => {
            state.status = 'pendingFetchProductPrices';
        });
        builder.addCase(fetchProductPricesAsync.fulfilled, (state, action) => {
            productPricesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.productPricesLoaded = true;
        });
        builder.addCase(fetchProductPricesAsync.rejected, (state, action) => {
            //console.log(action.payload);
            state.status = 'idle';
        });
    })
})

const productPriceSelector = (state: RootState) => state.productPrice
export const {
    addProductPrice,
    updateProductPrice,
    removeProductPrice,
    selectProductPriceId, resetProductPriceSlice
} = productPriceSlice.actions;
export const productPriceSelectors = productPricesAdapter.getSelectors((state: RootState) => state.productPrice);
export const {selectEntities} = productPriceSelectors

const getSelectedProductPriceId = createSelector(
    productPriceSelector,
    (productPrice) => productPrice.selectedProductPriceId
)

export const getSelectedProductPriceIdEntity = createSelector(
    selectEntities,
    getSelectedProductPriceId,
    (entities, id) => id && handleDatesObject(entities[id])
)


export const getModifiedProductPrices = createSelector(
    productPriceSelectors.selectAll,
    (entities) => {
        if (entities.length > 0) {
            return handleDatesArray(entities)
        } else {
            return entities
        }
    }
)