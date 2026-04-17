-- =============================================================
-- DIM COST CENTERS – Clean dimension table (Power BI DimCostCenters)
-- =============================================================
DROP VIEW IF EXISTS DimCostCenters;
CREATE OR REPLACE VIEW DimCostCenters AS
SELECT
    -- Surrogate key (string to preserve leading zeros if needed)
    cc.COST_CENTER_ID                                       AS CostCenterId,             -- KEY

    -- Primary display name – Arabic (your real business language)
    COALESCE(cc.DESCRIPTION, cc.COST_CENTER_ID)             AS CostCenterName,           -- e.g. "الصحراوى 10.5 فدان"

    -- Payment direction flag – critical for separating revenue vs expense reporting
    cc.IS_OUT_PAYMENT                                       AS IsOutPaymentFlag         -- 'Y' = Expense Cost Center, 'N' = Revenue/Project Cost Center

FROM COST_CENTER cc
-- Optional: filter only active cost centers in the future
-- WHERE (cc.THRU_DATE IS NULL OR cc.THRU_DATE >= CURDATE())
;

-- =============================================================
-- USAGE NOTES
-- =============================================================
-- • Join your fact tables (Payment, AcctgTrans, WorkEffortCost, etc.) using CostCenterId
-- • Use CostCenterName (Arabic) as default label in all Arabic dashboards
-- • Use CostCenterNameEnglish when building English/international reports
-- • Filter reports easily with CostCenterType = 'Project / Revenue Cost Center' vs 'Expense Cost Center'
-- • CostCenterGroup gives instant high-level grouping (Real Estate Projects vs Investments)