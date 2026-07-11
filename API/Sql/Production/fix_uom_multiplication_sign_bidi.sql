-- The Latin letter 'X' is a strong LTR bidi character; embedded inside an Arabic (RTL)
-- sentence it forces its own LTR run that the bidi algorithm can reorder to the wrong
-- spot. The actual multiplication sign U+00D7 (x) has bidi class ON (Other Neutral) --
-- same as '*' -- so it inherits the surrounding RTL direction correctly, exactly like
-- the original asterisk did. Idempotent: safe to re-run.

UPDATE UOM SET DESCRIPTION_ARABIC = 'المتر الطولي × 2', LAST_UPDATED_STAMP = NOW(), LAST_UPDATED_TX_STAMP = NOW() WHERE UOM_ID = 'LN_m_2x';
UPDATE UOM SET DESCRIPTION_ARABIC = 'المتر المسطح × 1.5', LAST_UPDATED_STAMP = NOW(), LAST_UPDATED_TX_STAMP = NOW() WHERE UOM_ID = 'AR_m2_1.5x';
UPDATE UOM SET DESCRIPTION_ARABIC = 'المتر الطولي × 1.5', LAST_UPDATED_STAMP = NOW(), LAST_UPDATED_TX_STAMP = NOW() WHERE UOM_ID = 'LN_m_1.5x';

-- Same fix, extended to the two remaining UOMs that still had a literal '*'.
UPDATE UOM SET ABBREVIATION = 'mX3', DESCRIPTION_ARABIC = 'المتر الطولي × 3', LAST_UPDATED_STAMP = NOW(), LAST_UPDATED_TX_STAMP = NOW() WHERE UOM_ID = 'LN_m_3x';
UPDATE UOM SET ABBREVIATION = 'm2X2', DESCRIPTION_ARABIC = 'المتر المسطح × 2', LAST_UPDATED_STAMP = NOW(), LAST_UPDATED_TX_STAMP = NOW() WHERE UOM_ID = 'AR_m2_2x';
