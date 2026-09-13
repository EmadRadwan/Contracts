-- Read-only diagnostic for GL accounts missing an Arabic name (ACCOUNT_NAME_ARABIC).
--
-- Why this is a report, not an auto-fill script: on this dev DB every account already has an
-- Arabic name, and looking at real examples (e.g. children of 100070 "Permanent Advances"),
-- naming isn't systematic enough to translate safely:
--   100071  احمد ماهر-عهدة مستديمة        (no linked party)
--   100072  محمد مختار-عهدة مستديمة        (PARTY_ID 160)
--   100074  عهدة مستديمة -  عبد الله عادل  (same concept, different word order)
-- Only ~2 in 10 sibling accounts even have a PARTY_GL_ACCOUNT link to pull a name from, and the
-- phrasing/order isn't consistent enough to template. Auto-generating Arabic labels for a live
-- accounting system risked writing plausible-looking but wrong text into reports real users read.
--
-- What this script does instead: finds every account missing an Arabic name and surfaces the
-- context a human needs to translate it correctly in one pass -- English name, parent's Arabic
-- name (for context/consistency), and the linked party's name where one exists.
--
-- Run: mysql -u <user> -p erp_contracts < "API/Sql/Localhost/ListGlAccountsMissingArabicName.sql"

SELECT
    ga.GL_ACCOUNT_ID                                   AS gl_account_id,
    ga.ACCOUNT_CODE                                    AS account_code,
    ga.ACCOUNT_NAME                                     AS account_name_english,
    parent.ACCOUNT_CODE                                 AS parent_account_code,
    parent.ACCOUNT_NAME_ARABIC                          AS parent_account_name_arabic,
    pty.PARTY_ID                                        AS linked_party_id,
    CONCAT_WS(' ', per.FIRST_NAME_LOCAL, per.LAST_NAME_LOCAL) AS linked_party_local_name
FROM GL_ACCOUNT ga
LEFT JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
LEFT JOIN PARTY_GL_ACCOUNT pty ON pty.GL_ACCOUNT_ID = ga.GL_ACCOUNT_ID
LEFT JOIN PERSON per ON per.PARTY_ID = pty.PARTY_ID
WHERE TRIM(COALESCE(ga.ACCOUNT_NAME_ARABIC, '')) = ''
ORDER BY ga.ACCOUNT_CODE;
