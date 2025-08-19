import {
    createAsyncThunk,
    createEntityAdapter,
    createSelector,
    createSlice,
    EntityState,
    PayloadAction
} from "@reduxjs/toolkit";
import agent from "../../../app/api/agent";
import {SupplierProduct} from "../../../app/models/product/supplierProduct";
import {RootState} from "../../../app/store/configureStore";
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";

interface SupplierProductState extends EntityState<SupplierProduct> {
    supplierProductsLoaded: boolean;
    status: string;
    selectedSupplierProductId: string | undefined;
}


const supplierProductsAdapter = createEntityAdapter<SupplierProduct>({
    selectId: (SupplierProduct) => SupplierProduct.productId.concat(SupplierProduct.partyId,
        SupplierProduct.availableFromDate.toString(), SupplierProduct.minimumOrderQuantity.toString()
        , SupplierProduct.currencyUomId
    )
});


export const fetchSupplierProductsAsync = createAsyncThunk<SupplierProduct[], string, { state: RootState }>(
    'SupplierProduct/fetchSupplierProductsAsync',
    async (productId, thunkAPI) => {
        try {
            return await agent.Products.getSupplierProducts(productId);
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)


export const initialState: SupplierProductState = supplierProductsAdapter.getInitialState({
    supplierProductsLoaded: false,
    status: 'idle',
    selectedSupplierProductId: undefined
})

export const supplierProductSlice = createSlice({
    name: 'supplierProduct',
    initialState: initialState,
    reducers: {
        addSupplierProduct: (state, action) => {
            supplierProductsAdapter.upsertOne(state, action.payload);
        },
        updateSupplierProduct: (state, action) => {
            //console.log('state', state.ids[0])
            supplierProductsAdapter.upsertOne(state, action.payload);

        },
        removeSupplierProduct: (state, action) => {
            supplierProductsAdapter.removeOne(state, action.payload);
            state.supplierProductsLoaded = false;
        },
        selectSupplierProductId(state, action: PayloadAction<string>) {
            state.selectedSupplierProductId = action.payload
        }
    },
    extraReducers: (builder => {
        builder.addCase(fetchSupplierProductsAsync.pending, (state) => {
            state.status = 'pendingFetchSupplierProducts';
        });
        builder.addCase(fetchSupplierProductsAsync.fulfilled, (state, action) => {
            supplierProductsAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.supplierProductsLoaded = true;
        });
        builder.addCase(fetchSupplierProductsAsync.rejected, (state, action) => {
            //console.log(action.payload);
            state.status = 'idle';
        });
    })
})

const supplierProductSelector = (state: RootState) => state.supplierProduct
export const {
    addSupplierProduct,
    updateSupplierProduct,
    removeSupplierProduct,
    selectSupplierProductId
} = supplierProductSlice.actions;
export const supplierProductSelectors = supplierProductsAdapter.getSelectors((state: RootState) => state.supplierProduct);
export const {selectEntities} = supplierProductSelectors

const getSelectedSupplierProductId = createSelector(
    supplierProductSelector,
    (supplierProduct) => supplierProduct.selectedSupplierProductId
)

export const getSelectedSupplierProductIdEntity = createSelector(
    selectEntities,
    getSelectedSupplierProductId,
    (entities, id) => id && handleDatesObject(entities[id])
)


export const getModifiedSupplierProducts = createSelector(
    supplierProductSelectors.selectAll,
    (entities) => {
        if (entities.length > 0) {
            return handleDatesArray(entities)
        } else {
            return entities
        }
    }
)