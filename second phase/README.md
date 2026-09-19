# Second phase — client requirements (Golden Land)

The client's second batch of requirements and the documents built from it.

| File | What it is |
|---|---|
| `2nd phase ERP SYS updated.pdf` | The original: 6 handwritten Arabic pages from the client, dated 29 Jul 2026. Scanned — no text layer. |
| `2nd-phase-ERP-SYS-transcription.md` | The handwriting transcribed to text, page by page, in the client's own Arabic wording. Uncertain readings marked ⟨ ⟩. |
| `2nd-phase-ERP-SYS-transcription.xlsx` | The same transcription as a filterable register (one row per item: page, section, item no., text, English term, uncertain flag), plus the p.6 progress-% example and a per-section count sheet. |
| `source-pages/page-1..6.png` | The same six pages rendered as images, for reading without a PDF viewer. |
| `Phase-Two-Business-Case.source.html` | Bilingual (EN / AR toggle) business case: all 42 requests, each with its business value, build size (S/M/L) and priority band. Written for the owner who funds the work. |
| `Phase-Two-Business-Case-EN.pdf` | Printable English version (10 pages). |
| `Phase-Two-Business-Case-AR.pdf` | Printable Arabic version (7 pages). |

Live version of the business case (private, EN/AR switch top-right):
https://claude.ai/code/artifact/1652372b-f6da-410c-9f31-8566ee0c7055
Append `?lang=ar` to open it in Arabic directly.

## Where the 42 items came from

| Pages | Area | Items |
|---|---|---|
| 1–2 | Reports and the system as a whole | 10 |
| 3 | Payment vouchers (صرف / قبض) | 9 |
| 4 | Journal entries | 3 |
| 4 | Payroll and staff advances | 7 |
| 5 | Sales | 7 |
| 6 | Contractor certificates (مستخلصات) | 6 |

Three marginal notes in the handwriting are ambiguous and are flagged in the business case footer
(PDC annotation on p.1, payroll/advances boundary on p.4, last line of p.6) — confirm with the user
before pricing those.

## Regenerating the PDFs

The HTML reads `?lang=en|ar` and carries its own print stylesheet. Render with headless Chrome:

```
"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" --headless --disable-gpu \
  --virtual-time-budget=12000 --no-pdf-header-footer --print-to-pdf-no-header \
  --print-to-pdf="Phase-Two-Business-Case-AR.pdf" \
  "file:///$(pwd)/Phase-Two-Business-Case.source.html?lang=ar"
```
