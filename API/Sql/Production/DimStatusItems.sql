-- =============================================================
-- DIM STATUS ITEMS – Clean dimension for all status types (Power BI DimStatusItems)
-- =============================================================
DROP VIEW IF EXISTS DimStatusItems;
CREATE OR REPLACE VIEW DimStatusItems AS
SELECT
    -- Surrogate key – used in almost every OFBiz entity that has status
    si.STATUS_ID                                            AS StatusId,                 -- KEY

    -- Primary display name (Arabic – matches your real usage)
    COALESCE(si.DESCRIPTION_ARABIC, si.DESCRIPTION, si.STATUS_ID)
                                                            AS StatusName,               -- e.g. "متصالح"

    -- English name (fallback or for English dashboards)
    COALESCE(si.DESCRIPTION, si.STATUS_ID)                  AS StatusNameEnglish,        -- e.g. "Reconciled"

    -- Status type (very useful for filtering specific workflows)
    si.STATUS_TYPE_ID                                       AS StatusTypeId,


    -- Audit
    si.LAST_UPDATED_STAMP                                   AS LastUpdatedStamp,
    si.CREATED_STAMP                                        AS CreatedStamp

FROM STATUS_ITEM si
-- Optional: filter only active/relevant status types if needed
-- WHERE si.STATUS_TYPE_ID IN ('ACCTG_ENREC_STATUS', 'ORDER_STATUS', 'INVOICE_STATUS', ...)
;

-- =============================================================
-- USAGE NOTES
-- =============================================================
-- • Join any fact table (Payment, AcctgTransEntry, Invoice, Order, etc.) on StatusId
-- • Use StatusName (Arabic) as default label in Arabic dashboards
-- • Use StatusNameEnglish in English or international reports
-- • SequenceNum enables perfect sorting in visuals (Not Reconciled → Partly → Reconciled)
-- • IsNotReconciled / IsPartlyReconciled / IsFullyReconciled = instant reconciliation KPIs