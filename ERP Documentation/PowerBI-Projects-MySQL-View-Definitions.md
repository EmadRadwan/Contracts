# Power BI — Projects Report: MySQL View Definitions

**Database:** `erp_contracts` on `129.146.22.240:3308`  
**Generated:** June 2026

---

## How to Read This Document

Each section covers one MySQL view used by the Power BI Projects report. For every view you will find:

- **What it does** — a plain-English explanation of the business purpose
- **Source tables** — which underlying ERP tables it reads from
- **Key logic** — the filters and rules that shape the result
- **SQL definition** — the full `CREATE VIEW` statement, formatted for readability

The ERP follows the OFBiz data model, so table names like `WORK_EFFORT`, `PAYMENT`, and `ACCTG_TRANS` are the OFBiz originals. The views translate those technical names into business-readable column names used in Power BI.

---

## Fact Views

---

### `Fact_Project_Revenues`

**What it does:**
Produces one row per customer payment scheduled under a sales contract. It reads every payment record linked to a sales request and classifies each one into five amount buckets based on its collection status and due date: scheduled (total contracted), collected (paid), outstanding (total unpaid), late (overdue), and future (not yet due). It also computes `ShouldHaveCollected` — how much was due on or before today, which serves as the denominator for the collection efficiency measure in Power BI.

**Source tables:** `PAYMENT`, `PARTY`, `PAYMENT_TYPE`, `STATUS_ITEM`, `SALES_REQUEST`, `PRODUCT`, `WORK_EFFORT`

**Key logic:**
- Only includes payments where `PARTY_ID_TO = 'Company'` (payments coming into the company)
- Only includes revenue payment types: `RECEIPT_ADVANCE_PAYMENT`, `RECEIPT_DUE_INSTALLMENT`, `RECEIPT_MAINTENANCE_AMOUNT`
- Excludes zero-amount rows
- Amount bucketing:
  - `CollectedAmount` → full amount if `STATUS_ID = 'PMNT_RECEIVED'`, else 0
  - `LateAmount` → full amount if not received AND `EFFECTIVE_DATE < today`
  - `FutureAmount` → full amount if not received AND `EFFECTIVE_DATE > today`
  - `DueTodayAmount` → full amount if not received AND `EFFECTIVE_DATE = today`
  - `ShouldHaveCollected` → full amount if `EFFECTIVE_DATE <= today` (regardless of payment status)
- `DueStatusArabic` is a dynamic Arabic label computed in SQL based on days until/past due date

```sql
CREATE VIEW `Fact_Project_Revenues` AS
SELECT
    p.PAYMENT_ID                                          AS PaymentId,
    p.SALES_REQUEST_ID                                    AS SalesRequestId,
    sr.PRODUCT_ID                                         AS ApartmentId,
    apt.BUILDING_NUMBER                                   AS BuildingNumber,
    COALESCE(apt.PROJECT_ID, p.WORK_EFFORT_ID)            AS ProjectId,
    proj.PROJECT_NAME                                     AS ProjectName,
    p.PARTY_ID_FROM                                       AS CustomerPartyId,
    pf.DESCRIPTION                                        AS CustomerName,
    p.PAYMENT_TYPE_ID                                     AS PAYMENT_TYPE_ID,
    pt_type.DESCRIPTION_ARABIC                            AS PaymentTypeArabic,
    CASE p.PAYMENT_TYPE_ID
        WHEN 'RECEIPT_ADVANCE_PAYMENT' THEN 'Advance Payment'
        WHEN 'RECEIPT_DUE_INSTALLMENT' THEN 'Installment'
        WHEN 'RECEIPT_MAINTENANCE_AMOUNT' THEN 'Maintenance Deposit'
        ELSE 'Other'
    END                                                   AS RevenueCategory,
    COALESCE(p.AMOUNT, 0)                                 AS ScheduledAmount,

    -- Collected: only if payment is confirmed received
    CASE WHEN p.STATUS_ID = 'PMNT_RECEIVED'
         THEN COALESCE(p.AMOUNT, 0) ELSE 0
    END                                                   AS CollectedAmount,

    -- Outstanding: anything not yet received
    CASE WHEN p.STATUS_ID <> 'PMNT_RECEIVED'
         THEN COALESCE(p.AMOUNT, 0) ELSE 0
    END                                                   AS OutstandingAmount,

    -- Late: not received AND due date is in the past
    CASE WHEN p.STATUS_ID <> 'PMNT_RECEIVED' AND p.EFFECTIVE_DATE < CURDATE()
         THEN COALESCE(p.AMOUNT, 0) ELSE 0
    END                                                   AS LateAmount,

    -- Future: not received AND due date is still ahead
    CASE WHEN p.STATUS_ID <> 'PMNT_RECEIVED' AND p.EFFECTIVE_DATE > CURDATE()
         THEN COALESCE(p.AMOUNT, 0) ELSE 0
    END                                                   AS FutureAmount,

    -- Due today: not received AND due date is today
    CASE WHEN p.STATUS_ID <> 'PMNT_RECEIVED' AND p.EFFECTIVE_DATE = CURDATE()
         THEN COALESCE(p.AMOUNT, 0) ELSE 0
    END                                                   AS DueTodayAmount,

    -- Should have collected: due on or before today (paid or not)
    CASE WHEN p.EFFECTIVE_DATE <= CURDATE()
         THEN COALESCE(p.AMOUNT, 0) ELSE 0
    END                                                   AS ShouldHaveCollected,

    p.STATUS_ID                                           AS StatusId,
    COALESCE(sts.DESCRIPTION_ARABIC, p.STATUS_ID)        AS StatusDescription,
    CASE WHEN p.STATUS_ID = 'PMNT_RECEIVED' THEN 'Received'
         WHEN p.EFFECTIVE_DATE < CURDATE()   THEN 'Late'
         ELSE 'Upcoming'
    END                                                   AS PaymentStatus,

    -- Overdue age bucket
    CASE WHEN p.STATUS_ID = 'PMNT_RECEIVED'                           THEN 'Received'
         WHEN p.EFFECTIVE_DATE >= CURDATE()                            THEN 'Upcoming'
         WHEN TO_DAYS(CURDATE()) - TO_DAYS(p.EFFECTIVE_DATE) <= 30    THEN 'Late (1-30 Days)'
         WHEN TO_DAYS(CURDATE()) - TO_DAYS(p.EFFECTIVE_DATE) <= 90    THEN 'Late (31-90 Days)'
         ELSE 'Late (Over 90 Days)'
    END                                                   AS OverdueBucket,

    CASE WHEN p.STATUS_ID <> 'PMNT_RECEIVED' AND p.EFFECTIVE_DATE < CURDATE()
         THEN TO_DAYS(CURDATE()) - TO_DAYS(p.EFFECTIVE_DATE)
         ELSE 0
    END                                                   AS DaysOverdue,

    p.EFFECTIVE_DATE                                      AS DueDate,
    p.CREATED_STAMP                                       AS CreatedDate,
    p.COMMENTS                                            AS COMMENTS,
    p.ChequeNumber                                        AS CHEQUENUMBER,

    -- Arabic due-status label (computed dynamically based on days)
    CASE
        WHEN p.STATUS_ID <> 'PMNT_NOT_PAID' AND p.STATUS_ID IS NOT NULL
             THEN COALESCE(sts.DESCRIPTION_ARABIC, p.STATUS_ID)
        WHEN p.EFFECTIVE_DATE IS NULL THEN 'غير محدد'
        ELSE /* Arabic label based on days until/past due date */ '...'
    END                                                   AS DueStatusArabic

FROM PAYMENT p
LEFT JOIN PARTY         pf       ON p.PARTY_ID_FROM    = pf.PARTY_ID
LEFT JOIN PAYMENT_TYPE  pt_type  ON p.PAYMENT_TYPE_ID  = pt_type.PAYMENT_TYPE_ID
LEFT JOIN STATUS_ITEM   sts      ON p.STATUS_ID         = sts.STATUS_ID
LEFT JOIN SALES_REQUEST sr       ON p.SALES_REQUEST_ID  = sr.SALES_REQUEST_ID
LEFT JOIN PRODUCT       apt      ON sr.PRODUCT_ID       = apt.PRODUCT_ID
LEFT JOIN WORK_EFFORT   proj     ON COALESCE(apt.PROJECT_ID, p.WORK_EFFORT_ID)
                                  = proj.WORK_EFFORT_ID
WHERE
    p.PARTY_ID_TO = 'Company'
    AND p.SALES_REQUEST_ID IS NOT NULL
    AND p.PAYMENT_TYPE_ID IN (
        'RECEIPT_ADVANCE_PAYMENT',
        'RECEIPT_DUE_INSTALLMENT',
        'RECEIPT_MAINTENANCE_AMOUNT'
    )
    AND COALESCE(p.AMOUNT, 0) > 0;
```

---

### `Fact_Project_Expenses`

**What it does:**
Produces one row per expense line item on an approved contractor certificate. A certificate is a formal document issued when a contractor completes a phase of work — it lists what was done, at what rate, and applies a set of deductions (insurance, gratuities, discount, transportation). The net certified amount after deductions is what the company owes the contractor. This view flattens the two-level certificate structure (header + items) into a single row per item and computes the net amount.

**Source tables:** `WORK_EFFORT` (header), `WORK_EFFORT` (items), `PARTY`, `PRODUCT`, `ORDER_PAYMENT_PREFERENCE`, `PAYMENT`

**Key logic:**
- Only includes certificates with `CURRENT_STATUS_ID = 'WEPR_APPROVED'`
- Excludes `COMPANY_SUPPLY_SALE_CERTIFICATE` (inter-company transfers, not project costs)
- Joins header (`PROJECT_CERTIFICATE` or `PAYMENT_CERTIFICATE`) to items (`CERTIFICATE_ITEM` or `PAYMENT_CERTIFICATE_ITEM`) via `WORK_EFFORT_PARENT_ID`
- `NetCertifiedAmount = GrossAmount − Discount − Deductions − Insurance + Transportation + Gratuities`
- `PaymentId` is populated from the linked `ORDER_PAYMENT_PREFERENCE → PAYMENT` chain (used for de-duplication in Power BI)

```sql
CREATE VIEW `Fact_Project_Expenses` AS
SELECT
    item.WORK_EFFORT_ID                                   AS ExpenseItemKey,
    header.WORK_EFFORT_ID                                 AS CertificateKey,

    -- Certificate number only available on PROJECT_CERTIFICATE type
    CASE WHEN header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
         THEN header.CERTIFICATE_NUMBER ELSE NULL
    END                                                   AS CertificateNumber,

    -- PaymentId: direct for PAYMENT_CERTIFICATE, via order preference for PROJECT_CERTIFICATE
    CASE WHEN header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
         THEN header.WORK_EFFORT_ID
         ELSE pyt.PAYMENT_ID
    END                                                   AS PaymentId,

    COALESCE(header.PROJECT_ID, item.PROJECT_ID)          AS ProjectId,

    -- Contractor / supplier / employee receiving the certificate
    COALESCE(
        header.PARTY_ID_SUPPLIER,
        header.PARTY_ID_CONTRACTOR,
        header.PartyIdEmployee
    )                                                     AS PartyId,
    p.DESCRIPTION                                         AS PartyName,

    COALESCE(item.PRODUCT_ID, item.SERVICE_ID)            AS ProductId,
    prod.PRODUCT_NAME                                     AS ProductName,

    CAST(COALESCE(
        item.ProcurementDate,
        item.ESTIMATED_START_DATE,
        header.ESTIMATED_START_DATE,
        header.CREATED_DATE
    ) AS DATE)                                            AS ExpenseDate,

    -- Certificate type classification
    CASE
        WHEN header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
             AND header.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE'
             THEN 'ProjectCertificate'
        WHEN header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
             THEN 'MultiPaymentCertificate'
        ELSE 'Other'
    END                                                   AS RecordType,

    CASE header.CERTIFICATE_CATEGORY
        WHEN 'SUPPLY_PROCUREMENT_CERTIFICATE'    THEN 'Supply Procurement'
        WHEN 'WORKMANSHIP_CONTRACTING_CERTIFICATE' THEN 'Workmanship Contracting'
        ELSE 'Project Certificate'
    END                                                   AS CertificateType,

    header.CERTIFICATE_CATEGORY                           AS CertificateCategoryCode,
    header.DESCRIPTION                                    AS CertificateDescription,
    item.DESCRIPTION                                      AS ItemDescription,
    header.RELATED_ORDER_ID                               AS RelatedPurchaseOrderId,

    (header.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE')    AS IsSupplyProcurement,
    (header.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE') AS IsWorkmanship,
    (header.WORK_EFFORT_TYPE_ID  = 'PAYMENT_CERTIFICATE')               AS IsMultiPaymentCertificate,

    COALESCE(item.QUANTITY, 1)                            AS Quantity,
    item.RATE                                             AS UnitRate,

    -- Gross amount before deductions
    COALESCE(item.TOTAL_AMOUNT, item.AMOUNT, 0)           AS GrossAmount,

    -- Individual deduction components
    COALESCE(item.Discount, 0)               AS DiscountAmount,
    COALESCE(item.Deductions, 0)             AS DeductionsAmount,
    COALESCE(item.Insurance, 0)              AS InsuranceAmount,
    COALESCE(item.TransportationExpenses, 0) AS TransportationExpensesAmount,
    COALESCE(item.Gratuities, 0)             AS GratuitiesAmount,

    -- Net = Gross - Discount - Deductions - Insurance + Transport + Gratuities
    (
        COALESCE(item.TOTAL_AMOUNT, item.AMOUNT, 0)
        - COALESCE(item.Discount, 0)
        - COALESCE(item.Deductions, 0)
        - COALESCE(item.Insurance, 0)
        + COALESCE(item.TransportationExpenses, 0)
        + COALESCE(item.Gratuities, 0)
    )                                                     AS NetCertifiedAmount,

    -- Completion percentage
    CASE
        WHEN header.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE'
             THEN COALESCE(item.AchievementPercent, 0)
        WHEN header.CERTIFICATE_CATEGORY IN (
                 'SUPPLY_PROCUREMENT_CERTIFICATE'
             ) OR header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
             THEN 100.0
        ELSE COALESCE(item.AchievementPercent, 0)
    END                                                   AS AchievementPercentage,

    item.AchievementPercent                               AS OriginalAchievementPercent,
    GREATEST(
        COALESCE(header.LAST_UPDATED_STAMP, '1900-01-01'),
        COALESCE(item.LAST_UPDATED_STAMP,   '1900-01-01')
    )                                                     AS LastUpdatedStamp

FROM WORK_EFFORT header
JOIN WORK_EFFORT item
    ON  item.WORK_EFFORT_PARENT_ID = header.WORK_EFFORT_ID
    AND item.WORK_EFFORT_TYPE_ID IN ('CERTIFICATE_ITEM', 'PAYMENT_CERTIFICATE_ITEM')
LEFT JOIN PARTY   p    ON COALESCE(header.PARTY_ID_SUPPLIER,
                                    header.PARTY_ID_CONTRACTOR,
                                    header.PartyIdEmployee) = p.PARTY_ID
LEFT JOIN PRODUCT prod ON COALESCE(item.PRODUCT_ID, item.SERVICE_ID) = prod.PRODUCT_ID
LEFT JOIN ORDER_PAYMENT_PREFERENCE opp ON header.RELATED_ORDER_ID = opp.ORDER_ID
LEFT JOIN PAYMENT pyt  ON opp.ORDER_PAYMENT_PREFERENCE_ID = pyt.PAYMENT_PREFERENCE_ID
WHERE
    header.CURRENT_STATUS_ID = 'WEPR_APPROVED'
    AND COALESCE(header.CERTIFICATE_CATEGORY, '') <> 'COMPANY_SUPPLY_SALE_CERTIFICATE'
    AND (
        header.WORK_EFFORT_TYPE_ID <> 'PAYMENT_CERTIFICATE'
        OR COALESCE(header.PROJECT_ID, item.PROJECT_ID) IS NOT NULL
    );
```

---

### `Fact_Project_DirectPayments_2`

**What it does:**
Produces one row per direct cash disbursement made to a vendor or contractor, where the payment is linked to a project through its `OVERRIDE_GL_ACCOUNT_ID` matching the project's main GL account. These are payments for purchases or services that did not go through the contractor certificate process.

**Source tables:** `PAYMENT`, `PAYMENT_TYPE`, `STATUS_ITEM`, `PARTY` (×4), `PAYMENT_METHOD_TYPE`, `COST_CENTER`, `SALES_REQUEST`, `PRODUCT`, `DimProject`

**Key logic:**
- Only includes disbursement payment types (`PARENT_TYPE_ID = 'DISBURSEMENT'`)
- Only includes sent/processed payments (`STATUS_ID = 'PMNT_SENT'`)
- Only includes payments where `OVERRIDE_GL_ACCOUNT_ID` matches a **project main account** (`GlAccountType = 'PROJECT_MAIN'` in `DimProject`) — this is how the payment is attributed to a specific project
- `DueStatusArabic` is computed in SQL as a dynamic Arabic label based on days relative to today
- Power BI further filters out rows whose `PaymentId` appears in `Fact_Project_Expenses` (the `ExpensePaymentIds` list) to prevent double-counting

```sql
CREATE VIEW `Fact_Project_DirectPayments_2` AS
SELECT
    pyt.PAYMENT_ID                                        AS PaymentId,
    pyt.PAYMENT_TYPE_ID                                   AS PAYMENT_TYPE_ID,
    ptt.DESCRIPTION_ARABIC                                AS PaymentTypeDescription,
    pyt.PAYMENT_METHOD_ID                                 AS PaymentMethodId,
    pyt.PAYMENT_METHOD_TYPE_ID                            AS PAYMENT_METHOD_TYPE_ID,
    pmt.DESCRIPTION_ARABIC                                AS PaymentMethodTypeDescription,
    pyt.PARTY_ID_FROM                                     AS PartyIdFrom,
    pty_from.DESCRIPTION                                  AS PartyIdFromName,
    pyt.PARTY_ID_TO                                       AS PartyIdTo,
    COALESCE(
        pty_to.DESCRIPTION,
        CASE WHEN pyt.PARTY_ID_TO = 'Company' THEN 'Golden Land'
             ELSE pyt.PARTY_ID_TO END
    )                                                     AS PartyIdToName,
    pyt.STATUS_ID                                         AS STATUS_ID,
    sts.DESCRIPTION_ARABIC                                AS StatusDescription,
    sts.DESCRIPTION                                       AS StatusDescriptionEnglish,
    pyt.EFFECTIVE_DATE                                    AS EFFECTIVE_DATE,
    pyt.CREATED_STAMP                                     AS CreatedStamp,
    pyt.COMMENTS                                          AS COMMENTS,
    pyt.PAYMENT_REF_NUM                                   AS PaymentRefNum,
    pyt.PAYMENT_PREFERENCE_ID                             AS PaymentPreferenceId,
    pyt.IS_BANK_TRANSFER                                  AS IsBankTransfer,
    pyt.AMOUNT                                            AS AMOUNT,
    COALESCE(pyt.ACTUAL_CURRENCY_AMOUNT, pyt.AMOUNT)      AS ActualCurrencyAmount,
    COALESCE(pyt.CURRENCY_UOM_ID, 'EGP')                  AS CurrencyUomId,
    TRUE                                                  AS IsDisbursement,
    NULL                                                  AS OrganizationPartyId,
    dp.ProjectId                                          AS ProjectId,
    dp.ProjectName                                        AS ProjectName,
    pyt.OVERRIDE_GL_ACCOUNT_ID                            AS OverrideGlAccountId,
    pyt.COST_CENTER_ID                                    AS CostCenterId,
    cc.DESCRIPTION                                        AS CostCenterDescription,
    sr.SALES_REQUEST_ID                                   AS SALES_REQUEST_ID,
    prod.PRODUCT_ID                                       AS PRODUCT_ID,
    prod.BUILDING_NUMBER                                  AS BUILDING_NUMBER,
    pyt.APPROVED_BY_PARTY_ID                              AS ApprovedByPartyId,
    approved.DESCRIPTION                                  AS ApprovedByPartyName,
    pyt.CREATED_BY_PARTY_ID                               AS CreatedByPartyId,
    created_by.DESCRIPTION                                AS CreatedByPartyName,
    pyt.ChequeNumber                                      AS ChequeNumber,
    pyt.ChequeDate                                        AS ChequeDate,
    /* DueStatusArabic: Arabic label based on days until/past due date */
    '...'                                                 AS DueStatusArabic

FROM PAYMENT pyt
JOIN  PAYMENT_TYPE        ptt       ON pyt.PAYMENT_TYPE_ID        = ptt.PAYMENT_TYPE_ID
JOIN  STATUS_ITEM         sts       ON pyt.STATUS_ID              = sts.STATUS_ID
JOIN  PARTY               pty_from  ON pyt.PARTY_ID_FROM          = pty_from.PARTY_ID
LEFT JOIN PAYMENT_METHOD_TYPE pmt   ON pyt.PAYMENT_METHOD_TYPE_ID = pmt.PAYMENT_METHOD_TYPE_ID
LEFT JOIN PARTY           pty_to    ON pyt.PARTY_ID_TO            = pty_to.PARTY_ID
LEFT JOIN COST_CENTER     cc        ON pyt.COST_CENTER_ID         = cc.COST_CENTER_ID
LEFT JOIN SALES_REQUEST   sr        ON pyt.SALES_REQUEST_ID       = sr.SALES_REQUEST_ID
LEFT JOIN PRODUCT         prod      ON sr.PRODUCT_ID              = prod.PRODUCT_ID
LEFT JOIN PARTY           approved  ON pyt.APPROVED_BY_PARTY_ID   = approved.PARTY_ID
LEFT JOIN PARTY           created_by ON pyt.CREATED_BY_PARTY_ID   = created_by.PARTY_ID

-- Project attribution: payment must be linked to a project's main GL account
JOIN DimProject dp ON pyt.OVERRIDE_GL_ACCOUNT_ID = dp.GlAccountId

WHERE
    (ptt.PARENT_TYPE_ID = 'DISBURSEMENT' OR ptt.PAYMENT_TYPE_ID = 'DISBURSEMENT')
    AND pyt.STATUS_ID = 'PMNT_SENT'
    AND dp.GlAccountType = 'PROJECT_MAIN'          -- direct project cost, not overhead
    AND pyt.OVERRIDE_GL_ACCOUNT_ID IS NOT NULL;
```

---

### `Fact_Project_OperatingExpenses_2`

**What it does:**
Produces one row per overhead or operating cost payment linked to a project. It uses the same source table (`PAYMENT`) and the same disbursement filter as `Fact_Project_DirectPayments_2`, but targets payments whose `OVERRIDE_GL_ACCOUNT_ID` links to a **child or operating account** under the project — not the project's main account. This separates indirect overhead from direct project costs.

**Source tables:** `PAYMENT`, `PAYMENT_TYPE`, `STATUS_ITEM`, `PARTY` (×3), `PAYMENT_METHOD_TYPE`, `COST_CENTER`, `DimProject`

**Key logic:**
- Same disbursement filter as `Fact_Project_DirectPayments_2`
- But `GlAccountType <> 'PROJECT_MAIN'` — targets `OPERATING_PARENT` and `OPERATING_CHILD` accounts
- `IsOperatingExpense = TRUE` (hardcoded flag for classification in Power BI)
- `OrganizationPartyId` is set to `PARTY_ID_FROM` (the paying organisation), unlike direct payments where it is NULL
- Power BI de-duplicates this view using the same `ExpensePaymentIds` list

```sql
CREATE VIEW `Fact_Project_OperatingExpenses_2` AS
SELECT
    pyt.PAYMENT_ID                                        AS PaymentId,
    pyt.PAYMENT_TYPE_ID                                   AS PAYMENT_TYPE_ID,
    ptt.DESCRIPTION_ARABIC                                AS PaymentTypeDescription,
    pyt.PAYMENT_METHOD_ID                                 AS PaymentMethodId,
    pyt.PAYMENT_METHOD_TYPE_ID                            AS PAYMENT_METHOD_TYPE_ID,
    pmt.DESCRIPTION_ARABIC                                AS PaymentMethodTypeDescription,
    pyt.PARTY_ID_FROM                                     AS PartyIdFrom,
    COALESCE(pty_from.DESCRIPTION, '')                    AS PartyIdFromName,
    pyt.PARTY_ID_TO                                       AS PartyIdTo,
    COALESCE(
        pty_to.DESCRIPTION,
        CASE WHEN pyt.PARTY_ID_TO = 'Company' THEN 'Golden Land'
             ELSE pyt.PARTY_ID_TO END
    )                                                     AS PartyIdToName,
    pyt.STATUS_ID                                         AS STATUS_ID,
    COALESCE(sts.DESCRIPTION_ARABIC, pyt.STATUS_ID)       AS StatusDescription,
    COALESCE(sts.DESCRIPTION,        pyt.STATUS_ID)       AS StatusDescriptionEnglish,
    pyt.EFFECTIVE_DATE                                    AS EFFECTIVE_DATE,
    pyt.CREATED_STAMP                                     AS CreatedStamp,
    pyt.COMMENTS                                          AS COMMENTS,
    pyt.PAYMENT_REF_NUM                                   AS PaymentRefNum,
    pyt.PAYMENT_PREFERENCE_ID                             AS PaymentPreferenceId,
    pyt.IS_BANK_TRANSFER                                  AS IsBankTransfer,
    pyt.AMOUNT                                            AS AMOUNT,
    COALESCE(pyt.ACTUAL_CURRENCY_AMOUNT, pyt.AMOUNT)      AS ActualCurrencyAmount,
    COALESCE(pyt.CURRENCY_UOM_ID, 'EGP')                  AS CurrencyUomId,
    TRUE                                                  AS IsDisbursement,
    TRUE                                                  AS IsOperatingExpense,
    pyt.PARTY_ID_FROM                                     AS OrganizationPartyId,  -- paying org
    dp.ProjectId                                          AS ProjectId,
    dp.ProjectName                                        AS ProjectName,
    pyt.OVERRIDE_GL_ACCOUNT_ID                            AS OverrideGlAccountId,
    pyt.COST_CENTER_ID                                    AS CostCenterId,
    cc.DESCRIPTION                                        AS CostCenterDescription,
    pyt.ChequeNumber                                      AS ChequeNumber,
    pyt.ChequeDate                                        AS ChequeDate

FROM PAYMENT pyt
JOIN  PAYMENT_TYPE        ptt       ON pyt.PAYMENT_TYPE_ID        = ptt.PAYMENT_TYPE_ID
LEFT JOIN STATUS_ITEM     sts       ON pyt.STATUS_ID              = sts.STATUS_ID
LEFT JOIN PARTY           pty_from  ON pyt.PARTY_ID_FROM          = pty_from.PARTY_ID
LEFT JOIN PAYMENT_METHOD_TYPE pmt   ON pyt.PAYMENT_METHOD_TYPE_ID = pmt.PAYMENT_METHOD_TYPE_ID
LEFT JOIN PARTY           pty_to    ON pyt.PARTY_ID_TO            = pty_to.PARTY_ID
LEFT JOIN COST_CENTER     cc        ON pyt.COST_CENTER_ID         = cc.COST_CENTER_ID

-- Project attribution via GL account — operating/overhead accounts only
JOIN DimProject dp ON pyt.OVERRIDE_GL_ACCOUNT_ID = dp.GlAccountId

WHERE
    (ptt.PARENT_TYPE_ID = 'DISBURSEMENT' OR ptt.PAYMENT_TYPE_ID = 'DISBURSEMENT')
    AND dp.GlAccountType <> 'PROJECT_MAIN'         -- overhead, not direct project cost
    AND pyt.OVERRIDE_GL_ACCOUNT_ID IS NOT NULL;
```

---

### `Fact_GL_Transactions`

**What it does:**
Flattens the double-entry accounting tables into a single row per journal entry line. Every financial event in the ERP — a customer payment, a supplier invoice, a manual journal — posts one or more debit/credit lines to `ACCTG_TRANS_ENTRY`. This view joins those lines to their parent transaction header to bring in the transaction date, type, and description. It also resolves the description: if the transaction header has no description, it falls back to a description from any of the entry lines.

**Source tables:** `ACCTG_TRANS` (header), `ACCTG_TRANS_ENTRY` (lines)

**Key logic:**
- Simple inner join: every entry line must have a parent transaction (no orphan entries)
- Description fallback: uses `MAX(entry.DESCRIPTION)` from sibling entry lines when the transaction header description is blank
- Dates are cast to `DATE` (stripping time component) for clean date filtering in Power BI
- No status filter — all posted entries are included; Power BI's `Dim_gl_account` join implicitly limits to classified accounts
- The `AMOUNT` field is always positive; `DEBIT_CREDIT_FLAG` (`D`/`C`) combined with `Dim_gl_account.SIGN_MULTIPLIER` determines the signed value in DAX

```sql
CREATE VIEW `Fact_GL_Transactions` AS
SELECT
    t.ACCTG_TRANS_ID                                      AS ACCTG_TRANS_ID,
    t.ACCTG_TRANS_TYPE_ID                                 AS ACCTG_TRANS_TYPE_ID,
    t.GL_FISCAL_TYPE_ID                                   AS GL_FISCAL_TYPE_ID,
    CAST(t.TRANSACTION_DATE AS DATE)                      AS transaction_date,
    CAST(t.POSTED_DATE      AS DATE)                      AS posted_date,

    -- Description: prefer transaction header; fall back to any non-blank entry description
    COALESCE(NULLIF(t.DESCRIPTION, ''), ed.entry_description) AS trans_description,

    t.INVOICE_ID                                          AS INVOICE_ID,
    t.PAYMENT_ID                                          AS PAYMENT_ID,
    t.PARTY_ID                                            AS PARTY_ID,
    t.VOUCHER_REF                                         AS VOUCHER_REF,

    e.ACCTG_TRANS_ENTRY_SEQ_ID                            AS ACCTG_TRANS_ENTRY_SEQ_ID,
    e.GL_ACCOUNT_ID                                       AS GL_ACCOUNT_ID,
    e.ORGANIZATION_PARTY_ID                               AS ORGANIZATION_PARTY_ID,
    e.DEBIT_CREDIT_FLAG                                   AS DEBIT_CREDIT_FLAG,
    e.AMOUNT                                              AS AMOUNT,
    e.CURRENCY_UOM_ID                                     AS CURRENCY_UOM_ID,
    e.RECONCILE_STATUS_ID                                 AS RECONCILE_STATUS_ID

FROM ACCTG_TRANS t
JOIN ACCTG_TRANS_ENTRY e ON t.ACCTG_TRANS_ID = e.ACCTG_TRANS_ID

-- Subquery: one fallback description per transaction (from any non-blank entry line)
LEFT JOIN (
    SELECT
        ACCTG_TRANS_ID,
        MAX(DESCRIPTION) AS entry_description
    FROM ACCTG_TRANS_ENTRY
    WHERE DESCRIPTION IS NOT NULL AND DESCRIPTION <> ''
    GROUP BY ACCTG_TRANS_ID
) ed ON t.ACCTG_TRANS_ID = ed.ACCTG_TRANS_ID;
```

---

## Dimension Views

---

### `Dim_gl_account`

**What it does:**
Builds the classified chart of accounts hierarchy used by all financial statement measures. It joins each leaf-level GL account to its classification labels at five hierarchy levels (Report, Class, SubClass, SubClass2, Account, SubAccount), resolves their Arabic descriptions and sort orders, and derives two Power BI-specific helper columns: `MEASURE_TYPE` (tells DAX whether to use FTP or TTD) and `IS_CURRENT` / `IS_OPERATING` flags.

Only leaf accounts are included — accounts that have no children in the hierarchy. This prevents double-counting when parent account totals would otherwise roll up sums already counted at the leaf level.

**Source tables:** `GL_ACCOUNT_ORGANIZATION`, `GL_ACCOUNT` (×2 for parent), `GL_REPORT`, `GL_CLASS_COURSE`, `GL_SUB_CLASS`, `GL_SUB_CLASS_2`, `GL_ACCOUNT_COURSE_LABEL`, `GL_SUB_ACCOUNT_COURSE_LABEL`

**Key logic:**
- Only includes accounts active in the organisation (`FROM_DATE <= NOW()` and `THRU_DATE IS NULL OR > NOW()`)
- Only includes fully-classified accounts — all five classification levels must be non-null
- Excludes structural/non-posting account classes: `DEBIT`, `CREDIT`, `RESOURCE`, `NON_POSTING`
- Only leaf accounts: `NOT EXISTS (SELECT 1 FROM GL_ACCOUNT child WHERE child.PARENT_GL_ACCOUNT_ID = a.GL_ACCOUNT_ID)`
- `SIGN_MULTIPLIER` comes from the account label — controls whether the amount adds or subtracts in `Total_FTP` DAX measure

```sql
CREATE VIEW `Dim_gl_account` AS
SELECT
    ao.GL_ACCOUNT_ID                                      AS GL_ACCOUNT_ID,
    a.ACCOUNT_NAME_ARABIC                                 AS ACCOUNT_NAME_ARABIC,
    a.PARENT_GL_ACCOUNT_ID                                AS PARENT_GL_ACCOUNT_ID,
    pa.ACCOUNT_NAME_ARABIC                                AS PARENT_ACCOUNT_NAME_ARABIC,
    acl.SIGN_MULTIPLIER                                   AS SIGN_MULTIPLIER,

    -- Financial statement hierarchy (5 levels)
    a.GL_REPORT_ID                                        AS REPORT,
    gr.DESCRIPTION_ARABIC                                 AS REPORT_AR,
    a.GL_CLASS_COURSE_ID                                  AS CLASS,
    gcc.DESCRIPTION_ARABIC                                AS CLASS_AR,
    a.GL_SUB_CLASS_ID                                     AS SUBCLASS,
    gsc.DESCRIPTION_ARABIC                                AS SUBCLASS_AR,
    a.GL_SUB_CLASS_2_ID                                   AS SUBCLASS2,
    gsc2.DESCRIPTION_ARABIC                               AS SUBCLASS2_AR,
    a.GL_ACCOUNT_COURSE_LABEL_ID                          AS ACCOUNT,
    acl.DESCRIPTION_ARABIC                                AS ACCOUNT_AR,
    a.GL_SUB_ACCOUNT_COURSE_LABEL_ID                      AS SUBACCOUNT,
    gsa.DESCRIPTION_ARABIC                                AS SUBACCOUNT_AR,

    -- Sort orders for each level (controls display order in Power BI visuals)
    gr.SORT_ORDER                                         AS REPORT_SORT,
    gcc.SORT_ORDER                                        AS CLASS_SORT,
    gsc.SORT_ORDER                                        AS SUBCLASS_SORT,
    gsc2.SORT_ORDER                                       AS SUBCLASS2_SORT,
    acl.SORT_ORDER                                        AS ACCOUNT_SORT,
    gsa.SORT_ORDER                                        AS SUBACCOUNT_SORT,

    -- DAX hint: whether to use cumulative (TTD) or period (FTP) aggregation
    CASE a.GL_REPORT_ID
        WHEN 'BALANCE_SHEET'   THEN 'TTD'
        WHEN 'PROFIT_AND_LOSS' THEN 'FTP'
        ELSE NULL
    END                                                   AS MEASURE_TYPE,

    -- Helper flags for quick filtering in Power BI
    CASE WHEN a.GL_SUB_CLASS_2_ID IN ('CURRENT_ASSETS', 'CURRENT_LIABILITIES')
         THEN 1 ELSE 0
    END                                                   AS IS_CURRENT,
    CASE WHEN a.GL_CLASS_COURSE_ID IN ('TRADING_ACCOUNT', 'OPERATING_ACCOUNT')
         THEN 1 ELSE 0
    END                                                   AS IS_OPERATING,

    a.LAST_UPDATED_STAMP                                  AS LAST_UPDATED_STAMP

FROM GL_ACCOUNT_ORGANIZATION ao
JOIN GL_ACCOUNT a
    ON  a.GL_ACCOUNT_ID = ao.GL_ACCOUNT_ID
    AND ao.FROM_DATE <= NOW()
    AND (ao.THRU_DATE IS NULL OR ao.THRU_DATE > NOW())

LEFT JOIN GL_ACCOUNT                pa   ON pa.GL_ACCOUNT_ID              = a.PARENT_GL_ACCOUNT_ID
LEFT JOIN GL_REPORT                 gr   ON gr.GL_REPORT_ID               = a.GL_REPORT_ID
LEFT JOIN GL_CLASS_COURSE           gcc  ON gcc.GL_CLASS_COURSE_ID        = a.GL_CLASS_COURSE_ID
LEFT JOIN GL_SUB_CLASS              gsc  ON gsc.GL_SUB_CLASS_ID           = a.GL_SUB_CLASS_ID
LEFT JOIN GL_SUB_CLASS_2            gsc2 ON gsc2.GL_SUB_CLASS_2_ID        = a.GL_SUB_CLASS_2_ID
LEFT JOIN GL_ACCOUNT_COURSE_LABEL   acl  ON acl.GL_ACCOUNT_COURSE_LABEL_ID = a.GL_ACCOUNT_COURSE_LABEL_ID
LEFT JOIN GL_SUB_ACCOUNT_COURSE_LABEL gsa ON gsa.GL_SUB_ACCOUNT_COURSE_LABEL_ID = a.GL_SUB_ACCOUNT_COURSE_LABEL_ID

WHERE
    a.GL_REPORT_ID               IS NOT NULL
    AND a.GL_CLASS_COURSE_ID     IS NOT NULL
    AND a.GL_SUB_CLASS_ID        IS NOT NULL
    AND a.GL_SUB_CLASS_2_ID      IS NOT NULL
    AND a.GL_ACCOUNT_COURSE_LABEL_ID IS NOT NULL
    AND a.GL_ACCOUNT_CLASS_ID NOT IN ('DEBIT', 'CREDIT', 'RESOURCE', 'NON_POSTING')
    -- Leaf accounts only (no children)
    AND NOT EXISTS (
        SELECT 1 FROM GL_ACCOUNT child
        WHERE child.PARENT_GL_ACCOUNT_ID = a.GL_ACCOUNT_ID
    );
```

---

### `DimProject`

**What it does:**
Resolves the relationship between projects and GL accounts using a recursive common table expression (CTE). A project in the ERP has one main GL account and one operating expense account. The operating expense account may itself be a parent with many child accounts. The CTE walks down the GL hierarchy from the operating expense account to collect all descendants. The final result is a flat list of every project–GL account combination, tagged with whether it is the main account, the operating parent, or an operating child.

**Source tables:** `WORK_EFFORT`, `GL_ACCOUNT`

**Key logic:**
- `PROJECT_MAIN` rows come from `WORK_EFFORT.GlAccountId` — the project's revenue/billing GL account
- `OPERATING_PARENT` rows come from `WORK_EFFORT.OPERATING_EXPENSE_GL_ACCOUNT_ID`
- `OPERATING_CHILD` rows are discovered recursively by following `PARENT_GL_ACCOUNT_ID` links downward from the operating parent
- Used by `Fact_Project_DirectPayments_2` and `Fact_Project_OperatingExpenses_2` to map payments to projects via their `OVERRIDE_GL_ACCOUNT_ID`

```sql
CREATE VIEW `DimProject` AS
WITH RECURSIVE gl_hierarchy AS (
    -- Base: start from each project's operating expense GL account
    SELECT
        we.WORK_EFFORT_ID       AS ProjectId,
        we.PROJECT_NAME         AS ProjectName,
        ga.GL_ACCOUNT_ID        AS GL_ACCOUNT_ID,
        ga.PARENT_GL_ACCOUNT_ID AS PARENT_GL_ACCOUNT_ID,
        0                       AS Level
    FROM WORK_EFFORT we
    JOIN GL_ACCOUNT ga ON ga.GL_ACCOUNT_ID = we.OPERATING_EXPENSE_GL_ACCOUNT_ID
    WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT'
      AND we.OPERATING_EXPENSE_GL_ACCOUNT_ID IS NOT NULL

    UNION ALL

    -- Recursive: descend through child GL accounts
    SELECT
        gh.ProjectId,
        gh.ProjectName,
        ga.GL_ACCOUNT_ID,
        ga.PARENT_GL_ACCOUNT_ID,
        gh.Level + 1
    FROM GL_ACCOUNT ga
    JOIN gl_hierarchy gh ON ga.PARENT_GL_ACCOUNT_ID = gh.GL_ACCOUNT_ID
)
SELECT DISTINCT
    t.ProjectId,
    t.ProjectName,
    t.GlAccountId,
    t.GlAccountType
FROM (
    -- Row 1: project main billing account
    SELECT WORK_EFFORT_ID AS ProjectId, PROJECT_NAME AS ProjectName,
           GlAccountId, 'PROJECT_MAIN' AS GlAccountType
    FROM WORK_EFFORT
    WHERE WORK_EFFORT_TYPE_ID = 'PROJECT' AND GlAccountId IS NOT NULL

    UNION ALL

    -- Row 2: operating expense parent account
    SELECT WORK_EFFORT_ID, PROJECT_NAME,
           OPERATING_EXPENSE_GL_ACCOUNT_ID, 'OPERATING_PARENT'
    FROM WORK_EFFORT
    WHERE WORK_EFFORT_TYPE_ID = 'PROJECT' AND OPERATING_EXPENSE_GL_ACCOUNT_ID IS NOT NULL

    UNION ALL

    -- Rows 3+: all child accounts discovered by the CTE
    SELECT ProjectId, ProjectName, GL_ACCOUNT_ID,
           CASE WHEN Level = 0 THEN 'OPERATING_PARENT' ELSE 'OPERATING_CHILD' END
    FROM gl_hierarchy
) t;
```

---

### `DimProjects`

**What it does:**
The project master list — one row per project with its name, planned dates, facility, and current status. The status code is translated from the internal `WEPR_*` codes into readable English labels. Used to drive the project slicer and timeline visuals in Power BI.

**Source tables:** `WORK_EFFORT`, `FACILITY`

**Key logic:**
- Filters to `WORK_EFFORT_TYPE_ID = 'PROJECT'` only
- Status is decoded from internal codes to human-readable labels via `CASE`
- Facility name is resolved with a left join (some projects may not have a facility)

```sql
CREATE VIEW `DimProjects` AS
SELECT
    we.WORK_EFFORT_ID           AS ProjectId,
    we.PROJECT_NAME             AS ProjectName,
    CASE we.CURRENT_STATUS_ID
        WHEN 'WEPR_CREATED'     THEN 'Created'
        WHEN 'WEPR_IN_PROGRESS' THEN 'In Progress'
        WHEN 'WEPR_COMPLETE'    THEN 'Completed'
        WHEN 'WEPR_CANCELLED'   THEN 'Cancelled'
        WHEN 'WEPR_ON_HOLD'     THEN 'On Hold'
        ELSE 'Unknown'
    END                         AS StatusName,
    we.ESTIMATED_START_DATE     AS PlannedStartDate,
    we.ESTIMATED_COMPLETION_DATE AS PlannedEndDate,
    we.FACILITY_ID              AS FacilityId,
    fac.FACILITY_NAME           AS FacilityName

FROM WORK_EFFORT we
LEFT JOIN FACILITY fac ON we.FACILITY_ID = fac.FACILITY_ID
LEFT JOIN WORK_EFFORT parent_we
    ON  we.WORK_EFFORT_PARENT_ID = parent_we.WORK_EFFORT_ID
    AND parent_we.WORK_EFFORT_TYPE_ID = 'PROJECT'

WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT';
```

---

### `DimParties`

**What it does:**
A filtered subset of the `PARTY` master covering only the three roles relevant to this report: contractors, customers, and suppliers. Used for party-level filtering on revenue and payment pages.

**Source table:** `PARTY`

**Key logic:**
- Single table, no joins
- Filter: `MAIN_ROLE IN ('CONTRACTOR', 'CUSTOMER', 'SUPPLIER')`

```sql
CREATE VIEW `DimParties` AS
SELECT
    p.PARTY_ID    AS PartyId,
    p.DESCRIPTION AS PartyName,
    p.MAIN_ROLE   AS PartyType
FROM PARTY p
WHERE p.MAIN_ROLE IN ('CONTRACTOR', 'CUSTOMER', 'SUPPLIER');
```

---

### `DimSuppliers`

**What it does:**
The supplier/vendor master. Reads from the same `PARTY` table as `DimParties` but is restricted to the `SUPPLIER` role and translates the internal `PARTY_TYPE_ID` and `STATUS_ID` codes into human-readable labels.

**Source table:** `PARTY`

**Key logic:**
- Filter: `MAIN_ROLE = 'SUPPLIER'`
- `PartyTypeId` decoded: `PERSON` → 'Individual', `PARTY_GROUP` → 'Company'
- `StatusId` decoded: `PARTY_ENABLED` → 'Active', `PARTY_DISABLED` → 'Inactive'

```sql
CREATE VIEW `DimSuppliers` AS
SELECT
    p.PARTY_ID          AS SupplierId,
    p.DESCRIPTION       AS SupplierName,
    p.PARTY_TYPE_ID     AS PartyTypeId,
    CASE p.PARTY_TYPE_ID
        WHEN 'PERSON'       THEN 'Individual'
        WHEN 'PARTY_GROUP'  THEN 'Company'
        ELSE p.PARTY_TYPE_ID
    END                 AS PartyType,
    p.STATUS_ID         AS StatusId,
    CASE p.STATUS_ID
        WHEN 'PARTY_ENABLED'  THEN 'Active'
        WHEN 'PARTY_DISABLED' THEN 'Inactive'
        ELSE 'Unknown'
    END                 AS StatusName,
    p.CREATED_DATE      AS CreatedDate,
    p.LAST_UPDATED_STAMP AS LastUpdatedDate
FROM PARTY p
WHERE p.MAIN_ROLE = 'SUPPLIER';
```

---

### `DimProducts`

**What it does:**
The product/materials catalog filtered to the two types relevant to project reporting: raw materials and services. Joins to `DimProductCategories` to resolve category names.

**Source tables:** `PRODUCT`, `DimProductCategories`

**Key logic:**
- Filter: `PRODUCT_TYPE_ID IN ('RAW_MATERIAL', 'SERVICE')`
- Category name and parent category name come from `DimProductCategories`

```sql
CREATE VIEW `DimProducts` AS
SELECT
    p.PRODUCT_ID                AS ProductId,
    p.PRODUCT_NAME              AS ProductName,
    p.PRODUCT_TYPE_ID           AS ProductType,
    p.PRIMARY_PRODUCT_CATEGORY_ID AS PrimaryCategoryId,
    cat.CategoryName            AS CategoryName,
    cat.ParentCategoryName      AS MainCategoryName,
    p.CREATED_DATE              AS CreatedDate,
    p.LAST_UPDATED_STAMP        AS LastUpdatedDate
FROM PRODUCT p
LEFT JOIN DimProductCategories cat
    ON p.PRIMARY_PRODUCT_CATEGORY_ID = cat.CategoryId
WHERE p.PRODUCT_TYPE_ID IN ('RAW_MATERIAL', 'SERVICE');
```

---

### `DimProductCategories`

**What it does:**
The product category hierarchy — two levels (parent and child categories). Used as a base for `DimProducts`, `DimProductRawMaterials`, and `DimProductServices`. Resolves parent category name and derives a `CategoryLevel` flag.

**Source table:** `PRODUCT_CATEGORY` (self-joined for parent)

```sql
CREATE VIEW `DimProductCategories` AS
SELECT
    pc.PRODUCT_CATEGORY_ID                        AS CategoryId,
    COALESCE(pc.DESCRIPTION_ARABIC, pc.DESCRIPTION) AS CategoryName,
    pc.DESCRIPTION                                AS CategoryNameEnglish,
    pc.PRIMARY_PARENT_CATEGORY_ID                 AS ParentCategoryId,
    parent.DESCRIPTION_ARABIC                     AS ParentCategoryName,
    CASE WHEN pc.PRIMARY_PARENT_CATEGORY_ID IS NULL THEN 1 ELSE 2 END AS CategoryLevel,
    (pc.PRIMARY_PARENT_CATEGORY_ID IS NULL)       AS IsTopLevelCategory
FROM PRODUCT_CATEGORY pc
LEFT JOIN PRODUCT_CATEGORY parent
    ON pc.PRIMARY_PARENT_CATEGORY_ID = parent.PRODUCT_CATEGORY_ID;
```

---

### `DimProductRawMaterials`

**What it does:**
Filtered subset of the product catalog restricted to raw materials (`PRODUCT_TYPE_ID = 'RAW_MATERIAL'`). Used where only material costs need to be shown, separate from services.

**Source tables:** `PRODUCT`, `DimProductCategories`

```sql
CREATE VIEW `DimProductRawMaterials` AS
SELECT
    p.PRODUCT_ID       AS MaterialId,
    p.PRODUCT_NAME     AS MaterialName,
    cat.CategoryName   AS CategoryName,
    cat.ParentCategoryName AS MainCategoryName
FROM PRODUCT p
LEFT JOIN DimProductCategories cat
    ON p.PRIMARY_PRODUCT_CATEGORY_ID = cat.CategoryId
WHERE p.PRODUCT_TYPE_ID = 'RAW_MATERIAL';
```

---

### `DimProductServices`

**What it does:**
Filtered subset of the product catalog restricted to services (`PRODUCT_TYPE_ID = 'SERVICE'`). Used for classifying service-type costs and expenses.

**Source tables:** `PRODUCT`, `DimProductCategories`

```sql
CREATE VIEW `DimProductServices` AS
SELECT
    p.PRODUCT_ID                    AS ProductId,
    p.PRODUCT_NAME                  AS ProductName,
    p.PRIMARY_PRODUCT_CATEGORY_ID   AS PrimaryProductCategoryId,
    cat.CategoryName                AS CategoryName
FROM PRODUCT p
LEFT JOIN DimProductCategories cat
    ON p.PRIMARY_PRODUCT_CATEGORY_ID = cat.CategoryId
WHERE p.PRODUCT_TYPE_ID = 'SERVICE';
```

---

## Supporting Views

---

### `BillingAccounts`

**What it does:**
Shows the credit facility position for each customer on each project. The `UsedBalance` is computed inline using a correlated subquery that sums `ADVANCE_TO_VENDOR_CONTRACTOR` payments sent to the customer on the same project. The `RemainingBalance` is the account limit minus the used balance.

**Source tables:** `BILLING_ACCOUNT`, `BILLING_ACCOUNT_ROLE`, `PARTY`, `UOM`, `WORK_EFFORT`, `PAYMENT`

**Key logic:**
- Only active billing accounts (`THRU_DATE IS NULL`)
- `UsedBalance` = sum of sent advance-to-vendor payments for this party and project
- Correlated subquery runs per row — may be slow on large datasets

```sql
CREATE VIEW `BillingAccounts` AS
SELECT
    ba.BILLING_ACCOUNT_ID                                 AS BillingAccountId,
    ba.ACCOUNT_LIMIT                                      AS AccountLimit,

    -- Used balance: sum of advance payments sent to this party on this project
    COALESCE((
        SELECT SUM(p.AMOUNT)
        FROM PAYMENT p
        WHERE p.WORK_EFFORT_ID = ba.WORK_EFFORT_ID
          AND p.PARTY_ID_TO    = bar.PARTY_ID
          AND p.STATUS_ID      = 'PMNT_SENT'
          AND p.PAYMENT_TYPE_ID = 'ADVANCE_TO_VENDOR_CONTRACTOR'
    ), 0)                                                 AS UsedBalance,

    -- Remaining = Limit - Used
    (
        COALESCE(ba.ACCOUNT_LIMIT, 0)
        - COALESCE((
            SELECT SUM(p.AMOUNT)
            FROM PAYMENT p
            WHERE p.WORK_EFFORT_ID = ba.WORK_EFFORT_ID
              AND p.PARTY_ID_TO    = bar.PARTY_ID
              AND p.STATUS_ID      = 'PMNT_SENT'
              AND p.PAYMENT_TYPE_ID = 'ADVANCE_TO_VENDOR_CONTRACTOR'
        ), 0)
    )                                                     AS RemainingBalance,

    ba.WORK_EFFORT_ID                                     AS ProjectId,
    we.PROJECT_NAME                                       AS ProjectName,
    ba.ACCOUNT_CURRENCY_UOM_ID                            AS AccountCurrencyUomId,
    uom.DESCRIPTION                                       AS AccountCurrencyUomDescription,
    bar.PARTY_ID                                          AS PartyId,
    pty.DESCRIPTION                                       AS PartyName,
    ba.FROM_DATE                                          AS FromDate,
    ba.THRU_DATE                                          AS ThruDate

FROM BILLING_ACCOUNT ba
JOIN BILLING_ACCOUNT_ROLE bar ON ba.BILLING_ACCOUNT_ID = bar.BILLING_ACCOUNT_ID
JOIN PARTY                pty ON bar.PARTY_ID           = pty.PARTY_ID
JOIN UOM                  uom ON ba.ACCOUNT_CURRENCY_UOM_ID = uom.UOM_ID
JOIN WORK_EFFORT          we  ON ba.WORK_EFFORT_ID      = we.WORK_EFFORT_ID
WHERE ba.THRU_DATE IS NULL
ORDER BY ba.BILLING_ACCOUNT_ID;
```

---

### `Payments`

**What it does:**
A broad consolidated register of all payments in the system — both incoming (from customers) and outgoing (to vendors). Unlike the two `Fact_Project_*` payment views, this view does not filter by payment type, project linkage, or status. Its purpose is to give Power BI a complete raw view of the `PAYMENT` table with all descriptive columns resolved and a `PaymentDirection` label computed.

**Source tables:** `PAYMENT`, `PARTY` (×2), `PAYMENT_METHOD`, `COST_CENTER`, `STATUS_ITEM`, `WORK_EFFORT`, `PAYMENT_TYPE`, `PAYMENT_METHOD_TYPE`, `ORDER_PAYMENT_PREFERENCE`, `ORDER_HEADER`, `SALES_REQUEST`, `PRODUCT`

**Key logic:**
- No status or type filter — all payments are included
- `PaymentDirection` is derived: if `PARENT_TYPE_ID = 'DISBURSEMENT'` → Outbound; if `PARTY_ID_TO = 'Company'` → Inbound
- `DaysUntilDue = TO_DAYS(EFFECTIVE_DATE) - TO_DAYS(CURDATE())` — negative means overdue
- `DueStatusArabic` is the same dynamic Arabic label pattern used in the fact views
- Results are ordered newest first (`EFFECTIVE_DATE DESC, PAYMENT_ID DESC`)

```sql
CREATE VIEW `Payments` AS
SELECT
    p.PAYMENT_ID                                          AS PaymentId,
    p.AMOUNT                                              AS Amount,
    p.ACTUAL_CURRENCY_AMOUNT                              AS ActualAmount,
    COALESCE(p.CURRENCY_UOM_ID, 'EGP')                   AS CurrencyUomId,
    p.PARTY_ID_FROM                                       AS PartyIdFrom,
    pf.DESCRIPTION                                        AS PartyNameFrom,
    p.PARTY_ID_TO                                         AS PartyIdTo,
    COALESCE(pt.DESCRIPTION,
        CASE WHEN p.PARTY_ID_TO = 'Company' THEN 'Company'
             ELSE p.PARTY_ID_TO END,
        'Unknown')                                        AS PartyNameTo,
    p.PAYMENT_TYPE_ID                                     AS PaymentTypeId,
    p.SALES_REQUEST_ID                                    AS SalesRequestId,
    pt_type.DESCRIPTION                                   AS PaymentTypeDescription,
    pt_type.DESCRIPTION_ARABIC                            AS PaymentTypeDescriptionArabic,
    p.PAYMENT_METHOD_TYPE_ID                              AS PaymentMethodTypeId,
    pmt_type.DESCRIPTION                                  AS PaymentMethodTypeName,
    pmt_type.DESCRIPTION_ARABIC                           AS PaymentMethodTypeNameArabic,
    p.PAYMENT_REF_NUM                                     AS PaymentRefNum,
    p.PAYMENT_METHOD_ID                                   AS PaymentMethodId,
    pm.DESCRIPTION                                        AS PaymentMethodName,
    prod.PRODUCT_ID                                       AS ProductId,
    prod.BUILDING_NUMBER                                  AS BuildingNumber,
    p.WORK_EFFORT_ID                                      AS ProjectId,
    we.PROJECT_NAME                                       AS ProjectName,
    p.COST_CENTER_ID                                      AS CostCenterId,
    cc.DESCRIPTION                                        AS CostCenterName,
    opp.ORDER_ID                                          AS OrderId,
    p.STATUS_ID                                           AS StatusId,
    si.DESCRIPTION                                        AS StatusNameEnglish,
    si.DESCRIPTION_ARABIC                                 AS StatusNameArabic,
    p.EFFECTIVE_DATE                                      AS EffectiveDate,

    -- Days until due: negative = overdue
    (TO_DAYS(p.EFFECTIVE_DATE) - TO_DAYS(CURDATE()))      AS DaysUntilDue,

    -- Overdue age bucket
    CASE
        WHEN p.STATUS_ID IN ('PMNT_RECEIVED') OR p.STATUS_ID <> 'PMNT_NOT_PAID'
             THEN 'Received'
        WHEN p.EFFECTIVE_DATE >= CURDATE() THEN 'Upcoming'
        WHEN TO_DAYS(CURDATE()) - TO_DAYS(p.EFFECTIVE_DATE) <= 30 THEN 'Late (1-30 Days)'
        WHEN TO_DAYS(CURDATE()) - TO_DAYS(p.EFFECTIVE_DATE) <= 90 THEN 'Late (31-90 Days)'
        ELSE 'Late (Over 90 Days)'
    END                                                   AS OverdueBucket,

    /* DueStatusArabic: dynamic Arabic label based on days */  '...' AS DueStatusArabic,

    -- Direction: Outbound (disbursement) vs Inbound (receipt from customer)
    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 1 ELSE 0 END AS IsDisbursement,
    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN p.PARTY_ID_FROM
         ELSE p.PARTY_ID_TO
    END                                                   AS OrganizationPartyId,
    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'Outbound'
         WHEN p.PARTY_ID_TO = 'Company'               THEN 'Inbound'
         ELSE 'Unknown'
    END                                                   AS PaymentDirection,

    p.COMMENTS                                            AS Comments,
    p.ChequeNumber                                        AS ChequeNumber,
    p.ChequeDate                                          AS ChequeDate,
    p.OVERRIDE_GL_ACCOUNT_ID                              AS OverrideGlAccountId,
    p.CREATED_STAMP                                       AS CreatedDate,
    p.PAYMENT_PREFERENCE_ID                               AS PaymentPreferenceId

FROM PAYMENT p
LEFT JOIN PARTY               pf        ON p.PARTY_ID_FROM          = pf.PARTY_ID
LEFT JOIN PARTY               pt        ON p.PARTY_ID_TO            = pt.PARTY_ID
LEFT JOIN PAYMENT_METHOD      pm        ON p.PAYMENT_METHOD_ID      = pm.PAYMENT_METHOD_ID
LEFT JOIN COST_CENTER         cc        ON p.COST_CENTER_ID         = cc.COST_CENTER_ID
LEFT JOIN STATUS_ITEM         si        ON p.STATUS_ID              = si.STATUS_ID
LEFT JOIN WORK_EFFORT         we        ON p.WORK_EFFORT_ID         = we.WORK_EFFORT_ID
LEFT JOIN PAYMENT_TYPE        pt_type   ON p.PAYMENT_TYPE_ID        = pt_type.PAYMENT_TYPE_ID
LEFT JOIN PAYMENT_METHOD_TYPE pmt_type  ON p.PAYMENT_METHOD_TYPE_ID = pmt_type.PAYMENT_METHOD_TYPE_ID
LEFT JOIN ORDER_PAYMENT_PREFERENCE opp  ON p.PAYMENT_PREFERENCE_ID  = opp.ORDER_PAYMENT_PREFERENCE_ID
LEFT JOIN ORDER_HEADER        ord       ON opp.ORDER_ID             = ord.ORDER_ID
LEFT JOIN SALES_REQUEST       sr        ON p.SALES_REQUEST_ID       = sr.SALES_REQUEST_ID
LEFT JOIN PRODUCT             prod      ON sr.PRODUCT_ID            = prod.PRODUCT_ID

ORDER BY p.EFFECTIVE_DATE DESC, p.PAYMENT_ID DESC;
```

---

### `SalesRequests`

**What it does:**
The sales contract master — one row per signed unit sale. Joins a sales request to its unit (product), the project that unit belongs to, the buyer, the sales agent, and the applicable status descriptions. Floor numbers are translated into Arabic text. Results are ordered newest first.

**Source tables:** `SALES_REQUEST`, `PRODUCT`, `PRODUCT_TYPE`, `STATUS_ITEM` (×2 for apartment status and request status), `PARTY` (×2 for buyer and employee), `WORK_EFFORT`

**Key logic:**
- `SALES_REQUEST` is the core table; all other joins are resolving descriptive labels
- Two separate `STATUS_ITEM` joins: one for the apartment delivery status (`APARTMENT_STATUS` type) and one for the sales request status (`SALES_REQUEST_STATUS` type)
- `FloorNameArabic` translates the raw floor number (0–6+) into Arabic ordinal text
- `ProductTypeDescription` prefers Arabic, falls back to English
- `StatusDescription` prefers Arabic, falls back to English, falls back to the raw status ID
- Ordered `CREATED_STAMP DESC` so most recent contracts appear first in Power BI

```sql
CREATE VIEW `SalesRequests` AS
SELECT
    sr.SALES_REQUEST_ID                                   AS SalesRequestId,
    sr.PRODUCT_ID                                         AS ApartmentId,
    p.PRODUCT_NAME                                        AS ApartmentName,
    p.BUILDING_NUMBER                                     AS BuildingNumber,
    pt.DESCRIPTION                                        AS ProductTypeDescriptionEnglish,
    pt.DESCRIPTION_ARABIC                                 AS ProductTypeDescriptionArabic,
    COALESCE(pt.DESCRIPTION_ARABIC, pt.DESCRIPTION)       AS ProductTypeDescription,
    p.PROJECT_ID                                          AS ProjectId,
    we.PROJECT_NAME                                       AS ProjectName,
    p.FLOOR_NUMBER                                        AS FloorNumberRaw,

    -- Arabic floor label
    CASE p.FLOOR_NUMBER
        WHEN '0' THEN 'الطابق الأرضي'
        WHEN '1' THEN 'الطابق الأول'
        WHEN '2' THEN 'الطابق الثاني'
        WHEN '3' THEN 'الطابق الثالث'
        WHEN '4' THEN 'الطابق الرابع'
        WHEN '5' THEN 'الطابق الخامس'
        WHEN '6' THEN 'الطابق السادس'
        ELSE CONCAT('الطابق ', COALESCE(p.FLOOR_NUMBER, 'غير محدد'))
    END                                                   AS FloorNameArabic,

    p.APARTMENT_SPACE_M2                                  AS ApartmentSpaceM2,
    p.GARDEN_SPACE_M2                                     AS GardenSpaceM2,
    p.APARTMENT_STATUS_ID                                 AS ApartmentStatusId,
    ast.DESCRIPTION                                       AS ApartmentStatusDescription,
    COALESCE(ast.DESCRIPTION_ARABIC, ast.DESCRIPTION)     AS ApartmentStatusDescriptionArabic,

    sr.FROM_PARTY_ID                                      AS FromPartyId,
    c.DESCRIPTION                                         AS FromPartyName,
    sr.EMPLOYEE_PARTY_ID                                  AS EmployeePartyId,
    e.DESCRIPTION                                         AS EmployeeName,

    sr.APARTMENT_PRICE_PER_M2                             AS ApartmentPricePerM2,
    sr.GARDEN_PRICE_PER_M2                                AS GardenPricePerM2,
    sr.DISCOUNT                                           AS Discount,
    sr.TOTAL_PRICE                                        AS TotalPrice,
    sr.ADVANCE_PAYMENT                                    AS AdvancePayment,
    sr.NUMBER_OF_INSTALLMENTS                             AS NumberOfInstallments,
    sr.DATE_OF_FIRST_INSTALLMENT                          AS DateOfFirstInstallment,
    sr.MONTHS_BETWEEN_INSTALLMENTS                        AS MonthsBetweenInstallments,
    sr.MAINTENANCE_DEPOSIT                                AS MaintenanceDeposit,

    sr.STATUS_ID                                          AS StatusId,
    srs.DESCRIPTION                                       AS SalesRequestStatusDescriptionEnglish,
    COALESCE(srs.DESCRIPTION_ARABIC, srs.DESCRIPTION)    AS SalesRequestStatusDescriptionArabic,
    COALESCE(srs.DESCRIPTION_ARABIC, srs.DESCRIPTION, sr.STATUS_ID) AS StatusDescription,

    sr.SALE_DATE                                          AS SaleDate,
    sr.COMMENTS                                           AS Comments,
    sr.CREATED_STAMP                                      AS CreatedStamp,
    sr.LAST_UPDATED_STAMP                                 AS LastUpdatedStamp

FROM SALES_REQUEST sr
JOIN PRODUCT     p   ON sr.PRODUCT_ID       = p.PRODUCT_ID
JOIN PRODUCT_TYPE pt  ON p.PRODUCT_TYPE_ID  = pt.PRODUCT_TYPE_ID

-- Apartment delivery status
LEFT JOIN STATUS_ITEM ast
    ON  p.APARTMENT_STATUS_ID = ast.STATUS_ID
    AND ast.STATUS_TYPE_ID    = 'APARTMENT_STATUS'

-- Sales request contract status
LEFT JOIN STATUS_ITEM srs
    ON  sr.STATUS_ID       = srs.STATUS_ID
    AND srs.STATUS_TYPE_ID = 'SALES_REQUEST_STATUS'

LEFT JOIN PARTY c  ON sr.FROM_PARTY_ID      = c.PARTY_ID
LEFT JOIN PARTY e  ON sr.EMPLOYEE_PARTY_ID  = e.PARTY_ID
LEFT JOIN WORK_EFFORT we
    ON  p.PROJECT_ID           = we.WORK_EFFORT_ID
    AND we.WORK_EFFORT_TYPE_ID = 'PROJECT'

ORDER BY sr.CREATED_STAMP DESC, sr.SALES_REQUEST_ID DESC;
```

---

## View Dependency Map

```
PAYMENT ──────────────────────────────────┬──► Fact_Project_Revenues
                                          ├──► Fact_Project_DirectPayments_2
                                          ├──► Fact_Project_OperatingExpenses_2
                                          └──► Payments

WORK_EFFORT ───────────────────────────── ┬──► DimProject  (recursive CTE)
                                          ├──► DimProjects
                                          └──► (joined into Fact_Project_Revenues,
                                               SalesRequests, Payments)

ACCTG_TRANS + ACCTG_TRANS_ENTRY ─────────────► Fact_GL_Transactions

WORK_EFFORT (certificate headers/items) ──────► Fact_Project_Expenses

GL_ACCOUNT + classification tables ──────────► Dim_gl_account

DimProject ────────────────────────────── ┬──► Fact_Project_DirectPayments_2
                                          └──► Fact_Project_OperatingExpenses_2

PRODUCT + PRODUCT_CATEGORY ───────────── ┬──► DimProductCategories
                                         ├──► DimProducts
                                         ├──► DimProductRawMaterials
                                         └──► DimProductServices

PARTY ────────────────────────────────── ┬──► DimParties
                                         └──► DimSuppliers

SALES_REQUEST + PRODUCT ─────────────────────► SalesRequests

BILLING_ACCOUNT + BILLING_ACCOUNT_ROLE ──────► BillingAccounts
```
