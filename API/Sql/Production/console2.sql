UPDATE SALES_OPPORTUNITY_STAGE
SET
    DESCRIPTION_ARABIC = CASE OPPORTUNITY_STAGE_ID
                             WHEN 'SOSTG_PROSPECT'     THEN 'عميل محتمل'
                             WHEN 'SOSTG_QUALIFIED'    THEN 'مؤهل'
                             WHEN 'SOSTG_PROPOSAL'     THEN 'عرض'
                             WHEN 'SOSTG_NEGOTIATION'  THEN 'تفاوض'
                             WHEN 'SOSTG_CLOSED_WON'   THEN 'صفقة مكتسبة'
                             WHEN 'SOSTG_CLOSED_LOST'  THEN 'صفقة خاسرة'
                             ELSE DESCRIPTION_ARABIC
        END,
    LAST_UPDATED_STAMP = NOW()
WHERE OPPORTUNITY_STAGE_ID IN ('SOSTG_PROSPECT', 'SOSTG_QUALIFIED', 'SOSTG_PROPOSAL',
                               'SOSTG_NEGOTIATION', 'SOSTG_CLOSED_WON', 'SOSTG_CLOSED_LOST');