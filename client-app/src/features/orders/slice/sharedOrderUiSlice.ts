import {createSlice, PayloadAction} from "@reduxjs/toolkit";
import {Vehicle} from "../../../app/models/service/vehicle";
import {Product} from "../../../app/models/product/product";
import {Order} from "../../../app/models/order/order";

interface SharedOrderUiState {
    selectedCustomerId: string | undefined;
    selectedProductId: string | undefined;
    selectedApprovedPurchaseOrder: Order | undefined;
    selectedApprovedSalesOrder: Order | undefined;
    selectedApprovedPurchaseOrderStatus: string | undefined;
    selectedApprovedSalesOrderStatus: string | undefined;
    selectedSupplierId: string | undefined;
    currentOrderType: string | undefined,
    addTax: boolean | undefined,
    needsTaxRecalculation: boolean | undefined,
    selectProductOrService: Product | undefined;
    selectedVehicleId: string | undefined;
    selectedVehicle: Vehicle | undefined;
    whatWasClicked: string;
}

export const initialState: SharedOrderUiState = {
    selectedCustomerId: undefined,
    selectedProductId: undefined,
    selectedSupplierId: undefined,
    selectedApprovedPurchaseOrder: undefined,
    selectedApprovedPurchaseOrderStatus: undefined,
    selectedApprovedSalesOrder: undefined,
    selectedApprovedSalesOrderStatus: undefined,
    currentOrderType: undefined,
    addTax: false,
    needsTaxRecalculation: false,
    selectProductOrService: undefined,
    selectedVehicleId: undefined,
    selectedVehicle: undefined,
    whatWasClicked: "",
};


export const sharedOrderUiSlice = createSlice({
    name: "sharedOrderUi",
    initialState: initialState,
    reducers: {
        resetSharedOrderUi: (state) => {
            state.selectedCustomerId = undefined;
            state.selectedProductId = undefined;
            state.selectedSupplierId = undefined;
            state.selectedApprovedPurchaseOrder = undefined;
            state.selectedApprovedPurchaseOrderStatus = undefined;
        },
        setProductId(state, action: PayloadAction<string | undefined>) {
            state.selectedProductId = action.payload
        },
        setCustomerId(state, action: PayloadAction<string | undefined>) {
            state.selectedCustomerId = action.payload
        },
        setSupplierId(state, action: PayloadAction<string | undefined>) {

            state.selectedSupplierId = action.payload
        },
        setSelectedApprovedPurchaseOrder(state, action: PayloadAction<Order | undefined>) {
            state.selectedApprovedPurchaseOrder = action.payload
        },
        setSelectedApprovedSalesOrder(state, action: PayloadAction<Order | undefined>) {
            state.selectedApprovedSalesOrder = action.payload
        },
        setSelectedApprovedSalesOrderStatus(state, action: PayloadAction<string | undefined>) {
            state.selectedApprovedSalesOrderStatus = action.payload
        },
        setSelectedApprovedPurchaseOrderStatus(state, action: PayloadAction<string | undefined>) {
            state.selectedApprovedPurchaseOrderStatus = action.payload
        },
        setCurrentOrderType(state, action: PayloadAction<string | undefined>) {
            state.currentOrderType = action.payload
        },
        setVehicleId(state, action: PayloadAction<string | undefined>) {
            state.selectedVehicleId = action.payload;
        },
        setSelectedVehicle(state, action: PayloadAction<Vehicle | undefined>) {
            state.selectedVehicle = action.payload;
        },
        setSelectProductOrService(
            state, action: PayloadAction<Product | undefined>,
        ) {
            state.selectProductOrService = action.payload;
        },
        setWhatWasClicked(state, action: PayloadAction<string>) {
            state.whatWasClicked = action.payload;
        },
        setAddTax: (state, action) => {
            state.addTax = action.payload;
            if (action.payload) {
                state.needsTaxRecalculation = true; // Trigger recalc when enabling tax
            } else {
                state.needsTaxRecalculation = false; // No recalc needed when disabling
            }
        },
        setNeedsTaxRecalculation: (state, action) => {
            state.needsTaxRecalculation = action.payload;
        },
    },
});

export const {
    setCustomerId, setWhatWasClicked,
    setProductId,
    resetSharedOrderUi,
    setSupplierId,
    setSelectedApprovedPurchaseOrder,
    setSelectedApprovedPurchaseOrderStatus, setAddTax, setNeedsTaxRecalculation,
    setCurrentOrderType, setVehicleId, setSelectedVehicle, setSelectProductOrService, setSelectedApprovedSalesOrder, setSelectedApprovedSalesOrderStatus
} = sharedOrderUiSlice.actions;