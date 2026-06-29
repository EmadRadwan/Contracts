# Power BI — Projects Report: MySQL Views & DAX Measures Reference

**Report file:** `Projects-28-June.pbip`  
**Database:** `erp_contracts` on `129.146.22.240:3308`  
**Generated:** June 2026

---

## How to Read This Document

The Projects Power BI report pulls data from MySQL database views and uses **measures** to calculate the numbers shown in charts and tables. A measure is simply a calculation — think of it like a formula in Excel, except Power BI re-calculates it automatically whenever you filter, slice, or drill down in the report.

This document explains:
1. What each database view contains and why it exists
2. What each measure calculates in plain business terms
3. How measures connect back to their source views

There are two types of database objects:
- **Fact views** — the transaction logs (payments, expenses, journal entries)
- **Dimension views** — the reference lists (chart of accounts, project master, customer list)

---

## Fact Views

### `Fact_Project_Revenues`

**What it is:** The payment schedule for every unit sold across all projects. When a customer signs a sales contract they commit to paying in multiple installments — an advance, monthly installments, and a maintenance deposit. Every one of those scheduled payments becomes a row in this view, along with its current collection status.

Think of it as the answer to: *"What did we agree to collect from each customer, and have we actually received it yet?"*

| Key Columns | Description |
|---|---|
| `PaymentId` | Unique payment record identifier |
| `SalesRequestId` | Parent sales contract |
| `ApartmentId`, `BuildingNumber` | Unit sold |
| `ProjectId`, `ProjectName` | Project the unit belongs to |
| `CustomerPartyId`, `CustomerName` | Buyer |
| `ScheduledAmount` | Original contracted payment amount |
| `CollectedAmount` | Amount actually received |
| `FutureAmount` | Unpaid amount whose due date has not arrived yet |
| `LateAmount` | Unpaid amount whose due date has already passed (overdue) |
| `OutstandingAmount` | Total unpaid = Future + Late |
| `ShouldHaveCollected` | The amount that was due on or before today — whether paid or not |
| `DueDate` | Contractual payment due date |
| `DaysOverdue` | How many calendar days past due (for unpaid payments) |
| `OverdueBucket` | Age bracket, e.g. 0–30 days, 31–60 days, 90+ days |
| `PaymentStatus`, `StatusId` | Current payment status code |
| `RevenueCategory`, `PAYMENT_TYPE_ID` | Type: advance payment, installment, or maintenance deposit |

**Measures that use this view:**

---

#### `Total Scheduled Revenue`
**What it means:** The total amount we contracted to receive from all customers — the full value of the payment schedule as agreed on signing day. This is the "top line" of what has been promised to us, regardless of whether it has been collected or not.

*Use it to answer: "How much in total have our customers promised to pay us?"*

---

#### `Total Collected Revenue`
**What it means:** The cash we have actually received to date. Only payments that have been fully confirmed as received are counted here.

*Use it to answer: "How much money has actually landed in our accounts?"*

---

#### `Total Future Revenue`
**What it means:** Installments and payments that are scheduled to come in, but whose due date is still in the future. These are not overdue — customers simply haven't been asked to pay them yet.

*Use it to answer: "How much more will customers owe us going forward, based on their contracts?"*

---

#### `Total Late Revenue`
**What it means:** Money that was supposed to have been paid by now but hasn't been. These are overdue receivables — the customer's due date has passed and the payment is still outstanding.

*Use it to answer: "How much is overdue and needs to be chased?"*

---

#### `Total Outstanding Revenue`
**What it means:** Everything that has not been collected yet — both future payments and overdue ones combined. It is the total remaining balance owed to us across all contracts.

*Use it to answer: "What is the total uncollected balance across all customers?"*

---

#### `Total Should Have Collected Revenue`
**What it means:** The amount that was contractually due on or before today — whether or not the customer actually paid. This is the "expected" collection baseline: if collections were perfectly on schedule, this is how much we would have received by now.

*Use it to answer: "Based on payment schedules, how much should we have collected by today?"* This is used as the denominator in the Collection Efficiency Ratio below.

---

#### `Collection Rate %`
**What it means:** The percentage of the total contracted amount that has been collected so far. A rate of 60% means we have received 60 fils of every 1 dinar contracted.

Formula: *Collected ÷ Scheduled*

*Use it to answer: "What fraction of everything we were supposed to collect have we actually received?"*

> Note: This measures progress against the whole contract lifetime, not just what was due so far. A low percentage is expected early in a project and improves over time.

---

#### `Collection Efficiency Ratio`
**What it means:** How well we are collecting relative to what we were supposed to have received by today. A ratio of 85% means we have collected 85% of the amount that was already due, implying 15% is overdue. A ratio above 100% would mean customers are paying ahead of schedule.

Formula: *Collected ÷ Should Have Collected*

*Use it to answer: "Are we keeping pace with our payment schedule, or are we falling behind?"*

> This is a more meaningful day-to-day indicator than Collection Rate %, because it compares against what is actually due today rather than the total contract lifetime.

---

#### `Late Amount (Filtered)`
**What it means:** The same as Total Late Revenue, but returns blank (empty) instead of zero when there are no overdue amounts. This is a display convenience — it keeps report tables clean by hiding rows for customers or projects that have no overdue balance, rather than showing a 0.

---

#### `Net Profit` *(project-level)*
**What it means:** Project profitability calculated as cash collected minus total costs spent. A positive number means the project has generated a cash surplus relative to what has been spent; a negative number means spending has outpaced collections so far.

Formula: *Total Collected Revenue − Total Spent*

> See `Total Spent` under the expenses section below.

---

### `Fact_Project_Expenses`

**What it is:** The record of contractor certificates issued on each project. When a construction contractor completes a phase of work, they submit a certificate claiming payment for the completed portion. The project manager certifies the amount, deductions are applied (insurance, gratuities, discounts), and the net certified amount becomes a liability to pay. Every line item on every certificate is a row in this view.

Think of it as: *"What have we certified as completed contractor work that we owe or have paid?"*

| Key Columns | Description |
|---|---|
| `PaymentId` | Certificate payment identifier |
| `ProjectId` | Project the certificate belongs to |
| `PartyId`, `PartyName` | Contractor receiving the certificate |
| `ProductId`, `ProductName` | Work item / cost category |
| `CertificateNumber`, `CertificateKey` | Certificate reference |
| `CertificateType`, `CertificateTypeArabic` | Type of certificate |
| `GrossAmount` | Pre-deduction certified amount |
| `NetCertifiedAmount` | Amount after all deductions (insurance, gratuities, discount, transport) |
| `DeductionsAmount`, `DiscountAmount`, `InsuranceAmount`, `GratuitiesAmount` | Deduction breakdown |
| `AchievementPercentage` | Completion percentage this certificate covers |
| `ExpenseDate` | Date the expense was certified |
| `IsWorkmanship`, `IsSupplyProcurement` | Whether the cost is for labour or materials |
| `IsMultiPaymentCertificate` | True when one certificate covers multiple payment runs |

> **De-duplication note:** A certificate generates a payment record in the payments system. To make sure these costs are not counted twice, any `PaymentId` that appears in this table is automatically excluded from the Direct Payments and Operating Expenses views. See `ExpensePaymentIds` at the end of this document.

**Measures that use this view:**

---

#### `Total Certificate Expenses`
**What it means:** The total net value of all contractor certificates certified to date — i.e., the amount we owe or have paid to contractors for completed, certified work. This uses the net amount after deductions, not the gross claim.

*Use it to answer: "How much have we committed to paying contractors for certified work?"*

---

### `Fact_Project_DirectPayments_2`

**What it is:** Cash payments made directly to vendors and contractors that did not go through the certificate process. These cover day-to-day site costs: buying materials from a supplier, paying a subcontractor for a specific task, purchasing tools or equipment. Any payment not tied to a contractor certificate ends up here.

Payments that were already counted as contractor certificates (see de-duplication note above) are excluded automatically to prevent double-counting.

| Key Columns | Description |
|---|---|
| `PaymentId` | Payment identifier |
| `ProjectId`, `ProjectName` | Project the cost belongs to |
| `AMOUNT` | Payment amount |
| `ActualCurrencyAmount` | Amount in original currency if a foreign currency was used |
| `CurrencyUomId` | Currency code |
| `PartyIdFrom`, `PartyIdFromName` | Paying entity (usually the company) |
| `PartyIdTo`, `PartyIdToName` | Receiving vendor or contractor |
| `PRODUCT_ID` | Cost category or product purchased |
| `CostCenterId`, `CostCenterDescription` | Which cost centre this is charged to |
| `PAYMENT_TYPE_ID`, `PaymentTypeDescription` | Nature of the payment |
| `PAYMENT_METHOD_TYPE_ID`, `PaymentMethodTypeDescription` | How it was paid: bank transfer, cheque, etc. |
| `ChequeDate`, `ChequeNumber` | Cheque details if applicable |
| `STATUS_ID`, `StatusDescription` | Payment status |
| `IsDisbursement`, `IsBankTransfer` | Method flags |
| `ApprovedByPartyId`, `ApprovedByPartyName` | Who approved the payment |
| `SALES_REQUEST_ID`, `BUILDING_NUMBER` | Link back to a specific unit or contract if applicable |
| `CreatedStamp` | When the record was created |

**Measures that use this view:**

---

#### `Total Direct Expenses`
**What it means:** The total of all direct cash payments to vendors and contractors — costs that went straight to the payee without going through a contractor certificate. This is the "cash out the door" for project-specific purchases.

*Use it to answer: "How much have we spent on direct site payments and vendor purchases?"*

---

### `Fact_Project_OperatingExpenses_2`

**What it is:** Overhead and indirect costs allocated to projects — things like staff salaries, office utilities, administration fees, and other costs that support the project but are not direct construction work. These are typically the company's running costs rather than site-specific expenditures.

Like direct payments, any payment already counted as a contractor certificate is excluded here to avoid double-counting.

| Key Columns | Description |
|---|---|
| `PaymentId` | Payment identifier |
| `ProjectId`, `ProjectName` | Project this overhead is allocated to |
| `AMOUNT` | Cost amount |
| `CostCenterId`, `CostCenterDescription` | Cost centre allocation |
| `PAYMENT_TYPE_ID`, `PaymentTypeDescription` | Payment type |
| `PartyIdFrom/To`, names | Payer and payee |
| `ChequeDate`, `ChequeNumber` | Cheque details |
| `STATUS_ID`, `StatusDescription` | Payment status |
| `IsOperatingExpense` | Confirms this is an operating (overhead) cost |
| `IsDisbursement`, `IsBankTransfer` | Method flags |
| `OrganizationPartyId` | The company entity making the payment |
| `CreatedStamp` | When the record was created |

**Measures that use this view:**

---

#### `Total Operating Expenses`
**What it means:** The total overhead and indirect costs charged to projects. This covers everything that is not direct construction work or a vendor payment — the running costs of keeping the project supported.

*Use it to answer: "What are the overhead costs being absorbed by our projects?"*

---

#### `Operating Expenses % of Total`
**What it means:** Each individual cost line's share of the total operating expenses visible on the current page. If the report is filtered to one project and one cost centre, this percentage tells you how much that cost centre contributes to that project's total overhead.

*Use it to answer: "Which cost types or cost centres account for the largest share of our overhead?"*

> Technically, this measure removes the detail filters on the expenses table itself so that the denominator always reflects the full project-level total, even when a row-level breakdown is being displayed.

---

### Expense Roll-up: `Total Spent` and `Net Profit`

These two measures combine data from all three expense views above.

#### `Total Spent`
**What it means:** The complete cost base of the project — contractor certificates, direct payments, and operating overhead combined. This is the total outflow on the cost side.

Formula: *Total Certificate Expenses + Total Direct Expenses + Total Operating Expenses*

*Use it to answer: "What have we spent in total across all cost types?"*

---

#### `Net Profit` *(project-level)*
**What it means:** How much cash the project has generated after all costs. It compares what customers have actually paid us against everything we have spent. Note that this is a **cash basis** measure — it uses collected revenue (cash in) not contracted revenue.

Formula: *Total Collected Revenue − Total Spent*

*Use it to answer: "Is the project making money on a cash basis — i.e., has cash in exceeded cash out?"*

> A negative result does not necessarily mean the project is failing — it may simply mean spending has happened ahead of collection milestones. Watch this alongside Collection Rate % to understand the timing gap.

---

### `Fact_GL_Transactions`

**What it is:** The general ledger — the complete double-entry accounting record of the business. Every financial event (sale, purchase, payment, journal entry) eventually appears here as one or more debit/credit lines. This is the authoritative source for all financial statements: Profit & Loss, Balance Sheet, and Trial Balance.

Each row in this view is a single debit or credit line on an accounting entry. The amount column is always positive; a separate flag (`DEBIT_CREDIT_FLAG`) tells Power BI whether it is a debit or a credit, and the account's own sign rule (stored in `Dim_gl_account[SIGN_MULTIPLIER]`) determines whether it adds or subtracts in financial reports.

| Key Columns | Description |
|---|---|
| `ACCTG_TRANS_ID`, `ACCTG_TRANS_ENTRY_SEQ_ID` | Transaction and line identifiers |
| `GL_ACCOUNT_ID` | Links to the chart of accounts (`Dim_gl_account`) |
| `AMOUNT` | Transaction amount — always positive |
| `DEBIT_CREDIT_FLAG` | `D` = Debit, `C` = Credit |
| `posted_date` | The date the entry was posted to the ledger |
| `transaction_date` | The original business event date |
| `ACCTG_TRANS_TYPE_ID` | Type: journal, payment, invoice, etc. |
| `ORGANIZATION_PARTY_ID` | The legal entity that owns this entry |
| `PARTY_ID` | The counter-party (customer, supplier, etc.) |
| `PAYMENT_ID` | Linked payment record if applicable |
| `INVOICE_ID` | Linked invoice if applicable |
| `VOUCHER_REF` | External reference number |
| `GL_FISCAL_TYPE_ID` | Fiscal period type |
| `RECONCILE_STATUS_ID` | Bank reconciliation status |
| `trans_description` | Free-text transaction description |

**Core building-block measures:**

---

#### `Total_FTP` — *For The Period*
**What it means:** The net signed activity in the general ledger for the period currently selected in the report (e.g., a chosen month or year). Think of it as: "What happened in the books during this time window?"

For income and revenue accounts the result is positive when activity is favourable; for expense and cost accounts the signs are handled automatically by each account's sign rule, so the number always reads in a business-meaningful direction.

*This is the foundation measure — almost every other financial measure is a filtered or time-adjusted version of this one.*

---

#### `Total_TTD` — *To The Date (cumulative)*
**What it means:** The running total of all activity from the very first transaction ever recorded, up to the last date in the current filter. This is the correct way to calculate balance sheet values — assets, liabilities, and equity accumulate over time and cannot be read from a single period slice.

*Use it to answer: "What is the current balance of this account as of the selected date?"*

> Balance sheet measures (Assets, Liabilities, Equity, Receivables, Payables) all use `Total_TTD`. Income statement measures (Sales, Gross Profit, Net Profit) use `Total_FTP`.

---

#### `BalanceSheetValue`
**What it means:** The cumulative balance of all accounts classified as balance sheet items, from the beginning of time to the selected date — regardless of any date slicer the user may have set. This ensures the balance sheet always shows a position, not a period movement.

---

**Income statement measures** — all filter the GL by account classification:

| Measure | What it shows |
|---|---|
| `SalesFTP` | Revenue from sales accounts for the selected period |
| `Cost of Sales` | Direct cost of delivering the product/units sold |
| `GrossProfit` | Revenue minus cost of sales — the trading margin before overhead |
| `OperatingProfit` | Gross profit after deducting operating overhead (admin, salaries, etc.) |
| `EBITDA` | Earnings before interest, tax, depreciation and amortisation — a measure of operational cash generation |
| `PBIT` | Profit before interest and tax — operating profit adjusted for non-operating items |
| `NetProfit` | The bottom-line profit after all items including non-operating income/expenses |
| `MarketingCostFTP` | Total marketing spend for the period (sign-reversed so it reads as a positive cost) |
| `Interest Expense` | Finance charges and loan interest for the period |

---

**Balance sheet measures** — all use the cumulative `Total_TTD`:

| Measure | What it shows |
|---|---|
| `Current Assets` | Assets expected to be converted to cash within 12 months (cash, receivables, etc.) |
| `Current Liabilities` | Obligations due within 12 months |
| `Total Assets` | Everything the business owns — current and non-current |
| `Total Debt` | All liabilities owed by the business |
| `Total Equity` | Owners' stake: what is left after all liabilities are subtracted from assets |
| `Capital Employed` | Long-term funding base = equity + long-term debt |
| `Inventory` | Unsold units or materials held at period end |
| `Receivables` | Amounts owed by customers (trade receivables balance) |
| `Payables` | Amounts owed to suppliers (trade payables balance) |

---

**Financial ratio measures** — derived from the income statement and balance sheet measures above:

| Measure | What it shows | Interpretation guide |
|---|---|---|
| `GPMargin` | Gross profit as a % of sales | Higher = better trading margin |
| `NPMargin` | Net profit as a % of sales | Higher = more of every sales dirham becomes profit |
| `Current Ratio` | Current assets ÷ current liabilities | Above 1.0 = can cover short-term debts; above 2.0 is comfortable |
| `Quick Ratio` | (Current assets − inventory) ÷ current liabilities | More conservative than Current Ratio — excludes stock that may be slow to sell |
| `Gearing` | Total debt ÷ equity | Higher = more reliant on borrowed money; lower = more self-financed |
| `Asset turnover` | Sales ÷ total assets | How efficiently assets generate revenue; higher is better |
| `ROE` | Net profit ÷ equity | Return generated on owners' investment; higher is better |
| `ROCE` | PBIT ÷ capital employed | How well the business uses its total long-term capital; higher is better |
| `Interest Cover` | PBIT ÷ interest expense | How many times over profit can cover interest payments; below 2× is a warning sign |
| `Receivables Days` | Receivables ÷ sales × 365 | Average number of days customers take to pay; lower is better |
| `Payable days` | Payables ÷ cost of sales × 365 | Average number of days we take to pay suppliers |
| `Inventory days` | Inventory ÷ cost of sales × 365 | How many days of stock are held; lower suggests faster-moving inventory |

---

**Trend and rolling measures** — used on the FTP Moving/Rolling and Banks pages to spot patterns over time:

| Measure | What it shows |
|---|---|
| `Total_TTD_Opening` | The balance at the *start* of the selected period (closing balance minus period activity) |
| `Total_TTD_Average` | The midpoint between opening and closing balance — smooths out within-period swings |
| `Total_TTD_PP` | The cumulative balance for the prior comparable period — used as a comparison baseline |
| `Total_FTP_PP` | The activity amount for the prior comparable period — shifted back by the number of months currently selected |
| `PoP_Growth_TTD` | How much the cumulative balance has grown compared to the prior period, as a percentage |
| `PoP_Growth_FTP` | How much period activity has changed compared to the prior period, as a percentage |
| `Total_FTP_90_Day_Rolling` | Activity summed over the last 90 days — smooths out short spikes |
| `Total_FTP_3_Month_Rolling` | Same idea but aligned to calendar month boundaries rather than a fixed 90-day window |
| `Total_FTP_Monthly_Moving_Average` | The 3-month total divided by 3 — gives an average monthly run-rate |
| `30_Day_Moving_Average` | An estimated monthly rate derived from the 90-day total — useful when calendar months differ in length |
| `Average_TTD_DL` | The average daily balance over the selected period — useful for average balance reporting (e.g., average cash held) |
| `Total_Sales_Bypass_COA_Filters` | Total period activity across *all* accounts, ignoring any account-level filters — used as the grand total denominator |
| `Value_as_a_percentage_of_Sales` | Each account's activity as a share of the overall total activity |
| `Total_Assets_Bypass_COA_filters` | Total cumulative balance across *all* accounts — used as the total assets denominator |
| `TTD_Value_as_a_percentage_of_assets` | Each account's cumulative balance as a share of total assets |

---

## Dimension Views

### `Dim_gl_account`

**What it is:** The chart of accounts — the master list of every account in the accounting system, organised into a hierarchy. Every GL transaction entry points to one account here. The hierarchy defines how amounts roll up into financial statements.

This is what makes a raw journal entry into a "Sales" number or an "Asset" balance — the classification columns in this view tell Power BI which bucket each transaction belongs to.

| Key Columns | Description |
|---|---|
| `GL_ACCOUNT_ID` | Primary key — every GL transaction line links here |
| `ACCOUNT`, `ACCOUNT_AR` | Account name in English and Arabic |
| `REPORT` | Whether this account belongs to `"Profit and Loss"` or `"BALANCE_SHEET"` |
| `CLASS`, `CLASS_AR` | Top-level type: `"Assets"`, `"Trading account"`, `"Operating account"`, `"Non-operating"` |
| `SUBCLASS`, `SUBCLASS_AR` | Mid-level grouping: `"Sales"`, `"Cost of Sales"`, `"Liabilities"`, `"Owners Equity"`, etc. |
| `SUBCLASS2`, `SUBCLASS2_AR` | Finer grouping: `"Current Assets"`, `"Current Liabilities"`, `"Long Term Liabilities"`, `"Marketing"` |
| `SUBACCOUNT`, `SUBACCOUNT_AR` | Leaf-level label, e.g. `"Trade Receivables"` |
| `SIGN_MULTIPLIER` | `+1` or `−1` — ensures debits and credits read correctly in reports (e.g. revenue shows as positive, costs may show as positive deductions) |
| `IS_CURRENT`, `IS_OPERATING` | Boolean flags for current/operating classification |
| `ACCOUNT_SORT`, `CLASS_SORT`, etc. | Controls the display order in financial statement visuals |
| `PARENT_GL_ACCOUNT_ID` | Parent in the hierarchy — enables drill-down |

**Used by:** Every single financial statement measure in `Fact_GL_Transactions`. Without this dimension, the model cannot distinguish a sales transaction from an asset purchase.

---

### `DimProject`

**What it is:** A lightweight mapping between projects and their associated general ledger accounts. Used to connect project-level reporting to the accounting hierarchy.

| Key Columns | Description |
|---|---|
| `ProjectId` | Project identifier |
| `ProjectName` | Project display name |
| `GlAccountId` | The GL account this project maps to |
| `GlAccountType` | Type of the linked GL account |

> `DimProject_Accounts` in the model is the same view loaded a second time under a different name to support a separate relationship path.

---

### `DimProjects`

**What it is:** The full project master — the main reference list of all real-estate or construction projects in the system, including their planned timeline and current status.

| Key Columns | Description |
|---|---|
| `ProjectId` | Project identifier |
| `ProjectName` | Project display name |
| `FacilityId`, `FacilityName` | The physical building complex this project belongs to |
| `PlannedStartDate` | When the project was planned to begin |
| `PlannedEndDate` | When the project was planned to finish |
| `StatusName` | Current project status (active, completed, on hold, etc.) |

**Measure using this view:**

#### `Project Title Dynamic`
**What it means:** Displays the name of whichever project the user has selected in the project slicer. Used in report page titles so the heading automatically updates when you switch between projects.

---

### `DimParties`

**What it is:** The master list of all parties in the system — customers, suppliers, employees, and internal organisations. Used to filter and label revenue and payment data by the people or companies involved.

| Key Columns | Description |
|---|---|
| `PartyId` | Unique party identifier — links to customer fields in revenue and payment views |
| `PartyName` | Display name |
| `PartyType` | Classification: customer, supplier, employee, etc. |

---

### `DimSuppliers`

**What it is:** The vendor/supplier register. Used to filter and group cost data by the companies we buy from, specifically on the Operating Expenses and Direct Payments pages.

| Key Columns | Description |
|---|---|
| `SupplierId` | Supplier identifier |
| `SupplierName` | Supplier display name |
| `PartyType`, `PartyTypeId` | Vendor classification |
| `StatusId`, `StatusName` | Whether the supplier relationship is active |
| `CreatedDate`, `LastUpdatedDate` | Lifecycle dates |

---

### `DimProducts`

**What it is:** The product and materials catalog. Used to classify costs and sales by what was bought or sold (construction materials, services, apartment types, etc.).

| Key Columns | Description |
|---|---|
| `ProductId` | Product identifier |
| `ProductName` | Display name |
| `ProductType` | Type code |
| `PrimaryCategoryId`, `CategoryName` | Primary category (e.g. Raw Materials, Services) |
| `MainCategoryName` | Top-level grouping |
| `CreatedDate`, `LastUpdatedDate` | Lifecycle dates |

**Related sub-type views:** `DimProductCategories`, `DimProductRawMaterials`, `DimProductServices` — these are filtered sub-sets of the same catalog, used where only one product type is relevant.

---

## Supporting Views

### `BillingAccounts`

**What it is:** The credit account register for customers. When a customer is granted a credit facility or payment arrangement linked to a project, a billing account is created. This view shows the limit, how much has been drawn, and the remaining available balance.

| Key Columns | Description |
|---|---|
| `BillingAccountId` | Account identifier |
| `PartyId`, `PartyName` | Customer |
| `ProjectId`, `ProjectName` | Project this account is tied to |
| `AccountLimit` | Maximum credit amount authorised |
| `UsedBalance` | Amount drawn against the account to date |
| `RemainingBalance` | How much credit remains (`Limit − Used`) |
| `AccountCurrencyUomId` | Currency of the account |
| `FromDate`, `ThruDate` | Account validity period |

Used on the **Billing Accounts** report page. Visuals on that page read these columns directly rather than going through named measures.

---

### `Payments`

**What it is:** A consolidated register of all money movements — both incoming (collections from customers) and outgoing (disbursements to vendors). It brings together receipts and payments in one place, making it useful for cash-flow-style pages where you want to see all transactions regardless of direction.

| Key Columns | Description |
|---|---|
| `PaymentId` | Payment identifier |
| `ProjectId`, `ProjectName` | Project allocation |
| `SalesRequestId` | Linked sales contract if applicable |
| `PartyIdFrom`, `PartyNameFrom` | Who paid |
| `PartyIdTo`, `PartyNameTo` | Who received |
| `Amount`, `ActualAmount` | Contracted and actual payment amounts |
| `EffectiveDate` | Value date of the payment |
| `ChequeDate`, `ChequeNumber` | Cheque details |
| `StatusId`, `StatusNameEnglish`, `StatusNameArabic` | Payment status |
| `PaymentDirection` | Incoming or Outgoing |
| `PaymentTypeId/Description` | Nature of the payment |
| `PaymentMethodTypeId/Name` | How it was made: bank transfer, cash, cheque, etc. |
| `IsDisbursement` | True for outgoing cash disbursements |
| `DaysUntilDue`, `OverdueBucket`, `DueStatusArabic` | Aging information |
| `CostCenterId`, `CostCenterName` | Cost centre for disbursements |
| `OverrideGlAccountId` | Manual GL account override if used |

Used on the **Payment Det**, **Incom Payment Det**, and **Outgo Payment Det** report pages.

---

### `SalesRequests`

**What it is:** The sales contract master. Each row is one signed contract for the sale of a residential unit. This view holds everything about the deal: who bought it, which unit, the total price, discount, how many installments were agreed, and the contract's current status.

| Key Columns | Description |
|---|---|
| `SalesRequestId` | Contract identifier |
| `ProjectId`, `ProjectName` | Project |
| `ApartmentId`, `ApartmentName` | The unit that was sold |
| `BuildingNumber` | Building within the project |
| `FromPartyId`, `FromPartyName` | Buyer |
| `TotalPrice` | Full contracted sale price |
| `AdvancePayment` | Advance amount agreed |
| `MaintenanceDeposit` | Deposit amount |
| `Discount` | Discount granted |
| `NumberOfInstallments`, `MonthsBetweenInstallments` | How the payment schedule is structured |
| `DateOfFirstInstallment` | When the first installment falls due |
| `SaleDate` | Contract signing date |
| `StatusId`, `StatusDescription` | Contract status (active, cancelled, completed, etc.) |
| `ApartmentStatusId`, `ApartmentStatusDescription` | Delivery status of the unit itself |
| `ApartmentSpaceM2`, `ApartmentPricePerM2` | Unit size and price per square metre |
| `GardenSpaceM2`, `GardenPricePerM2` | Garden add-on details if applicable |
| `EmployeePartyId`, `EmployeeName` | Sales agent who closed the deal |
| `ProductTypeDescription` | Product type (apartment, villa, etc.) |
| `FloorNameArabic`, `FloorNumberRaw` | Floor details |
| `CreatedStamp`, `LastUpdatedStamp` | Audit timestamps |

Used on the **Sales**, **Sales Matrix**, **Pastdue Sales**, and **Sales Chart** report pages.

---

## Computed Object (Power Query — not a MySQL view)

### `ExpensePaymentIds`

**What it is:** An internal helper list created by Power BI itself — not a MySQL view. When a contractor certificate is issued it generates both a record in `Fact_Project_Expenses` and a corresponding payment record. If that payment were also included in `Fact_Project_DirectPayments_2` or `Fact_Project_OperatingExpenses_2`, the cost would be double-counted.

Power BI solves this by collecting all `PaymentId` values from `Fact_Project_Expenses` into a deduplicated list, then using that list to exclude matching rows from the two payments views before any measure calculations happen.

This is a data quality safeguard — it ensures `Total Spent` counts each cost exactly once.

---

## View → Measure Dependency Summary

| MySQL View | Measures / pages that depend on it |
|---|---|
| `Fact_Project_Revenues` | Total Scheduled Revenue, Total Collected Revenue, Total Future Revenue, Total Late Revenue, Total Outstanding Revenue, Total Should Have Collected Revenue, Collection Rate %, Collection Efficiency Ratio, Late Amount (Filtered), Net Profit |
| `Fact_Project_Expenses` | Total Certificate Expenses, Total Spent, Net Profit |
| `Fact_Project_DirectPayments_2` | Total Direct Expenses, Total Spent, Net Profit |
| `Fact_Project_OperatingExpenses_2` | Total Operating Expenses, Operating Expenses % of Total, Total Spent, Net Profit |
| `Fact_GL_Transactions` | Total_FTP, Total_TTD, BalanceSheetValue, GrossProfit, NetProfit, OperatingProfit, EBITDA, PBIT, SalesFTP, Cost of Sales, MarketingCostFTP, Interest Expense, Current Assets, Current Liabilities, Total Assets, Total Debt, Total Equity, Capital Employed, Inventory, Receivables, Payables, all ratio measures, all rolling/trend measures |
| `Dim_gl_account` | All `Fact_GL_Transactions` measures (provides the account classification that every financial statement measure filters on) |
| `DimProjects` | Project Title Dynamic; project slicer on all pages |
| `DimProject` | GL-to-project linking in ledger pages |
| `DimParties` | Customer filtering on revenue and payment pages |
| `DimSuppliers` | Supplier filtering on expense pages |
| `DimProducts` | Product/category filtering on cost and sales pages |
| `BillingAccounts` | Billing Accounts page — direct column aggregations |
| `Payments` | Payment Det, Incom Payment Det, Outgo Payment Det pages |
| `SalesRequests` | Sales, Sales Matrix, Pastdue Sales, Sales Chart pages |
