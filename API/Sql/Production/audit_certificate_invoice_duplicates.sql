-- ============================================================================
-- Read-only audit: detects duplicate/orphaned invoices across ALL project
-- certificates (WORKMANSHIP_CONTRACTING_CERTIFICATE, SUPPLY_PROCUREMENT_
-- CERTIFICATE). Safe to run anytime — no writes, no transaction needed.
--
-- Run this periodically (or whenever a certificate looks like it has more
-- invoices than expected) to catch the two known failure modes early:
--   1. Same order item billed into more than one invoice (the double-click
--      race in InvoiceHelperService.CreateInvoiceFromOrder — patched
--      2026-08-02 with a FOR UPDATE lock + a frontend button guard, and
--      2026-08-03 with a server-side idempotency check in
--      ProcessWorkmanCertificatePurchaseOrder, but this catches any recurrence
--      or any other code path that creates invoices the same way).
--   2. An invoice with a certificate-item type (PINV_CERTIFICATE%) but zero
--      ORDER_ITEM_BILLING rows — an orphan, invisible to check #1. Known to
--      arise from ResetProjectCertificate.cs's invoice cleanup not fully
--      completing (hardened 2026-08-03 with a post-delete verification that
--      now fails loudly instead of silently leaving orphans).
--
-- Also checks for overpayment (payments exceeding invoice total) on any
-- certificate-linked invoice, and reports the COMPANY_SUPPLY_SALE_CERTIFICATE
-- count for context (that category never invoices via purchase orders, so it
-- is structurally exempt from both checks above).
-- ============================================================================

SELECT '--- 1. Order items billed to more than one invoice ---' AS section;
SELECT we.CERTIFICATE_NUMBER, we.WORK_EFFORT_ID, we.CERTIFICATE_CATEGORY, oib.ORDER_ID, oib.ORDER_ITEM_SEQ_ID,
       COUNT(DISTINCT oib.INVOICE_ID) invoice_count,
       GROUP_CONCAT(DISTINCT oib.INVOICE_ID ORDER BY oib.INVOICE_ID) invoices,
       SUM(oib.AMOUNT * oib.QUANTITY) total_billed
FROM ORDER_ITEM_BILLING oib
JOIN WORK_EFFORT we ON we.RELATED_ORDER_ID = oib.ORDER_ID AND we.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
GROUP BY we.WORK_EFFORT_ID, oib.ORDER_ID, oib.ORDER_ITEM_SEQ_ID
HAVING invoice_count > 1
ORDER BY we.CERTIFICATE_CATEGORY, we.CERTIFICATE_NUMBER;

SELECT '--- 2. Orphaned certificate invoices (no ORDER_ITEM_BILLING at all) ---' AS section;
SELECT i.INVOICE_ID, i.PARTY_ID_FROM, i.STATUS_ID, i.CREATED_STAMP,
       (SELECT SUM(QUANTITY * AMOUNT) FROM INVOICE_ITEM WHERE INVOICE_ID = i.INVOICE_ID) total,
       (SELECT COUNT(*) FROM ACCTG_TRANS WHERE INVOICE_ID = i.INVOICE_ID) acctg_trans_count,
       (SELECT COUNT(*) FROM PAYMENT_APPLICATION WHERE INVOICE_ID = i.INVOICE_ID) payment_app_count
FROM INVOICE i
WHERE i.INVOICE_ID IN (SELECT DISTINCT INVOICE_ID FROM INVOICE_ITEM WHERE INVOICE_ITEM_TYPE_ID LIKE 'PINV_CERTIFICATE%')
  AND (SELECT COUNT(*) FROM ORDER_ITEM_BILLING WHERE INVOICE_ID = i.INVOICE_ID) = 0
ORDER BY i.CREATED_STAMP;

SELECT '--- 3. Certificate invoices overpaid (payments exceed invoice total) ---' AS section;
SELECT we.CERTIFICATE_NUMBER, we.CERTIFICATE_CATEGORY, cert_inv.INVOICE_ID,
       ROUND((SELECT SUM(QUANTITY * AMOUNT) FROM INVOICE_ITEM WHERE INVOICE_ID = cert_inv.INVOICE_ID), 2) invoice_total,
       ROUND((SELECT COALESCE(SUM(AMOUNT_APPLIED), 0) FROM PAYMENT_APPLICATION WHERE INVOICE_ID = cert_inv.INVOICE_ID), 2) paid_total
FROM (
  SELECT DISTINCT we.WORK_EFFORT_ID, oib.INVOICE_ID
  FROM ORDER_ITEM_BILLING oib
  JOIN WORK_EFFORT we ON we.RELATED_ORDER_ID = oib.ORDER_ID AND we.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
) cert_inv
JOIN WORK_EFFORT we ON we.WORK_EFFORT_ID = cert_inv.WORK_EFFORT_ID
HAVING paid_total > invoice_total + 0.01
ORDER BY we.CERTIFICATE_CATEGORY, we.CERTIFICATE_NUMBER;

SELECT '--- 4. Certificate counts by category (context) ---' AS section;
SELECT CERTIFICATE_CATEGORY,
       COUNT(*) total,
       SUM(CASE WHEN RELATED_ORDER_ID IS NOT NULL THEN 1 ELSE 0 END) has_related_order
FROM WORK_EFFORT
WHERE WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
GROUP BY CERTIFICATE_CATEGORY;

-- ============================================================================
-- Sections 1-3 returning no rows = clean. Any row there is a finding worth
-- investigating before it's discovered by a user noticing extra invoices.
-- ============================================================================
