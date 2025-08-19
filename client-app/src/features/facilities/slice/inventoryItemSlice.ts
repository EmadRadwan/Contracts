import {createEntityAdapter, createSelector, createSlice, EntityState} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {InventoryItem, InventoryItemParams} from "../../../app/models/facility/inventoryItem";
import {MetaData} from "../../../app/models/pagination";

interface InventoryItemState extends EntityState<InventoryItem> {
    inventoryItemsLoaded: boolean;
    status: string;
    inventoryItemParams: InventoryItemParams;
    selectedInventoryItemId: string | undefined;
    metaData: MetaData | null;

}


const inventoryItemsAdapter = createEntityAdapter<InventoryItem>({
    selectId: (InventoryItem) => InventoryItem.productId,
});


// export const fetchInventoryItemsAsync = createAsyncThunk<InventoryItem[], void, { state: RootState }>(
//     'inventoryItem/fetchInventoryItemsAsync',
//     async (_, thunkAPI) => {
//         const params = getAxiosParams(thunkAPI.getState().inventoryItem.inventoryItemParams);
//         try {
//             const response = await agent.Facilities.listFacilityInventoriesByInventoryItem(params);
//             thunkAPI.dispatch(setMetaData(response.metaData));
//             return response.items;
//         } catch (error: any) {
//             return thunkAPI.rejectWithValue({error: error.data})
//         }
//     }
// )


function getAxiosParams(inventoryItemParams: InventoryItemParams) {
    const params = new URLSearchParams();
    params.append('pageNumber', inventoryItemParams.pageNumber.toString());
    params.append('pageSize', inventoryItemParams.pageSize.toString());
    params.append('facilityId', inventoryItemParams.facilityId);
    params.append('productId', inventoryItemParams.productId);
    if (inventoryItemParams.searchTerm) params.append('searchTerm', inventoryItemParams.searchTerm);

    return params;
}

function initParams() {
    return {
        pageNumber: 1,
        pageSize: 6,
        facilityId: '',
        productId: '',
    }
}

export const initialState: InventoryItemState = inventoryItemsAdapter.getInitialState({
    inventoryItemsLoaded: false,
    status: 'idle',
    inventoryItemParams: initParams(),
    selectedInventoryItemId: undefined,
    metaData: null,

})

export const inventoryItemSlice = createSlice({
    name: 'inventoryItem',
    initialState: initialState,
    reducers: {
        setInventoryItemParams: (state, action) => {
            state.inventoryItemsLoaded = false;
            state.inventoryItemParams = {...state.inventoryItemParams, ...action.payload, pageNumber: 1};
        },
        // setPageNumber: (state, action) => {
        //     state.inventoryItemsLoaded = false;
        //     state.inventoryItemParams = {...state.inventoryItemParams, ...action.payload};
        // },
        // setMetaData: (state, action) => {
        //     state.metaData = action.payload;
        // },
        // resetInventoryItemParams: (state) => {
        //     state.inventoryItemParams = initParams();
        // },
        // setInventoryItem: (state, action) => {
        //     inventoryItemsAdapter.upsertOne(state, action.payload);
        // },
        // removeInventoryItem: (state, action) => {
        //     inventoryItemsAdapter.updateOne(state, action.payload[0]);
        //     state.inventoryItemsLoaded = false;
        // },
        // selectInventoryItemById(state, action: PayloadAction<string>) {
        //     state.selectedInventoryItemId = action.payload
        // }
    },
    extraReducers: (builder => {


    })
})

const inventoryItemSelector = (state: RootState) => state.inventoryItem
export const {
    // selectInventoryItemById,
    // resetInventoryItemParams,
    // setInventoryItem,
    // removeInventoryItem,
    // setPageNumber,
    setInventoryItemParams,
    // setMetaData
} = inventoryItemSlice.actions;
export const inventoryItemSelectors = inventoryItemsAdapter.getSelectors((state: RootState) => state.inventoryItem);
export const {selectEntities} = inventoryItemSelectors

const getSelectedInventoryItemId = createSelector(
    inventoryItemSelector,
    (inventoryItem) => inventoryItem.selectedInventoryItemId
)

export const getSelectedInventoryItemIdEntity = createSelector(
    selectEntities,
    getSelectedInventoryItemId,
    (entities, id) => id && entities[id]
)

export const getModifiedInventoryItems = createSelector(
    inventoryItemSelectors.selectAll,
    (entities) => {
        if (entities.length > 0) {
            return entities
        } else {
            return entities
        }
    }
)
