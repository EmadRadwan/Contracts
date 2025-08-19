import {configureStore} from "@reduxjs/toolkit";
import {TypedUseSelectorHook, useDispatch, useSelector} from "react-redux";
import {accountSlice} from "../../features/account/accountSlice";
import {productPriceSlice} from "../../features/catalog/slice/productPriceSlice";
import {currencySlice} from "../../features/catalog/slice/currencySlice";
import {productPriceTypeSlice} from "../../features/catalog/slice/productPriceTypeSlice";
import {productTypeSlice} from "../../features/catalog/slice/productTypeSlice";
import {productCategorySlice} from "../../features/catalog/slice/productCategorySlice";
import {partyTypeSlice} from "../../features/parties/slice/partyTypeSlice";
import {partySlice} from "../../features/parties/slice/partySlice";
import {geoSlice} from "../common/slice/geoSlice";
import {facilitySlice} from "../../features/facilities/slice/FacilitySlice";
import {productFacilitySlice} from "../../features/catalog/slice/productFacilitySlice";
import {partyContactSlice} from "../../features/parties/slice/partyContactSlice";
import {singlePartySlice} from "../../features/parties/slice/singlePartySlice";
import {supplierProductSlice} from "../../features/catalog/slice/productSupplierSlice";
import {quantitySlice} from "../../features/catalog/slice/quantitySlice";
import {inventoryItemSlice} from "../../features/facilities/slice/inventoryItemSlice";
import {
    availableProductPromotionsApi,
    internalAccountingOrganizationsApi,
    orderAdjustmentsApi,
    orderAdjustmentTypesApi,
    orderItemsApi,
    organizationGlChartOfAccountsApi,
    orgGlSettingsApi,
    partiesApi,
    paymentMethodTypesApi,
    processOrderItemApi,
    processPurchaseOrderItemApi,
    processQuoteItemApi,
    roleTypesApi,
    salesOrderPromoProductDiscountApi,
    salesOrderTaxAdjustmentsApi,
} from "./apis";
import {setupListeners} from "@reduxjs/toolkit/query";
import {customerTaxStatusApi} from "./apis/customerTaxStatusApi";
import {productPromosApi} from "./apis/productPromosApi";
import {paymentsApi} from "./apis/payment/paymentsApi";
import {ordersApi} from "./apis/ordersApi";
import {productsApi} from "./apis/productsApi";
import {productCategoriesApi} from "./apis/productCategoriesApi";
import {productUiSlice} from "../../features/catalog/slice/productUiSlice";
import {facilitiesApi} from "./apis/facility/facilitiesApi";
import {inventoriesApi} from "./apis/inventory/inventoriesApi";
import {facilityTypesApi} from "./apis/facility/facilityTypesApi";
import {facilityInventoryUiSlice} from "../../features/facilities/slice/facilityInventoryUiSlice";
import {facilityInventorySlice} from "../../features/facilities/slice/facilityInventorySlice";
import {localizationSlice} from "../common/slice/localizationSlice";
import {productStoreUiSlice} from "../../features/catalog/slice/productStoreUiSlice";
import {returnsApi} from "./apis/return/returnsApi";
import {returnUiSlice} from "../../features/orders/slice/returnUiSlice";
import {returnItemsApi} from "./apis/return/returnItemsApi";
import {vehiclesApi} from "./apis/service/vehiclesApi";
import {vehicleContentsApi} from "./apis/content/vehicleContentsApi";
import {productAssociationTypesApi} from "./apis/productAssociationTypesApi";
import {productAssociationsApi} from "./apis/productAssociationsApi";
import {quoteUiSlice} from "../../features/services/slice/quoteUiSlice";
import {quoteItemsApi} from "./apis/quote/quoteItemsApi";
import {quoteAdjustmentsApi} from "./apis/quote/quoteAdjustmentsApi";
import {quoteAdjustmentTypesApi} from "./apis/quote/quoteAdjustmentTypesApi";
import {quotePromoProductDiscountApi} from "./apis/quote/quotePromoProductDiscountApi";
import {quoteTaxAdjustmentsApi} from "./apis/quote/quoteTaxAdjustmentsApi";
import {invoicesApi} from "./apis/invoice/invoicesApi";
import {invoiceItemsApi} from "./apis/invoice/invoiceItemsApi";
import {productTypesApi} from "./apis/productTypesApi";
import {productPricesApi} from "./apis/productPricesApi";
import {productFacilitiesApi} from "./apis/productFacilitiesApi";
import {jobOrderUiSlice} from "../../features/orders/slice/jobOrderUiSlice";
import {jobOrdersApi} from "./apis/jobOrder/jobOrdersApi";
import {jobOrderItemsApi} from "./apis/jobOrder/jobOrderItemsApi";
import {jobOrderAdjustmentsApi} from "./apis/jobOrder/jobOrderAdjustmentsApi";
import {quotesApi} from "./apis/quote/quotesApi";
import {globalGlSettingsApi} from "./apis/accounting/globalGlSettingsApi";
import {accountingUiSlice} from "../../features/accounting/slice/accountingUiSlice";
import {orderPaymentMethodsApi} from "./apis/orderPaymentMethodsApi";
import {billingAccountsApi} from "./apis/accounting/billingAccountsApi";
import {sharedOrderUiSlice} from "../../features/orders/slice/sharedOrderUiSlice";
import {appUiSlice} from "../../app/slice/appUiSlice";
import {orderItemsSlice} from "../../features/orders/slice/orderItemsUiSlice";
import {orderAdjustmentsSlice} from "../../features/orders/slice/orderAdjustmentsUiSlice";
import {orderPaymentsSlice} from "../../features/orders/slice/orderPaymentsUiSlice";
import {ordersSlice} from "../../features/orders/slice/ordersUiSlice";
import {quotesSlice} from "../../features/orders/slice/quotesUiSlice";
import {quoteItemsSlice} from "../../features/orders/slice/quoteItemsUiSlice";
import {quoteAdjustmentsSlice} from "../../features/orders/slice/quoteAdjustmentsUiSlice";
import {fixedAssetsApi} from "./apis/accounting/fixedAssetsApi";
import {agreementsApi} from "./apis/accounting/agreementsApi";
import {orgChartOfAccountsLovApi} from "./apis/accounting/orgChartOfAccountsLovApi";
import {productsWithBOMApi} from "./apis/manufacturing/productsWithBOMApi";
import {bomProductComponentsApi} from "./apis/manufacturing/bomProductComponentsApi";
import {accountingSharedSlice} from "../../features/accounting/slice/accountingSharedUiSlice";
import {customTimePeriodsApi} from "./apis/accounting/customTimePeriodsApi";
import {invoiceItemTypesApi} from "./apis/accounting/invoiceItemTypesApi";
import {invoiceItemsSlice} from "../../features/accounting/invoice/slice/invoiceItemsUiSlice";
import {invoiceSlice} from "../../features/accounting/invoice/slice/invoiceUiSlice";
import {financialAccountsApi} from "./apis/accounting/financialAccountsApi";
import {productStoresApi} from "./apis/productStoresApi";
import {shipmentReceiptsApi} from "./apis/shipment/shipmentReceiptsApi";
import {paymentsSlice} from "../../features/accounting/payment/slice/paymentsUiSlice";
import {glAccountTypeDefaultsApi} from "./apis/accounting/glAccountTypeDefaultsApi";
import {acctTransApi} from "./apis/accounting/acctTransApi";
import { glVarianceReasonsApi } from "./apis/accounting/glVarianceReasonsApi";
import {workEffortsApi} from "./apis/manufacturing/workEffortsApi";
import { fxRatesApi } from "./apis/accounting/fxRatesApi";
import {costsApi} from "./apis/manufacturing/costsApi";
import {manufacturingSharedSlice} from "../../features/manufacturing/slice/manufacturingSharedUiSlice";
import { invoicePaymentApplicationsApi } from "./apis/invoice/invoicePaymentsApplicationsApi";
import { productGlAccountsApi } from "./apis/accounting/productGlAccountsApi";
import { productCategoryGlAccountsApi } from "./apis/accounting/productCategoryGlAccountsApi";
import {taxAuthoritiesApi} from "./apis/accounting/taxAuthoritiesApi";
import { finAccountTypesApi } from "./apis/accounting/finAccountTypesApi";
import { finAccountGlAccountsApi } from "./apis/accounting/finAccountGlAccountsApi";
import { partyGlAccountsApi } from "./apis/accounting/partyGlAccountsApi";
import { paymentMethodTypeGlAccountsApi } from "./apis/accounting/paymentMethodTypeGlAccountsApi";
import { paymentTypesApi } from "./apis/accounting/paymentTypesApi";
import { paymentTypesGlAccountsApi } from "./apis/accounting/paymentTypesGlAccountsApi";
import { creditCardTypeGlAccountsApi } from "./apis/accounting/creditCardTypeGlAccountsApi";
import { creditCardTypesApi } from "./apis/accounting/creditCardTypesApi";
import { taxAuthoritiesGlAccountsApi } from "./apis/accounting/taxAuthorityGlAccountsApi";
import { physicalInventoryApi } from "./apis/facility/physicalInventoryApi";
import { geoApi } from "./apis/geoApi";
import { facilityLocationsApi } from "./apis/facility/facilityLocations/facilityLocationsApi";
import { termTypesApi } from "./apis/termTypesApi";
import { orderTermsUiSlice } from "../../features/orders/slice/orderTermsUiSlice";
import { accountingReportsApi } from "./apis/accounting/accountingReportsApi";
import { finAccountStatusApi } from "./apis/accounting/finAccountStatusApi";
import { paymentGroupsApi } from "./apis/payment/paymentGroupsApi";
import {paymentGroupTypesApi} from "./apis/payment/paymentGroupTypesApi";
import {taxApi} from "./apis/accounting/taxApi";
import {productFeaturesApi} from "./apis/productFeaturesApi";



const composeEnhancers = {
    features: {
        pause: true, // start/pause recording of dispatched actions
        lock: true, // lock/unlock dispatching actions and side effects
        persist: false, // persist states on page reloading
        export: false, // export history of actions in a file

        // fairly certrain jump and skip is broken, if I set both to false then neither show on hover, if I set Jump to true, it still doesnt show either them,
        // if I set skip to true and jump to false, it shows both of them on hover

        jump: false, // jump back and forth (time travelling)
        skip: false, // skip (cancel) actions

        reorder: true, // drag and drop actions in the history list
        dispatch: true, // dispatch custom actions or action creators
        //test: false, // generate tests for the selected actions
        //trace: true, // trace all dispatched actions
    },
};




export const store = configureStore({
    reducer: {
        localization: localizationSlice.reducer,
        party: partySlice.reducer,
        productType: productTypeSlice.reducer,
        partyType: partyTypeSlice.reducer,
        productCategory: productCategorySlice.reducer,
        productPriceType: productPriceTypeSlice.reducer,
        productPrice: productPriceSlice.reducer,
        geo: geoSlice.reducer,
        account: accountSlice.reducer,
        currency: currencySlice.reducer,
        quantity: quantitySlice.reducer,
        facility: facilitySlice.reducer,
        inventoryItem: inventoryItemSlice.reducer,
        productFacility: productFacilitySlice.reducer,
        singleParty: singlePartySlice.reducer,
        supplierProduct: supplierProductSlice.reducer,
        orderItemsUi: orderItemsSlice.reducer,
        quoteItemsUi: quoteItemsSlice.reducer,
        ordersUi: ordersSlice.reducer,
        quotesUi: quotesSlice.reducer,
        orderPaymentsUi: orderPaymentsSlice.reducer,
        orderAdjustmentsUi: orderAdjustmentsSlice.reducer,
        quoteAdjustmentsUi: quoteAdjustmentsSlice.reducer,
        sharedOrderUi: sharedOrderUiSlice.reducer,
        appUi: appUiSlice.reducer,
        accountingSharedUi: accountingSharedSlice.reducer,
        manufacturingSharedUi: manufacturingSharedSlice.reducer,
        jobOrderUi: jobOrderUiSlice.reducer,
        quoteUi: quoteUiSlice.reducer,
        returnUi: returnUiSlice.reducer,
        productUi: productUiSlice.reducer,
        facilityInventoryUi: facilityInventoryUiSlice.reducer,
        facilityInventory: facilityInventorySlice.reducer,
        invoiceUi: invoiceSlice.reducer,
        invoiceItemsUi: invoiceItemsSlice.reducer,
        paymentsUi: paymentsSlice.reducer,
        partyContact: partyContactSlice.reducer,
        accountingUi: accountingUiSlice.reducer,
        productStoreUi: productStoreUiSlice.reducer,
        orderTermsUi: orderTermsUiSlice.reducer,
        [partiesApi.reducerPath]: partiesApi.reducer,
        [vehiclesApi.reducerPath]: vehiclesApi.reducer,
        [quoteItemsApi.reducerPath]: quoteItemsApi.reducer,
        [quoteAdjustmentsApi.reducerPath]: quoteAdjustmentsApi.reducer,
        [vehicleContentsApi.reducerPath]: vehicleContentsApi.reducer,
        [facilitiesApi.reducerPath]: facilitiesApi.reducer,
        [inventoriesApi.reducerPath]: inventoriesApi.reducer,
        [productsApi.reducerPath]: productsApi.reducer,
        [productsWithBOMApi.reducerPath]: productsWithBOMApi.reducer,
        [costsApi.reducerPath]: costsApi.reducer,
        [workEffortsApi.reducerPath]: workEffortsApi.reducer,
        [paymentsApi.reducerPath]: paymentsApi.reducer,
        [productPromosApi.reducerPath]: productPromosApi.reducer,
        [ordersApi.reducerPath]: ordersApi.reducer,
        [jobOrdersApi.reducerPath]: jobOrdersApi.reducer,
        [returnsApi.reducerPath]: returnsApi.reducer,
        [shipmentReceiptsApi.reducerPath]: shipmentReceiptsApi.reducer,
        [customerTaxStatusApi.reducerPath]: customerTaxStatusApi.reducer,
        [orderItemsApi.reducerPath]: orderItemsApi.reducer,
        [jobOrderItemsApi.reducerPath]: jobOrderItemsApi.reducer,
        [returnItemsApi.reducerPath]: returnItemsApi.reducer,
        [orderAdjustmentsApi.reducerPath]: orderAdjustmentsApi.reducer,
        [jobOrderAdjustmentsApi.reducerPath]: jobOrderAdjustmentsApi.reducer,
        [orderAdjustmentTypesApi.reducerPath]: orderAdjustmentTypesApi.reducer,
        [quoteAdjustmentTypesApi.reducerPath]: quoteAdjustmentTypesApi.reducer,
        [roleTypesApi.reducerPath]: roleTypesApi.reducer,
        [facilityTypesApi.reducerPath]: facilityTypesApi.reducer,
        [salesOrderTaxAdjustmentsApi.reducerPath]:
        salesOrderTaxAdjustmentsApi.reducer,
        [quoteTaxAdjustmentsApi.reducerPath]: quoteTaxAdjustmentsApi.reducer,
        [availableProductPromotionsApi.reducerPath]:
        availableProductPromotionsApi.reducer,
        [processOrderItemApi.reducerPath]: processOrderItemApi.reducer,
        [taxApi.reducerPath]: taxApi.reducer,
        [processPurchaseOrderItemApi.reducerPath]: processPurchaseOrderItemApi.reducer,
        [processQuoteItemApi.reducerPath]: processQuoteItemApi.reducer,
        [salesOrderPromoProductDiscountApi.reducerPath]:
        salesOrderPromoProductDiscountApi.reducer,
        [quotePromoProductDiscountApi.reducerPath]:
        quotePromoProductDiscountApi.reducer,
        [paymentMethodTypesApi.reducerPath]: paymentMethodTypesApi.reducer,
        [productCategoriesApi.reducerPath]: productCategoriesApi.reducer,
        [productAssociationTypesApi.reducerPath]:
        productAssociationTypesApi.reducer,
        [productAssociationsApi.reducerPath]: productAssociationsApi.reducer,
        [invoicesApi.reducerPath]: invoicesApi.reducer,
        [acctTransApi.reducerPath]: acctTransApi.reducer,
        [invoiceItemsApi.reducerPath]: invoiceItemsApi.reducer,
        [productTypesApi.reducerPath]: productTypesApi.reducer,
        [productPricesApi.reducerPath]: productPricesApi.reducer,
        [productFacilitiesApi.reducerPath]: productFacilitiesApi.reducer,
        [productFeaturesApi.reducerPath]: productFeaturesApi.reducer,
        [quotesApi.reducerPath]: quotesApi.reducer,
        [globalGlSettingsApi.reducerPath]: globalGlSettingsApi.reducer,
        [orderPaymentMethodsApi.reducerPath]: orderPaymentMethodsApi.reducer,
        [billingAccountsApi.reducerPath]: billingAccountsApi.reducer,
        [bomProductComponentsApi.reducerPath]: bomProductComponentsApi.reducer,
        [workEffortsApi.reducerPath]: workEffortsApi.reducer,
        [fixedAssetsApi.reducerPath]: fixedAssetsApi.reducer,
        [agreementsApi.reducerPath]: agreementsApi.reducer,
        [taxAuthoritiesApi.reducerPath]: taxAuthoritiesApi.reducer,
        [orgChartOfAccountsLovApi.reducerPath]: orgChartOfAccountsLovApi.reducer,
        [organizationGlChartOfAccountsApi.reducerPath]: organizationGlChartOfAccountsApi.reducer,
        [internalAccountingOrganizationsApi.reducerPath]: internalAccountingOrganizationsApi.reducer,
        [orgGlSettingsApi.reducerPath]: orgGlSettingsApi.reducer,
        [customTimePeriodsApi.reducerPath]: customTimePeriodsApi.reducer,
        [invoiceItemTypesApi.reducerPath]: invoiceItemTypesApi.reducer,
        [financialAccountsApi.reducerPath]: financialAccountsApi.reducer,
        [glAccountTypeDefaultsApi.reducerPath]: glAccountTypeDefaultsApi.reducer,
        [productStoresApi.reducerPath]: productStoresApi.reducer,
        [glVarianceReasonsApi.reducerPath]: glVarianceReasonsApi.reducer,
        [fxRatesApi.reducerPath]: fxRatesApi.reducer,
        [invoicePaymentApplicationsApi.reducerPath]: invoicePaymentApplicationsApi.reducer,
        [productGlAccountsApi.reducerPath]: productGlAccountsApi.reducer,
        [productCategoryGlAccountsApi.reducerPath]: productCategoryGlAccountsApi.reducer,
        [finAccountTypesApi.reducerPath]: finAccountTypesApi.reducer,
        [finAccountGlAccountsApi.reducerPath]: finAccountGlAccountsApi.reducer,
        [paymentMethodTypeGlAccountsApi.reducerPath]: paymentMethodTypeGlAccountsApi.reducer,
        [partyGlAccountsApi.reducerPath]: partyGlAccountsApi.reducer,
        [paymentTypesApi.reducerPath]: paymentTypesApi.reducer,
        [paymentTypesGlAccountsApi.reducerPath]: paymentTypesGlAccountsApi.reducer,
        [creditCardTypeGlAccountsApi.reducerPath]: creditCardTypeGlAccountsApi.reducer,
        [creditCardTypesApi.reducerPath]: creditCardTypesApi.reducer,
        [taxAuthoritiesGlAccountsApi.reducerPath]: taxAuthoritiesGlAccountsApi.reducer,
        [physicalInventoryApi.reducerPath]: physicalInventoryApi.reducer,
        [geoApi.reducerPath]: geoApi.reducer,
        [facilityLocationsApi.reducerPath]: facilityLocationsApi.reducer,
        [termTypesApi.reducerPath]: termTypesApi.reducer,
        [accountingReportsApi.reducerPath]: accountingReportsApi.reducer,
        [finAccountStatusApi.reducerPath]: finAccountStatusApi.reducer,
        [paymentGroupsApi.reducerPath]: paymentGroupsApi.reducer,
        [paymentGroupTypesApi.reducerPath]: paymentGroupTypesApi.reducer
    },
    middleware: (getDefaultMiddleware) => {
        return getDefaultMiddleware()
            .concat(partiesApi.middleware)
            .concat(quoteItemsApi.middleware)
            .concat(quoteAdjustmentsApi.middleware)
            .concat(vehiclesApi.middleware)
            .concat(vehicleContentsApi.middleware)
            .concat(facilitiesApi.middleware)
            .concat(facilityTypesApi.middleware)
            .concat(inventoriesApi.middleware)
            .concat(productsApi.middleware)
            .concat(productsWithBOMApi.middleware)
            .concat(costsApi.middleware)
            .concat(workEffortsApi.middleware)
            .concat(paymentsApi.middleware)
            .concat(jobOrdersApi.middleware)
            .concat(ordersApi.middleware)
            .concat(returnsApi.middleware)
            .concat(shipmentReceiptsApi.middleware)
            .concat(orderItemsApi.middleware)
            .concat(jobOrderItemsApi.middleware)
            .concat(returnItemsApi.middleware)
            .concat(orderAdjustmentsApi.middleware)
            .concat(jobOrderAdjustmentsApi.middleware)
            .concat(orderAdjustmentTypesApi.middleware)
            .concat(quoteAdjustmentTypesApi.middleware)
            .concat(roleTypesApi.middleware)
            .concat(salesOrderTaxAdjustmentsApi.middleware)
            .concat(quoteTaxAdjustmentsApi.middleware)
            .concat(processOrderItemApi.middleware)
            .concat(taxApi.middleware)
            .concat(processPurchaseOrderItemApi.middleware)
            .concat(processQuoteItemApi.middleware)
            .concat(salesOrderPromoProductDiscountApi.middleware)
            .concat(quotePromoProductDiscountApi.middleware)
            .concat(customerTaxStatusApi.middleware)
            .concat(availableProductPromotionsApi.middleware)
            .concat(productPromosApi.middleware)
            .concat(productCategoriesApi.middleware)
            .concat(paymentMethodTypesApi.middleware)
            .concat(productAssociationTypesApi.middleware)
            .concat(productAssociationsApi.middleware)
            .concat(invoicesApi.middleware)
            .concat(acctTransApi.middleware)
            .concat(glAccountTypeDefaultsApi.middleware)
            .concat(invoiceItemsApi.middleware)
            .concat(productTypesApi.middleware)
            .concat(productPricesApi.middleware)
            .concat(productFacilitiesApi.middleware)
            .concat(productFeaturesApi.middleware)
            .concat(quotesApi.middleware)
            .concat(globalGlSettingsApi.middleware)
            .concat(orderPaymentMethodsApi.middleware)
            .concat(billingAccountsApi.middleware)
            .concat(bomProductComponentsApi.middleware)
            .concat(fixedAssetsApi.middleware)
            .concat(agreementsApi.middleware)
            .concat(taxAuthoritiesApi.middleware)
            .concat(orgChartOfAccountsLovApi.middleware)
            .concat(organizationGlChartOfAccountsApi.middleware)
            .concat(internalAccountingOrganizationsApi.middleware)
            .concat(orgGlSettingsApi.middleware)
            .concat(customTimePeriodsApi.middleware)
            .concat(invoiceItemTypesApi.middleware)
            .concat(financialAccountsApi.middleware)
            .concat(productStoresApi.middleware)
            .concat(glVarianceReasonsApi.middleware)
            .concat(fxRatesApi.middleware)
            .concat(invoicePaymentApplicationsApi.middleware)
            .concat(productGlAccountsApi.middleware)
            .concat(productCategoryGlAccountsApi.middleware)
            .concat(finAccountTypesApi.middleware)
            .concat(finAccountGlAccountsApi.middleware)
            .concat(partyGlAccountsApi.middleware)
            .concat(paymentMethodTypeGlAccountsApi.middleware)
            .concat(paymentTypesApi.middleware)
            .concat(paymentTypesGlAccountsApi.middleware)
            .concat(creditCardTypeGlAccountsApi.middleware)
            .concat(creditCardTypesApi.middleware)
            .concat(taxAuthoritiesGlAccountsApi.middleware)
            .concat(physicalInventoryApi.middleware)
            .concat(geoApi.middleware)
            .concat(facilityLocationsApi.middleware)
            .concat(termTypesApi.middleware)
            .concat(accountingReportsApi.middleware)
            .concat(finAccountStatusApi.middleware)
            .concat(paymentGroupsApi.middleware)
            .concat(paymentGroupTypesApi.middleware)
    },
    devTools: process.env.NODE_ENV !== 'production' ? {features: composeEnhancers.features} : false,
});
setupListeners(store.dispatch);

export {useFetchProductCategoriesQuery} from "./apis/productCategoriesApi";

export {
    useFetchVehiclesQuery,
    useFetchVehicleMakesQuery,
    useFetchVehicleModelsByMakeIdQuery,
    useFetchVehicleInteriorColorsQuery,
    useFetchVehicleExteriorColorsQuery,
    useFetchVehicleTypesQuery,
    useFetchVehicleTransmissionTypesQuery,
    useCreateVehicleMutation,
    useUpdateVehicleMutation,
    useFetchServiceRatesQuery,
    useFetchServiceSpecificationQuery,
    useCreateServiceRateMutation,
    useUpdateServiceRateMutation,
    useCreateServiceSpecificationMutation,
    useUpdateServiceSpecificationMutation,
    useCreateVehicleMakeMutation,
    useUpdateVehicleMakeMutation,
    useCreateVehicleModelMutation,
    useUpdateVehicleModelMutation,
    useFetchVehicleAnnotationsQuery,
} from "./apis/service/vehiclesApi";

export {useFetchOrderPaymentMethodsQuery} from "./apis/orderPaymentMethodsApi";

export {useFetchAcctTransQuery, useFetchAcctgTransTypesQuery
, useCreateAcctgTransQuickMutation, useFetchGeneralAcctTransEntriesQuery, useCreateAcctgTransEntryMutation
, useUpdateAcctgTransEntryMutation, useDeleteAcctgTransEntryMutation, useCreateAcctgTransMutation
, useUpdateAcctgTransMutation, useFetchAcctTransEntriesQuery, useFetchInvoiceAcctTransEntriesQuery, useFetchPaymentAcctTransEntriesQuery} from "./apis/accounting/acctTransApi";

export {useFetchGlobalGlAccountsQuery} from "./apis/accounting/globalGlSettingsApi";

export {
    useFetchBillingAccountsQuery
} from "./apis/accounting/billingAccountsApi";

export {useFetchFixedAssetsQuery} from "./apis/accounting/fixedAssetsApi";
export {
    useFetchGlAccountTypesQuery,
    useFetchGlAccountTypeDefaultsQuery,
    useFetchGlAccountOrganizationAndClassQuery
} from "./apis/accounting/glAccountTypeDefaultsApi";
export {useFetchAgreementsQuery} from "./apis/accounting/agreementsApi";
export {useFetchTaxAuthoritiesQuery} from "./apis/accounting/taxAuthoritiesApi";
export {useFetchInternalAccountingOrganizationsQuery} from "./apis/accounting/internalAccountingOrganizationsApi";
export {useFetchOrgGlSettingsQuery} from "./apis/accounting/orgGlSettingsApi";
export {useFetchOrgChartOfAccountsLovQuery} from "./apis/accounting/orgChartOfAccountsLovApi";
export {useFetchOrganizationGlChartOfAccountsQuery} from "./apis/accounting/organizationGlChartOfAccountsApi";

export {
    useFetchVehicleContentsQuery,
    useCreateVehicleContentMutation,
} from "./apis/content/vehicleContentsApi";

export {
    useFetchQuotesQuery,
    useCreateQuoteMutation,
    useUpdateQuoteMutation,
    useAddOrderFromQuoteMutation,
} from "./apis/quote/quotesApi";

export {
    useFetchQuoteItemsQuery,
    useFetchQuoteItemProductQuery,
    useAddQuoteItemsMutation,
    useUpdateQuoteItemsMutation,
    useApproveQuoteItemsMutation,
} from "./apis/quote/quoteItemsApi";

export {
    useAddQuoteAdjustmentsMutation,
    useUpdateQuoteAdjustmentsMutation,
    useApproveQuoteAdjustmentsMutation,
    useFetchQuoteAdjustmentsQuery,
    quoteAdjustmentsApi,
} from "./apis/quote/quoteAdjustmentsApi";

export {
    useFetchFacilitiesQuery,
    useFetchRejectionReasonsQuery,
} from "./apis/facility/facilitiesApi";

export {
    useFetchFacilityInventoriesByProductQuery,
    useFetchFacilityInventoriesByInventoryItemQuery,
    useFetchFacilityInventoryItemProductQuery,
    useFetchFacilityInventoriesByInventoryItemDetailsQuery,
    useFetchInventoryTransferQuery,
    useReceiveInventoryProductsMutation, useUpdateInventoryTransferMutation, useAddInventoryTransferMutation
} from "./apis/inventory/inventoriesApi";

export {
    useFetchPartiesQuery,
    useFetchCustomerQuery,
    useFetchSupplierQuery,
    useFetchCompaniesQuery
} from "./apis/partiesApi";


export {
    useFetchPaymentsQuery, useAddSalesOrderPaymentsMutation
    , useFetchOutgoingPaymentTypesQuery,
    useFetchIncomingPaymentTypesQuery,
    useFetchPaymentMethodsQuery, useSetPaymentStatusToReceivedMutation
} from "./apis/payment/paymentsApi";

export {useFetchProductPromosQuery} from "./apis/productPromosApi";
export {useFetchProductsWithBOMQuery} from "./apis/manufacturing/productsWithBOMApi";
export {useFetchCostComponentCalcsQuery, useFetchProductCostComponentCalcsQuery,
    useFetchCostComponentsQuery, useFetchSimulatedBomCostQuery, useCalculateProductCostsMutation} from "./apis/manufacturing/costsApi";
export {useFetchBomProductComponentsApiQuery} from "./apis/manufacturing/bomProductComponentsApi";
export {useFetchRoutingsQuery, useFetchRoutingTasksQuery, useFetchProductRoutingsQuery,
    useCreateProductionRunMutation, useFetchProductionRunsQuery,
    useFetchProductionRunTasksQuery, useFetchProductionRunMaterialsQuery
, useChangeProductionRunStatusMutation, useChangeProductionRunTaskStatusMutation,
    useFetchProductionRunPartyAssignmentsQuery, useIssueProductionRunTaskMutation} from "./apis/manufacturing/workEffortsApi";

export {useFetchCustomerTaxStatusQuery} from "./apis/customerTaxStatusApi";

export {useFetchAvailableProductPromotionsQuery} from "./apis/availableProductPromotionsApi";

export {useFetchQuotePromoProductDiscountQuery} from "./apis/quote/quotePromoProductDiscountApi";

export {useFetchSalesOrderPromoProductDiscountQuery} from "./apis/salesOrderPromoProductDiscountApi";

export * from "./apis/ordersApi";

export {
    useFetchJobOrdersQuery,
    useUpdateOrApproveJobOrderMutation,
    useCompleteJobOrderMutation,
} from "./apis/jobOrder/jobOrdersApi";

export {
    useFetchReturnsQuery,
    useFetchReturnHeaderTypesQuery,
    useFetchReturnReasonsQuery,
    useFetchReturnTypesQuery,
    useAddReturnMutation,
    useUpdateReturnMutation,
    useApproveReturnMutation,
    useCompleteReturnMutation,
} from "./apis/return/returnsApi";

export {
    useFetchProductsQuery,
    useFetchProductUOMsQuery,
    useAddProductMutation,
    useUpdateProductMutation,
    useFetchProductStoreFacilitiesQuery,
    useFetchProductQuantityUomQuery,
} from "./apis/productsApi";

export {
    shipmentReceiptsApi,
} from "./apis/shipment/shipmentReceiptsApi";

export {
    useFetchSalesOrderItemsQuery,
    useFetchPurchaseOrderItemsQuery,
    useFetchOrderItemProductQuery,
    useAddSalesOrderItemsMutation,
    useAddPurchaseOrderItemsMutation,
    useUpdateSalesOrderItemsMutation,
    useUpdatePurchaseOrderItemsMutation,
    useApproveSalesOrderItemsMutation,
    useApprovePurchaseOrderItemsMutation,
} from "./apis/orderItemsApi";

export {useFetchJobOrderItemsQuery} from "./apis/jobOrder/jobOrderItemsApi";

export {useFetchSalesReturnItemsQuery} from "./apis/return/returnItemsApi";

export {
    useFetchOrderAdjustmentsQuery,
    useAddSalesOrderAdjustmentsMutation,
    useUpdateSalesOrderAdjustmentsMutation,
    useApproveSalesOrderAdjustmentsMutation,
} from "./apis/orderAdjustmentsApi";

export {useFetchJobOrderAdjustmentsQuery} from "./apis/jobOrder/jobOrderAdjustmentsApi";

export {useFetchSalesOrderTaxAdjustmentsQuery} from "./apis/salesOrderTaxAdjustmentsApi";

export {useFetchQuoteTaxAdjustmentsQuery} from "./apis/quote/quoteTaxAdjustmentsApi";

export * from "./apis/paymentMethodTypesApi";

export {useFetchOrderAdjustmentTypesQuery} from "./apis/orderAdjustmentTypesApi";

export {useFetchQuoteAdjustmentTypesQuery} from "./apis/quote/quoteAdjustmentTypesApi";

export {useFetchRoleTypesQuery} from "./apis/roleTypesApi";

export {useFetchProductAssociationTypesQuery} from "./apis/productAssociationTypesApi";

export {
    useAddProductAssociationMutation,
    useUpdateProductAssociationMutation,
} from "./apis/productAssociationsApi";

export * from "./apis/invoice/invoicesApi";

export {
    useFetchInvoiceItemsQuery,
    useFetchInvoiceItemProductQuery,
    useUpdateInvoiceItemsMutation,
    useApproveInvoiceItemsMutation,
} from "./apis/invoice/invoiceItemsApi";

export {useFetchProductTypesQuery} from "./apis/productTypesApi";
export {useCreateProductPriceMutation, useGetProductPricesQuery, useUpdateProductPriceMutation} from "./apis/productPricesApi";
export * from "./apis/productFacilitiesApi";

export {useFetchProductFeatureColorsQuery, useFetchProductFeatureTrademarksQuery} from "./apis/productFeaturesApi";

export {useFetchCustomTimePeriodsQuery} from "./apis/accounting/customTimePeriodsApi"

export {useFetchInvoiceItemTypesQuery} from "./apis/accounting/invoiceItemTypesApi"

export {useFetchFinancialAccountsQuery} from "./apis/accounting/financialAccountsApi"

export {useFetchProductStoresQuery} from "./apis/productStoresApi"

export {useFetchVarianceReasonsQuery} from "./apis/accounting/glVarianceReasonsApi"

export * from "./apis/accounting/fxRatesApi"

export * from "./apis/invoice/invoicePaymentsApplicationsApi"

export * from "./apis/accounting/productGlAccountsApi"

export * from "./apis/accounting/productCategoryGlAccountsApi"

export * from "./apis/accounting/finAccountTypesApi"

export * from "./apis/accounting/finAccountGlAccountsApi"

export * from "./apis/accounting/partyGlAccountsApi"

export * from "./apis/accounting/paymentTypesApi"

export * from "./apis/accounting/paymentTypesGlAccountsApi"

export * from "./apis/accounting/creditCardTypeGlAccountsApi"

export * from "./apis/accounting/creditCardTypesApi"

export * from "./apis/accounting/taxAuthorityGlAccountsApi"

export * from "./apis/facility/physicalInventoryApi"

export * from "./apis/geoApi"

export * from "./apis/facility/facilityLocations/facilityLocationsApi"

export * from "./apis/accounting/finAccountStatusApi"

export * from "./apis/payment/paymentGroupsApi"
export * from "./apis/payment/paymentGroupTypesApi"



export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
