# InnovaTech ERP — Modules & Features Overview

Compiled from the *InnovaTech Corporate Profile 2026* deck, the backend module structure
(`Application/*`, `Domain/*`), the frontend feature modules (`client-app/src/features/*`),
and the Power BI project (`Projects-06-Jul/*`).

The corporate profile frames the product as an integrated set of components sitting on top
of an Apache OFBiz-derived data model, rebuilt on .NET/C# + React, with a Power BI
intelligence layer that goes beyond static reporting. The sections below map each marketed
component to what actually exists in the codebase, plus the supporting modules (Shipments,
CRM, platform foundations) that are fully implemented alongside them.

*Note: property-development / real-estate-specific features (unit reservations, sales
commissions, installment plans) are intentionally excluded from this version.*

---

## 1. Party Management
*Marketing: "الأطراف" — a unified file for everyone the business deals with.*

**Code:** `Application/Parties/Parties`, `PartyContacts`, `PartyTypes`, `ContactMechTypes`, `ContactMechPurposeTypes`

- Single party model shared by individuals and organizations — no separate "customer table" vs "vendor table"
- Role-based classification on top of one party record: Customer, Supplier, Employee, Contractor, Broker, Sales Rep, External Sales Rep, Sales Manager, External Sales Manager, Lead
- Contact mechanisms (phone, email, address) typed by purpose, with a match/dedup map
- Employee-specific data (department, position type, salary) as a party role, feeding HR/payroll
- Per-party GL sub-ledger account assignment, so every party's transactions post to their own ledger line
- Party tax status tracking for invoicing
- Single vs. batch lead creation (bulk import into the party model)

---

## 2. Product & Catalog Management
*Marketing: "المنتجات" — raw materials, services, and finished products.*

**Code:** `Application/Catalog/*` (Products, ProductCategories, ProductFeatures, ProductPrices, ProductPromos, ProductStores, ProductSuppliers, ProductTypes, ProductAssociations)

- One product entity powers raw materials, manufactured goods, and services
- Product variants and associations (assemblies, substitutes, complementary items)
- Hierarchical category trees, including a dedicated raw-material category hierarchy
- Multiple price types (list, wholesale, promo, default) with full price history
- UoM-aware quantity conversions and a currency/UoM lookup service
- Product-to-facility association (which warehouse(s) stock which item)
- Promotions engine (`ProductPromos`, available-promotion lookups)
- Preferred/approved supplier association per product
- Standard & actual product cost roll-up, including routing-task labor and factory-overhead (FOH) allocation

---

## 3. Inventory & Facility Management
*Marketing: "مراقبة المخزون" — real-time, multi-warehouse stock control.*

**Code:** `Application/Facilities/*` (Facilities, FacilityInventories, PhysicalInventories, InventoryTransfer, RejectionReasons, FacilityLocations)

- Multi-warehouse inventory with on-hand vs. available-to-promise quantity per item
- Lot and serial-number tracking on inventory items
- Physical inventory counts with variance-reason coding
- Inter-facility inventory transfers
- Reorder-point tracking to flag low stock before it becomes a shortage
- Facility typing (raw-material warehouses vs. finished-goods warehouses)

---

## 4. Order Management (Sales & Purchase)
*Marketing: "الطلبات" — centralized order handling from request to fulfillment.*

**Code:** `Application/Order/Orders`, `Quotes`, `CustomerRequests`, `JobOrders`

- Full sales-order and purchase-order lifecycle: draft → approved → shipped/received, tracked at both order and line-item level
- Order-level adjustments (discounts/fees) and integrated tax calculation on orders and quotes
- Quotes that convert directly into sales orders or into job orders
- Customer requests (RFQ / complaint-style intake) with typed request categories
- Order payment preferences with integrated payment capture, application, and offline-payment receipt
- Backorder tracking, inventory reservation, and pick-list bin generation for warehouse picking
- Payment/shipping terms captured per order

---

## 5. Manufacturing & Production
*Marketing: "التصنيع" — BOM, work orders, and production cost tracking from raw material to finished product.*

**Code:** `Application/Manufacturing/*`, `Application/WorkEfforts/*`

- Bill of Materials (BOM) definition, multi-level BOM tree explosion, and BOM cost simulation
- Routing and routing tasks (operation sequencing) with time and cost estimation per stage
- Full production-run lifecycle: create → issue materials → run tasks → declare & produce → complete, plus one-click "quick" variants for each step
- Component reservation against a production run, and return-to-stock for unused materials
- Cost-component calculation (direct labor, overhead, materials) at both the routing-task and product level
- Work-effort-to-inventory "goods standard" association (what a task is expected to consume/produce)

---

## 6. Projects & Contractor Certificates
*Marketing: "المشاريع" — progress, budget, and certificate tracking per project.*

**Code:** `Application/Projects/*`

- Project master with sub-projects, budgets, and milestone tracking
- Contractor payment certificates: create, approve, review, reset, duplicate, and export to PDF
- Multi-payment certificates (batched contractor payment runs) with their own approval workflow
- Supply/material certificates tied to purchase orders and project GL accounts, so no delivered material goes untracked
- Automatic retention and deduction calculation (تأمين وخصومات) from percent-complete to net payable
- Per-project financial reporting: revenue, cost, and margin by period, for comparing project profitability

---

## 7. Accounting & Finance
*Marketing: "الحسابات" — chart of accounts, sub-ledgers, invoicing, payments, and reporting.*

**Code:** `Application/Accounting/*` — by far the largest module

- Chart of accounts setup at both global and per-organization level, with account hierarchy and account classes/types
- General ledger: transaction entries, posting/un-posting, multi-entry batch transactions
- AP/AR invoicing, including batch payroll-invoice generation sourced from attendance/absence data
- Payments: incoming and outgoing, payment groups, payment-to-invoice application, financial-account deposits/withdrawals, offline payment receipt
- Contractor payment governance: billing-account balances, spend limits, and guarantee enforcement so no disbursement exceeds what's agreed
- Financial (bank/cash) accounts with full transaction ledgers
- Tax engine: tax authorities, tax categories, product tax rates, automatic tax calculation on orders and quotes
- Fixed assets and standard cost types
- Agreements (contract terms) linked to parties, items, and geography
- Cost centers and foreign-exchange rate tables
- Financial reporting: trial balance, balance sheet (with comparative and vertical/horizontal analysis), income statement / P&L (with comparative), cash-flow statement, inventory valuation, transaction totals

---

## 8. Human Resources & Payroll
*Marketing: "الموارد البشرية" — payroll tied directly to biometric attendance.*

**Code:** `Application/HumanResources/*`, employee data in `Application/Parties/Parties`

- Employee master data (department, position, salary) as a party role
- Employee cash advances: create/approve/delete, with repayment schedules and date-range reporting
- Payroll-invoice batch generation linked to biometric attendance and absence data
- Payroll-advance netting against payroll runs

---

## 9. CRM — Customer Relationship Management
*Marketing: "إدارة علاقات العملاء" — lead-to-deal tracking, integrated with sales and collections.*

**Code:** `Application/CRM/Leads`, `SalesOpportunities`

- Lead capture, including single and batch (bulk import) creation
- Sales-opportunity pipeline with stages and a full history/audit log per opportunity
- Fully integrated with the party record and order/collection history, so a sales rep sees the whole customer relationship in one place — not a separate silo

---

## Supporting Module: Shipments & Fulfillment
*Not one of the marketed "10," but a fully built fulfillment layer connecting Orders and Facilities.*

**Code:** `Application/Shipments/*`

- Warehouse picking and packing sessions per shipment
- Item issuance against sales orders; shipment receipt against purchase orders
- One-click quick-ship and quick-receive flows
- Ship-group-to-facility assignment and order-item reservation tracking

---

## Business Intelligence Layer (Power BI)
*Marketing: "قوة التعمّق" — not static reports, a live analytical model built on Microsoft Power BI.*

**Code:** `Projects-06-Jul/Projects-06-Jul.SemanticModel`, `Projects-06-Jul/Projects-06-Jul.Report`

**Semantic model**
- Fact tables: GL Transactions, Project Revenues, Project Expenses, Project Operating Expenses, Project Direct Payments
- Dimension tables: Parties, Products, Product Categories, Raw Materials, Services, Suppliers, GL Accounts, Projects / Project Accounts
- Dedicated Billing Accounts and Payments tables feeding the collections dashboards
- Central measures table (DAX) driving all report pages

**Report pages** (29 pages across dashboards, drill-throughs, and tooltips)
- **Executive overview:** Executive Summary — revenue, billed, collected, overdue, expenses, and net profit in one screen
- **Financial statements:** P&L (two variants), Balance Sheet, Trial Balance, Ledgers, Cash-flow-oriented Banks page
- **Comparative analysis:** vertical/horizontal analysis baked into the balance sheet and income statement views
- **Sales & collections:** Sales, Sales Chart, Sales Matrix, Matrix Sales Revenue, Sales Payment
- **AR aging / collections risk:** Past-due Sales, Late Amounts, Top Late Customers (tooltip drill-through)
- **Payments detail:** Payment Detail, Incoming Payment Detail, Outgoing Payment Detail, Billing Accounts
- **Project financials:** Mostakhlasat (contractor/supply certificates dashboard), tying directly into the Projects module
- **Forward-looking:** Future Revenue Projection, FTP Moving/Rolling forecast
- **Expense analytics:** Operating Expenses Details, Expense-by-Product, Direct Expense Type (tooltip drill-throughs), Efficiency tooltip

This is the layer the marketing deck contrasts with "ordinary ERP reports": rather than a fixed printout, users click into any number (overdue balance, an expense line, a revenue figure) and get the underlying detail immediately, with the stated direction of natural-language Q&A and anomaly/forecast detection via Microsoft's AI tooling on top of the same Power BI model.

---

## Platform Foundations (cross-cutting)
*Shared reference data and conventions used by every module above.*

**Code:** `Application/Common/*` (DataSources, Geos, RoleTypes, UomTypes, Uoms)

- Units of measure and currency conversion, including dated (historical) conversion rates
- Geography reference data (countries)
- Generic role-type framework reused by the party model (Module 1)
- Data-source tracking for record provenance
- Bilingual Arabic/English support throughout (`Accept-Language`-driven label resolution), snake_case JSON on the domain layer, and OData-backed list endpoints for all major record types (orders, quotes, parties, etc.)

---

## How the Modules Connect

The marketing narrative's core claim — "every corner of your company has its own dashboard,
all connected" — reflects a real structural pattern in the code, not just a slogan:

- **Party** is the hub: every order, invoice, payment, certificate, and CRM record is a role
  played by a party, and every party can carry its own GL sub-ledger.
- **Product** is shared across Catalog, Manufacturing (as BOM input/output), and Facilities
  (as the thing sitting in inventory).
- **Projects** ties together Manufacturing/Facilities (material issuance against
  certificates) and Accounting (certificate payments post to project-specific GL accounts) —
  which is exactly what the Power BI "project comparison" and Mostakhlasat dashboards
  visualize.
- **Accounting** is the settlement layer underneath every other module: sales orders,
  purchase orders, payroll, and project certificates all ultimately produce GL transactions,
  which is what feeds the entire BI layer in real time.
