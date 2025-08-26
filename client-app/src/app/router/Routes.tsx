import App from "../layout/App";
import ServerError from "../../features/errors/ServerError";
import {Navigate, Outlet} from "react-router";
import NotFound from "../../features/errors/NotFound";
import {createBrowserRouter, RouteObject, useParams} from "react-router-dom";
import RequireAuth from "./RequireAuth";
import FacilitiesList from "../../features/facilities/dashboard/FacilitiesList";
import PartiesList from "../../features/parties/dashboard/PartiesList";
import OrderDashboard from "../../features/orders/dashboard/OrderDashboard";
import AccountingDashboard from "../../features/accounting/AccountingDashboard";
import OrdersList from "../../features/orders/dashboard/order/OrdersList";
import InvoicesList from "../../features/accounting/invoice/dashboard/InvoicesList";
import PaymentsList from "../../features/accounting/payment/dashboard/PaymentsList";
import FacilityDashboard from "../../features/facilities/dashboard/FacilityDashboard";
import InventoryItemsList from "../../features/facilities/dashboard/InventoryItemsList";
import ProductPricesList from "../../features/catalog/dashboard/productPrice/ProductPricesList";
import PartyContactsList from "../../features/parties/dashboard/PartyContactsList";
import ProductCategoriesList from "../../features/catalog/dashboard/productCategory/ProductCategoriesList";
import ProductSuppliersList from "../../features/catalog/dashboard/productSupplier/ProductSuppliersList";
import ProductFacilitiesList from "../../features/catalog/dashboard/productFacility/ProductFacilitiesList";
import LoginForm from "../../features/account/LoginForm";
import ProductsList from "../../features/catalog/dashboard/product/ProductsList";
import PromosList from "../../features/catalog/dashboard/productPromo/PromosList";
import StoresList from "../../features/catalog/dashboard/productStore/StoresList";
import InventoryItemDetailsList from "../../features/facilities/dashboard/InventoryItemDetailsList";
import ReturnsList from "../../features/orders/dashboard/return/ReturnsList";
import ReceiveInventoryList from "../../features/facilities/dashboard/ReceiveInventoryList";
import ServiceDashboard from "../../features/services/dashboard/ServiceDashboard";
import ProductAssociationsList from "../../features/catalog/dashboard/productAssociation/ProductAssociationList";
import FixedAssetsList from "../../features/accounting/fixedAssets/dashboard/FixedAssetsList";
import TaxAuthoritiesList from "../../features/accounting/taxAuthorities/dashboard/TaxAuthoritiesList";
import FinancialAccountsList from "../../features/accounting/financialAccount/dashboard/FinancialAccountsList";
import BillingAccountsList from "../../features/accounting/billingAccounts/dashboard/BillingAccountList";
import OrganizationGlSettingsList
    from "../../features/accounting/organizationGlSettings/dashboard/OrganizationGlSettingsList";
import GlobalGlSettingsList from "../../features/accounting/globalGlSetting/GlobalGlSettingsList";
import ChartOfAccountsList
    from "../../features/accounting/globalGlSetting/chartOfAccounts/dashboard/ChartOfAccountsList";
import PaymentMethodTypeList
    from "../../features/accounting/globalGlSetting/paymentMethodType/dashboard/PaymentMethodTypeList";
import QuotesList from "../../features/orders/dashboard/quote/QuotesList";
//import JobQuotesList from "../../features/services/dashboard/JobQuotesList";
import ManufacturingDashboard from "../../features/manufacturing/dashboard/ManufacturingDashboard";
import BOMProductComponentsList from "../../features/manufacturing/dashboard/BOMProductComponentsList";
import AgreementsList from "../../features/accounting/agreements/dashboard/AgreementsList";
import ChartOfAccountAssignForm from "../../features/accounting/organizationGlSettings/form/ChartOfAccountAssignForm";
import CustomTimePeriods from "../../features/accounting/globalGlSetting/customTimePeriods/dashboard/CustomTimePeriods";
import InvoiceItemTypeList
    from "../../features/accounting/globalGlSetting/invoiceItemType/dashboard/InvoiceItemTypeList";
import GlAccountTypeDefaults from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/GlAccountTypeDefaults";
import InventoryTransferList from "../../features/facilities/dashboard/InventoryTransferList";
import OrgAccountingSummary from "../../features/accounting/organizationGlSettings/dashboard/OrgAccountingSummary";
import AccountingTransactionsList
    from "../../features/accounting/organizationGlSettings/dashboard/AccountingTransactionsList";
import AccountingTransactionEntriesList
    from "../../features/accounting/organizationGlSettings/dashboard/AccountingTransactionEntriesList";
import EditAcctgTrans from "../../features/accounting/transaction/form/EditAcctgTrans";
import BOMProductsList from "../../features/manufacturing/dashboard/BOMProductsList";
import GlVarianceReason from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/GlVarianceReason";
import RoutingsList from "../../features/manufacturing/dashboard/RoutingsList";
import RoutingTasksList from "../../features/manufacturing/dashboard/RoutingTasksList";
import CostComponentCalcsList from "../../features/manufacturing/dashboard/CostComponentCalcsList";
import ProductCostsList from "../../features/catalog/dashboard/productCost/ProductCostsList";
import ForeignExchangeRatesList from "../../features/accounting/globalGlSetting/fxRates/dashboard/ForeignExchangeRatesList";
import SalesInvoiceAccountList from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/SalesInvoiceAccountList";
import PurchaseInvoiceAccountList from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/PurchaseInvoiceAccountList";
import StandardCostsList from "../../features/accounting/fixedAssets/dashboard/StandardCostsList";
import ProductionRunsList from "../../features/manufacturing/dashboard/ProductionRunsList";
import ProductGlDefaults from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/ProductGlAccounts";
import ProductCategoryGlAccounts from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/ProductCategoryGlAccounts";
import FinAccountGlAccounts from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/FinAccountGlAccounts";
import PartyGlAccounts from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/PartyGlAccounts";
import PaymentMethodTypeGlAccounts from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/PaymentMethodTypeGlAccounts";
import PaymentTypeGlAccounts from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/PaymentTypeGlAccounts";
import CreditCardTypesGlAccounts from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/CreditCardTypesGlAccounts";
import TaxAuthorityGlAccounts from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/TaxAuthorityGlAccounts";
import FixedAssetTypeGlMappings from "../../features/accounting/organizationGlSettings/dashboard/glAccountDefaults/FixedAssetTypeGlMappings";
import TrialBalance from "../../features/accounting/organizationGlSettings/dashboard/TrialBalance";
import AccountingReportsDashboard from "../../features/accounting/organizationGlSettings/dashboard/AccountingReportsDashboard";
import TransactionTotals from "../../features/accounting/organizationGlSettings/dashboard/TransactionTotals";
import IncomeStatement from "../../features/accounting/organizationGlSettings/dashboard/IncomeStatement";
import CashFlowStatement from "../../features/accounting/organizationGlSettings/dashboard/CashFlowStatement";
import BalanceSheet from "../../features/accounting/organizationGlSettings/dashboard/BalanceSheet";
import ComparativeIncomeStatement from "../../features/accounting/organizationGlSettings/dashboard/ComparativeIncomeStatement";
import ComaparativeCashFlowStatement from "../../features/accounting/organizationGlSettings/dashboard/ComaparativeCashFlowStatement";
import ComparativeBalanceSheet from "../../features/accounting/organizationGlSettings/dashboard/ComparativeBalanceSheet";
import GlAccountTrialBalance from "../../features/accounting/organizationGlSettings/dashboard/GlAccountTrialBalance";
import CostCenters from "../../features/accounting/organizationGlSettings/dashboard/CostCenters";
import PhysicalInventoryList from "../../features/facilities/dashboard/PhysicalInventoryList";
import FacilityLocationsList from "../../features/facilities/dashboard/FacilityLocationsList";
import AccountingCosts from "../../features/accounting/globalGlSetting/costs/dashboard/AccountingCosts";
import AgreementItemsList from "../../features/accounting/agreements/dashboard/AgreementItemsList";
import AgreementTermsList from "../../features/accounting/agreements/dashboard/AgreementTermsList";
import InventoryValuation from "../../features/accounting/organizationGlSettings/dashboard/InventoryValuation";
import OrgSetupTimePeriodList from "../../features/accounting/organizationGlSettings/dashboard/OrgSetupTimePeriodList";
import FacilityPickingList from "../../features/facilities/dashboard/FacilityPickingList";
import StockMovesList from "../../features/facilities/dashboard/StockMovesList";
import ManagePicklists from "../../features/facilities/dashboard/ManagePicklists";
import BillingAccountInvoicesList from "../../features/accounting/billingAccounts/dashboard/BillingAccountInvoicesList";
import BillingAccountPayments from "../../features/accounting/billingAccounts/dashboard/BillingAccountPayments";
import BillingAccountOrders from "../../features/accounting/billingAccounts/dashboard/BillingAccountOrders";
import FinancialAccountTransactions from "../../features/accounting/financialAccount/dashboard/FinancialAccountTransactions";
import FinancialAccountDepositWithdrawal from "../../features/accounting/financialAccount/dashboard/FinancialAccountDepositWithdrawal";
import BillingAccountsLayout from "../../features/accounting/billingAccounts/dashboard/BillingAccountsLayout";
import FinancialAccountLayout from "../../features/accounting/financialAccount/dashboard/FinancialAccountLayout";
import WorkEffortsWithReservationsList from "../../features/manufacturing/dashboard/WorkEffortsWithReservationsList";
import QuickCreateAcctgTransForm from "../../features/accounting/transaction/form/QuickCreateAcctgTransform";
import CreateAcctgTransForm from "../../features/accounting/transaction/form/CreateAcctgTrans";
import PackingList from "../../features/facilities/dashboard/PackingList";
import PaymentGroupsList from "../../features/accounting/paymentGroups/dashboard/PaymentGroupsList";
import PaymentGroupLayout from "../../features/accounting/paymentGroups/dashboard/PaymentGroupLayout";
import PaymentGroupPaymentsList from "../../features/accounting/paymentGroups/dashboard/PaymentGroupPaymentsList";
import PartyFinancialHistory from "../../features/parties/dashboard/PartyFinancialHistory";
import EditInvoice from "../../features/accounting/invoice/form/EditInvoice";
import EditReturn from "../../features/orders/form/return/EditReturn";
import OrderReturnItems from "../../features/orders/dashboard/return/OrderReturnItems";
import InvoiceDisplayForm from "../../features/accounting/invoice/form/InvoiceDisplayForm";
import NewInvoice from "../../features/accounting/invoice/form/NewInvoice";
import EditRouting from "../../features/manufacturing/form/EditRouting";
import ListRoutingTaskAssoc from "../../features/manufacturing/dashboard/ListRoutingTaskAssoc";
import ListRoutingProductLink from "../../features/manufacturing/dashboard/ListRoutingProductLink";
import ListRoutingTaskCosts from "../../features/manufacturing/dashboard/ListRoutingTaskCosts";
import EditRoutingTask from "../../features/manufacturing/form/EditRoutingTask";
import ProjectsDashboard from "../../features/Projects/dashboard/ProjectsDashboard";
import ProjectCertificatesList from "../../features/Projects/dashboard/ProjectCertificatesList";

// Wrapper component to extract partyId from URL
const PartyFinancialHistoryWrapper = () => {
    const { partyId } = useParams<{ partyId: string }>();
    return <PartyFinancialHistory partyId={partyId!} />;
};

const InvoiceWrapper = () => {
    const { invoiceId } = useParams<{ invoiceId: string }>();

    // Pass invoiceId to child components via context or props if needed
    return <Outlet context={{ invoiceId }} />;
};

export default InvoiceWrapper;

// Add RoutingWrapper to extract workEffortId for routing-related routes
// This simplifies passing workEffortId to EditRouting, EditRoutingTaskAssoc, and EditRoutingProductLink
const RoutingWrapper = () => {
    const { workEffortId } = useParams<{ workEffortId: string }>();
    return <Outlet context={{ workEffortId }} />;
};

export const routes: RouteObject[] = [
    {
        path: "/",
        element: <App/>,

        children: [
            {path: "login", element: <LoginForm/>},
            {
                element: <RequireAuth/>,
                children: [
                    {path: "facilities", element: <FacilitiesList/>},
                    {path: "parties", element: <PartiesList/>},
                    {path: "manufacturingDashboard", element: <ManufacturingDashboard/>},
                    {path: "ordersDashboard", element: <OrderDashboard/>},
                    {path: "facilitiesDashboard", element: <FacilityDashboard/>},
                    {path: "servicesDashboard", element: <ServiceDashboard/>},
                    {path: "invoicesDashboard", element: <AccountingDashboard/>},
                    {path: "orders", element: <OrdersList/>},
                    {path: "returns", element: <ReturnsList/>},
                    {path: "returns/:returnId", element: <EditReturn/>},
                    {path: "returns/:returnId/items", element: <OrderReturnItems/>},
                    {path: "quotes", element: <QuotesList/>},
                    {path: "promos", element: <PromosList/>},
                    {path: "stores", element: <StoresList/>},
                    { path: "projects", element: <ProjectsDashboard /> },
                    { path: "projectCertificates", element: <ProjectCertificatesList /> },
                    {path: "payments", element: <PaymentsList/>},
                    {
                        path: "invoices",
                        children: [
                            { index: true, element: <InvoicesList /> },
                            { path: "new", element: <NewInvoice /> },
                            {
                                path: ":invoiceId",
                                element: <InvoiceWrapper />,
                                children: [
                                    { index: true, element: <InvoiceDisplayForm mode="view" /> }, // Explicitly set mode="view"
                                    { path: "edit", element: <EditInvoice /> },
                                    { path: "items", element: <InvoiceDisplayForm mode="items" /> },
                                ],
                            },
                        ],
                    },
                    {path: "paymentGroups", element: <PaymentGroupLayout/>, children: [
                        {path: "/paymentGroups", element: <PaymentGroupsList/>},
                        {path: "/paymentGroups/overview", element: <PaymentGroupsList/>},
                        {path: "/paymentGroups/payments", element: <PaymentGroupPaymentsList />},
                    ]},
                    {path: "facilityInventories", element: <FacilityDashboard/>},
                    {path: "receiveInventory", element: <ReceiveInventoryList/>},
                    {path: "products", element: <ProductsList/>},
                    {path: "productPrices", element: <ProductPricesList/>},
                    {path: "partyContacts", element: <PartyContactsList/>},
                    {path: "productCategories", element: <ProductCategoriesList/>},
                    {path: "productSuppliers", element: <ProductSuppliersList/>},
                    {path: "productCosts", element: <ProductCostsList/>},
                    {path: "productFacilities", element: <ProductFacilitiesList/>},
                    {path: "inventoryItems", element: <InventoryItemsList/>},
                    {path: "inventoryTransfer", element: <InventoryTransferList/>},
                    {path: "inventoryItemDetails", element: <InventoryItemDetailsList/>},
                    {path: "picking", element: <FacilityPickingList/>},
                    {path: "productAssociations", element: <ProductAssociationsList/>},
                    {path: "fixedAssets", element: <FixedAssetsList/>},
                    {path: "agreements", element: <AgreementsList/>},
                    {path: "orgGL", element: <OrganizationGlSettingsList/>},
                    {path: "orgAccountingSummary", element: <OrgAccountingSummary/>},
                    {path: "accountingTransaction", element: <AccountingTransactionsList/>},
                    {path: "accountingTransactionEntries", element: <AccountingTransactionEntriesList/>},
                    {path: "glQuickCreateAccountingTransaction", element: <QuickCreateAcctgTransForm/>},
                    {path: "glCreateAccountingTransaction", element: <CreateAcctgTransForm/>},
                    {path: "editAcctgTrans/:acctgTransId", element: <EditAcctgTrans/>},
                    {path: "orgChartOfAccount", element: <ChartOfAccountAssignForm/>},
                    {path: "gLAccountDefaults", element: <GlAccountTypeDefaults/>},
                    {path: "varianceReasonGLAccounts", element: <GlVarianceReason/>},
                    {path: "gLAccountTypeDefaults", element: <GlAccountTypeDefaults/>},
                    {path: "taxAuth", element: <TaxAuthoritiesList/>},
                    {path: "globalGL", element: <GlobalGlSettingsList/>},
                    {path: "chartOfAccounts", element: <ChartOfAccountsList/>},
                    {path: "paymentMethodType", element: <PaymentMethodTypeList/>},
                    {path: "bomProductComponents", element: <BOMProductComponentsList/>},
                    {path: "billOfMaterials", element: <BOMProductsList/>},
                    {path: "jobShop", element: <ProductionRunsList/>},
                    {path: "costs", element: <CostComponentCalcsList/>},
                    {
                        path: 'routings',
                        children: [
                            { index: true, element: <RoutingsList /> },
                            {
                                path: ':workEffortId',
                                element: <RoutingWrapper />,
                                children: [
                                    { path: 'edit', element: <EditRouting /> },
                                    { path: 'task-assoc', element: <ListRoutingTaskAssoc /> }, // REFACTOR: Changed to ListRoutingTaskAssoc
                                    { path: "product-link", element: <ListRoutingProductLink /> },
                                    { path: 'task-costs', element: <ListRoutingTaskCosts /> },
                                    { path: 'task', element: <EditRoutingTask /> },

                                ],
                            },
                            { path: 'new', element: <EditRouting /> },
                        ],
                    },
                    {path: "routingTasks", element: <RoutingTasksList/>},
                    {path: "customTimePeriods", element: <CustomTimePeriods/>},
                    {path: "invoiceItemType", element: <InvoiceItemTypeList/>},
                    {path: "FXRates", element: <ForeignExchangeRatesList/>},
                    {path: "salesInvoiceGLAccount", element: <SalesInvoiceAccountList />},
                    {path: "purchaseInvoiceGLAccount", element: <PurchaseInvoiceAccountList />},
                    {path: "standardCosts", element: <StandardCostsList />},
                    {path: "productCategoryGLAccount", element: <ProductCategoryGlAccounts />},
                    {path: "productGLAccounts", element: <ProductGlDefaults />},
                    {path: "partyGLAccounts", element: <PartyGlAccounts />},
                    {path: "paymentMethodIDGLAccountID", element: <PaymentMethodTypeGlAccounts />},
                    {path: "paymentTypeGLAccountTypeID", element: <PaymentTypeGlAccounts />},
                    {path: "creditCardTypeGLAccount", element: <CreditCardTypesGlAccounts />},
                    {path: "taxAuthorityGLAccounts", element: <TaxAuthorityGlAccounts />},
                    {path: "fixedAssetTypeGLMappings", element: <FixedAssetTypeGlMappings />},
                    {path: "finAccountTypeGLAccount", element: <FinAccountGlAccounts />},
                    {path: "trialBalance", element: <TrialBalance />},
                    {path: "accountingReports", element: <AccountingReportsDashboard />},
                    {path: "transactionTotals", element: <TransactionTotals />},
                    {path: "incomeStatement", element: <IncomeStatement />},
                    {path: "cashFlowStatement", element: <CashFlowStatement />},
                    {path: "balanceSheet", element: <BalanceSheet />},
                    {path: "comparativeIncomeStatement", element: <ComparativeIncomeStatement />},
                    {path: "comparativeCashFlowStatement", element: <ComaparativeCashFlowStatement />},
                    {path: "comparativeBalanceSheet", element: <ComparativeBalanceSheet />},
                    {path: "glAccountTrialBalance", element: <GlAccountTrialBalance />},
                    {path: "inventoryValuation", element: <InventoryValuation />},
                    {path: "costCenters", element: <CostCenters />},
                    {path: "physicalInventory", element: <PhysicalInventoryList />},
                    {path: "locations", element: <FacilityLocationsList />},
                    {path: "accountingCosts", element: <AccountingCosts />},
                    {path: "agreementItems", element: <AgreementItemsList />},
                    {path: "agreementTerms", element: <AgreementTermsList />},
                    {path: "timePeriod", element: <OrgSetupTimePeriodList />},
                    {path: "stockMoves", element: <StockMovesList />},
                    {path: "packing", element: <PackingList />},
                    {path: "managePicklists", element: <ManagePicklists />},
                    {path: "issueRawMaterials", element: <WorkEffortsWithReservationsList />},
                    {path: "billingAccounts", element: <BillingAccountsLayout />, children: [
                        {path: "/billingAccounts", element: <BillingAccountsList/>},
                        {path: "/billingAccounts/invoices", element: <BillingAccountInvoicesList />},
                        {path: "/billingAccounts/payments", element: <BillingAccountPayments />},
                        {path: "/billingAccounts/orders", element: <BillingAccountOrders />},
                    ]},
                    {path: "financialAccounts", element: <FinancialAccountLayout />, children: [
                        {path: "/financialAccounts", element: <FinancialAccountsList/>},
                        {path: "/financialAccounts/transactions", element: <FinancialAccountTransactions />},
                        {path: "/financialAccounts/depositWithdraw", element: <FinancialAccountDepositWithdrawal />}
                    ]},
                    { path: "party/:partyId/financial-history", element: <PartyFinancialHistoryWrapper /> },
                ],
            },
            {path: "not-found", element: <NotFound/>},
            {path: "server-error", element: <ServerError/>},
            {path: "*", element: <Navigate replace to="/not-found"/>},
        ],
    },
];

export const router = createBrowserRouter(routes);
