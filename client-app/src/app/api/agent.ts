import {Party} from "../models/party/party";
import {toast} from "react-toastify";
import axios, {AxiosError, AxiosResponse} from "axios";
import {User, UserFormValues} from "../models/party/user";
import {store} from "../store/configureStore";
import {ProductType} from "../models/product/productType";
import {Product, ProductLov, ServiceLov} from "../models/product/product";
import {ProductPriceType} from "../models/product/productPriceType";
import {ProductPrice} from "../models/product/productPrice";
import {ProductCategory} from "../models/product/productCategory";
import {Facility} from "../models/facility/facility";
import {ProductCategoryMember} from "../models/product/productCategoryMember";
import {ProductFacility} from "../models/product/productFacility";
import {PartyContact} from "../models/party/partyContact";
import {ContactMechPurposeType} from "../models/party/contactMechPurposeType";
import {SupplierProduct} from "../models/product/supplierProduct";
import {CustomerRequest} from "../models/order/customerRequest";
import {Quote} from "../models/order/quote";
import {QuoteAdjustmentType} from "../models/order/quoteAdjustmentType";
import {Order} from "../models/order/order";
import {OrderAdjustmentType} from "../models/order/orderAdjustmentType";
import {FacilityType} from "../models/facility/facilityType";
import {FacilityInventory} from "../models/facility/facilityInventory";
import {PaymentMethodType} from "../models/accounting/paymentMethodType";
import {PaginatedResponse} from "../models/pagination";
import {InvoiceType} from "../models/accounting/invoiceType";
import {InvoiceItemType} from "../models/accounting/invoiceItemType";
import {PaymentType} from "../models/accounting/paymentType";
import {Payment} from "../models/accounting/payment";
import {VehicleContent} from "../models/content/vehicleContent";
import {VehicleLov} from "../models/service/vehicle";
import {Invoice} from "../models/accounting/invoice";

axios.defaults.baseURL = import.meta.env.VITE_API_URL;
axios.defaults.withCredentials = true;

const responseBody = (response: AxiosResponse) => response.data;

axios.interceptors.request.use((config) => {
    const token = store.getState().account.user?.token;
    const lang = store.getState().localization.language;
    if (lang) config.headers['Accept-Language'] = `${lang}`
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
});

axios.interceptors.response.use(
    async (response) => {
        const pagination = response.headers["pagination"];
        if (pagination) {
            response.data = new PaginatedResponse(
                response.data,
                JSON.parse(pagination),
            );
            return response;
        }
        return response;
    },
    (error: AxiosError) => {
        const {data, status} = error.response!;
        switch (status) {
            case 400:
                if (data.errors) {
                    const modelStateErrors: string[] = [];
                    for (const key in data.errors) {
                        if (data.errors[key]) {
                            modelStateErrors.push(data.errors[key]);
                        }
                    }
                    throw modelStateErrors.flat();
                }
                toast.error(data.title);
                break;
            case 401:
                toast.error(data.title);
                break;
            case 403:
                toast.error("You are not allowed to do that!");
                break;
            case 500:
                /*router.navigate({
                        pathname: '/server-error',
                        state: {error: data}
                    });*/
                break;
            default:
                break;
        }
        return Promise.reject(error.response);
    },
);

const requests = {
    get: <T>(url: string) => axios.get<T>(url).then(responseBody),
    post: <T>(url: string, body: {}) =>
        axios.post<T>(url, body).then(responseBody),
    put: <T>(url: string, body: {}) => axios.put<T>(url, body).then(responseBody),
    del: <T>(url: string) => axios.delete<T>(url).then(responseBody),
};

const Uoms = {
    listCurrency: () => requests.get("/uoms/currency"),
    listQuantity: () => requests.get("/uoms/quantity"),
};

const PartyTypes = {
    list: () => requests.get("/partyTypes"),
};

const RoleTypes = {
    list: () => requests.get("/roleTypes"),
};

const ProductTypes = {
    list: () => requests.get<ProductType[]>("/productTypes"),
};

const ContactMechPurposeTypes = {
    list: () =>
        requests.get<ContactMechPurposeType[]>("/contactMechPurposeTypes"),
};

const ProductPriceTypes = {
    list: () => requests.get<ProductPriceType[]>("/productPriceTypes"),
};

const QuoteAdjustmentTypes = {
    list: () => requests.get<QuoteAdjustmentType[]>("/quoteAdjustmentTypes"),
};

const FacilityTypes = {
    list: () => requests.get<FacilityType[]>("/facilityTypes"),
};

const OrderAdjustmentTypes = {
    list: () => requests.get<OrderAdjustmentType[]>("/orderAdjustmentTypes"),
};

const PaymentMethodTypes = {
    list: () => requests.get<PaymentMethodType[]>("/paymentMethodTypes"),
};

const InvoiceTypes = {
    list: () => requests.get<InvoiceType[]>("/invoiceTypes"),
};

const PaymentTypes = {
    list: () => requests.get<PaymentType[]>("/paymentTypes"),
};

const InvoiceItemTypes = {
    list: () => requests.get<InvoiceItemType[]>("/invoiceItemTypes"),
};

const ProductCategories = {
    list: () => requests.get<ProductCategory[]>("/productCategories"),
    getProductCategories: (productId: string) =>
        requests.get<ProductCategoryMember[]>(`/productCategories/${productId}`),
    updateProductCategoryMember: (productCategoryMember: any) =>
        requests.put<void>(
            "/productCategories/updateProductCategoryMember",
            productCategoryMember,
        ),
    createProductCategoryMember: (productCategoryMember: any) =>
        requests.post("/productCategories", productCategoryMember),
};

const Products = {
    list: (params: URLSearchParams) =>
        axios.get<Product[]>("/products", {params}).then(responseBody),
    getSalesProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getSalesProductsLov`, {params})
            .then(responseBody),
    getFinishedProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getFinishedProductsLov`, {params})
            .then(responseBody),
    getPhysicalInventoryProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getPhysicalInventoryProductsLov`, {params})
            .then(responseBody),
    getJobQuoteProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getJobQuoteProductsLov`, {params})
            .then(responseBody),
    getPurchaseProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getPurchaseProductsLov`, {params})
            .then(responseBody),
    getInventoryItemProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getInventoryItemProductsLov`, {params})
            .then(responseBody),
    getFacilityProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getFacilityProductsLov`, {params})
            .then(responseBody),
    getAssocsProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getAssocsProductsLov`, {params})
            .then(responseBody),
    getProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getProductsLov`, {params})
            .then(responseBody),
    getFinishedProductsLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/products/getFinishedProductsLov2`, {params})
            .then(responseBody),
    getServiceLov: (params: URLSearchParams) =>
        axios
            .get<ServiceLov[]>(`/products/getServicesLov`, {params})
            .then(responseBody),
    getProduct: (productId: string) =>
        requests.get<Product>(`/products/${productId}`),
    getProductPrices: (productId: string) =>
        requests.get<ProductPrice[]>(`/productPrices/${productId}`),
    getSupplierProducts: (productId: string) =>
        requests.get<SupplierProduct[]>(`/productSuppliers/${productId}`),
    createProduct: (product: any) => requests.post("/products/createProduct", product),
    updateProduct: (product: any) => requests.put(`/products`, product),
    updateProductPrice: (productPrice: any) =>
        requests.put<void>("/productPrices/updateProductPrice", productPrice),
    createProductPrice: (productPrice: any) =>
        requests.post("/productPrices", productPrice),
    updateSupplierProduct: (supplierProduct: any) =>
        requests.put<void>(
            "/productSuppliers/updateProductSupplier",
            supplierProduct,
        ),
    createSupplierProduct: (supplierProduct: any) =>
        requests.post("/productSuppliers", supplierProduct),
};


const WorkEfforts = {
    getRoutingTasksLov: (params: URLSearchParams) =>
        axios
            .get<ProductLov>(`/workEffort/getRoutingTasksLov`, {params})
            .then(responseBody),
    
};

const Services = {
    getVehiclesLov: (params: URLSearchParams) =>
        axios
            .get<VehicleLov>(`/vehicles/getVehiclesLov`, {params})
            .then(responseBody),
};

const Facilities = {
    listFacilityInventoriesByProduct: (params: URLSearchParams) =>
        axios
            .get<FacilityInventory[]>(
                "/facilityInventories/listFacilityInventoriesByProduct",
                {params},
            )
            .then(responseBody),
    listFacilityInventoriesByInventoryItem: (params: URLSearchParams) =>
        axios
            .get<FacilityInventory[]>(
                "/facilityInventories/listFacilityInventoriesByInventoryItem",
                {params},
            )
            .then(responseBody),
    list: () => requests.get<Facility[]>("/facilities"),
    createFacility: (facility: any) => requests.post("/facilities", facility),
    updateFacility: (facility: any) => requests.put(`/facilities`, facility),
    getProductFacilities: (productId: string) =>
        requests.get<ProductFacility[]>(`/productFacilities/${productId}`),
    updateProductFacility: (productFacility: any) =>
        requests.put<void>(
            "/productFacilities/updateProductFacility",
            productFacility,
        ),
    createProductFacility: (productFacility: any) =>
        requests.post("/productFacilities", productFacility),
};

const Parties = {
    list: (params: URLSearchParams) =>
        axios.get<Party[]>("/parties", {params}).then(responseBody),
    getCustomersLov: (params: URLSearchParams) =>
        axios
            .get<Party[]>(`/parties/getCustomersLov`, {params})
            .then(responseBody),
    getSuppliersLov: (params: URLSearchParams) =>
        axios
            .get<Party[]>(`/parties/getSuppliersLov`, {params})
            .then(responseBody),
    getPartiesLov: (params: URLSearchParams) =>
        axios
            .get<Party[]>(`/parties/getPartiesLov`, {params})
            .then(responseBody),
    getAllPartiesLov: (params: URLSearchParams) =>
        axios
            .get<Party[]>(`/parties/getAllPartiesLov`, {params})
            .then(responseBody),
    getPartiesWithEmployeesLov: (params: URLSearchParams) =>
        axios
            .get<Party[]>(`/parties/getPartiesWithEmployeesLov`, {params})
            .then(responseBody),
    createCustomer: (customer: any) =>
        requests.post("/parties/createCustomer", customer),
    updateCustomer: (customer: any) =>
        requests.put(`/parties/updateCustomer`, customer),
    createContractor: (contractor: any) =>
        requests.post("/parties/createContractor", contractor),
    updateContractor: (contractor: any) =>
        requests.post("/parties/updateContractor", contractor),
    createSupplier: (supplier: any) =>
        requests.post("/parties/createSupplier", supplier),
    updateSupplier: (supplier: any) =>
        requests.put(`/parties/updateSupplier`, supplier),
    getPartyContacts: (partyId: string) =>
        requests.get<PartyContact[]>(`/partyContacts/${partyId}`),
    getCustomer: (partyId: string) =>
        requests.get<Party>(`/parties/${partyId}/getCustomer`),
    getSupplier: (partyId: string) =>
        requests.get<Party>(`/parties/${partyId}/getSupplier`),
    getSuppliers: () => requests.get<Party[]>(`/parties/getSuppliers`),
    updatePartyContact: (partyContact: any) =>
        requests.put<void>("/partyContacts/updatePartyContact", partyContact),
    createPartyContact: (partyContact: any) =>
        requests.post("/partyContacts", partyContact),
};

const BillingAccounts = {
    getBillingAccountsLov: (params: URLSearchParams) =>
        axios
            .get<Party[]>(`/billingAccounts/getBillingAccountsLov`, {params})
            .then(responseBody),
};

const Orders = {
    listQuotes: (params: URLSearchParams) =>
        axios.get<Quote[]>("/quotes", {params}).then(responseBody),
    getQuote: (quoteId: string) =>
        requests.get<Quote>(`/quotes/${quoteId}/getQuote`),
    createQuote: (quote: any) => requests.post("/quotes/createQuote", quote),
    updateQuote: (quote: any) => requests.put("/quotes/updateQuote", quote),
    checkAutoAdjustmentsForSalesOrder: (orderItemsAndAdjustments: any) =>
        requests.post(
            "/orders/checkAutoAdjustmentsForSalesOrder",
            orderItemsAndAdjustments,
        ),

    listOrders: (params: URLSearchParams) =>
        axios.get<Order[]>("/orders", {params}).then(responseBody),
    getSalesOrder: (orderId: string) =>
        requests.get<Order>(`/orders/${orderId}/getSalesOrder`),
    getPurchaseOrder: (orderId: string) =>
        requests.get<Order>(`/orders/${orderId}/getPurchaseOrder`),
    createSalesOrder: (order: any) =>
        requests.post("/orders/createSalesOrder", order),
    updateSalesOrder: (order: any) =>
        requests.put("/orders/updateSalesOrder", order),
    approveSalesOrder: (order: any) =>
        requests.put("/orders/approveSalesOrder", order),
    completeSalesOrder: (order: any) =>
        requests.put("/orders/completeSalesOrder", order),
    createPurchaseOrder: (order: any) =>
        requests.post("/orders/createPurchaseOrder", order),
    updatePurchaseOrder: (order: any) =>
        requests.put("/orders/updatePurchaseOrder", order),
    approvePurchaseOrder: (order: any) =>
        requests.put("/orders/approvePurchaseOrder", order),
    completePurchaseOrder: (order: any) =>
        requests.put("/orders/completePurchaseOrder", order),

    listCustomerRequests: (params: URLSearchParams) =>
        axios
            .get<CustomerRequest[]>("/customerRequests", {params})
            .then(responseBody),
    getCustomerRequest: (custRequestId: string) =>
        requests.get<CustomerRequest>(
            `/customerRequests/${custRequestId}/getCustomerRequest`,
        ),
    createCustomerRequest: (customerRequest: any) =>
        requests.post("/customerRequests/createCustomerRequest", customerRequest),
    updateCustomerRequest: (customerRequest: any) =>
        requests.put("/customerRequests/updateCustomerRequest", customerRequest),
};

const Accounting = {
    listInvoices: (params: URLSearchParams) =>
        axios.get<Invoice[]>("/invoices", {params}).then(responseBody),
    getInvoice: (invoiceId: string) =>
        requests.get<Invoice>(`/invoices/${invoiceId}/getInvoice`),
    createSalesInvoice: (invoice: any) =>
        requests.post("/invoices/createSalesInvoice", invoice),
    updateSalesInvoice: (invoice: any) =>
        requests.put("/invoices/updateSalesInvoice", invoice),
    approveSalesInvoice: (invoice: any) =>
        requests.put("/invoices/approveSalesInvoice", invoice),
    completeSalesInvoice: (invoice: any) =>
        requests.put("/invoices/completeSalesInvoice", invoice),

    listPayments: (params: URLSearchParams) =>
        axios.get<Payment[]>("/payments", {params}).then(responseBody),
    listIncomingPayments: (params: URLSearchParams) =>
        axios.get<Payment[]>("/payments/getPaymentsIncoming", {params}).then(responseBody),
    listOutgoingPayments: (params: URLSearchParams) =>
        axios.get<Payment[]>("/payments/getPaymentsOutgoing", {params}).then(responseBody),
    getPayment: (paymentId: string) =>
        requests.get<Payment>(`/payments/${paymentId}/getPayment`),
    createPayment: (payment: any) =>
        requests.post("/payments/createPayment", payment),
    updatePayment: (payment: any) =>
        requests.put("/payments/updatePayment", payment),
    getPaymentApplications: (params: URLSearchParams) => 
        axios.get<Payment[]>("/payments/getPaymentApplicationsLov", {params}).then(responseBody)
};

const Geos = {
    ListCountry: () => requests.get("/GeoCountry"),
};

const Account = {
    current: () => requests.get<User>("/account"),
    login: (user: UserFormValues) => requests.post<User>("/account/login", user),
    register: (user: UserFormValues) =>
        requests.post<User>("/account/register", user),
};

const VehicleContents = {
    uploadFile: (file: Blob, vehicleId: string) => {
        const formData = new FormData();
        formData.append("File", file);
        return axios.post<VehicleContent>(`contents/${vehicleId}`, formData, {
            headers: {"Content-type": "multipart/form-data"},
        });
    },
    deleteFile: (id: string) => requests.del(`/content/${id}`),
};

const agent = {
    Account,
    Parties,
    PartyTypes,
    Geos,
    ProductTypes,
    ProductPriceTypes,
    Products,
    ProductCategories,
    Uoms,
    Facilities,
    FacilityTypes,
    RoleTypes,
    ContactMechPurposeTypes,
    Orders,
    QuoteAdjustmentTypes,
    OrderAdjustmentTypes,
    PaymentMethodTypes,
    InvoiceTypes,
    PaymentTypes,
    InvoiceItemTypes,
    Accounting,
    VehicleContents,
    Services,
    BillingAccounts, WorkEfforts,
};

export default agent;
