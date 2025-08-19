import {createEntityAdapter, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {Facility} from "../../../app/models/facility/facility";

interface FacilityInventoryUiState {
    selectedFacilityId: string | undefined;
    selectedProductId: string | undefined;
    selectedInventoryItemId: string | undefined;
    selectedProductName: string | undefined;
    facilitiesUi: EntityState<Facility>;
    ordersForPickOrMoveStock: any[] | undefined
}


const facilitiesAdapter = createEntityAdapter<Facility>({
    selectId: (facility) => facility.facilityId
})


export const initialState: FacilityInventoryUiState = {
    selectedFacilityId: undefined,
    selectedProductId: undefined,
    selectedInventoryItemId: undefined,
    selectedProductName: undefined,
    facilitiesUi: facilitiesAdapter.getInitialState(),
    ordersForPickOrMoveStock: undefined
}

export const facilityInventoryUiSlice = createSlice({
    name: 'facilityInventoryUi',
    initialState: initialState,
    reducers: {
        resetUiFacilityInventory: (state, action) => {
            state.facilitiesUi = facilitiesAdapter.getInitialState();
            state.selectedFacilityId = undefined;
            //state.selectedOrderItem = undefined;
        },
        setFacilityId(state, action: PayloadAction<string | undefined>) {
            state.selectedFacilityId = action.payload
        },
        selectProductById(state, action: PayloadAction<string | undefined>) {
            state.selectedProductId = action.payload
        },
        setInventoryItemId(state, action: PayloadAction<string | undefined>) {
            state.selectedInventoryItemId = action.payload
        },
        setSelectedProductName(state, action: PayloadAction<string | undefined>) {
            state.selectedProductName = action.payload
        },
        setOrdersForPickOrMoveStock(state, {payload}: PayloadAction<any[] | undefined>) {
            state.ordersForPickOrMoveStock = payload
        }
    },
})

export const {
    resetUiFacilityInventory, setFacilityId,
    selectProductById, setInventoryItemId, setSelectedProductName, setOrdersForPickOrMoveStock
} = facilityInventoryUiSlice.actions;

const facilityInventoryUiSelector = (state: RootState) => state.facilityInventoryUi;








