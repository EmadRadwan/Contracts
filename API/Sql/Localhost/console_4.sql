SELECT ate.acctg_trans_id, ate.acctg_trans_entry_seq_id,
       act.acctg_trans_type_id, p.payment_id, p.comments, ate.DESCRIPTION
FROM acctg_trans_entry ate
         JOIN acctg_trans act ON ate.acctg_trans_id = act.acctg_trans_id
         JOIN payment p       ON act.payment_id = p.payment_id
WHERE act.payment_id IS NOT NULL
  AND p.comments IS NOT NULL
  AND p.comments <> '' AND act.acctg_trans_type_id IN (
                                                       'INCOMING_PAYMENT',
                                                       'APARTMENT_MAINTENANCE_DEPOSIT',
                                                       'CHECK_ISSUED',
                                                       'OUTGOING_PAYMENT'
    )    ;


UPDATE acctg_trans_entry ate
    JOIN acctg_trans act ON ate.acctg_trans_id = act.acctg_trans_id
    JOIN payment p       ON act.payment_id = p.payment_id
SET ate.description = p.comments
WHERE act.payment_id IS NOT NULL
  AND p.comments IS NOT NULL
  AND p.comments <> '' AND p.comments <> '' AND act.acctg_trans_type_id IN (
                                                                            'INCOMING_PAYMENT',
                                                                            'APARTMENT_MAINTENANCE_DEPOSIT',
                                                                            'CHECK_ISSUED',
                                                                            'OUTGOING_PAYMENT'
    )    ;   